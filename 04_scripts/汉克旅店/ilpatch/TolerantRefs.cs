using Mono.Cecil;

// 写盘期「宽容解析器」+ 缺引用程序集垫片工厂。
//
// ── 问题 ────────────────────────────────────────────────────────────────
// `asm.Write(dst)` 抛：
//   Mono.Cecil.AssemblyResolutionException: Failed to resolve assembly: 'Unity.ThirdParty, Version=0.0.0.0…'
//   栈：MetadataBuilder.GetConstantType ← Mixin.CheckedResolve ← TypeReference.Resolve
//        ← MetadataResolver.Resolve ← DefaultAssemblyResolver.Resolve ← BaseAssemblyResolver.Resolve
//        ← … ← AddParameter ← AddParameters ← AddMethod ← AddMethods ← AddType ← BuildMetadata
//
// 根因：Cecil 建 Param/Field/Property 表时，对**每个**成员都走 `GetConstantType`，而该函数对
//       「无常量」哨兵不做提前返回，直接 `CheckedResolve(成员类型)`。于是只要模块里**任何**一个
//       成员的类型属于解析不到的引用程序集，整个写盘就中止——哪怕那个成员与本次补丁毫无关系。
//       本包实测 67 处（`RewardItemInfoComonpent::SetValue1(onClick : EventDelegate/Callback)`、
//       `AlertMgr::OpenYesOrNoPanel(callback : Callback`1<System.Boolean>)` … 全是 UI 回调委托）。
//       ⇒ 这不是「补丁写错」，而是「没有全套引用程序集就不能落盘」。
//
// ── 处置（三层）─────────────────────────────────────────────────────────
// ① ShimFactory：写盘前扫描「**真有**默认常量、但类型解析不到」的成员，按常量自身的 CLR 类型
//    反推该类型是哪个枚举，生成同名垫片程序集（`Unity.ThirdParty.dll` …）放进搜索目录。
//    真实读入的垫片类型带正确的 is_enum/value__，常量原样保留 ⇒ 输出与「有全套 DummyDll」时一致。
//    **这一层保证保真。**
// ② TolerantAssemblyResolver / TolerantMetadataResolver：垫片也覆盖不到的引用（例如无常量成员的
//    类型、嵌套委托类）由合成空壳兜底，**永不抛**，让写盘能走完。**这一层保证不中断。**
// ③ ConstantAudit：写盘前后逐条比对「真实默认值」，少一条即 FAIL 并阻断出包。**这一层是安全网。**
internal static class TolerantRefs
{
    // 常量 CLR 类型 → Cecil 基元名
    public static string PrimitiveOf(object constant)
    {
        switch (constant)
        {
            case bool: return "System.Boolean";
            case char: return "System.Char";
            case sbyte: return "System.SByte";
            case byte: return "System.Byte";
            case short: return "System.Int16";
            case ushort: return "System.UInt16";
            case int: return "System.Int32";
            case uint: return "System.UInt32";
            case long: return "System.Int64";
            case ulong: return "System.UInt64";
            case float: return "System.Single";
            case double: return "System.Double";
            case string: return "System.String";
            default: return null;
        }
    }

    // Cecil 的「无常量」哨兵是裸 object 实例（Mixin.NoValue），据此区分真常量。
    public static bool IsRealConstant(bool hasConstant, object value) =>
        hasConstant && value != null && value.GetType() != typeof(object);
}

/// <summary>缺引用程序集垫片：写盘前把「解析不到但有真常量」的类型补成一个真 dll。</summary>
internal static class ShimFactory
{
    public sealed class Need
    {
        public string ScopeName; public Version ScopeVersion;
        public string Namespace; public string[] Path; public string Primitive;   // Path: 由外到内（嵌套类型链）
    }

