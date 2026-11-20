using Mono.Cecil;
using Mono.Cecil.Cil;

// 冒险战争 (com.mxzz.zbml) 热更程序集 IL 补丁器。
//
// 模式：注入读文本配置的类型（Starry.StarryConfig），再对目标方法做「入口前置门控」——
//   配置缺失或开关关闭时，原 IL 原样执行（不删改任何原指令，只做插入）。
//
// ★★ 静态结论门禁（用户 2026-11-19 硬要求）★★
//   本文件的补丁语义只能来自「静态分析 → 代码反推验证 → 结论表」，
//   结论落在 10_docs/冒险战争/实现记录_星光版.md §静态结论 与 re-analyst 报告；
//   结论未完整落地时 ApplyPatches() 抛 StaticConclusionPending，禁止产出补丁体。
//
// 铁律（见 04_scripts/_templates/ilpatch/README.md）：
//   1. 参数下标：Parameters 不含 this ⇒ SlotToParamIndex(slot) = HasThis ? slot-1 : slot
//   2. 写盘期解析：任何成员类型解析不到都会中止 Write ⇒ 靠 TolerantRefs.cs 兜底
//   3. 字节容器：产物比原文件大 ⇒ native 在加载边界按新长度重新分配容器
internal static class Patcher
{
    const string FILE_NAME = "starry_config.ini";
    // 目标包私有目录（native 侧 MxzzMod.cpp 写同一路径）。禁 Application.persistentDataPath。
    static string CfgPath = "/data/user/0/com.mxzz.zbml/files/" + FILE_NAME;
    static string LogPath = "/data/user/0/com.mxzz.zbml/files/mxzz_patch.log";

    static ModuleDefinition mod;
    static MethodReference mGetBool, mGetInt;
    static TypeReference tStr, tI32, tBool;
    static int nPatch;
    // b10：探针计数器字段（BuildHelper 里创建，ApplyPatches 里自增；native 侧读取）
    static FieldDefinition FAdd, FSet, FRecv, FWake, FStart;

    // 被补丁的方法清单（Program.cs 据此打印完整 IL + 断言落点）
    public static readonly (string Type, string Method)[] Targets =
    {
        ("Battle.Life", "AddHP"),
    };

