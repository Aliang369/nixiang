using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class Dumper
{
    // 【枚举模式】按方法名/类型名正则列出成员签名，一次看全（避免对同一问题反复 dump）
    //   dotnet run -- --find <方法名正则> [<类型名正则>]
    public static void Find(AssemblyDefinition asm, string mRe, string tRe)
    {
        var mrx = new System.Text.RegularExpressions.Regex(mRe);
        var trx = new System.Text.RegularExpressions.Regex(tRe);
        int n = 0;
        foreach (var t in asm.MainModule.GetTypes())
        {
            if (!trx.IsMatch(t.FullName)) continue;
            foreach (var m in t.Methods)
            {
                if (!mrx.IsMatch(m.Name)) continue;
                var flags = new List<string>();
                if (m.IsStatic) flags.Add("static");
                if (m.IsVirtual) flags.Add("virtual");
                if (!m.HasBody) flags.Add("nobody");
                Console.WriteLine($"{t.FullName}::{m.Name}  ret={m.ReturnType.Name} params=[{string.Join(",", m.Parameters.Select(p => p.ParameterType.Name))}] {(flags.Count > 0 ? "{" + string.Join(",", flags) + "}" : "")}");
                n++;
            }
        }
        Console.WriteLine($"-- 命中 {n} 条");
    }

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
