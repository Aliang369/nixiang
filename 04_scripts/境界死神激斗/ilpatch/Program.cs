using Mono.Cecil;
using Mono.Cecil.Cil;

// 境界死神激斗（com.cmge.ssjd.x7sy 1.38.2）IL 补丁入口
//   dotnet run                                  → 默认 in/out 路径
//   dotnet run -- <src> <dst>                   → 指定 in/out
//   dotnet run -- --dump <Full.Type.Name> <method|*>   → 只打印 IL（排障用）
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
if (a.Count >= 2) { src = a[0]; dst = a[1]; }

Patcher.Run(src, dst);

// 产出复核：把打过补丁的方法前 10 条 IL 打出来，肉眼确认门控在最前
Console.WriteLine("==== 复核（补丁后 IL 头）====");
using (var v = AssemblyDefinition.ReadAssembly(dst))
{
    void Show(string tn, string mn)
    {
        var t = v.MainModule.GetType(tn);
        var m = t?.Methods.FirstOrDefault(x => x.Name == mn);
        if (m == null) { Console.WriteLine($"  !! {tn}::{mn} NOT FOUND"); return; }
        Console.WriteLine($"-- {tn}::{mn}");
        foreach (var ins in m.Body.Instructions.Take(10))
            Console.WriteLine($"     IL_{ins.Offset:X4}: {ins.OpCode,-12} {ins}");
    }
    Show("BleachBattle.ActorDataControl", "get_IsInvincible");
    Show("BleachBattle.ActorDataControl", "ModifyHealth");
    Show("BleachBattle.ActorDataControl", "_finalAttrImpl");
    Show("BleachBattle.SkillInstance", "get_iSkillState");
    Show("BleachBattle.SkillInstance", "get_CDLeftTimeUs");
    var cfg = v.MainModule.GetType("Starry.StarryConfig");
    Console.WriteLine($"-- Starry.StarryConfig: {(cfg == null ? "MISSING" : string.Join(",", cfg.Methods.Select(x => x.Name)))}");
}
