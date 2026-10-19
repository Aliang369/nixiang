using Mono.Cecil;
using Mono.Cecil.Cil;

// 【绿梦：时空之声】Assembly-ACT.dll 注入器
//
// 设计（已冻结，见 10_docs/绿梦/挂点候选.md §6/§7 与本会话任务书）：
//   ① 游戏侧只打 IL 补丁 + 读状态文件；逻辑在独立小程序集 StarryLvmeng.dll 里，运行时由
//      Hello::Run() prefix 的引导代码用 File.ReadAllBytes + Assembly.Load 从文件路径加载。
//   ② 注入的 IL 只做「加载 + 转发调用」；6 处注入点全部落在已有方法上，不新增类型。
//
// 6 处注入点：
//   1 Hello::Run()                                       prefix   引导加载 mod dll → Init() → 缓存 Tick 委托
//   2 ACT.Battle.SystemTime::FixedUpdate()               postfix  Hello.s_starryTick?.Invoke()
//   3 ACT.Battle.CompAbility::ServerSetHP(Int32)         postfix  Starry.Mod.OnSetHP(this) → IL 内判负后 stfld m_HP
//   4 ACT.Battle.SystemAbility::ServerResolveDamage(...) prefix   Starry.Mod.OnResolveDamage(attackData)
//   5 ACT.Battle.Configs.ActionSetJumpCount::Do(...)     postfix  Starry.Mod.OnSetJumpCount(memory)
//   6 ACT.Battle.CompMotion::RecoverMotionCount()        postfix  Starry.Mod.OnRecoverMotion(this)
//
// 阶段 1 只注入 1/2（先把加载管道跑通并验收）；阶段 2 追加 3/4/5/6。
internal static class Patcher
{
    public const string MOD_ASM = "StarryLvmeng";
    public static readonly string[] MOD_DIRS =
    {
        "/storage/emulated/0/Android/media/com.zhuziplay.lvmengnew.op",
        "/storage/emulated/0/Android/data/com.zhuziplay.lvmengnew.op/files",
        "/data/data/com.zhuziplay.lvmengnew.op/files",
    };

    /// <summary>引导期证据文件。logcat 不可靠：游戏用 LuminApplication 的日志器，
    /// 且 `LogEnabled` 为 false 时**连 LogError 都被吞掉**（`LuminApplication.cs:68`），
    /// 所以引导链的证据一律**落文件**，用 root 读回。见 侦察笔记 §8。</summary>
    public const string LOG_PATH = "/data/data/com.zhuziplay.lvmengnew.op/files/starry_loader.log";

    // ------------------------------------------------------------------ 零填充
    public static void PadTo(string path, long size)
    {
        var fi = new FileInfo(path);
        if (fi.Length > size) throw new InvalidOperationException($"文件 {fi.Length} B 已超过目标 {size} B，无法零填充");
        if (fi.Length == size) return;
        using var fs = new FileStream(path, FileMode.Append, FileAccess.Write);
        fs.Write(new byte[size - fi.Length]);
    }

    // ------------------------------------------------------------------ PE 版图还原
    //
    // 为什么需要：Cecil 无损重写后 PE 变小（元数据压缩），它自己的节表末尾 = 14420992，
    // 而**原始** dll 的节表末尾恰好 = 14529536（= 文件长度，SerializedFile 对象表的
    // byteSize 就按它算）。若只在文件尾部补零，节表末尾仍是 14420992，落到设备后
    // `install_dll_bundle.sh` 闸门 ①b 的 `locate-pe`（按节表 praw+sraw 求和）会读出
    // 错尺寸而拒绝落地，且等于把「等长」建立在文件长度而非 PE 结构上。
    // ⇒ 这里把补丁后的 PE **按原始节表版图重排**：每个节的 PointerToRawData/SizeOfRawData
    //   恢复成原始值，多出来的空间全部零填充（原始 .text 本来就有 sraw > vsize 的余量）。
    //   节的虚拟地址（RVA/vsize）完全不动，因此节内容与重定位不受影响。
    sealed class PeSection
    {
        public string Name; public long HdrOff, Praw, Sraw, Vsize;
        public override string ToString() => $"{Name} praw={Praw} sraw={Sraw} vsize={Vsize}";
    }

