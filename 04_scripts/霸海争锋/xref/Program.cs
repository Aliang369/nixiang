using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

// 霸海争锋 私有 xref 工具（只读）：按 目标类型名.方法名 过滤 IL 中的引用
// 用法:
//   dotnet run --project . -- <assembly...> --type ClientLife --method set_HP [--method set_HPPercent]
//   dotnet run --project . -- <assembly...> --type SkillAction.UnitMgr --method CreateUnit
// 命中形态: call / callvirt / ldftn / ldvirtftn（含 MethodSpec 泛型实例化）
internal static class Program
{
    private static int Main(string[] args)
    {
        var asms = new List<string>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        var types = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--type") types.Add(args[++i]);
            else if (args[i] == "--method") names.Add(args[++i]);
            else asms.Add(args[i]);
        }
        if (asms.Count == 0 || names.Count == 0)
        {
            Console.Error.WriteLine("usage: bhzf_xref <asm.dll...> --type <TypeName> [--type ...] --method <method> [--method ...]");
            return 2;
        }
        Console.WriteLine("caller_type\tcaller_method\tmethod_token\til_offset\tform\tcallee\treceiver_hint\targs_hint");
        foreach (var path in asms)
        {
            AssemblyDefinition asm;
            try { asm = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = false }); }
            catch (Exception ex) { Console.Error.WriteLine($"read fail {path}: {ex.Message}"); continue; }
            foreach (var mod in asm.Modules)
            foreach (var t in mod.Types.SelectMany(Flatten))
            foreach (var m in t.Methods)
            {
                if (!m.HasBody) continue;
                var body = m.Body;
                foreach (var ins in body.Instructions)
                {
                    MethodReference mr = null;
                    string form = null;
                    if (ins.OpCode.Code == Code.Call || ins.OpCode.Code == Code.Callvirt)
                    { mr = ins.Operand as MethodReference; form = ins.OpCode.Code.ToString().ToLowerInvariant(); }
                    else if (ins.OpCode.Code == Code.Ldftn || ins.OpCode.Code == Code.Ldvirtftn)
                    { mr = ins.Operand as MethodReference; form = ins.OpCode.Code.ToString().ToLowerInvariant(); }
                    if (mr == null) continue;
                    var decl = mr.DeclaringType;
                    bool typeOk = types.Count == 0 || types.Contains(decl.FullName) || types.Contains(decl.Name);
                    bool nameOk = names.Contains(mr.Name);
                    if (!typeOk || !nameOk) continue;
                    var recv = DescribeReceiver(body, ins);
                    var argv = DescribeArgs(body, ins, mr);
                    Console.WriteLine($"{t.FullName}\t{m.FullName}\t0x{m.MetadataToken.ToInt32():X8}\tIL_{ins.Offset:X4}\t{form}\t{mr.FullName}\t{recv}\t{argv}");
                }
            }
        }
        return 0;
    }

    private static IEnumerable<TypeDefinition> Flatten(TypeDefinition t)
    {
        yield return t;
        foreach (var n in t.NestedTypes)
        foreach (var x in Flatten(n)) yield return x;
    }

    private static Instruction Prev(Instruction ins, int n)
    {
        var cur = ins;
        for (int i = 0; i < n && cur != null; i++) cur = cur.Previous;
        return cur;
    }

    private static string DescribeReceiver(MethodBody body, Instruction call)
    {
        // 粗略回溯 4 条：看 receiver 来自 ldarg / ldfld / ldloc / call
        var hints = new List<string>();
        for (int k = 1; k <= 6; k++)
        {
            var p = Prev(call, k);
            if (p == null) break;
            if (p.OpCode.Code == Code.Ldarg_0 || p.OpCode.Code == Code.Ldarg || p.OpCode.Code == Code.Ldarg_S)
                hints.Add("arg:" + (p.Operand?.ToString() ?? "0"));
            else if (p.OpCode.Code == Code.Ldfld || p.OpCode.Code == Code.Ldflda)
                hints.Add("field:" + ((FieldReference)p.Operand).Name);
            else if (p.OpCode.Code == Code.Ldloc || p.OpCode.Code == Code.Ldloc_S || p.OpCode.Code == Code.Ldloc_0 || p.OpCode.Code == Code.Ldloc_1 || p.OpCode.Code == Code.Ldloc_2 || p.OpCode.Code == Code.Ldloc_3)
                hints.Add("local:" + (p.Operand?.ToString() ?? p.OpCode.Name));
            else if (p.OpCode.Code == Code.Call || p.OpCode.Code == Code.Callvirt)
            { hints.Add("call:" + ((MethodReference)p.Operand).Name); break; }
        }
        hints.Reverse();
        return string.Join(">", hints);
    }

    private static string DescribeArgs(MethodBody body, Instruction call, MethodReference mr)
    {
        var hints = new List<string>();
        for (int k = 1; k <= 4; k++)
        {
            var p = Prev(call, k);
            if (p == null) break;
            if (p.OpCode.Code == Code.Ldc_I4 || p.OpCode.Code == Code.Ldc_I4_S || p.OpCode.Code == Code.Ldc_I8)
                hints.Add("const:" + p.Operand);
            else if (p.OpCode.Code == Code.Ldarg || p.OpCode.Code == Code.Ldarg_S)
                hints.Add("arg:" + p.Operand);
            else if (p.OpCode.Code == Code.Ldfld)
                hints.Add("field:" + ((FieldReference)p.Operand).Name);
            else if (p.OpCode.Code == Code.Call || p.OpCode.Code == Code.Callvirt)
            { hints.Add("call:" + ((MethodReference)p.Operand).Name); break; }
        }
        hints.Reverse();
        return string.Join(">", hints);
    }
}
