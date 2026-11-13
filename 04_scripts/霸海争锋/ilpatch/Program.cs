using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

// 霸海争锋 1.1.036 热更程序集 IL 补丁器（游戏私有）
// 产物: 07_patched/霸海争锋/mygame_bhzf.dll —— 由 native 在 Assembly.Load(byte[]) 边界替换
internal static class Program
{
    private const string Root = "/Users/aliang/Documents/nixiang";
    private static int Main(string[] args)
    {
        var src = Environment.GetEnvironmentVariable("BHZF_SRC") is { Length: > 0 } s ? s
                : Root + "/06_dumped/霸海争锋/mygame_runtime.dll";
        var dst = Root + "/07_patched/霸海争锋/mygame_bhzf.dll";
        if (args.Length > 0 && args[0] == "--dump")
        {
            using var a0 = AssemblyDefinition.ReadAssembly(src);
            var t = a0.MainModule.GetType(args[1]);
            foreach (var m in t.Methods.Where(x => args.Length < 3 || x.Name == args[2]))
            {
                Console.WriteLine($"== {t.FullName}::{m.Name} ({m.Parameters.Count} params) ret={m.ReturnType.FullName}");
                if (!m.HasBody) continue;
                foreach (var ins in m.Body.Instructions.Take(int.Parse(Environment.GetEnvironmentVariable("BHZF_DUMP_N") ?? "40")))
                    Console.WriteLine($"   IL_{ins.Offset:X4}: {ins.OpCode,-14} {ins}");
            }
            return 0;
        }
        if (args.Length >= 2) { src = args[0]; dst = args[1]; }
        Patcher.Run(src, dst);
        return 0;
    }
}
