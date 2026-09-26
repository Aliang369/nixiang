// ilsrch — 汇编检索小工具（Mono.Cecil），给「找锚点」用。
// 用法:
//   dotnet ilsrch.dll <assembly.dll> types   <正则>   # 类型（含嵌套）全名
//   dotnet ilsrch.dll <assembly.dll> fields  <正则>   # 字段：Type::Field / 类型 / static|inst
//   dotnet ilsrch.dll <assembly.dll> props   <正则>   # 属性
//   dotnet ilsrch.dll <assembly.dll> methods <正则>   # 方法：Type::Method (sig) ret= IL=指令数
//   dotnet ilsrch.dll <assembly.dll> callers <正则>   # 谁调用了匹配的方法（方法全名正则）
//   dotnet ilsrch.dll <assembly.dll> str     <正则>   # 哪些方法里 ldstr 了匹配的字符串
//   dotnet ilsrch.dll <assembly.dll> fref    <正则>   # 哪些方法引用了匹配的字段（含 stfld/ldfld，输出指令）
//   dotnet ilsrch.dll <assembly.dll> dump    <类型::方法>  # 反汇编一个方法（本地变量 + 全部 IL + 异常表）
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

if (args.Length < 3) { Console.Error.WriteLine("用法: ilsrch <assembly.dll> <types|fields|props|methods|callers|str> <正则>"); return 2; }

var path = args[0];
var mode = args[1];
var rx = new Regex(args[2], RegexOptions.IgnoreCase);
var asm = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = false });
var mod = asm.MainModule;

static string Fmt(object o) => o switch
{
    null => "",
    string s => "\"" + s + "\"",
    Instruction i => $"IL_{i.Offset:X4}",
    Instruction[] arr => string.Join(",", arr.Select(x => $"IL_{x.Offset:X4}")),
    MethodReference mr => mr.FullName,
    FieldReference fr => fr.DeclaringType.FullName + "::" + fr.Name,
    TypeReference tr => tr.FullName,
    _ => o.ToString()
};

IEnumerable<TypeDefinition> Nested(TypeDefinition t){
    foreach (var n in t.NestedTypes) { yield return n; foreach (var x in Nested(n)) yield return x; }
}
IEnumerable<TypeDefinition> AllTypes()
{
    foreach (var t in mod.Types) { yield return t; foreach (var n in Nested(t)) yield return n; }
}

switch (mode)
{
    case "types":
        foreach (var t in AllTypes())
            if (rx.IsMatch(t.FullName))
                Console.WriteLine($"{t.FullName}\tfields={t.Fields.Count}\tmethods={t.Methods.Count}\tprops={t.Properties.Count}");
        break;

    case "fields":
        foreach (var t in AllTypes())
            foreach (var f in t.Fields)
                if (rx.IsMatch(t.FullName + "::" + f.Name) || rx.IsMatch(f.Name))
                    Console.WriteLine($"{t.FullName}::{f.Name}\t{f.FieldType.FullName}\t{(f.IsStatic ? "static" : "inst")}");
        break;

    case "props":
        foreach (var t in AllTypes())
            foreach (var p in t.Properties)
                if (rx.IsMatch(t.FullName + "::" + p.Name) || rx.IsMatch(p.Name))
                    Console.WriteLine($"{t.FullName}::{p.Name}\t{p.PropertyType.FullName}");
        break;

    case "methods":
        foreach (var t in AllTypes())
            foreach (var m in t.Methods)
            {
                var sig = string.Join(",", m.Parameters.Select(p => p.ParameterType.FullName));
                if (rx.IsMatch(t.FullName + "::" + m.Name) || rx.IsMatch(m.Name))
                    Console.WriteLine($"{t.FullName}::{m.Name}\t({sig})\tret={m.ReturnType.FullName}\tIL={(m.HasBody ? m.Body.Instructions.Count : 0)}");
            }
        break;

    case "callers":
        foreach (var t in AllTypes())
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                foreach (var ins in m.Body.Instructions)
                    if (ins.Operand is MethodReference mr && rx.IsMatch(mr.FullName))
                    {
                        Console.WriteLine($"{t.FullName}::{m.Name}\t-> {mr.FullName}\t@IL_{ins.Offset:X4}");
                        break;
                    }
            }
        break;

    case "str":
        foreach (var t in AllTypes())
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                foreach (var ins in m.Body.Instructions)
                    if (ins.OpCode.Code == Code.Ldstr && ins.Operand is string s && rx.IsMatch(s))
                        Console.WriteLine($"{t.FullName}::{m.Name}\t\"{s}\"");
            }
        break;

    case "fref":
        foreach (var t in AllTypes())
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                foreach (var ins in m.Body.Instructions)
                    if (ins.Operand is FieldReference fr && (rx.IsMatch(fr.DeclaringType.FullName + "::" + fr.Name) || rx.IsMatch(fr.Name)))
                        Console.WriteLine($"{t.FullName}::{m.Name}\t{ins.OpCode.Code}\t{fr.DeclaringType.FullName}::{fr.Name}\t@IL_{ins.Offset:X4}");
            }
        break;

    case "dump":
        foreach (var t in AllTypes())
            foreach (var m in t.Methods)
            {
                if (!rx.IsMatch(t.FullName + "::" + m.Name)) continue;
                Console.WriteLine($"=== {t.FullName}::{m.Name} ({string.Join(",", m.Parameters.Select(p => p.ParameterType.FullName))}) ret={m.ReturnType.FullName} ===");
                if (!m.HasBody) { Console.WriteLine("(no body)"); continue; }
                foreach (var v in m.Body.Variables) Console.WriteLine($"  local V_{v.Index}: {v.VariableType.FullName}");
                foreach (var ins in m.Body.Instructions)
                    Console.WriteLine($"  IL_{ins.Offset:X4}: {ins.OpCode,-12} {Fmt(ins.Operand)}");
                foreach (var h in m.Body.ExceptionHandlers)
                    Console.WriteLine($"  EH {h.HandlerType} try IL_{h.TryStart?.Offset:X4}-IL_{h.TryEnd?.Offset:X4} handler IL_{h.HandlerStart?.Offset:X4} catch={h.CatchType?.FullName}");
            }
        break;

    default:
        Console.Error.WriteLine("模式: types|fields|props|methods|callers|str");
        return 2;
}
Console.Error.WriteLine($"# types={AllTypes().Count()} module={asm.Name.Name}");
return 0;
