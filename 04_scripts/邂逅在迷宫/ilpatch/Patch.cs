using Mono.Cecil;
using Mono.Cecil.Cil;

// 【邂逅在迷宫 / cn.alltune.endless 2.6.53】HybridCLR 热更程序集 IL 补丁器
//
// 目标程序集：EndlessStrike.dll（24,288,768 B，从真机 files/Bundles/endlessstrikedll_<hash>.bundle
//   解出的 TextAsset；bundle 前缀 41 字节为 xasset 头，UnityFS 之后才是 Unity 数据）。
//
// 模式（与宝石战争/境界死神激斗同构）：注入 Starry.StarryCheat（读状态镜像文本），再对目标方法做
//   「入口前置门控」——只 InsertBefore 插入，不删改任何原指令；配置缺失/开关关闭 ⇒ 原逻辑不变。
//
// 三个功能锚点（全部来自 Person.cs / SkillCD.cs 反编译）：
//   无敌     : Person.TakeDamage / TakeDamageDistant / Hurt  →  this.IsPlayer && god ⇒ return 0f
//   伤害倍率 : Person.Hurt                                    →  owner.IsPlayer ⇒ damage *= N
//   无限技能 : SkillCD.isTimeOver / CanUse                    →  nocd ⇒ return true
//
// 铁律（详见 04_scripts/_templates/ilpatch/README.md）：
//   1. Parameters 不含 this ⇒ IL 槽 0 = this；Hurt 的 damage 落在槽 1
//   2. Cecil 写盘期会强制 Resolve 带常量成员的类型 ⇒ 靠 TolerantRefs.cs 兜底
//   3. 产物比原文件大时必须在加载边界按新长度重新分配容器（bundle 侧由 UnityPy 处理）
internal static class Patcher
{
    // ── 状态镜像通道 ────────────────────────────────────────────────────────
    // starry 菜单的 StateMirror.java 把 featureList 各值写成单行、空格分隔的 8 个 int：
    //   {filesDir}/starry_state.txt
    // featureList 下标 == featNum（Category_ 也占下标），本作约定：
    //   0 Category_战斗 / 1 SeekBar_伤害倍率_1_100_1 / 2 Toggle_无敌 / 3 Toggle_无限技能
    // 主路径 = 应用 **Android/media** 目录。Android 11+ 的 FUSE 只对 Android/data 与
    //   Android/obb 做跨包隔离；Android/media/<pkg> 是应用自己的外部目录，宿主 App 天然可读，
    //   而星光助手凭 MANAGE_EXTERNAL_STORAGE 也能跨包写入（实测 Android/data 跨包写被 EACCES 拒绝）。
    // 次路径 = 应用外部私有目录 Android/data（旧版 adb push 通道，保留兼容）。
    // 兜底路径 = 内部私有目录（root 场景 / 旧行为）。
    const string STATE_PATH = "/storage/emulated/0/Android/media/cn.alltune.endless/starry_state.txt";
    const string STATE_PATH_ALT = "/storage/emulated/0/Android/data/cn.alltune.endless/files/starry_state.txt";
    const string STATE_PATH_FALLBACK = "/data/user/0/cn.alltune.endless/files/starry_state.txt";
    const int FEAT_DMG = 1;
    const int FEAT_GOD = 2;
    const int FEAT_NOCD = 3;
    const int FEAT_NOAD = 4;   // Toggle_免广告：跳过视频播放，直接走本类的广告完成回调（其余链路照旧）
    // 诊断探针前缀（走 UnityEngine.Debug.Log → logcat 里 grep "StarryAd| "）
    const string PROBE_PREFIX = "StarryAd| ";

    static ModuleDefinition mod;
    static int nPatch;
    static MethodReference mRaw, mGod, mMult, mNoCd, mNoAd, mProbe;

