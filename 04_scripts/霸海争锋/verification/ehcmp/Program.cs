using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

// 原 vs 克隆 单方法指令/EH 索引归一化对照（只读）
internal static class Program
{
    private static void Main(string[] a)
    {
        var p1 = a.Length > 0 ? a[0] : "/Users/aliang/Documents/nixiang/07_patched/霸海争锋/rc_b8/mygame_bhzf_dev_rc_b8.dll";
        var p2 = a.Length > 1 ? a[1] : "/tmp/bhzf_harness.dll";
        var orig = AssemblyDefinition.ReadAssembly(p1);
        var clone = AssemblyDefinition.ReadAssembly(p2);
        foreach (var name in new[] { "Refresh", "TryAdjust" })
        {
            var om = orig.MainModule.GetType("Starry.BhzfPatch").Methods.First(m => m.Name == name);
            var cm = clone.MainModule.GetType("Starry.BhzfPatch").Methods.First(m => m.Name == name);
            Console.WriteLine($"===== {name}");
            Dump("O", om);
            Dump("C", cm);
        }
    }

    private static void Dump(string tag, MethodDefinition m)
    {
        var ins = m.Body.Instructions.ToList();
        Console.WriteLine($"[{tag}] sig={m.FullName}");
        Console.WriteLine($"[{tag}] maxstack={m.Body.MaxStackSize} initlocals={m.Body.InitLocals} il={ins.Count} locals={string.Join(",", m.Body.Variables.Select(v => v.VariableType.FullName))}");
        foreach (var h in m.Body.ExceptionHandlers)
            Console.WriteLine($"[{tag}] EH {h.HandlerType} try={Idx(ins,h.TryStart)}..{Idx(ins,h.TryEnd)} h={Idx(ins,h.HandlerStart)}..{Idx(ins,h.HandlerEnd)} filter={Idx(ins,h.FilterStart)} catch={h.CatchType?.FullName ?? "null"}");
        for (int i = 0; i < ins.Count; i++)
        {
            var x = ins[i];
            string op = x.OpCode.Code.ToString();
            string operand = x.Operand switch
            {
                null => "",
                string s => "str:" + s,
                int v => "i4:" + v,
                long v => "i8:" + v,
                Instruction t => "->" + Idx(ins, t),
                Instruction[] arr => "->[" + string.Join(",", arr.Select(y => Idx(ins, y))) + "]",
                MethodReference mr => "m:" + mr.DeclaringType.Name + "::" + mr.Name + "/" + mr.Parameters.Count,
                FieldReference fr => "f:" + fr.DeclaringType.Name + "::" + fr.Name,
                TypeReference tr => "t:" + tr.Name,
                ParameterDefinition pd => "arg" + pd.Index,
                VariableDefinition vd => "v" + vd.Index,
                _ => "?"
            };
            Console.WriteLine($"[{tag}] {i,3} {op,-12} {operand}");
        }
    }

    private static string Idx(System.Collections.Generic.List<Instruction> l, Instruction i)
        => i == null ? "null" : l.IndexOf(i).ToString();
}
