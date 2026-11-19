using Mono.Cecil;

// 只读：RID（元数据行号）连续性/位移审计 —— 官方基线 vs 补丁产物
// 用法: ilrid <official.dll> <patched.dll>
var a = AssemblyDefinition.ReadAssembly(args[0]).MainModule;
var b = AssemblyDefinition.ReadAssembly(args[1]).MainModule;

IEnumerable<TypeDefinition> All(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x; }

string Sig(MethodDefinition m) => m.FullName;

List<(int rid, string owner, string name, string sig)> Methods(ModuleDefinition m)
{
    var l = new List<(int, string, string, string)>();
    foreach (var t in m.Types.SelectMany(All))
        foreach (var md in t.Methods)
            l.Add(((int)md.MetadataToken.RID, t.FullName, md.Name, Sig(md)));
    l.Sort((x, y) => x.Item1.CompareTo(y.Item1));
    return l;
}
List<(int rid, string name)> TypeRows(ModuleDefinition m)
{
    var l = new List<(int, string)>();
    foreach (var t in m.Types.SelectMany(All)) l.Add(((int)t.MetadataToken.RID, t.FullName));
    l.Sort((x, y) => x.Item1.CompareTo(y.Item1));
    return l;
}
List<(int rid, string owner, string name)> Fields(ModuleDefinition m)
{
    var l = new List<(int, string, string)>();
    foreach (var t in m.Types.SelectMany(All))
        foreach (var f in t.Fields)
            l.Add(((int)f.MetadataToken.RID, t.FullName, f.Name));
    l.Sort((x, y) => x.Item1.CompareTo(y.Item1));
    return l;
}

void Cmp<T>(string label, List<T> ra, List<T> rb, Func<T, int> rid, Func<T, string> key)
{
    int n = Math.Min(ra.Count, rb.Count), bad = 0, firstBad = -1;
    for (int i = 0; i < n; i++)
    {
        if (rid(ra[i]) != rid(rb[i]) || key(ra[i]) != key(rb[i]))
        {
            bad++;
            if (firstBad < 0) firstBad = i;
            if (bad <= 8) Console.WriteLine($"  [DIFF] #{i}  off(rid={rid(ra[i])}) {key(ra[i])}   vs   patch(rid={rid(rb[i])}) {key(rb[i])}");
        }
    }
    Console.WriteLine($"  {label}: 比较 {n} 行, 不一致 {bad} 行" + (firstBad >= 0 ? $"，首个差异 idx={firstBad}" : "（逐行一致）"));
    Console.WriteLine($"  {label}: 行数 off={ra.Count} patch={rb.Count}");
}

var ma = Methods(a); var mb = Methods(b);
var ta = TypeRows(a); var tb = TypeRows(b);
var fa = Fields(a); var fb = Fields(b);
Console.WriteLine($"== {Path.GetFileName(args[0])}  vs  {Path.GetFileName(args[1])}");
Console.WriteLine($"MethodDef rows: {ma.Count} / {mb.Count}   TypeDef: {ta.Count} / {tb.Count}   FieldDef: {fa.Count} / {fb.Count}");
Console.WriteLine("\n[1] MethodDef 前缀逐行比对");
Cmp("MethodDef", ma, mb, x => x.rid, x => x.owner + "::" + x.name + " " + x.sig);
Console.WriteLine("\n[2] TypeDef 前缀逐行比对");
Cmp("TypeDef", ta, tb, x => x.rid, x => x.name);
Console.WriteLine("\n[3] FieldDef 前缀逐行比对");
Cmp("FieldDef", fa, fb, x => x.rid, x => x.owner + "::" + x.name);

Console.WriteLine("\n[4] 补丁侧尾部新增行（RID > 官方行数）");
foreach (var x in mb.Skip(ma.Count)) Console.WriteLine($"  M rid={x.rid}  {x.owner}::{x.name}");
foreach (var x in tb.Skip(ta.Count)) Console.WriteLine($"  T rid={x.rid}  {x.name}");
foreach (var x in fb.Skip(fa.Count)) Console.WriteLine($"  F rid={x.rid}  {x.owner}::{x.name}");

Console.WriteLine("\n[6] 官方侧三张表的尾部 8 行（判断「追加安全宿主」）");
foreach (var x in ma.Skip(Math.Max(0, ma.Count - 8))) Console.WriteLine($"  M rid={x.rid}  {x.owner}::{x.name}");
foreach (var x in fa.Skip(Math.Max(0, fa.Count - 8))) Console.WriteLine($"  F rid={x.rid}  {x.owner}::{x.name}");
foreach (var x in ta.Skip(Math.Max(0, ta.Count - 6))) Console.WriteLine($"  T rid={x.rid}  {x.name}");
Console.WriteLine("\n[5] 补丁侧 StarryConfig 成员 RID 连续性");
var sc = mb.Where(x => x.owner == "Starry.StarryConfig").ToList();
Console.WriteLine($"  方法数={sc.Count}  RID={string.Join(",", sc.Select(x => x.rid))}");
var scf = fb.Where(x => x.owner == "Starry.StarryConfig").ToList();
Console.WriteLine($"  字段数={scf.Count}  RID={string.Join(",", scf.Select(x => x.rid))}");
