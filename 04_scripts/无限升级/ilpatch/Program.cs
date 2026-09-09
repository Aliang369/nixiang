using Mono.Cecil;

// 2026-09-07 热更适配：官方热更 targetfuntional.dll 1955840 -> 1960960（src 指向新原版 dump）
var src = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_NEW_20260907.dll";
var dst = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_v1_NEW_20260907.dll";
if (args.Length == 2) { var a = AssemblyDefinition.ReadAssembly(src); Dumper.DumpIL(a, args[0], args[1]); return; }
Patcher.Run(src, dst);