    public static void Run(string srcPath, string dstPath, string pkg)
    {
        if (!string.IsNullOrEmpty(pkg)) { CfgPath = "/data/user/0/" + pkg + "/files/" + FILE_NAME; LogPath = "/data/user/0/" + pkg + "/files/mxzz_patch.log"; }

        // 宽容解析链：缺引用程序集时不能整包写盘失败（Cecil 对每个成员都 GetConstantType → Resolve）
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

        // 垫片：把「有真默认值、但类型解析不到」的枚举补成真 dll，保证常量不丢
        var realDirs = RefDirs(srcPath).Where(Directory.Exists).ToList();
        var needs = ShimFactory.Discover(mod, realDirs);
        var shimDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcPath)), "refs_shim");
        var nShim = ShimFactory.Materialize(shimDir, needs);
        if (nShim > 0) { resolver.AddSearchDirectory(shimDir); Console.WriteLine($"[shim] {nShim} 个垫片程序集已就绪 → {shimDir}"); }

        BuildStarryConfig();

        ApplyPatches();

        // 写盘前后复核「真实常量」没有被宽容解析吞掉
        var constantsBefore = ConstantAudit.Snapshot(mod);
        var wp = new WriterParameters { WriteSymbols = false };
        asm.Write(dstPath, wp);
        asm.Dispose();
        Console.WriteLine($"[out] {dstPath}  ({new FileInfo(dstPath).Length} B)");
        Console.WriteLine($"[ok ] patches={nPatch}");

        using var back = AssemblyDefinition.ReadAssembly(dstPath,
            new ReaderParameters { AssemblyResolver = resolver, MetadataResolver = metaResolver, ReadSymbols = false });
        var lost = ConstantAudit.Diff(constantsBefore, back.MainModule);
        Console.WriteLine(lost.Count == 0
            ? $"[gate] 常量台账 PASS（{constantsBefore.Count} 条真实默认值全部保留）"
            : $"[gate] 常量台账 FAIL（丢失 {lost.Count} 条）：\n  " + string.Join("\n  ", lost));
        if (lost.Count != 0) Environment.Exit(1);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  补丁区 —— 只有拿到「静态定位 + 反推验证」结论后才允许填写
    // ═══════════════════════════════════════════════════════════════════════
    static void ApplyPatches()
    {
        // ── 静态定位（证据：--info 输出，见 10_docs/冒险战争/实现记录_星光版.md §二）──
        // dev 1.1.085 : Battle.Life AddHP token=0x0601A468  _curHP 0x04012A89
        // APK 基线    : Battle.Life AddHP token=0x06019125  _curHP 0x04011FB6
        // 两版 AddHP 均 IL=245 / maxstack=8 / initlocals=True / locals=6 / EH=0
        var life = FindType("Battle", "Life");
        var addHp = life.Methods.FirstOrDefault(m => m.Name == "AddHP" && m.Parameters.Count == 3)
                    ?? throw new Exception("Battle.Life::AddHP(3 params) 未找到 —— 目标程序集与静态结论不符");
        AssertAddHpSignature(addHp);

        var helper = BuildHelper(life);
        var allOff  = helper.Methods.First(m => m.Name == "AllOff");
        var adjust  = helper.Methods.First(m => m.Name == "AdjustValue");

        // ── 前缀注入（9 条；语义见实现记录 §三「选择理由」）──
        //   if (AllOff()) goto ORIG;                       // 全关 ⇒ 零 JInt 读/写/转换
        //   if (AdjustValue(this, value, out adj)) value = adj;   // god→0 / kill→-(int)curHP-1
        //   （不早退：value 改写后**落回原方法体**，保留 _curHP 写入/OnLifeHPChange/
        //     AddLogicRecord 的单一写路径 —— 静态证据：AddHP 尾部有 4 处 _curHP stfld，
        //     早退会跳过全部记录，造成状态分叉）
        var jint = mod.GetType("JEngine.AntiCheat.JInt")
                   ?? throw new Exception("JEngine.AntiCheat.JInt 未找到");
        var body = addHp.Body;
        var il = body.GetILProcessor();
        var first = body.Instructions[0];
        var localAdj = new VariableDefinition(mod.ImportReference(jint));
        body.InitLocals = true;
        body.Variables.Add(localAdj);

        var enter = helper.Methods.First(m => m.Name == "Enter");
        var seq = new[]
        {
            // b10：零依赖计数（int 自增，不调外部方法）
            il.Create(OpCodes.Ldsfld, FAdd),
            il.Create(OpCodes.Ldc_I4_1),
            il.Create(OpCodes.Add),
            il.Create(OpCodes.Stsfld, FAdd),
            // b7 诊断：仅记录「AddHP 被调用」（2s 限流），不改语义
            il.Create(OpCodes.Call, enter),
            il.Create(OpCodes.Call, mod.ImportReference(allOff)),
            il.Create(OpCodes.Brtrue, first),
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Ldarg_1),
            il.Create(OpCodes.Ldloca, localAdj),
            il.Create(OpCodes.Call, mod.ImportReference(adjust)),
            il.Create(OpCodes.Brfalse, first),
            il.Create(OpCodes.Ldloc, localAdj),
            il.Create(OpCodes.Starg, addHp.Parameters[0]),
        };
        foreach (var ins in seq) il.InsertBefore(first, ins);
        if (body.MaxStackSize < 8) body.MaxStackSize = 8;
        Console.WriteLine($"[map] AddHP token=0x{addHp.MetadataToken.ToInt32():X8} 注入 {seq.Length} 条前缀；localAdj=V{localAdj.Index}({jint.FullName})");
        nPatch++;

        // ── b9 诊断：伤害路径探针（只加「一条 Log 调用」，不改任何语义）──
        //   目的：若 AddHP 探针不出现，用 SetHP / ReceiveDamage 探针判断「伤害走了哪条路」或「补丁体根本没执行」。
        var helperLog = helper.Methods.First(m => m.Name == "Log");
        void InjectMarker(MethodDefinition mm, string text, FieldDefinition counter)
        {
            var mp = mm.Body.GetILProcessor();
            var firstIns = mm.Body.Instructions[0];
            // b10：先做零依赖计数（int++），再走日志通道（日志失败也不影响计数）
            mp.InsertBefore(firstIns, mp.Create(OpCodes.Ldsfld, counter));
            mp.InsertBefore(firstIns, mp.Create(OpCodes.Ldc_I4_1));
            mp.InsertBefore(firstIns, mp.Create(OpCodes.Add));
            mp.InsertBefore(firstIns, mp.Create(OpCodes.Stsfld, counter));
            mp.InsertBefore(firstIns, mp.Create(OpCodes.Ldstr, text));
            mp.InsertBefore(firstIns, mp.Create(OpCodes.Call, mod.ImportReference(helperLog)));
            if (mm.Body.MaxStackSize < 8) mm.Body.MaxStackSize = 8;
            Console.WriteLine($"[map] 探针已注入 {mm.DeclaringType.Name}::{mm.Name}  text={text} counter={counter.Name}");
        }
        var setHp = life.Methods.FirstOrDefault(x => x.Name == "SetHP" && x.Parameters.Count == 1)
                    ?? throw new Exception("Battle.Life::SetHP(1 参) 未找到 —— 探针静态前提不符");
        InjectMarker(setHp, "[P] enter SetHP", FSet);
        var recvDmg = life.Methods.FirstOrDefault(x => x.Name == "ReceiveDamage" && x.Parameters.Count == 7);
        if (recvDmg != null) InjectMarker(recvDmg, "[P] enter ReceiveDamage", FRecv);
        else Console.WriteLine("[map] 警告：ReceiveDamage(7 参) 未找到，跳过该探针");

        // ── b11 机制自检：把计数器挂到「启动必执行」的 MonoBehaviour 上 ──
        //   依据：GTLauncher : UnityEngine.MonoBehaviour，Awake(IL=12)/Start(IL=55) 必在开局执行；
        //   若这两个计数也不涨 ⇒ 问题在「计数器读取通道」而不是「战斗不走 AddHP」。
        void InjectCounter(MethodDefinition mm, FieldDefinition counter)
        {
            var mp = mm.Body.GetILProcessor();
            var fi = mm.Body.Instructions[0];
            mp.InsertBefore(fi, mp.Create(OpCodes.Ldsfld, counter));
            mp.InsertBefore(fi, mp.Create(OpCodes.Ldc_I4_1));
            mp.InsertBefore(fi, mp.Create(OpCodes.Add));
            mp.InsertBefore(fi, mp.Create(OpCodes.Stsfld, counter));
            if (mm.Body.MaxStackSize < 8) mm.Body.MaxStackSize = 8;
            Console.WriteLine($"[map] 自检计数已注入 {mm.DeclaringType.Name}::{mm.Name} counter={counter.Name}");
        }
        var gtl = mod.GetType("GTLauncher");
        if (gtl != null) {
            foreach (var pair in new[] { ("Awake", FWake), ("Start", FStart) })
            {
                var mm = gtl.Methods.FirstOrDefault(x => x.Name == pair.Item1 && x.Parameters.Count == 0);
                if (mm != null) InjectCounter(mm, pair.Item2);
                else Console.WriteLine($"[map] 警告：GTLauncher::{pair.Item1}() 未找到，跳过自检计数");
            }
        } else Console.WriteLine("[map] 警告：类型 GTLauncher 未找到，跳过启动自检计数");
    }

    // 参数名/类型断言：签名漂移（同名不同义）必须在这里炸，而不是等到真机
    static void AssertAddHpSignature(MethodDefinition m)
    {
        var p = m.Parameters;
        void Need(int i, string name, string type)
        {
            var got = p[i].ParameterType.FullName;
            if (p[i].Name != name || got != type)
                throw new Exception($"AddHP 参数[{i}] 期望 {type} {name}，实际 {got} {p[i].Name} —— 静态结论不符");
        }
        Need(0, "value", "JEngine.AntiCheat.JInt");
        Need(1, "pLauncher", "Battle.Life");
        Need(2, "RecursionCount", "System.Int32");
        Console.WriteLine("[map] AddHP 签名核对通过: (JInt value, Life pLauncher, Int32 RecursionCount) -> Void");
    }

    static TypeDefinition BuildHelper(TypeDefinition life)
    {
        var jint = mod.GetType("JEngine.AntiCheat.JInt")
                   ?? throw new Exception("JEngine.AntiCheat.JInt 未找到");
        var getPlayer = life.Methods.FirstOrDefault(m => m.Name == "GetPlayer" && m.Parameters.Count == 0)
                        ?? throw new Exception("Battle.Life::GetPlayer 未找到");
        var playerTd = getPlayer.ReturnType.Resolve() ?? throw new Exception("Battle.Player 解析失败");
        var getCamp = playerTd.Methods.FirstOrDefault(m => m.Name == "GetCamp" && m.Parameters.Count == 0)
                      ?? throw new Exception("Battle.Player::GetCamp 未找到");
        var campTd = getCamp.ReturnType.Resolve() ?? throw new Exception("E_CAMP 解析失败");
        int campEnemy = EnumValue(campTd, "ENEMY_CAMP");
        int campFriend = EnumValue(campTd, "FRIEND_CAMP");
        Console.WriteLine($"[map] {campTd.FullName}: ENEMY_CAMP={campEnemy} FRIEND_CAMP={campFriend}");

        var getValue = jint.Methods.FirstOrDefault(m => m.Name == "get_Value" && m.ReturnType.FullName == "System.Int64")
                       ?? throw new Exception("JInt::get_Value():Int64 未找到");
        var curHp = life.Fields.FirstOrDefault(f => f.Name == "_curHP")
                    ?? throw new Exception("Battle.Life::_curHP 未找到");
        if (curHp.FieldType.FullName != "JEngine.AntiCheat.JInt")
            throw new Exception($"_curHP 期望 JInt，实际 {curHp.FieldType.FullName} —— 静态结论不符");
        var ctorLong = jint.Methods.FirstOrDefault(m => m.IsConstructor && m.Parameters.Count == 1
                            && m.Parameters[0].ParameterType.FullName == "System.Int64")
                       ?? throw new Exception("JInt::.ctor(Int64) 未找到");
        Console.WriteLine($"[map] _curHP token=0x{curHp.MetadataToken.ToInt32():X8} type={curHp.FieldType.FullName}");

        var coreLib = Corlib();
        var strT = mod.TypeSystem.String;
        var intT = mod.TypeSystem.Int32;
        var boolT = mod.TypeSystem.Boolean;
        var voidT = mod.TypeSystem.Void;

        var fileType = new TypeReference("System.IO", "File", mod, coreLib);
        var readAll = new MethodReference("ReadAllText", strT, fileType) { HasThis = false };
        readAll.Parameters.Add(new ParameterDefinition(strT));

        var envType = new TypeReference("System", "Environment", mod, coreLib);
        var tick = new MethodReference("get_TickCount", intT, envType) { HasThis = false };

        var contains = new MethodReference("Contains", boolT, strT) { HasThis = true };
        contains.Parameters.Add(new ParameterDefinition(strT));

        var exType = new TypeReference("System", "Exception", mod, coreLib);

        var t = new TypeDefinition("Starry", "MxzzPatch",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            mod.TypeSystem.Object);
        mod.Types.Add(t);
        var fGod  = new FieldDefinition("s_god",  FieldAttributes.Static | FieldAttributes.Private, intT);
        var fKill = new FieldDefinition("s_kill", FieldAttributes.Static | FieldAttributes.Private, intT);
        var fTick = new FieldDefinition("s_tick", FieldAttributes.Static | FieldAttributes.Private, intT);
        t.Fields.Add(fGod); t.Fields.Add(fKill); t.Fields.Add(fTick);

        // ---- Refresh(): 1 秒缓存读 config；任何异常 ⇒ 两个开关都按关 ----
        var refresh = new MethodDefinition("Refresh",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, voidT);
        refresh.Body.InitLocals = true;
        refresh.Body.Variables.Add(new VariableDefinition(strT));
        t.Methods.Add(refresh);
        var r = refresh.Body.GetILProcessor();
        var ret = r.Create(OpCodes.Ret);
        r.Append(r.Create(OpCodes.Call, tick));
        r.Append(r.Create(OpCodes.Ldsfld, fTick));
        r.Append(r.Create(OpCodes.Sub));
        r.Append(r.Create(OpCodes.Ldc_I4, 1000));
        r.Append(r.Create(OpCodes.Blt, ret));
        r.Append(r.Create(OpCodes.Call, tick));
        r.Append(r.Create(OpCodes.Stsfld, fTick));
        r.Append(r.Create(OpCodes.Ldc_I4_0)); r.Append(r.Create(OpCodes.Stsfld, fGod));
        r.Append(r.Create(OpCodes.Ldc_I4_0)); r.Append(r.Create(OpCodes.Stsfld, fKill));
        var tryStart = r.Create(OpCodes.Ldstr, CfgPath);
        r.Append(tryStart);
        r.Append(r.Create(OpCodes.Call, readAll));
        r.Append(r.Create(OpCodes.Stloc_0));
        var s1 = r.Create(OpCodes.Nop);
        r.Append(r.Create(OpCodes.Ldloc_0)); r.Append(r.Create(OpCodes.Ldstr, "god=1"));
        r.Append(r.Create(OpCodes.Callvirt, contains)); r.Append(r.Create(OpCodes.Brfalse, s1));
        r.Append(r.Create(OpCodes.Ldc_I4_1)); r.Append(r.Create(OpCodes.Stsfld, fGod));
        r.Append(s1);
        var s2 = r.Create(OpCodes.Nop);
        r.Append(r.Create(OpCodes.Ldloc_0)); r.Append(r.Create(OpCodes.Ldstr, "kill=1"));
        r.Append(r.Create(OpCodes.Callvirt, contains)); r.Append(r.Create(OpCodes.Brfalse, s2));
        r.Append(r.Create(OpCodes.Ldc_I4_1)); r.Append(r.Create(OpCodes.Stsfld, fKill));
        r.Append(s2);
        r.Append(r.Create(OpCodes.Leave, ret));
        var tryEnd = r.Create(OpCodes.Nop); r.Append(tryEnd);
        var hStart = r.Create(OpCodes.Pop); r.Append(hStart);
        r.Append(r.Create(OpCodes.Leave, ret));
        var hEnd = r.Create(OpCodes.Nop); r.Append(hEnd);
        r.Append(ret);
        refresh.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        { CatchType = exType, TryStart = tryStart, TryEnd = tryEnd, HandlerStart = hStart, HandlerEnd = hEnd });

        // ---- CampOf(Life self) -> int（null/未初始化 ⇒ -1，fail-safe 走原逻辑）----
        var campOf = new MethodDefinition("CampOf",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, intT);
        campOf.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, life));
        t.Methods.Add(campOf);
        var c = campOf.Body.GetILProcessor();
        var nn = c.Create(OpCodes.Nop);
        c.Append(c.Create(OpCodes.Ldarg_0));
        c.Append(c.Create(OpCodes.Brtrue, nn));
        c.Append(c.Create(OpCodes.Ldc_I4_M1)); c.Append(c.Create(OpCodes.Ret));
        c.Append(nn);
        var pn = c.Create(OpCodes.Nop);
        c.Append(c.Create(OpCodes.Ldarg_0));
        c.Append(c.Create(OpCodes.Callvirt, mod.ImportReference(getPlayer)));
        c.Append(c.Create(OpCodes.Brtrue, pn));
        c.Append(c.Create(OpCodes.Ldc_I4_M1)); c.Append(c.Create(OpCodes.Ret));
        c.Append(pn);
        c.Append(c.Create(OpCodes.Ldarg_0));
        c.Append(c.Create(OpCodes.Callvirt, mod.ImportReference(getPlayer)));
        c.Append(c.Create(OpCodes.Callvirt, mod.ImportReference(getCamp)));
        c.Append(c.Create(OpCodes.Conv_I4));
        c.Append(c.Create(OpCodes.Ret));

        // ---- b7 诊断：只加观测，不改语义 ----
        //   输出：/data/user/0/<pkg>/files/mxzz_patch.log，每行前缀 "[P]"，单次运行最多 200 行；
        //   全部写入包在 try/catch 里（日志失败绝不影响功能）。
        var fLogN    = new FieldDefinition("s_logLines",  FieldAttributes.Static | FieldAttributes.Private, intT);
        var fTickEnt = new FieldDefinition("s_tickEnter", FieldAttributes.Static | FieldAttributes.Private, intT);
        var fTickAdj = new FieldDefinition("s_tickAdj",   FieldAttributes.Static | FieldAttributes.Private, intT);
        t.Fields.Add(fLogN); t.Fields.Add(fTickEnt); t.Fields.Add(fTickAdj);
        // b10：**零依赖探针计数器**——只做 int 自增，不调任何外部方法（不依赖 File/Debug/字符串），
        //   由 native 侧经 il2cpp_field_static_get_value 读取后写 logcat。
        //   依据：b8/b9 的 File 与 Debug.Log 两条通道在本运行时全部静默，无法区分「方法没被调用」与「日志调用失败」。
        var fCntAdd  = new FieldDefinition("s_cAdd",  FieldAttributes.Static | FieldAttributes.Private, intT);
        var fCntSet  = new FieldDefinition("s_cSet",  FieldAttributes.Static | FieldAttributes.Private, intT);
        var fCntRecv = new FieldDefinition("s_cRecv", FieldAttributes.Static | FieldAttributes.Private, intT);
        var fCntAdj  = new FieldDefinition("s_cAdj",  FieldAttributes.Static | FieldAttributes.Private, intT);
        // b11：机制自检计数器——挂在「一定会在启动时执行」的方法上（GTLauncher.Awake/Start）
        var fCntWake = new FieldDefinition("s_cAwake", FieldAttributes.Static | FieldAttributes.Private, intT);
        var fCntStart = new FieldDefinition("s_cStart", FieldAttributes.Static | FieldAttributes.Private, intT);
        t.Fields.Add(fCntAdd); t.Fields.Add(fCntSet); t.Fields.Add(fCntRecv); t.Fields.Add(fCntAdj);
        t.Fields.Add(fCntWake); t.Fields.Add(fCntStart);
        FAdd = fCntAdd; FSet = fCntSet; FRecv = fCntRecv; FWake = fCntWake; FStart = fCntStart;

        var mAppend = new MethodReference("AppendAllText", voidT, fileType) { HasThis = false };
        mAppend.Parameters.Add(new ParameterDefinition(strT));
        mAppend.Parameters.Add(new ParameterDefinition(strT));
        var mConcat2 = new MethodReference("Concat", strT, strT) { HasThis = false };
        mConcat2.Parameters.Add(new ParameterDefinition(strT));
        mConcat2.Parameters.Add(new ParameterDefinition(strT));
        var i32ToStr = new MethodReference("ToString", strT, mod.TypeSystem.Int32) { HasThis = true };
        var i64ToStr = new MethodReference("ToString", strT, mod.TypeSystem.Int64) { HasThis = true };

        // static string I2S(int v) / I8S(long v)
        MethodDefinition MakeToStr(string name, TypeReference argc, MethodReference target)
        {
            var m = new MethodDefinition(name, MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, strT);
            m.Parameters.Add(new ParameterDefinition("v", ParameterAttributes.None, argc));
            var ip = m.Body.GetILProcessor();
            ip.Append(ip.Create(OpCodes.Ldarg_0));
            ip.Append(ip.Create(OpCodes.Call, target));
            ip.Append(ip.Create(OpCodes.Ret));
            t.Methods.Add(m);
            return m;
        }
        var mI2S = MakeToStr("I2S", mod.TypeSystem.Int32, i32ToStr);
        var mI8S = MakeToStr("I8S", mod.TypeSystem.Int64, i64ToStr);
        // b8 诊断第二通道：logcat（UnityEngine.Debug.Log）——静态依据：mygame 的 TypeRef 里已存在
        //   UnityEngine.Debug scope=UnityEngine.CoreModule（Cecil 实测），故复用该 TypeRef，不新增 AssemblyRef。
        //   目的：若 File 通道在本运行时被拦（AppendAllText 解析失败/异常被吞），仍能从 logcat 看到 [P] 行，
        //   从而区分「AddHP 没被调用」与「被调用但写文件失败」。
        var dbgTypeRef = mod.GetTypeReferences().FirstOrDefault(tr => tr.FullName == "UnityEngine.Debug");
        MethodDefinition logD = null;
        if (dbgTypeRef != null) {
            var dbgLog = new MethodReference("Log", voidT, dbgTypeRef) { HasThis = false };
            dbgLog.Parameters.Add(new ParameterDefinition(mod.TypeSystem.Object));
            logD = new MethodDefinition("LogD", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, voidT);
            logD.Parameters.Add(new ParameterDefinition("msg", ParameterAttributes.None, strT));
            t.Methods.Add(logD);
            var dp = logD.Body.GetILProcessor();
            var dRet = dp.Create(OpCodes.Ret);
            var dTry = dp.Create(OpCodes.Ldarg_0);
            dp.Append(dTry);
            dp.Append(dp.Create(OpCodes.Call, dbgLog));
            dp.Append(dp.Create(OpCodes.Leave, dRet));
            var dTryEnd = dp.Create(OpCodes.Nop); dp.Append(dTryEnd);
            var dCatch = dp.Create(OpCodes.Pop); dp.Append(dCatch);
            dp.Append(dp.Create(OpCodes.Leave, dRet));
            var dCatchEnd = dp.Create(OpCodes.Nop); dp.Append(dCatchEnd);
            dp.Append(dRet);
            logD.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            { CatchType = exType, TryStart = dTry, TryEnd = dTryEnd, HandlerStart = dCatch, HandlerEnd = dCatchEnd });
            Console.WriteLine("[cfg] 诊断第二通道已启用：UnityEngine.Debug.Log（logcat 关键字 [P]）");
        } else {
            Console.WriteLine("[cfg] 警告：未找到 UnityEngine.Debug TypeRef，仅文件通道");
        }

        // static void Log(string msg) { try { if (s_logLines < 200) { s_logLines++; File.AppendAllText(path, "[P] "+msg+"\n"); } } catch {} }
        var logM = new MethodDefinition("Log", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, voidT);
        logM.Parameters.Add(new ParameterDefinition("msg", ParameterAttributes.None, strT));
        t.Methods.Add(logM);
        var lp = logM.Body.GetILProcessor();
        var lRet = lp.Create(OpCodes.Ret);
        var lSkip = lp.Create(OpCodes.Nop);
        var lTry = lp.Create(OpCodes.Ldsfld, fLogN);
        lp.Append(lTry);
        lp.Append(lp.Create(OpCodes.Ldc_I4, 200));
        lp.Append(lp.Create(OpCodes.Bge, lSkip));
        lp.Append(lp.Create(OpCodes.Ldsfld, fLogN));
        lp.Append(lp.Create(OpCodes.Ldc_I4_1));
        lp.Append(lp.Create(OpCodes.Add));
        lp.Append(lp.Create(OpCodes.Stsfld, fLogN));
        // b9：logcat 通道放在 200 行上限之内，避免刷屏（两通道同时限流）
        if (logD != null) {
            lp.Append(lp.Create(OpCodes.Ldarg_0));
            lp.Append(lp.Create(OpCodes.Call, logD));
        }
        lp.Append(lp.Create(OpCodes.Ldstr, LogPath));
        lp.Append(lp.Create(OpCodes.Ldstr, "[P] "));
        lp.Append(lp.Create(OpCodes.Ldarg_0));
        lp.Append(lp.Create(OpCodes.Call, mConcat2));
        lp.Append(lp.Create(OpCodes.Ldstr, "\n"));
        lp.Append(lp.Create(OpCodes.Call, mConcat2));
        lp.Append(lp.Create(OpCodes.Call, mAppend));
        lp.Append(lp.Create(OpCodes.Leave, lSkip));
        var lTryEnd = lp.Create(OpCodes.Nop); lp.Append(lTryEnd);
        var lCatch = lp.Create(OpCodes.Pop); lp.Append(lCatch);
        lp.Append(lp.Create(OpCodes.Leave, lSkip));
        var lCatchEnd = lp.Create(OpCodes.Nop); lp.Append(lCatchEnd);
        lp.Append(lSkip);
        lp.Append(lRet);
        logM.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        { CatchType = exType, TryStart = lTry, TryEnd = lTryEnd, HandlerStart = lCatch, HandlerEnd = lCatchEnd });

        // static void Enter()（2s 限流）：区分「AddHP 没被调用」与「被调用了」
        var entM = new MethodDefinition("Enter", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, voidT);
        t.Methods.Add(entM);
        entM.Body.InitLocals = true;
        entM.Body.Variables.Add(new VariableDefinition(mod.TypeSystem.Int32));
        var ep = entM.Body.GetILProcessor();
        var eRet = ep.Create(OpCodes.Ret);
        ep.Append(ep.Create(OpCodes.Call, tick));
        ep.Append(ep.Create(OpCodes.Stloc_0));
        ep.Append(ep.Create(OpCodes.Ldloc_0));
        ep.Append(ep.Create(OpCodes.Ldsfld, fTickEnt));
        ep.Append(ep.Create(OpCodes.Sub));
        ep.Append(ep.Create(OpCodes.Ldc_I4, 2000));
        ep.Append(ep.Create(OpCodes.Blt, eRet));
        ep.Append(ep.Create(OpCodes.Ldloc_0));
        ep.Append(ep.Create(OpCodes.Stsfld, fTickEnt));
        ep.Append(ep.Create(OpCodes.Ldstr, "enter AddHP"));
        ep.Append(ep.Create(OpCodes.Call, logM));
        ep.Append(eRet);

        // static void LogAdjust(long v)（2s 限流）：value / god / kill
        var adjLogM = new MethodDefinition("LogAdjust", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, voidT);
        adjLogM.Parameters.Add(new ParameterDefinition("v", ParameterAttributes.None, mod.TypeSystem.Int64));
        t.Methods.Add(adjLogM);
        adjLogM.Body.InitLocals = true;
        adjLogM.Body.Variables.Add(new VariableDefinition(mod.TypeSystem.Int32));
        var gp = adjLogM.Body.GetILProcessor();
        var gRet = gp.Create(OpCodes.Ret);
        gp.Append(gp.Create(OpCodes.Call, tick));
        gp.Append(gp.Create(OpCodes.Stloc_0));
        gp.Append(gp.Create(OpCodes.Ldloc_0));
        gp.Append(gp.Create(OpCodes.Ldsfld, fTickAdj));
        gp.Append(gp.Create(OpCodes.Sub));
        gp.Append(gp.Create(OpCodes.Ldc_I4, 2000));
        gp.Append(gp.Create(OpCodes.Blt, gRet));
        gp.Append(gp.Create(OpCodes.Ldloc_0));
        gp.Append(gp.Create(OpCodes.Stsfld, fTickAdj));
        gp.Append(gp.Create(OpCodes.Ldstr, "adjust v="));
        gp.Append(gp.Create(OpCodes.Ldarg_0));
        gp.Append(gp.Create(OpCodes.Call, mI8S));
        gp.Append(gp.Create(OpCodes.Call, mConcat2));
        gp.Append(gp.Create(OpCodes.Ldstr, " god="));
        gp.Append(gp.Create(OpCodes.Ldsfld, fGod));
        gp.Append(gp.Create(OpCodes.Call, mI2S));
        gp.Append(gp.Create(OpCodes.Call, mConcat2));
        gp.Append(gp.Create(OpCodes.Ldstr, " kill="));
        gp.Append(gp.Create(OpCodes.Ldsfld, fKill));
        gp.Append(gp.Create(OpCodes.Call, mI2S));
        gp.Append(gp.Create(OpCodes.Call, mConcat2));
        gp.Append(gp.Create(OpCodes.Call, logM));
        gp.Append(gRet);

        // 静态构造：[P] cctor
        var cctor = new MethodDefinition(".cctor",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig
            | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, voidT);
        t.Methods.Add(cctor);
        var cp = cctor.Body.GetILProcessor();
        cp.Append(cp.Create(OpCodes.Ldstr, "cctor"));
        cp.Append(cp.Create(OpCodes.Call, logM));
        cp.Append(cp.Create(OpCodes.Ret));

        // ---- AllOff(): 两个开关都关 ⇒ true（短路，零 JInt 操作）----
        var allOff = new MethodDefinition("AllOff",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, boolT);
        t.Methods.Add(allOff);
        var ao = allOff.Body.GetILProcessor();
        var aoFalse = ao.Create(OpCodes.Ldc_I4_0);
        var aoTrue  = ao.Create(OpCodes.Ldc_I4_1);
        ao.Append(ao.Create(OpCodes.Call, refresh));
        ao.Append(ao.Create(OpCodes.Ldsfld, fGod));
        ao.Append(ao.Create(OpCodes.Brtrue, aoFalse));
        ao.Append(ao.Create(OpCodes.Ldsfld, fKill));
        ao.Append(ao.Create(OpCodes.Brtrue, aoFalse));
        ao.Append(aoTrue);
        ao.Append(ao.Create(OpCodes.Ret));
        ao.Append(aoFalse);
        ao.Append(ao.Create(OpCodes.Ret));

        // ---- AdjustValue(Life self, JInt value, out JInt adjusted) -> bool ----
        //   单一时段判定（互斥）：god && v<0 && Camp==FRIEND  → adjusted = 0
        //                          kill && v<0 && Camp==ENEMY  → adjusted = -(int)_curHP - 1
        //   失败/不满足 → 返回 false，调用方**保留原 value**（绝不写 0 覆盖）
        var mImpI32 = jint.Methods.FirstOrDefault(m => m.Name == "op_Implicit" && m.Parameters.Count == 1
                        && m.Parameters[0].ParameterType.FullName == "System.Int32"
                        && m.ReturnType.FullName == jint.FullName)
                   ?? throw new Exception("JInt::op_Implicit(Int32) 未找到");
        var adjust = new MethodDefinition("AdjustValue",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, boolT);
        adjust.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, life));
        adjust.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, jint));
        adjust.Parameters.Add(new ParameterDefinition("adjusted", ParameterAttributes.Out, new ByReferenceType(jint)));
        adjust.Body.InitLocals = true;
        adjust.Body.Variables.Add(new VariableDefinition(mod.TypeSystem.Int64));   // [0] long v
        adjust.Body.Variables.Add(new VariableDefinition(boolT));                   // [1] bool ok
        t.Methods.Add(adjust);
        var a = adjust.Body.GetILProcessor();
        var aEnd = a.Create(OpCodes.Ldloc_1);
        a.Append(a.Create(OpCodes.Ldc_I4_0));
        a.Append(a.Create(OpCodes.Stloc_1));
        var aTry = a.Create(OpCodes.Call, refresh);
        a.Append(aTry);
        // b7 诊断：value / god / kill（2s 限流）
        a.Append(a.Create(OpCodes.Ldarga_S, adjust.Parameters[1]));
        a.Append(a.Create(OpCodes.Call, mod.ImportReference(getValue)));
        a.Append(a.Create(OpCodes.Call, adjLogM));

        // god 分支
        var notGod = a.Create(OpCodes.Nop);
        a.Append(a.Create(OpCodes.Ldsfld, fGod));
        a.Append(a.Create(OpCodes.Brfalse, notGod));
        a.Append(a.Create(OpCodes.Ldarga_S, adjust.Parameters[1]));
        a.Append(a.Create(OpCodes.Call, mod.ImportReference(getValue)));
        a.Append(a.Create(OpCodes.Stloc_0));
        a.Append(a.Create(OpCodes.Ldloc_0));
        a.Append(a.Create(OpCodes.Ldc_I8, 0L));
        a.Append(a.Create(OpCodes.Bge, notGod));                  // 治疗/零值 → 看 kill 分支后原样返回
        a.Append(a.Create(OpCodes.Ldarg_0));
        a.Append(a.Create(OpCodes.Call, campOf));
        a.Append(a.Create(OpCodes.Ldc_I4, campFriend));
        a.Append(a.Create(OpCodes.Bne_Un, notGod));               // 只保护己方
        a.Append(a.Create(OpCodes.Ldarg_2));                      // out JInt&
        a.Append(a.Create(OpCodes.Ldc_I4_0));
        a.Append(a.Create(OpCodes.Call, mod.ImportReference(mImpI32)));
        a.Append(a.Create(OpCodes.Stobj, mod.ImportReference(jint)));
        a.Append(a.Create(OpCodes.Ldc_I4_1));
        a.Append(a.Create(OpCodes.Stloc_1));
        // b7 诊断：god 生效（camp=FRIEND）
        a.Append(a.Create(OpCodes.Ldstr, "changed by god (camp=FRIEND) -> value=0"));
        a.Append(a.Create(OpCodes.Call, logM));
        a.Append(a.Create(OpCodes.Leave, aEnd));
        a.Append(notGod);

        // kill 分支
        a.Append(a.Create(OpCodes.Ldsfld, fKill));
        a.Append(a.Create(OpCodes.Brfalse, aEnd));
        a.Append(a.Create(OpCodes.Ldarga_S, adjust.Parameters[1]));
        a.Append(a.Create(OpCodes.Call, mod.ImportReference(getValue)));
        a.Append(a.Create(OpCodes.Stloc_0));
        a.Append(a.Create(OpCodes.Ldloc_0));
        a.Append(a.Create(OpCodes.Ldc_I8, 0L));
        a.Append(a.Create(OpCodes.Bge, aEnd));                    // 治疗/零值 → 原逻辑
        a.Append(a.Create(OpCodes.Ldarg_0));
        a.Append(a.Create(OpCodes.Call, campOf));
        a.Append(a.Create(OpCodes.Brtrue, aEnd));                 // 非 ENEMY → 不改写
        a.Append(a.Create(OpCodes.Ldarg_2));
        a.Append(a.Create(OpCodes.Ldarg_0));
        a.Append(a.Create(OpCodes.Ldflda, mod.ImportReference(curHp)));
        a.Append(a.Create(OpCodes.Call, mod.ImportReference(getValue)));
        a.Append(a.Create(OpCodes.Neg));
        a.Append(a.Create(OpCodes.Ldc_I8, 1L));
        a.Append(a.Create(OpCodes.Sub));
        a.Append(a.Create(OpCodes.Newobj, mod.ImportReference(ctorLong)));
        a.Append(a.Create(OpCodes.Stobj, mod.ImportReference(jint)));
        a.Append(a.Create(OpCodes.Ldc_I4_1));
        a.Append(a.Create(OpCodes.Stloc_1));
        // b7 诊断：kill 生效（camp=ENEMY）
        a.Append(a.Create(OpCodes.Ldstr, "changed by kill (camp=ENEMY) -> value=-(curHP+1)"));
        a.Append(a.Create(OpCodes.Call, logM));
        var aTryEnd = a.Create(OpCodes.Leave, aEnd); a.Append(aTryEnd);
        var aCatch = a.Create(OpCodes.Pop); a.Append(aCatch);
        a.Append(a.Create(OpCodes.Ldc_I4_0));
        a.Append(a.Create(OpCodes.Stloc_1));
        a.Append(a.Create(OpCodes.Leave, aEnd));
        var aCatchEnd = a.Create(OpCodes.Nop); a.Append(aCatchEnd);
        a.Append(aEnd);
        a.Append(a.Create(OpCodes.Ret));
        adjust.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        { CatchType = exType, TryStart = aTry, TryEnd = aTryEnd, HandlerStart = aCatch, HandlerEnd = aCatchEnd });

        Console.WriteLine($"[cfg] Starry.MxzzPatch 已注入（1s 缓存, {CfgPath}；键 god / kill）");
        return t;
    }

    public static void Gate(ModuleDefinition mm, List<string> fails)
    {
        void A(string name, bool ok, string detail)
        {
            Console.WriteLine($"[assert] {(ok ? "OK  " : "FAIL")} {name}: {detail}");
            if (!ok) fails.Add($"{name}: {detail}");
        }
        const string tCfg = "Starry.StarryConfig";
        const string tHlp = "Starry.MxzzPatch";

        var cfg = mm.GetType(tCfg);
        if (cfg == null) A("StarryConfig", false, "注入类型缺失");
        else
        {
            var names = cfg.Methods.Select(x => x.Name).ToList();
            bool ok = names.Contains("Get") && names.Contains("GetBool") && names.Contains("GetInt");
            bool path = cfg.Methods.Where(x => x.HasBody)
                            .SelectMany(x => x.Body.Instructions.AsEnumerable())
                            .Any(i => i.OpCode == OpCodes.Ldstr && ((string)i.Operand ?? "").Contains("/files/"));
            A("StarryConfig", ok && path, $"方法={string.Join(",", names)}；配置路径已内联={path}");
        }
        var hlp = mm.GetType(tHlp);
        if (hlp == null) { A("MxzzPatch", false, "注入类型缺失"); return; }
        var hNames = hlp.Methods.Select(x => x.Name).ToList();
        A("MxzzPatch.方法齐备", hNames.Contains("Refresh") && hNames.Contains("CampOf")
            && hNames.Contains("AllOff") && hNames.Contains("AdjustValue"),
          string.Join(",", hNames));

        // 1. AllOff 短路判定必须零 JInt 操作
        var mAllOff = hlp.Methods.First(x => x.Name == "AllOff");
        bool touchesJint = mAllOff.Body.Instructions.Any(i =>
            (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) && i.Operand is MethodReference mr &&
            (mr.Name == "get_Value" || mr.Name == "op_Implicit" || mr.Name == "op_Addition"));
        A("AllOff.零JInt", !touchesJint, $"无 get_Value/op_Implicit = {!touchesJint}");

        // 2. AdjustValue 单出口 + out 参数 + 两个阵营常量都出现过
        var mAdj = hlp.Methods.First(x => x.Name == "AdjustValue");
        A("AdjustValue.签名", mAdj.ReturnType.FullName == "System.Boolean" && mAdj.Parameters.Count == 3
            && mAdj.Parameters[2].ParameterType.IsByReference,
          $"{mAdj.ReturnType.Name}({string.Join(",", mAdj.Parameters.Select(p => p.ParameterType.Name))})");
        int rets = mAdj.Body.Instructions.Count(i => i.OpCode == OpCodes.Ret);
        A("AdjustValue.单出口", rets == 1, $"Ret 数量={rets}（期望 1）");
        var ldci4 = mAdj.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldc_I4)
                                          .Select(i => (int)i.Operand).ToList();
        bool hasFriend = ldci4.Contains(1), hasEnemyZero = mAdj.Body.Instructions.Any(i =>
            i.OpCode == OpCodes.Brtrue && i.Previous?.OpCode == OpCodes.Call &&
            (i.Previous.Operand as MethodReference)?.Name == "CampOf");
        A("AdjustValue.阵营语义", hasFriend && hasEnemyZero,
          $"FRIEND 常量出现={hasFriend}；ENEMY 判定(CampOf!=0 即跳过)={hasEnemyZero}");

        // 3. AddHP 前缀形状
        var life = mm.GetType("Battle.Life");
        var addHp = life?.Methods.FirstOrDefault(x => x.Name == "AddHP" && x.Parameters.Count == 3);
        if (addHp == null) { A("AddHP", false, "目标方法缺失"); return; }
        var ins = addHp.Body.Instructions.ToList();
        A("AddHP.前缀条数", ins.Count == 259, $"IL 总数={ins.Count}（期望 245+14=259：4 条 b10 计数 + 1 条 b7 诊断 Enter + 9 条语义前缀）");
        bool enterOk = ins[4].OpCode == OpCodes.Call && (ins[4].Operand as MethodReference)?.Name == "Enter";
        A("AddHP.诊断入口", enterOk, $"ins[4]={(ins[4].Operand as MethodReference)?.Name}（b7 诊断，期望 Enter）");
        bool headOk = ins[5].OpCode == OpCodes.Call && (ins[5].Operand as MethodReference)?.Name == "AllOff";
        bool shortCircuit = headOk && (ins[6].OpCode == OpCodes.Brtrue || ins[6].OpCode == OpCodes.Brtrue_S)
            && ins[6].Operand is Instruction t1 && t1.Offset > ins[13].Offset;
        A("AddHP.全关短路", shortCircuit,
          $"ins[5]=AllOff，ins[6]={ins[6].OpCode.Name} → IL_{(ins[6].Operand as Instruction)?.Offset.ToString("X4")}（须跳出注入块）");
        bool adjOk = ins[10].OpCode == OpCodes.Call && (ins[10].Operand as MethodReference)?.Name == "AdjustValue"
            && (ins[11].OpCode == OpCodes.Brfalse || ins[11].OpCode == OpCodes.Brfalse_S);
        A("AddHP.改写即落回原体", adjOk,
          $"ins[10]={(ins[10].Operand as MethodReference)?.Name}，ins[11]={ins[11].OpCode.Name}（不得为 ret）");
        var starg = ins.FirstOrDefault(i => i.OpCode == OpCodes.Starg || i.OpCode == OpCodes.Starg_S);
        var wp = starg?.Operand as ParameterDefinition;
        A("AddHP.参数对齐", wp != null && wp.Index == 0 && wp.Name == "value"
            && wp.ParameterType.FullName == "JEngine.AntiCheat.JInt",
          $"starg 落点={(wp == null ? "非参数" : $"Parameters[{wp.Index}]={wp.Name}({wp.ParameterType.Name})")}");
        var ldloca = ins.FirstOrDefault(i => i.OpCode == OpCodes.Ldloca || i.OpCode == OpCodes.Ldloca_S);
        var lv = ldloca?.Operand as VariableDefinition;
        A("AddHP.局部类型", lv != null && lv.VariableType.FullName == "JEngine.AntiCheat.JInt",
          $"ldloca → V{lv?.Index}({lv?.VariableType.Name})");
        A("AddHP.栈额", addHp.Body.MaxStackSize >= 8 && addHp.Body.InitLocals,
          $"maxstack={addHp.Body.MaxStackSize} initlocals={addHp.Body.InitLocals}");
        // 前缀区不得含 ret（早退会被跳过记录，见实现记录 §三）
        int retInPrefix = ins.Take(10).Count(i => i.OpCode == OpCodes.Ret);
        A("AddHP.前缀无早退", retInPrefix == 0, $"前缀内 Ret={retInPrefix}（期望 0）");

        // 4. 引用卫生：不得引入 System.Private.CoreLib
        var coreLib = mm.AssemblyReferences.Any(x => x.Name == "System.Private.CoreLib");
        A("引用卫生", !coreLib,
          coreLib ? "产物新增 System.Private.CoreLib AssemblyRef（Unity 只有 mscorlib）" : "无 CoreLib 引用");
    }

    // ---------------- 通用 helper ----------------
    // ★ mscorlib 一律取**目标程序集自带的引用**，禁止 typeof(...) 导入宿主 net8 类型：
    //   旧 b8 用 typeof 后产物新增 System.Private.CoreLib AssemblyRef，而 Unity 侧只有 mscorlib
    //   ⇒ 运行期类型解析失败（旧 b5/b7「双方无伤」事故链的一环）。
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

    static void P(Action f, string name)
    {
        f();
        nPatch++;
    }

    // 引用程序集搜索目录（写盘期解析用）。顺序：同目录 refs → 环境变量 → 本仓 dll/DummyDll。
    static IEnumerable<string> RefDirs(string srcPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(srcPath));
        yield return Path.Combine(dir, "refs");
        var env = Environment.GetEnvironmentVariable("MXZZ_REF_DIR");
        if (!string.IsNullOrEmpty(env)) yield return env;
        var root = Environment.GetEnvironmentVariable("NIXIANG_ROOT");
        if (!string.IsNullOrEmpty(root))
        {
            yield return Path.Combine(root, "13_scratch", "冒险战争", "dll");
            yield return Path.Combine(root, "06_dumped", "冒险战争", "1.1.036_il2cpp", "dump", "DummyDll");
        }
    }

    // ---------------- StarryConfig 注入 ----------------
    // 文件：/data/user/0/com.mxzz.zbml/files/starry_config.ini，每行 key=value，2 秒缓存
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
        var t0 = Instruction.Create(OpCodes.Ldstr, CfgPath);
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

        Console.WriteLine($"[cfg] Starry.StarryConfig 已注入（2s 缓存, {CfgPath}）");
    }

}