    /// <summary>用「只含真实目录」的探测解析器找出必须垫片的类型。</summary>
    public static List<Need> Discover(ModuleDefinition mod, IEnumerable<string> realDirs)
    {
        var probe = new DefaultAssemblyResolver();
        foreach (var d in realDirs) if (Directory.Exists(d)) probe.AddSearchDirectory(d);
        var mr = new MetadataResolver(probe);

        var needs = new Dictionary<string, Need>();
        foreach (var pair in ConstantMembers(mod))
        {
            var typeRef = pair.Item1;
            var constant = pair.Item2;
            if (typeRef == null || !(typeRef.Scope is AssemblyNameReference)) continue;
            var scope = (AssemblyNameReference)typeRef.Scope;
            bool resolvable = false;
            try { resolvable = mr.Resolve(typeRef) != null; } catch (Exception) { }
            if (resolvable) continue;

            var prim = TolerantRefs.PrimitiveOf(constant);
            if (prim == null) continue;                        // 非常量基元 → 不可能有真默认值
            if (typeRef.Namespace == "System" && typeRef.Name == prim.Substring(7)) continue; // 就是基元本身

            var key = $"{scope.Name}|{typeRef.FullName}";
            if (needs.ContainsKey(key)) continue;
            needs[key] = new Need
            {
                ScopeName = scope.Name,
                ScopeVersion = scope.Version ?? new Version(0, 0, 0, 0),
                Namespace = OutermostNamespace(typeRef),
                Path = DeclChain(typeRef),
                Primitive = prim,
            };
            Console.WriteLine($"[shim?] {scope.Name}|{typeRef.FullName} → 垫片枚举（底层 {prim}，常量 {constant}）");
        }
        return needs.Values.ToList();
    }

    static string OutermostNamespace(TypeReference t)
    {
        while (t.DeclaringType != null) t = t.DeclaringType;
        return t.Namespace ?? "";
    }

    /// <summary>由外向内的类型名链（嵌套类型必须原样重建，否则 Cecil 解析不到）。</summary>
    static string[] DeclChain(TypeReference t)
    {
        var list = new List<string>();
        while (t != null) { list.Insert(0, t.Name); t = t.DeclaringType; }
        return list.ToArray();
    }

    /// <summary>把垫片程序集写进 dir，返回写入的程序集数量。</summary>
    public static int Materialize(string dir, List<Need> needs)
    {
        if (needs.Count == 0) return 0;
        Directory.CreateDirectory(dir);
        foreach (var f in Directory.GetFiles(dir, "*.dll")) File.Delete(f);   // 全量重建，杜绝陈旧垫片

        int n = 0;
        foreach (var grp in needs.GroupBy(x => x.ScopeName))
        {
            var first = grp.First();
            var asm = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition(first.ScopeName, first.ScopeVersion), first.ScopeName, ModuleKind.Dll);
            var enumBase = new TypeReference("System", "Enum", asm.MainModule,
                asm.MainModule.TypeSystem.Object.Scope);
            int nTypes = 0;
            foreach (var it in grp)
            {
                // 由外向内重建类型链；最内层是枚举（base = System.Enum + 非静态 value__ 字段），
                // Cecil 读回来 is_enum 才为真，GetConstantType 才会用底层基元写常量。
                TypeDefinition parent = null;
                for (int i = 0; i < it.Path.Length; i++)
                {
                    var isLeaf = i == it.Path.Length - 1;
                    var outerNs = i == 0 ? it.Namespace : "";
                    var existing = (parent == null
                        ? asm.MainModule.Types.FirstOrDefault(x => x.Name == it.Path[i] && x.Namespace == outerNs)
                        : parent.NestedTypes.FirstOrDefault(x => x.Name == it.Path[i]));
                    if (existing == null)
                    {
                        var td = new TypeDefinition(outerNs, it.Path[i],
                            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                            isLeaf ? enumBase : asm.MainModule.TypeSystem.Object);
                        if (isLeaf)
                            td.Fields.Add(new FieldDefinition("value__",
                                FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
                                ResolvePrimitive(asm, it.Primitive)));
                        if (parent == null) asm.MainModule.Types.Add(td); else parent.NestedTypes.Add(td);
                        existing = td;
                        nTypes++;
                    }
                    parent = existing;
                }
            }
            var outPath = Path.Combine(dir, first.ScopeName + ".dll");
            asm.Write(outPath);
            asm.Dispose();
            Console.WriteLine($"[shim] {outPath}  types={nTypes}");
            n++;
        }
        return n;
    }

    static TypeReference ResolvePrimitive(AssemblyDefinition asm, string cecilName)
    {
        var ts = asm.MainModule.TypeSystem;
        switch (cecilName)
        {
            case "System.Boolean": return ts.Boolean;
            case "System.Char": return ts.Char;
            case "System.SByte": return ts.SByte;
            case "System.Byte": return ts.Byte;
            case "System.Int16": return ts.Int16;
            case "System.UInt16": return ts.UInt16;
            case "System.Int32": return ts.Int32;
            case "System.UInt32": return ts.UInt32;
            case "System.Int64": return ts.Int64;
            case "System.UInt64": return ts.UInt64;
            case "System.Single": return ts.Single;
            case "System.Double": return ts.Double;
            case "System.String": return ts.String;
            default: return ts.Object;
        }
    }

    /// <summary>枚举模块里所有「带真常量」的成员及其声明类型。</summary>
    public static IEnumerable<Tuple<TypeReference, object>> ConstantMembers(ModuleDefinition mod)
    {
        foreach (var t in mod.GetTypes())
        {
            foreach (var f in t.Fields)
                if (TolerantRefs.IsRealConstant(f.HasConstant, f.Constant))
                    yield return Tuple.Create(f.FieldType, f.Constant);
            foreach (var m in t.Methods)
                foreach (var p in m.Parameters)
                    if (TolerantRefs.IsRealConstant(p.HasConstant, p.Constant))
                        yield return Tuple.Create(p.ParameterType, p.Constant);
        }
    }
}

