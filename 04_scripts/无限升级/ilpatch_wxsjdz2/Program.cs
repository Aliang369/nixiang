// ilpatch_wxsjdz2 — 无限升级 wxsjdz2 定制号专用增量补丁（2026-10-27）
//
// 输入 = 12 线 A 线交付口径的补丁 DLL（ilpatch v1(31 patches) + ilpatch_delta 的产物
//        patched_new.dll 2,022,912 B / fnv 已定案），输出 = 再加三个新功能的补丁 DLL。
// 目标官方数组长度 = 2,028,544 B（2026-10-26 现网，fnv 0x1bf6c08f1ba38809）。
//
// 三个新功能（用户 2026-10-27 点名，落点均按现网 DLL 实测 IL 固定，禁止改判）：
//   ① 技能投掷物倍数  skill_projectile_x (1..100, 默认 1)
//      SkillAssets::GetBulletAmount() 的最终数量 × N（进阶技能加成已算在内）
//   ② 技能伤害倍数    skill_damage_x (1..1000000, 默认 1)
//      SkillAssets::get_damage()（Physic/Magic 两个 ret）+ GetSafeDamage()（两个 ret，反作弊基准同步放大）
//   ③ 转生无条件升级  zhuan_sheng_free_point (bool, 默认关)
//      LegacySystem.LegacyCell 内 13 处 Calculater::GetLegacyCost/GetLegacyCostSpecial 调用结果归零
//      （只免「神力/轮回点」属性升级消耗；转生门槛 CalculateZhuanSheng、重置神力 CalculateAllLegacyCost 不动）
//
// 用法：
//   WXSJ_SRC=<patched_new.dll> WXSJ_DST=<out.dll> dotnet run --configuration Release
//   WXSJ_SRC=<任意 dll> dotnet run --configuration Release -- --verify      # 只校验不写
//
// 幂等：重复跑检测标记串，已是最新形态则原样复制（与 ilpatch_vc1637 口径一致）。
using Mono.Cecil;
using Mono.Cecil.Cil;

static class Wxsjdz2Patch
{
    // 期望命中数（实测基线，2026-10-27）：LegacyCell 内 13 处成本调用点
    //   PiLiangFuntion 2 / Show 4 / Upgrade 4 / Upgrade10 1 / Upgrade100 1 / ReturnOver5000 1
    // ReturnOver5000 = 「超 5000 级退回」的退还额，同样走 GetLegacyCost；包裹后口径自洽：
    //   功能关 → 返回真实成本（退还照旧）；功能开 → 成本 0 ⇒ 退还 0（本来也没扣过），不会凭空刷神力。
    const int kLegacyCostSites = 13;

    static MethodReference mGetBool, mGetInt, mOpMul;
    static TypeReference tLargeNumber;

    static int Main(string[] args)
    {
        var src = Environment.GetEnvironmentVariable("WXSJ_SRC")
                  ?? "/Users/aliang/Documents/nixiang/07_patched/无限升级/newdump_20261026/patched_new.dll";
        var dst = Environment.GetEnvironmentVariable("WXSJ_DST")
                  ?? "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_wxsjdz2_patched.dll";
        bool verifyOnly = args.Length > 0 && args[0] == "--verify";

        if (!File.Exists(src)) { Console.WriteLine($"[ERR] 输入不存在: {src}"); return 2; }
        var asm = AssemblyDefinition.ReadAssembly(src);
        var mod = asm.MainModule;

        if (!ResolveBase(mod)) return 3;

        if (verifyOnly)
        {
            bool ok = Verify(mod, quiet: false);
            Console.WriteLine(ok ? "[verify] PASS" : "[verify] FAIL");
            return ok ? 0 : 1;
        }

        Console.WriteLine($"[src] {src}  ({new FileInfo(src).Length} B)");
        int n1 = PatchSkillProjectile(mod);
        int n2 = PatchSkillDamage(mod);
        int n3 = PatchLegacyPointFree(mod);
        Console.WriteLine($"[patch] projectile rets={n1} / damage rets={n2} / legacy cost sites={n3}");

        if (!Verify(mod, quiet: false)) { Console.WriteLine("[ERR] 自检失败，不写盘"); return 4; }

        asm.Write(dst);
        long len = new FileInfo(dst).Length;
        // 官方数组长度由调用方（wxsjdz2_so_only.sh）按变体校验；这里只给参考值
        long margin = 2028544L - len;
        Console.WriteLine($"[out] {dst}  {len} B  (相对现网 2028544 余量 {margin} B；实际变体长度由构建脚本按官方数组校验)");
        if (margin < 0) { Console.WriteLine("[ERR] 超过官方数组长度，so 侧 pLen<=len 不成立"); return 5; }

        // 输出后复读校验
        var re = AssemblyDefinition.ReadAssembly(dst);
        if (!Verify(re.MainModule, quiet: false)) { Console.WriteLine("[ERR] 落盘后复读校验失败"); return 6; }
        Console.WriteLine("[verify] PASS (落盘后复读)");
        return 0;
    }

