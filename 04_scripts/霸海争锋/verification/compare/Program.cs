using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// 霸海争锋 b8 离线 body 对照（游戏私有，只读）
// 用法: bhzf_compare <original.dll> <patched.dll> [prefixLen]
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 2) { Console.Error.WriteLine("usage: bhzf_compare <original> <patched> [prefixLen]"); return 2; }
        var origPath = args[0]; var patchPath = args[1];
        int prefixLen = args.Length > 2 ? int.Parse(args[2]) : 16;

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(origPath)));
        resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(patchPath)));
        resolver.AddSearchDirectory("/Users/aliang/Documents/nixiang/06_dumped/霸海争锋/1.1.036_il2cpp/dump/DummyDll");
        var o = AssemblyDefinition.ReadAssembly(origPath, new ReaderParameters { AssemblyResolver = resolver });
        var p = AssemblyDefinition.ReadAssembly(patchPath, new ReaderParameters { AssemblyResolver = resolver });

        var om = Find(o); var pm = Find(p);
        if (om == null || pm == null) { Console.WriteLine("AddHP(JInt,Life,int) 未找到"); return 1; }

        var oi = om.Body.Instructions.ToList();
        var pi = pm.Body.Instructions.ToList();
        Console.WriteLine($"original: {oi.Count} IL, locals={om.Body.Variables.Count}, EH={om.Body.ExceptionHandlers.Count}");
        Console.WriteLine($"patched : {pi.Count} IL, locals={pm.Body.Variables.Count}, EH={pm.Body.ExceptionHandlers.Count}");
        Console.WriteLine($"maxstack: original={om.Body.MaxStackSize} patched={pm.Body.MaxStackSize}");
        Console.WriteLine($"initlocals: original={om.Body.InitLocals} patched={pm.Body.InitLocals}");

        int start = pi.Count - oi.Count;
        Console.WriteLine($"prefix(推定)={start}  期望={prefixLen}  {(start == prefixLen ? "MATCH" : "DIFF")}");
        if (start < 0) { Console.WriteLine("patched 指令数少于 original"); return 1; }

        var suffix = pi.Skip(start).ToList();
        int n = Math.Min(suffix.Count, oi.Count);
        int opDiff = 0, operandDiff = 0, branchDiff = 0;
        for (int k = 0; k < n; k++)
        {
            var a = oi[k]; var b = suffix[k];
            var na = Norm(a, oi, null); var nb = Norm(b, suffix, null);
            if (na.op != nb.op) { opDiff++; if (opDiff <= 10) Console.WriteLine($"  opdiff @{k}: {na.op} != {nb.op}"); }
            if (na.operand != nb.operand) { operandDiff++; if (operandDiff <= 10) Console.WriteLine($"  opnddiff @{k}: {na.op} [{na.operand}] != [{nb.operand}]"); }
            if (na.target != nb.target) { branchDiff++; if (branchDiff <= 10) Console.WriteLine($"  brdiff @{k}: {na.target} != {nb.target}"); }
        }
        Console.WriteLine($"suffix 指令数: original={oi.Count} patched={suffix.Count} 比对={n}");
        Console.WriteLine($"counter: op={opDiff} operand={operandDiff} branch={branchDiff}");

        // locals：原 locals 逐项比对；patched 多出的列名
        int extraLocals = pm.Body.Variables.Count - om.Body.Variables.Count;
        bool localsPrefixSame = true;
        for (int i = 0; i < om.Body.Variables.Count && i < pm.Body.Variables.Count; i++)
            if (om.Body.Variables[i].VariableType.FullName != pm.Body.Variables[i].VariableType.FullName) { localsPrefixSame = false; Console.WriteLine($"  localdiff @{i}: {om.Body.Variables[i].VariableType.FullName} != {pm.Body.Variables[i].VariableType.FullName}"); }
        Console.WriteLine($"locals: original={om.Body.Variables.Count} patched={pm.Body.Variables.Count} extra={extraLocals} prefixSame={localsPrefixSame}");
        foreach (var v in pm.Body.Variables.Skip(om.Body.Variables.Count)) Console.WriteLine($"  new local: {v.VariableType.FullName}");

        // EH：按指令 index 映射比较
        string EhKey(MethodBody body, ExceptionHandler h)
            => $"{h.HandlerType}:{Idx(body, h.TryStart)}-{Idx(body, h.TryEnd)} h{Idx(body, h.HandlerStart)}-{Idx(body, h.HandlerEnd)} catch={h.CatchType?.FullName}";
        var oeh = om.Body.ExceptionHandlers.Select(h => EhKey(om.Body, h)).ToList();
        var peh = pm.Body.ExceptionHandlers.Select(h => EhKey(pm.Body, h)).ToList();
        Console.WriteLine($"EH: original={oeh.Count} patched={peh.Count}");
        foreach (var e in oeh) Console.WriteLine($"  O {e}");
        foreach (var e in peh) Console.WriteLine($"  P {e}");

        bool ok = start == prefixLen && opDiff == 0 && operandDiff == 0 && branchDiff == 0
                  && suffix.Count == oi.Count && localsPrefixSame && oeh.SequenceEqual(peh);
        Console.WriteLine(ok ? "VERDICT: SUFFIX-IDENTICAL" : "VERDICT: DIFFERENCES-FOUND");
        return ok ? 0 : 1;
    }

    private static int Idx(MethodBody body, Instruction ins)
    {
        if (ins == null) return -1;
        var list = body.Instructions.ToList();
        return list.IndexOf(ins);
    }

    private static (string op, string operand, string target) Norm(Instruction ins, List<Instruction> list, List<Instruction> other)
    {
        string op = ins.OpCode.Code.ToString();
        // short/long branch 统一
        op = op.Replace("_S", "").Replace(".s", "");
        string operand = ins.Operand switch
        {
            null => "",
            string s => "str:" + s,
            int i => "i4:" + i,
            long l => "i8:" + l,
            float f => "r4:" + f,
            double d => "r8:" + d,
            MethodReference mr => "m:" + mr.FullName,
            FieldReference fr => "f:" + fr.FullName,
            TypeReference tr => "t:" + tr.FullName,
            ParameterDefinition pd => "arg:" + pd.Index + ":" + pd.ParameterType.FullName,
            VariableDefinition vd => "loc:" + vd.Index + ":" + vd.VariableType.FullName,
            Instruction _ => "",
            _ => ins.Operand.GetType().Name
        };
        string target = "";
        if (ins.Operand is Instruction t) target = "idx:" + list.IndexOf(t);
        if (ins.Operand is Instruction[] arr) target = "idx:[" + string.Join(",", arr.Select(x => list.IndexOf(x))) + "]";
        return (op, operand, target);
    }

    private static MethodDefinition Find(AssemblyDefinition a)
        => a.MainModule.GetType("Battle.Life")?.Methods
            .FirstOrDefault(m => m.Name == "AddHP" && m.Parameters.Count == 3
                && m.Parameters[0].ParameterType.FullName == "JEngine.AntiCheat.JInt"
                && m.Parameters[1].ParameterType.FullName == "Battle.Life"
                && m.Parameters[2].ParameterType.FullName == "System.Int32"
                && m.ReturnType.FullName == "System.Void");
}
