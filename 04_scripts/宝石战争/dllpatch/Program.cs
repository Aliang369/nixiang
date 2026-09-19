using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class Program
{
    private const string RuntimeLibrary = "starry_magic";
    private const string ExportPrefix = "StarryGw_";
    // StateMirror（菜单 Java 侧）写出的状态镜像文件：单行，feature 0..7 空格分隔。
    private const string StatePath = "/data/data/com.acing.gow.adv/files/starry_state.txt";

    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: BszzDllPatch <input.dll> <output.dll> [dependency-dir...]");
            return 2;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0])));
        for (var i = 2; i < args.Length; i++)
        {
            resolver.AddSearchDirectory(args[i]);
        }
        var readerParameters = new ReaderParameters { AssemblyResolver = resolver };

        var assembly = AssemblyDefinition.ReadAssembly(args[0], readerParameters);
        var module = assembly.MainModule;
        var bridge = BuildBridge(module);

        var playerType = RequireType(module, "PuzzlePlayer");
        var troopType = RequireType(module, "PuzzleTroop");
        var playerIndexField = RequireField(module, playerType, "PlayerIndex");
        var getPuzzlePlayer = RequireMethod(troopType, "get_PuzzlePlayer", 0);

        var adjustHealth = RequireMethod(playerType, "AdjustHealth", 12);
        InjectAdjustDamage(module, adjustHealth, playerIndexField, bridge.SafeAdjustDamage);

        var setMana = RequireMethod(troopType, "SetMana", 1);
        InjectKeepMana(module, setMana, getPuzzlePlayer, playerIndexField, bridge.SafeShouldKeepMana);

        // 无限技能原实现只忽略 SetMana(0)，开局 Init 直接写 Mana=0 时不会补满，
        // 所以玩家仍要消块攒第一次能量。这里同时覆盖显示刷新与可用性查询：
        // 玩家侧在无沉默/技能封锁时保持 Mana=ManaRequired，敌方与状态效果边界不变。
        var manaField = RequireField(module, troopType, "Mana");
        var manaRequiredField = RequireField(module, troopType, "ManaRequired");
        var hasSilence = RequireMethod(troopType, "HasSilence", 0);
        var hasSpellDisable = RequireMethod(troopType, "HasSpellDisable", 0);
        var updateManaDisplay = RequireMethod(troopType, "UpdateManaDisplay", 0);
        var canCastSpell = RequireMethod(troopType, "CanCastSpell", 0);
        InjectFullMana(module, updateManaDisplay, getPuzzlePlayer, playerIndexField,
            bridge.SafeShouldKeepMana, manaField, manaRequiredField, hasSilence, hasSpellDisable, null);
        InjectFullMana(module, canCastSpell, getPuzzlePlayer, playerIndexField,
            bridge.SafeShouldKeepMana, manaField, manaRequiredField, hasSilence, hasSpellDisable, updateManaDisplay);

        foreach (var method in troopType.Methods.Where(m => m.Name == "SetHealth" && (m.Parameters.Count == 1 || m.Parameters.Count == 2)))
        {
            InjectHealthValueGuard(module, method, getPuzzlePlayer, playerIndexField, bridge.SafeAdjustHealthValue);
        }

        var runAway = troopType.Methods.FirstOrDefault(m => m.Name == "RunAway" && m.Parameters.Count == 3);
        if (runAway != null)
        {
            InjectRunAwayGuard(module, runAway, getPuzzlePlayer, playerIndexField, bridge.SafeShouldBlockRunAway);
        }

        assembly.Write(args[1]);
        Console.WriteLine($"patched {args[0]} -> {args[1]}");
        var sidecar = System.IO.Path.ChangeExtension(args[1], null) + ".pinvoke.json";
        var entries = new[] { "AdjustDamage", "ShouldKeepMana", "AdjustHealthValue", "ShouldBlockRunAway" }
            .Select(n => ExportPrefix + n).ToArray();
        System.IO.File.WriteAllText(sidecar,
            "{\n  \"library\": \"" + RuntimeLibrary + "\",\n  \"entry_points\": [\n    \"" +
            string.Join("\",\n    \"", entries) + "\"\n  ]\n}\n");
        Console.WriteLine($"pinvoke sidecar: {sidecar}");
        foreach (var entry in entries) Console.WriteLine($"pinvoke entry: {entry}");
        Console.WriteLine($"AdjustDamage/Call={bridge.AdjustDamage.FullName}");
        Console.WriteLine($"SetMana/Call={bridge.ShouldKeepMana.FullName}");
        Console.WriteLine($"SetHealth/Call={bridge.AdjustHealthValue.FullName}");
        return 0;
    }

    private sealed class Bridge
    {
        public MethodReference AdjustDamage;
        public MethodReference ShouldKeepMana;
        public MethodReference AdjustHealthValue;
        public MethodReference ShouldBlockRunAway;
        public MethodReference SafeAdjustDamage;
        public MethodReference SafeShouldKeepMana;
        public MethodReference SafeAdjustHealthValue;
        public MethodReference SafeShouldBlockRunAway;
    }

    private static Bridge BuildBridge(ModuleDefinition module)
    {
        var existing = module.Types.FirstOrDefault(t => t.Namespace == "StarryMagic" && t.Name == "StarryGwBridge");
        if (existing != null)
        {
            // 二次补丁场景：已存在的桥接类型也要把入口名归一化到真实导出符号。
            var existingNative = module.ModuleReferences.FirstOrDefault(r => r.Name == RuntimeLibrary)
                ?? new ModuleReference(RuntimeLibrary);
            if (!module.ModuleReferences.Contains(existingNative))
            {
                module.ModuleReferences.Add(existingNative);
            }
            foreach (var m in existing.Methods.Where(m => m.HasPInvokeInfo))
            {
                m.PInvokeInfo = new PInvokeInfo(PInvokeAttributes.CallConvCdecl, ExportPrefix + m.Name, existingNative);
            }
            var existingRaw = new Bridge
            {
                AdjustDamage = existing.Methods.Single(m => m.Name == "AdjustDamage"),
                ShouldKeepMana = existing.Methods.Single(m => m.Name == "ShouldKeepMana"),
                AdjustHealthValue = existing.Methods.Single(m => m.Name == "AdjustHealthValue"),
                ShouldBlockRunAway = existing.Methods.Single(m => m.Name == "ShouldBlockRunAway"),
            };
            return AddSafeWrappers(module, existing, existingRaw);
        }

        var bridge = new TypeDefinition("StarryMagic", "StarryGwBridge",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);
        module.Types.Add(bridge);

        var nativeModule = new ModuleReference(RuntimeLibrary);
        module.ModuleReferences.Add(nativeModule);

        var raw = new Bridge
        {
            AdjustDamage = AddPInvoke(bridge, nativeModule, "AdjustDamage", module.TypeSystem.Int32,
                new[] { module.TypeSystem.Int32, module.TypeSystem.Int32 }),
            ShouldKeepMana = AddPInvoke(bridge, nativeModule, "ShouldKeepMana", module.TypeSystem.Int32,
                new[] { module.TypeSystem.Int32 }),
            AdjustHealthValue = AddPInvoke(bridge, nativeModule, "AdjustHealthValue", module.TypeSystem.Int32,
                new[] { module.TypeSystem.Int32, module.TypeSystem.Int32 }),
            ShouldBlockRunAway = AddPInvoke(bridge, nativeModule, "ShouldBlockRunAway", module.TypeSystem.Int32,
                new[] { module.TypeSystem.Int32 }),
        };
        return AddSafeWrappers(module, bridge, raw);
    }

    private static MethodReference AddPInvoke(TypeDefinition bridge, ModuleReference nativeModule,
        string name, TypeReference returnType, IEnumerable<TypeReference> argumentTypes)
    {
        var method = new MethodDefinition(name,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | (MethodAttributes)0x2000,
            returnType);
        foreach (var argumentType in argumentTypes)
        {
            method.Parameters.Add(new ParameterDefinition(argumentType));
        }
        method.ImplAttributes = MethodImplAttributes.PreserveSig;
        // 入口名必须与 libstarry_magic.so 的 C 导出符号一致（StarryGw_*），
        // 否则运行期 dlsym 失败抛 EntryPointNotFoundException，战斗协程直接被打断。
        method.PInvokeInfo = new PInvokeInfo(PInvokeAttributes.CallConvCdecl, ExportPrefix + name, nativeModule);
        bridge.Methods.Add(method);
        return method;
    }

    // ------------------------------------------------------------------
    // 状态通道（2026-09-16 实测后定型）：不再调 DllImport —— HybridCLR 解释执行的热更代码
    // 里 DllImport 解析不到符号，调用必抛异常（旧版就是把战斗协程打断，表现为卡死）。
    // 改读 StateMirror（菜单 Java 侧）写出的镜像文件：state = mult + god*1000 + mana*2000。
    // 全部只用 File.ReadAllText / String.Split / int.Parse（游戏热更代码自身也在用）。
    // ------------------------------------------------------------------
    private static Bridge AddSafeWrappers(ModuleDefinition module, TypeDefinition bridge, Bridge raw)
    {
        var state = bridge.Methods.FirstOrDefault(m => m.Name == "StarryState" && m.Parameters.Count == 0)
            ?? AddStateReader(module, bridge);
        var exceptionType = new TypeReference("System", "Exception", module, module.TypeSystem.CoreLibrary);
        raw.SafeAdjustDamage = bridge.Methods.FirstOrDefault(m => m.Name == "SafeAdjustDamage" && m.Parameters.Count == 2)
            ?? AddSafeAdjustDamage(module, bridge, state, exceptionType);
        raw.SafeShouldKeepMana = bridge.Methods.FirstOrDefault(m => m.Name == "SafeShouldKeepMana" && m.Parameters.Count == 1)
            ?? AddSafeShouldKeepMana(module, bridge, state, exceptionType);
        raw.SafeAdjustHealthValue = bridge.Methods.FirstOrDefault(m => m.Name == "SafeAdjustHealthValue" && m.Parameters.Count == 2)
            ?? AddSafeAdjustHealthValue(module, bridge, state, exceptionType);
        raw.SafeShouldBlockRunAway = bridge.Methods.FirstOrDefault(m => m.Name == "SafeShouldBlockRunAway" && m.Parameters.Count == 1)
            ?? AddSafeShouldBlockRunAway(module, bridge, state, exceptionType);
        return raw;
    }

    private static MethodReference AddSafeWrapper(ModuleDefinition module, TypeDefinition bridge, MethodReference target,
        string name, bool fallbackFromFirstArgument, TypeReference exceptionType)
    {
        var method = new MethodDefinition(name,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            module.TypeSystem.Int32);
        foreach (var parameter in target.Parameters)
        {
            method.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
        }
        bridge.Methods.Add(method);

        var body = method.Body;
        body.InitLocals = true;
        var result = new VariableDefinition(module.TypeSystem.Int32);
        body.Variables.Add(result);
        var caught = new VariableDefinition(module.ImportReference(exceptionType));
        body.Variables.Add(caught);

        var end = Instruction.Create(OpCodes.Ldloc, result);
        var ret = Instruction.Create(OpCodes.Ret);

        var tryStart = Instruction.Create(OpCodes.Ldarg_0);
        var trySecond = target.Parameters.Count > 1 ? Instruction.Create(OpCodes.Ldarg_1) : null;
        var callTarget = Instruction.Create(OpCodes.Call, module.ImportReference(target));
        var storeResult = Instruction.Create(OpCodes.Stloc, result);
        var leaveTry = Instruction.Create(OpCodes.Leave, end);

        var handlerStart = Instruction.Create(OpCodes.Stloc, caught);
        var fallback = fallbackFromFirstArgument
            ? Instruction.Create(OpCodes.Ldarg_0)
            : Instruction.Create(OpCodes.Ldc_I4_0);
        var storeFallback = Instruction.Create(OpCodes.Stloc, result);
        var leaveHandler = Instruction.Create(OpCodes.Leave, end);

        var il = body.GetILProcessor();
        il.Append(tryStart);
        if (trySecond != null)
        {
            il.Append(trySecond);
        }
        il.Append(callTarget);
        il.Append(storeResult);
        il.Append(leaveTry);
        il.Append(handlerStart);
        il.Append(fallback);
        il.Append(storeFallback);
        il.Append(leaveHandler);
        il.Append(end);
        il.Append(ret);

        body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = module.ImportReference(exceptionType),
            TryStart = tryStart,
            TryEnd = handlerStart,
            HandlerStart = handlerStart,
            HandlerEnd = end,
        });
        return method;
    }

    private static void Append(ILProcessor il, params Instruction[] instructions)
    {
        foreach (var instruction in instructions)
        {
            il.Append(instruction);
        }
    }

    private static MethodReference BclStatic(ModuleDefinition module, string ns, string type, string name,
        TypeReference returnType, params TypeReference[] arguments)
    {
        var declaring = new TypeReference(ns, type, module, module.TypeSystem.CoreLibrary);
        var method = new MethodReference(name, returnType, declaring) { HasThis = false };
        foreach (var argument in arguments)
        {
            method.Parameters.Add(new ParameterDefinition(argument));
        }
        return module.ImportReference(method);
    }

    private static MethodReference BclInstance(ModuleDefinition module, TypeReference declaring,
        string name, TypeReference returnType, params TypeReference[] arguments)
    {
        var method = new MethodReference(name, returnType, declaring) { HasThis = true };
        foreach (var argument in arguments)
        {
            method.Parameters.Add(new ParameterDefinition(argument));
        }
        return module.ImportReference(method);
    }

    /** int StarryState()：读镜像文件并打包成 mult + god*1000 + mana*2000（文件缺失/格式错就抛，由 caller 兜底）。 */
    private static MethodDefinition AddStateReader(ModuleDefinition module, TypeDefinition bridge)
    {
        var int32 = module.TypeSystem.Int32;
        var stringType = module.TypeSystem.String;
        var stringArray = new ArrayType(stringType);

        var method = new MethodDefinition("StarryState",
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, int32);
        bridge.Methods.Add(method);

        var parts = new VariableDefinition(stringArray);
        method.Body.Variables.Add(parts);
        method.Body.InitLocals = true;

        var readAllText = BclStatic(module, "System.IO", "File", "ReadAllText", stringType, stringType);
        var split = BclInstance(module, stringType, "Split", stringArray, new ArrayType(module.TypeSystem.Char));
        var parse = BclStatic(module, "System", "Int32", "Parse", int32, stringType);

        var il = method.Body.GetILProcessor();
        Append(il,
            Instruction.Create(OpCodes.Ldstr, StatePath),
            Instruction.Create(OpCodes.Call, readAllText),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Newarr, module.TypeSystem.Char),
            Instruction.Create(OpCodes.Dup),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)32),
            Instruction.Create(OpCodes.Stelem_I2),
            Instruction.Create(OpCodes.Callvirt, split),
            Instruction.Create(OpCodes.Stloc, parts));
        // mult
        Append(il,
            Instruction.Create(OpCodes.Ldloc, parts),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ldelem_Ref),
            Instruction.Create(OpCodes.Call, parse));
        // + god * 1000
        Append(il,
            Instruction.Create(OpCodes.Ldloc, parts),
            Instruction.Create(OpCodes.Ldc_I4_2),
            Instruction.Create(OpCodes.Ldelem_Ref),
            Instruction.Create(OpCodes.Call, parse),
            Instruction.Create(OpCodes.Ldc_I4, 1000),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Add));
        // + mana * 2000
        Append(il,
            Instruction.Create(OpCodes.Ldloc, parts),
            Instruction.Create(OpCodes.Ldc_I4_3),
            Instruction.Create(OpCodes.Ldelem_Ref),
            Instruction.Create(OpCodes.Call, parse),
            Instruction.Create(OpCodes.Ldc_I4, 2000),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Add),
            Instruction.Create(OpCodes.Ret));
        return method;
    }

    /** (state / 1000) % 2 == 1：上帝模式开关。 */
    private static void EmitIsGod(ILProcessor il, VariableDefinition state)
    {
        Append(il,
            Instruction.Create(OpCodes.Ldloc, state),
            Instruction.Create(OpCodes.Ldc_I4, 1000),
            Instruction.Create(OpCodes.Div),
            Instruction.Create(OpCodes.Ldc_I4_2),
            Instruction.Create(OpCodes.Rem),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ceq));
    }

    private static MethodReference AddSafeAdjustDamage(ModuleDefinition module, TypeDefinition bridge,
        MethodReference stateReader, TypeReference exceptionType)
    {
        var int32 = module.TypeSystem.Int32;
        var method = new MethodDefinition("SafeAdjustDamage",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, int32);
        method.Parameters.Add(new ParameterDefinition(int32));
        method.Parameters.Add(new ParameterDefinition(int32));
        bridge.Methods.Add(method);

        var body = method.Body;
        body.InitLocals = true;
        var result = new VariableDefinition(int32);
        var state = new VariableDefinition(int32);
        var mult = new VariableDefinition(int32);
        var scaled = new VariableDefinition(int32);
        var caught = new VariableDefinition(module.ImportReference(exceptionType));
        body.Variables.Add(result);
        body.Variables.Add(state);
        body.Variables.Add(mult);
        body.Variables.Add(scaled);
        body.Variables.Add(caught);

        var end = Instruction.Create(OpCodes.Ldloc, result);
        var doneTry = Instruction.Create(OpCodes.Nop);
        var checkTarget1 = Instruction.Create(OpCodes.Nop);
        var minCheck = Instruction.Create(OpCodes.Nop);
        var storeScaled = Instruction.Create(OpCodes.Nop);

        var il = body.GetILProcessor();
        Append(il,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Call, module.ImportReference(stateReader)),
            Instruction.Create(OpCodes.Stloc, state),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Bge_S, doneTry),
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Bne_Un_S, checkTarget1));
        EmitIsGod(il, state);
        Append(il,
            Instruction.Create(OpCodes.Brfalse_S, doneTry),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Br_S, doneTry));
        il.Append(checkTarget1);
        Append(il,
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Bne_Un_S, doneTry),
            Instruction.Create(OpCodes.Ldloc, state),
            Instruction.Create(OpCodes.Ldc_I4, 1000),
            Instruction.Create(OpCodes.Rem),
            Instruction.Create(OpCodes.Stloc, mult),
            Instruction.Create(OpCodes.Ldloc, mult),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Ble_S, doneTry),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldloc, mult),
            Instruction.Create(OpCodes.Mul),
            Instruction.Create(OpCodes.Stloc, scaled),
            Instruction.Create(OpCodes.Ldloc, scaled),
            Instruction.Create(OpCodes.Ldc_I4, 100000000),
            Instruction.Create(OpCodes.Ble_S, minCheck),
            Instruction.Create(OpCodes.Ldc_I4, 100000000),
            Instruction.Create(OpCodes.Stloc, scaled));
        il.Append(minCheck);
        Append(il,
            Instruction.Create(OpCodes.Ldloc, scaled),
            Instruction.Create(OpCodes.Ldc_I4, 100000000),
            Instruction.Create(OpCodes.Neg),
            Instruction.Create(OpCodes.Bge_S, storeScaled),
            Instruction.Create(OpCodes.Ldc_I4, 100000000),
            Instruction.Create(OpCodes.Neg),
            Instruction.Create(OpCodes.Stloc, scaled));
        il.Append(storeScaled);
        Append(il,
            Instruction.Create(OpCodes.Ldloc, scaled),
            Instruction.Create(OpCodes.Stloc, result));
        il.Append(doneTry);
        il.Append(Instruction.Create(OpCodes.Leave, end));

        var handlerStart = Instruction.Create(OpCodes.Stloc, caught);
        il.Append(handlerStart);
        Append(il,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Leave, end));
        il.Append(end);
        il.Append(Instruction.Create(OpCodes.Ret));

        body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = module.ImportReference(exceptionType),
            TryStart = body.Instructions[0],
            TryEnd = handlerStart,
            HandlerStart = handlerStart,
            HandlerEnd = end,
        });
        return method;
    }

    private static MethodReference AddSafeShouldKeepMana(ModuleDefinition module, TypeDefinition bridge,
        MethodReference stateReader, TypeReference exceptionType)
    {
        var int32 = module.TypeSystem.Int32;
        var method = new MethodDefinition("SafeShouldKeepMana",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, int32);
        method.Parameters.Add(new ParameterDefinition(int32));
        bridge.Methods.Add(method);

        var body = method.Body;
        body.InitLocals = true;
        var result = new VariableDefinition(int32);
        var state = new VariableDefinition(int32);
        var caught = new VariableDefinition(module.ImportReference(exceptionType));
        body.Variables.Add(result);
        body.Variables.Add(state);
        body.Variables.Add(caught);

        var end = Instruction.Create(OpCodes.Ldloc, result);
        var doneTry = Instruction.Create(OpCodes.Nop);
        var il = body.GetILProcessor();
        Append(il,
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Call, module.ImportReference(stateReader)),
            Instruction.Create(OpCodes.Stloc, state),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Brtrue_S, doneTry),
            Instruction.Create(OpCodes.Ldloc, state),
            Instruction.Create(OpCodes.Ldc_I4, 2000),
            Instruction.Create(OpCodes.Blt_S, doneTry),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Stloc, result));
        il.Append(doneTry);
        il.Append(Instruction.Create(OpCodes.Leave, end));

        var handlerStart = Instruction.Create(OpCodes.Stloc, caught);
        il.Append(handlerStart);
        Append(il,
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Leave, end));
        il.Append(end);
        il.Append(Instruction.Create(OpCodes.Ret));

        body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = module.ImportReference(exceptionType),
            TryStart = body.Instructions[0],
            TryEnd = handlerStart,
            HandlerStart = handlerStart,
            HandlerEnd = end,
        });
        return method;
    }

    private static MethodReference AddSafeAdjustHealthValue(ModuleDefinition module, TypeDefinition bridge,
        MethodReference stateReader, TypeReference exceptionType)
    {
        var int32 = module.TypeSystem.Int32;
        var method = new MethodDefinition("SafeAdjustHealthValue",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, int32);
        method.Parameters.Add(new ParameterDefinition(int32));
        method.Parameters.Add(new ParameterDefinition(int32));
        bridge.Methods.Add(method);

        var body = method.Body;
        body.InitLocals = true;
        var result = new VariableDefinition(int32);
        var state = new VariableDefinition(int32);
        var caught = new VariableDefinition(module.ImportReference(exceptionType));
        body.Variables.Add(result);
        body.Variables.Add(state);
        body.Variables.Add(caught);

        var end = Instruction.Create(OpCodes.Ldloc, result);
        var doneTry = Instruction.Create(OpCodes.Nop);
        var il = body.GetILProcessor();
        Append(il,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Call, module.ImportReference(stateReader)),
            Instruction.Create(OpCodes.Stloc, state),
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Brtrue_S, doneTry),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Bgt_S, doneTry));
        EmitIsGod(il, state);
        Append(il,
            Instruction.Create(OpCodes.Brfalse_S, doneTry),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Stloc, result));
        il.Append(doneTry);
        il.Append(Instruction.Create(OpCodes.Leave, end));

        var handlerStart = Instruction.Create(OpCodes.Stloc, caught);
        il.Append(handlerStart);
        Append(il,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Leave, end));
        il.Append(end);
        il.Append(Instruction.Create(OpCodes.Ret));

        body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = module.ImportReference(exceptionType),
            TryStart = body.Instructions[0],
            TryEnd = handlerStart,
            HandlerStart = handlerStart,
            HandlerEnd = end,
        });
        return method;
    }

    private static MethodReference AddSafeShouldBlockRunAway(ModuleDefinition module, TypeDefinition bridge,
        MethodReference stateReader, TypeReference exceptionType)
    {
        var int32 = module.TypeSystem.Int32;
        var method = new MethodDefinition("SafeShouldBlockRunAway",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, int32);
        method.Parameters.Add(new ParameterDefinition(int32));
        bridge.Methods.Add(method);

        var body = method.Body;
        body.InitLocals = true;
        var result = new VariableDefinition(int32);
        var state = new VariableDefinition(int32);
        var caught = new VariableDefinition(module.ImportReference(exceptionType));
        body.Variables.Add(result);
        body.Variables.Add(state);
        body.Variables.Add(caught);

        var end = Instruction.Create(OpCodes.Ldloc, result);
        var doneTry = Instruction.Create(OpCodes.Nop);
        var il = body.GetILProcessor();
        Append(il,
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Call, module.ImportReference(stateReader)),
            Instruction.Create(OpCodes.Stloc, state),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Brtrue_S, doneTry));
        EmitIsGod(il, state);
        Append(il,
            Instruction.Create(OpCodes.Brfalse_S, doneTry),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Stloc, result));
        il.Append(doneTry);
        il.Append(Instruction.Create(OpCodes.Leave, end));

        var handlerStart = Instruction.Create(OpCodes.Stloc, caught);
        il.Append(handlerStart);
        Append(il,
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Stloc, result),
            Instruction.Create(OpCodes.Leave, end));
        il.Append(end);
        il.Append(Instruction.Create(OpCodes.Ret));

        body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = module.ImportReference(exceptionType),
            TryStart = body.Instructions[0],
            TryEnd = handlerStart,
            HandlerStart = handlerStart,
            HandlerEnd = end,
        });
        return method;
    }

    private static TypeDefinition RequireType(ModuleDefinition module, string name)
    {
        var type = module.Types.FirstOrDefault(t => t.Name == name);
        if (type == null) throw new InvalidOperationException($"type not found: {name}");
        return type;
    }

    private static FieldReference RequireField(ModuleDefinition module, TypeDefinition owner, string name)
    {
        var field = owner.Fields.FirstOrDefault(f => f.Name == name);
        if (field == null) throw new InvalidOperationException($"field not found: {owner.FullName}.{name}");
        return module.ImportReference(field);
    }

    private static MethodDefinition RequireMethod(TypeDefinition owner, string name, int parameterCount)
    {
        var method = owner.Methods.FirstOrDefault(m => m.Name == name && m.Parameters.Count == parameterCount);
        if (method == null) throw new InvalidOperationException($"method not found: {owner.FullName}.{name}/{parameterCount}");
        return method;
    }

    private static void InjectAdjustDamage(ModuleDefinition module, MethodDefinition method,
        FieldReference playerIndexField, MethodReference bridgeMethod)
    {
        if (AlreadyCalls(method, bridgeMethod))
        {
            return;
        }
        var il = method.Body.GetILProcessor();
        var first = method.Body.Instructions[0];
        var sequence = new[]
        {
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, playerIndexField),
            Instruction.Create(OpCodes.Call, module.ImportReference(bridgeMethod)),
            Instruction.Create(OpCodes.Starg, method.Parameters[0]),
        };
        foreach (var ins in sequence)
        {
            il.InsertBefore(first, ins);
        }
    }

    private static void InjectKeepMana(ModuleDefinition module, MethodDefinition method,
        MethodReference getPuzzlePlayer, FieldReference playerIndexField, MethodReference bridgeMethod)
    {
        if (AlreadyCalls(method, bridgeMethod))
        {
            return;
        }
        var il = method.Body.GetILProcessor();
        var first = method.Body.Instructions[0];
        var ret = Instruction.Create(OpCodes.Ret);
        var toOriginal = Instruction.Create(OpCodes.Brtrue_S, first);
        var toOriginalNull = Instruction.Create(OpCodes.Brfalse_S, first);
        var toRet = Instruction.Create(OpCodes.Brtrue_S, ret);

        var sequence = new[]
        {
            Instruction.Create(OpCodes.Ldarg_1),
            toOriginal,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            toOriginalNull,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            Instruction.Create(OpCodes.Ldfld, playerIndexField),
            Instruction.Create(OpCodes.Call, module.ImportReference(bridgeMethod)),
            toRet,
        };
        foreach (var ins in sequence)
        {
            il.InsertBefore(first, ins);
        }
        il.Append(ret);
    }

    private static void InjectHealthValueGuard(ModuleDefinition module, MethodDefinition method,
        MethodReference getPuzzlePlayer, FieldReference playerIndexField, MethodReference bridgeMethod)
    {
        if (AlreadyCalls(method, bridgeMethod))
        {
            return;
        }
        var il = method.Body.GetILProcessor();
        var first = method.Body.Instructions[0];
        var playerIndex = new VariableDefinition(module.TypeSystem.Int32);
        method.Body.Variables.Add(playerIndex);
        method.Body.InitLocals = true;

        var afterPlayerIndex = Instruction.Create(OpCodes.Nop);
        var sequence = new[]
        {
            Instruction.Create(OpCodes.Ldc_I4_M1),
            Instruction.Create(OpCodes.Stloc, playerIndex),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            Instruction.Create(OpCodes.Brfalse_S, afterPlayerIndex),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            Instruction.Create(OpCodes.Ldfld, playerIndexField),
            Instruction.Create(OpCodes.Stloc, playerIndex),
            afterPlayerIndex,
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Ldloc, playerIndex),
            Instruction.Create(OpCodes.Call, module.ImportReference(bridgeMethod)),
            Instruction.Create(OpCodes.Starg, method.Parameters[0]),
        };
        foreach (var ins in sequence)
        {
            il.InsertBefore(first, ins);
        }
    }

    private static void InjectRunAwayGuard(ModuleDefinition module, MethodDefinition method,
        MethodReference getPuzzlePlayer, FieldReference playerIndexField, MethodReference bridgeMethod)
    {
        if (AlreadyCalls(method, bridgeMethod))
        {
            return;
        }
        var il = method.Body.GetILProcessor();
        var first = method.Body.Instructions[0];
        var ret = Instruction.Create(OpCodes.Ret);
        var toOriginal = Instruction.Create(OpCodes.Brfalse_S, first);
        var toOriginalAgain = Instruction.Create(OpCodes.Brfalse_S, first);

        var sequence = new[]
        {
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            toOriginal,
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            Instruction.Create(OpCodes.Ldfld, playerIndexField),
            Instruction.Create(OpCodes.Call, module.ImportReference(bridgeMethod)),
            toOriginalAgain,
            ret,
        };
        foreach (var ins in sequence)
        {
            il.InsertBefore(first, ins);
        }
    }

    private static void InjectFullMana(ModuleDefinition module, MethodDefinition method,
        MethodReference getPuzzlePlayer, FieldReference playerIndexField, MethodReference bridgeMethod,
        FieldReference manaField, FieldReference manaRequiredField,
        MethodReference hasSilence, MethodReference hasSpellDisable, MethodReference updateManaDisplay)
    {
        if (AlreadyCalls(method, bridgeMethod))
        {
            return;
        }

        var il = method.Body.GetILProcessor();
        var first = method.Body.Instructions[0];
        var playerIndex = new VariableDefinition(module.TypeSystem.Int32);
        method.Body.Variables.Add(playerIndex);
        method.Body.InitLocals = true;

        var afterPlayerIndex = Instruction.Create(OpCodes.Nop);
        var done = Instruction.Create(OpCodes.Nop);
        var sequence = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldc_I4_M1),
            Instruction.Create(OpCodes.Stloc, playerIndex),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            Instruction.Create(OpCodes.Brfalse_S, afterPlayerIndex),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(getPuzzlePlayer)),
            Instruction.Create(OpCodes.Ldfld, playerIndexField),
            Instruction.Create(OpCodes.Stloc, playerIndex),
            afterPlayerIndex,
            Instruction.Create(OpCodes.Ldloc, playerIndex),
            Instruction.Create(OpCodes.Call, module.ImportReference(bridgeMethod)),
            Instruction.Create(OpCodes.Brfalse_S, done),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(hasSilence)),
            Instruction.Create(OpCodes.Brtrue_S, done),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, module.ImportReference(hasSpellDisable)),
            Instruction.Create(OpCodes.Brtrue_S, done),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, manaRequiredField),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Ble_S, done),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, manaField),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, manaRequiredField),
            Instruction.Create(OpCodes.Bge_S, done),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, manaRequiredField),
            Instruction.Create(OpCodes.Stfld, manaField),
        };
        if (updateManaDisplay != null)
        {
            sequence.Add(Instruction.Create(OpCodes.Ldarg_0));
            sequence.Add(Instruction.Create(OpCodes.Call, module.ImportReference(updateManaDisplay)));
        }
        sequence.Add(done);

        foreach (var ins in sequence)
        {
            il.InsertBefore(first, ins);
        }
    }

    private static bool AlreadyCalls(MethodDefinition method, MethodReference target)
    {
        return method.Body.Instructions
            .Take(64)
            .Any(i => (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
                && i.Operand is MethodReference called
                && called.FullName == target.FullName);
    }
}
