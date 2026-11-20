using Mono.Cecil;
using Mono.Cecil.Cil;

// 冒险战争 (com.mxzz.zbml) 热更程序集 IL 补丁器 —— 入口
//
//   用法（路径全部显式，禁写死）：
//     dotnet run -- <src.dll> <dst.dll> [--pkg com.mxzz.zbml]
//     dotnet run -- --dump <Full.Type.Name> <method|*> [<src.dll>]     只看 IL（读输入，只读排障）
//     dotnet run -- --find <regex> [<src.dll>]                          列出匹配的成员全名（只读）
//     dotnet run -- --probe [<src.dll>]                                 诊断写盘期引用解析
//
//   退出码：0 = 补丁成功且自检 PASS；1 = 自检 FAIL；2 = 用法错误；3 = 静态结论未落地（本阶段预期）
//
// 本文件兼任「出包前自检门禁」：把每个补丁方法**完整** IL 打到 stdout（旧版只打前 10 条，
// 而 starg 往往在第 11 条之后，曾因此写坏相邻参数），再逐补丁跑意图断言（Patcher.Gate）。
var argsList = new List<string>(args);
string pkg = "com.mxzz.zbml";
string src = Environment.GetEnvironmentVariable("MXZZ_SRC") ?? "";
string dst = Environment.GetEnvironmentVariable("MXZZ_DST") ?? "";

// --pkg 可在任意位置
int pi = argsList.IndexOf("--pkg");
if (pi >= 0 && pi + 1 < argsList.Count) { pkg = argsList[pi + 1]; argsList.RemoveRange(pi, 2); }

if (argsList.Count > 0 && argsList[0] == "--dump")
{
    var tName = argsList.Count > 1 ? argsList[1] : "";
    var mName = argsList.Count > 2 ? argsList[2] : "*";
    var p0 = argsList.Count > 3 ? argsList[3] : src;
    if (string.IsNullOrEmpty(p0) || !File.Exists(p0)) { Console.Error.WriteLine("[err] 缺输入 dll（--dump 第 4 参数或 MXZZ_SRC）"); return 2; }
    using var asm0 = AssemblyDefinition.ReadAssembly(p0);
    Console.WriteLine($"[in ] {p0}  ({new FileInfo(p0).Length} B)  assembly={asm0.Name.Name}");
    Dumper.DumpIL(asm0, tName, mName);
    return 0;
}

// --diff-all：全程序集 IL 级对照（只读）——确认补丁只改了预期方法。
//   逐类型逐方法比对 opcode 与操作数；报「只在一侧存在的方法」「IL 不同的方法」。
//   预期输出：不同的方法只有 Battle.Life::AddHP；只在 patched 侧存在的类型是 Starry.StarryConfig / Starry.MxzzPatch。
if (argsList.Count > 0 && argsList[0] == "--diff-all")
{
    if (argsList.Count < 3) { Console.Error.WriteLine("[err] 用法: --diff-all <orig.dll> <patched.dll>"); return 2; }
    var f1 = argsList[1]; var f2 = argsList[2];
    using var b1 = AssemblyDefinition.ReadAssembly(f1);
    using var b2 = AssemblyDefinition.ReadAssembly(f2);
    var map1 = new Dictionary<string, MethodDefinition>();
    foreach (var t in b1.MainModule.GetTypes())
        foreach (var m in t.Methods) map1[t.FullName + "::" + m.Name + "/" + m.Parameters.Count] = m;
    var map2 = new Dictionary<string, MethodDefinition>();
    foreach (var t in b2.MainModule.GetTypes())
        foreach (var m in t.Methods) map2[t.FullName + "::" + m.Name + "/" + m.Parameters.Count] = m;
    int onlyOne = 0, onlyTwo = 0, ilDiff = 0, same = 0;
    var diffNames = new List<string>();
    foreach (var kv in map2)
    {
        if (!map1.TryGetValue(kv.Key, out var m1)) { onlyTwo++; if (diffNames.Count < 20) diffNames.Add("+ " + kv.Key); continue; }
        var m2 = kv.Value;
        if (!m1.HasBody || !m2.HasBody) { same++; continue; }
        var a = m1.Body.Instructions; var b = m2.Body.Instructions;
        if (a.Count != b.Count) { ilDiff++; if (diffNames.Count < 20) diffNames.Add("IL≠ " + kv.Key + " (" + a.Count + "→" + b.Count + ")"); continue; }
        bool eq = true;
        for (int i = 0; i < a.Count && eq; i++)
        {
            if (a[i].OpCode != b[i].OpCode) { eq = false; break; }
            if ((a[i].Operand?.ToString() ?? "") != (b[i].Operand?.ToString() ?? "")) eq = false;
        }
        if (eq) same++; else { ilDiff++; if (diffNames.Count < 20) diffNames.Add("IL≠ " + kv.Key); }
    }
    foreach (var kv in map1) if (!map2.ContainsKey(kv.Key)) { onlyOne++; if (diffNames.Count < 40) diffNames.Add("- " + kv.Key); }
    Console.WriteLine($"[diff-all] methods: 相同={same}  IL不同={ilDiff}  只在 patched={onlyTwo}  只在 orig={onlyOne}");
    foreach (var n in diffNames) Console.WriteLine("   " + n);
    return (ilDiff == 0 && onlyOne == 0) ? 0 : 1;
}

