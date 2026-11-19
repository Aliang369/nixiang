using Mono.Cecil;

// 只读：元数据表几何对比（TypeDef 行序 / 每类方法数与字段数 / 表尾归属）
// 用法: ilmdt <official.dll> <patched.dll>
var a = AssemblyDefinition.ReadAssembly(args[0]).MainModule;
var b = AssemblyDefinition.ReadAssembly(args[1]).MainModule;

IEnumerable<TypeDefinition> All(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x; }

List<(int idx, string name, int mc, int fc)> Rows(ModuleDefinition m)
{
    var list = new List<(int, string, int, int)>();
    int i = 0;
    foreach (var t in m.Types.SelectMany(All)) list.Add((i++, t.FullName, t.Methods.Count, t.Fields.Count));
    return list;
}
var ra = Rows(a); var rb = Rows(b);
Console.WriteLine($"official: TypeDef={ra.Count} MethodDef={ra.Sum(x => x.mc)} FieldDef={ra.Sum(x => x.fc)}");
Console.WriteLine($"patched : TypeDef={rb.Count} MethodDef={rb.Sum(x => x.mc)} FieldDef={rb.Sum(x => x.fc)}");

var ma = ra.ToDictionary(x => x.name, x => x);
var mb = rb.ToDictionary(x => x.name, x => x);
Console.WriteLine("\n== 仅出现在补丁（新增类型）");
foreach (var k in mb.Keys.Where(k => !ma.ContainsKey(k))) Console.WriteLine($"  + {k}  methods={mb[k].mc} fields={mb[k].fc} rowIdx={mb[k].idx}");
Console.WriteLine("== 仅出现在官方（丢失类型）");
foreach (var k in ma.Keys.Where(k => !mb.ContainsKey(k))) Console.WriteLine($"  - {k}  methods={ma[k].mc} fields={ma[k].fc}");

Console.WriteLine("\n== 同名类型但 方法数/字段数 变化");
int changed = 0;
foreach (var k in ma.Keys.Where(k => mb.ContainsKey(k)))
{
    if (ma[k].mc != mb[k].mc || ma[k].fc != mb[k].fc)
    { changed++; Console.WriteLine($"  ! {k}: methods {ma[k].mc}->{mb[k].mc}  fields {ma[k].fc}->{mb[k].fc}"); }
}
Console.WriteLine($"  合计 {changed} 个");

Console.WriteLine("\n== 表尾 10 个类型（补丁）");
foreach (var r in rb.Skip(Math.Max(0, rb.Count - 10))) Console.WriteLine($"  [{r.idx}] {r.name}  m={r.mc} f={r.fc}");

Console.WriteLine("\n== 秘境/敌人相关类型对照");
foreach (var r in rb.Where(x => System.Text.RegularExpressions.Regex.IsMatch(x.name, "MiJing|EnemyGroupData|EnemyGenerate|^Enemy$|^BossEnemy$")))
{
    ma.TryGetValue(r.name, out var o);
    Console.WriteLine($"  {r.name,-46} methods {o.mc,4}->{r.mc,4}  fields {o.fc,3}->{r.fc,3}  rowIdx {o.idx}->{r.idx}");
}

// 是否出现「方法数为 0 或负数/异常」
Console.WriteLine("\n== 方法数为 0 的类型（可能是范围断裂征兆）");
foreach (var r in rb.Where(x => x.mc == 0 && !x.name.Contains("/<"))) Console.WriteLine($"  {r.name}  f={r.fc}");
