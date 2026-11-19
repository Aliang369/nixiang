using Mono.Cecil;
using Mono.Cecil.Cil;

// 只读：调用点反查（谁调用了 X::Y），以及按名字找方法
// 用法:
//   ilxref <dll> callers <TypeFullName> <MethodName>
//   ilxref <dll> find <nameSubstring>
var mod = AssemblyDefinition.ReadAssembly(args[0]).MainModule;
IEnumerable<TypeDefinition> All(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x; }
var types = mod.Types.SelectMany(All).ToList();

if (args[1] == "callers")
{
    string tn = args[2], mn = args[3];
    int n = 0;
    foreach (var t in types)
        foreach (var m in t.Methods)
        {
            if (!m.HasBody) continue;
            foreach (var ins in m.Body.Instructions)
            {
                if (ins.OpCode != OpCodes.Call && ins.OpCode != OpCodes.Callvirt && ins.OpCode != OpCodes.Ldftn) continue;
                if (ins.Operand is not MethodReference r) continue;
                if (r.Name != mn) continue;
                if (!r.DeclaringType.FullName.Contains(tn)) continue;
                n++;
                Console.WriteLine($"  {m.FullName}  @IL_{ins.Offset:X4}  {ins.OpCode}  -> {r.DeclaringType.Name}::{r.Name}");
            }
        }
    Console.WriteLine($"== callers of {tn}::{mn}: {n}");
}
else if (args[1] == "find")
{
    string sub = args[2];
    foreach (var t in types)
    {
        if (t.FullName.Contains(sub, StringComparison.OrdinalIgnoreCase))
            foreach (var m in t.Methods)
                Console.WriteLine($"  TYPE {t.FullName} :: {m.Name} ({m.Parameters.Count}p)");
    }
    foreach (var t in types)
        foreach (var m in t.Methods)
            if (m.Name.Contains(sub, StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"  METH {t.FullName}::{m.Name} ({m.Parameters.Count}p)");
}