    static List<PeSection> ReadSections(byte[] d)
    {
        int pe = BitConverter.ToInt32(d, 0x3C);
        if (d[pe] != 'P' || d[pe + 1] != 'E') throw new InvalidOperationException("不是 PE");
        int nsec = BitConverter.ToInt16(d, pe + 6);
        int optsz = BitConverter.ToInt16(d, pe + 20);
        int secoff = pe + 24 + optsz;
        var list = new List<PeSection>();
        for (int i = 0; i < nsec; i++)
        {
            int so = secoff + 40 * i;
            list.Add(new PeSection
            {
                Name = System.Text.Encoding.ASCII.GetString(d, so, 8).TrimEnd('\0'),
                HdrOff = so,
                Vsize = BitConverter.ToUInt32(d, so + 8),
                Sraw = BitConverter.ToUInt32(d, so + 16),
                Praw = BitConverter.ToUInt32(d, so + 20),
            });
        }
        return list;
    }

    public static void RestorePeLayout(string original, string patched, long target)
    {
        var od = File.ReadAllBytes(original);
        var pd = File.ReadAllBytes(patched);
        var os = ReadSections(od);
        var ps = ReadSections(pd);
        if (os.Count != ps.Count) throw new InvalidOperationException($"节数不同：原始 {os.Count} vs 补丁 {ps.Count}");
        for (int i = 0; i < os.Count; i++)
            if (os[i].Name != ps[i].Name)
                throw new InvalidOperationException($"第 {i} 节名不同：{os[i].Name} vs {ps[i].Name}");

        long total = os[os.Count - 1].Praw + os[os.Count - 1].Sraw;
        if (total != target) throw new InvalidOperationException($"原始 PE 节表末尾 {total} != 目标 {target}");
        if (pd.Length > total) throw new InvalidOperationException($"补丁 PE {pd.Length} B 已超过目标 {total} B");

        var nd = new byte[total];
        long hdrEnd = Math.Min(pd.Length, Math.Max(ps[0].Praw, ps[ps.Count - 1].HdrOff + 40));
        Buffer.BlockCopy(pd, 0, nd, 0, (int)hdrEnd);
        for (int i = 0; i < os.Count; i++)
        {
            var o = os[i]; var p = ps[i];
            if (i > 0 && (o.Praw < ps[i - 1].Praw + ps[i - 1].Sraw))
                throw new InvalidOperationException($"节 {o.Name} 会与前节在原始版图里重叠");
            if (p.Praw + p.Sraw > pd.Length) throw new InvalidOperationException($"节 {p.Name} 越界");
            Buffer.BlockCopy(pd, (int)p.Praw, nd, (int)o.Praw, (int)p.Sraw);
            BitConverter.GetBytes((uint)o.Praw).CopyTo(nd, p.HdrOff + 20);
            BitConverter.GetBytes((uint)o.Sraw).CopyTo(nd, p.HdrOff + 16);
        }
        File.WriteAllBytes(patched, nd);
        Console.WriteLine($"[pe] 版图还原：{pd.Length} → {nd.Length} B；节表末尾 {total} B（原始几何）");
        foreach (var o in os) Console.WriteLine($"     {o}");
    }

    // ------------------------------------------------------------------ 元数据工具
    static TypeReference TypeRef(ModuleDefinition mod, string ns, string name, string asmName = "mscorlib")
    {
        var full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        foreach (var tr in mod.GetTypeReferences())
            if (tr.FullName == full) return tr;
        var scope = mod.AssemblyReferences.FirstOrDefault(x => x.Name == asmName);
        if (scope == null)
        {
            scope = new AssemblyNameReference(asmName, new Version(1, 0, 0, 0));
            mod.AssemblyReferences.Add(scope);
        }
        return new TypeReference(ns, name, mod, scope);
    }

    static MethodReference MRef(TypeReference decl, string name, TypeReference ret, bool hasThis, params TypeReference[] ps)
    {
        var m = new MethodReference(name, ret, decl) { HasThis = hasThis, ExplicitThis = false };
        foreach (var p in ps) m.Parameters.Add(new ParameterDefinition(p));
        return m;
    }

    static TypeDefinition Need(ModuleDefinition mod, string fullName)
        => mod.GetType(fullName) ?? throw new InvalidOperationException($"找不到类型 {fullName}");

    static MethodDefinition NeedMethod(TypeDefinition t, string name, int pcount)
    {
        var m = t.Methods.FirstOrDefault(x => x.Name == name && x.Parameters.Count == pcount)
            ?? throw new InvalidOperationException($"找不到方法 {t.FullName}::{name}/{pcount}");
        if (!m.HasBody) throw new InvalidOperationException($"{t.FullName}::{name} 无方法体");
        if (m.Body.ExceptionHandlers.Count != 0)
            throw new InvalidOperationException($"{t.FullName}::{name} 含异常处理器，本注入器不支持（需人工确认边界）");
        return m;
    }

