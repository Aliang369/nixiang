// =============================================================================
// 冒险战争（com.mxzz.zbml / gameid mxzz）IL 静态反推与成品验证工具 —— 全只读
// -----------------------------------------------------------------------------
// 移植自 04_scripts/霸海争锋/verification/{Program.cs, compare/Program.cs}
//  （MemberRef 逐条解析到真实目标程序集；IL 序列 + operand + branch 相对索引计数比对）
//  并按 2026-11-19 冒险战争任务需要新增 baseline / xref 两个子命令。
//  与旧版差异（参数化，不写死游戏路径与 hash）：
//    - dll 路径、DummyDll 目录、MemberRef 目标类型全部走命令行参数；
//    - 前缀长度不再要求人工指定，auto 模式穷举后给判定；
//    - 新增 AssemblyRef 集合增删、跨版本方法 token 对照、字段访问点/写入点定位。
// 证据分级：本工具输出为「实测」（直接读 PE/CLI 元数据与 IL），
//           凡是需要运行期才能确定的量（字段绝对偏移、堆布局）一律标「待验证」。
// -----------------------------------------------------------------------------
// 用法（详细见 README.md）：
//   dotnet bin/Release/net8.0/advwar_static.dll baseline <dll> [--method Type::Name]
//            [--out il.txt] [--report sum.txt] [--dummy <DummyDllDir>]
//   dotnet ... xref     <dll> [--method Type::Name] [--field Type::Name]
//            [--out xref.txt] [--window 8] [--dummy <dir>]
//   dotnet ... verify   <dll> [--type <regex>] [--method Type::Name]
//            [--verbose] [--max-print 50] [--dummy <dir>]
//   dotnet ... compare  <orig.dll> <patched.dll> [--method Type::Name]
//            [--prefix auto|<int>] [--out cmp.txt] [--dummy <dir>]
//   exit 0 = 通过；1 = 有缺陷/差异；2 = 用法错误；3 = 致命异常
// =============================================================================
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

internal static class Program
{
    private const string DefaultMethod = "Battle.Life::AddHP";
    private const string DefaultField = "Battle.Life::_curHP";

    private static int Main(string[] argv)
    {
        if (argv.Length == 0) { Usage(); return 2; }
        try
        {
            switch (argv[0].ToLowerInvariant())
            {
                case "baseline": return CmdBaseline(argv.Skip(1).ToArray());
                case "xref": return CmdXref(argv.Skip(1).ToArray());
                case "verify": return CmdVerify(argv.Skip(1).ToArray());
                case "compare": return CmdCompare(argv.Skip(1).ToArray());
                case "shapes": return CmdShapes(argv.Skip(1).ToArray());
        case "il": return CmdIl(argv.Skip(1).ToArray());
                case "-h": case "--help": case "help": Usage(); return 0;
                default: Console.Error.WriteLine("未知子命令: " + argv[0]); Usage(); return 2;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FATAL " + ex.GetType().Name + ": " + ex.Message);
            return 3;
        }
    }

    private static void Usage()
    {
        Console.WriteLine(@"advwar_static —— 冒险战争 IL 静态反推/成品验证（只读）

  baseline <dll> [--method Type::Name] [--out <il.txt>] [--report <sum.txt>]
           [--dummy <DummyDll>] [--refdir <dir>]...
      反推某方法的元数据基线：token / 参数类型全名 / IL 数 / locals / EH / MaxStack /
      InitLocals / 完整 IL 文本；并反推关键字段 token 与访问点、GetPlayer/GetCamp、
      Camp 枚举值。

  xref <dll> [--method Type::Name] [--field Type::Name] [--window N]
       [--out <txt>] [--dummy <dir>]
      交叉引用：谁 call/callvirt/ldftn 目标方法（含调用点上下文窗口、条件保护、
      负常数线索、方法内阵营引用计数）；以及目标字段全部 ldfld/stfld/ldflda 站点，
      并单列目标方法内部的写入点（stfld）。

  verify <dll> [--type <regex>] [--method Type::Name] [--verbose] [--max-print N]
         [--dummy <dir>] [--refdir <dir>]...
      成员引用全解析：每条 call/callvirt/newobj 的 MemberRef 解析到真实程序集并核对
      scope/HasThis/返回类型/参数类型。缺依赖(missing)与签名不符(sigbad)分开计数；
      System.Private.CoreLib / netstandard / System.Runtime 引用单独告警（旧事故）。

  shapes <dllA> [dllB]
      类型/方法清单；给两个 dll 时输出 A→B 的 added/removed 类型与方法（功能范围/越界改动的粗筛证据）。

  compare <orig.dll> <patched.dll> [--method Type::Name] [--prefix auto|<int>]
          [--out <txt>] [--dummy <dir>]
      成品反推：指令数差 → 前缀长度（auto 穷举）→ 后缀指令逐条比对
      （op / operand / 分支相对索引）→ locals 前缀 → EH（按前缀平移）→
      AssemblyRef 集合增删 → MaxStack/InitLocals 不降低。

公共选项：--dummy <dir>  DummyDll 搜索目录（可多次）；--refdir <dir> 额外搜索目录（可多次）
默认 DummyDll: $NIXIANG_ROOT/06_dumped/冒险战争/1.1.036_il2cpp/dump/DummyDll");
    }

    // ------------------------------------------------------------------ 公共
    private sealed class Opts
    {
        public readonly List<string> Pos = new();
        public readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> Flags = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Repeated(string k) => Multi.TryGetValue(k, out var v) ? v : new List<string>();
        public readonly Dictionary<string, List<string>> Multi = new(StringComparer.OrdinalIgnoreCase);

        public static Opts Parse(string[] a, string[] boolFlags)
        {
            var o = new Opts();
            for (int i = 0; i < a.Length; i++)
            {
                var s = a[i];
                if (!s.StartsWith("--")) { o.Pos.Add(s); continue; }
                var k = s.Substring(2);
                if (boolFlags.Any(f => f.Equals(k, StringComparison.OrdinalIgnoreCase))) { o.Flags.Add(k); continue; }
                if (i + 1 >= a.Length) throw new ArgumentException("选项缺少取值: " + s);
                var v = a[++i];
                o.Map[k] = v;
                if (!o.Multi.TryGetValue(k, out var lst)) o.Multi[k] = lst = new List<string>();
                lst.Add(v);
            }
            return o;
        }
        public string Get(string k, string def = null) => Map.TryGetValue(k, out var v) ? v : def;
        public int GetInt(string k, int def) => Map.TryGetValue(k, out var v) ? int.Parse(v, CultureInfo.InvariantCulture) : def;
        public bool Has(string k) => Flags.Contains(k);
    }

    private static string RepoRoot()
    {
        var env = Environment.GetEnvironmentVariable("NIXIANG_ROOT");
        if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env)) return env;
        var cur = new DirectoryInfo(AppContext.BaseDirectory);
        while (cur != null)
        {
            if (Directory.Exists(Path.Combine(cur.FullName, "04_scripts")) &&
                Directory.Exists(Path.Combine(cur.FullName, "06_dumped"))) return cur.FullName;
            cur = cur.Parent;
        }
        return @"E:\nixiang";
    }

    private static string DefaultDummy()
        => Path.Combine(RepoRoot(), "06_dumped", "冒险战争", "1.1.036_il2cpp", "dump", "DummyDll");

