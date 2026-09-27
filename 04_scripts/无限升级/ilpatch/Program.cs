using Mono.Cecil;

// 2026-09-27 晚间回滚适配：官方 dll 从 hotfix3 2028544 回滚到 hotfix2 2030080。
// v7.18 的单版本策略不变：只重放当前官方版，不内置第二份 dll。
// GameData.AddItem 的锚点仍按「参数0=string 且 参数1=long」匹配主路径。
var src = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_OFFICIAL_12.10701_hotfix2_2030080.dll";
var dst = "/Users/aliang/Documents/nixiang/07_patched/无限升级/targetfuntional_v1_20260927_hotfix2.dll";
if (args.Length == 2) { var a = AssemblyDefinition.ReadAssembly(src); Dumper.DumpIL(a, args[0], args[1]); return; }
Patcher.Run(src, dst);