    // ------------------------------------------------------------------ 顺序发射器
    sealed class Seq
    {
        readonly ILProcessor p;
        public readonly List<Instruction> List = new();
        public Seq(ILProcessor p) { this.p = p; }

        public Instruction Add(OpCode op) { var i = p.Create(op); List.Add(i); return i; }
        public Instruction Add(OpCode op, Instruction t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(OpCode op, MethodReference t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(OpCode op, FieldReference t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(OpCode op, TypeReference t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(OpCode op, VariableDefinition t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(OpCode op, string t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(OpCode op, sbyte t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(OpCode op, ParameterDefinition t) { var i = p.Create(op, t); List.Add(i); return i; }
        public Instruction Add(Instruction i) { List.Add(i); return i; }

        /// <summary>
        /// 把本序列插到 target 之前。retarget=true（postfix）：所有以 target 为分支目标的分支改指序列首指令，
        /// 保证每个出口都跑 postfix；retarget=false（prefix）：不动分支，避免回边重复触发。
        /// </summary>
        public void InsertBefore(MethodDefinition m, Instruction target, bool retarget = true)
        {
            foreach (var eh in m.Body.ExceptionHandlers)
                if (ReferenceEquals(eh.TryStart, target) || ReferenceEquals(eh.HandlerStart, target) ||
                    ReferenceEquals(eh.FilterStart, target))
                    throw new InvalidOperationException($"{m.FullName}: 目标指令是异常处理器边界，拒绝改写");
            foreach (var i in List) p.InsertBefore(target, i);
            if (!retarget) return;
            var first = List[0];
            foreach (var ins in m.Body.Instructions)
            {
                if (ReferenceEquals(ins, target)) continue;
                if (ReferenceEquals(ins.Operand, target)) ins.Operand = first;
                else if (ins.Operand is Instruction[] arr)
                    for (int k = 0; k < arr.Length; k++) if (ReferenceEquals(arr[k], target)) arr[k] = first;
            }
        }
    }

    // ------------------------------------------------------------------ 引导代码（注入进 Hello）
    static void AddBootstrap(ModuleDefinition mod, TypeDefinition hello)
    {
        if (hello.Fields.Any(f => f.Name == "s_starryTick"))
            throw new InvalidOperationException("Hello.s_starryTick 已存在（重复注入？）");
        // 字段存 MethodInfo（不是 Action）：实测 `Delegate::CreateDelegate` 在游戏的 AOT 里
        // **0 处使用 ⇒ 已被裁剪**，调用会抛 MissingMethodException。改成每帧用
        // MethodBase::Invoke(null,null) 反射调用 —— 该方法游戏自己在 InitDll::LoadOver 里用过，
        // 是**已被证实存在**的（见 侦察笔记 §8 与本次 MissingMethodException 定位）。
        var miT2 = TypeRef(mod, "System.Reflection", "MethodInfo");
        var fTick = new FieldDefinition("s_starryTick", FieldAttributes.Public | FieldAttributes.Static, miT2);
        hello.Fields.Add(fTick);

        var fileLog = new MethodDefinition("StarryFileLog",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, mod.TypeSystem.Void);
        fileLog.Parameters.Add(new ParameterDefinition("line", ParameterAttributes.None, mod.TypeSystem.String));
        hello.Methods.Add(fileLog);
        EmitFileLog(mod, fileLog);

        var tryLoad = new MethodDefinition("StarryTryLoad",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, mod.TypeSystem.Boolean);
        tryLoad.Parameters.Add(new ParameterDefinition("path", ParameterAttributes.None, mod.TypeSystem.String));
        hello.Methods.Add(tryLoad);
        EmitTryLoad(mod, tryLoad, fTick, fileLog);

        var boot = new MethodDefinition("StarryBootstrap",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, mod.TypeSystem.Void);
        hello.Methods.Add(boot);
        EmitBootstrap(mod, boot, tryLoad, fileLog);
    }

    static void EmitBootstrap(ModuleDefinition mod, MethodDefinition boot, MethodDefinition tryLoad, MethodDefinition fileLog)
    {
        var p = boot.Body.GetILProcessor();
        boot.Body.InitLocals = true;
        var exV = new VariableDefinition(TypeRef(mod, "System", "Exception"));
        boot.Body.Variables.Add(exV);

        // 记录器用 LogError：游戏的 LuminLogHandler 只转发 Error/Exception，
        // Debug.Log / Debug.LogWarning 会被静默吞掉（实测踩坑，见 Mod.cs 头注释）。
        var logWarn = MRef(TypeRef(mod, "UnityEngine", "Debug", "UnityEngine.CoreModule"),
            "LogError", mod.TypeSystem.Void, false, mod.TypeSystem.Object);
        var concat = MRef(TypeRef(mod, "System", "String"), "Concat",
            mod.TypeSystem.String, false, mod.TypeSystem.String, mod.TypeSystem.String);

        var L_ok = p.Create(OpCodes.Nop);            // try 内：探测成功
        var L_end = p.Create(OpCodes.Nop);           // try/catch 之外
        var L_tryEnd = p.Create(OpCodes.Nop);
        var L_catch = p.Create(OpCodes.Stloc, exV);

        var tryStart = p.Create(OpCodes.Nop);
        p.Append(tryStart);
        // ★ 第一条证据：只要这行落盘，就证明 Hello::Run 的 prefix **确实执行了**
        p.Append(p.Create(OpCodes.Ldstr, "[boot] enter"));
        p.Append(p.Create(OpCodes.Call, fileLog));
        foreach (var d in MOD_DIRS)
        {
            p.Append(p.Create(OpCodes.Ldstr, "[boot] try: " + d));
            p.Append(p.Create(OpCodes.Call, fileLog));
            p.Append(p.Create(OpCodes.Ldstr, d + "/starry_lvmeng.dll"));
            p.Append(p.Create(OpCodes.Call, tryLoad));
            p.Append(p.Create(OpCodes.Brtrue, L_ok));
        }
        p.Append(p.Create(OpCodes.Ldstr, "[StarryLvmeng] mod dll not found in any candidate dir"));
        p.Append(p.Create(OpCodes.Call, logWarn));
        p.Append(p.Create(OpCodes.Ldstr, "[boot] not found in any dir"));
        p.Append(p.Create(OpCodes.Call, fileLog));
        p.Append(p.Create(OpCodes.Leave, L_end));
        p.Append(L_ok);
        p.Append(p.Create(OpCodes.Leave, L_end));
        p.Append(L_tryEnd);

        p.Append(L_catch);
        p.Append(p.Create(OpCodes.Ldstr, "[StarryLvmeng] bootstrap failed: "));
        p.Append(p.Create(OpCodes.Ldloc, exV));
        p.Append(p.Create(OpCodes.Callvirt, MRef(TypeRef(mod, "System", "Exception"), "get_Message",
            mod.TypeSystem.String, true)));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, logWarn));
        p.Append(p.Create(OpCodes.Ldstr, "[boot] exc: "));
        p.Append(p.Create(OpCodes.Ldloc, exV));
        p.Append(p.Create(OpCodes.Callvirt, MRef(TypeRef(mod, "System", "Object"), "ToString",
            mod.TypeSystem.String, true)));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, fileLog));
        p.Append(p.Create(OpCodes.Leave, L_end));

        p.Append(L_end);
        p.Append(p.Create(OpCodes.Ret));

        boot.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = TypeRef(mod, "System", "Exception"),
            TryStart = tryStart,
            TryEnd = L_tryEnd,
            HandlerStart = L_catch,
            HandlerEnd = L_end,
        });
    }