    public static void Run(string srcPath, string dstPath)
    {
        var resolver = new TolerantAssemblyResolver();
        foreach (var d in RefDirs(srcPath))
            if (Directory.Exists(d)) { resolver.AddSearchDirectory(d); Console.WriteLine($"[ref] {d}"); }
        var metaResolver = new TolerantMetadataResolver(resolver);
        var asm = AssemblyDefinition.ReadAssembly(srcPath,
            new ReaderParameters
            {
                AssemblyResolver = resolver,
                MetadataResolver = metaResolver,
                ReadSymbols = false,
            });
        mod = asm.MainModule;
        Console.WriteLine($"[in ] {srcPath}  ({new FileInfo(srcPath).Length} B)");
        Console.WriteLine($"[in ] assembly={asm.Name.Name}  types={mod.Types.Count}");

        var realDirs = RefDirs(srcPath).Where(Directory.Exists).ToList();
        var needs = ShimFactory.Discover(mod, realDirs);
        var shimDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcPath)), "refs_shim");
        var nShim = ShimFactory.Materialize(shimDir, needs);
        if (nShim > 0) { resolver.AddSearchDirectory(shimDir); Console.WriteLine($"[shim] {nShim} 个垫片程序集已就绪 → {shimDir}"); }

        var person = FindType("", "Person");
        var skillCd = FindType("", "SkillCD");
        var fIsPlayer = person.Fields.FirstOrDefault(f => f.Name == "IsPlayer");
        if (fIsPlayer == null) throw new Exception("Person.IsPlayer 字段未找到");
        Console.WriteLine($"[map] {person.FullName}  IsPlayer={fIsPlayer.FieldType.Name}");
        Console.WriteLine($"[map] {skillCd.FullName}");

        BuildStarryCheat(person, fIsPlayer);

        P(() => PatchZeroIfPlayerGod(person, "TakeDamage", 6), "Person.TakeDamage          [god]");
        P(() => PatchZeroIfPlayerGod(person, "TakeDamageDistant", 5), "Person.TakeDamageDistant   [god]");
        P(() => PatchHurt(person), "Person.Hurt                [god + dmg_mul]");
        P(() => PatchSkillCdBool(skillCd, "isTimeOver"), "SkillCD.isTimeOver         [nocd]");
        P(() => PatchSkillCdBool(skillCd, "CanUse"), "SkillCD.CanUse             [nocd]");

        // ── 免广告：各入口「看广告」按钮前置门控 → 直接调本类的广告完成回调 ──
        // 保留原链路（完成回调里照旧上报服务端 / 照旧发奖），只是不播视频。
        // 全作共 3 个真实播放入口；HomepageLeftButtonBar/ManorLeftButtonBar::OnAdsClick 只是打开本菜单，无需改动。
        P(() => PatchAdEntry("AdsRewardMenu", "GetAds", 0, "onRewardAdsCompleted", 0),
            "AdsRewardMenu.GetAds        [noad]");
        P(() => PatchAdEntry("SearchMapMenuLogic", "SearchAllByAds", 0, "onRewardAdsCompleted", 0),
            "SearchMapMenuLogic.SearchAllByAds [noad]");
        P(() => PatchAdEntry("BuildingUpgradeView", "OnBtnFinishUpgradeByAdsClicked", 0, "OnAdsFinished", 0),
            "BuildingUpgradeView.OnBtnFinishUpgradeByAdsClicked [noad]");

        // ── 诊断探针：不改行为，只在关键路径打一行日志（判据：服务端是否放行）──
        P(() => PatchProbe("AdsRewardMenu", "OnlyReward", 0, "AdsRewardMenu.OnlyReward()"),
            "probe AdsRewardMenu.OnlyReward");
        P(() => PatchProbe("AdsRewardMenu", "SetGetBtn", 1, "AdsRewardMenu.SetGetBtn(uint)"),
            "probe AdsRewardMenu.SetGetBtn");
        P(() => PatchProbe("AdsRewardMenu", "GetReward", 0, "AdsRewardMenu.GetReward()"),
            "probe AdsRewardMenu.GetReward");
        P(() => PatchProbe("SearchMapMenuLogic", "SetAdsBtnState", 1, "SearchMapMenuLogic.SetAdsBtnState"),
            "probe SearchMapMenuLogic.SetAdsBtnState");

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
    public static readonly (string Type, string Method)[] Targets =
    {
        ("Person",  "TakeDamage"),
        ("Person",  "TakeDamageDistant"),
        ("Person",  "Hurt"),
        ("SkillCD", "isTimeOver"),
        ("SkillCD", "CanUse"),
    };

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
        // 注意：复核时读的是**新**模块，方法引用是新对象 ⇒ 只能按名字比，不能比引用。
        static bool HeadIsCallNamed(MethodDefinition m, string name) =>
            m.Body.Instructions.Count > 0 && m.Body.Instructions[0].OpCode == OpCodes.Call &&
            m.Body.Instructions[0].Operand is MethodReference r && r.Name == name;
        static bool HeadIs(MethodDefinition m, OpCode oc) =>
            m.Body.Instructions.Count > 0 && m.Body.Instructions[0].OpCode == oc;
        static bool Calls(MethodDefinition m, string name) =>
            m.Body.Instructions.Any(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference r && r.Name == name);
        static bool LoadsField(MethodDefinition m, string f) =>
            m.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference r && r.Name == f);

        // ① 无敌：TakeDamage/TakeDamageDistant 首条必须是 ldarg.0（→ God(this)）
        foreach (var mn in new[] { "TakeDamage", "TakeDamageDistant" })
        {
            var m = Need("Person", mn);
            if (m == null) continue;
            A($"Person.{mn}", HeadIs(m, OpCodes.Ldarg_0) && Calls(m, "God"),
                $"首条={m.Body.Instructions[0].OpCode.Name}；调用 God={Calls(m, "God")}；IL 条数={m.Body.Instructions.Count}");
        }
        // ② Hurt：God + Mult 都要在，且 starg 落在 slot 1（damage，不是相邻参数）
        {
            var m = Need("Person", "Hurt");
            if (m != null)
            {
                var starg = m.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Starg || i.OpCode == OpCodes.Starg_S);
                var pi = starg?.Operand as ParameterDefinition;
                A("Person.Hurt", Calls(m, "God") && Calls(m, "Mult") && pi != null &&
                                 pi.Name == "damage" && pi.ParameterType.FullName == "System.Single",
                    $"God={Calls(m, "God")} Mult={Calls(m, "Mult")} starg→{pi?.Name}:{pi?.ParameterType.Name}（须为 damage:System.Single）");
            }
        }
        // ③ 无限技能：两个 getter 首条必须是 call（不是 ldarg.0）
        foreach (var mn in new[] { "isTimeOver", "CanUse" })
        {
            var m = Need("SkillCD", mn);
            if (m == null) continue;
            A($"SkillCD.{mn}", HeadIsCallNamed(m, "NoCD"),
                $"首条={m.Body.Instructions[0].OpCode.Name} {m.Body.Instructions[0].Operand}；调用 NoCD={Calls(m, "NoCD")}");
        }
        // ④ 注入类型本体
        var cheat = mm.GetType("Starry.StarryCheat");
        if (cheat == null) A("Starry.StarryCheat", false, "注入类型不存在");
        else
        {
            var names = cheat.Methods.Select(x => x.Name).ToList();
            bool ok = names.Contains("Raw") && names.Contains("God") && names.Contains("Mult") && names.Contains("NoCD") && names.Contains("NoAd");
            bool path = cheat.Methods.Where(x => x.HasBody)
                            .SelectMany(x => x.Body.Instructions.AsEnumerable())
                            .Any(i => i.OpCode == OpCodes.Ldstr && ((string)i.Operand ?? "") == STATE_PATH);
            A("Starry.StarryCheat", ok && path, $"方法={string.Join(",", names)}；状态文件路径已内联={path}");
            A("Starry.StarryCheat.IsPlayer", cheat.Methods.Any(x => LoadsField(x, "IsPlayer")),
                "God()/Mult() 必须读 Person.IsPlayer 作为归属判据");
        }
        // ⑤ 免广告：三个真实播放入口首条必须是 call NoAd（→ 直接走完成回调）
        foreach (var spec in new[]
                 {
                     "AdsRewardMenu::GetAds",
                     "SearchMapMenuLogic::SearchAllByAds",
                     "BuildingUpgradeView::OnBtnFinishUpgradeByAdsClicked",
                 })
        {
            var sp = spec.Split(new[] { "::" }, StringSplitOptions.None);
            var m = Need(sp[0], sp[1]);
            if (m == null) continue;
            A($"Ad.{sp[1]}", HeadIsCallNamed(m, "NoAd"),
                $"首条={m.Body.Instructions[0].OpCode.Name} {m.Body.Instructions[0].Operand}；调用 NoAd={Calls(m, "NoAd")}");
        }
        // ⑥ 诊断探针：AdsRewardMenu.OnlyReward()（0 参重载）首条必须是 call Probe
        //    （探针未注入时跳过：探针只是取证手段，不能因为拿不到判据就卡出包）
        if (mProbe == null)
        {
            Console.WriteLine("[warn] 探针未注入（未定位到 UnityEngine.CoreModule）⇒ 跳过 ⑥ 探针断言");
        }
        else
        {
            var m = mm.GetType("AdsRewardMenu")?.Methods.FirstOrDefault(x => x.Name == "OnlyReward" && x.Parameters.Count == 0);
            if (m == null) fails.Add("AdsRewardMenu::OnlyReward() NOT FOUND（探针未落到该方法）");
            else
            {
                var h0 = m.Body.Instructions[0];
                var lit = h0.OpCode == OpCodes.Ldstr ? (h0.Operand as string) : null;
                A("Probe.AdsRewardMenu.OnlyReward",
                    lit != null && lit.StartsWith(PROBE_PREFIX, StringComparison.Ordinal) && Calls(m, "Probe"),
                    $"首条={h0.OpCode.Name} \"{lit}\"；前缀={PROBE_PREFIX}；调用 Probe={Calls(m, "Probe")}");
            }
        }
    }

    // ── 引用程序集搜索目录 ──────────────────────────────────────────────────
    // EndlessStrike.dll 是**真 .NET 程序集**（非 IL2CPP dump），引用 mscorlib/UniRx/UnityEngine.* 等。
    // 有真实参照程序集最好；没有则由 TolerantAssemblyResolver 合成空壳 + ShimFactory 垫片兜底。
    static IEnumerable<string> RefDirs(string srcPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(srcPath));
        yield return Path.Combine(dir, "refs");
        yield return "/tmp/xhzm/refs";
        yield return "/tmp/xhzm/DummyDll";
        yield return Path.Combine(dir, "refs_shim");
        var env = Environment.GetEnvironmentVariable("XHZM_REF_DIR");
        if (!string.IsNullOrEmpty(env)) yield return env;
    }

    static void P(Action f, string name)
    {
        f();
        nPatch++;
        Console.WriteLine($"[patch] {name}");
    }

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

    static AssemblyNameReference Corlib()
    {
        foreach (var want in new[] { "mscorlib", "System.Runtime", "netstandard", "System.Private.CoreLib" })
        {
            var r = mod.AssemblyReferences.FirstOrDefault(x => x.Name == want);
            if (r != null) return r;
        }
        throw new Exception("找不到 corlib 引用: " + string.Join(",", mod.AssemblyReferences.Select(x => x.Name)));
    }

    static void InsertAt(MethodDefinition m, int index, IEnumerable<Instruction> seq)
    {
        var ins = m.Body.Instructions;
        int i = index;
        foreach (var x in seq) ins.Insert(i++, x);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 补丁区
    // ═══════════════════════════════════════════════════════════════════════

    // TakeDamage(float,Person,bool,bool,bool[,bool]) / TakeDamageDistant(float,Person,bool,bool,bool)
    //   if (StarryCheat.God(this)) return 0f;
    static void PatchZeroIfPlayerGod(TypeDefinition person, string name, int paramCount)
    {
        var m = FindM(person, name, paramCount);
        var orig = m.Body.Instructions[0];
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, mGod),
            Instruction.Create(OpCodes.Brfalse, orig),
            Instruction.Create(OpCodes.Ldc_R4, 0f),
            Instruction.Create(OpCodes.Ret),
        });
    }

    // Hurt(float damage, Person owner, ref bool isDie, ref float elementRatio, bool withOwnerBuff, SkillTag tag)
    //   if (StarryCheat.God(this)) { isDie = false; return 0f; }
    //   int mul = StarryCheat.Mult(owner, this);
    //   if (mul > 1) damage *= mul;
    // IL 槽：0=this 1=damage 2=owner 3=isDie(bool&) 4=elementRatio(float&) 5=withOwnerBuff 6=tag
    static void PatchHurt(TypeDefinition person)
    {
        var m = FindM(person, "Hurt", 6);
        var pDamage = m.Parameters[0];
        var pOwner = m.Parameters[1];
        var pIsDie = m.Parameters[2];
        if (pDamage.Name != "damage" || pDamage.ParameterType.FullName != "System.Single")
            throw new Exception($"Hurt 参数 0 不是 float damage（实际 {pDamage.Name}:{pDamage.ParameterType.FullName}）");
        if (pOwner.Name != "owner" || pOwner.ParameterType.FullName != "Person")
            throw new Exception($"Hurt 参数 1 不是 Person owner（实际 {pOwner.Name}:{pOwner.ParameterType.FullName}）");
        if (pIsDie.Name != "isDie" || !pIsDie.ParameterType.IsByReference)
            throw new Exception($"Hurt 参数 2 不是 ref bool isDie（实际 {pIsDie.Name}:{pIsDie.ParameterType.FullName}）");

        m.Body.InitLocals = true;
        var vMul = new VariableDefinition(mod.TypeSystem.Int32);
        m.Body.Variables.Add(vMul);

        var orig = m.Body.Instructions[0];
        var lAfterGod = Instruction.Create(OpCodes.Nop);
        var lSkipMul = Instruction.Create(OpCodes.Nop);

        InsertAt(m, 0, new List<Instruction>
        {
            // if (God(this)) { isDie = false; return 0f; }
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, mGod),
            Instruction.Create(OpCodes.Brfalse, lAfterGod),
            Instruction.Create(OpCodes.Ldarg, pIsDie),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stind_I1),
            Instruction.Create(OpCodes.Ldc_R4, 0f),
            Instruction.Create(OpCodes.Ret),

            // int mul = Mult(owner, this);
            lAfterGod,
            Instruction.Create(OpCodes.Ldarg, pOwner),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, mMult),
            Instruction.Create(OpCodes.Stloc, vMul),
            Instruction.Create(OpCodes.Ldloc, vMul),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble, lSkipMul),

            // damage *= mul;
            Instruction.Create(OpCodes.Ldarg, pDamage),
            Instruction.Create(OpCodes.Ldloc, vMul),
            Instruction.Create(OpCodes.Conv_R4),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Starg, pDamage),

            lSkipMul,
        });
        // orig 仍然在 lSkipMul 之后（InsertAt 只插在第 0 位之前，orig 原样保留）
        _ = orig;
    }

    // isTimeOver() / CanUse() -> bool : if (StarryCheat.NoCD()) return true;
    static void PatchSkillCdBool(TypeDefinition skillCd, string name)
    {
        var m = FindM(skillCd, name, 0);
        if (m.ReturnType.FullName != "System.Boolean")
            throw new Exception($"{skillCd.FullName}::{name} 返回类型不是 bool（实际 {m.ReturnType.FullName}）");
        var orig = m.Body.Instructions[0];
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Call, mNoCd),
            Instruction.Create(OpCodes.Brfalse, orig),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ret),
        });
    }

    // ---------------- 免广告 ----------------
    // 入口「看广告」按钮 → 不播视频，直接调本类的广告完成回调（后续上报/发奖链路一字不改）。
    //   if (StarryCheat.NoAd()) { Probe("T.m"); this.m(); return; }
    static void PatchAdEntry(string typeName, string entry, int entryParams, string completion, int completionParams)
    {
        var t = FindTypeByName(typeName);
        var em = FindM(t, entry, entryParams);
        var cm = FindM(t, completion, completionParams);
        if (cm.Parameters.Count != 0)
            throw new Exception($"{typeName}::{completion} 必须无参（实际 {cm.Parameters.Count} 个）");
        if (em.Body.HasExceptionHandlers)
            Console.WriteLine($"[warn] {typeName}::{entry} 有异常处理区，头部插入会使首个 try 覆盖新增前置指令（需观察）");
        var orig = em.Body.Instructions[0];
        var list = new List<Instruction>
        {
            Instruction.Create(OpCodes.Call, mNoAd),
            Instruction.Create(OpCodes.Brfalse, orig),
        };
        if (mProbe != null)
        {
            list.Add(Instruction.Create(OpCodes.Ldstr, $"{typeName}.{entry}"));
            list.Add(Instruction.Create(OpCodes.Call, mProbe));
        }
        list.Add(Instruction.Create(OpCodes.Ldarg_0));
        list.Add(Instruction.Create(OpCodes.Call, cm));
        if (cm.ReturnType.FullName != "System.Void")
        {
            var v = new VariableDefinition(cm.ReturnType);
            em.Body.Variables.Add(v);
            em.Body.InitLocals = true;
            list.Add(Instruction.Create(OpCodes.Stloc, v));
        }
        list.Add(Instruction.Create(OpCodes.Ret));
        InsertAt(em, 0, list);
    }

    // 纯诊断：在某方法头部插一行日志（零行为改动），用于判定服务端是否放行。
    static void PatchProbe(string typeName, string method, int paramCount, string tag)
    {
        if (mProbe == null)
        {
            Console.WriteLine($"[skip] 探针不可用，跳过 {tag}");
            return;
        }
        var t = FindTypeByName(typeName);
        var m = FindM(t, method, paramCount);
        InsertAt(m, 0, new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, PROBE_PREFIX + tag),
            Instruction.Create(OpCodes.Call, mProbe),
        });
    }

    static TypeDefinition FindTypeByName(string name)
    {
        var t = mod.GetTypes().FirstOrDefault(x => x.Name == name);
        if (t == null) throw new Exception($"类型未找到（按名）: {name}");
        return t;
    }

    // ---------------- Starry.StarryCheat 注入 ----------------
    // 文件：/data/user/0/cn.alltune.endless/files/starry_state.txt（单行、空格分隔 8 个 int）
    // 2 秒缓存；文件缺失/解析异常 → 全默认（关）→ 与原版行为一致。
    static void BuildStarryCheat(TypeDefinition person, FieldReference fIsPlayer)
    {
        tStr = mod.TypeSystem.String;
        tI32 = mod.TypeSystem.Int32;
        tBool = mod.TypeSystem.Boolean;
        var tObj = mod.TypeSystem.Object;
        var tChar = mod.TypeSystem.Char;
        var tExc = new TypeReference("System", "Exception", mod, Corlib());
        var tStrArr = new ArrayType(tStr);
        var tCharArr = new ArrayType(tChar);

        var t = new TypeDefinition("Starry", "StarryCheat",
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract | TypeAttributes.BeforeFieldInit, tObj);
        mod.Types.Add(t);

        var fText = new FieldDefinition("g_text", FieldAttributes.Static | FieldAttributes.Private, tStr);
        var fToks = new FieldDefinition("g_tokens", FieldAttributes.Static | FieldAttributes.Private, tStrArr);
        var fNext = new FieldDefinition("g_next", FieldAttributes.Static | FieldAttributes.Private, tI32);
        t.Fields.Add(fText);
        t.Fields.Add(fToks);
        t.Fields.Add(fNext);

        // 外部方法引用
        var tEnv = new TypeReference("System", "Environment", mod, Corlib());
        var mTick = new MethodReference("get_TickCount", tI32, tEnv) { HasThis = false };
        var tFile = new TypeReference("System.IO", "File", mod, Corlib());
        var mReadAll = new MethodReference("ReadAllText", tStr, tFile) { HasThis = false };
        mReadAll.Parameters.Add(new ParameterDefinition(tStr));
        var mExists = new MethodReference("Exists", mod.TypeSystem.Boolean, tFile) { HasThis = false };
        mExists.Parameters.Add(new ParameterDefinition(tStr));
        var mSplit = new MethodReference("Split", tStrArr, tStr) { HasThis = true };
        mSplit.Parameters.Add(new ParameterDefinition(tCharArr));
        var mTrim = new MethodReference("Trim", tStr, tStr) { HasThis = true };
        var mLen = new MethodReference("get_Length", tI32, tStr) { HasThis = true };
        var mParse = new MethodReference("Parse", tI32, mod.TypeSystem.Int32) { HasThis = false };
        mParse.Parameters.Add(new ParameterDefinition(tStr));

        // .cctor
        var cctor = new MethodDefinition(".cctor",
            MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            mod.TypeSystem.Void);
        t.Methods.Add(cctor);
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fToks));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, fText));
        cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        // ── static int Raw(int idx) ────────────────────────────────────────
        var mRawDef = new MethodDefinition("Raw",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tI32);
        mRawDef.Parameters.Add(new ParameterDefinition("idx", ParameterAttributes.None, tI32));
        t.Methods.Add(mRawDef);
        var vToks = new VariableDefinition(tStrArr);
        var vS = new VariableDefinition(tStr);
        var vR = new VariableDefinition(tI32);
        mRawDef.Body.Variables.Add(vToks);
        mRawDef.Body.Variables.Add(vS);
        mRawDef.Body.Variables.Add(vR);
        mRawDef.Body.InitLocals = true;
        var g = mRawDef.Body.Instructions;

        var reload = Instruction.Create(OpCodes.Ldstr, STATE_PATH);
        var reloadAlt = Instruction.Create(OpCodes.Ldstr, STATE_PATH_ALT);
        var reloadAlt2 = Instruction.Create(OpCodes.Ldstr, STATE_PATH_FALLBACK);
        var reloadPri2 = Instruction.Create(OpCodes.Ldstr, STATE_PATH);
        var doPrimary = Instruction.Create(OpCodes.Nop);
        var tryAlt = Instruction.Create(OpCodes.Nop);
        var doAlt = Instruction.Create(OpCodes.Nop);
        var pathDone = Instruction.Create(OpCodes.Nop);
        var hReload = Instruction.Create(OpCodes.Pop);
        var skipReload = Instruction.Create(OpCodes.Nop);
        var lZero = Instruction.Create(OpCodes.Ldc_I4_0);
        var lRet = Instruction.Create(OpCodes.Ldloc, vR);

        // if (Environment.TickCount > g_next) { 重载（异常吞掉，全清空） }
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fNext));
        g.Add(Instruction.Create(OpCodes.Ble, skipReload));
        g.Add(Instruction.Create(OpCodes.Ldc_I4, 2000));
        g.Add(Instruction.Create(OpCodes.Call, mTick));
        g.Add(Instruction.Create(OpCodes.Add));
        g.Add(Instruction.Create(OpCodes.Stsfld, fNext));
        // try { g_text = ReadAllText(Exists(主路径) ? 主路径 : 兜底路径); ... }
        // 选路径：media 主 → Android/data 外部 → 私有兜底
        g.Add(reload);
        g.Add(Instruction.Create(OpCodes.Call, mExists));
        g.Add(Instruction.Create(OpCodes.Brtrue, doPrimary));
        g.Add(reloadAlt);
        g.Add(Instruction.Create(OpCodes.Call, mExists));
        g.Add(Instruction.Create(OpCodes.Brtrue, tryAlt));
        g.Add(reloadAlt2);
        g.Add(Instruction.Create(OpCodes.Br, pathDone));
        g.Add(tryAlt);
        g.Add(Instruction.Create(OpCodes.Br, doAlt));
        g.Add(doPrimary);
        g.Add(reloadPri2);
        g.Add(Instruction.Create(OpCodes.Br, pathDone));
        g.Add(doAlt);
        g.Add(reloadAlt);
        g.Add(pathDone);
        g.Add(Instruction.Create(OpCodes.Call, mReadAll));
        g.Add(Instruction.Create(OpCodes.Dup));
        g.Add(Instruction.Create(OpCodes.Stsfld, fText));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_4));
        g.Add(Instruction.Create(OpCodes.Newarr, tChar));
        EmitCharElem(g, 0, 32);
        EmitCharElem(g, 1, 10);
        EmitCharElem(g, 2, 13);
        EmitCharElem(g, 3, 9);
        g.Add(Instruction.Create(OpCodes.Callvirt, mSplit));
        g.Add(Instruction.Create(OpCodes.Stsfld, fToks));
        g.Add(Instruction.Create(OpCodes.Leave, skipReload));
        // catch (Exception) { g_text = null; g_tokens = null; }
        g.Add(hReload);
        g.Add(Instruction.Create(OpCodes.Ldnull));
        g.Add(Instruction.Create(OpCodes.Stsfld, fText));
        g.Add(Instruction.Create(OpCodes.Ldnull));
        g.Add(Instruction.Create(OpCodes.Stsfld, fToks));
        g.Add(Instruction.Create(OpCodes.Leave, skipReload));
        mRawDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = reload, TryEnd = hReload, HandlerStart = hReload, HandlerEnd = skipReload, CatchType = tExc,
        });

        g.Add(skipReload);
        // if (g_tokens == null) return 0;
        g.Add(Instruction.Create(OpCodes.Ldsfld, fToks));
        g.Add(Instruction.Create(OpCodes.Brfalse, lZero));
        g.Add(Instruction.Create(OpCodes.Ldsfld, fToks));
        g.Add(Instruction.Create(OpCodes.Stloc, vToks));
        // if (idx < 0 || g_tokens.Length <= idx) return 0;
        g.Add(Instruction.Create(OpCodes.Ldarg_0));
        g.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        g.Add(Instruction.Create(OpCodes.Blt, lZero));
        g.Add(Instruction.Create(OpCodes.Ldloc, vToks));
        g.Add(Instruction.Create(OpCodes.Ldlen));
        g.Add(Instruction.Create(OpCodes.Conv_I4));
        g.Add(Instruction.Create(OpCodes.Ldarg_0));
        g.Add(Instruction.Create(OpCodes.Ble, lZero));
        // s = g_tokens[idx].Trim(); if (s.Length == 0) return 0;
        g.Add(Instruction.Create(OpCodes.Ldloc, vToks));
        g.Add(Instruction.Create(OpCodes.Ldarg_0));
        g.Add(Instruction.Create(OpCodes.Ldelem_Ref));
        g.Add(Instruction.Create(OpCodes.Callvirt, mTrim));
        g.Add(Instruction.Create(OpCodes.Stloc, vS));
        g.Add(Instruction.Create(OpCodes.Ldloc, vS));
        g.Add(Instruction.Create(OpCodes.Callvirt, mLen));
        g.Add(Instruction.Create(OpCodes.Brfalse, lZero));
        // try { return int.Parse(s); } catch { return 0; }
        var tParse = Instruction.Create(OpCodes.Ldloc, vS);
        var hParse = Instruction.Create(OpCodes.Pop);
        g.Add(tParse);
        g.Add(Instruction.Create(OpCodes.Call, mParse));
        g.Add(Instruction.Create(OpCodes.Stloc, vR));
        g.Add(Instruction.Create(OpCodes.Leave, lRet));
        g.Add(hParse);
        g.Add(Instruction.Create(OpCodes.Leave, lZero));
        mRawDef.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = tParse, TryEnd = hParse, HandlerStart = hParse, HandlerEnd = lZero, CatchType = tExc,
        });
        g.Add(lZero);
        g.Add(Instruction.Create(OpCodes.Ret));
        g.Add(lRet);
        g.Add(Instruction.Create(OpCodes.Ret));
        mRaw = mRawDef;

        // ── static bool God(Person p) ──────────────────────────────────────
        // p != null && p.IsPlayer && Raw(FEAT_GOD) != 0
        var mGodDef = new MethodDefinition("God",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tBool);
        mGodDef.Parameters.Add(new ParameterDefinition("p", ParameterAttributes.None, person));
        t.Methods.Add(mGodDef);
        var gb = mGodDef.Body.Instructions;
        var gFalse = Instruction.Create(OpCodes.Ldc_I4_0);
        gb.Add(Instruction.Create(OpCodes.Ldarg_0));
        gb.Add(Instruction.Create(OpCodes.Brfalse, gFalse));
        gb.Add(Instruction.Create(OpCodes.Ldarg_0));
        gb.Add(Instruction.Create(OpCodes.Ldfld, fIsPlayer));
        gb.Add(Instruction.Create(OpCodes.Brfalse, gFalse));
        gb.Add(Instruction.Create(OpCodes.Ldc_I4, FEAT_GOD));
        gb.Add(Instruction.Create(OpCodes.Call, mRaw));
        gb.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        gb.Add(Instruction.Create(OpCodes.Cgt_Un));
        gb.Add(Instruction.Create(OpCodes.Ret));
        gb.Add(gFalse);
        gb.Add(Instruction.Create(OpCodes.Ret));
        mGod = mGodDef;

        // ── static int Mult(Person owner, Person sufferer) ─────────────────
        // owner != null && owner.IsPlayer && !(sufferer != null && sufferer.IsPlayer)
        //   && Raw(FEAT_DMG) > 1  →  Raw(FEAT_DMG)  否则 0
        var mMultDef = new MethodDefinition("Mult",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tI32);
        mMultDef.Parameters.Add(new ParameterDefinition("owner", ParameterAttributes.None, person));
        mMultDef.Parameters.Add(new ParameterDefinition("sufferer", ParameterAttributes.None, person));
        t.Methods.Add(mMultDef);
        var vM = new VariableDefinition(tI32);
        mMultDef.Body.Variables.Add(vM);
        mMultDef.Body.InitLocals = true;
        var mb = mMultDef.Body.Instructions;
        var mZero = Instruction.Create(OpCodes.Ldc_I4_0);
        var mGo = Instruction.Create(OpCodes.Nop);
        mb.Add(Instruction.Create(OpCodes.Ldarg_0));
        mb.Add(Instruction.Create(OpCodes.Brfalse, mZero));
        mb.Add(Instruction.Create(OpCodes.Ldarg_0));
        mb.Add(Instruction.Create(OpCodes.Ldfld, fIsPlayer));
        mb.Add(Instruction.Create(OpCodes.Brfalse, mZero));
        mb.Add(Instruction.Create(OpCodes.Ldarg_1));
        mb.Add(Instruction.Create(OpCodes.Brfalse, mGo));
        mb.Add(Instruction.Create(OpCodes.Ldarg_1));
        mb.Add(Instruction.Create(OpCodes.Ldfld, fIsPlayer));
        mb.Add(Instruction.Create(OpCodes.Brtrue, mZero));
        mb.Add(mGo);
        mb.Add(Instruction.Create(OpCodes.Ldc_I4, FEAT_DMG));
        mb.Add(Instruction.Create(OpCodes.Call, mRaw));
        mb.Add(Instruction.Create(OpCodes.Stloc, vM));
        mb.Add(Instruction.Create(OpCodes.Ldloc, vM));
        mb.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        mb.Add(Instruction.Create(OpCodes.Ble, mZero));
        mb.Add(Instruction.Create(OpCodes.Ldloc, vM));
        mb.Add(Instruction.Create(OpCodes.Ret));
        mb.Add(mZero);
        mb.Add(Instruction.Create(OpCodes.Ret));
        mMult = mMultDef;

        // ── static bool NoCD() ─────────────────────────────────────────────
        var mNoCdDef = new MethodDefinition("NoCD",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tBool);
        t.Methods.Add(mNoCdDef);
        mNoCdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, FEAT_NOCD));
        mNoCdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Call, mRaw));
        mNoCdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        mNoCdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Cgt_Un));
        mNoCdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        mNoCd = mNoCdDef;

        // ── static bool NoAd() ─────────────────────────────────────────────
        var mNoAdDef = new MethodDefinition("NoAd",
            MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, tBool);
        t.Methods.Add(mNoAdDef);
        mNoAdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, FEAT_NOAD));
        mNoAdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Call, mRaw));
        mNoAdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        mNoAdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Cgt_Un));
        mNoAdDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        mNoAd = mNoAdDef;

        // ── static void Probe(string tag) ──────────────────────────────────
        // 诊断探针（方案 A 判据取证）：只打一行日志，不改行为。
        // 走 UnityEngine.Debug.Log（logcat / Unity tag，带时间戳），不用 File IO
        // —— 热更 DLL 里写文件的目标路径权限不确定，宿主进程自己的日志最可靠。
        // ⚠ v3.4 血案修正：不能拿「第一个 namespace 以 UnityEngine 开头的 TypeReference」的 Scope。
        //    实测该 scope 是 UnityEngine.AnimationModule ⇒ 运行时报
        //    Could not load type 'UnityEngine.Debug' from assembly 'UnityEngine.AnimationModule'
        //    （发生在 AdsRewardMenu.Init → SetGetBtn），直接把广告面板 Init 打断 → 面板卡死。
        //    Debug 只住在 UnityEngine.CoreModule；解析不出就**整个不出探针**：宁可不取证，也不能把游戏打崩。
        var mLog = ResolveUnityDebugLog(mod);
        if (mLog == null)
        {
            Console.WriteLine("[warn] 未定位到可用的 UnityEngine.Debug.Log 引用 ⇒ 本次不出诊断探针（免广告仍生效，只是拿不到判据日志）");
        }
        else
        {
            var mProbeDef = new MethodDefinition("Probe",
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig, mod.TypeSystem.Void);
            mProbeDef.Parameters.Add(new ParameterDefinition("tag", ParameterAttributes.None, tStr));
            t.Methods.Add(mProbeDef);
            var pb = mProbeDef.Body.Instructions;
            // 纯直传：字符串拼接在**调用点**（编译期常量）完成，运行期只做一次 ldarg/call
            // —— 不再自建 String::Concat / Debug.Log 签名（两处手搓签名各炸过一次，见 ResolveUnityDebugLog 注释）
            pb.Add(Instruction.Create(OpCodes.Ldarg_0));
            pb.Add(Instruction.Create(OpCodes.Call, mLog));
            pb.Add(Instruction.Create(OpCodes.Ret));
            mProbe = mProbeDef;
            Console.WriteLine($"[cfg] 诊断探针已注入（Debug.Log 直传, 前缀 \"{PROBE_PREFIX}\"）");
        }

        Console.WriteLine($"[cfg] Starry.StarryCheat 已注入（2s 缓存, {STATE_PATH}, god={FEAT_GOD} dmg={FEAT_DMG} nocd={FEAT_NOCD} noad={FEAT_NOAD}）");
    }

    // ── UnityEngine.Debug 的**正确**声明程序集解析 ────────────────────────────
    // Tuanjie / Unity 2019+ 里 UnityEngine.Debug 属于 UnityEngine.CoreModule.dll。
    // 库里任何 namespace 以 UnityEngine 开头的 TypeReference 都可能是 AnimationModule /
    // PhysicsModule 等 ⇒ scope 用错就是运行时 TypeLoad 爆炸（v3.3 实测：广告面板 Init 被打断）。
    // ── UnityEngine.Debug.Log 的**真实**方法引用 ────────────────────────────────
    // v3.4 血案二（logcat 实证 2026-10-10 19:44:52）：
    //   手搓 `new MethodReference("Log", void, tDbg)` 且把参数写成 System.String
    //   ⇒ 运行时 `MethodNotFind UnityEngine.Debug::Log`
    //      at AdsRewardMenu.SetGetBtn (System.UInt32 progress) → AdsRewardMenu.Init() 中断 → 广告面板卡死。
    //   （血案一：手搓 DeclaringType 落到 AnimationModule ⇒ Could not load type。）
    // 铁律：**不要手搓 Unity API 的签名**。优先借用 DLL 里真实调用点上的 MethodReference
    //   —— 它的 scope 与签名必然和运行期一致；实在没有调用点，才自建正确的 Log(object)。
    static MethodReference ResolveUnityDebugLog(ModuleDefinition mod)
    {
        // ① 库里确实有人调过 Debug.Log ⇒ 直接借（含 CoreModule scope + 真实参数类型）
        foreach (var mr in mod.GetMemberReferences().OfType<MethodReference>())
        {
            if (mr.Name != "Log") continue;
            if (mr.HasGenericParameters || mr.GenericParameters.Count > 0) continue;
            if (mr.Parameters.Count != 1) continue;
            var dt = mr.DeclaringType;
            if (dt == null || dt.Namespace != "UnityEngine" || dt.Name != "Debug") continue;
            if (!(dt.Scope is AssemblyNameReference a) || a.Name != "UnityEngine.CoreModule") continue;
            Console.WriteLine($"[cfg] 探针借用库内既有 Debug.Log：{mr.FullName}"
                + $"（参数类型 {mr.Parameters[0].ParameterType.FullName}, scope {a.Name}）");
            return mr;
        }
        // ② 没有现成调用点 ⇒ 用 CoreModule 的**正确签名** Log(object) 自建
        var tDbg = ResolveUnityDebugType(mod);
        if (tDbg == null) return null;
        var mLog = new MethodReference("Log", mod.TypeSystem.Void, tDbg) { HasThis = false };
        mLog.Parameters.Add(new ParameterDefinition(mod.TypeSystem.Object));
        Console.WriteLine($"[cfg] 探针自建 Debug.Log(object)：scope={tDbg.Scope}（库里无既有调用点）");
        return mLog;
    }

    // 三级回退，全失败返回 null（调用方据此放弃探针）。
    static TypeReference ResolveUnityDebugType(ModuleDefinition mod)
    {
        const string NS = "UnityEngine";
        const string NAME = "Debug";
        const string CORE = "UnityEngine.CoreModule";

        // ① 库里已经有人调过 Debug.Log/LogWarning/… ⇒ 借它的 DeclaringType，scope 必然正确
        foreach (var mr in mod.GetMemberReferences().OfType<MethodReference>())
        {
            var dt = mr.DeclaringType;
            if (dt != null && dt.Namespace == NS && dt.Name == NAME
                && dt.Scope is AssemblyNameReference a0 && a0.Name == CORE)
            {
                return dt;
            }
        }
        // ② 现成 TypeReference，但必须是 CoreModule 的（AnimationModule 之类一律丢弃）
        var good = mod.GetTypeReferences().FirstOrDefault(r =>
            r.Namespace == NS && r.Name == NAME
            && r.Scope is AssemblyNameReference a1 && a1.Name == CORE);
        if (good != null) return good;
        // ③ 自己造一个：只有本模块真的引用了 CoreModule 才安全
        var core = mod.AssemblyReferences.FirstOrDefault(x => x.Name == CORE);
        if (core == null)
        {
            var anyUni = mod.GetTypeReferences().FirstOrDefault(r => r.Namespace != null && r.Namespace.StartsWith(NS));
            Console.WriteLine("[warn] 本模块未引用 " + CORE
                + (anyUni == null ? "，也没有任何 UnityEngine 类型引用" : $"（可疑 scope: {anyUni.Scope}）")
                + " ⇒ 不出探针");
            return null;
        }
        return new TypeReference(NS, NAME, mod, core);
    }

    static void EmitCharElem(IList<Instruction> g, int index, int ch)
    {
        g.Add(Instruction.Create(OpCodes.Dup));
        g.Add(Instruction.Create(OpCodes.Ldc_I4, index));
        g.Add(Instruction.Create(OpCodes.Ldc_I4, ch));
        g.Add(Instruction.Create(OpCodes.Stelem_I2));
    }

    static TypeReference tStr, tI32, tBool;
}
