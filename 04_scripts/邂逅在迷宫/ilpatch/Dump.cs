using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class Dumper
{
    public static void DumpIL(AssemblyDefinition asm, string typeName, string methodName)
    {
        var t = asm.MainModule.GetType(typeName);
        if (t == null) { Console.WriteLine($"type {typeName} NOT FOUND"); return; }
        foreach (var m in t.Methods)
        {
            if (methodName != "*" && m.Name != methodName) continue;
            if (!m.HasBody) continue;
            Console.WriteLine($"==== {typeName}::{m.Name}  ret={m.ReturnType.Name} params=[{string.Join(",", m.Parameters.Select(p => p.ParameterType.Name))}]");
            int idx = 0;
            foreach (var v in m.Body.Variables)
                Console.WriteLine($"  local V{idx++}: {v.VariableType.Name}");
            foreach (var ins in m.Body.Instructions)
                Console.WriteLine($"  IL_{ins.Offset:X4}: {ins.OpCode,-12} {ins}");
            foreach (var h in m.Body.ExceptionHandlers)
                Console.WriteLine($"  EH: try IL_{h.TryStart.Offset:X4}.. handler IL_{h.HandlerStart.Offset:X4} type={h.HandlerType}");
            Console.WriteLine();
        }
    }
}
