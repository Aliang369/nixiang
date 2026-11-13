using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

internal sealed class FallbackResolver : BaseAssemblyResolver
{
    private readonly System.Collections.Generic.Dictionary<string, AssemblyDefinition> _cache = new();
    public override AssemblyDefinition Resolve(AssemblyNameReference name)
    {
        try { return base.Resolve(name); }
        catch (AssemblyResolutionException)
        {
            if (_cache.TryGetValue(name.Name, out var a)) return a;
            var stub = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition(name.Name, name.Version ?? new Version(0, 0, 0, 0)), name.Name, ModuleKind.Dll);
            _cache[name.Name] = stub;
            return stub;
        }
    }
}

// 霸海争锋 IL 补丁器 —— b8：全关零干扰
// 语义：
//   AllOff()==true                  → 直接跳原方法首条（不做任何 JInt 读/写/转换）
//   god 开 且 victim 属 FRIEND(1) 且 v<0 → return（跳过本次扣血）
//   kill 开 且 victim 属 ENEMY(0)  且 v<0 → value = new JInt(-_curHP.Value - 1)（走原死亡流程）
internal static class Patcher
{
    private static ModuleDefinition _mod;

    public static void Run(string src, string dst)
    {
        var resolver = new FallbackResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(src)));
        resolver.AddSearchDirectory("/Users/aliang/Documents/nixiang/13_scratch/霸海争锋/dll");
        resolver.AddSearchDirectory("/Users/aliang/Documents/nixiang/06_dumped/霸海争锋");
        resolver.AddSearchDirectory("/Users/aliang/Documents/nixiang/06_dumped/霸海争锋/1.1.036_il2cpp/dump/DummyDll");
        var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { AssemblyResolver = resolver });
        _mod = asm.MainModule;

        var life = _mod.GetType("Battle.Life") ?? throw new Exception("Battle.Life not found");
        var addHp = life.Methods.FirstOrDefault(m => m.Name == "AddHP" && m.Parameters.Count == 3)
                    ?? throw new Exception("AddHP(JInt,Life,int) not found");
        var helper = BuildHelper(life);

        var allOff = helper.Methods.First(m => m.Name == "AllOff");
        var blockGod = helper.Methods.First(m => m.Name == "BlockGod");

        var il = addHp.Body.GetILProcessor();
        var first = addHp.Body.Instructions[0];

        var afterBlock = il.Create(OpCodes.Nop);
        var endLabel = il.Create(OpCodes.Nop);
        var tryAdjust = helper.Methods.First(m => m.Name == "TryAdjust");
        var localAdj = new VariableDefinition(_mod.ImportReference(_mod.GetType("JEngine.AntiCheat.JInt")));
        addHp.Body.InitLocals = true;
        addHp.Body.Variables.Add(localAdj);
        var seq = new[]
        {
            il.Create(OpCodes.Call, _mod.ImportReference(allOff)),
            il.Create(OpCodes.Brtrue_S, first),
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Ldarg_1),
            il.Create(OpCodes.Call, _mod.ImportReference(blockGod)),
            il.Create(OpCodes.Brfalse_S, afterBlock),
            il.Create(OpCodes.Ret),
            afterBlock,
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Ldarg_1),
            il.Create(OpCodes.Ldarg_2),
            il.Create(OpCodes.Ldloca_S, localAdj),
            il.Create(OpCodes.Call, _mod.ImportReference(tryAdjust)),
            il.Create(OpCodes.Brfalse_S, first),
            il.Create(OpCodes.Ldloc_S, localAdj),
            il.Create(OpCodes.Starg_S, addHp.Parameters[0]),
        };
        foreach (var ins in seq) il.InsertBefore(first, ins);
        if (addHp.Body.MaxStackSize < 8) addHp.Body.MaxStackSize = 8;

        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        asm.Write(dst);
        Console.WriteLine($"[ok] b8 patched {addHp.FullName} (+{seq.Length} IL) -> {dst}");
    }

    private static TypeDefinition BuildHelper(TypeDefinition life)
    {
        var jint = _mod.GetType("JEngine.AntiCheat.JInt");
        var player = _mod.GetType("Battle.Player");
        var getPlayer = life.Methods.First(m => m.Name == "GetPlayer" && m.Parameters.Count == 0);
        var getCamp = player.Methods.First(m => m.Name == "GetCamp" && m.Parameters.Count == 0);
        var getValue = jint.Methods.First(m => m.Name == "get_Value" && m.ReturnType.FullName == "System.Int64");
        var curHp = life.Fields.First(f => f.Name == "_curHP");
        var fromIntSafe = jint.Methods.First(m => m.Name == "op_Implicit" && m.Parameters.Count == 1
                    && m.Parameters[0].ParameterType.FullName == "System.Int32" && m.ReturnType.FullName == jint.FullName);
        var ctorLong = jint.Methods.First(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Int64");
        // 目标作用域一律走 mscorlib（禁止 typeof(host) 引入 System.Private.CoreLib）
        var coreLib = _mod.TypeSystem.CoreLibrary;
        var strT = _mod.TypeSystem.String;
        var intT = _mod.TypeSystem.Int32;
        var boolT = _mod.TypeSystem.Boolean;
        var voidT = _mod.TypeSystem.Void;

        var fileType = new TypeReference("System.IO", "File", _mod, coreLib);
        var readAll = new MethodReference("ReadAllText", strT, fileType) { HasThis = false };
        readAll.Parameters.Add(new ParameterDefinition(strT));

        var envType = new TypeReference("System", "Environment", _mod, coreLib);
        var tick = new MethodReference("get_TickCount", intT, envType) { HasThis = false };

        var contains = new MethodReference("Contains", boolT, strT) { HasThis = true };
        contains.Parameters.Add(new ParameterDefinition(strT));

        var exType = new TypeReference("System", "Exception", _mod, coreLib);

        var t = new TypeDefinition("Starry", "BhzfPatch",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            _mod.TypeSystem.Object);
        _mod.Types.Add(t);
        var fGod = new FieldDefinition("s_god", FieldAttributes.Static | FieldAttributes.Private, _mod.TypeSystem.Int32);
        var fKill = new FieldDefinition("s_kill", FieldAttributes.Static | FieldAttributes.Private, _mod.TypeSystem.Int32);
        var fTick = new FieldDefinition("s_tick", FieldAttributes.Static | FieldAttributes.Private, _mod.TypeSystem.Int32);
        t.Fields.Add(fGod); t.Fields.Add(fKill); t.Fields.Add(fTick);

        // Refresh：1 秒缓存读 ini
        var refresh = new MethodDefinition("Refresh", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, _mod.TypeSystem.Void);
        refresh.Body.InitLocals = true;
        refresh.Body.Variables.Add(new VariableDefinition(_mod.TypeSystem.String));
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
        var tryStart = r.Create(OpCodes.Ldstr, "/data/user/0/com.bhzf.zbml/files/starry_config.ini");
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

        // CampOf(self)
        var campOf = new MethodDefinition("CampOf", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, _mod.TypeSystem.Int32);
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
        c.Append(c.Create(OpCodes.Callvirt, _mod.ImportReference(getPlayer)));
        c.Append(c.Create(OpCodes.Brtrue, pn));
        c.Append(c.Create(OpCodes.Ldc_I4_M1)); c.Append(c.Create(OpCodes.Ret));
        c.Append(pn);
        c.Append(c.Create(OpCodes.Ldarg_0));
        c.Append(c.Create(OpCodes.Callvirt, _mod.ImportReference(getPlayer)));
        c.Append(c.Create(OpCodes.Callvirt, _mod.ImportReference(getCamp)));
        c.Append(c.Create(OpCodes.Conv_I4));
        c.Append(c.Create(OpCodes.Ret));

        // AllOff()
        var allOff = new MethodDefinition("AllOff", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, _mod.TypeSystem.Boolean);
        t.Methods.Add(allOff);
        var ao = allOff.Body.GetILProcessor();
        var aoFalse = ao.Create(OpCodes.Ldc_I4_0);
        var aoTrue = ao.Create(OpCodes.Ldc_I4_1);
        ao.Append(ao.Create(OpCodes.Call, refresh));
        ao.Append(ao.Create(OpCodes.Ldsfld, fGod));
        ao.Append(ao.Create(OpCodes.Brtrue, aoFalse));
        ao.Append(ao.Create(OpCodes.Ldsfld, fKill));
        ao.Append(ao.Create(OpCodes.Brtrue, aoFalse));
        ao.Append(aoTrue);
        ao.Append(ao.Create(OpCodes.Ret));
        ao.Append(aoFalse);
        ao.Append(ao.Create(OpCodes.Ret));

        // BlockGod(self, JInt v) —— 结果存局部，单一出口
        MethodDefinition BoolHelper(string name)
        {
            var m = new MethodDefinition(name, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, boolT);
            m.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, life));
            m.Parameters.Add(new ParameterDefinition("v", ParameterAttributes.None, jint));
            m.Body.InitLocals = true;
            m.Body.Variables.Add(new VariableDefinition(_mod.TypeSystem.Int64));   // [0] long v
            m.Body.Variables.Add(new VariableDefinition(boolT));                    // [1] bool r
            t.Methods.Add(m);
            return m;
        }

        var blockGod = BoolHelper("BlockGod");
        var g = blockGod.Body.GetILProcessor();
        var gEnd = g.Create(OpCodes.Ldloc_1);
        g.Append(g.Create(OpCodes.Ldc_I4_0));
        g.Append(g.Create(OpCodes.Stloc_1));
        var gTry = g.Create(OpCodes.Call, refresh);
        g.Append(gTry);
        g.Append(g.Create(OpCodes.Ldsfld, fGod));
        g.Append(g.Create(OpCodes.Brfalse, gEnd));
        g.Append(g.Create(OpCodes.Ldarga_S, blockGod.Parameters[1]));
        g.Append(g.Create(OpCodes.Call, _mod.ImportReference(getValue)));
        g.Append(g.Create(OpCodes.Stloc_0));
        g.Append(g.Create(OpCodes.Ldloc_0));
        g.Append(g.Create(OpCodes.Ldc_I8, 0L));
        g.Append(g.Create(OpCodes.Bge, gEnd));
        g.Append(g.Create(OpCodes.Ldarg_0));
        g.Append(g.Create(OpCodes.Call, campOf));
        g.Append(g.Create(OpCodes.Ldc_I4_1));
        g.Append(g.Create(OpCodes.Ceq));
        g.Append(g.Create(OpCodes.Stloc_1));
        var gTryEnd = g.Create(OpCodes.Leave, gEnd); g.Append(gTryEnd);
        var gCatch = g.Create(OpCodes.Pop); g.Append(gCatch);
        g.Append(g.Create(OpCodes.Ldc_I4_0));
        g.Append(g.Create(OpCodes.Stloc_1));
        g.Append(g.Create(OpCodes.Leave, gEnd));
        var gCatchEnd = g.Create(OpCodes.Nop); g.Append(gCatchEnd);
        g.Append(gEnd);
        g.Append(g.Create(OpCodes.Ret));
        blockGod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        { CatchType = exType, TryStart = gTry, TryEnd = gTryEnd, HandlerStart = gCatch, HandlerEnd = gCatchEnd });

        // TryAdjust(self, JInt value, Life launcher, out JInt adjusted) -> bool
        var tryAdjustM = new MethodDefinition("TryAdjust", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, boolT);
        tryAdjustM.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, life));
        tryAdjustM.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, jint));
        tryAdjustM.Parameters.Add(new ParameterDefinition("launcher", ParameterAttributes.None, life));
        var outP = new ParameterDefinition("adjusted", ParameterAttributes.Out, new ByReferenceType(jint));
        tryAdjustM.Parameters.Add(outP);
        tryAdjustM.Body.InitLocals = true;
        tryAdjustM.Body.Variables.Add(new VariableDefinition(_mod.TypeSystem.Int64));  // [0] long v
        tryAdjustM.Body.Variables.Add(new VariableDefinition(boolT));                   // [1] bool ok
        t.Methods.Add(tryAdjustM);
        var a = tryAdjustM.Body.GetILProcessor();
        var aEnd = a.Create(OpCodes.Ldloc_1);
        a.Append(a.Create(OpCodes.Ldc_I4_0));
        a.Append(a.Create(OpCodes.Stloc_1));
        var aTry = a.Create(OpCodes.Call, refresh);
        a.Append(aTry);
        a.Append(a.Create(OpCodes.Ldsfld, fKill));
        a.Append(a.Create(OpCodes.Brfalse, aEnd));
        a.Append(a.Create(OpCodes.Ldarga_S, tryAdjustM.Parameters[1]));
        a.Append(a.Create(OpCodes.Call, _mod.ImportReference(getValue)));
        a.Append(a.Create(OpCodes.Stloc_0));
        a.Append(a.Create(OpCodes.Ldloc_0));
        a.Append(a.Create(OpCodes.Ldc_I8, 0L));
        a.Append(a.Create(OpCodes.Bge, aEnd));
        a.Append(a.Create(OpCodes.Ldarg_0));
        a.Append(a.Create(OpCodes.Call, campOf));
        a.Append(a.Create(OpCodes.Brtrue, aEnd));
        // adjusted = new JInt(-self._curHP.Value - 1)
        a.Append(a.Create(OpCodes.Ldarg_3));
        a.Append(a.Create(OpCodes.Ldarg_0));
        a.Append(a.Create(OpCodes.Ldflda, _mod.ImportReference(curHp)));
        a.Append(a.Create(OpCodes.Call, _mod.ImportReference(getValue)));
        a.Append(a.Create(OpCodes.Neg));
        a.Append(a.Create(OpCodes.Ldc_I8, 1L));
        a.Append(a.Create(OpCodes.Sub));
        a.Append(a.Create(OpCodes.Newobj, _mod.ImportReference(ctorLong)));
        a.Append(a.Create(OpCodes.Stobj, _mod.ImportReference(jint)));
        a.Append(a.Create(OpCodes.Ldc_I4_1));
        a.Append(a.Create(OpCodes.Stloc_1));
        var aTryEnd = a.Create(OpCodes.Leave, aEnd); a.Append(aTryEnd);
        var aCatch = a.Create(OpCodes.Pop); a.Append(aCatch);
        a.Append(a.Create(OpCodes.Ldc_I4_0));
        a.Append(a.Create(OpCodes.Stloc_1));
        a.Append(a.Create(OpCodes.Leave, aEnd));
        var aCatchEnd = a.Create(OpCodes.Nop); a.Append(aCatchEnd);
        a.Append(aEnd);
        a.Append(a.Create(OpCodes.Ret));
        tryAdjustM.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        { CatchType = exType, TryStart = aTry, TryEnd = aTryEnd, HandlerStart = aCatch, HandlerEnd = aCatchEnd });

        return t;
    }
}
