using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

// 无限升级 targetfuntional.dll 增量补丁器（v7 全量单遍，2026-09-07）
//
// 用法: dotnet run            （基于 v1 备份单遍跑出当前定稿）
//
// 输入 SRC = targetfuntional_v1_20260906.dll
//   ⚠️ = v1 全量补丁产物（ilpatch/Patch.cs 于 09-05 基于原始 dll 构建：六功能开关 +
//   CheckAmount/Record/CheckBan系/BanGameAciton/Report/ForceBanGameAciton 早退 +
//   SendRT 旧 9 项黑名单 + StarryConfig），原始 dll 已随 03_decompiled 清理，
//   此文件是全部后续版本的唯一基底，勿删！
// 输出 DST = targetfuntional.dll
//
// v7 方案（用户定稿）：**在线 + 屏蔽封号执法**——保持联网，服务器服务全部恢复
// （商城广告奖励/异界召唤/排行榜），只拦执法与数据外泄：
//   ① SendRT 黑名单 10 项（上报/日志类，含 CheckMMM）
//   ② AccountMgrClient.SendRT 同款黑名单（不继承 NetBehaviour，v1 管不到）
//   ③ 8 个执法 RPC no-op（封禁弹窗/强退/清档/服务器弹窗/版本封锁/存档回滚）
//   ④ COS 直传杀（硬编码桶+密钥，不依赖游戏服务器）
//   ⑤ AdsFree 免广告直发（free_ads 开 → OnAdsSuccessed+action）
//   ⑥ CheckOnLine 本地开门（商城免等服务器确认）
// v3 断连（ip 毒化/Connect 调用点跳过）在本方案**移除**：服务器能看到数据并标记，
// 仅客户端不执行封禁——该取舍用户知情确认。
// 历史：v5 广告奖励本地发放、v6 异界召唤（AngleOpenWindow）均已否定，不再包含。
//
// 铁律：有过滤（block_report 门控，缺失=原版行为）；调原函数；不碰未列出 IL。

// 2026-09-27 三次热更适配：官方 dll 2030080 → 2028544 B（fnv1a64 0x43df8d0d87bf08b2），
// SRC = 用新官方 dll 重打出的 v1（31 patches）；DST = 最终补丁 dll（必须 ≤ kOrigDllLen=2028544）
const string SRC = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_v1_20260927_hotfix3.dll";
const string DST = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional.dll";

string[] Blacklist = {
    "CheckBan", "CheckBanReport", "AddPlayerReport", "RecordItemUseAnomaly",
    "AddRecord", "ShowLogs", "ShowMessage", "UploadLog", "SendLocalLogToServer",
    "CheckMMM",
    // v7.6 全面审计补漏（2026-09-07）：
    "SendCurrentFileDataTime",          // 游玩时长上报×2
    "RecordSaveTime",                   // 游玩时长上报（CloudSaveMgr）×2
    "SendCurrentFileInfo",              // 整个 GameData 存档对象上报（CloudSaveMgr，第二条云存档通道）
    "RecordItemUse",                    // 道具使用记录（itemID,num）
    "RecordExchangeOperation",          // 兑换操作记录
    "SendDeviceInfoBack",               // 设备信息字典回传（SendBackInfo RPC 响应）
    "SendRewardRecordToServer",         // 奖励记录全量上报
    "RequestRealtimeSaveUploadSession", // 实时主档会话请求（双保险）
    "CheckCodeFiled",                   // 验证码失败上报（调用点已死，双保险）
    "UploadOpenID",                     // 平台 OpenID 冗余上报（登录载荷已含）
};

var asm = AssemblyDefinition.ReadAssembly(SRC);
var mod = asm.MainModule;
Console.WriteLine($"[load] {mod.Types.Count} types");

var tStr = mod.TypeSystem.String;
var tBool = mod.TypeSystem.Boolean;
var tVoid = mod.TypeSystem.Void;
var corlib = mod.AssemblyReferences.First(r => r.Name == "mscorlib");
var tExc = new TypeReference("System", "Exception", mod, corlib);

