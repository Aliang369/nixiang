using Mono.Cecil;

// 【绿梦：时空之声】热更程序集 IL 补丁入口
//   dotnet run -- roundtrip <src> <dst> [refdir...]   → 只读改写，测写盘可行性与尺寸增量（linchpin）
//   dotnet run -- find <src> <方法名正则> [类型名正则]  → 枚举成员（找挂点）
//   dotnet run -- dump <src> <类型全名> <方法名|*>      → 反汇编打印完整 IL
//   dotnet run -- refs <dll>                          → 打印 AssemblyRef 集合（核对 mod dll 引用）
//   dotnet run -- build <src> <dst> [--phase N] [--padsize N] [--keep-size 0|1]
//                                                     → 注入补丁 + 零填充回原长度
//   dotnet run -- pad <file> <size>                    → 只做零填充（自查用）
const string ROOT = "/Users/aliang/Documents/nixiang";
string DLLDIR = ROOT + "/06_dumped/绿梦_20261016/hotfix_dll";
string SRC = DLLDIR + "/Assembly-ACT.dll";

var a = new List<string>(args);
if (a.Count == 0) { Console.WriteLine("usage: roundtrip|find|dump|refs|build|pad ..."); return 1; }
var mode = a[0];

// 统一的宽容读入（垫片 + 宽容解析器 + 常量台账三层处置，见 TolerantRefs.cs）
(AssemblyDefinition asm, List<string> shimDirs) Open(string src, List<string> refDirs)
{
    var resolver = new TolerantAssemblyResolver();
    foreach (var d in refDirs) { resolver.AddSearchDirectory(d); Console.WriteLine($"[ref] {d}"); }
    var metaResolver = new TolerantMetadataResolver(resolver);
    var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters
    { AssemblyResolver = resolver, MetadataResolver = metaResolver, ReadSymbols = false });

    var needs = ShimFactory.Discover(asm.MainModule, refDirs);
    var shimDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(src))!, "refs_shim");
    var nShim = ShimFactory.Materialize(shimDir, needs);
    if (nShim > 0) { resolver.AddSearchDirectory(shimDir); Console.WriteLine($"[shim] {nShim} 个垫片 → {shimDir}"); }
    return (asm, new List<string> { shimDir });
}

switch (mode)
{
    case "find":
    {
        var src = a.Count > 1 ? a[1] : SRC;
        var mRe = a.Count > 2 ? a[2] : ".*";
        var tRe = a.Count > 3 ? a[3] : ".*";
        var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { ReadSymbols = false });
        Dumper.Find(asm, mRe, tRe);
        return 0;
    }
    case "dump":
    {
        var src = a.Count > 1 ? a[1] : SRC;
        var tName = a.Count > 2 ? a[2] : "Hello";
        var mName = a.Count > 3 ? a[3] : "*";
        var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { ReadSymbols = false });
        Dumper.DumpIL(asm, tName, mName);
        return 0;
    }
    case "refs":
    {
        var src = a.Count > 1 ? a[1] : SRC;
        var asm = AssemblyDefinition.ReadAssembly(src, new ReaderParameters { ReadSymbols = false });
        Console.WriteLine($"[asm] {asm.Name.FullName}  runtime={asm.MainModule.Runtime}");
        Console.WriteLine($"[refs] {asm.MainModule.AssemblyReferences.Count} 条");
        foreach (var r in asm.MainModule.AssemblyReferences)
            Console.WriteLine($"   {r.Name}  {r.Version}  (publicKeyToken={BitConverter.ToString(r.PublicKeyToken ?? Array.Empty<byte>()).Replace("-", "").ToLowerInvariant()})");
        return 0;
    }
    case "pad":
    {
        var f = a[1]; var size = long.Parse(a[2]);
        Patcher.PadTo(f, size);
        Console.WriteLine($"[pad] {f} → {new FileInfo(f).Length} B");
        return 0;
    }
    case "roundtrip":
    {
        var src = a.Count > 1 ? a[1] : SRC;
        var dst = a.Count > 2 ? a[2] : "/tmp/lvmeng/ACT_rt.dll";
        var refDirs = a.Count > 3 ? a.Skip(3).ToList() : new List<string> { DLLDIR };
        refDirs = refDirs.Where(Directory.Exists).ToList();
        var (asm, _) = Open(src, refDirs);
        var mod = asm.MainModule;
        Console.WriteLine($"[in ] {src}  {new FileInfo(src).Length} B  types={mod.Types.Count}");
        var before = ConstantAudit.Snapshot(mod);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dst))!);
        asm.Write(dst, new WriterParameters { WriteSymbols = false });
        long si = new FileInfo(src).Length, so = new FileInfo(dst).Length;
        Console.WriteLine($"[out] {dst}  {so} B   delta={so - si}  ({(so - si) * 100.0 / si:F3}%)");
        using (var back = AssemblyDefinition.ReadAssembly(dst, new ReaderParameters
        { AssemblyResolver = new TolerantAssemblyResolver(), ReadSymbols = false }))
        {
            var lost = ConstantAudit.Diff(before, back.MainModule);
            Console.WriteLine($"[audit] 常量丢失 = {lost.Count}");
            foreach (var l in lost.Take(20)) Console.WriteLine("   " + l);
        }
        return 0;
    }
    case "build":
    {
        var src = a.Count > 1 ? a[1] : SRC;
        var dst = a.Count > 2 ? a[2] : "/tmp/lvmeng/ACT_patched.dll";
        int phase = 1; long pad = 14529536; bool keepSize = true;
        for (int i = 3; i < a.Count; i++)
        {
            if (a[i] == "--phase") phase = int.Parse(a[++i]);
            else if (a[i] == "--padsize") pad = long.Parse(a[++i]);
            else if (a[i] == "--keep-size") keepSize = a[++i] != "0";
        }
        var refDirs = new List<string> { DLLDIR }.Where(Directory.Exists).ToList();
        var (asm, _) = Open(src, refDirs);
        return Patcher.Build(asm, src, dst, phase, pad, keepSize);
    }
}
Console.WriteLine("unknown mode");
return 1;
