// ilpatch_vc1637_feat3 — 无限升级 11 线（vc1637）三功能增量补丁（2026-11-04）
//
// 输入 = 11 线既有补丁产物（hf4 = v1+δ+vc1637 增量，2,026,496 B；官方现网数组 2,028,544 B）
// 输出 = 再加三个新功能（与 12 线 wxsjdz2 的落点、判据、键名逐字一致）：
//   ① 技能投掷物倍数  skill_projectile_x (1..100, 默认 1)   SkillAssets::GetBulletAmount 末尾 ×N
//   ② 技能伤害倍数    skill_damage_x    (1..1000, 默认 1)   get_damage 两处 ret + GetSafeDamage 两处
//   ③ 转生无条件升级  zhuan_sheng_free_point (bool, 默认关) LegacyCell 内 GetLegacyCost* 结果归零
//
// 用法：
//   dotnet run -- --src <in.dll> --dst <out.dll> [--maxlen 2028544]
//   dotnet run -- --src <in.dll> --verify
// 幂等：已含标记串则原样复制（重复跑不会二次放大）。
using Mono.Cecil;
using Mono.Cecil.Cil;

static class Feat3
{
    // hf4 基线实测成本点分布：PiLiangFuntion 2 / Show 4 / Upgrade 4 / Upgrade10 1 / Upgrade100 1 / ReturnOver5000 1
    const int kExpectLegacySites = 13;

    static MethodReference mGetBool, mGetInt, mOpMul;
    static TypeReference tLargeNumber;