var cfgType = mod.Types.FirstOrDefault(t => t.Name == "StarryConfig")
    ?? throw new Exception("StarryConfig 缺失（SRC 必须为 v1 补丁产物）");
var mGetBool = cfgType.Methods.First(x => x.Name == "GetBool");

MethodReference StringEq()
{
    var eq = new MethodReference("op_Equality", tBool, tStr) { HasThis = false };
    eq.Parameters.Add(new ParameterDefinition(tStr));
    eq.Parameters.Add(new ParameterDefinition(tStr));
    return eq;
}

MethodDefinition FindM(string typeFull, string name, int? paramCount = null)
{
    var t = mod.GetType(typeFull) ?? throw new Exception($"{typeFull} 不存在");
    var list = t.Methods.Where(x => x.Name == name &&
        (paramCount == null || x.Parameters.Count == paramCount)).ToList();
    if (list.Count != 1) throw new Exception($"{typeFull}::{name} {list.Count} 个重载，需消歧");
    if (!list[0].HasBody) throw new Exception($"{typeFull}::{name} 无方法体");
    return list[0];
}

// ① SendRT 黑名单（func ∈ 黑名单 → return；funcArgIndex = IL 参数索引含 this）
void AddBlacklist(MethodDefinition m, int funcArgIndex, string tag)
{
    var first = m.Body.Instructions[0];
    var retI = m.Body.Instructions.FirstOrDefault(i => i.OpCode.Code == Code.Ret)
        ?? throw new Exception($"{tag}: 无 ret");
    var seq = new List<Instruction>();
    foreach (var fun in Blacklist)
    {
        seq.Add(funcArgIndex switch
        {
            0 => Instruction.Create(OpCodes.Ldarg_0),
            1 => Instruction.Create(OpCodes.Ldarg_1),
            2 => Instruction.Create(OpCodes.Ldarg_2),
            3 => Instruction.Create(OpCodes.Ldarg_3),
            _ => Instruction.Create(OpCodes.Ldarg, m.Parameters.First(p => p.Index == funcArgIndex)),
        });
        seq.Add(Instruction.Create(OpCodes.Ldstr, fun));
        seq.Add(Instruction.Create(OpCodes.Call, StringEq()));
        seq.Add(Instruction.Create(OpCodes.Brtrue, retI));
    }
    seq.Add(Instruction.Create(OpCodes.Br, first));
    for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    Console.WriteLine($"[patch] {tag}: {Blacklist.Length} fun @ILarg{funcArgIndex}");
}

// ② void 执法 RPC no-op
void NoOpVoid(MethodDefinition m, string tag)
{
    var first = m.Body.Instructions[0];
    var seq = new List<Instruction>
    {
        Instruction.Create(OpCodes.Ldstr, "block_report"),
        Instruction.Create(OpCodes.Call, mGetBool),
        Instruction.Create(OpCodes.Brfalse, first),
        Instruction.Create(OpCodes.Ret),
    };
    for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    Console.WriteLine($"[patch] {tag}: no-op");
}

// ②a2 无条件 no-op（不依赖 block_report）：v7.9 封号提示/执法链路专用
void NoOpVoidAlways(MethodDefinition m, string tag)
{
    m.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ret));
    Console.WriteLine($"[patch] {tag}: no-op(无条件)");
}

// ②b bool 方法强制 false（v7.7 2026-09-23：断 [Rpc] 回调路径）
void FalseOnEntry(MethodDefinition m, string tag)
{
    var first = m.Body.Instructions[0];
    var seq = new List<Instruction>
    {
        Instruction.Create(OpCodes.Ldstr, "block_report"),
        Instruction.Create(OpCodes.Call, mGetBool),
        Instruction.Create(OpCodes.Brfalse, first),
        Instruction.Create(OpCodes.Ldc_I4_0),
        Instruction.Create(OpCodes.Ret),
    };
    for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    Console.WriteLine($"[patch] {tag}: false-on-entry");
}

