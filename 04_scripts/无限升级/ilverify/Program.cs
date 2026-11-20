using Mono.Cecil;
using Mono.Cecil.Cil;

// 只读校验器：对 patched DLL 逐条断言「补丁标记 + 结构性质」，并做功能边界反查。
// 用法: ilverify <dll> [v1|merged]
var path = args[0];
var stage = args.Length > 1 ? args[1] : "merged";
// 2026-11-20：19 项载荷（_allfix_19feat）之后，D 段由「禁止」翻成「必须有」8 项。
// 复核旧 11 项产物（如 allfix / hf6 / hf7）时传 --feat11 跳过该段，避免把正常产物判 FAIL。
bool feat11 = args.Any(a => a == "--feat11");
var asm = AssemblyDefinition.ReadAssembly(path);
var mod = asm.MainModule;
int pass = 0, fail = 0;
var fails = new List<string>();

IEnumerable<TypeDefinition> All(TypeDefinition t)
{
    yield return t;
    foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x;
}
var types = mod.Types.SelectMany(All).ToList();

void Ok(string m) { pass++; Console.WriteLine($"  [PASS] {m}"); }
void No(string m) { fail++; fails.Add(m); Console.WriteLine($"  [FAIL] {m}"); }

int LdstrCount(MethodDefinition m, string s)
{
    if (!m.HasBody) return -1;
    return m.Body.Instructions.Count(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == s);
}
bool HasCall(MethodDefinition m, string typeName, string name)
{
    if (!m.HasBody) return false;
    return m.Body.Instructions.Any(i => i.Operand is MethodReference mr && mr.Name == name && mr.DeclaringType.Name == typeName);
}
int CallCountAll(string typeName, string name) =>
    types.SelectMany(t => t.Methods).Count(m => HasCall(m, typeName, name) && m.Body.Instructions.Count(i =>
        i.Operand is MethodReference mr && mr.Name == name && mr.DeclaringType.Name == typeName) > 0);

MethodDefinition M(string typeFull, string name, int? pc = null)
{
    var t = mod.GetType(typeFull);
    if (t == null) return null;
    var l = t.Methods.Where(x => x.Name == name && (pc == null || x.Parameters.Count == pc)).ToList();
    return l.Count == 1 ? l[0] : (l.Count == 0 ? null : null);
}
// 重载族：返回全部同名候选（同名多载时按「命中标记」判定，不猜）
List<MethodDefinition> Ms(string typeFull, string name, int? pc = null)
{
    var t = mod.GetType(typeFull);
    if (t == null) return new List<MethodDefinition>();
    return t.Methods.Where(x => x.Name == name && (pc == null || x.Parameters.Count == pc)).ToList();
}

Console.WriteLine($"== ilverify {Path.GetFileName(path)}  stage={stage}  types(top)={mod.Types.Count} all={types.Count}");

// ---------- A. StarryConfig 骨架 ----------
Console.WriteLine("[A] StarryConfig 骨架");
// 2026-11-19：helper 宿主可能是官方已有类型（不新增类型）或历史新增类型，两种都接受
var cfg = mod.GetType("Starry.StarryConfig") ?? mod.GetType("$Obfuz$ConstFieldHolder$0");
if (cfg == null) No("StarryConfig 不存在");
else
{
    foreach (var (mn, need) in new[] { ("Get", stage), ("GetBool", stage), ("GetInt", stage), ("NoteBlockedRpc", stage), ("StrictBlockedRpc", stage) })
    {
        var e = cfg.Methods.FirstOrDefault(x => x.Name == mn);
        if (e == null && mn == "StrictBlockedRpc" && stage == "v1") { Console.WriteLine($"  [N/A ] StrictBlockedRpc（{stage} 不含 delta）"); continue; }
        if (e != null && e.HasBody) Ok($"StarryConfig::{mn}  存在 ins={e.Body.Instructions.Count}");
        else No($"StarryConfig::{mn}  缺失/无体");
    }
}