    static int Main(string[] args)
    {
        string src = Env("WXSJ_SRC"), dst = Env("WXSJ_DST");
        long maxLen = 0;
        bool verifyOnly = false;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--src": src = args[++i]; break;
                case "--dst": dst = args[++i]; break;
                case "--maxlen": maxLen = long.Parse(args[++i]); break;
                case "--verify": verifyOnly = true; break;
                default: Console.WriteLine($"[warn] 未知参数 {args[i]}"); break;
            }
        }
        if (string.IsNullOrEmpty(src) || !File.Exists(src)) { Console.WriteLine($"[ERR] 输入不存在: {src}"); return 2; }
        var asm = AssemblyDefinition.ReadAssembly(src);
        var mod = asm.MainModule;
        Console.WriteLine($"[src] {src}  ({new FileInfo(src).Length} B)");
        if (!ResolveBase(mod)) return 3;

        if (verifyOnly)
        {
            bool okv = Verify(mod, false);
            Console.WriteLine(okv ? "[verify] PASS" : "[verify] FAIL");
            return okv ? 0 : 1;
        }

        int n1 = PatchSkillProjectile(mod);
        int n2 = PatchSkillDamage(mod);
        int n3 = PatchLegacyPointFree(mod);
        Console.WriteLine($"[patch] projectile={n1} / damage rets={n2} / legacy sites={n3}");
        int n4 = PatchMultiplierGates(mod);
        Console.WriteLine($"[patch] 倍数生效开关门禁 {n4} 处");

        if (!Verify(mod, false)) { Console.WriteLine("[ERR] 自检失败，不写盘"); return 4; }

        asm.Write(dst);
        long len = new FileInfo(dst).Length;
        if (maxLen > 0)
        {
            Console.WriteLine($"[out] {dst}  {len} B  (官方数组 {maxLen} 余量 {maxLen - len} B)");
            if (len > maxLen) { Console.WriteLine("[ERR] 超过官方数组长度，so 侧 pLen<=len 不成立"); return 5; }
        }
        else Console.WriteLine($"[out] {dst}  {len} B");

        var re = AssemblyDefinition.ReadAssembly(dst);
        if (!Verify(re.MainModule, false)) { Console.WriteLine("[ERR] 落盘后复读校验失败"); return 6; }
        Console.WriteLine("[verify] PASS (落盘后复读)");
        return 0;
    }

    static string Env(string k) => Environment.GetEnvironmentVariable(k) ?? "";

    static bool ResolveBase(ModuleDefinition mod)
    {
        var cfg = mod.GetType("Starry.StarryConfig");
        if (cfg == null) { Console.WriteLine("[ERR] 缺 Starry.StarryConfig（输入必须是 11 线补丁产物）"); return false; }
        mGetBool = cfg.Methods.First(m => m.Name == "GetBool");
        mGetInt = cfg.Methods.First(m => m.Name == "GetInt");
        var sa = mod.GetType("SkillAssets");
        if (sa == null) { Console.WriteLine("[ERR] 缺 SkillAssets 类型"); return false; }
        var gd = sa.Methods.FirstOrDefault(m => m.Name == "get_damage");
        if (gd == null) { Console.WriteLine("[ERR] 缺 SkillAssets::get_damage"); return false; }
        tLargeNumber = gd.ReturnType;
        foreach (var ins in gd.Body.Instructions)
        {
            if (ins.Operand is MethodReference mr && mr.Name == "op_Multiply"
                && mr.DeclaringType.Name == "LargeNumber" && mr.Parameters.Count == 2
                && mr.Parameters[1].ParameterType.MetadataType == MetadataType.Double)
            { mOpMul = mr; break; }
        }
        if (mOpMul == null) { Console.WriteLine("[ERR] 没在 get_damage 里找到 LargeNumber::op_Multiply(LargeNumber,double)"); return false; }
        return true;
    }

    // ---------- ① 技能投掷物倍数 ----------
    static int PatchSkillProjectile(ModuleDefinition mod)
    {
        var m = mod.GetType("SkillAssets")?.Methods.FirstOrDefault(x => x.Name == "GetBulletAmount");
        if (m == null || !m.HasBody) { Console.WriteLine("[ERR] GetBulletAmount 未找到"); return 0; }
        if (HasLdstr(m, "skill_projectile_x")) { Console.WriteLine("[skip] 投掷物倍数已是新形态"); return 1; }

        var il = m.Body.GetILProcessor();
        var vRes = new VariableDefinition(mod.TypeSystem.Int32);
        var vX = new VariableDefinition(mod.TypeSystem.Int32);
        m.Body.Variables.Add(vRes);
        m.Body.Variables.Add(vX);
        m.Body.InitLocals = true;

        var ret = m.Body.Instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);
        if (ret == null) { Console.WriteLine("[ERR] GetBulletAmount 无 ret"); return 0; }

        var skip = Instruction.Create(OpCodes.Ldloc, vRes);
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Stloc, vRes),
            Instruction.Create(OpCodes.Ldstr, "skill_projectile_x"),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Call, mGetInt),
            Instruction.Create(OpCodes.Stloc, vX),
            Instruction.Create(OpCodes.Ldloc, vX),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble, skip),
            Instruction.Create(OpCodes.Ldloc, vRes),
            Instruction.Create(OpCodes.Ldloc, vX),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Stloc, vRes),
            skip,
            Instruction.Create(OpCodes.Ret),
        };
        foreach (var i in seq) il.InsertBefore(ret, i);
        il.Remove(ret);
        if (m.Body.MaxStackSize < 4) m.Body.MaxStackSize = 4;
        Console.WriteLine($"[patch] GetBulletAmount ×skill_projectile_x");
        return 1;
    }

    // ---------- ② 技能伤害倍数 ----------
    static int PatchSkillDamage(ModuleDefinition mod)
    {
        var sa = mod.GetType("SkillAssets");
        int patched = 0;
        foreach (var name in new[] { "get_damage", "GetSafeDamage" })
        {
            var m = sa.Methods.FirstOrDefault(x => x.Name == name);
            if (m == null || !m.HasBody) { Console.WriteLine($"[ERR] SkillAssets::{name} 未找到"); continue; }
            if (HasLdstr(m, "skill_damage_x")) { Console.WriteLine($"[skip] {name} 已是新形态"); continue; }

            var il = m.Body.GetILProcessor();
            var vL = new VariableDefinition(tLargeNumber);
            var vX = new VariableDefinition(mod.TypeSystem.Int32);
            m.Body.Variables.Add(vL);
            m.Body.Variables.Add(vX);
            m.Body.InitLocals = true;

            var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
            foreach (var ret in rets)
            {
                var skip = Instruction.Create(OpCodes.Ldloc, vL);
                var block = new List<Instruction>
                {
                    Instruction.Create(OpCodes.Stloc, vL),
                    Instruction.Create(OpCodes.Ldstr, "skill_damage_x"),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Call, mGetInt),
                    Instruction.Create(OpCodes.Stloc, vX),
                    Instruction.Create(OpCodes.Ldloc, vX),
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Ble, skip),
                    Instruction.Create(OpCodes.Ldloc, vL),
                    Instruction.Create(OpCodes.Ldloc, vX),
                    Instruction.Create(OpCodes.Conv_R8),
                    Instruction.Create(OpCodes.Call, mOpMul),
                    Instruction.Create(OpCodes.Stloc, vL),
                    skip,
                    Instruction.Create(OpCodes.Ret),
                };
                foreach (var i in block) il.InsertBefore(ret, i);
                il.Remove(ret);
                patched++;
            }
            if (m.Body.MaxStackSize < 5) m.Body.MaxStackSize = 5;
            Console.WriteLine($"[patch] {name} ×skill_damage_x @ {rets.Count} 个 ret");
        }
        return patched;
    }

    // ---------- ③ 转生无条件升级 ----------
    static int PatchLegacyPointFree(ModuleDefinition mod)
    {
        var cell = mod.GetType("LegacySystem.LegacyCell");
        if (cell == null) { Console.WriteLine("[ERR] LegacySystem.LegacyCell 未找到"); return 0; }

        foreach (var mm in cell.Methods)
            if (mm.HasBody && mm.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"))
            { Console.WriteLine("[skip] LegacyCell 已是新形态"); return CountCostZeroSites(cell); }

        EnsureHelper(mod, out var costM);

        int sites = 0;
        foreach (var mm in cell.Methods)
        {
            if (!mm.HasBody) continue;
            var il = mm.Body.GetILProcessor();
            var calls = mm.Body.Instructions
                .Where(i => i.Operand is MethodReference r
                            && r.DeclaringType.Name == "Calculater"
                            && r.Name.StartsWith("GetLegacyCost", StringComparison.Ordinal))
                .ToList();
            foreach (var c in calls) { il.InsertAfter(c, Instruction.Create(OpCodes.Call, costM)); sites++; }
            if (calls.Count > 0) Console.WriteLine($"[patch] LegacyCell::{mm.Name} 成本点 {calls.Count} 处");
        }
        if (sites != kExpectLegacySites)
            Console.WriteLine($"[warn] 成本点 {sites} != 基线 {kExpectLegacySites}（官方改过 IL？需人工复核）");
        return sites;
    }

    static TypeDefinition EnsureHelper(ModuleDefinition mod, out MethodDefinition costM)
    {
        var t = mod.GetType("Starry.StarryLegacy");
        if (t == null)
        {
            t = new TypeDefinition("Starry", "StarryLegacy",
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                mod.TypeSystem.Object);
            mod.Types.Add(t);
        }
        var exist = t.Methods.FirstOrDefault(x => x.Name == "StarryCostZero");
        if (exist != null) { costM = exist; return t; }

        costM = new MethodDefinition("StarryCostZero",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, tLargeNumber);
        costM.Parameters.Add(new ParameterDefinition("cost", ParameterAttributes.None, tLargeNumber));
        t.Methods.Add(costM);

        var body = costM.Body;
        body.InitLocals = true;
        body.MaxStackSize = 2;
        var vZero = new VariableDefinition(tLargeNumber);
        body.Variables.Add(vZero);
        var il = body.GetILProcessor();
        var retOrig = Instruction.Create(OpCodes.Ldarg_0);
        il.Append(Instruction.Create(OpCodes.Ldstr, "zhuan_sheng_free_point"));
        il.Append(Instruction.Create(OpCodes.Call, mGetBool));
        il.Append(Instruction.Create(OpCodes.Brfalse, retOrig));
        il.Append(Instruction.Create(OpCodes.Ldloca, vZero));
        il.Append(Instruction.Create(OpCodes.Initobj, tLargeNumber));
        il.Append(Instruction.Create(OpCodes.Ldloc, vZero));
        il.Append(Instruction.Create(OpCodes.Ret));
        il.Append(retOrig);
        il.Append(Instruction.Create(OpCodes.Ret));
        return t;
    }

    static int CountCostZeroSites(TypeDefinition cell)
    {
        int n = 0;
        foreach (var mm in cell.Methods)
            if (mm.HasBody) n += mm.Body.Instructions.Count(i => i.Operand is MethodReference r && r.Name == "StarryCostZero");
        return n;
    }

    // ---------- ④ 三个倍数输入框的「生效开关」 ----------
    // 用户 2026-11-04 定：输入框只存倍数，是否生效由独立开关控制（默认关 = 官方原样）。
    // 做法：在每处 StarryConfig.GetInt("<key>",1) 之后插入
    //   if (!GetBool("<key>_on")) value = 1;
    // ⇒ 关闭时 DLL 侧读到的就是 1，配置里的倍数原样保留（再打开即恢复）。
    static readonly (string valKey, string onKey)[] kGates = new[]
    {
        ("ads_reward_x", "ads_reward_on"),
        ("item_double_x", "item_double_on"),
        ("exp_multi", "exp_multi_on"),
    };

    static int PatchMultiplierGates(ModuleDefinition mod)
    {
        int total = 0;
        foreach (var g in kGates)
        {
            if (ModuleHasLdstr(mod, g.onKey)) { Console.WriteLine($"[skip] {g.onKey} 已是新形态"); continue; }
            int n = 0;
            foreach (var ty in mod.Types.ToList())
            {
                if (!ty.HasMethods) continue;
                foreach (var m in ty.Methods.ToList())
                {
                    if (!m.HasBody) continue;
                    var il = m.Body.GetILProcessor();
                    var snapshot = m.Body.Instructions.ToList();
                    for (int i = 0; i < snapshot.Count; i++)
                    {
                        if (snapshot[i].OpCode != OpCodes.Ldstr || (string)snapshot[i].Operand != g.valKey) continue;
                        int j = i + 1;
                        while (j < snapshot.Count && !(snapshot[j].Operand is MethodReference mr0 && mr0.Name == "GetInt")) j++;
                        if (j >= snapshot.Count) continue;
                        var call = snapshot[j];
                        var vRes = new VariableDefinition(mod.TypeSystem.Int32);
                        m.Body.Variables.Add(vRes);
                        m.Body.InitLocals = true;
                        var keep = Instruction.Create(OpCodes.Ldloc, vRes);
                        var seq = new List<Instruction>
                        {
                            Instruction.Create(OpCodes.Stloc, vRes),
                            Instruction.Create(OpCodes.Ldstr, g.onKey),
                            Instruction.Create(OpCodes.Call, mGetBool),
                            Instruction.Create(OpCodes.Brtrue, keep),
                            Instruction.Create(OpCodes.Ldc_I4_1),
                            Instruction.Create(OpCodes.Stloc, vRes),
                            keep,
                            Instruction.Create(OpCodes.Ldloc, vRes),
                        };
                        Instruction anchorIns = call;
                        foreach (var ins in seq) { il.InsertAfter(anchorIns, ins); anchorIns = ins; }
                        n++;
                        Console.WriteLine($"[patch] {ty.Name}::{m.Name} 处 {g.valKey} 加 {g.onKey} 门");
                        if (m.Body.MaxStackSize < 3) m.Body.MaxStackSize = 3;
                    }
                }
            }
            if (n == 0) Console.WriteLine($"[ERR] {g.valKey} 没有命中任何 GetInt 读取点");
            total += n;
        }
        return total;
    }

    static bool ModuleHasLdstr(ModuleDefinition mod, string s)
    {
        foreach (var ty in mod.Types)
            if (ty.HasMethods)
                foreach (var m in ty.Methods)
                    if (m.HasBody && HasLdstr(m, s)) return true;
        return false;
    }

    static bool Verify(ModuleDefinition mod, bool quiet)
    {
        bool ok = true;
        void Chk(bool cond, string msg)
        {
            if (!cond) { ok = false; if (!quiet) Console.WriteLine($"[verify][FAIL] {msg}"); }
            else if (!quiet) Console.WriteLine($"[verify][ok] {msg}");
        }

        var sa = mod.GetType("SkillAssets");
        Chk(sa != null, "SkillAssets 存在");
        var gba = sa?.Methods.FirstOrDefault(m => m.Name == "GetBulletAmount");
        Chk(gba != null && HasLdstr(gba, "skill_projectile_x"), "GetBulletAmount 含 skill_projectile_x");
        var gd = sa?.Methods.FirstOrDefault(m => m.Name == "get_damage");
        Chk(gd != null && CountLdstr(gd, "skill_damage_x") == 2, "get_damage 两处 ret 均含 skill_damage_x");
        var gsd = sa?.Methods.FirstOrDefault(m => m.Name == "GetSafeDamage");
        Chk(gsd != null && CountLdstr(gsd, "skill_damage_x") == 2, "GetSafeDamage 两处 ret 均含 skill_damage_x");

        var cell = mod.GetType("LegacySystem.LegacyCell");
        Chk(cell != null, "LegacyCell 存在");
        int sites = cell == null ? 0 : CountCostZeroSites(cell);
        Chk(sites > 0, $"LegacyCell 成本点 {sites} 处已被 StarryCostZero 包裹");
        Chk(mod.GetType("Starry.StarryLegacy") != null, "Starry.StarryLegacy 已注入");

        foreach (var g in kGates)
        {
            int n = 0;
            foreach (var ty in mod.Types)
                if (ty.HasMethods)
                    foreach (var m in ty.Methods)
                        if (m.HasBody) n += m.Body.Instructions.Count(i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == g.onKey);
            Chk(n >= 1, $"{g.valKey} 生效开关门 {g.onKey} 已插入 {n} 处");
        }

        var alc = mod.GetType("Calculater")?.Methods.FirstOrDefault(m => m.Name == "CalculateAllLegacyCost");
        Chk(alc != null && !alc.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"),
            "CalculateAllLegacyCost 未被包裹（重置神力退还口径不动）");
        var zs = mod.GetType("Calculater")?.Methods.FirstOrDefault(m => m.Name == "CalculateZhuanSheng");
        Chk(zs != null && !zs.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"),
            "CalculateZhuanSheng 未被包裹（转生门槛不动）");
        return ok;
    }

    static bool HasLdstr(MethodDefinition m, string s) => CountLdstr(m, s) > 0;
    static int CountLdstr(MethodDefinition m, string s)
        => m.HasBody ? m.Body.Instructions.Count(i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == s) : 0;
}
