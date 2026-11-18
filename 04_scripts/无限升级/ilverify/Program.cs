using Mono.Cecil;
using Mono.Cecil.Cil;

// 只读校验器：对 patched DLL 逐条断言「补丁标记 + 结构性质」，并做功能边界反查。
// 用法: ilverify <dll> [v1|merged]
var path = args[0];
var stage = args.Length > 1 ? args[1] : "merged";
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
var cfg = mod.GetType("Starry.StarryConfig");
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
    ("NetBehaviour","SendRT",null,"CheckMMM",stage == "merged" ? 2 : 1),
    ("AccountMgr.LoginScene.AccountMgrClient","SendRT",3,"CheckMMM",stage == "merged" ? 2 : 1),
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

// ---------- C. delta 层断言 ----------
Console.WriteLine($"[C] delta 层断言（stage={stage}）");
if (stage == "merged")
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
    int gates = types.SelectMany(t => t.Methods).Count(m => HasCall(m, "StarryConfig", "StrictBlockedRpc"));
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

// ---------- D. 功能边界反查：禁止 feat13/14/15 ----------
Console.WriteLine("[D] 功能边界反查（禁止 wxsjdz2 feat13/14/15）");
foreach (var bad in new[] { "skill_projectile_x", "skill_damage_x", "legacy_cost_zero" })
{
    int c = types.SelectMany(t => t.Methods).Count(m => LdstrCount(m, bad) > 0);
    if (c == 0) Ok($"未见 {bad}（未带投掷物/技能伤害/转生归零）"); else No($"出现 {bad} ×{c} —— 越界！");
}
foreach (var (T, N, pc) in new (string, string, int?)[] { ("SkillAssets","GetBulletAmount",null), ("SkillAssets","get_damage",null), ("SkillAssets","GetSafeDamage",null), ("LegacySystem.LegacyCell",null,null) })
{
    if (N == null) continue;
    var t = mod.GetType(T);
    var md = t?.Methods.FirstOrDefault(x => x.Name == N && (pc == null || x.Parameters.Count == pc));
    if (md == null) { Console.WriteLine($"  [N/A ] {T}::{N}（新包不存在或签名变化，边界无需查）"); continue; }
    bool dirty = md.Body.Instructions.Any(i => i.OpCode.Code == Code.Ldstr && ((string)i.Operand).StartsWith("skill_"));
    if (!dirty) Ok($"{T}::{N} 未被 feat13/14 补丁污染"); else No($"{T}::{N} 出现 skill_* 键 —— 越界！");
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
