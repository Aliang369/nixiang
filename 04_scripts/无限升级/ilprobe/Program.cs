using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.RegularExpressions;

// 只读探针：对给定程序集做「类型/方法签名/IL 形状」查询，不改文件、不写产物。
// 用法:
//   ilprobe <asm> types <regex>                     列出类型全名（含嵌套），匹配 regex
//   ilprobe <asm> m <TypeFull|Simple> <Name> [pc]   列方法签名（static/params/ins count/局部变量）
//   ilprobe <asm> il <TypeFull|Simple> <Name> [pc] [max]   dump IL（默认最多 120 条）
//   ilprobe <asm> calls <TypeFull|Simple> <Name>    列出该方法的被调用点（全模块扫描）
//   ilprobe <asm> strs <TypeFull|Simple> <Name>     列出方法内 ldstr 常量
var asmPath = args[0];
var mode = args[1];
var asm = AssemblyDefinition.ReadAssembly(asmPath, new ReaderParameters { ReadSymbols = false });
var mod = asm.MainModule;

IEnumerable<TypeDefinition> All(TypeDefinition t)
{
    yield return t;
    foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x;
}
var allTypes = mod.Types.SelectMany(All).ToList();
Console.Error.WriteLine($"[probe] top-level={mod.Types.Count} all(incl nested)={allTypes.Count}");

TypeDefinition Resolve(string name)
{
    var t = mod.GetType(name);
    if (t != null) return t;
    var cand = allTypes.Where(x => x.Name == name || x.FullName == name).ToList();
    if (cand.Count == 1) return cand[0];
    if (cand.Count == 0) { Console.WriteLine($"TYPE NOT FOUND: {name}"); return null; }
    Console.WriteLine($"TYPE AMBIGUOUS: {name} -> {string.Join(", ", cand.Select(c => c.FullName))}");
    return null;
}