// --cmp：代码反推验证（只读）——「补丁体 == 原方法体 + N 条前缀」的结构证明
//   判据：逐条比对 patched[i+N] 与 orig[i] 的 opcode 与操作数；分支目标按**索引**对齐
//   （偏移整体平移 N 条，不能用偏移比较）。任一不一致即 exit 1。
if (argsList.Count > 0 && argsList[0] == "--cmp")
{
    if (argsList.Count < 5) { Console.Error.WriteLine("[err] 用法: --cmp <orig.dll> <patched.dll> <Type> <Method>"); return 2; }
    var fOrig = argsList[1]; var fPat = argsList[2]; var tName = argsList[3]; var mName = argsList[4];
    if (!File.Exists(fOrig) || !File.Exists(fPat)) { Console.Error.WriteLine("[err] 输入不存在"); return 2; }
    using var a1 = AssemblyDefinition.ReadAssembly(fOrig);
    using var a2 = AssemblyDefinition.ReadAssembly(fPat);
    var m1 = a1.MainModule.GetType(tName)?.Methods.FirstOrDefault(x => x.Name == mName && x.Parameters.Count == 3);
    var m2 = a2.MainModule.GetType(tName)?.Methods.FirstOrDefault(x => x.Name == mName && x.Parameters.Count == 3);
    if (m1 == null || m2 == null) { Console.Error.WriteLine("[err] 方法未找到"); return 2; }
    var i1 = m1.Body.Instructions.ToList();
    var i2 = m2.Body.Instructions.ToList();
    int n = i2.Count - i1.Count;
    Console.WriteLine($"[cmp] {tName}::{mName}  original IL={i1.Count}  patched IL={i2.Count}  prefix={n}");
    Console.WriteLine($"[cmp] maxstack {m1.Body.MaxStackSize} -> {m2.Body.MaxStackSize}   initlocals {m1.Body.InitLocals} -> {m2.Body.InitLocals}   locals {m1.Body.Variables.Count} -> {m2.Body.Variables.Count}   EH {m1.Body.ExceptionHandlers.Count} -> {m2.Body.ExceptionHandlers.Count}");
    if (n < 0) { Console.Error.WriteLine("[cmp] FAIL: 补丁体比原体更短（有指令被删除）"); return 1; }
    int idx1(Instruction x) => i1.IndexOf(x);
    int idx2(Instruction x) => i2.IndexOf(x);
    int bad = 0, branch = 0;
    for (int i = 0; i < i1.Count; i++)
    {
        var a = i1[i]; var b = i2[i + n];
        if (a.OpCode != b.OpCode) { Console.Error.WriteLine($"[cmp] FAIL opcode @orig[{i}] IL_{a.Offset:X4}: {a.OpCode} vs {b.OpCode}"); bad++; if (bad > 8) break; continue; }
        if (a.Operand is Instruction ta && b.Operand is Instruction tb)
        {
            branch++;
            if (idx2(tb) != idx1(ta) + n) { Console.Error.WriteLine($"[cmp] FAIL 分支目标 @orig[{i}]: {idx1(ta)}+{n} vs {idx2(tb)}"); bad++; }
        }
        else if ((a.Operand?.ToString() ?? "") != (b.Operand?.ToString() ?? ""))
        { Console.Error.WriteLine($"[cmp] FAIL 操作数 @orig[{i}] IL_{a.Offset:X4}: '{a.Operand}' vs '{b.Operand}'"); bad++; if (bad > 8) break; }
    }
    // locals 前缀必须一脉相承（新增局部只在尾部追加）
    for (int i = 0; i < m1.Body.Variables.Count; i++)
        if (m1.Body.Variables[i].VariableType.FullName != m2.Body.Variables[i].VariableType.FullName)
        { Console.Error.WriteLine($"[cmp] FAIL local V{i} 类型变化"); bad++; }
    Console.WriteLine($"[cmp] 比对 {i1.Count} 条（其中分支 {branch} 条）→ 差异 {bad} 条");
    return bad == 0 ? 0 : 1;
}

