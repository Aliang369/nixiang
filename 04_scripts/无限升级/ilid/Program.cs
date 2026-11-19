using Mono.Cecil;
using Mono.Cecil.Cil;

// 只读：注入点身份核验 —— 找出含指定 ldstr 的方法，并逐条对照官方同方法同索引处
// 用法: ilid <patched.dll> <official.dll> <stringLiteral>
var pa = AssemblyDefinition.ReadAssembly(args[0]).MainModule;
var of = AssemblyDefinition.ReadAssembly(args[1]).MainModule;
string lit = args[2];

IEnumerable<TypeDefinition> All(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x; }

foreach (var t in pa.Types.SelectMany(All))
    foreach (var m in t.Methods)
    {
        if (!m.HasBody) continue;
        int idx = -1;
        for (int i = 0; i < m.Body.Instructions.Count; i++)
        {
            var ins = m.Body.Instructions[i];
            if (ins.OpCode == OpCodes.Ldstr && (string)ins.Operand == lit) { idx = i; break; }
        }
        if (idx < 0) continue;

        Console.WriteLine("==================================================");
        Console.WriteLine($"方法: {m.FullName}");
        Console.WriteLine($"  声明类型全名 : {t.FullName}");
        Console.WriteLine($"  类型 TypeDef RID={t.MetadataToken.RID} token=0x{t.MetadataToken.ToUInt32():X8}  attrs={t.Attributes}");
        Console.WriteLine($"  方法 RID={m.MetadataToken.RID} token=0x{m.MetadataToken.ToUInt32():X8}  参数={m.Parameters.Count}  返回={m.ReturnType.FullName}");
        Console.WriteLine($"  签名 : {m.FullName}");
        Console.WriteLine($"  ldstr 处: 指令索引 {idx}  IL_{m.Body.Instructions[idx].Offset:X4}");
        Console.WriteLine($"  该 ldstr 前一条: {m.Body.Instructions[idx-1].OpCode} {m.Body.Instructions[idx-1].Operand}");

        // 官方同方法
        var omT = of.Types.SelectMany(All).FirstOrDefault(x => x.FullName == t.FullName);
        MethodDefinition om = null;
        if (omT != null) om = omT.Methods.FirstOrDefault(x => x.FullName == m.FullName);
        Console.WriteLine($"  官方对应方法: {(om == null ? "** 未找到 **" : "找到，指令数 " + om.Body.Instructions.Count + "（补丁侧 " + m.Body.Instructions.Count + "）")}");
        if (om == null) continue;

        int from = Math.Max(0, idx - 6), to = Math.Min(m.Body.Instructions.Count - 1, idx + 14);
        Console.WriteLine("  --- 补丁侧（索引 / IL偏移 / 指令） ---");
        for (int i = from; i <= to; i++)
        {
            var x = m.Body.Instructions[i];
            string mark = i == idx ? "  <<< ldstr oneshot" : (i == idx + 9 ? "  <<< 注入块最后一条" : "");
            Console.WriteLine($"   [{i,4}] IL_{x.Offset:X4}  {x.OpCode,-12} {Operand(x),-60}{mark}");
        }
        Console.WriteLine("  --- 官方侧同索引区间（对照：注入块插在哪个索引之前） ---");
        for (int i = from; i <= Math.Min(om.Body.Instructions.Count - 1, to); i++)
        {
            var x = om.Body.Instructions[i];
            string mark = i == idx ? "  <<< 官方此处即注入块插入位置（块前的 ldarg.0 之后）" : "";
            Console.WriteLine($"   [{i,4}] IL_{x.Offset:X4}  {x.OpCode,-12} {Operand(x),-60}{mark}");
        }
    }

static string Operand(Instruction x)
{
    if (x.Operand == null) return "";
    if (x.Operand is Instruction ti) return "IL_" + ti.Offset.ToString("X4");
    if (x.Operand is Instruction[] tis) return string.Join(",", tis.Select(q => "IL_" + q.Offset.ToString("X4")));
    return x.Operand.ToString();
}
