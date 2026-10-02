using Mono.Cecil;
using Mono.Cecil.Cil;

// 境界死神激斗（com.cmge.ssjd.x7sy 1.38.2）HybridCLR 热更 DLL 补丁器
//
// 目标 DLL：Assembly-CSharp（运行时 dump，BleachBattle 命名空间全在 HybridCLR 解释执行区内）
// 注入一个 Starry.StarryConfig 类型（读文本配置），再对三功能目标方法做「入口门控注入」：
//   无敌     inv=1        ActorDataControl.ModifyHealth 负 delta 直接 return（血量不下降）
//   完全无敌 inv_full=1   ActorDataControl.get_IsInvincible → true（走 Hurt 入口 IsInvincible 早退，免疫硬直/不被索敌）
//   伤害倍率 dmg_mul=N    ActorDataControl._finalAttrImpl 入口 finalDamage *= N（仅当攻击者 Camp==Player）
//   无限技能 nocd=1       SkillInstance.get_iSkillState 抹掉 Cooling|NotEnoughMp|NotEnoughFury 位；
//                         SkillInstance.get_CDLeftTimeUs → 0（UI 不显示 CD）
//
// 铁律：全部经 StarryConfig 开关，配置缺失 = 原版行为（关 → 原 IL 原样执行）。
internal static class Patcher
{
    const string FILE_NAME = "starry_config.ini";
    // HybridCLR 解释器下 Application.persistentDataPath 返回空（无限升级 2026-09-05 实测），
    // ⇒ 直接硬编码包内绝对路径；native 侧写同一路径（JjssjdMod.cpp kCfgPath）
    const string CFG_PATH = "/data/user/0/com.cmge.ssjd.x7sy/files/" + FILE_NAME;

    static ModuleDefinition mod;
    static MethodReference mGetBool, mGetInt;
    static TypeReference tStr, tI32, tBool;
    static int nPatch;