// --info：静态结论核对用（只读）——token / IL 条数 / maxstack / locals / EH / 字段表
if (argsList.Count > 0 && argsList[0] == "--info")
{
    var tName = argsList.Count > 1 ? argsList[1] : "";
    var mName = argsList.Count > 2 ? argsList[2] : "";
    var p0 = argsList.Count > 3 ? argsList[3] : src;
    if (string.IsNullOrEmpty(p0) || !File.Exists(p0)) { Console.Error.WriteLine("[err] 用法: --info <Type> <Method|*> <dll>"); return 2; }
    using var asm = AssemblyDefinition.ReadAssembly(p0);
    Console.WriteLine($"[in ] {p0}  ({new FileInfo(p0).Length} B)  assembly={asm.Name.Name}");
    var t = asm.MainModule.GetType(tName);
    if (t == null) { Console.Error.WriteLine($"[err] type {tName} not found"); return 2; }
    Console.WriteLine($"[type] {t.FullName}  token=0x{t.MetadataToken.ToInt32():X8}  fields={t.Fields.Count} methods={t.Methods.Count}");
    foreach (var f in t.Fields)
        Console.WriteLine($"  F 0x{f.MetadataToken.ToInt32():X8}  {f.Name} : {f.FieldType.FullName}{(f.IsStatic ? "  [static]" : "")}" +
                          (f.HasConstant ? $"  = {f.Constant}" : ""));
    foreach (var m in t.Methods)
    {
        if (mName != "*" && m.Name != mName) continue;
        var ps = string.Join(", ", m.Parameters.Select(x => $"{x.ParameterType.FullName} {x.Name}"));
        Console.WriteLine($"  M 0x{m.MetadataToken.ToInt32():X8}  {m.Name}({ps}) -> {m.ReturnType.FullName}" +
                          (m.HasThis ? "  [instance]" : "  [static]"));
        if (!m.HasBody) { Console.WriteLine("      (no body)"); continue; }
        var b = m.Body;
        var eh = b.ExceptionHandlers.Count;
        Console.WriteLine($"      IL={b.Instructions.Count}  maxstack={b.MaxStackSize}  initlocals={b.InitLocals}  locals={b.Variables.Count}  EH={eh}");
        for (int i = 0; i < b.Variables.Count; i++)
            Console.WriteLine($"      V{i}: {b.Variables[i].VariableType.FullName}");
        foreach (var h in b.ExceptionHandlers)
            Console.WriteLine($"      EH {h.HandlerType}: try IL_{h.TryStart?.Offset:X4}..IL_{h.TryEnd?.Offset:X4} handler IL_{h.HandlerStart?.Offset:X4}..IL_{h.HandlerEnd?.Offset:X4} catch={h.CatchType?.FullName}");
    }
    return 0;
}

