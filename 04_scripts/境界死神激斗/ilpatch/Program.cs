using Mono.Cecil;

// 境界死神激斗（com.cmge.ssjd.x7sy 1.38.2）IL 补丁入口
//   dotnet run                                  → 默认 in/out 路径
//   dotnet run -- <src> <dst>                   → 指定 in/out
//   dotnet run -- --dump <Full.Type.Name> <method|*>   → 只打印 IL（排障用，读**输入**）
//   dotnet run -- --probe [<dll>]               → 诊断写盘期 AssemblyResolutionException
//
// 本文件兼任「出包前自检门禁」，三步（断言本体在 Patch.cs 的 Patcher.Gate）：
//   ① 把每个补丁方法**完整** IL 打到 stdout（旧版只打前 10 条 —— 而 starg 往往在第 11 条之后，
//      曾因此把 `Parameters[3]`（attackerSkillId）当成 finalDamage 写出去，出包+装机整轮报废）；
//   ② 逐补丁跑**意图断言**（见 Patcher.Gate）；
//   ③ 任一 FAIL → Environment.Exit(1) 阻断后续出包。
var root = "/Users/aliang/Documents/nixiang";
var src = root + "/07_patched/境界死神激斗/runtime_dump/Assembly-CSharp_1.38.2_dev.dll";
var dst = root + "/07_patched/境界死神激斗/Assembly-CSharp_1.38.2_patched.dll";

var a = new List<string>(args);
if (a.Count > 0 && a[0] == "--dump")
{
    var tName = a.Count > 1 ? a[1] : "BleachBattle.ActorDataControl";
    var mName = a.Count > 2 ? a[2] : "*";
    var asm = AssemblyDefinition.ReadAssembly(src);
    Dumper.DumpIL(asm, tName, mName);
    return;
}
// --probe：诊断「写盘期 AssemblyResolutionException」。Cecil 在 AddParameter/AddField 阶段会对
// **带常量**的成员解析其类型（MetadataBuilder.GetConstantType → CheckedResolve），解析不到就抛，
// 而抛的那个类型往往跟本任务补丁毫无关系（是本模块里某个默认参数值）。本模式统计并列出元凶。
if (a.Count > 0 && a[0] == "--probe")
{
    var p0 = a.Count > 1 ? a[1] : src;
    var asm = AssemblyDefinition.ReadAssembly(p0, new ReaderParameters { ReadSymbols = false });
    int nParam = 0, nField = 0, nProp = 0; var bad = new List<string>();
    foreach (var t in asm.MainModule.GetTypes())
    {
        foreach (var m in t.Methods)
            foreach (var p in m.Parameters)
                if (p.HasConstant)
                {
                    nParam++;
                    try { p.ParameterType.Resolve(); }
                    catch (Exception e) { bad.Add($"[param] {t.FullName}::{m.Name}({p.Name} : {p.ParameterType.FullName})  {e.GetType().Name}"); }
                }
        foreach (var f in t.Fields)
            if (f.HasConstant)
            {
                nField++;
                try { f.FieldType.Resolve(); }
                catch (Exception e) { bad.Add($"[field] {t.FullName}::{f.Name} : {f.FieldType.FullName}  {e.GetType().Name}"); }
            }
        foreach (var pr in t.Properties)
            foreach (var p in pr.Parameters)
                if (p.HasConstant)
                {
                    nProp++;
                    try { p.ParameterType.Resolve(); }
                    catch (Exception e) { bad.Add($"[propparam] {t.FullName}::{pr.Name}({p.Name})  {e.GetType().Name}"); }
                }
    }
    Console.WriteLine($"[probe] 带常量的成员：param={nParam} field={nField} propParam={nProp}");
    Console.WriteLine($"[probe] 类型解析失败 = {bad.Count}");
    foreach (var b in bad.Take(60)) Console.WriteLine("   " + b);
    if (bad.Count > bad.Take(60).Count()) Console.WriteLine($"   …（共 {bad.Count} 条）");
    Console.WriteLine("提示：只有「有**真实默认值**」的失败才丢数据（哨兵不算）；TolerantRefs.ShimFactory 会自动垫片。");
    return;
}
if (a.Count >= 2) { src = a[0]; dst = a[1]; }

Patcher.Run(src, dst);

Console.WriteLine();
Console.WriteLine("==== 复核 ①：补丁方法完整 IL ====");
var fails = new List<string>();
using (var v = AssemblyDefinition.ReadAssembly(dst))
{
    var mm = v.MainModule;
    foreach (var (tn, mn) in Patcher.Targets)
    {
        var t = mm.GetType(tn);
        var m = t?.Methods.FirstOrDefault(x => x.Name == mn);
        if (m == null) { Console.WriteLine($"-- {tn}::{mn}  (NOT FOUND)"); continue; }
        Console.WriteLine($"-- {tn}::{mn}  ({m.Body.Instructions.Count} 条 IL, locals={m.Body.Variables.Count})");
        foreach (var ins in m.Body.Instructions)
            Console.WriteLine($"     IL_{ins.Offset:X4}: {ins.OpCode,-14} {ins}");
    }

    Console.WriteLine();
    Console.WriteLine("==== 复核 ②：逐补丁意图断言 ====");
    Patcher.Gate(mm, fails);
}

Console.WriteLine();
if (fails.Count == 0)
{
    Console.WriteLine("[gate] 补丁自检 PASS —— 可以进入 .a 内嵌 → so 构建 → 外科出包");
    return;
}
Console.WriteLine($"[gate] 补丁自检 FAIL（{fails.Count} 项），禁止出包：");
foreach (var f in fails) Console.WriteLine($"       - {f}");
Console.WriteLine("       修 Patch.cs 后重跑；不要带着 FAIL 去 llvm-objcopy/ndk-build/装机。");
Environment.Exit(1);
