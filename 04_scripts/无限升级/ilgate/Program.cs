using Mono.Cecil;
using Mono.Cecil.Cil;

// 只读：补丁门控核验 —— 找出所有对 host(GetBool/GetInt) 的调用，报 key、类型、IL 偏移、以及门控形态
// 用法: ilgate <patched.dll> <hostTypeFullName>
var mod = AssemblyDefinition.ReadAssembly(args[0]).MainModule;
string host = args[1];
IEnumerable<TypeDefinition> All(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x; }

int n = 0;
foreach (var t in mod.Types.SelectMany(All))
    foreach (var m in t.Methods)
    {
        if (!m.HasBody) continue;
        var ins = m.Body.Instructions;
        for (int i = 0; i < ins.Count; i++)
        {
            if (ins[i].Operand is not MethodReference r) continue;
            if (r.Name != "GetBool" && r.Name != "GetInt") continue;
            if (r.DeclaringType.FullName != host) continue;
            n++;
            // 往前找最近的 ldstr = key
            string key = "?", def = "";
            for (int k = i - 1; k >= 0 && k >= i - 4; k--)
            {
                if (ins[k].OpCode == OpCodes.Ldstr) { key = (string)ins[k].Operand; break; }
            }
            if (r.Name == "GetInt")
            {
                for (int k = i - 1; k >= 0 && k >= i - 3; k--)
                    if (ins[k].OpCode == OpCodes.Ldc_I4 || ins[k].OpCode == OpCodes.Ldc_I4_S || ins[k].OpCode == OpCodes.Ldc_I4_0
                        || ins[k].OpCode == OpCodes.Ldc_I4_1 || ins[k].OpCode == OpCodes.Ldc_I4_2 || ins[k].OpCode == OpCodes.Ldc_I4_3)
                    { def = ins[k].Operand?.ToString() ?? ins[k].OpCode.ToString(); break; }
            }
            // 门控形态：调用之后第 1 条是不是 brfalse/brtrue/ble...
            string gate = i + 1 < ins.Count ? ins[i + 1].OpCode.ToString() : "-";
            string target = i + 1 < ins.Count && ins[i + 1].Operand is Instruction ti ? "IL_" + ti.Offset.ToString("X4") : "-";
            Console.WriteLine($"{t.FullName}::{m.Name}|{r.Name}|{key}|def={def}|{gate}->{target}|IL_{ins[i].Offset:X4}");
        }
    }
Console.WriteLine($"== 共 {n} 处门控调用");
