using Mono.Cecil;

// 2026-09-23 12.10701 适配：官方热更 dll 与 11.266020 逐字节相同（sha256 1cecb470…），
// src 指向本轮真机 DIAG dump 归档；本轮在登记表新增「无限挑战秘境 mijing_free」→ 产出新 v1。
var src = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_OFFICIAL_12.10701.dll";
var dst = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_v1_20260923.dll";
if (args.Length == 2) { var a = AssemblyDefinition.ReadAssembly(src); Dumper.DumpIL(a, args[0], args[1]); return; }
Patcher.Run(src, dst);