// --find：按正则列出成员全名（只读；给静态定位用，不做任何结论推断）
if (argsList.Count > 0 && argsList[0] == "--find")
{
    var re = argsList.Count > 1 ? argsList[1] : "";
    var p0 = argsList.Count > 2 ? argsList[2] : src;
    if (string.IsNullOrEmpty(re) || string.IsNullOrEmpty(p0) || !File.Exists(p0)) { Console.Error.WriteLine("[err] 用法: --find <regex> <dll>"); return 2; }
    var rx = new System.Text.RegularExpressions.Regex(re, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    int n = 0;
    using var asm1 = AssemblyDefinition.ReadAssembly(p0);
    foreach (var t in asm1.MainModule.GetTypes())
    {
        if (rx.IsMatch(t.FullName)) { Console.WriteLine($"T {t.FullName}"); n++; }
        foreach (var m in t.Methods)
            if (rx.IsMatch(t.FullName + "::" + m.Name))
            {
                Console.WriteLine($"M {t.FullName}::{m.Name}({string.Join(",", m.Parameters.Select(x => x.ParameterType.Name))}) -> {m.ReturnType.Name}{(m.HasBody ? "" : "  [no body]")}");
                n++;
            }
        foreach (var f in t.Fields)
            if (rx.IsMatch(t.FullName + "::" + f.Name)) { Console.WriteLine($"F {t.FullName}::{f.Name} : {f.FieldType.Name}"); n++; }
    }
    Console.WriteLine($"[find] {n} 命中（regex={re}）");
    return 0;
}

if (argsList.Count > 0 && argsList[0] == "--probe")
{
    var p0 = argsList.Count > 1 ? argsList[1] : src;
    if (string.IsNullOrEmpty(p0) || !File.Exists(p0)) { Console.Error.WriteLine("[err] 缺输入 dll"); return 2; }
    using var asm = AssemblyDefinition.ReadAssembly(p0, new ReaderParameters { ReadSymbols = false });
    int nParam = 0, nField = 0; var bad = new List<string>();
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
    }
    Console.WriteLine($"[probe] 带常量的成员：param={nParam} field={nField}");
    Console.WriteLine($"[probe] 类型解析失败 = {bad.Count}");
    foreach (var b in bad.Take(60)) Console.WriteLine("   " + b);
    return 0;
}

if (argsList.Count >= 2) { src = argsList[0]; dst = argsList[1]; }
if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(dst))
{
    Console.Error.WriteLine("用法: mxzz_ilpatch <src.dll> <dst.dll> [--pkg com.mxzz.zbml]");
    return 2;
}
if (!File.Exists(src)) { Console.Error.WriteLine($"[err] 输入不存在: {src}"); return 2; }

try
{
    Patcher.Run(src, dst, pkg);
}
catch (StaticConclusionPendingException e)
{
    Console.Error.WriteLine("[blocked] " + e.Message);
    Console.Error.WriteLine("          静态结论落地前不允许产出补丁体（用户 2026-11-19 硬要求）。");
    return 3;
}

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
        if (m == null) { Console.WriteLine($"-- {tn}::{mn}  (NOT FOUND)"); fails.Add($"{tn}::{mn} NOT FOUND"); continue; }
        Console.WriteLine($"-- {tn}::{mn}  ({m.Body.Instructions.Count} 条 IL, locals={m.Body.Variables.Count})");
        foreach (var ins in m.Body.Instructions)
            Console.WriteLine($"     IL_{ins.Offset:X4}: {ins.OpCode,-14} {ins}");
    }

    Console.WriteLine();
    Console.WriteLine("==== 复核 ②：逐补丁意图断言 ====");
    Patcher.Gate(mm, fails);
}

Console.WriteLine();
if (fails.Count == 0 && Patcher.Targets.Length > 0)
{
    Console.WriteLine("[gate] 补丁自检 PASS —— 可以进入 .a 内嵌 → so 构建 → 外科出包");
    return 0;
}
if (Patcher.Targets.Length == 0)
{
    Console.Error.WriteLine("[gate] FAIL：补丁清单为空，本产物不是补丁体（禁止出包）");
    return 1;
}
Console.WriteLine($"[gate] 补丁自检 FAIL（{fails.Count} 项），禁止出包：");
foreach (var f in fails) Console.WriteLine($"       - {f}");
Console.WriteLine("       修 Patch.cs 后重跑；不要带着 FAIL 去 llvm-objcopy/ndk-build/装机。");
return 1;

internal sealed class StaticConclusionPendingException : Exception
{
    public StaticConclusionPendingException(string m) : base(m) { }
}