// ---------- B. ilpatch 31 条落点（stage=deltaonly 时不适用：该版**故意不注入** 31 条） ----------
if (stage != "deltaonly")
{
// ---------- B. ilpatch 31 条：逐方法断言 key 标记 ----------
Console.WriteLine("[B] ilpatch 31 条落点标记");
// (typeFull, method, pc, key, 期望该方法内该 key 的出现次数)
var t31 = new (string T, string N, int? Pc, string Key, int Cnt)[]
{
    ("InfinityCore.ItemUseAnomalyMonitor","CheckAmount",null,"block_report",1),
    ("InfinityCore.ItemAddSourceAnomalyMonitor","CheckAddSource",null,"block_report",1),
    ("DamageCheatRecordData","Record",null,"block_report",1),
    ("NetWorkPlayerMgr","CheckBan",null,"block_report",1),
    ("NetWorkPlayerMgr","CheckBanReport",null,"block_report",1),
    ("NetWorkPlayerMgr","CheckBan2",null,"block_report",1),
    ("NetWorkPlayerMgr","BanGameAciton",null,"block_report",1),
    ("NetWorkPlayerMgr","Report",null,"block_report",1),
    ("NetBehaviour","SendRT",null,"CheckMMM",stage == "merged" || stage == "vtpin" ? 2 : 1),
    ("AccountMgr.LoginScene.AccountMgrClient","SendRT",3,"CheckMMM",stage == "merged" || stage == "vtpin" ? 2 : 1),
    ("InfinityCoreNormal.NetConver","ForceBanGameAciton",null,"block_report",1),
    ("PlayerMgr","OnHit",null,"god",1),
    ("Enemy","OnHit",null,"oneshot",1),
    ("BossEnemy","OnHit",null,"oneshot",1),
    ("Calculater","CalculateSkillUpgrad",null,"skill_free",1),
    ("SkillDetialWindow","QuickUpgrade",null,"skill_free",1),
    ("SkillDetialWindow","CanLevelUp",null,"skill_free",1),
    ("GameData","MinusItem",null,"item_keep",1),
    ("GameData","AddItem",4,"item_double",1),
    ("NetWorkPlayerMgr","GetAdsRewardCallBack",null,"ads_reward_x",1),
    ("DataManager","AddExp",null,"exp_multi",1),
    ("EnemyGroupData","CanChanllage",3,"mijing_free",4),
    ("GameData","GetAmount",1,"skin_unlock",1),
    ("GameData","MinusItem",null,"skin_unlock",1),
    ("GameData","HasClothes",1,"skin_unlock",1),
    ("SkillDetialWindow","RefreshSkillSkins",null,"skin_unlock",1),
    ("InfinityCore.FuntionButtonMgr","Check",1,"func_unlock",1),
    ("UnLockChecker","CheckIsUnlock",null,"func_unlock",1),
    ("CiYuan_DiffCell","IsUnLock",null,"func_unlock",1),
    ("WorldSelectCell","CanUse",1,"func_unlock",1),
    ("WorldSelectCell","Show",1,"func_unlock",3),
};
int i31 = 0;
foreach (var e in t31)
{
    i31++;
    var cands = Ms(e.T, e.N, e.Pc);
    if (cands.Count == 0) { No($"#{i31} {e.T}::{e.N} 方法未找到"); continue; }
    var hit = cands.FirstOrDefault(x => LdstrCount(x, e.Key) == e.Cnt);
    if (hit != null)
        Ok($"#{i31} {e.T.Split('.').Last()}::{e.N}{(cands.Count>1?$"（{cands.Count} 重载中唯一命中）":"")}  \"{e.Key}\"×{e.Cnt}  签名=({string.Join(",", hit.Parameters.Select(z=>z.ParameterType.Name))})");
    else
        No($"#{i31} {e.T.Split('.').Last()}::{e.N} 无候选满足 \"{e.Key}\"×{e.Cnt}（实际 {string.Join("/", cands.Select(x=>LdstrCount(x,e.Key)))}）");
}
// 免广告（ShowAdsNewFlow.Show → free_ads）
{
    var md = M("AdsManager/ShowAdsNewFlow", "Show");
    var c = md == null ? -1 : LdstrCount(md, "free_ads");
    if (c >= 1) Ok($"#22 免广告 AdsManager/ShowAdsNewFlow::Show  \"free_ads\"×{c}（merged 另含 AdsManager.AdsFree 直发，见 C 段）"); else No($"#22 ShowAdsNewFlow::Show free_ads 缺失");
}

}
else Console.WriteLine("[B] 跳过（stage=deltaonly：本版不含功能补丁，按设计）");