// ③ Task<bool> COS 直传入口抛异常（两调用点均 try/catch 包裹，已核实）
void ThrowOnEntry(MethodDefinition m, string tag)
{
    var first = m.Body.Instructions[0];
    var ctor = new MethodReference(".ctor", tVoid, tExc) { HasThis = true };
    ctor.Parameters.Add(new ParameterDefinition(tStr));
    var seq = new List<Instruction>
    {
        Instruction.Create(OpCodes.Ldstr, "block_report"),
        Instruction.Create(OpCodes.Call, mGetBool),
        Instruction.Create(OpCodes.Brfalse, first),
        Instruction.Create(OpCodes.Ldstr, "starry offline"),
        Instruction.Create(OpCodes.Newobj, ctor),
        Instruction.Create(OpCodes.Throw),
    };
    for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    Console.WriteLine($"[patch] {tag}: entry-throw");
}

// ⑦ 静默防封：**只拦上报名单**（无条件、加载即生效，无菜单开关）
//    语义：命中「纯上报/反作弊/日志」名单的 RPC 记录后丢包；**其余（含所有请求与应答）一律放行**，
//          保证服务器下发照常（异界召唤/羽灵召唤等依赖 `GetPlayerMainData` 应答的功能不受影响）。
//          与既有防封同构——无条件，不由玩家菜单控制。
//    覆盖三类出站通道：SendRT / Send / SendBA（各自 string func 重载族）。
var mNote = cfgType.Methods.FirstOrDefault(x => x.Name == "NoteBlockedRpc")
    ?? throw new Exception("StarryConfig.NoteBlockedRpc 缺失（SRC 需含 v1 白名单诊断）");

// ⚠️ v7.11（2026-09-23 用户实测）：**放弃白名单，改「只拦纯上报」名单**。
//    白名单把 `GetPlayerMainData`（登录主数据请求，响应 `UpDatePlayerMainData` 下发 playerMainData）
//    一并拦掉 → 异界召唤/羽灵召唤等依赖服务器下发主数据的功能失效（用户实测反馈）。
//    收口原则：**请求/应答一律放行**（服务器下发必须通），只丢「纯状态上报 / 反作弊上报 / 日志外发」。
//    名单外的未知 RPC 一律放行（宁可少拦，不可断功能）。
string[] SendDeny = {
    // 反作弊 / 举报 / 封号上报
    "AddPlayerReport", "CheckBanReport", "CheckBan", "OnRecordNewCheckCode", "AddActiveRecord",
    // 2026-09-23 二次热更新增：道具来源异常上报（配合 InfinityCore.ItemAddSourceAnomalyMonitor）
    "RecordItemAddSourceAnomaly",
    // 设备与在线状态外发
    "SendDeviceInfoBack", "PlayerToBackGround", "PlayerToFrontGround",
    // 日志外发
    "SendLocalLogToServer", "SendRewardRecordToServer", "UploadLog", "ShowLogs",
    // 存档直传会话 / 实时存档上传（COS 链路，另有 no-op 兜底）
    "RequestRealtimeSaveUploadSession",
};

IEnumerable<TypeDefinition> AllTypes(TypeDefinition t)
{
    yield return t;
    foreach (var n in t.NestedTypes)
        foreach (var x in AllTypes(n)) yield return x;
}

// ⚠️ v7.9 血案修复（2026-09-23 实测）：判定若内联进每个通道（19 处 × 66 项比较 ≈ 22.5KB），
//    补丁 dll 会超过官方热更数组长度（kOrigDllLen=1960960 B）→ HookExec 里 pLen > len
//    静默 skip（日志 `embedded dll size 1976832 > array 1960960, skip`）→ 实际跑官方 dll：
//    防封/功能全失效、封号弹窗裸奔。
//    改为「共享判定」：名单只生成一份 StarryConfig.StrictBlockedRpc(string)，
//    各通道内只留 6 条指令（体积 ~1.5KB）。返回 true = 丢包。
var mStrict = new MethodDefinition("StrictBlockedRpc",
    MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, tBool);
