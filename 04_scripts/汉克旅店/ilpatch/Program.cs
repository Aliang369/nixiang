using Mono.Cecil;

// 汉克旅店 1.1.0 — 热更程序集 IL 补丁入口
//   dotnet run                                → 默认 in/out
//   dotnet run -- <src> <dst>                 → 指定 in/out
//   dotnet run -- --dump <Full.Type> <method|*> → 只打印 IL（排障/核签名）
const string root = "/Users/aliang/Documents/nixiang";
// ★基线说明（2026-11-13 真机实锤）：包内 bundle 里那份是 5,328,896 B（d2e7edf0…），
// 但**运行时实际装载**的是热更下发的 5,328,384 B（263e11eb…，已由 mod 落盘 hkld_seen_5328384.dll
// 拉回并存为 Assembly-CSharp_runtime.dll）。两份从偏移 136 起就不同（388 万字节差异）⇒ 补丁必须
// 打在**运行时那份**上，否则装上去只会被 native 的版本判据挡掉（PASSTHROUGH）。
var src = Environment.GetEnvironmentVariable("HKLD_SRC") is { Length: > 0 } e ? e
        : root + "/06_dumped/汉克旅店/Assembly-CSharp_runtime.dll";
var dst = root + "/07_patched/汉克旅店/Assembly-CSharp_hkld.dll";

var a = new List<string>(args);
if (a.Count > 0 && a[0] == "--dump")
{
    var tName = a.Count > 1 ? a[1] : "PlayerManager";
    var mName = a.Count > 2 ? a[2] : "*";
    Dumper.DumpIL(AssemblyDefinition.ReadAssembly(src), tName, mName);
    return;
}
if (a.Count >= 2) { src = a[0]; dst = a[1]; }

Patcher.Run(src, dst);

Console.WriteLine();
Console.WriteLine("==== 复核：补丁方法完整 IL ====");
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
        int shown = 0;
        foreach (var ins in m.Body.Instructions)
        {
            if (shown++ > 40) { Console.WriteLine("     …(完整 IL 用 --dump)"); break; }
            Console.WriteLine($"     IL_{ins.Offset:X4}: {ins.OpCode,-14} {ins}");
        }
    }
    Console.WriteLine();
    Console.WriteLine("==== 逐补丁意图断言 ====");
    Patcher.Gate(mm, fails);
}

Console.WriteLine();
if (fails.Count == 0)
{
    Console.WriteLine("[gate] 补丁自检 PASS —— 可进入 .a 内嵌 → so 构建 → 外科出包");
    return;
}
Console.WriteLine($"[gate] 补丁自检 FAIL（{fails.Count} 项），禁止出包：");
foreach (var f in fails) Console.WriteLine($"       - {f}");
Environment.Exit(1);
