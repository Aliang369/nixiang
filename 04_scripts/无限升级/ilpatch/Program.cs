using Mono.Cecil;

// 2026-09-23 第二次热更适配：官方 dll 1960960 → 2030080 B（sha256 66866590…），
//   src = 本轮真机 DIAG dump 归档（hotfix2）；本次重打 v1 → 新 v1 DLL。
//   注意：GameData.AddItem 签名被官方改写（新增「道具来源」字符串参数），
//   v1 锚点已改为按「参数0=string 且 参数1=long」匹配主路径。
var src = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_OFFICIAL_12.10701_hotfix2_2030080.dll";
var dst = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_v1_20260923_hotfix2.dll";
if (args.Length == 2) { var a = AssemblyDefinition.ReadAssembly(src); Dumper.DumpIL(a, args[0], args[1]); return; }
Patcher.Run(src, dst);