mStrict.Parameters.Add(new ParameterDefinition("fun", ParameterAttributes.None, tStr));
{
    var cil = mStrict.Body.Instructions;
    var lblDrop = Instruction.Create(OpCodes.Ldc_I4_1);
    var lblAllow = Instruction.Create(OpCodes.Ldc_I4_0);
    foreach (var fun in SendDeny)                        // 命中上报名单 → 丢包
    {
        cil.Add(Instruction.Create(OpCodes.Ldarg_0));
        cil.Add(Instruction.Create(OpCodes.Ldstr, fun));
        cil.Add(Instruction.Create(OpCodes.Call, StringEq()));
        cil.Add(Instruction.Create(OpCodes.Brtrue, lblDrop));
    }
    cil.Add(Instruction.Create(OpCodes.Br, lblAllow));    // 名单外 → 一律放行
    cil.Add(lblDrop);
    cil.Add(Instruction.Create(OpCodes.Ret));
    cil.Add(lblAllow);
    cil.Add(Instruction.Create(OpCodes.Ret));
}
cfgType.Methods.Add(mStrict);
Console.WriteLine($"[patch] StarryConfig.StrictBlockedRpc: 上报名单判定(deny {SendDeny.Length} / 其余全部放行)");

// 上报名单门：命中名单 → NoteBlockedRpc 记录 + 丢包；名单外 → 原逻辑
void StrictGate(MethodDefinition m, int funIdx, string tag)
{
    var first = m.Body.Instructions[0];
    if (m.ReturnType.FullName != "System.Void")
    { Console.WriteLine($"[skip] {tag}: 非 void 返回，跳过"); return; }
    Instruction LdFun() => funIdx switch
    {
        0 => Instruction.Create(OpCodes.Ldarg_0),
        1 => Instruction.Create(OpCodes.Ldarg_1),
        2 => Instruction.Create(OpCodes.Ldarg_2),
        3 => Instruction.Create(OpCodes.Ldarg_3),
        _ => Instruction.Create(OpCodes.Ldarg, m.Parameters.First(p => p.Index == funIdx - (m.HasThis ? 1 : 0))),
    };
    // 无条件生效（与既有防封一致：块/黑名单/入口 throw 都不挂菜单开关）。
    var seq = new List<Instruction>
    {
        LdFun(),
        Instruction.Create(OpCodes.Call, mStrict),       // 共享判定（true=上报类，丢包）
        Instruction.Create(OpCodes.Brfalse, first),      // 非上报 → 原行为
        LdFun(),                                         // 丢包：记录后直接 ret
        Instruction.Create(OpCodes.Call, mNote),
        Instruction.Create(OpCodes.Ret),                 // 自带 ret：不跳入原函数体（避免跨区栈高不一致）
    };
    for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    Console.WriteLine($"[patch] {tag}: 上报名单门(无条件, fun@ILarg{funIdx})");
}

// ⑤ 免广告直发：AdsFree 原体要求 isLogin+可达网，free_ads 开 → OnAdsSuccessed+action 直发
void AdsFreeOffline()
{
    var t = mod.GetType("AdsManager") ?? throw new Exception("AdsManager 不存在");
    var m = t.Methods.First(x => x.Name == "AdsFree" && x.Parameters.Count == 2)
        ?? throw new Exception("AdsManager.AdsFree(Action,bool) 未找到");
    var onSuccess = t.Methods.First(x => x.Name == "OnAdsSuccessed")
        ?? throw new Exception("OnAdsSuccessed 未找到");
    var invoke = new MethodReference("Invoke", tVoid, m.Parameters[0].ParameterType) { HasThis = true };
    var first = m.Body.Instructions[0];
    var skipInv = Instruction.Create(OpCodes.Nop);
    var orig = Instruction.Create(OpCodes.Nop);
    var seq = new List<Instruction>
    {
        Instruction.Create(OpCodes.Ldstr, "free_ads"),
        Instruction.Create(OpCodes.Call, mGetBool),
        Instruction.Create(OpCodes.Brfalse, orig),
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Ldarg_2),   // countAds（IL: this=0, action=1, countAds=2）
        Instruction.Create(OpCodes.Call, onSuccess),
        Instruction.Create(OpCodes.Ldarg_1),   // action
        Instruction.Create(OpCodes.Dup),
        Instruction.Create(OpCodes.Brfalse, skipInv),
        Instruction.Create(OpCodes.Callvirt, invoke),
        skipInv,
        Instruction.Create(OpCodes.Ret),
        orig,
    };
    for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    Console.WriteLine("[patch] AdsFree 免广告直发(free_ads→OnAdsSuccessed+action)");
}

