using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SRF = System.Reflection;

// 霸海争锋 b8 行为级离线验证（游戏私有）
// 做法：把 rc_b8 里 **真实 Starry.BhzfPatch 方法体指令** 逐条克隆到 hanrness 类型，
//       仅把 Battle.Life / Battle.Player / JInt 重映射到最小替身，配置路径常量改到测试目录。
// 不手写 Block/Scale 逻辑镜像；指令、EH、locals、branch 均来自真实补丁产物。
internal static class Program
{
    private const string RcDev = "/Users/aliang/Documents/nixiang/07_patched/霸海争锋/rc_b8/mygame_bhzf_dev_rc_b8.dll";
    private const string IniPath = "/tmp/bhzf_behavior_config.ini";
    private static SRF.Assembly _dynamic;
    private static Type _host, _life, _player, _jint;

    private static int Main()
    {
        var asm = AssemblyDefinition.ReadAssembly(RcDev);
        var src = asm.MainModule.GetType("Starry.BhzfPatch");
        var dyn = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("BhzfHarness", new Version(1, 0, 0, 0)), "BhzfHarness", ModuleKind.Dll);
        var mod = dyn.MainModule;

        var jint = new TypeDefinition("Stub", "JInt", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout, mod.ImportReference(typeof(System.ValueType)));
        var jintField = new FieldDefinition("v", FieldAttributes.Public, mod.TypeSystem.Int64);
        jint.Fields.Add(jintField);
        var jintCtor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, mod.TypeSystem.Void);
        jintCtor.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, mod.TypeSystem.Int64));
        jint.Methods.Add(jintCtor);
        var cil = jintCtor.Body.GetILProcessor();
        cil.Emit(OpCodes.Ldarg_0); cil.Emit(OpCodes.Ldarg_1); cil.Emit(OpCodes.Stfld, jintField); cil.Emit(OpCodes.Ret);
        var getV = new MethodDefinition("get_Value", MethodAttributes.Public | MethodAttributes.HideBySig, mod.TypeSystem.Int64);
        jint.Methods.Add(getV);
        var gv = getV.Body.GetILProcessor();
        gv.Emit(OpCodes.Ldarg_0); gv.Emit(OpCodes.Ldfld, jintField); gv.Emit(OpCodes.Ret);
        mod.Types.Add(jint);

        var player = new TypeDefinition("Stub", "Player", TypeAttributes.Public | TypeAttributes.Class, mod.TypeSystem.Object);
        var playerCtor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, mod.TypeSystem.Void);
        player.Methods.Add(playerCtor);
        var pc = playerCtor.Body.GetILProcessor();
        pc.Emit(OpCodes.Ldarg_0);
        pc.Emit(OpCodes.Call, mod.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes)));
        pc.Emit(OpCodes.Ret);
        var campField = new FieldDefinition("camp", FieldAttributes.Public, mod.TypeSystem.Int32);
        player.Fields.Add(campField);
        var getCamp = new MethodDefinition("GetCamp", MethodAttributes.Public | MethodAttributes.HideBySig, mod.TypeSystem.Int32);
        player.Methods.Add(getCamp);
        var gc = getCamp.Body.GetILProcessor();
        gc.Emit(OpCodes.Ldarg_0); gc.Emit(OpCodes.Ldfld, campField); gc.Emit(OpCodes.Ret);
        mod.Types.Add(player);

        var life = new TypeDefinition("Stub", "Life", TypeAttributes.Public | TypeAttributes.Class, mod.TypeSystem.Object);
        var lifeCtor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, mod.TypeSystem.Void);
        life.Methods.Add(lifeCtor);
        var lc = lifeCtor.Body.GetILProcessor();
        lc.Emit(OpCodes.Ldarg_0);
        lc.Emit(OpCodes.Call, mod.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes)));
        lc.Emit(OpCodes.Ret);
        var curHp = new FieldDefinition("_curHP", FieldAttributes.Public, jint);
        var pField = new FieldDefinition("player", FieldAttributes.Public, player);
        life.Fields.Add(curHp); life.Fields.Add(pField);
        var getPlayer = new MethodDefinition("GetPlayer", MethodAttributes.Public | MethodAttributes.HideBySig, player);
        life.Methods.Add(getPlayer);
        var gp = getPlayer.Body.GetILProcessor();
        gp.Emit(OpCodes.Ldarg_0); gp.Emit(OpCodes.Ldfld, pField); gp.Emit(OpCodes.Ret);
        mod.Types.Add(life);

        var host = new TypeDefinition("Starry", "BhzfPatch", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, mod.TypeSystem.Object);
        mod.Types.Add(host);
        // 静态字段
        foreach (var f in src.Fields)
        {
            var nf = new FieldDefinition(f.Name, f.Attributes, f.FieldType.FullName == "System.Int32" ? mod.TypeSystem.Int32 : mod.TypeSystem.Int64);
            host.Fields.Add(nf);
        }
        // 方法
        var map = new Dictionary<string, MethodDefinition>();
        foreach (var m in src.Methods.Where(m => m.HasBody))
        {
            var nm = new MethodDefinition(m.Name, m.Attributes, mod.TypeSystem.Void);
            foreach (var prm in m.Parameters)
                nm.Parameters.Add(new ParameterDefinition(prm.Name, prm.Attributes, Remap(mod, prm.ParameterType)));
            nm.Attributes = m.Attributes;
            nm.ReturnType = Remap(mod, m.ReturnType);
            host.Methods.Add(nm);
            map[m.Name] = nm;
        }
        // Refresh 需要读文件：把路径常量重映射到测试 ini，其余逻辑照搬
        foreach (var m in src.Methods.Where(m => m.HasBody))
            Clone(mod, m, map[m.Name], host, life, player, jint, curHp, pField, jintField, map);

        var path = "/tmp/bhzf_harness.dll";
        dyn.Write(path);
        _dynamic = SRF.Assembly.LoadFile(path);
        _host = _dynamic.GetType("Starry.BhzfPatch");
        _life = _dynamic.GetType("Stub.Life");
        _player = _dynamic.GetType("Stub.Player");
        _jint = _dynamic.GetType("Stub.JInt");
        Console.WriteLine("[build] harness ok: " + path);
        Console.WriteLine($"methods: {string.Join(",", _host.GetMethods(SRF.BindingFlags.Public|SRF.BindingFlags.NonPublic|SRF.BindingFlags.Static).Select(x=>x.Name))}");

        RunMatrix();
        return 0;
    }

    private static void RunMatrix()
    {
        int pass = 0, fail = 0;
        void Check(string name, long input, int god, int kill, int camp, bool expectAdjusted, long expectVal)
        {
            WriteIni(god, kill);
            _host.GetField("s_tick", SRF.BindingFlags.NonPublic | SRF.BindingFlags.Static).SetValue(null, -100000);   // 复位 1s 缓存
            var life = MakeLife(1000, camp);
            var value = MakeJInt(input);
            var launcher = life;
            var args = new object[] { life, value, launcher, null };
            var tryAdjust = _host.GetMethod("TryAdjust", SRF.BindingFlags.Public|SRF.BindingFlags.Static);
            var ok = (bool)tryAdjust.Invoke(null, args);
            long got = ok ? (long)_jint.GetMethod("get_Value").Invoke(args[3], null) : input;
            var status = (ok == expectAdjusted && got == expectVal) ? "PASS" : "FAIL";
            if (status == "PASS") pass++; else fail++;
            Console.WriteLine($"{status}\t{name}\tinput={input} god={god} kill={kill} camp={camp} -> adjusted={ok} value={got} (expect {expectAdjusted}/{expectVal})");
        }

        // 隔离：先调无 EH 的 CampOf，再调含 EH 的 Refresh
        try
        {
            var life0 = MakeLife(100, 0);
            var camp = (int)_host.GetMethod("CampOf", SRF.BindingFlags.NonPublic | SRF.BindingFlags.Static).Invoke(null, new object[] { life0 });
            Console.WriteLine($"PROBE CampOf(no-EH) -> {camp} (expect 0)");
        }
        catch (Exception ex) { var e = ex; while (e.InnerException != null) e = e.InnerException; Console.WriteLine("PROBE CampOf EX: " + e.GetType().Name); }
        // 首例：只验 AllOff + Refresh（真实 IL，含 EH）
        try
        {
            WriteIni(0, 0);
            _host.GetField("s_tick", SRF.BindingFlags.NonPublic | SRF.BindingFlags.Static).SetValue(null, -100000);
            var allOff = (bool)_host.GetMethod("AllOff", SRF.BindingFlags.Public | SRF.BindingFlags.Static).Invoke(null, null);
            WriteIni(1, 0);
            _host.GetField("s_tick", SRF.BindingFlags.NonPublic | SRF.BindingFlags.Static).SetValue(null, -100000);
            var allOff2 = (bool)_host.GetMethod("AllOff", SRF.BindingFlags.Public | SRF.BindingFlags.Static).Invoke(null, null);
            Console.WriteLine($"FIRST-CASE alloff: god0kill0 -> {allOff} (expect True); god1kill0 -> {allOff2} (expect False)");
        }
        catch (Exception ex) { var e = ex; while (e.InnerException != null) e = e.InnerException; Console.WriteLine("FIRST-CASE EXCEPTION: " + e.GetType().FullName + ": " + e.Message); Console.WriteLine(e.StackTrace); return; }

        // BlockGod 独立用例（真实 IL）
        void CheckBlock(string name, long input, int god, int camp, bool expect)
        {
            WriteIni(god, 0);
            _host.GetField("s_tick", SRF.BindingFlags.NonPublic | SRF.BindingFlags.Static).SetValue(null, -100000);
            var life = MakeLife(1000, camp);
            var value = MakeJInt(input);
            var m = _host.GetMethod("BlockGod", SRF.BindingFlags.Public | SRF.BindingFlags.Static);
            bool got = false;
            try { got = (bool)m.Invoke(null, new object[] { life, value }); }
            catch (Exception ex) { var e = ex; while (e.InnerException != null) e = e.InnerException; Console.WriteLine($"FAIL	{name}	EXCEPTION {e.GetType().Name}"); fail++; return; }
            var st = got == expect ? "PASS" : "FAIL";
            if (st == "PASS") pass++; else fail++;
            Console.WriteLine($"{st}	{name}	god={god} camp={camp} v={input} -> block={got} (expect {expect})");
        }
        CheckBlock("god/friend-negative", -100, 1, 1, true);
        CheckBlock("god/enemy-negative", -100, 1, 0, false);
        CheckBlock("god/friend-positive", 100, 1, 1, false);

        Check("all-off/positive", 500, 0, 0, 0, false, 500);
        Check("all-off/negative", -500, 0, 0, 1, false, -500);
        Check("all-off/zero", 0, 0, 0, 0, false, 0);
        Check("all-off/long-neg", -4294967296L, 0, 0, 0, false, -4294967296L);
        Check("all-off/long-overflow", -9223372036854775807L, 0, 0, 0, false, -9223372036854775807L);
        Check("kill/enemy-negative", -500, 0, 1, 0, true, -1001);
        Check("kill/friend-negative", -500, 0, 1, 1, false, -500);
        Check("kill/heal-positive", 500, 0, 1, 0, false, 500);
        Check("kill/unknown-camp", -500, 0, 1, -1, false, -500);
        Console.WriteLine($"SUMMARY pass={pass} fail={fail}");
    }

    private static void WriteIni(int god, int kill)
        => File.WriteAllText(IniPath, $"god={god}\nkill={kill}\n");

    private static object MakeLife(long hp, int camp)
    {
        var life = Activator.CreateInstance(_life);
        var player = Activator.CreateInstance(_player);
        _player.GetField("camp").SetValue(player, camp);
        _life.GetField("player").SetValue(life, player);
        _life.GetField("_curHP").SetValue(life, MakeJInt(hp));
        return life;
    }

    private static object MakeJInt(long v)
    {
        var o = Activator.CreateInstance(_jint);
        _jint.GetConstructor(new[] { typeof(long) }).Invoke(o, new object[] { v });
        return o;
    }

    private static MethodReference SysMethod(MethodReference mr, ModuleDefinition mod)
    {
        var decl = mr.DeclaringType.FullName;
        if (decl == "System.IO.File" && mr.Name == "ReadAllText")
            return mod.ImportReference(typeof(File).GetMethod("ReadAllText", new[] { typeof(string) }));
        if (decl == "System.Environment" && mr.Name == "get_TickCount")
            return mod.ImportReference(typeof(Environment).GetProperty("TickCount").GetGetMethod());
        if (decl == "System.String" && mr.Name == "Contains")
            return mod.ImportReference(typeof(string).GetMethod("Contains", new[] { typeof(string) }));
        return null;
    }

    private static TypeReference Remap(ModuleDefinition mod, TypeReference t)
    {
        switch (t.FullName)
        {
            case "JEngine.AntiCheat.JInt": return mod.GetType("Stub.JInt");
            case "Battle.Life": return mod.GetType("Stub.Life");
            case "Battle.Player": return mod.GetType("Stub.Player");
            case var _ when t is ByReferenceType brt: return new ByReferenceType(Remap(mod, brt.ElementType));
            case var _ when t is ArrayType at: return new ArrayType(Remap(mod, at.ElementType));
            case var _ when t is PointerType pt: return new PointerType(Remap(mod, pt.ElementType));
            default: return mod.ImportReference(t);
        }
    }

    private static void Clone(ModuleDefinition mod, MethodDefinition src, MethodDefinition dst,
        TypeDefinition host, TypeDefinition life, TypeDefinition player, TypeDefinition jint,
        FieldDefinition curHp, FieldDefinition pField, FieldDefinition jintField,
        Dictionary<string, MethodDefinition> hostMethods)
    {
        var body = dst.Body;
        body.InitLocals = src.Body.InitLocals;
        body.MaxStackSize = src.Body.MaxStackSize;
        foreach (var v in src.Body.Variables) body.Variables.Add(new VariableDefinition(Remap(mod, v.VariableType)));

        var insMap = new Dictionary<Instruction, Instruction>();
        foreach (var ins in src.Body.Instructions) insMap[ins] = Instruction.Create(OpCodes.Nop);

        var il = body.GetILProcessor();
        int idx = 0;
        foreach (var ins in src.Body.Instructions)
        {
            var ni = insMap[ins];
            ni.OpCode = ins.OpCode;
            object op = ins.Operand;
            switch (op)
            {
                case null: break;
                case Instruction t: ni.Operand = insMap[t]; break;
                case Instruction[] arr: ni.Operand = arr.Select(x => insMap[x]).ToArray(); break;
                case MethodReference mr:
                    var decl = mr.DeclaringType.FullName;
                    if (decl == "Starry.BhzfPatch" && hostMethods.TryGetValue(mr.Name, out var hm)) { ni.Operand = hm; break; }
                    var declType = decl == "Battle.Life" ? life : decl == "Battle.Player" ? player : decl == "JEngine.AntiCheat.JInt" ? jint : null;
                    MethodDefinition target = null;
                    if (declType != null)
                    {
                        target = declType.Methods.First(x => x.Name == mr.Name && x.Parameters.Count == mr.Parameters.Count);
                    }
                    if (target == null)
                    {
                        // 目标在 Stub 之外：若是 JInt op 或 System 类型，则按名字在宿主/系统里找
                        if (decl == "JEngine.AntiCheat.JInt")
                            target = jint.Methods.First(x => x.Name == mr.Name);
                        else
                        {
                            var host2 = SysMethod(mr, mod);
                            if (host2 != null) { ni.Operand = host2; break; }
                            ni.Operand = mr;
                            break;
                        }
                    }
                    if (target != null)
                    {
                        // 实例方法调用需要 HasThis 匹配
                        ni.Operand = target;
                    }
                    break;
                case FieldReference fr:
                    if (fr.DeclaringType.FullName == "Starry.BhzfPatch") ni.Operand = host.Fields.First(f => f.Name == fr.Name);
                    else if (fr.DeclaringType.FullName == "Battle.Life") ni.Operand = fr.Name == "_curHP" ? curHp : pField;
                    else if (fr.DeclaringType.FullName == jint.FullName) ni.Operand = jintField;
                    else ni.Operand = fr;
                    break;
                case string s: ni.Operand = s.Contains("starry_config.ini") ? IniPath : s; break;
                case TypeReference tr: ni.Operand = Remap(mod, tr); break;
                case ParameterDefinition pd: ni.Operand = dst.Parameters[pd.Index]; break;
                case VariableDefinition vd: ni.Operand = body.Variables[vd.Index]; break;
                default: ni.Operand = op; break;
            }
            il.Append(ni);
            idx++;
        }
        // 修正 import：对仍指向原程序集的 MethodReference/FieldReference 做 import
        foreach (var ins in body.Instructions)
        {
            if (ins.Operand is MethodReference mr && !(mr is MethodDefinition) && mr.DeclaringType.Scope is AssemblyNameReference)
                ins.Operand = mod.ImportReference(mr);
            if (ins.Operand is FieldReference fr && !(fr is FieldDefinition) && fr.DeclaringType.Scope is AssemblyNameReference)
                ins.Operand = mod.ImportReference(fr);
        }
        foreach (var h in src.Body.ExceptionHandlers)
            body.ExceptionHandlers.Add(new ExceptionHandler(h.HandlerType)
            {
                CatchType = h.CatchType == null ? null : mod.ImportReference(typeof(Exception)),
                TryStart = insMap[h.TryStart], TryEnd = insMap[h.HandlerStart],   // 单变量：TryEnd 归一化到 HandlerStart
                HandlerStart = insMap[h.HandlerStart], HandlerEnd = h.HandlerEnd == null ? null : insMap[h.HandlerEnd]
            });
    }
}