// ---------- C. delta 层断言 ----------
Console.WriteLine($"[C] delta 层断言（stage={stage}）");
if (stage == "merged" || stage == "vtpin" || stage == "deltaonly")
{
    var ndc = mod.GetType("InfinityCore.NetDataCalculater");
    var renamed = new (string New, string Old)[]
    { ("$C","PlayerOnLine"), ("$B","GetAllPlayerPFR"), ("$G","GetInventory"),
      ("$H","GetBaoShiWeight"), ("$i","GetUsdDaoJu"), ("$L","GetDataInfo") };
    foreach (var (nn, old) in renamed)
    {
        var md = ndc?.Methods.FirstOrDefault(x => x.Name == nn);
        if (md == null) { No($"重定位 {old}->{nn} 方法缺失"); continue; }
        bool firstIsRet = md.Body.Instructions[0].OpCode.Code == Code.Ret;
        bool hasGate = md.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == "block_report");
        if (firstIsRet || hasGate) Ok($"重定位 {old} -> {nn}  已插 {(firstIsRet ? "无条件 ret" : "block_report 门")}  ins={md.Body.Instructions.Count}");
        else No($"重定位 {old} -> {nn}  未见补丁痕迹");
    }
    // 2026-11-18 lead 裁决：GetMiFaInventory($h) 不硬塞 ⇒ 断言「未被打」并标待验证
    {
        var h = ndc?.Methods.FirstOrDefault(x => x.Name == "$h");
        if (h == null) No("$h 方法不存在");
        else
        {
            bool touched = h.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr && (string)i.Operand == "block_report")
                        || h.Body.Instructions[0].OpCode.Code == Code.Ret;
            if (!touched) Ok("GetMiFaInventory($h) 未被注入（13.101 落点无法确信 ⇒ 待验证，防封覆盖 18/19 通道）");
            else No("GetMiFaInventory($h) 出现注入痕迹，与「剔除」裁决矛盾");
        }
    }
    // 无条件执法 no-op（首条必须是 ret）
    var always = new (string T, string N, int? Pc)[]
    {
        ("InfinityCoreNormal.NetConver","ForceBanGameAciton",null),
        ("NetWorkPlayerMgr","BanGameAciton",1),
        ("NetWorkPlayerMgr","BanGame",null),
        ("InfinityCoreNormal.NetConver","UpDateBanStatu",1),
        ("InfinityCoreNormal.NetConver","CheckGamePlayerAction",null),
    };
    foreach (var (T,N,Pc) in always)
    {
        var md = M(T,N,Pc);
        if (md == null) { No($"无条件 no-op {T}::{N} 未找到"); continue; }
        if (md.Body.Instructions[0].OpCode.Code == Code.Ret) Ok($"无条件 no-op {T.Split('.').Last()}::{N}  首条=ret");
        else No($"无条件 no-op {T.Split('.').Last()}::{N}  首条={md.Body.Instructions[0].OpCode}");
    }
    // 静默防封闭环：StrictBlockedRpc 调用数
    // 2026-11-19：宿主可能不是 StarryConfig（不新增类型方案），按实际宿主类型名统计
    var hostName = cfg != null ? cfg.Name : "StarryConfig";
    int gates = types.SelectMany(t => t.Methods).Count(m => HasCall(m, hostName, "StrictBlockedRpc"));
    if (gates == 19) Ok($"静默防封闭环 StarryConfig::StrictBlockedRpc 调用点 {gates} 个（历史口径 19）");
    else No($"静默防封闭环调用点 {gates} 个（期望 19）");
    // CLS 日志链
    foreach (var (T,N,kind) in new[] { ("GameDatas.TencentClsLogService","Enqueue","gate"), ("GameDatas.TencentClsLogService","TryBeginFlush","false"), ("GameDatas.TencentClsLogService","ConsumeFlushRequest","false") })
    {
        var md = M(T,N,null);
        if (md == null) { No($"CLS {N} 未找到"); continue; }
        var i0 = md.Body.Instructions[0];
        if (kind == "gate" ? (i0.OpCode.Code == Code.Ldstr && (string)i0.Operand == "block_report") : (i0.OpCode.Code == Code.Ldstr && (string)i0.Operand == "block_report"))
            Ok($"CLS {N} 入口门已插");
        else No($"CLS {N} 入口未见门 (首条 {i0.OpCode})");
    }
    // COS 直传 throw
    foreach (var T in new[] { "GameDatas.MainSaveCosUploadTool" })
        foreach (var N in new[] { "UploadMainSaveAsync", "UploadBanSaveAsync" })
        {
            var md = M(T, N, 1);
            if (md == null) { No($"COS {N} 未找到"); continue; }
            bool thr = md.Body.Instructions.Any(i => i.OpCode.Code == Code.Throw);
            if (thr) Ok($"COS {N} 入口 throw 已插"); else No($"COS {N} 未见 throw");
        }
}

