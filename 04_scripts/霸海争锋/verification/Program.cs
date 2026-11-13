using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// 霸海争锋 b8 离线成员引用校验（游戏私有，只读）
// 目标：Starry.BhzfPatch 与 Battle.Life::AddHP 注入段里所有 call/callvirt/newobj 的 MemberRef，
//       逐个解析到真实目标程序集（DummyDll 的 mscorlib/UnityEngine.CoreModule + mygame），
//       核对 scope / HasThis / 返回类型 / 参数类型，缺依赖与签名不符分开计数。
internal static class Program
{
    private static int Main(string[] args)
    {
        var asmPath = args.Length > 0 ? args[0] : "/Users/aliang/Documents/nixiang/07_patched/霸海争锋/rc_b8/mygame_bhzf_dev_rc_b8.dll";
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(asmPath));
        resolver.AddSearchDirectory("/Users/aliang/Documents/nixiang/06_dumped/霸海争锋/1.1.036_il2cpp/dump/DummyDll");
        resolver.AddSearchDirectory("/Users/aliang/Documents/nixiang/06_dumped/霸海争锋");
        var asm = AssemblyDefinition.ReadAssembly(asmPath, new ReaderParameters { AssemblyResolver = resolver });
        var mod = asm.MainModule;

        var scopeInfo = new Dictionary<string, string>();
        var targets = new List<MethodDefinition>();
        var helper = mod.GetType("Starry.BhzfPatch");
        if (helper != null) targets.AddRange(helper.Methods.Where(m => m.HasBody));
        var life = mod.GetType("Battle.Life");
        var addHp = life?.Methods.FirstOrDefault(m => m.Name == "AddHP" && m.Parameters.Count == 3);
        if (addHp != null) targets.Add(addHp);

        int ok = 0, missing = 0, sigBad = 0, skipped = 0;
        Console.WriteLine("kind\tcaller\tmember\tdeclScope\tresult");
        foreach (var m in targets)
        foreach (var ins in m.Body.Instructions)
        {
            if (ins.OpCode.Code != Code.Call && ins.OpCode.Code != Code.Callvirt && ins.OpCode.Code != Code.Newobj) continue;
            if (ins.Operand is not MethodReference mr) { continue; }
            var decl = mr.DeclaringType;
            if (decl is TypeSpecification || mr is GenericInstanceMethod)
            {
                ++skipped;
                Console.WriteLine($"skip\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{decl.GetType().Name}\tgeneric/typespec(未校验)");
                continue;
            }
            ModuleDefinition targetMod = null;
            string scopeName = "?";
            try
            {
                if (decl.Scope is AssemblyNameReference anr)
                {
                    targetMod = resolver.Resolve(anr).MainModule;
                    scopeName = anr.Name;
                    if (!scopeInfo.ContainsKey(anr.Name))
                    {
                        var f = targetMod.FileName;
                        var sha = "";
                        try { using var fs = File.OpenRead(f); using var h = System.Security.Cryptography.SHA256.Create(); sha = Convert.ToHexString(h.ComputeHash(fs)).ToLowerInvariant(); } catch { }
                        scopeInfo[anr.Name] = f + " sha256=" + sha;
                    }
                }
                else if (decl.Scope is ModuleDefinition md2) { targetMod = md2; scopeName = "(self)"; }
            }
            catch (Exception ex) { scopeName = "resolve-fail:" + ex.GetType().Name; }

            var td = targetMod?.GetType(decl.FullName) ?? targetMod?.GetType(decl.Namespace, decl.Name);
            if (td == null)
            {
                ++missing;
                Console.WriteLine($"missing\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{scopeName}\tDeclaringType 未找到");
                continue;
            }
            var cand = td.Methods.Where(x => x.Name == mr.Name && x.Parameters.Count == mr.Parameters.Count).ToList();
            var exact = cand.FirstOrDefault(x =>
                x.ReturnType.FullName == mr.ReturnType.FullName &&
                x.IsStatic == !mr.HasThis &&
                x.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(mr.Parameters.Select(p => p.ParameterType.FullName)));
            if (exact == null)
            {
                ++sigBad;
                var got = cand.Count == 0 ? "无同名同参方法" :
                    $"候选 {cand.Count} 个，签名不符（首个 {cand[0].ReturnType.Name} static={cand[0].IsStatic} argc={cand[0].Parameters.Count}）";
                Console.WriteLine($"sigbad\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{scopeName}\t{got}");
                continue;
            }
            ++ok;
            Console.WriteLine($"ok\t{m.DeclaringType.Name}.{m.Name}\t{mr.Name}\t{scopeName}\tHasThis={mr.HasThis} static={exact.IsStatic} ret={exact.ReturnType.Name} argc={exact.Parameters.Count}");
        }

        Console.WriteLine();
        Console.WriteLine("RESOLVED-SCOPES");
        foreach (var kv in scopeInfo) Console.WriteLine($"  {kv.Key} -> {kv.Value}");
        Console.WriteLine();
        Console.WriteLine($"SUMMARY ok={ok} missing={missing} sigbad={sigBad} skipped={skipped}");
        return (missing + sigBad) == 0 ? 0 : 1;
    }
}