    private static DefaultAssemblyResolver MakeResolver(string dllPath, Opts o)
    {
        var r = new DefaultAssemblyResolver();
        r.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(dllPath)));
        foreach (var d in o.Repeated("refdir")) if (Directory.Exists(d)) r.AddSearchDirectory(Path.GetFullPath(d));
        var dummy = o.Get("dummy", DefaultDummy());
        if (Directory.Exists(dummy)) r.AddSearchDirectory(dummy);
        else Console.Error.WriteLine("WARN DummyDll 目录不存在: " + dummy);
        return r;
    }

    private static AssemblyDefinition ReadAsm(string path, IAssemblyResolver res)
        => AssemblyDefinition.ReadAssembly(Path.GetFullPath(path), new ReaderParameters
        {
            AssemblyResolver = res,
            ReadingMode = ReadingMode.Immediate,
            InMemory = true
        });

    private static IEnumerable<TypeDefinition> AllTypes(ModuleDefinition mod, IEnumerable<TypeDefinition> roots = null)
    {
        foreach (var t in roots ?? mod.Types)
        {
            yield return t;
            foreach (var n in AllTypes(mod, t.NestedTypes)) yield return n;
        }
    }

    private static TypeDefinition FindType(ModuleDefinition mod, string spec)
        => AllTypes(mod).FirstOrDefault(t => t.FullName == spec || t.Name == spec);

    private static List<MethodDefinition> FindMethods(ModuleDefinition mod, string spec)
    {
        int sep = spec.IndexOf("::", StringComparison.Ordinal);
        if (sep < 0) return AllTypes(mod).SelectMany(t => t.Methods).Where(m => m.Name == spec).ToList();
        var tn = spec.Substring(0, sep);
        var mn = spec.Substring(sep + 2);
        var t = FindType(mod, tn);
        if (t == null) return new List<MethodDefinition>();
        return t.Methods.Where(m => m.Name == mn).ToList();
    }

    private static string Sha256(string path)
    {
        using var fs = File.OpenRead(path);
        using var h = SHA256.Create();
        return Convert.ToHexString(h.ComputeHash(fs)).ToLowerInvariant();
    }

    private static string Sig(MethodReference m)
    {
        var paras = string.Join(", ", m.Parameters.Select(p =>
            (string.IsNullOrEmpty(p.Name) ? "" : p.Name + " ") + p.ParameterType.FullName));
        var gen = m is GenericInstanceMethod ? "<generic>" : "";
        return $"{(m.HasThis ? "instance" : "static")} {m.ReturnType.FullName} {m.DeclaringType.FullName}::{m.Name}{gen}({paras})";
    }

    private static string InsText(Instruction ins)
    {
        switch (ins.Operand)
        {
            case null: return "";
            case string s: return "\"" + s + "\"";
            case Instruction t: return "IL_" + t.Offset.ToString("x4");
            case Instruction[] arr: return string.Join(", ", arr.Select(x => "IL_" + x.Offset.ToString("x4")));
            case MethodReference mr: return Sig(mr);
            case FieldReference fr: return fr.FieldType.FullName + " " + fr.DeclaringType.FullName + "::" + fr.Name;
            case TypeReference tr: return tr.FullName;
            case ParameterDefinition pd: return "arg" + pd.Index + ":" + pd.ParameterType.FullName;
            case VariableDefinition vd: return "loc" + vd.Index + ":" + vd.VariableType.FullName;
            case sbyte sb: return sb.ToString(CultureInfo.InvariantCulture);
            case byte b: return b.ToString(CultureInfo.InvariantCulture);
            case int i: return i.ToString(CultureInfo.InvariantCulture);
            case long l: return l.ToString(CultureInfo.InvariantCulture);
            case float f: return f.ToString("R", CultureInfo.InvariantCulture);
            case double d: return d.ToString("R", CultureInfo.InvariantCulture);
            default: return ins.Operand.ToString();
        }
    }

    private static void WriteFile(string path, string text)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, text, new UTF8Encoding(false));
        Console.WriteLine("WROTE " + Path.GetFullPath(path));
    }

    // --------------------------------------------------------------- baseline
    private static int CmdBaseline(string[] argv)
    {
        var o = Opts.Parse(argv, Array.Empty<string>());
        if (o.Pos.Count < 1) { Console.Error.WriteLine("usage: baseline <dll> [--method Type::Name] [--out il.txt] [--report sum.txt] [--dummy dir]"); return 2; }
        var dll = Path.GetFullPath(o.Pos[0]);
        var methodSpec = o.Get("method", DefaultMethod);
        var res = MakeResolver(dll, o);
        var asm = ReadAsm(dll, res);
        var mod = asm.MainModule;
        var sb = new StringBuilder();

        sb.AppendLine("== FILE ==");
        sb.AppendLine($"path    {dll}");
        sb.AppendLine($"size    {new FileInfo(dll).Length}");
        sb.AppendLine($"sha256  {Sha256(dll)}");
        sb.AppendLine($"mtime   {File.GetLastWriteTime(dll).ToString("o")}");
        sb.AppendLine($"asm     {asm.Name.Name} {asm.Name.Version}  culture={asm.Name.Culture ?? "-"} pkt={asm.Name.PublicKeyToken?.Length}");
        sb.AppendLine($"module  {mod.Name}  mvid={mod.Mvid}  runtime={mod.Runtime}  arch={mod.Architecture}");
        sb.AppendLine($"types   {AllTypes(mod).Count()}  methods(with body)={AllTypes(mod).SelectMany(t => t.Methods).Count(m => m.HasBody)}");

        sb.AppendLine();
        sb.AppendLine("== ASSEMBLY REFS ==");
        foreach (var r in mod.AssemblyReferences.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase))
            sb.AppendLine($"  {r.Name} {r.Version}");

        // ---- 目标方法
        var ms = FindMethods(mod, methodSpec);
        sb.AppendLine();
        sb.AppendLine($"== METHOD {methodSpec} ==");
        if (ms.Count == 0) { sb.AppendLine("  NOT FOUND"); Console.Write(sb.ToString()); return 1; }

        string ilText = null;
        foreach (var m in ms)
        {
            sb.AppendLine($"  token     0x{m.MetadataToken.ToInt32():X8}  ({(m.MetadataToken.TokenType)})");
            sb.AppendLine($"  sig       {Sig(m)}");
            sb.AppendLine($"  attrs     {m.Attributes}  implattrs={m.ImplAttributes}  static={m.IsStatic} virtual={m.IsVirtual}");
            sb.AppendLine($"  ret       {m.ReturnType.FullName}");
            sb.AppendLine($"  params    {m.Parameters.Count}");
            foreach (var p in m.Parameters)
                sb.AppendLine($"    arg{p.Index}  {p.ParameterType.FullName}   name={p.Name}");
            var body = m.HasBody ? m.Body : null;
            if (body == null)
            {
                sb.AppendLine("  body      <none: abstract/pinvoke/runtime>");
                continue;
            }
            sb.AppendLine($"  IL        {body.Instructions.Count}");
            sb.AppendLine($"  locals    {body.Variables.Count}");
            sb.AppendLine($"  EH        {body.ExceptionHandlers.Count}");
            sb.AppendLine($"  MaxStack  {body.MaxStackSize}");
            sb.AppendLine($"  InitLocals {body.InitLocals}");
            sb.AppendLine($"  CodeSize  0x{body.CodeSize:x} ({body.CodeSize})");
            for (int i = 0; i < body.Variables.Count; i++)
                sb.AppendLine($"    local[{i}] {body.Variables[i].VariableType.FullName}");
            foreach (var h in body.ExceptionHandlers)
                sb.AppendLine($"    eh {h.HandlerType} try=IL_{h.TryStart?.Offset:x4}..IL_{h.TryEnd?.Offset:x4} handler=IL_{h.HandlerStart?.Offset:x4}..IL_{(h.HandlerEnd != null ? h.HandlerEnd.Offset.ToString("x4") : "end")} catch={h.CatchType?.FullName ?? "-"}");
            sb.AppendLine("  IL-TEXT");
            var il = new StringBuilder();
            il.AppendLine($"# {Sig(m)}");
            il.AppendLine($"# token=0x{m.MetadataToken.ToInt32():X8} il={body.Instructions.Count} locals={body.Variables.Count} eh={body.ExceptionHandlers.Count} maxstack={body.MaxStackSize} initlocals={body.InitLocals} codesize=0x{body.CodeSize:x}");
            il.AppendLine($"# file={dll}");
            il.AppendLine($"# sha256={Sha256(dll)}");
            for (int i = 0; i < body.Variables.Count; i++)
                il.AppendLine($"# local[{i}] {body.Variables[i].VariableType.FullName}");
            il.AppendLine("#");
            foreach (var ins in body.Instructions)
                il.AppendLine($"IL_{ins.Offset:x4}\t{ins.OpCode.Code}\t{InsText(ins)}");
            ilText = il.ToString();
            foreach (var line in ilText.Split('\n').Take(0)) { }
            sb.AppendLine("    (完整文本见 --out 文件)");
        }

        // ---- 关键字段
        var fieldSpec = o.Get("field", DefaultField);
        sb.AppendLine();
        sb.AppendLine($"== FIELD {fieldSpec} ==");
        int fsep = fieldSpec.IndexOf("::", StringComparison.Ordinal);
        if (fsep > 0)
        {
            var ft = FindType(mod, fieldSpec.Substring(0, fsep));
            var fn = fieldSpec.Substring(fsep + 2);
            var f = ft?.Fields.FirstOrDefault(x => x.Name == fn);
            if (f == null) sb.AppendLine("  NOT FOUND");
            else
            {
                sb.AppendLine($"  token     0x{f.MetadataToken.ToInt32():X8}");
                sb.AppendLine($"  type      {f.FieldType.FullName}  (isValueType={f.FieldType.IsValueType})");
                sb.AppendLine($"  attrs     {f.Attributes}  static={f.IsStatic}  literal={f.HasConstant}");
                sb.AppendLine($"  ordinal   {ft.Fields.IndexOf(f) + 1}/{ft.Fields.Count} (含静态)");
                // 字段类型自身布局（值类型是否内嵌 → 决定“绝对偏移”能否静态推导）
                try
                {
                    var ftDef = f.FieldType.Resolve();
                    if (ftDef != null)
                    {
                        sb.AppendLine($"  declared  {ftDef.FullName}  isValueType={ftDef.IsValueType}  base={(ftDef.BaseType?.FullName ?? "-")}  explicitLayout={(ftDef.IsExplicitLayout ? "YES" : "no")}  packing={ftDef.PackingSize}  classSize={ftDef.ClassSize}");
                        foreach (var sub in ftDef.Fields.Where(x => !x.IsStatic))
                            sb.AppendLine($"    subfield {sub.Name} : {sub.FieldType.FullName}  token=0x{sub.MetadataToken.ToInt32():X8}");
                    }
                }
                catch (Exception ex) { sb.AppendLine("  declared  <resolver: " + ex.GetType().Name + ">"); }
                sb.AppendLine("  OFFSET    【待验证】托管字段无静态偏移；绝对偏移由运行期对象布局决定。");
                sb.AppendLine("            静态可确定的是：该字段为值类型内嵌字段 ⇒ 值本体位于 Life 实例内、");
                sb.AppendLine("            紧随其前序实例字段之后；实测偏移须在运行期用 /proc/<pid>/mem 或 frida 读取。");
            }
        }

        // ---- GetPlayer / GetCamp
        sb.AppendLine();
        sb.AppendLine("== GetPlayer / GetCamp ==");
        foreach (var name in new[] { "GetPlayer", "GetCamp" })
        {
            var hits = AllTypes(mod).SelectMany(t => t.Methods).Where(x => x.Name == name).ToList();
            sb.AppendLine($"  {name}: {hits.Count} 处");
            foreach (var h in hits.Take(20))
            {
                var b = h.HasBody ? h.Body : null;
                sb.AppendLine($"    0x{h.MetadataToken.ToInt32():X8}  {Sig(h)}  il={(b == null ? -1 : b.Instructions.Count)} maxstack={(b == null ? -1 : b.MaxStackSize)} locals={(b == null ? -1 : b.Variables.Count)}");
            }
        }

        // ---- Camp 枚举
        sb.AppendLine();
        sb.AppendLine("== Camp enums ==");
        foreach (var t in AllTypes(mod).Where(t => t.IsEnum && (t.Name.IndexOf("CAMP", StringComparison.OrdinalIgnoreCase) >= 0 || t.Name.IndexOf("Camp", StringComparison.Ordinal) >= 0)))
        {
            sb.AppendLine($"  {t.FullName}  token=0x{t.MetadataToken.ToInt32():X8}  base={t.BaseType?.FullName}");
            foreach (var f in t.Fields.Where(x => x.HasConstant))
                sb.AppendLine($"    {f.Name} = {Convert.ToInt64(f.Constant, CultureInfo.InvariantCulture)}");
        }

        Console.Write(sb.ToString());
        if (o.Get("report") is string rep) WriteFile(rep, sb.ToString());
        if (ilText != null && o.Get("out") is string outPath) WriteFile(outPath, ilText);
        return 0;
    }

    // ------------------------------------------------------------------- xref
    private static int CmdXref(string[] argv)
    {
        var o = Opts.Parse(argv, new[] { "no-field" });
        if (o.Pos.Count < 1) { Console.Error.WriteLine("usage: xref <dll> [--method Type::Name] [--field Type::Name] [--window N] [--out txt]"); return 2; }
        var dll = Path.GetFullPath(o.Pos[0]);
        var res = MakeResolver(dll, o);
        var asm = ReadAsm(dll, res);
        var mod = asm.MainModule;
        var methodSpec = o.Get("method", DefaultMethod);
        var fieldSpec = o.Get("field", DefaultField);
        int window = o.GetInt("window", 8);

        var sb = new StringBuilder();
        sb.AppendLine("== FILE ==");
        sb.AppendLine($"path    {dll}");
        sb.AppendLine($"size    {new FileInfo(dll).Length}");
        sb.AppendLine($"sha256  {Sha256(dll)}");
        sb.AppendLine($"asm     {asm.Name.Name} {asm.Name.Version}");

        int msep = methodSpec.IndexOf("::", StringComparison.Ordinal);
        var mType = msep > 0 ? methodSpec.Substring(0, msep) : null;
        var mName = msep > 0 ? methodSpec.Substring(msep + 2) : methodSpec;
        int fsep = fieldSpec.IndexOf("::", StringComparison.Ordinal);
        var fType = fsep > 0 ? fieldSpec.Substring(0, fsep) : null;
        var fName = fsep > 0 ? fieldSpec.Substring(fsep + 2) : fieldSpec;

        var callSites = new List<(MethodDefinition m, Instruction ins, string kind)>();
        var fieldSites = new List<(MethodDefinition m, Instruction ins, string kind)>();
        var writesInTarget = new List<(MethodDefinition m, Instruction ins)>();

        foreach (var m in AllTypes(mod).SelectMany(t => t.Methods))
        {
            if (!m.HasBody) continue;
            foreach (var ins in m.Body.Instructions)
            {
                if (ins.Operand is MethodReference mr)
                {
                    bool typeOk = mType == null || mr.DeclaringType.FullName == mType || mr.DeclaringType.Name == mType;
                    if (typeOk && mr.Name == mName)
                    {
                        var kind = ins.OpCode.Code switch
                        {
                            Code.Call => "call",
                            Code.Callvirt => "callvirt",
                            Code.Newobj => "newobj",
                            Code.Ldftn => "ldftn",
                            Code.Ldvirtftn => "ldvirtftn",
                            _ => null
                        };
                        if (kind != null) callSites.Add((m, ins, kind));
                    }
                }
                if (ins.Operand is FieldReference fr)
                {
                    bool typeOk = fType == null || fr.DeclaringType.FullName == fType || fr.DeclaringType.Name == fType;
                    if (typeOk && fr.Name == fName)
                    {
                        var kind = ins.OpCode.Code switch
                        {
                            Code.Ldfld => "ldfld",
                            Code.Stfld => "stfld",
                            Code.Ldflda => "ldflda",
                            Code.Ldsfld => "ldsfld",
                            Code.Stsfld => "stsfld",
                            _ => null
                        };
                        if (kind != null)
                        {
                            fieldSites.Add((m, ins, kind));
                            if (m.Name == mName) writesInTarget.Add((m, ins));
                        }
                    }
                }
            }
        }

        // --- 调用点
        sb.AppendLine();
        sb.AppendLine($"== CALLERS of {methodSpec} : {callSites.Count} 处 / {callSites.Select(x => x.m.FullName).Distinct().Count()} 个方法 ==");
        int idx = 0;
        foreach (var (m, ins, kind) in callSites)
        {
            idx++;
            var list = m.Body.Instructions.ToList();
            int pos = list.IndexOf(ins);
            var body = m.Body;
            var campRefs = list.Where(x => x.Operand != null && (
                    (x.Operand is MethodReference a && a.Name.IndexOf("Camp", StringComparison.Ordinal) >= 0) ||
                    (x.Operand is FieldReference b && (b.Name.IndexOf("Camp", StringComparison.Ordinal) >= 0 || b.FieldType.FullName.IndexOf("CAMP", StringComparison.OrdinalIgnoreCase) >= 0))))
                .ToList();
            var back = list.Skip(Math.Max(0, pos - window)).Take(pos - Math.Max(0, pos - window)).ToList();
            bool condGuard = back.Any(b => b.OpCode.FlowControl == FlowControl.Cond_Branch && b.Operand is Instruction t && list.IndexOf(t) > pos);
            bool negConst = back.Any(b => b.OpCode.Code == Code.Ldc_I4 || b.OpCode.Code == Code.Ldc_I4_S)
                            && back.Any(b => (b.OpCode.Code == Code.Ldc_I4 && (int)b.Operand < 0) || (b.OpCode.Code == Code.Ldc_I4_S && (sbyte)b.Operand < 0));
            bool negate = back.Any(b => b.OpCode.Code == Code.Neg);
            bool subOp = back.Any(b => b.Operand is MethodReference x && (x.Name == "op_Subtraction" || x.Name == "op_UnaryNegation"));
            var hints = new List<string>();
            if (negConst) hints.Add("NEG_CONST");
            if (negate) hints.Add("NEG_OP");
            if (subOp) hints.Add("SUB/NEG_METHOD");
            if (condGuard) hints.Add("COND_GUARD");

            sb.AppendLine();
            sb.AppendLine($"[{idx}] {kind} @IL_{ins.Offset:x4}  in  {Sig(m)}  token=0x{m.MetadataToken.ToInt32():X8}");
            sb.AppendLine($"    target  {Sig((MethodReference)ins.Operand)}");
            sb.AppendLine($"    method-il={list.Count}  campRefs-in-method={campRefs.Count}{(campRefs.Count > 0 ? " first@IL_" + campRefs[0].Offset.ToString("x4") : "")}  hints=[{string.Join(",", hints)}]");
            sb.AppendLine("    ctx:");
            for (int k = Math.Max(0, pos - window); k < pos; k++)
                sb.AppendLine($"      IL_{list[k].Offset:x4}  {list[k].OpCode.Code,-14} {InsText(list[k])}");
            sb.AppendLine($"      IL_{ins.Offset:x4}  {ins.OpCode.Code,-14} {InsText(ins)}   <<< call site");
        }

        // --- 字段站点
        sb.AppendLine();
        sb.AppendLine($"== FIELD ACCESS {fieldSpec} : {fieldSites.Count} 处 ==");
        foreach (var g in fieldSites.GroupBy(x => x.kind))
            sb.AppendLine($"  {g.Key} x{g.Count()}");
        foreach (var (m, ins, kind) in fieldSites)
        {
            sb.AppendLine($"  {kind,-6} IL_{ins.Offset:x4}  {Sig(m)}");
        }

        sb.AppendLine();
        sb.AppendLine($"== WRITE POINTS of {fieldSpec} inside {methodSpec} ==");
        foreach (var (m, ins) in writesInTarget)
            sb.AppendLine($"  {ins.OpCode.Code} @IL_{ins.Offset:x4}  in {m.Name}");
        if (writesInTarget.Count == 0) sb.AppendLine("  (none)");

        sb.AppendLine();
        sb.AppendLine("== SUMMARY ==");
        sb.AppendLine($"  callSites={callSites.Count}  callers={callSites.Select(x => x.m.FullName).Distinct().Count()}  fieldSites={fieldSites.Count}  writesInTarget={writesInTarget.Count}");

        Console.Write(sb.ToString());
        if (o.Get("out") is string outp) WriteFile(outp, sb.ToString());
        return callSites.Count > 0 ? 0 : 1;
    }

    // ----------------------------------------------------------------- verify
    private static int CmdVerify(string[] argv)
    {
        var o = Opts.Parse(argv, new[] { "verbose" });
        if (o.Pos.Count < 1) { Console.Error.WriteLine("usage: verify <dll> [--type regex] [--method Type::Name] [--verbose] [--max-print N]"); return 2; }
        var dll = Path.GetFullPath(o.Pos[0]);
        var res = MakeResolver(dll, o);
        var asm = ReadAsm(dll, res);
        var mod = asm.MainModule;

        var typeRe = o.Get("type") is string tre ? new Regex(tre, RegexOptions.IgnoreCase) : null;
        var methodSpec = o.Get("method");
        int maxPrint = o.GetInt("max-print", 50);
        bool verbose = o.Has("verbose");

        var targets = new List<MethodDefinition>();
        if (methodSpec != null) targets.AddRange(FindMethods(mod, methodSpec).Where(m => m.HasBody));
        else
        {
            foreach (var m in AllTypes(mod).SelectMany(t => t.Methods))
            {
                if (!m.HasBody) continue;
                if (typeRe != null && !typeRe.IsMatch(m.DeclaringType.FullName)) continue;
                targets.Add(m);
            }
        }

        Console.WriteLine($"== VERIFY {Path.GetFileName(dll)} ==");
        Console.WriteLine($"sha256  {Sha256(dll)}");
        Console.WriteLine($"targets {targets.Count} methods ({(typeRe == null ? "全部类型" : "type~/" + typeRe + "/")})");
        Console.WriteLine("kind\tcaller\tmember\tscope\tnote");

        int ok = 0, missing = 0, sigBad = 0, skipped = 0, unresolved = 0;
        var fails = new List<string>();
        var scopes = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var m in targets)
        foreach (var ins in m.Body.Instructions)
        {
            if (ins.OpCode.Code != Code.Call && ins.OpCode.Code != Code.Callvirt && ins.OpCode.Code != Code.Newobj) continue;
            if (ins.Operand is not MethodReference mr) continue;
            var decl = mr.DeclaringType;
            if (decl is TypeSpecification || mr is GenericInstanceMethod)
            {
                skipped++;
                if (verbose) Console.WriteLine($"skip\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{decl.GetType().Name}\tgeneric/typespec(未校验)");
                continue;
            }

            ModuleDefinition targetMod = null;
            string scopeName = "?";
            if (decl.Scope is AssemblyNameReference anr)
            {
                scopeName = anr.Name;
                try
                {
                    var ra = res.Resolve(anr);
                    targetMod = ra.MainModule;
                    if (!scopes.ContainsKey(anr.Name))
                    {
                        string sha = "";
                        try { sha = Sha256(ra.MainModule.FileName); } catch { sha = "<unavailable>"; }
                        scopes[anr.Name] = ra.MainModule.FileName + " sha256=" + sha;
                    }
                }
                catch (Exception ex) { scopeName = "resolve-fail:" + anr.Name + "(" + ex.GetType().Name + ")"; }
            }
            else if (decl.Scope is ModuleDefinition self) { targetMod = self; scopeName = "(self)"; }

            if (targetMod == null)
            {
                unresolved++;
                var line = $"unresolved\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{scopeName}\t目标程序集无法解析";
                fails.Add(line);
                if (fails.Count <= maxPrint) Console.WriteLine(line);
                continue;
            }

            var td = targetMod.GetType(decl.FullName) ?? AllTypes(targetMod).FirstOrDefault(t => t.FullName == decl.FullName);
            if (td == null)
            {
                missing++;
                var line = $"missing\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{scopeName}\tDeclaringType 未找到";
                fails.Add(line);
                if (fails.Count <= maxPrint) Console.WriteLine(line);
                continue;
            }
            var cand = td.Methods.Where(x => x.Name == mr.Name && x.Parameters.Count == mr.Parameters.Count).ToList();
            var exact = cand.FirstOrDefault(x =>
                x.ReturnType.FullName == mr.ReturnType.FullName &&
                x.IsStatic == !mr.HasThis &&
                x.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(mr.Parameters.Select(p => p.ParameterType.FullName)));
            if (exact == null)
            {
                sigBad++;
                var got = cand.Count == 0 ? "无同名同参方法"
                    : $"候选 {cand.Count} 个但签名不符（首个 ret={cand[0].ReturnType.Name} static={cand[0].IsStatic} argc={cand[0].Parameters.Count}）";
                var line = $"sigbad\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{scopeName}\t{got}";
                fails.Add(line);
                if (fails.Count <= maxPrint) Console.WriteLine(line);
                continue;
            }
            ok++;
            if (verbose) Console.WriteLine($"ok\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{scopeName}\tHasThis={mr.HasThis} static={exact.IsStatic} ret={exact.ReturnType.Name} argc={exact.Parameters.Count}");
        }

        Console.WriteLine();
        Console.WriteLine("RESOLVED-SCOPES");
        foreach (var kv in scopes) Console.WriteLine($"  {kv.Key} -> {kv.Value}");

        // 旧事故告警：不应出现的新式/门面程序集引用
        var suspicious = new[] { "System.Private.CoreLib", "netstandard", "System.Runtime" };
        var hits = mod.AssemblyReferences.Where(r => suspicious.Contains(r.Name, StringComparer.OrdinalIgnoreCase)).ToList();
        Console.WriteLine();
        Console.WriteLine("SUSPICIOUS-REFS " + (hits.Count == 0 ? "(none)" : string.Join(", ", hits.Select(h => h.Name))));

        Console.WriteLine();
        Console.WriteLine($"SUMMARY ok={ok} missing={missing} sigbad={sigBad} skipped={skipped} unresolved={unresolved} suspicious={hits.Count}");
        bool pass = missing + sigBad + unresolved == 0 && hits.Count == 0;
        Console.WriteLine(pass ? "VERDICT: PASS" : "VERDICT: FAIL");
        return pass ? 0 : 1;
    }

    // ------------------------------------------------------------------ shapes
    private static int CmdShapes(string[] argv)
    {
        var o = Opts.Parse(argv, Array.Empty<string>());
        if (o.Pos.Count < 1) { Console.Error.WriteLine("usage: shapes <dllA> [dllB]"); return 2; }
        var a = ReadAsm(Path.GetFullPath(o.Pos[0]), MakeResolver(o.Pos[0], o));
        var ta = Inventory(a);
        Console.WriteLine($"== {Path.GetFileName(o.Pos[0])} ==");
        Console.WriteLine($"sha256={Sha256(o.Pos[0])}");
        Console.WriteLine($"types={ta.types.Count} methods={ta.methods.Count} fields={ta.fields.Count}");
        if (o.Pos.Count < 2) return 0;

        var b = ReadAsm(Path.GetFullPath(o.Pos[1]), MakeResolver(o.Pos[1], o));
        var tb = Inventory(b);
        Console.WriteLine();
        Console.WriteLine($"== {Path.GetFileName(o.Pos[1])} ==");
        Console.WriteLine($"sha256={Sha256(o.Pos[1])}");
        Console.WriteLine($"types={tb.types.Count} methods={tb.methods.Count} fields={tb.fields.Count}");
        var at = tb.types.Except(ta.types).ToList();
        var rt = ta.types.Except(tb.types).ToList();
        var am = tb.methods.Except(ta.methods).ToList();
        var rm = ta.methods.Except(tb.methods).ToList();
        var af = tb.fields.Except(ta.fields).ToList();
        var rf = ta.fields.Except(tb.fields).ToList();
        Console.WriteLine();
        Console.WriteLine("== TYPE DIFF (B - A) ==");
        Console.WriteLine($"  added={at.Count} removed={rt.Count}");
        foreach (var x in at.Take(200)) Console.WriteLine("  + T " + x);
        foreach (var x in rt.Take(200)) Console.WriteLine("  - T " + x);
        Console.WriteLine("== METHOD DIFF (B - A) ==");
        Console.WriteLine($"  added={am.Count} removed={rm.Count}");
        foreach (var x in am.Take(200)) Console.WriteLine("  + M " + x);
        foreach (var x in rm.Take(200)) Console.WriteLine("  - M " + x);
        Console.WriteLine("== FIELD DIFF (B - A) ==");
        Console.WriteLine($"  added={af.Count} removed={rf.Count}");
        foreach (var x in af.Take(200)) Console.WriteLine("  + F " + x);
        foreach (var x in rf.Take(200)) Console.WriteLine("  - F " + x);
        Console.WriteLine();
        Console.WriteLine(at.Count + rt.Count + am.Count + rm.Count + af.Count + rf.Count == 0
            ? "VERDICT: SHAPE-IDENTICAL" : "VERDICT: SHAPE-CHANGED");
        return (at.Count + rt.Count + am.Count + rm.Count + af.Count + rf.Count) == 0 ? 0 : 1;
    }

    private static (HashSet<string> types, HashSet<string> methods, HashSet<string> fields) Inventory(AssemblyDefinition a)
    {
        var t = new HashSet<string>(StringComparer.Ordinal);
        var m = new HashSet<string>(StringComparer.Ordinal);
        var f = new HashSet<string>(StringComparer.Ordinal);
        foreach (var ty in AllTypes(a.MainModule))
        {
            t.Add(ty.FullName);
            foreach (var md in ty.Methods) m.Add(md.FullName);
            foreach (var fd in ty.Fields) f.Add(fd.DeclaringType.FullName + "::" + fd.Name);
        }
        return (t, m, f);
    }

    // ---------------------------------------------------------------- compare
    private static int CmdCompare(string[] argv)
    {
        var o = Opts.Parse(argv, Array.Empty<string>());
        if (o.Pos.Count < 2) { Console.Error.WriteLine("usage: compare <orig.dll> <patched.dll> [--method Type::Name] [--prefix auto|N] [--out txt]"); return 2; }
        var origPath = Path.GetFullPath(o.Pos[0]);
        var patPath = Path.GetFullPath(o.Pos[1]);
        var methodSpec = o.Get("method", DefaultMethod);
        var res = MakeResolver(origPath, o);
        res.AddSearchDirectory(Path.GetDirectoryName(patPath));

        var oa = ReadAsm(origPath, res);
        var pa = ReadAsm(patPath, res);
        var sb = new StringBuilder();

        sb.AppendLine("== INPUTS ==");
        sb.AppendLine($"orig     {origPath}");
        sb.AppendLine($"         size={new FileInfo(origPath).Length} sha256={Sha256(origPath)}");
        sb.AppendLine($"patched  {patPath}");
        sb.AppendLine($"         size={new FileInfo(patPath).Length} sha256={Sha256(patPath)}");
        sb.AppendLine($"target   {methodSpec}");

        var om = FindMethods(oa.MainModule, methodSpec).FirstOrDefault(x => x.HasBody);
        var pm = FindMethods(pa.MainModule, methodSpec).FirstOrDefault(x => x.HasBody);
        if (om == null) { Console.WriteLine(sb.ToString()); Console.WriteLine("原版方法未找到"); return 1; }
        if (pm == null) { Console.WriteLine(sb.ToString()); Console.WriteLine("补丁版方法未找到"); return 1; }
        var ob = om.Body; var pb = pm.Body;
        var oi = ob.Instructions.ToList();
        var pi = pb.Instructions.ToList();

        sb.AppendLine();
        sb.AppendLine("== METHOD META ==");
        sb.AppendLine($"token      orig=0x{om.MetadataToken.ToInt32():X8}  patched=0x{pm.MetadataToken.ToInt32():X8}");
        sb.AppendLine($"sig        orig  {Sig(om)}");
        sb.AppendLine($"           patch {Sig(pm)}");
        sb.AppendLine($"IL         orig={oi.Count}  patched={pi.Count}  delta={pi.Count - oi.Count}");
        sb.AppendLine($"locals     orig={ob.Variables.Count}  patched={pb.Variables.Count}");
        sb.AppendLine($"EH         orig={ob.ExceptionHandlers.Count}  patched={pb.ExceptionHandlers.Count}");
        sb.AppendLine($"MaxStack   orig={ob.MaxStackSize}  patched={pb.MaxStackSize}   {(pb.MaxStackSize >= ob.MaxStackSize ? "OK(不降低)" : "FAIL(降低)")}");
        sb.AppendLine($"InitLocals orig={ob.InitLocals}  patched={pb.InitLocals}   {(!ob.InitLocals || pb.InitLocals ? "OK" : "FAIL(被关掉)")}");

        // ---- 前缀长度：auto 穷举 / 指定
        var cands = new List<int>();
        if (pi.Count >= oi.Count)
            for (int k = 0; k <= pi.Count - oi.Count; k++)
                if (SuffixIdentical(oi, pi, k)) cands.Add(k);
        int? want = null;
        var prefOpt = o.Get("prefix", "auto");
        if (!prefOpt.Equals("auto", StringComparison.OrdinalIgnoreCase)) want = int.Parse(prefOpt, CultureInfo.InvariantCulture);

        int start = cands.Count > 0 ? cands[0] : (pi.Count - oi.Count);
        sb.AppendLine();
        sb.AppendLine("== PREFIX ==");
        sb.AppendLine($"候选前缀长度(后缀逐条一致) = [{string.Join(",", cands)}]");
        if (want.HasValue) sb.AppendLine($"指定前缀 = {want.Value}  {(cands.Contains(want.Value) ? "命中候选" : "不在候选内")}");
        sb.AppendLine($"采用前缀 = {start}");
        int prefixReach = PrefixReachability(pi, start, oi.Count);
        sb.AppendLine($"前缀指令数 = {start}");
        sb.AppendLine("前缀 IL：");
        for (int k = 0; k < start; k++)
            sb.AppendLine($"  +IL_{pi[k].Offset:x4}  {pi[k].OpCode.Code,-14} {InsText(pi[k])}");
        sb.AppendLine($"前缀可达性: {DescribeReach(prefixReach)}");

        // ---- 后缀比对
        var suffix = pi.Skip(start).ToList();
        int n = Math.Min(suffix.Count, oi.Count);
        int opDiff = 0, operandDiff = 0, branchDiff = 0;
        var diffs = new List<string>();
        for (int k = 0; k < n; k++)
        {
            var a = oi[k]; var b = suffix[k];
            var na = Norm(a, oi); var nb = Norm(b, suffix);
            if (na.op != nb.op) { opDiff++; if (diffs.Count < 20) diffs.Add($"  opdiff @{k} (IL_{b.Offset:x4}): {na.op} != {nb.op}"); }
            if (na.operand != nb.operand) { operandDiff++; if (diffs.Count < 20) diffs.Add($"  opnddiff @{k} (IL_{b.Offset:x4}): {na.op} [{na.operand}] != [{nb.operand}]"); }
            if (na.target != nb.target) { branchDiff++; if (diffs.Count < 20) diffs.Add($"  brdiff @{k} (IL_{b.Offset:x4}): {na.target} != {nb.target}"); }
        }
        sb.AppendLine();
        sb.AppendLine("== SUFFIX ==");
        sb.AppendLine($"suffix 指令数: orig={oi.Count} patched={suffix.Count} 比对={n}");
        sb.AppendLine($"counter: op={opDiff} operand={operandDiff} branch={branchDiff}");
        foreach (var d in diffs) sb.AppendLine(d);

        // ---- 结构化差异（LCS）：区分「纯插入」与「原位改写」
        var dl = DiffStructure(oi, pi);
        sb.AppendLine();
        sb.AppendLine("== INLINE-DIFF (LCS, 分支目标不计入对齐键) ==");
        sb.AppendLine($"pureInsertion={dl.pureInsert}  removed={dl.removed}  inserted={dl.inserted}  blocks={dl.blocks.Count}  (n*m={oi.Count * pi.Count})");
        foreach (var b in dl.blocks) sb.AppendLine("  " + b);

        // ---- locals
        bool localsPrefixSame = true;
        for (int i = 0; i < ob.Variables.Count && i < pb.Variables.Count; i++)
            if (ob.Variables[i].VariableType.FullName != pb.Variables[i].VariableType.FullName) { localsPrefixSame = false; sb.AppendLine($"  localdiff @{i}: {ob.Variables[i].VariableType.FullName} != {pb.Variables[i].VariableType.FullName}"); }
        sb.AppendLine();
        sb.AppendLine("== LOCALS ==");
        sb.AppendLine($"orig={ob.Variables.Count} patched={pb.Variables.Count} prefixSame={localsPrefixSame} extra={pb.Variables.Count - ob.Variables.Count}");
        foreach (var v in pb.Variables.Skip(ob.Variables.Count)) sb.AppendLine($"  new local: {v.VariableType.FullName}");

        // ---- EH（按前缀平移）
        var oeh = ob.ExceptionHandlers.Select(h => EhKey(ob, h, 0)).ToList();
        var peh = pb.ExceptionHandlers.Select(h => EhKey(pb, h, -start)).ToList();
        var pehRaw = pb.ExceptionHandlers.Select(h => EhKey(pb, h, 0)).ToList();
        bool ehOk = oeh.SequenceEqual(peh);
        sb.AppendLine();
        sb.AppendLine("== EH ==");
        sb.AppendLine($"orig={oeh.Count} patched={pehRaw.Count} 平移后一致={ehOk}");
        foreach (var e in oeh) sb.AppendLine($"  O {e}");
        foreach (var e in pehRaw) sb.AppendLine($"  P {e}");

        // ---- AssemblyRef
        var orefs = oa.MainModule.AssemblyReferences.Select(r => r.Name).ToList();
        var prefs = pa.MainModule.AssemblyReferences.Select(r => r.Name).ToList();
        var added = prefs.Except(orefs, StringComparer.OrdinalIgnoreCase).ToList();
        var removed = orefs.Except(prefs, StringComparer.OrdinalIgnoreCase).ToList();
        sb.AppendLine();
        sb.AppendLine("== ASSEMBLY REFS ==");
        sb.AppendLine($"orig({orefs.Count})    {string.Join(", ", orefs)}");
        sb.AppendLine($"patched({prefs.Count}) {string.Join(", ", prefs)}");
        sb.AppendLine($"ADDED   {added.Count}  {string.Join(", ", added)}");
        sb.AppendLine($"REMOVED {removed.Count}  {string.Join(", ", removed)}");

        // ---- 其它整体差异（粗筛，防“顺手重编译”）
        int otc = AllTypes(oa.MainModule).Count(), ptc = AllTypes(pa.MainModule).Count();
        int omc = AllTypes(oa.MainModule).SelectMany(t => t.Methods).Count(), pmc = AllTypes(pa.MainModule).SelectMany(t => t.Methods).Count();
        sb.AppendLine();
        sb.AppendLine("== SHAPE ==");
        sb.AppendLine($"types   orig={otc} patched={ptc}  delta={ptc - otc}");
        sb.AppendLine($"methods orig={omc} patched={pmc}  delta={pmc - omc}");

        bool prefixOk = cands.Count > 0 && opDiff == 0 && operandDiff == 0 && branchDiff == 0 && suffix.Count == oi.Count;
        bool bodyOk = prefixOk || (cands.Count == 0 && dl.pureInsert);
        bool metaOk = localsPrefixSame && ehOk && added.Count == 0
                      && pb.MaxStackSize >= ob.MaxStackSize
                      && (pb.InitLocals || !ob.InitLocals)
                      && Sig(om) == Sig(pm);
        bool pass = bodyOk && metaOk;
        sb.AppendLine();
        sb.AppendLine($"gate  body={(bodyOk ? "OK" : "FAIL")}  meta={(metaOk ? "OK" : "FAIL")}  mode={(prefixOk ? "纯前缀注入" : (dl.pureInsert ? "纯插入(原位)" : "含原位改写"))}");
        sb.AppendLine(pass
            ? (prefixOk ? "VERDICT: PREFIX-IDENTICAL (差异仅限注入前缀；签名/引用/栈/局部/EH 均合规)"
                        : "VERDICT: INSERTION-ONLY (差异仅限插入块，原指令序列未改写；签名/引用/栈/局部/EH 均合规)")
            : "VERDICT: DIFFERENCES-FOUND");

        Console.Write(sb.ToString());
        if (o.Get("out") is string outp) WriteFile(outp, sb.ToString());
        return pass ? 0 : 1;
    }

    private static string NormKey(Instruction ins, List<Instruction> list)
    {
        var (op, operand, _) = Norm(ins, list);
        return op + "|" + operand;
    }

    /// <summary>LCS 对齐：区分“纯插入”（原指令序列保持）与“原位改写/删除”。</summary>
    private static (bool pureInsert, int removed, int inserted, List<string> blocks) DiffStructure(
        List<Instruction> oi, List<Instruction> pi)
    {
        var blocks = new List<string>();
        int n = oi.Count, m = pi.Count;
        if ((long)n * m > 4_000_000)
        {
            blocks.Add("(过大方法，跳过 LCS)");
            return (false, -1, -1, blocks);
        }
        var ko = oi.Select(x => NormKey(x, oi)).ToList();
        var kp = pi.Select(x => NormKey(x, pi)).ToList();
        var dp = new int[n + 1, m + 1];
        for (int i = n - 1; i >= 0; i--)
            for (int j = m - 1; j >= 0; j--)
                dp[i, j] = ko[i] == kp[j] ? dp[i + 1, j + 1] + 1 : Math.Max(dp[i + 1, j], dp[i, j + 1]);

        int a = 0, b = 0, removed = 0, inserted = 0;
        int runStart = -1; var run = new List<Instruction>();
        void Flush()
        {
            if (run.Count == 0) return;
            inserted += run.Count;
            blocks.Add($"INSERT @patched[{runStart}] ({run.Count} ins): " +
                       string.Join("; ", run.Take(8).Select(x => x.OpCode.Code + " " + InsText(x))) +
                       (run.Count > 8 ? $" ... (+{run.Count - 8})" : ""));
            run.Clear(); runStart = -1;
        }
        while (a < n && b < m)
        {
            if (ko[a] == kp[b]) { Flush(); a++; b++; continue; }
            if (dp[a + 1, b] >= dp[a, b + 1])
            {
                removed++;
                blocks.Add($"REMOVE @orig[{a}] il=IL_{oi[a].Offset:x4} {oi[a].OpCode.Code} {InsText(oi[a])}");
                a++;
            }
            else
            {
                if (run.Count == 0) runStart = b;
                run.Add(pi[b]); b++;
            }
        }
        while (b < m) { if (run.Count == 0) runStart = b; run.Add(pi[b]); b++; }
        Flush();
        while (a < n) { removed++; blocks.Add($"REMOVE @orig[{a}] il=IL_{oi[a].Offset:x4} {oi[a].OpCode.Code} {InsText(oi[a])}"); a++; }

        return (removed == 0, removed, inserted, blocks);
    }

    private static string DescribeReach(int r) => r switch
    {
        0 => "suffix 可达（前缀无早退/无条件跳转越过后缀）",
        1 => "suffix 不可达：前缀含 ret/throw 早退（早退式补丁，属预期）",
        2 => "suffix 不可达：前缀以无条件 br 跳过（需人工确认是否预期）",
        _ => "未知"
    };

    /// <summary>0=可落到 suffix；1=前缀含 ret/throw；2=前缀以无条件 br 跳出前缀</summary>
    private static int PrefixReachability(List<Instruction> pi, int start, int origCount)
    {
        if (start <= 0) return 0;
        for (int k = 0; k < start; k++)
        {
            var c = pi[k].OpCode.Code;
            if (c == Code.Ret || c == Code.Throw || c == Code.Rethrow || c == Code.Endfinally || c == Code.Endfilter) return 1;
        }
        var last = pi[start - 1].OpCode;
        if (last.FlowControl == FlowControl.Branch) return 2;
        return 0;
    }

    private static bool SuffixIdentical(List<Instruction> oi, List<Instruction> pi, int start)
    {
        var suffix = pi.Skip(start).ToList();
        if (suffix.Count != oi.Count) return false;
        for (int k = 0; k < oi.Count; k++)
        {
            var na = Norm(oi[k], oi);
            var nb = Norm(suffix[k], suffix);
            if (na.op != nb.op || na.operand != nb.operand || na.target != nb.target) return false;
        }
        return true;
    }

    private static string EhKey(MethodBody body, ExceptionHandler h, int shift)
    {
        var list = body.Instructions.ToList();
        int I(Instruction i) => i == null ? -1 : list.IndexOf(i) + shift;
        return $"{h.HandlerType}:try{I(h.TryStart)}-{I(h.TryEnd)} h{I(h.HandlerStart)}-{I(h.HandlerEnd)} catch={h.CatchType?.FullName ?? "-"} filter={I(h.FilterStart)}";
    }

    private static (string op, string operand, string target) Norm(Instruction ins, List<Instruction> list)
    {
        // short/long 分支统一（br.s -> br），索引按所在列表相对位置
        var op = ins.OpCode.Code.ToString().Replace(".s", "");
        string operand = ins.Operand switch
        {
            null => "",
            string s => "str:" + s,
            int i => "i4:" + i,
            long l => "i8:" + l,
            float f => "r4:" + f.ToString("R", CultureInfo.InvariantCulture),
            double d => "r8:" + d.ToString("R", CultureInfo.InvariantCulture),
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

    // =========================================================================
    // il <dll> --type <regex> [--method A,B] [--all] [--out file]
    //   打印类型的字段表（含 static/字面量）与指定方法的完整 IL（含异常处理器）
    // =========================================================================
    private static int CmdIl(string[] argv)
    {
        if (argv.Length < 1) { Console.Error.WriteLine("用法: il <dll> --type <regex> [--method A,B] [--all] [--out file]"); return 2; }
        string dll = argv[0];
        string typeRe = null, outPath = null;
        var want = new List<string>();
        bool all = false;
        for (int i = 1; i < argv.Length; i++)
        {
            switch (argv[i])
            {
                case "--type": typeRe = argv[++i]; break;
                case "--method": want.AddRange(argv[++i].Split(',')); break;
                case "--all": all = true; break;
                case "--out": outPath = argv[++i]; break;
                default: Console.Error.WriteLine("未知参数: " + argv[i]); return 2;
            }
        }
        if (typeRe == null) { Console.Error.WriteLine("必须给 --type"); return 2; }
        var sb = new StringBuilder();
        void W(string s) { sb.AppendLine(s); }
        var asm = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { ReadSymbols = false });
        W("# dll = " + dll + "  size = " + new FileInfo(dll).Length + "  sha256 = " + Sha256Of(dll));
        int hits = 0;
        foreach (var type in FlattenTypes(asm.MainModule.Types))
        {
            if (!Regex.IsMatch(type.FullName, typeRe)) continue;
            hits++;
            W("");
            W("=== TYPE " + type.FullName + "  (attrs=" + type.Attributes + ") ===");
            foreach (var f in type.Fields)
            {
                string val = "";
                if (f.HasConstant) val = "  const=" + (f.Constant == null ? "<null>" : f.Constant.ToString());
                W(string.Format("  field {0} {1} {2}{3}", f.Attributes, f.FieldType.FullName, f.Name, val));
            }
            foreach (var m in type.Methods)
            {
                if (!all && want.Count > 0 && !want.Contains(m.Name) && m.Name != ".cctor") continue;
                if (!all && want.Count == 0) continue;
                W("");
                W("--- METHOD " + m.FullName);
                W("    attrs=" + m.Attributes + "  static=" + m.IsStatic + "  ret=" + m.ReturnType.FullName);
                var ps = m.Parameters.Select(x => x.ParameterType.FullName + " " + x.Name);
                W("    params=(" + string.Join(", ", ps) + ")");
                if (!m.HasBody) { W("    <no body>"); continue; }
                var b = m.Body;
                W("    ILcount=" + b.Instructions.Count + "  locals=" + b.Variables.Count +
                  "  EH=" + b.ExceptionHandlers.Count + "  MaxStack=" + b.MaxStackSize + "  InitLocals=" + b.InitLocals);
                if (b.Variables.Count > 0)
                    W("    locals: " + string.Join("; ", b.Variables.Select((v, i) => "V" + i + ":" + v.VariableType.FullName)));
                foreach (var eh in b.ExceptionHandlers)
                    W(string.Format("    EH {0} try=IL_{1:x4}..IL_{2:x4} handler=IL_{3:x4}..IL_{4:x4} catch={5}",
                        eh.HandlerType, eh.TryStart?.Offset ?? -1, eh.TryEnd?.Offset ?? -1,
                        eh.HandlerStart?.Offset ?? -1, eh.HandlerEnd?.Offset ?? -1,
                        eh.CatchType?.FullName ?? "-"));
                foreach (var ins in b.Instructions)
                {
                    string op = ins.OpCode.ToString();
                    string operand;
                    if (ins.Operand is Instruction ti) operand = "IL_" + ti.Offset.ToString("x4");
                    else if (ins.Operand is Instruction[] tia) operand = string.Join(",", tia.Select(x => "IL_" + x.Offset.ToString("x4")));
                    else if (ins.Operand is string s2) operand = "\"" + s2 + "\"";
                    else if (ins.Operand is null) operand = "";
                    else operand = ins.Operand.ToString();
                    W(string.Format("    IL_{0:x4}: {1,-12} {2}", ins.Offset, op, operand));
                }
            }
        }
        if (hits == 0) { Console.Error.WriteLine("没有类型匹配 " + typeRe); return 1; }
        string text = sb.ToString();
        if (outPath != null) { File.WriteAllText(outPath, text, new UTF8Encoding(false)); Console.WriteLine("written " + outPath + " (" + text.Length + " chars)"); }
        else Console.Write(text);
        return 0;
    }

    private static IEnumerable<TypeDefinition> FlattenTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (var t in types)
        {
            yield return t;
            foreach (var n in FlattenTypes(t.NestedTypes)) yield return n;
        }
    }

    private static string Sha256Of(string path)
    {
        using var fs = File.OpenRead(path);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(fs)).ToLowerInvariant();
    }

}