/// <summary>永不抛的程序集解析器：真实目录优先，解析不到时合成空壳。</summary>
internal sealed class TolerantAssemblyResolver : BaseAssemblyResolver
{
    readonly Dictionary<string, AssemblyDefinition> vault = new();

    public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
    {
        try
        {
            var real = base.Resolve(name, parameters);
            if (real != null) return real;
        }
        catch (AssemblyResolutionException) { /* 落到合成分支 */ }

        if (vault.TryGetValue(name.Name, out var cached))
        {
            // 搜索目录可能在合成之后才追加（垫片），此时真实解析优先
            try { var late = base.Resolve(name, parameters); if (late != null) { vault.Remove(name.Name); return late; } }
            catch (AssemblyResolutionException) { }
            return cached;
        }
        var stub = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(name.Name, name.Version ?? new Version(0, 0, 0, 0)),
            name.Name, ModuleKind.Dll);
        vault[name.Name] = stub;
        return stub;
    }

    public override AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, new ReaderParameters());
}

/// <summary>与上者配套：找不到的类型返回合成空壳（语义 = Class ⇒ 按「无常量」处理），不抛。</summary>
internal sealed class TolerantMetadataResolver : MetadataResolver
{
    readonly Dictionary<string, TypeDefinition> synth = new();

    public TolerantMetadataResolver(IAssemblyResolver assemblyResolver) : base(assemblyResolver) { }

    public override TypeDefinition Resolve(TypeReference type)
    {
        try
        {
            var real = base.Resolve(type);
            if (real != null) return real;
        }
        catch (Exception) { /* 落到合成分支 */ }

        var key = type.FullName;
        if (synth.TryGetValue(key, out var cached)) return cached;

        var home = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("SyntheticRefs", new Version(0, 0, 0, 0)), "SyntheticRefs", ModuleKind.Dll);
        var td = new TypeDefinition(type.Namespace ?? "", type.Name,
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit,
            home.MainModule.TypeSystem.Object);
        home.MainModule.Types.Add(td);
        synth[key] = td;
        return td;
    }
}

/// <summary>常量台账：写盘前记「真实默认值」，写盘后逐条复核，确保宽容解析没吞真数据。</summary>
internal static class ConstantAudit
{
    public static Dictionary<string, string> Snapshot(ModuleDefinition mod)
    {
        var map = new Dictionary<string, string>();
        foreach (var t in mod.GetTypes())
        {
            foreach (var f in t.Fields)
                if (TolerantRefs.IsRealConstant(f.HasConstant, f.Constant))
                    map[$"F {t.FullName}::{f.Name}"] = Describe(f.Constant);
            foreach (var m in t.Methods)
                foreach (var p in m.Parameters)
                    if (TolerantRefs.IsRealConstant(p.HasConstant, p.Constant))
                        map[$"P {t.FullName}::{m.Name}#{p.Index}"] = Describe(p.Constant);
        }
        return map;
    }

    static string Describe(object v) => v == null ? "<null>" : $"{v.GetType().Name}:{v}";

    /// <summary>写盘后调用：返回「丢失的真实常量」列表（应为空）。</summary>
    public static List<string> Diff(Dictionary<string, string> before, ModuleDefinition after)
    {
        var now = Snapshot(after);
        var lost = new List<string>();
        foreach (var kv in before)
        {
            string v;
            if (!now.TryGetValue(kv.Key, out v) || v != kv.Value)
                lost.Add($"{kv.Key}  {kv.Value} → {(v == null ? "<缺失>" : v)}");
        }
        return lost;
    }
}