switch (mode)
{
    case "types":
    {
        var re = new Regex(args[2]);
        foreach (var t in allTypes.Where(t => re.IsMatch(t.FullName)))
            Console.WriteLine($"{(t.IsNested ? "NESTED" : "TOP")}  {t.FullName}  methods={t.Methods.Count} fields={t.Fields.Count}");
        break;
    }
    case "m":
    {
        var t = Resolve(args[2]); if (t == null) break;
        var nm = args[3]; int? pc = args.Length > 4 ? int.Parse(args[4]) : (int?)null;
        var ms = t.Methods.Where(x => x.Name == nm && (pc == null || x.Parameters.Count == pc)).ToList();
        Console.WriteLine($"== {t.FullName}::{nm}  matches={ms.Count}");
        foreach (var m in ms)
        {
            Console.WriteLine($"  {(m.IsStatic ? "static" : "instance")} {m.ReturnType.Name} {m.Name}({string.Join(", ", m.Parameters.Select(p => p.ParameterType.FullName))})  ins={(m.HasBody ? m.Body.Instructions.Count : -1)} loc={(m.HasBody ? m.Body.Variables.Count : -1)} attrs={m.Attributes}");
        }
        break;
    }
    case "il":
    {
        var t = Resolve(args[2]); if (t == null) break;
        var nm = args[3]; int? pc = args.Length > 4 ? int.Parse(args[4]) : (int?)null;
        int max = args.Length > 5 ? int.Parse(args[5]) : 120;
        foreach (var m in t.Methods.Where(x => x.Name == nm && (pc == null || x.Parameters.Count == pc)))
        {
            Console.WriteLine($"==== {t.FullName}::{m.Name} {(m.IsStatic ? "static" : "instance")} ret={m.ReturnType.Name} params=[{string.Join(",", m.Parameters.Select(p => p.ParameterType.Name))}]");
            if (!m.HasBody) { Console.WriteLine("  (no body)"); continue; }
            int idx = 0; foreach (var v in m.Body.Variables) Console.WriteLine($"  local V{idx++}: {v.VariableType.Name}");
            int n = 0;
            foreach (var ins in m.Body.Instructions)
            {
                if (n++ >= max) { Console.WriteLine($"  ... ({m.Body.Instructions.Count} ins total)"); break; }
                Console.WriteLine($"  IL_{ins.Offset:X4}: {ins.OpCode,-12} {ins}");
            }
            foreach (var h in m.Body.ExceptionHandlers)
                Console.WriteLine($"  EH: try IL_{h.TryStart.Offset:X4}.. handler IL_{h.HandlerStart.Offset:X4} type={h.HandlerType}");
        }
        break;
    }
    case "calls":
    {
        var t0 = Resolve(args[2]); if (t0 == null) break;
        var nm = args[3];
        foreach (var t in allTypes)
            foreach (var md in t.Methods)
            {
                if (!md.HasBody) continue;
                foreach (var i in md.Body.Instructions)
                    if (i.Operand is MethodReference mr && mr.Name == nm && (mr.DeclaringType.Name == t0.Name || mr.DeclaringType.FullName == t0.FullName))
                        Console.WriteLine($"{t.FullName}::{md.Name}  IL_{i.Offset:X4}  {i.OpCode} {mr.DeclaringType.Name}::{mr.Name}({mr.Parameters.Count}p)");
            }
        break;
    }
    case "around":
    {
        // 用法: around <Type> <Name> <needle> [ctx] [pc]
        var t = Resolve(args[2]); if (t == null) break;
        var nm = args[3]; var needle = args[4];
        int ctx = args.Length > 5 ? int.Parse(args[5]) : 8;
        int? pc = args.Length > 6 ? int.Parse(args[6]) : (int?)null;
        foreach (var m in t.Methods.Where(x => x.Name == nm && (pc == null || x.Parameters.Count == pc)))
        {
            var ins = m.Body.Instructions;
            Console.WriteLine($"==== {t.FullName}::{m.Name} {(m.IsStatic?"static":"inst")} params=[{string.Join(",", m.Parameters.Select(p => p.ParameterType.Name))}] ins={ins.Count}");
            for (int i = 0; i < ins.Count; i++)
            {
                var txt = $"{ins[i].OpCode} {ins[i].Operand}";
                if (!txt.Contains(needle)) continue;
                Console.WriteLine($"  --- hit #{i} ---");
                for (int j = Math.Max(0, i - ctx); j <= Math.Min(ins.Count - 1, i + ctx); j++)
                    Console.WriteLine($"  {(j == i ? ">>" : "  ")} IL_{ins[j].Offset:X4}[{j}]: {ins[j].OpCode,-12} {ins[j].Operand}");
            }
        }
        break;
    }
    case "fields":
    {
        var t = Resolve(args[2]); if (t == null) break;
        Console.WriteLine($"== fields of {t.FullName} (base={t.BaseType?.FullName})");
        foreach (var fd in t.Fields) Console.WriteLine($"  {fd.Attributes}  {fd.FieldType.FullName} {fd.Name}");
        break;
    }
    case "findstr":
    {
        // 全模块扫 ldstr 常量 == needle，打印宿主方法 + 上下文
        var needle = args[2];
        int ctx = args.Length > 3 ? int.Parse(args[3]) : 3;
        foreach (var t in allTypes)
            foreach (var md in t.Methods)
            {
                if (!md.HasBody) continue;
                var ins = md.Body.Instructions;
                for (int i = 0; i < ins.Count; i++)
                {
                    if (ins[i].OpCode.Code != Code.Ldstr) continue;
                    if ((string)ins[i].Operand != needle) continue;
                    Console.WriteLine($"### {t.FullName}::{md.Name}  @IL_{ins[i].Offset:X4}[{i}]");
                    for (int j = Math.Max(0, i - ctx); j <= Math.Min(ins.Count - 1, i + ctx); j++)
                        Console.WriteLine($"   {(j==i?">>":"  ")} IL_{ins[j].Offset:X4}[{j}]: {ins[j].OpCode,-10} {ins[j].Operand}");
                }
            }
        break;
    }
    case "strgrep":
    {
        var re = new Regex(args[2]);
        foreach (var t in allTypes)
            foreach (var md in t.Methods)
            {
                if (!md.HasBody) continue;
                var hits = md.Body.Instructions.Where(i => i.OpCode.Code == Code.Ldstr && re.IsMatch((string)i.Operand))
                            .Select(i => (string)i.Operand).ToList();
                if (hits.Count == 0) continue;
                Console.WriteLine($"{t.FullName}::{md.Name}  ->  {string.Join(" | ", hits.Distinct().Take(12))}");
            }
        break;
    }
    case "refs":
    {
        // 外部引用指纹：字段引用名 + 方法引用名（排除 Obfuz 与自身类型），排序去重
        var t = Resolve(args[2]); if (t == null) break;
        var nm = args[3];
        foreach (var md in t.Methods.Where(x => nm == "*" || x.Name == nm))
        {
            if (!md.HasBody) continue;
            var fs2 = new SortedSet<string>();
            var ms2 = new SortedSet<string>();
            foreach (var i in md.Body.Instructions)
            {
                if (i.Operand is FieldReference fr && !fr.DeclaringType.FullName.Contains("Obfuz") && !fr.DeclaringType.Name.StartsWith("$Obfuz"))
                    fs2.Add(fr.DeclaringType.Name + "." + fr.Name);
                else if (i.Operand is MethodReference mr && !mr.DeclaringType.FullName.Contains("Obfuz") && !mr.DeclaringType.Name.StartsWith("$Obfuz"))
                    ms2.Add(mr.DeclaringType.Name + "." + mr.Name);
            }
            Console.WriteLine($"TYPE {t.FullName} METHOD {md.Name} INS {md.Body.Instructions.Count}");
            Console.WriteLine("F " + string.Join(",", fs2));
            Console.WriteLine("M " + string.Join(",", ms2));
        }
        break;
    }
    case "grep":
    {
        var re = new Regex(args[2]);
        foreach (var t in allTypes)
            foreach (var m in t.Methods)
                if (re.IsMatch(m.Name))
                    Console.WriteLine($"{t.FullName}::{m.Name}({m.Parameters.Count}p) {(m.IsStatic?"static":"inst")} ins={(m.HasBody?m.Body.Instructions.Count:-1)}");
        break;
    }
    case "strs":
    {
        var t = Resolve(args[2]); if (t == null) break;
        var nm = args[3];
        foreach (var m in t.Methods.Where(x => x.Name == nm))
        {
            if (!m.HasBody) continue;
            var ss = m.Body.Instructions.Where(i => i.OpCode.Code == Code.Ldstr).Select(i => (string)i.Operand).ToList();
            Console.WriteLine($"{t.Name}::{m.Name} ldstr[{ss.Count}]: {string.Join(" | ", ss)}");
        }
        break;
    }
}