// ⑥ 商城开门：CheckOnLine(Action) 本地直调 CheckOnLineCallback（原=SendRT 等服务器回包）
void CheckOnLineLocal()
{
    var npm = mod.GetType("NetWorkPlayerMgr") ?? throw new Exception("NetWorkPlayerMgr 不存在");
    var m = npm.Methods.FirstOrDefault(x => x.Name == "CheckOnLine" && x.Parameters.Count == 1
        && x.Parameters[0].ParameterType.Name == "Action")
        ?? throw new Exception("CheckOnLine(Action) 未找到");
    var cb = npm.Methods.First(x => x.Name == "CheckOnLineCallback")
        ?? throw new Exception("CheckOnLineCallback 未找到");
    var first = m.Body.Instructions[0];
    var seq = new List<Instruction>
    {
        Instruction.Create(OpCodes.Ldstr, "block_report"),
        Instruction.Create(OpCodes.Call, mGetBool),
        Instruction.Create(OpCodes.Brfalse, first),
        Instruction.Create(OpCodes.Ldarg_0),
        Instruction.Create(OpCodes.Call, cb),
        Instruction.Create(OpCodes.Ret),
    };
    for (int i = seq.Count - 1; i >= 0; i--) m.Body.Instructions.Insert(0, seq[i]);
    Console.WriteLine("[patch] CheckOnLine(Action): 本地触发开门");
}

// ================= 执行 =================
// ① SendRT 黑名单（v1 已有旧 9 项链，叠加新 10 项链 = 超集，冗余无害）
AddBlacklist(FindM("NetBehaviour", "SendRT"), 2, "NetBehaviour.SendRT 黑名单");
AddBlacklist(FindM("AccountMgr.LoginScene.AccountMgrClient", "SendRT", 3), 2,
    "AccountMgrClient.SendRT(byte,string,object[]) 黑名单");

// ② 执法 RPC no-op
NoOpVoid(FindM("InfinityCore.NetDataCalculater", "PlayerOnLine"), "PlayerOnLine(您的账号已被封禁+Quit)");
NoOpVoid(FindM("NetWorkPlayerMgr", "ShowContentAndCloseGame"), "ShowContentAndCloseGame(弹字+Quit)");
NoOpVoid(FindM("NetWorkPlayerMgr", "DeleteData"), "DeleteData(远程清档+Quit)");
NoOpVoid(FindM("NetWorkPlayerMgr", "Warning"), "Warning(服务器任意弹窗)");
NoOpVoid(FindM("NetWorkPlayerMgr", "ServerMsg"), "ServerMsg(服务器弹窗队列)");
NoOpVoid(FindM("NetWorkPlayerMgr", "ServerMsgBig"), "ServerMsgBig(服务器弹窗队列)");
NoOpVoid(FindM("NetWorkPlayerMgr", "BlockVersion"), "BlockVersion(版本封锁)");
// v7.1 补漏（2026-09-07 用户实测弹出「修改游戏数据 已进行封号处理」）：
//   执法链=服务器推 UpDateBanStatu(true)→本地 ban=true→四处周期检查→CheckGamePlayerAction 冻结+弹窗
NoOpVoid(FindM("InfinityCoreNormal.NetConver", "UpDateBanStatu"), "UpDateBanStatu(服务器写本地封禁标志)");
NoOpVoid(FindM("InfinityCoreNormal.NetConver", "CheckGamePlayerAction"), "CheckGamePlayerAction(冻结+修改游戏数据弹窗,static)");
// 【v7.18 血案修复】OnSaveGuardFailed 曾被 no-op —— 「存档校验失败冻结」是**安全气囊**，不是执法。
// 后果（2026-09-27 实测）：存档校验失败时无人冻结写入 ⇒ 新档直接原地覆写主档
// files/gfldhdsbadbcvc ⇒ 玩家回档到 0 级、旧档永久丢失。
// 现在改为「只屏蔽上报、保留冻结与提示」：SendBA("CheckCodeFiled") 已被 SendBA 名单拦掉，
// 故整个方法**不再 no-op**（恢复原版：saveGuardFailed=true 冻结 + 弹「存档安全校验不通过…点击确认」）。
// 注意：弹窗文案会让玩家紧张，但「能看见的冻结 + 可回滚」远胜「静默丢档」——这是用户明确要求的取舍。