    /// <summary>把一行证据追加进 LOG_PATH（读旧文 + 拼接 + 整写）。
    /// 只用游戏自己也在用的 BCL：File.Exists / File.ReadAllText(string) /
    /// File.WriteAllText(string,string,Encoding) + Encoding.UTF8（AotCrypto.cs:22 同款）。
    /// 全程 try/catch 吞异常，绝不因"写日志失败"影响引导。</summary>
    static void EmitFileLog(ModuleDefinition mod, MethodDefinition m)
    {
        var p = m.Body.GetILProcessor();
        m.Body.InitLocals = true;
        var fileT = TypeRef(mod, "System.IO", "File");
        var encT = TypeRef(mod, "System.Text", "Encoding");
        var strT = mod.TypeSystem.String;
        var excT = TypeRef(mod, "System", "Exception");
        var Vold = new VariableDefinition(strT); m.Body.Variables.Add(Vold);

        var exists = MRef(fileT, "Exists", mod.TypeSystem.Boolean, false, strT);
        var readAll = MRef(fileT, "ReadAllText", strT, false, strT);
        var writeAll = MRef(fileT, "WriteAllText", mod.TypeSystem.Void, false, strT, strT, encT);
        var getUtf8 = MRef(encT, "get_UTF8", encT, false);
        var concat = MRef(strT, "Concat", strT, false, strT, strT);

        var L_empty = p.Create(OpCodes.Nop);
        var L_join = p.Create(OpCodes.Nop);
        var L_end = p.Create(OpCodes.Nop);
        var L_tryEnd = p.Create(OpCodes.Nop);
        var L_catch = p.Create(OpCodes.Pop);
        var tryStart = p.Create(OpCodes.Nop);

        p.Append(tryStart);
        p.Append(p.Create(OpCodes.Ldstr, LOG_PATH));
        p.Append(p.Create(OpCodes.Call, exists));
        p.Append(p.Create(OpCodes.Brfalse, L_empty));
        p.Append(p.Create(OpCodes.Ldstr, LOG_PATH));
        p.Append(p.Create(OpCodes.Call, readAll));
        p.Append(p.Create(OpCodes.Stloc, Vold));
        p.Append(p.Create(OpCodes.Br, L_join));
        p.Append(L_empty);
        p.Append(p.Create(OpCodes.Ldstr, ""));
        p.Append(p.Create(OpCodes.Stloc, Vold));
        p.Append(L_join);
        p.Append(p.Create(OpCodes.Ldstr, LOG_PATH));
        p.Append(p.Create(OpCodes.Ldloc, Vold));
        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Ldstr, "\n"));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, getUtf8));
        p.Append(p.Create(OpCodes.Call, writeAll));
        p.Append(p.Create(OpCodes.Leave, L_end));
        p.Append(L_tryEnd);
        p.Append(L_catch);
        p.Append(p.Create(OpCodes.Leave, L_end));
        p.Append(L_end);
        p.Append(p.Create(OpCodes.Ret));

        m.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = excT,
            TryStart = tryStart,
            TryEnd = L_tryEnd,
            HandlerStart = L_catch,
            HandlerEnd = L_end,
        });
    }

    static void EmitTryLoad(ModuleDefinition mod, MethodDefinition m, FieldDefinition fTick, MethodDefinition fileLog)
    {
        var p = m.Body.GetILProcessor();
        m.Body.InitLocals = true;

        var fileT = TypeRef(mod, "System.IO", "File");
        var asmT = TypeRef(mod, "System.Reflection", "Assembly");
        var typeT = TypeRef(mod, "System", "Type");
        var miT = TypeRef(mod, "System.Reflection", "MethodInfo");
        var mbT = TypeRef(mod, "System.Reflection", "MethodBase");   // Invoke 的**声明类型**（实测必须用它）
        var delT = TypeRef(mod, "System", "Delegate");
        var rthT = TypeRef(mod, "System", "RuntimeTypeHandle");
        var bfT = TypeRef(mod, "System.Reflection", "BindingFlags");
        var excT = TypeRef(mod, "System", "Exception");
        var strT = TypeRef(mod, "System", "String");
        var dbgT = TypeRef(mod, "UnityEngine", "Debug", "UnityEngine.CoreModule");
        var actionT = TypeRef(mod, "System", "Action");
        var objT = mod.TypeSystem.Object;
        var byteArr = new ArrayType(mod.TypeSystem.Byte);
        var objArr = new ArrayType(mod.TypeSystem.Object);

        var Vasm = new VariableDefinition(asmT); m.Body.Variables.Add(Vasm);
        var Vt = new VariableDefinition(typeT); m.Body.Variables.Add(Vt);
        var Vmi = new VariableDefinition(miT); m.Body.Variables.Add(Vmi);
        var Vtk = new VariableDefinition(miT); m.Body.Variables.Add(Vtk);
        var Vret = new VariableDefinition(mod.TypeSystem.Boolean); m.Body.Variables.Add(Vret);
        var Vex = new VariableDefinition(excT); m.Body.Variables.Add(Vex);

        var concat = MRef(strT, "Concat", strT, false, strT, strT);
        var log = MRef(dbgT, "LogError", mod.TypeSystem.Void, false, objT);
        var logWarn = MRef(dbgT, "LogError", mod.TypeSystem.Void, false, objT);
        // 只用 1 参重载 GetMethod(string)：游戏在 InitDll::LoadOver 里用的就是它（已证实存在）；
        // 2 参 BindingFlags 重载在游戏里 0 处使用 ⇒ 有被裁剪风险，不用。
        var getMethod = MRef(typeT, "GetMethod", miT, true, strT);

        var L_false = p.Create(OpCodes.Nop);
        var L_skipInit = p.Create(OpCodes.Nop);
        var L_skipTick = p.Create(OpCodes.Nop);
        var L_end = p.Create(OpCodes.Nop);
        var L_tryEnd = p.Create(OpCodes.Nop);
        var L_catch = p.Create(OpCodes.Stloc, Vex);

        var tryStart = p.Create(OpCodes.Nop);
        p.Append(tryStart);

        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, MRef(fileT, "Exists", mod.TypeSystem.Boolean, false, strT)));
        p.Append(p.Create(OpCodes.Brfalse, L_false));

        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, MRef(fileT, "ReadAllBytes", byteArr, false, strT)));
        p.Append(p.Create(OpCodes.Call, MRef(asmT, "Load", asmT, false, byteArr)));
        p.Append(p.Create(OpCodes.Stloc, Vasm));

        p.Append(p.Create(OpCodes.Ldloc, Vasm));
        p.Append(p.Create(OpCodes.Ldstr, "Starry.Mod"));
        p.Append(p.Create(OpCodes.Callvirt, MRef(asmT, "GetType", typeT, true, strT)));
        p.Append(p.Create(OpCodes.Stloc, Vt));
        p.Append(p.Create(OpCodes.Ldloc, Vt));
        p.Append(p.Create(OpCodes.Brfalse, L_false));

        // Init()
        p.Append(p.Create(OpCodes.Ldloc, Vt));
        p.Append(p.Create(OpCodes.Ldstr, "Init"));
        p.Append(p.Create(OpCodes.Callvirt, getMethod));
        p.Append(p.Create(OpCodes.Stloc, Vmi));
        p.Append(p.Create(OpCodes.Ldloc, Vmi));
        p.Append(p.Create(OpCodes.Brfalse, L_skipInit));
        p.Append(p.Create(OpCodes.Ldloc, Vmi));
        p.Append(p.Create(OpCodes.Ldnull));
        p.Append(p.Create(OpCodes.Ldnull));
        p.Append(p.Create(OpCodes.Callvirt, MRef(mbT, "Invoke", objT, true, objT, objArr)));
        p.Append(p.Create(OpCodes.Pop));
        p.Append(L_skipInit);

        // Tick → Action 委托
        p.Append(p.Create(OpCodes.Ldloc, Vt));
        p.Append(p.Create(OpCodes.Ldstr, "Tick"));
        p.Append(p.Create(OpCodes.Callvirt, getMethod));
        p.Append(p.Create(OpCodes.Stloc, Vtk));
        p.Append(p.Create(OpCodes.Ldloc, Vtk));
        p.Append(p.Create(OpCodes.Brfalse, L_skipTick));
        p.Append(p.Create(OpCodes.Ldloc, Vtk));
        p.Append(p.Create(OpCodes.Stsfld, fTick));
        p.Append(L_skipTick);

        p.Append(p.Create(OpCodes.Ldstr, "[StarryLvmeng] loaded mod dll: "));
        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, log));
        p.Append(p.Create(OpCodes.Ldstr, "[load] ok: "));
        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, fileLog));
        p.Append(p.Create(OpCodes.Ldc_I4_1));
        p.Append(p.Create(OpCodes.Stloc, Vret));
        p.Append(p.Create(OpCodes.Leave, L_end));

        p.Append(L_false);
        p.Append(p.Create(OpCodes.Ldstr, "[load] miss: "));
        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, fileLog));
        p.Append(p.Create(OpCodes.Ldc_I4_0));
        p.Append(p.Create(OpCodes.Stloc, Vret));
        p.Append(p.Create(OpCodes.Leave, L_end));
        p.Append(L_tryEnd);

        p.Append(L_catch);
        p.Append(p.Create(OpCodes.Ldstr, "[StarryLvmeng] load failed: "));
        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Ldstr, " : "));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Ldloc, Vex));
        p.Append(p.Create(OpCodes.Callvirt, MRef(excT, "get_Message", strT, true)));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, logWarn));
        p.Append(p.Create(OpCodes.Ldstr, "[load] exc: "));
        p.Append(p.Create(OpCodes.Ldarg_0));
        p.Append(p.Create(OpCodes.Call, concat));
        p.Append(p.Create(OpCodes.Call, fileLog));
        p.Append(p.Create(OpCodes.Ldc_I4_0));
        p.Append(p.Create(OpCodes.Stloc, Vret));
        p.Append(p.Create(OpCodes.Leave, L_end));

        p.Append(L_end);
        p.Append(p.Create(OpCodes.Ldloc, Vret));
        p.Append(p.Create(OpCodes.Ret));

        m.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = excT,
            TryStart = tryStart,
            TryEnd = L_tryEnd,
            HandlerStart = L_catch,
            HandlerEnd = L_end,
        });
    }

    // ------------------------------------------------------------------ 注入点
    static void InjectRunPrefix(ModuleDefinition mod, TypeDefinition hello)
    {
        var run = NeedMethod(hello, "Run", 0);
        var boot = hello.Methods.First(x => x.Name == "StarryBootstrap");
        var p = run.Body.GetILProcessor();
        p.InsertBefore(run.Body.Instructions[0], p.Create(OpCodes.Call, boot));
        Console.WriteLine($"[inj] Hello::Run prefix → call StarryBootstrap  (IL {run.Body.Instructions.Count})");
    }

    static void InjectTickPostfix(ModuleDefinition mod, TypeDefinition hello)
    {
        var fu = NeedMethod(Need(mod, "ACT.Battle.SystemTime"), "FixedUpdate", 0);
        var fTick = hello.Fields.First(x => x.Name == "s_starryTick");
        var mbT2 = TypeRef(mod, "System.Reflection", "MethodBase");
        var objArr2 = new ArrayType(mod.TypeSystem.Object);
        var invoke = MRef(mbT2, "Invoke", mod.TypeSystem.Object, true, mod.TypeSystem.Object, objArr2);

        var rets = fu.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        if (rets.Count == 0) throw new InvalidOperationException("SystemTime::FixedUpdate 没有 ret");
        foreach (var ret in rets)
        {
            var p = fu.Body.GetILProcessor();
            var s = new Seq(p);
            s.Add(OpCodes.Ldsfld, fTick);
            var br = s.Add(OpCodes.Brfalse, ret);   // 占位目标，紧接下行改正
            s.Add(OpCodes.Ldsfld, fTick);
            s.Add(OpCodes.Ldnull);      // obj
            s.Add(OpCodes.Ldnull);      // parameters
            s.Add(OpCodes.Callvirt, invoke);
            s.Add(OpCodes.Pop);         // 丢弃返回值
            var lSkip = s.Add(OpCodes.Nop);
            br.Operand = lSkip;
            s.InsertBefore(fu, ret);
        }
        Console.WriteLine($"[inj] SystemTime::FixedUpdate postfix → s_starryTick?.Invoke(null,null)  (IL {fu.Body.Instructions.Count}, ret×{rets.Count})");
    }

    // 阶段 2 -------------------------------------------------------------------
    static TypeReference ModType(ModuleDefinition mod)
    {
        var scope = mod.AssemblyReferences.FirstOrDefault(x => x.Name == MOD_ASM);
        if (scope == null)
        {
            scope = new AssemblyNameReference(MOD_ASM, new Version(1, 0, 0, 0));
            mod.AssemblyReferences.Add(scope);
        }
        return new TypeReference("Starry", "Mod", mod, scope);
    }

    static void InjectOnSetHP(ModuleDefinition mod, TypeReference modT)
    {
        var ca = Need(mod, "ACT.Battle.CompAbility");
        var sshp = NeedMethod(ca, "ServerSetHP", 1);
        var onSet = MRef(modT, "OnSetHP", mod.TypeSystem.Int32, false, ca);

        // prefix 改写参数：原方法体的 Clamp(value,0,maxHP) + onHPChange 事件全部保留，
        // 只把入口值换成 OnSetHP 的返回值（<0 ⇒ 不改）。比 postfix 直写 m_HP 少一个「血条不刷新」的坑。
        var p = sshp.Body.GetILProcessor();
        var v = new VariableDefinition(mod.TypeSystem.Int32);
        sshp.Body.Variables.Add(v);
        sshp.Body.InitLocals = true;

        var lSkip = p.Create(OpCodes.Nop);
        var s = new Seq(p);
        s.Add(OpCodes.Ldarg_0);
        s.Add(OpCodes.Call, onSet);
        s.Add(OpCodes.Stloc, v);
        s.Add(OpCodes.Ldloc, v);
        s.Add(OpCodes.Ldc_I4_0);
        s.Add(OpCodes.Blt, lSkip);              // v < 0 ⇒ 不动参数
        s.Add(OpCodes.Ldloc, v);
        s.Add(OpCodes.Starg, sshp.Parameters[0]);   // 实例方法：Parameters 不含 this，value 是第 0 个
        s.Add(lSkip);
        s.InsertBefore(sshp, sshp.Body.Instructions[0], retarget: false);

        Console.WriteLine($"[inj] CompAbility::ServerSetHP prefix → OnSetHP + 条件 starg value  (IL {sshp.Body.Instructions.Count})");
    }

    static void InjectOnResolveDamage(ModuleDefinition mod, TypeReference modT)
    {
        var m = NeedMethod(Need(mod, "ACT.Battle.SystemAbility"), "ServerResolveDamage", 2);
        var ad = Need(mod, "ACT.Battle.AttackData");
        var p = m.Body.GetILProcessor();
        var s = new Seq(p);
        s.Add(OpCodes.Ldarg_1);
        s.Add(OpCodes.Call, MRef(modT, "OnResolveDamage", mod.TypeSystem.Void, false, ad));
        s.InsertBefore(m, m.Body.Instructions[0], retarget: false);
        Console.WriteLine($"[inj] SystemAbility::ServerResolveDamage prefix → OnResolveDamage  (IL {m.Body.Instructions.Count})");
    }

    static void InjectOnSetJumpCount(ModuleDefinition mod, TypeReference modT)
    {
        var m = NeedMethod(Need(mod, "ACT.Battle.Configs.ActionSetJumpCount"), "Do", 1);
        var am = Need(mod, "ACT.Battle.Configs.ActionMemory");
        var call = MRef(modT, "OnSetJumpCount", mod.TypeSystem.Void, false, am);
        var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        foreach (var ret in rets)
        {
            var p = m.Body.GetILProcessor();
            var s = new Seq(p);
            s.Add(OpCodes.Ldarg_1);
            s.Add(OpCodes.Call, call);
            s.InsertBefore(m, ret);
        }
        Console.WriteLine($"[inj] ActionSetJumpCount::Do postfix → OnSetJumpCount  (IL {m.Body.Instructions.Count}, ret×{rets.Count})");
    }

    static void InjectOnRecoverMotion(ModuleDefinition mod, TypeReference modT)
    {
        var cm = Need(mod, "ACT.Battle.CompMotion");
        var m = NeedMethod(cm, "RecoverMotionCount", 0);
        var call = MRef(modT, "OnRecoverMotion", mod.TypeSystem.Void, false, cm);
        var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        foreach (var ret in rets)
        {
            var p = m.Body.GetILProcessor();
            var s = new Seq(p);
            s.Add(OpCodes.Ldarg_0);
            s.Add(OpCodes.Call, call);
            s.InsertBefore(m, ret);
        }
        Console.WriteLine($"[inj] CompMotion::RecoverMotionCount postfix → OnRecoverMotion  (IL {m.Body.Instructions.Count})");
    }

    // ------------------------------------------------------------------ 主流程
    public static int Build(AssemblyDefinition asm, string src, string dst, int phase, long pad, bool keepSize)
    {
        var mod = asm.MainModule;
        Console.WriteLine($"[in ] {src}  {new FileInfo(src).Length} B  types={mod.Types.Count}  phase={phase}");

        var hello = Need(mod, "Hello");
        var before = ConstantAudit.Snapshot(mod);

        AddBootstrap(mod, hello);
        InjectRunPrefix(mod, hello);
        InjectTickPostfix(mod, hello);

        if (phase >= 2)
        {
            var modT = ModType(mod);
            InjectOnSetHP(mod, modT);
            InjectOnResolveDamage(mod, modT);
            InjectOnSetJumpCount(mod, modT);
            InjectOnRecoverMotion(mod, modT);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dst))!);
        asm.Write(dst, new WriterParameters { WriteSymbols = false });
        long raw = new FileInfo(dst).Length;
        Console.WriteLine($"[out] {dst}  {raw} B  (原始 {new FileInfo(src).Length} B, 余量 {new FileInfo(src).Length - raw} B)");
        if (keepSize) { RestorePeLayout(src, dst, pad); PadTo(dst, pad); Console.WriteLine($"[pad] 最终 {new FileInfo(dst).Length} B (目标 {pad})"); }

        using (var back = AssemblyDefinition.ReadAssembly(dst, new ReaderParameters
        { AssemblyResolver = new TolerantAssemblyResolver(), ReadSymbols = false }))
        {
            var lost = ConstantAudit.Diff(before, back.MainModule);
            Console.WriteLine($"[audit] 常量丢失 = {lost.Count}");
            foreach (var l in lost.Take(20)) Console.WriteLine("   " + l);
            if (lost.Count > 0) { Console.WriteLine("✗ FAIL：常量丢失"); return 3; }
        }
        return 0;
    }
}