    public static void Run(string srcPath, string dstPath)
    {
        // ★ 宽容解析链（TolerantRefs.cs）：缺引用程序集时不能整包写盘失败。
        //   Cecil 建 Param/Field 表会对**每个**成员走 GetConstantType → 强制 Resolve 其类型，
        //   任何一处解析不到就抛 AssemblyResolutionException 中止 Write（与本次补丁无关也会炸）。
        var resolver = new TolerantAssemblyResolver();
        foreach (var d in RefDirs(srcPath))
            if (Directory.Exists(d)) { resolver.AddSearchDirectory(d); Console.WriteLine($"[ref] {d}"); }
        var metaResolver = new TolerantMetadataResolver(resolver);
        var asm = AssemblyDefinition.ReadAssembly(srcPath,
            new ReaderParameters {
                AssemblyResolver = resolver,
                MetadataResolver = metaResolver,
                ReadSymbols = false,
            });
        mod = asm.MainModule;
        Console.WriteLine($"[in ] {srcPath}  ({new FileInfo(srcPath).Length} B)");
        Console.WriteLine($"[in ] assembly={asm.Name.Name}  types={mod.Types.Count}");

        // ★ 垫片：把「有真默认值、但类型解析不到」的枚举补成真 dll，保证常量不丢。
        //   必须在任何解析发生之前做完（Discover 用独立的探测解析器，不受本 resolver 影响）。
        var realDirs = RefDirs(srcPath).Where(Directory.Exists).ToList();
        var needs = ShimFactory.Discover(mod, realDirs);
        var shimDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcPath)), "refs_shim");
        var nShim = ShimFactory.Materialize(shimDir, needs);
        if (nShim > 0) { resolver.AddSearchDirectory(shimDir); Console.WriteLine($"[shim] {nShim} 个垫片程序集已就绪 → {shimDir}"); }

        BuildStarryConfig();

        var adc = FindType("BleachBattle", "ActorDataControl");
        var sk = FindType("BleachBattle", "SkillInstance");
        Console.WriteLine($"[map] {adc.FullName}");
        Console.WriteLine($"[map] {sk.FullName}");

        var mGetCamp = FindM(adc, "get_Camp", 0);
        var campEnum = mGetCamp.ReturnType.Resolve();
        int campPlayer = EnumValue(campEnum, "Player");
        Console.WriteLine($"[map] {campEnum.FullName}.Player = {campPlayer}");

        P(() => PatchInvincible(adc, mGetCamp, campPlayer), "ActorDataControl.get_IsInvincible  [inv_full]");
        P(() => PatchModifyHealth(adc, mGetCamp, campPlayer), "ActorDataControl.ModifyHealth      [inv]");
        P(() => PatchFinalAttr(adc, mGetCamp, campPlayer), "ActorDataControl._finalAttrImpl    [dmg_mul]");
        P(() => PatchSkillState(sk), "SkillInstance.get_iSkillState      [nocd]");
        P(() => PatchSkillCdLeft(sk), "SkillInstance.get_CDLeftTimeUs     [nocd]");

        // 写盘前后复核「真实常量」没有被宽容解析吞掉
        var constantsBefore = ConstantAudit.Snapshot(mod);
        var wp = new WriterParameters { WriteSymbols = false };
        asm.Write(dstPath, wp);
        asm.Dispose();
        Console.WriteLine($"[out] {dstPath}  ({new FileInfo(dstPath).Length} B)");
        Console.WriteLine($"[ok ] patches={nPatch}");

        using (var back = AssemblyDefinition.ReadAssembly(dstPath,
                   new ReaderParameters { AssemblyResolver = resolver, MetadataResolver = metaResolver, ReadSymbols = false }))
        {
            var lost = ConstantAudit.Diff(constantsBefore, back.MainModule);
            Console.WriteLine(lost.Count == 0
                ? $"[gate] 常量台账 PASS（{constantsBefore.Count} 条真实默认值全部保留）"
                : $"[gate] 常量台账 FAIL（丢失 {lost.Count} 条）：\n  " + string.Join("\n  ", lost));
            if (lost.Count != 0) Environment.Exit(1);
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    // 出包门禁
    // ───────────────────────────────────────────────────────────────────────
    // 被补丁的方法清单（Program.cs 据此打印完整 IL）。
    public static readonly (string Type, string Method)[] Targets =
    {
        ("BleachBattle.ActorDataControl", "get_IsInvincible"),
        ("BleachBattle.ActorDataControl", "ModifyHealth"),
        ("BleachBattle.ActorDataControl", "_finalAttrImpl"),
        ("BleachBattle.SkillInstance",    "get_iSkillState"),
        ("BleachBattle.SkillInstance",    "get_CDLeftTimeUs"),
    };

    // 逐补丁**意图断言**（不是「跑通没报错」的通过，而是「注入的语义对不对」的通过）。
    // 任一 FAIL → 调用方 Environment.Exit(1) 阻断出包。
    // 2026-10-02 事故（见 skill/re-implement/hotfix-dll-replace-pattern.md 第 5 项）的回归测试在这里。
    public static void Gate(ModuleDefinition mm, List<string> fails)
    {
        void A(string name, bool ok, string detail)
        {
            Console.WriteLine($"[assert] {(ok ? "OK  " : "FAIL")} {name}: {detail}");
            if (!ok) fails.Add($"{name}: {detail}");
        }
        MethodDefinition Need(string tn, string mn)
        {
            var t = mm.GetType(tn);
            var m = t?.Methods.FirstOrDefault(x => x.Name == mn);
            if (m == null) fails.Add($"{tn}::{mn} NOT FOUND（补丁未落到该方法）");
            return m;
        }
        // Cecil 里 `ldarg.N` 的 N 是 arg slot（含 this），Parameters[i].Index **不含** this。
        static int SlotToParamIndex(MethodDefinition x, int slot) => x.HasThis ? slot - 1 : slot;
        static bool HeadIsLdstr(MethodDefinition m, string s) =>
            m.Body.Instructions.Count > 0 && m.Body.Instructions[0].OpCode == OpCodes.Ldstr &&
            (string?)m.Body.Instructions[0].Operand == s;
        static bool Calls(MethodDefinition m, string owner, string name) =>
            m.Body.Instructions.Any(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference r &&
                                         r.Name == name && r.DeclaringType.FullName == owner);

        const string tAdc = "BleachBattle.ActorDataControl";
        const string tSk = "BleachBattle.SkillInstance";
        const string tCfg = "Starry.StarryConfig";

        // ── 1. get_IsInvincible [inv_full] ─────────────────────────────────────
        var mInv = Need(tAdc, "get_IsInvincible");
        if (mInv != null)
            A("get_IsInvincible", HeadIsLdstr(mInv, "inv_full") && Calls(mInv, tCfg, "GetBool") &&
                mInv.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4_1 && i.Next?.OpCode == OpCodes.Ret),
              "门控 ldstr \"inv_full\" + call GetBool + 命中即 `ldc.i4.1; ret`");

        // ── 2. ModifyHealth [inv] ──────────────────────────────────────────────
        var mHp = Need(tAdc, "ModifyHealth");
        if (mHp != null)
        {
            // delta<0 才拦 ⇒ 注入块里必须出现 conv.i8 / bge（正 delta=治疗 走原版）
            bool shape = mHp.Body.Instructions.Take(60).Any(i => i.OpCode == OpCodes.Conv_I8)
                         && mHp.Body.Instructions.Take(60).Any(i => i.OpCode == OpCodes.Bge || i.OpCode == OpCodes.Bge_S)
                         && mHp.Body.Instructions.Take(60).Any(i => i.OpCode == OpCodes.Ret);
            A("ModifyHealth", HeadIsLdstr(mHp, "inv") && Calls(mHp, tCfg, "GetBool") && shape,
              "门控 ldstr \"inv\" + call GetBool + delta<0 判定(conv.i8/bge) + 注入块内 ret（正 delta=治疗走原版）");
        }

        // ── 3. _finalAttrImpl [dmg_mul] —— 本断言即 2026-10-02 事故的回归测试 ──
        var mFa = Need(tAdc, "_finalAttrImpl");
        if (mFa != null)
        {
            A("_finalAttrImpl", Calls(mFa, tCfg, "GetInt"), "含 call StarryConfig.GetInt(\"dmg_mul\")");
            // 读侧 slot 3（finalDamage），写侧必须落到**同一个** ParameterDefinition
            var ld = mFa.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Ldarg_3);
            var st = mFa.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Starg || i.OpCode == OpCodes.Starg_S);
            if (ld == null || st == null)
                A("_finalAttrImpl.参数对齐", false, $"找不到注入块的 Ldarg_3(st={st != null})/Starg(ld={ld != null})");
            else
            {
                const int readSlot = 3;
                int expectIdx = SlotToParamIndex(mFa, readSlot);
                var wp = st.Operand as ParameterDefinition;
                var rp = mFa.Parameters[expectIdx];
                A("_finalAttrImpl.参数对齐", wp != null && wp.Index == expectIdx && rp.ParameterType.FullName == "System.Int64",
                  $"读 Ldarg_{readSlot} → Parameters[{expectIdx}]={rp.Name}({rp.ParameterType.Name})；" +
                  $"注入 Starg 落点={(wp == null ? "非参数" : $"Parameters[{wp.Index}]={wp.Name}({wp.ParameterType.Name})")}" +
                  (wp != null && wp.Index == expectIdx ? " ✅ 一致" : " ❌ 不一致（写错参数！）"));
                A("_finalAttrImpl.参数名", rp.Name == "finalDamage", $"Parameters[{expectIdx}].Name = {rp.Name}（期望 finalDamage）");
            }
        }

        // ── 4. get_iSkillState [nocd] ─────────────────────────────────────────
        var mSs = Need(tSk, "get_iSkillState");
        if (mSs != null)
            A("get_iSkillState", HeadIsLdstr(mSs, "nocd") && Calls(mSs, tCfg, "GetBool") &&
                mSs.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4 && (int)i.Operand == -12) &&
                mSs.Body.Instructions.Any(i => i.OpCode == OpCodes.And),
              "门控 ldstr \"nocd\" + call GetBool + 掩码 ldc.i4 -12 + and（清 Cooling|NotEnoughMp|NotEnoughFury）");

        // ── 5. get_CDLeftTimeUs [nocd] ────────────────────────────────────────
        var mCd = Need(tSk, "get_CDLeftTimeUs");
        if (mCd != null)
            A("get_CDLeftTimeUs", HeadIsLdstr(mCd, "nocd") && Calls(mCd, tCfg, "GetBool") &&
                mCd.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4_0 && i.Next?.OpCode == OpCodes.Conv_I8),
              "门控 ldstr \"nocd\" + call GetBool + ldc.i4.0; conv.i8（返回 0L）");

        // ── 6. Starry.StarryConfig 本体 ────────────────────────────────────────
        var cfg = mm.GetType(tCfg);
        if (cfg == null) A("StarryConfig", false, "注入类型缺失");
        else
        {
            var names = cfg.Methods.Select(x => x.Name).ToList();
            bool ok = names.Contains("Get") && names.Contains("GetBool") && names.Contains("GetInt");
            bool path = cfg.Methods.Where(x => x.HasBody)
                            .SelectMany(x => x.Body.Instructions.AsEnumerable())
                            .Any(i => i.OpCode == OpCodes.Ldstr && ((string?)i.Operand ?? "").Contains("/files/"));
            A("StarryConfig", ok && path, $"方法={string.Join(",", names)}；硬编码配置路径已内联={path}");
        }
    }

    // 引用程序集搜索目录。Cecil 写盘期需要解析成员类型（见 TolerantRefs.cs 的说明），
    // 有真实 DummyDll 时保真度最高；没有则由 TolerantAssemblyResolver 合成空壳兜底。
    static IEnumerable<string> RefDirs(string srcPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(srcPath));
        yield return Path.Combine(dir, "refs");
        yield return "/tmp/ssjd2/refs";
        yield return "/tmp/ssjd2/dump/DummyDll";
        var env = Environment.GetEnvironmentVariable("JJSSJD_REF_DIR");
        if (!string.IsNullOrEmpty(env)) yield return env;
    }

    static void P(Action f, string name)
    {
        f();
        nPatch++;
        Console.WriteLine($"[patch] {name}");
    }

    static MethodDefinition FindTypeM(TypeDefinition t, string name, int? paramCount = null)
        => FindM(t, name, paramCount);

    static TypeDefinition FindType(string ns, string name)
    {
        var t = mod.Types.FirstOrDefault(x => x.Namespace == ns && x.Name == name);
        if (t == null) throw new Exception($"类型未找到: {ns}.{name}");
        return t;
    }

    static MethodDefinition FindM(TypeDefinition t, string name, int? paramCount = null)
    {
        var list = t.Methods.Where(m => m.Name == name &&
            (paramCount == null || m.Parameters.Count == paramCount)).ToList();
        if (list.Count == 0) throw new Exception($"{t.FullName}::{name}({paramCount}) not found");
        if (list.Count > 1) throw new Exception($"{t.FullName}::{name} has {list.Count} overloads");
        if (!list[0].HasBody) throw new Exception($"{t.FullName}::{name} no body");
        return list[0];
    }

    static int EnumValue(TypeDefinition e, string field)
    {
        var f = e.Fields.FirstOrDefault(x => x.Name == field);
        if (f == null) throw new Exception($"{e.FullName}.{field} not found");
        return Convert.ToInt32(f.Constant);
    }

    static AssemblyNameReference Corlib()
    {
        foreach (var want in new[] { "mscorlib", "System.Runtime", "netstandard", "System.Private.CoreLib" })
        {
            var r = mod.AssemblyReferences.FirstOrDefault(x => x.Name == want);
            if (r != null) return r;
        }
        throw new Exception("找不到 corlib 引用: " + string.Join(",", mod.AssemblyReferences.Select(x => x.Name)));
    }

    static AssemblyNameReference AsmRef(string name)
    {
        var r = mod.AssemblyReferences.FirstOrDefault(x => x.Name == name);
        if (r != null) return r;
        r = new AssemblyNameReference(name, new Version(0, 0, 0, 0));
        mod.AssemblyReferences.Add(r);
        return r;
    }

    static void InsertAt(MethodDefinition m, int index, IEnumerable<Instruction> seq)
    {
        var ins = m.Body.Instructions;
        int i = index;
        foreach (var x in seq) ins.Insert(i++, x);
    }

    // ---------------- 三个功能补丁 ----------------

    // get_IsInvincible():  if (StarryConfig.GetBool("inv_full") && this.Camp == Camp.Player) return true;
    static void PatchInvincible(TypeDefinition adc, MethodReference mGetCamp, int campPlayer)
    {
        var m = FindM(adc, "get_IsInvincible", 0);
        var orig = m.Body.Instructions[0];
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "inv_full"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, orig),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Callvirt, mGetCamp),
            Instruction.Create(OpCodes.Ldc_I4, campPlayer),
            Instruction.Create(OpCodes.Bne_Un, orig),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ret),
        });
    }

    // ModifyHealth():  if (StarryConfig.GetBool("inv") && delta < 0 && this.Camp == Camp.Player) return;
    static void PatchModifyHealth(TypeDefinition adc, MethodReference mGetCamp, int campPlayer)
    {
        var m = FindM(adc, "ModifyHealth", 6);
        var orig = m.Body.Instructions[0];
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "inv"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, orig),
            Instruction.Create(OpCodes.Ldarg_1),          // long delta
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Conv_I8),
            Instruction.Create(OpCodes.Bge, orig),         // delta >= 0 → 治疗，走原逻辑
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Callvirt, mGetCamp),
            Instruction.Create(OpCodes.Ldc_I4, campPlayer),
            Instruction.Create(OpCodes.Bne_Un, orig),
            Instruction.Create(OpCodes.Ret),
        });
    }

    // _finalAttrImpl(): if (attacker != null && attacker.Camp == Camp.Player && finalDamage > 0) { int v = GetInt("dmg_mul",1);
    //                    if (v > 1 && v <= 1000) finalDamage *= v; }
    static void PatchFinalAttr(TypeDefinition adc, MethodReference mGetCamp, int campPlayer)
    {
        var m = FindM(adc, "_finalAttrImpl", 10);
        var vMul = new VariableDefinition(tI32);
        m.Body.Variables.Add(vMul);
        m.Body.InitLocals = true;
        var orig = m.Body.Instructions[0];
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldarg_1),           // mainAttackerDataCtrl
            Instruction.Create(OpCodes.Brfalse, orig),
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Callvirt, mGetCamp),
            Instruction.Create(OpCodes.Ldc_I4, campPlayer),
            Instruction.Create(OpCodes.Bne_Un, orig),
            Instruction.Create(OpCodes.Ldarg_3),           // long finalDamage
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Conv_I8),
            Instruction.Create(OpCodes.Ble, orig),         // <= 0 → 治疗/反伤，不放大
            Instruction.Create(OpCodes.Ldstr, "dmg_mul"),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Call, mGetInt),
            Instruction.Create(OpCodes.Stloc, vMul),
            Instruction.Create(OpCodes.Ldloc, vMul),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble, orig),         // mul <= 1 → 原版
            Instruction.Create(OpCodes.Ldloc, vMul),
            Instruction.Create(OpCodes.Ldc_I4, 1000),
            Instruction.Create(OpCodes.Bgt, orig),         // 上限保护
            Instruction.Create(OpCodes.Ldarg_3),
            Instruction.Create(OpCodes.Ldloc, vMul),
            Instruction.Create(OpCodes.Conv_I8),
            Instruction.Create(OpCodes.Mul),
            // ⚠️ 必须 Parameters[2]（= long finalDamage）。参数表：[0]mainAttackerDataCtrl [1]sufferDataCtrl
            //    [2]finalDamage [3]attackerSkillId…；Cecil 的 Starg 按 param.Index + (HasThis?1:0)
            //    映射到 arg slot 3，与上面的 Ldarg_3 对齐。写成 Parameters[3] 会写坏 attackerSkillId
            //    （紧随其后就是 `if (attackerSkillId > 0) mainAttackerDataCtrl.GetSkill(attackerSkillId)`）
            Instruction.Create(OpCodes.Starg, m.Parameters[2]),
        });
    }

    // get_iSkillState(): if (StarryConfig.GetBool("nocd")) return _skillState & ~(Cooling|NotEnoughMp|NotEnoughFury);
    static void PatchSkillState(TypeDefinition sk)
    {
        var m = FindM(sk, "get_iSkillState", 0);
        var fld = m.Body.Instructions.Select(x => x.Operand).OfType<FieldReference>().FirstOrDefault();
        if (fld == null) throw new Exception("get_iSkillState: 找不到 _skillState 字段");
        Console.WriteLine($"[map] _skillState → {fld.FullName}");
        // Ready=0 Cooling=1 NotEnoughMp=2 NotEnoughFury=8 ⇒ 掩码 ~0xB = -12
        const int mask = ~(1 | 2 | 8);
        var orig = m.Body.Instructions[0];
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "nocd"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, orig),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, fld),
            Instruction.Create(OpCodes.Ldc_I4, mask),
            Instruction.Create(OpCodes.And),
            Instruction.Create(OpCodes.Ret),
        });
    }

    // get_CDLeftTimeUs(): if (StarryConfig.GetBool("nocd")) return 0L;
    static void PatchSkillCdLeft(TypeDefinition sk)
    {
        var m = FindM(sk, "get_CDLeftTimeUs", 0);
        var orig = m.Body.Instructions[0];
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, "nocd"),
            Instruction.Create(OpCodes.Call, mGetBool),
            Instruction.Create(OpCodes.Brfalse, orig),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Conv_I8),
            Instruction.Create(OpCodes.Ret),
        });
    }

    // ---------------- StarryConfig 注入 ----------------
    // 文件：/data/user/0/com.cmge.ssjd.x7sy/files/starry_config.ini，每行 key=value，2 秒缓存
    // 文件不存在/解析异常 → 全默认（关）→ 与原版行为一致
    static void BuildStarryConfig()
    {
        tStr = mod.TypeSystem.String;
        tI32 = mod.TypeSystem.Int32;
        tBool = mod.TypeSystem.Boolean;
        var tObj = mod.TypeSystem.Object;
        var tStrArr = new ArrayType(tStr);
        var tExc = new TypeReference("System", "Exception", mod, Corlib());

        var cfgType = new TypeDefinition("Starry", "StarryConfig",
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit, tObj);
        mod.Types.Add(cfgType);
        var fLines = new FieldDefinition("lines", FieldAttributes.Static | FieldAttributes.Private, tStrArr);
        var fNext = new FieldDefinition("nextCheck", FieldAttributes.Static | FieldAttributes.Private, tI32);
        cfgType.Fields.Add(fLines);
        cfgType.Fields.Add(fNext);

        var env = new TypeReference("System", "Environment", mod, Corlib());
        var mTick = new MethodReference("get_TickCount", tI32, env);
        var file = new TypeReference("System.IO", "File", mod, Corlib());
        var mReadAll = new MethodReference("ReadAllLines", tStrArr, file);
        mReadAll.Parameters.Add(new ParameterDefinition(tStr));
        var mConcat = new MethodReference("Concat", tStr, tStr) { HasThis = false };
        mConcat.Parameters.Add(new ParameterDefinition(tStr));
        mConcat.Parameters.Add(new ParameterDefinition(tStr));
        var mStartsWith = new MethodReference("StartsWith", tBool, tStr) { HasThis = true };
        mStartsWith.Parameters.Add(new ParameterDefinition(tStr));
        var mSubstr = new MethodReference("Substring", tStr, tStr) { HasThis = true };
        mSubstr.Parameters.Add(new ParameterDefinition(tI32));
        var mLen = new MethodReference("get_Length", tI32, tStr) { HasThis = true };
        var mEq = new MethodReference("op_Equality", tBool, tStr) { HasThis = false };
        mEq.Parameters.Add(new ParameterDefinition(tStr));
        mEq.Parameters.Add(new ParameterDefinition(tStr));
        var mParse = new MethodReference("Parse", tI32, mod.TypeSystem.Int32);
        mParse.Parameters.Add(new ParameterDefinition(tStr));

        var cctor = new MethodDefinition(".cctor",
            MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            mod.TypeSystem.Void);
        cfgType.Methods.Add(cctor);
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fLines));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        // static string Get(string key)
        var mGet = new MethodDefinition("Get",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tStr);
        mGet.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        cfgType.Methods.Add(mGet);
        var vLine = new VariableDefinition(tStr);
        var vI = new VariableDefinition(tI32);
        mGet.Body.Variables.Add(vLine);
        mGet.Body.Variables.Add(vI);
        mGet.Body.InitLocals = true;
        var g = mGet.Body.Instructions;
        var loopExit = Instruction.Create(OpCodes.Ldnull);
        var skipReload = Instruction.Create(OpCodes.Nop);
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fNext));
        g.Add(Instruction.Create(OpCodes.Ble, skipReload));
        g.Add(Instruction.Create(OpCodes.Ldc_I4, 2000));
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        var t0 = Instruction.Create(OpCodes.Ldstr, CFG_PATH);
        g.Add(t0);
        g.Add(Instruction.Create(OpCodes.Call, mReadAll));
        g.Add(Instruction.Create(OpCodes.Stsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Leave, skipReload));
        var h0 = Instruction.Create(OpCodes.Pop);
        g.Add(h0);
        g.Add(Instruction.Create(OpCodes.Ldnull));
        g.Add(Instruction.Create(OpCodes.Stsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Leave, skipReload));
        mGet.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = t0, TryEnd = h0, HandlerStart = h0, HandlerEnd = skipReload, CatchType = tExc,
        });
        g.Add(skipReload);
        g.Add(Instruction.Create(OpCodes.Ldsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Brfalse, loopExit));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        g.Add(Instruction.Create(OpCodes.Stloc, vI));
        var loopTop = Instruction.Create(OpCodes.Nop);
        g.Add(loopTop);
        g.Add(Instruction.Create(OpCodes.Ldloc, vI));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Ldlen));
        g.Add(Instruction.Create(OpCodes.Conv_I4));
        g.Add(Instruction.Create(OpCodes.Bge, loopExit));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fLines));
        g.Add(Instruction.Create(OpCodes.Ldloc, vI));
        g.Add(Instruction.Create(OpCodes.Ldelem_Ref));
        g.Add(Instruction.Create(OpCodes.Stloc, vLine));
        g.Add(Instruction.Create(OpCodes.Ldloc, vLine));
        g.Add(Instruction.Create(OpCodes.Ldarg_0));
        g.Add(Instruction.Create(OpCodes.Ldstr, "="));
        g.Add(Instruction.Create(OpCodes.Call, mConcat));
        g.Add(Instruction.Create(OpCodes.Callvirt, mStartsWith));
        var nextIter = Instruction.Create(OpCodes.Nop);
        g.Add(Instruction.Create(OpCodes.Brfalse, nextIter));
        g.Add(Instruction.Create(OpCodes.Ldloc, vLine));
        g.Add(Instruction.Create(OpCodes.Ldarg_0));
        g.Add(Instruction.Create(OpCodes.Callvirt, mLen));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Callvirt, mSubstr));
        g.Add(Instruction.Create(OpCodes.Ret));
        g.Add(nextIter);
        g.Add(Instruction.Create(OpCodes.Ldloc, vI));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Stloc, vI));
        g.Add(Instruction.Create(OpCodes.Br, loopTop));
        g.Add(loopExit);
        g.Add(Instruction.Create(OpCodes.Ret));

        // static bool GetBool(string key)
        var mGetBoolDef = new MethodDefinition("GetBool",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tBool);
        mGetBoolDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        cfgType.Methods.Add(mGetBoolDef);
        var vB = new VariableDefinition(tStr);
        mGetBoolDef.Body.Variables.Add(vB);
        mGetBoolDef.Body.InitLocals = true;
        var b = mGetBoolDef.Body.Instructions;
        var falsePath = Instruction.Create(OpCodes.Ldc_I4_0);
        b.Add(Instruction.Create(OpCodes.Ldarg_0));
        b.Add(Instruction.Create(OpCodes.Call, mGet));
        b.Add(Instruction.Create(OpCodes.Stloc, vB));
        b.Add(Instruction.Create(OpCodes.Ldloc, vB));
        b.Add(Instruction.Create(OpCodes.Brfalse, falsePath));
        b.Add(Instruction.Create(OpCodes.Ldloc, vB));
        b.Add(Instruction.Create(OpCodes.Ldstr, "1"));
        b.Add(Instruction.Create(OpCodes.Call, mEq));
        var notOne = Instruction.Create(OpCodes.Nop);
        b.Add(Instruction.Create(OpCodes.Brfalse, notOne));
        b.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        b.Add(Instruction.Create(OpCodes.Ret));
        b.Add(notOne);
        b.Add(Instruction.Create(OpCodes.Ldloc, vB));
        b.Add(Instruction.Create(OpCodes.Ldstr, "true"));
        b.Add(Instruction.Create(OpCodes.Call, mEq));
        b.Add(Instruction.Create(OpCodes.Ret));
        b.Add(falsePath);
        b.Add(Instruction.Create(OpCodes.Ret));
        mGetBool = mGetBoolDef;

        // static int GetInt(string key, int def)
        var mGetDef = new MethodDefinition("GetInt",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tI32);
        mGetDef.Parameters.Add(new ParameterDefinition("key", ParameterAttributes.None, tStr));
        mGetDef.Parameters.Add(new ParameterDefinition("def", ParameterAttributes.None, tI32));
        cfgType.Methods.Add(mGetDef);
        var vV = new VariableDefinition(tStr);
        var vR = new VariableDefinition(tI32);
        mGetDef.Body.Variables.Add(vV);
        mGetDef.Body.Variables.Add(vR);
        mGetDef.Body.InitLocals = true;
        var mi = mGetDef.Body.Instructions;
        var defPath = Instruction.Create(OpCodes.Ldarg_1);
        mi.Add(Instruction.Create(OpCodes.Ldarg_0));
        mi.Add(Instruction.Create(OpCodes.Call, mGet));
        mi.Add(Instruction.Create(OpCodes.Stloc, vV));
        mi.Add(Instruction.Create(OpCodes.Ldloc, vV));
        mi.Add(Instruction.Create(OpCodes.Brfalse, defPath));
        var tP = Instruction.Create(OpCodes.Ldloc, vV);
        mi.Add(tP);
        mi.Add(Instruction.Create(OpCodes.Call, mParse));
        mi.Add(Instruction.Create(OpCodes.Stloc, vR));
        var afterTry = Instruction.Create(OpCodes.Ldloc, vR);
        mi.Add(Instruction.Create(OpCodes.Leave, afterTry));
        var hP = Instruction.Create(OpCodes.Pop);
        mi.Add(hP);
        mi.Add(Instruction.Create(OpCodes.Leave, defPath));
        mi.Add(afterTry);
        mi.Add(Instruction.Create(OpCodes.Ret));
        mi.Add(defPath);
        mi.Add(Instruction.Create(OpCodes.Ret));
        mGetDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = tP, TryEnd = hP, HandlerStart = hP, HandlerEnd = defPath, CatchType = tExc,
        });
        mGetInt = mGetDef;

        Console.WriteLine($"[cfg] Starry.StarryConfig 已注入（2s 缓存, {CFG_PATH}）");
    }
}