// ⑦c v7.9（2026-09-23 用户实测「涉嫌修改封禁」「您的账号已被封禁」两条提示漏出）：
//   原实现全部挂 block_report 门控，一旦菜单把 block_report 关掉（或被清档/改配置）提示就复活。
//   这两条提示与封号执法**无条件**拦截（no ret，含 banMask 激活 / Time.timeScale=0 冻结 / 断连）：
NoOpVoidAlways(FindM("InfinityCoreNormal.NetConver", "ForceBanGameAciton"), "ForceBanGameAciton(强制封号提示+banMask+断连)");
NoOpVoidAlways(FindM("NetWorkPlayerMgr", "BanGameAciton", 1), "BanGameAciton(涉嫌修改游戏数据提示+banMask+冻结)");
NoOpVoidAlways(FindM("NetWorkPlayerMgr", "BanGame"), "BanGame(封号执法入口→BanGameAciton)");
NoOpVoidAlways(FindM("InfinityCore.NetDataCalculater", "PlayerOnLine", 1), "PlayerOnLine(您的账号已被封禁弹窗)");
NoOpVoidAlways(FindM("InfinityCoreNormal.NetConver", "UpDateBanStatu", 1), "UpDateBanStatu(服务器写本地封禁标志)");
NoOpVoidAlways(FindM("InfinityCoreNormal.NetConver", "CheckGamePlayerAction"), "CheckGamePlayerAction(周期检查→修改游戏数据弹窗+冻结)");