    // ---------- 公共引用 ----------
    static bool ResolveBase(ModuleDefinition mod)
    {
        var cfg = mod.GetType("Starry.StarryConfig");
        if (cfg == null) { Console.WriteLine("[ERR] 缺 Starry.StarryConfig（输入必须是 v1+δ 产物 patched_new.dll）"); return false; }
        mGetBool = cfg.Methods.First(m => m.Name == "GetBool");
        mGetInt = cfg.Methods.First(m => m.Name == "GetInt");
        var sa = mod.GetType("SkillAssets");
        if (sa == null) { Console.WriteLine("[ERR] 缺 SkillAssets 类型"); return false; }
        var gd = sa.Methods.FirstOrDefault(m => m.Name == "get_damage");
        if (gd == null) { Console.WriteLine("[ERR] 缺 SkillAssets::get_damage"); return false; }
        tLargeNumber = gd.ReturnType;   // LargeNumbers.LargeNumber
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
        if (m == null || !m.HasBody) { Console.WriteLine("[warn] GetBulletAmount 未找到"); return 0; }
        if (HasLdstr(m, "skill_projectile_x")) { Console.WriteLine("[skip] 投掷物倍数已是新形态"); return CountMarker(m, "skill_projectile_x"); }

        var il = m.Body.GetILProcessor();
        var vRes = new VariableDefinition(mod.TypeSystem.Int32);
        var vX = new VariableDefinition(mod.TypeSystem.Int32);
        m.Body.Variables.Add(vRes);
        m.Body.Variables.Add(vX);
        m.Body.InitLocals = true;

        // 单 ret 结构：末尾 IL_xxxx: ldloc.0 / ret
        var ret = m.Body.Instructions.LastOrDefault(i => i.OpCode == OpCodes.Ret);
        if (ret == null) { Console.WriteLine("[warn] GetBulletAmount 无 ret"); return 0; }

        var skip = Instruction.Create(OpCodes.Ldloc, vRes);
        var seq = new List<Instruction>
        {
            Instruction.Create(OpCodes.Stloc, vRes),                 // 弹出最终数量
            Instruction.Create(OpCodes.Ldstr, "skill_projectile_x"),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Call, mGetInt),               // GetInt("skill_projectile_x", 1)
            Instruction.Create(OpCodes.Stloc, vX),
            Instruction.Create(OpCodes.Ldloc, vX),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble, skip),                   // N<=1 -> 原样
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
        Console.WriteLine($"[patch] GetBulletAmount ×skill_projectile_x @ {m.Body.Instructions.Count} ins");
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
            if (m == null || !m.HasBody) { Console.WriteLine($"[warn] SkillAssets::{name} 未找到"); continue; }
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
                    Instruction.Create(OpCodes.Stloc, vL),               // 弹出 LargeNumber
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
                    Instruction.Create(OpCodes.Call, mOpMul),            // ×(double)N
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

    // ---------- ③ 转生无条件升级（神力/轮回点属性升级消耗归零） ----------
    static int PatchLegacyPointFree(ModuleDefinition mod)
    {
        var cell = mod.GetType("LegacySystem.LegacyCell");
        if (cell == null) { Console.WriteLine("[warn] LegacySystem.LegacyCell 未找到"); return 0; }

        // 幂等
        foreach (var mm in cell.Methods)
            if (mm.HasBody && mm.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"))
            {
                Console.WriteLine("[skip] LegacyCell 已是新形态");
                return CountCostZeroSites(cell);
            }

        // 注入 Starry.StarryLegacy::StarryCostZero(LargeNumber) -> LargeNumber
        var helper = EnsureHelper(mod, out var costM);

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
        if (sites != kLegacyCostSites)
            Console.WriteLine($"[warn] 成本点数量 {sites} != 基线 {kLegacyCostSites}（服务端/官方又改 IL？需人工复核）");
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
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            tLargeNumber);
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

    // ---------- 校验 ----------
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
        Chk(sites == kLegacyCostSites, $"LegacyCell 成本点 {sites}/{kLegacyCostSites} 已被 StarryCostZero 包裹");
        Chk(mod.GetType("Starry.StarryLegacy") != null, "Starry.StarryLegacy 已注入");

        // 不许误伤：这两条必须保持原样（转生门槛 / 重置神力退还口径）
        var zhuan = mod.GetType("Calculater")?.Methods.FirstOrDefault(m => m.Name == "CalculateAllLegacyCost");
        Chk(zhuan != null && !zhuan.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"),
            "CalculateAllLegacyCost 未被包裹（重置神力退还口径不动）");
        var zs = mod.GetType("Calculater")?.Methods.FirstOrDefault(m => m.Name == "CalculateZhuanSheng");
        Chk(zs != null && !zs.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "StarryCostZero"),
            "CalculateZhuanSheng 未被包裹（转生门槛不动）");
        return ok;
    }

    static bool HasLdstr(MethodDefinition m, string s) => CountLdstr(m, s) > 0;
    static int CountLdstr(MethodDefinition m, string s)
        => m.HasBody ? m.Body.Instructions.Count(i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == s) : 0;
    static int CountMarker(MethodDefinition m, string s) => CountLdstr(m, s);
}
