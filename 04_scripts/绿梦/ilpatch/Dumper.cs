using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class Dumper
{
    public static void Find(AssemblyDefinition asm, string mRe, string tRe)
    {
        var mr = new Regex(mRe, RegexOptions.IgnoreCase);
        var tr = new Regex(tRe, RegexOptions.IgnoreCase);
        int n = 0;
        foreach (var t in asm.MainModule.GetTypes())
        {
            if (!tr.IsMatch(t.FullName)) continue;
            foreach (var m in t.Methods)
            {
                if (!mr.IsMatch(m.Name)) continue;
                var ps = string.Join(", ", m.Parameters.Select(p => p.ParameterType.Name + " " + p.Name));
                Console.WriteLine($"{t.FullName}::{m.Name}({ps}) -> {m.ReturnType.Name}  [IL={m.Body?.Instructions.Count ?? 0}]");
                n++;
            }
        }
        Console.WriteLine($"-- {n} 命中");
    }

    public static void DumpIL(AssemblyDefinition asm, string typeName, string methodName)
    {
        foreach (var t in asm.MainModule.GetTypes())
        {
            if (!string.Equals(t.FullName, typeName, StringComparison.OrdinalIgnoreCase) &&
                !t.FullName.EndsWith("." + typeName, StringComparison.OrdinalIgnoreCase)) continue;
            foreach (var m in t.Methods)
            {
                if (methodName != "*" && !string.Equals(m.Name, methodName, StringComparison.OrdinalIgnoreCase)) continue;
                Console.WriteLine($"==== {t.FullName}::{m.Name} ({m.Parameters.Count} params) -> {m.ReturnType.FullName}");
                if (!m.HasBody) { Console.WriteLine("  <no body>"); continue; }
                foreach (var v in m.Body.Variables) Console.WriteLine($"  .local {v.VariableType.Name} V{v.Index}");
                foreach (var i in m.Body.Instructions) Console.WriteLine($"  IL_{i.Offset:x4}: {i.OpCode,-12} {Fmt(i.Operand)}");
            }
        }
    }

    static string Fmt(object o) => o switch
    {
        null => "",
        string s => "\"" + s + "\"",
        Instruction i => $"IL_{i.Offset:x4}",
        Instruction[] arr => string.Join(", ", arr.Select(x => $"IL_{x.Offset:x4}")),
        _ => o.ToString()!
    };
}