// v7.4 数据外泄拦截（2026-09-07 用户指定四类）：
// 1) 实时主档上传（COS，带 accountid/playtime/savesnccode 元数据）
NoOpVoid(FindM("CloudRealtimeSaveMgr", "UpdateProgress"), "UpdateProgress(实时存档上传触发,static)");
NoOpVoid(FindM("CloudRealtimeSaveMgr", "RequestUpload"), "RequestUpload(上传会话请求,private)");
// v7.7 补漏（2026-09-23）：UpdateProgress/RequestUpload 已 no-op，但服务器仍可能主动推 [Rpc]
// RealtimeSaveUploadSessionCallback → ApplyUploadSession → UploadAsync（COS PUT）绕过二者。
// HasValidUploadSession()=false 时 UploadAsync(40722) 直接 ResetUploadSession+return，回调路径同样失效。
FalseOnEntry(FindM("CloudRealtimeSaveMgr", "HasValidUploadSession"), "HasValidUploadSession(实时存档会话校验)");
// 2) 进度公开（排行榜上报）
NoOpVoid(FindM("NetWorkPlayerMgr", "SendRank"), "SendRank(排行榜上报)");
NoOpVoid(FindM("NetWorkPlayerMgr", "SendRankNew02"), "SendRankNew02(排行榜上报新)");
// 3) 作弊痕迹量化（服务器请求→客户端回数据的一致性检查）
NoOpVoid(FindM("InfinityCore.NetDataCalculater", "GetAllPlayerPFR"), "GetAllPlayerPFR(玩家功能记录)");
NoOpVoid(FindM("InfinityCore.NetDataCalculater", "GetInventory"), "GetInventory(背包偏移对比)");
NoOpVoid(FindM("InfinityCore.NetDataCalculater", "GetMiFaInventory"), "GetMiFaInventory(秘法背包)");
NoOpVoid(FindM("InfinityCore.NetDataCalculater", "GetBaoShiWeight"), "GetBaoShiWeight(宝石权重)");
NoOpVoid(FindM("InfinityCore.NetDataCalculater", "GetUsdDaoJu"), "GetUsdDaoJu(道具使用流水)");
// 4) 完整进度（存档全量同步，含 bcode 改包指纹/验效码计数/伤害作弊检测/异精对比）
NoOpVoid(FindM("InfinityCore.NetDataCalculater", "GetDataInfo"), "GetDataInfo(存档全量同步,含bcode)");
// 5) v7.5 补漏：世界 Boss（巨猿）伤害排行上报——FinishJuYuan 在 Boss 结束时把累计伤害
//    SendBA("AddRank_LargeNum","巨猿",伤害) 报上大数值排行（进度公开+作弊痕迹，v7.4 漏网）
NoOpVoid(FindM("NetWorkPlayerMgr", "FinishJuYuan"), "FinishJuYuan(世界Boss伤害排行上报)");

// ③ COS 直传杀
ThrowOnEntry(FindM("GameDatas.MainSaveCosUploadTool", "UploadMainSaveAsync", 1), "UploadMainSaveAsync(错误存档直传)");
ThrowOnEntry(FindM("GameDatas.MainSaveCosUploadTool", "UploadBanSaveAsync", 1), "UploadBanSaveAsync(封号存档直传)");

// ⑤⑥
AdsFreeOffline();
CheckOnLineLocal();

// ⑦ 静默防封：全部出站通道（SendRT / Send / SendBA 的 string func 重载族）插白名单门
int gateN = 0;
foreach (var t in mod.Types.SelectMany(AllTypes))
{
    foreach (var m in t.Methods.ToList())
    {
        if (m.Name != "SendRT" && m.Name != "Send" && m.Name != "SendBA") continue;
        if (t.Name == "TranferClient") continue;        // 传输层不加门：业务层已全覆盖，避免动线协议
        if (!m.HasBody || m.IsAbstract) continue;
        var ps = m.Parameters;
        if (ps.Count < 1) continue;
        if (!(ps[ps.Count - 1].ParameterType is ArrayType at) || at.ElementType.FullName != "System.Object")
            continue;                                   // 末参必须是 params object[] pars
        int si = -1;
        for (int i = 0; i < ps.Count; i++)
            if (ps[i].ParameterType.FullName == "System.String") { si = i; break; }
        if (si < 0) continue;                           // 无 func 字符串（如 SendRT(byte[])）→ 跳过
        StrictGate(m, m.HasThis ? si + 1 : si, $"{t.Name}.{m.Name}({ps.Count}p fun#{si})");
        gateN++;
    }
}
Console.WriteLine($"[patch] 静默防封闭环: {gateN} 个出站通道已插白名单门");

// ⑦b 腾讯 CLS 日志上报全链路断（block_report 门控；Enqueue 断掉防队列无出口堆内存）
NoOpVoid(FindM("GameDatas.TencentClsLogService", "Enqueue", 1), "CLS日志 Enqueue(队列入口)");
FalseOnEntry(FindM("GameDatas.TencentClsLogService", "TryBeginFlush"), "CLS日志 TryBeginFlush(不启协程)");
FalseOnEntry(FindM("GameDatas.TencentClsLogService", "ConsumeFlushRequest"), "CLS日志 ConsumeFlushRequest");

asm.Write(DST);
Console.WriteLine($"[done] → {DST}");