// ---------- D. 8 项增量（feat13-20）：stage=merged 时断言「必须有」 ----------
// 2026-11-20 翻转：19 项载荷（..._allfix_19feat.dll）之后，原「禁止 skill_projectile_x / skill_damage_x /
// legacy_cost_zero」的反查必须改成「必须有」，否则 8 项打完后本条必然 FAIL。
Console.WriteLine($"[D] 8 项增量断言（feat13-20，stage={stage}{(feat11 ? "，--feat11 跳过" : "")}）");
if (stage == "merged" && !feat11)
{
    // (typeFull, method, pc, key, 期望次数)
    var tFeat = new (string T, string N, int? Pc, string Key, int Cnt, string Tag)[]
    {
        ("AdsManager/ShowAdsNewFlow","Show",null,"shop_ads_unlimited",1,"feat13 商店广告无限（Show 入口清零）"),
        ("ShopWindow","Refresh",0,"shop_ads_unlimited",1,"feat13 商店广告无限（Refresh 入口清零）"),
        ("GameData","MinusItem",null,"item_reverse_x",1,"feat15 物品反增倍率（本体认负额）"),
        ("LunHuiWindow","WindowOpen",0,"zhuansheng_free",2,"feat16 转生时间无限制（2 处比较点）"),
        ("SkillAssets","GetBulletAmount",null,"skill_projectile_x",1,"feat17 技能投掷物倍数"),
        ("SkillAssets","get_damage",null,"skill_damage_x",2,"feat18 技能伤害倍数（physic 分支 ret）"),
        ("SkillAssets","GetSafeDamage",null,"skill_damage_x",2,"feat18 技能伤害倍数（skill 分支 ret）"),
        ("PlayerGo","ChangeClothes",1,"chenghao_lv",1,"feat20 称号等级（开局/换装写入）"),
        ("PlayerGo","FixedUpdate",0,"chenghao_lv",1,"feat20 称号等级（变化即重刷）"),
    };
    foreach (var e in tFeat)
    {
        var cands = Ms(e.T, e.N, e.Pc);
        if (cands.Count == 0) { No($"{e.Tag}：{e.T}::{e.N} 方法未找到"); continue; }
        var hit = cands.FirstOrDefault(x => LdstrCount(x, e.Key) == e.Cnt);
        if (hit != null) Ok($"{e.Tag}：{e.Key} ×{e.Cnt}");
        else No($"{e.Tag}：{e.Key} 期望 ×{e.Cnt}，实际 {string.Join("/", cands.Select(x => LdstrCount(x, e.Key)))}");
    }
    // feat14：全模块 item_reverse 门数（allfix 实测 136 处可打点；排除 8 处存档链/库存驱动循环）
    // 门是按「MinusItem 调用点」插的，一个方法可能有多处（本包 136 门分布在 122 个方法里）⇒ 数指令不数方法
    int rev = types.SelectMany(t => t.Methods).Sum(m => m.HasBody ? LdstrCount(m, "item_reverse") : 0);
    // 137 = 136 个调用点门 + 1 处 MinusItem 本体里 item_reverse_x 取数块的 item_reverse 开关
    if (rev == 137) Ok("feat14/15 物品使用反增：item_reverse 键 137 处（136 调用点 + 1 本体内）");
    else No($"feat14 item_reverse 键 {rev} 处（期望 137 = 136 调用点 + 1 本体）");
    // feat19：成本点归零 + helper
    var cellT = mod.GetType("LegacySystem.LegacyCell");
    int cost = cellT == null ? -1 : cellT.Methods.Where(m => m.HasBody)
        .Sum(m => m.Body.Instructions.Count(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"));
    if (cost == 13) Ok("feat19 转生无条件升级：LegacyCell 成本点 13 处被 StarryCostZero 包裹（实测值）");
    else No($"feat19 LegacyCell StarryCostZero 落点 {cost} 处（期望 13）");
    bool helper = types.SelectMany(t => t.Methods).Any(m => m.Name == "StarryCostZero" && m.HasBody);
    if (helper) Ok("feat19 helper StarryCostZero 已注入（宿主类型上，不新增类型）");
    else No("feat19 helper StarryCostZero 缺失");
    // 越界反查：11 线 feat3 的「倍数生效开关」键在本包不应存在（21 槽用 InputValue 语义，无 on 开关）
    foreach (var bad in new[] { "skill_projectile_on", "skill_damage_on", "zhuan_sheng_free_point_on" })
    {
        int c = types.SelectMany(t => t.Methods).Count(m => LdstrCount(m, bad) > 0);
        if (c == 0) Ok($"未见 {bad}（21 槽口径无「生效开关」）"); else No($"出现 {bad} ×{c} —— 越界！");
    }
}
else if (feat11) Console.WriteLine("  [N/A ] --feat11：按 11 项旧口径复核，跳过 8 项断言");
else Console.WriteLine("  [N/A ] 非 merged 产物：8 项增量按设计不在本 stage");

// ---------- F. 版本钉死族（vtpin）：stage=vtpin 断言「已钉死」；stage=merged 断言「未被钉死」 ----------
Console.WriteLine($"[F] 版本钉死族（stage={stage}）");
{
    bool pin = stage == "vtpin";
    var gv = M("ResVersionApi", "GetVersion", 0);
    if (gv == null) No("ResVersionApi::GetVersion() 未找到");
    else
    {
        var i0 = gv.Body.Instructions[0]; var i1 = gv.Body.Instructions[1];
        bool pinned = i0.OpCode.Code == Code.Ldc_I8 && (long)i0.Operand == long.MaxValue && i1.OpCode.Code == Code.Ret;
        if (pin == pinned) Ok($"ResVersionApi.GetVersion() {(pin ? "已钉死 Int64.MaxValue" : "未钉死（基线口径）")}  ins={gv.Body.Instructions.Count}");
        else No($"ResVersionApi.GetVersion() {(pin ? "未钉死" : "被意外钉死")}  首条={i0.OpCode} {i0.Operand}");
    }
    var anchors = new (string T, string N, int? Pc)[]
    {
        ("NetWorkPlayerMgr","Vch",0), ("NetWorkPlayerMgr","BlockVersion",0),
        ("MainUIFuntions","ShowNewVersion",0), ("MainUIFuntions","OnNewVersionClick",0),
        ("LowVersionMgr","NeedUpdate",0), ("LowVersionMgr","OpenUrl",1),
        ("LowVersionMgr_InitScene","ShowOnShow",0), ("LowVersionMgr_InitScene","OnDownLoadClick",0),
        ("InfinityCore.NetDataCalculater","$o",1),
    };
    foreach (var (T, N, Pc) in anchors)
    {
        var md = M(T, N, Pc);
        if (md == null) { No($"版本门 {T}::{N} 未找到"); continue; }
        bool isRet = md.Body.Instructions[0].OpCode.Code == Code.Ret;
        if (pin == isRet) Ok($"版本门 {T.Split('.').Last()}::{N} {(pin ? "首条=ret（已 no-op）" : "未 no-op（基线）")}");
        else No($"版本门 {T.Split('.').Last()}::{N} {(pin ? "首条非 ret" : "被意外 no-op")}");
    }
    var cur = M("InfinityCore.NetDataCalculater", "$o", 1);
    if (cur == null) No("NetDataCalculater::$o 未找到（CurrentVersion 重定位失效）");
    else if (HasCall(cur, "Application", "get_version")) Ok("CurrentVersion→$o 原体保留（含 Application.get_version，指纹仍可复核）");
    else No("NetDataCalculater::$o 内未见 Application.get_version（指纹不符）");
    var vtext = new (string T, string N, int? Pc)[]
    {
        ("InfinityCore.TransferHandler","CfgCallBack",1), ("NetWorkPlayerMgr","UpDatePlayerData",7), ("NetWorkPlayerMgr","UpDatePlayerMainData",1),
    };
    foreach (var (T, N, Pc) in vtext)
    {
        var md = M(T, N, Pc);
        if (md == null) { No($"版本常量 {T}::{N} 未找到"); continue; }
        int gvn = md.Body.Instructions.Count(i => i.Operand is MethodReference mr && mr.Name == "get_version" && mr.DeclaringType.FullName == "UnityEngine.Application");
        int c999 = LdstrCount(md, "999999");
        if (pin ? (gvn == 0 && c999 == 1) : (gvn == 1 && c999 == 0))
            Ok($"Application.version {T.Split('.').Last()}::{N} {(pin ? "已换常量 999999" : "保持官方 get_version")}  (get_version={gvn}, 999999x{c999})");
        else No($"Application.version {T.Split('.').Last()}::{N} 形态异常 (get_version={gvn}, 999999x{c999})");
    }
}

// ---------- E. 结构完整性：分支目标必须落在本方法体内 ----------
Console.WriteLine("[E] 结构完整性（分支目标 / ret 存在性）");
{
    int bad = 0, checkedM = 0, checkedI = 0;
    foreach (var t in types)
        foreach (var md in t.Methods)
        {
            if (!md.HasBody) continue;
            checkedM++;
            var set = new HashSet<Instruction>(md.Body.Instructions);
            foreach (var i in md.Body.Instructions)
            {
                checkedI++;
                if (i.Operand is Instruction tgt && !set.Contains(tgt)) { bad++; Console.WriteLine($"  [FAIL] {t.Name}::{md.Name} IL_{i.Offset:X4} 分支目标越界"); }
                else if (i.Operand is Instruction[] arr && arr.Any(x => !set.Contains(x))) { bad++; Console.WriteLine($"  [FAIL] {t.Name}::{md.Name} IL_{i.Offset:X4} switch 目标越界"); }
            }
            if (md.ReturnType.FullName == "System.Void" && !md.Body.Instructions.Any(i => i.OpCode.Code == Code.Ret) && !md.Body.Instructions.Any(i => i.OpCode.Code == Code.Throw))
            { bad++; Console.WriteLine($"  [FAIL] {t.Name}::{md.Name} 无 ret/throw 出口"); }
        }
    if (bad == 0) Ok($"全模块 {checkedM} 个方法 / {checkedI} 条指令：分支目标全部在本体内，void 方法均有出口");
    else No($"结构完整性异常 {bad} 处");
}

Console.WriteLine($"\n== 汇总: PASS={pass} FAIL={fail}");
if (fail > 0) { foreach (var f in fails) Console.WriteLine("   FAIL: " + f); Environment.ExitCode = 1; }
