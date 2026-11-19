using System.Text;
using Mono.Cecil;

// 只读「映像几何审计」：PE/CLR 头 + 元数据流 + 方法体边界（tiny/fat + EH），不依赖反编译。
// 用法: ilgeo <dll> [--bodies]
var path = args[0];
var showBodies = args.Contains("--bodies");
var bytes = File.ReadAllBytes(path);

int U16(int o) => BitConverter.ToUInt16(bytes, o);
int U32(int o) => BitConverter.ToInt32(bytes, o);
long U64(int o) => BitConverter.ToInt64(bytes, o);

int pe = U32(0x3C);
if (bytes[pe] != 'P' || bytes[pe + 1] != 'E') throw new Exception("no PE");
int coff = pe + 4;
int nsec = U16(coff + 2);
int optSize = U16(coff + 16);
int opt = coff + 20;
int magic = U16(opt);
bool pe32plus = magic == 0x20b;
int p = opt + 2 + 1 + 1 + 4 + 4 + 4 + 4 + 4 + (pe32plus ? 0 : 4);
long imageBase = pe32plus ? U64(p) : U32(p); p += pe32plus ? 8 : 4;
int secAlign = U32(p); p += 4;
int fileAlign = U32(p); p += 4;
p += 2 * 6; p += 4;
int sizeOfImage = U32(p); p += 4;
int sizeOfHeaders = U32(p); p += 4;
p += 4 + 2 + 2;
p += pe32plus ? 4 * 8 : 4 * 4;
p += 4;
int numDirs = U32(p); p += 4;
int dd = p;
int clrRva = U32(dd + 14 * 8), clrSize = U32(dd + 14 * 8 + 4);

var secs = new List<(string name, int vsize, int va, int rawsize, int rawptr)>();
int st = opt + optSize;
for (int i = 0; i < nsec; i++)
{
    int o = st + i * 40;
    var name = Encoding.ASCII.GetString(bytes, o, 8).TrimEnd('\0');
    secs.Add((name, U32(o + 8), U32(o + 12), U32(o + 16), U32(o + 20)));
}
int Rva2Off(int rva)
{
    foreach (var s in secs)
        if (rva >= s.va && rva < s.va + Math.Max(s.vsize, s.rawsize))
        {
            int d = rva - s.va;
            if (d < s.rawsize) return s.rawptr + d;
        }
    return -1;
}

Console.WriteLine($"== {Path.GetFileName(path)}  len={bytes.Length}  sections={nsec}  SizeOfImage=0x{sizeOfImage:X}  ImageBase=0x{imageBase:X}  align(sec/file)={secAlign}/{fileAlign}");
foreach (var s in secs) Console.WriteLine($"   sec {s.name,-8} VA=0x{s.va:X8} VSize=0x{s.vsize:X6} Raw=0x{s.rawsize:X6}@{s.rawptr:X8}");

int cor = Rva2Off(clrRva);
int mdRva = U32(cor + 8), mdSize = U32(cor + 12);
int corFlags = U32(cor + 16), entryTok = U32(cor + 20);
Console.WriteLine($"   CLR dir rva=0x{clrRva:X8} size={clrSize}  cb={U32(cor)}  flags=0x{corFlags:X8}  entryTok=0x{entryTok:X8}");
Console.WriteLine($"   MetaData rva=0x{mdRva:X8} size={mdSize}");

int mdr = Rva2Off(mdRva);
if (U32(mdr) != 0x424A5342) throw new Exception("no BSJB");
int verLen = U32(mdr + 12);
int afterVer = mdr + 16 + verLen;
int nstreams = U16(afterVer + 2);
int sh = afterVer + 4;
Console.WriteLine($"   streams={nstreams}");
for (int i = 0; i < nstreams; i++)
{
    int off = U32(sh), size = U32(sh + 4);
    var nm = Encoding.ASCII.GetString(bytes, sh + 8, 32).Split('\0')[0];
    Console.WriteLine($"     {nm,-12} off=0x{off:X6} size=0x{size:X6}");
    sh += 8 + ((nm.Length + 4) & ~3);
}

var asm = AssemblyDefinition.ReadAssembly(path);
var mod = asm.MainModule;
IEnumerable<TypeDefinition> AllT(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in AllT(n)) yield return x; }
var types = mod.Types.SelectMany(AllT).ToList();
var methods = types.SelectMany(t => t.Methods).ToList();
Console.WriteLine($"   TypeDef(all)={types.Count}  MethodDef={methods.Count}  TypeRef={mod.GetTypeReferences().Count()}  MemberRef={mod.GetMemberReferences().Count()}  AssemblyRef={mod.AssemblyReferences.Count}");

int bodyOk = 0, bodyBadSize = 0, bodyBadEh = 0, bodyNoRva = 0;
var ranges = new List<(int start, int end, string name)>();
var problems = new List<string>();
foreach (var m in methods)
{
    if (m.RVA == 0) { bodyNoRva++; continue; }
    int r = Rva2Off((int)m.RVA);
    if (r < 0) { problems.Add($"[RVA-NOT-MAPPED] {m.FullName} rva=0x{m.RVA:X}"); continue; }
    int b0 = bytes[r];
    int codeStart, codeSize, end;
    if ((b0 & 3) == 2) { codeSize = b0 >> 2; codeStart = r + 1; end = codeStart + codeSize; }
    else
    {
        int hw = U16(r);
        int hdrSize = (hw >> 12) * 4;
        int flags = hw & 0xFFF;
        codeSize = U32(r + 4);
        codeStart = r + hdrSize;
        end = codeStart + codeSize;
        if ((flags & 0x8) != 0)
        {
            int eh = codeStart + codeSize;
            int pad = (4 - (eh & 3)) & 3;
            eh += pad;
            int kind = bytes[eh];
            int dataSize = (kind & 0x3F) == 1 ? (bytes[eh + 1] | (bytes[eh + 2] << 8) | (bytes[eh + 3] << 16)) : bytes[eh + 1];
            int ehTotal = (kind & 0x40) != 0 ? bytes[eh + 1] : ((kind & 0x3F) == 1 ? dataSize * 4 + 4 : dataSize * 4 + 4);
            end = eh + ehTotal;
        }
    }
    // 期望长度 = 指令编码之和
    int expect = 0;
    if (m.HasBody)
    {
        expect = m.Body.CodeSize;
        var last = m.Body.Instructions[m.Body.Instructions.Count - 1];
        int span = last.Offset + InsSize(last);
        if (span != codeSize) { bodyBadSize++; if (problems.Count < 60) problems.Add($"[CODE-SIZE] {m.FullName} header={codeSize} cecil={m.Body.CodeSize} span={span} lastIL=0x{last.Offset:X}"); }
        foreach (var h in m.Body.ExceptionHandlers)
        {
            foreach (var z in new[] { h.TryStart, h.TryEnd, h.HandlerStart, h.HandlerEnd, h.FilterStart })
            {
                if (z == null) continue;
                if (z.Offset > codeSize) { bodyBadEh++; if (problems.Count < 60) problems.Add($"[EH-OOB] {m.FullName} {h.HandlerType} off=0x{z.Offset:X} codeSize={codeSize}"); }
            }
        }
    }
    else bodyOk++;
    if (m.HasBody && codeStart + codeSize > bytes.Length) { problems.Add($"[BODY-PAST-EOF] {m.FullName}"); }
    ranges.Add((codeStart, codeStart + codeSize, m.FullName + "#0x{m.RVA:X}"));
}

ranges = ranges.GroupBy(x => x.start).Select(g => g.First()).ToList();
ranges.Sort((a, b) => a.start.CompareTo(b.start));
int overlap = 0;
for (int i = 1; i < ranges.Count; i++)
{
    if (ranges[i].start < ranges[i - 1].end)
    {
        overlap++;
        if (overlap <= 20) problems.Add($"[BODY-OVERLAP] {ranges[i - 1].name} [0x{ranges[i - 1].start:X},0x{ranges[i - 1].end:X}) 与 {ranges[i].name} [0x{ranges[i].start:X},0x{ranges[i].end:X})");
    }
}
Console.WriteLine($"   bodies: ok={bodyOk} noRva={bodyNoRva} codeSizeMismatch={bodyBadSize} ehOOB={bodyBadEh} overlaps={overlap}");
foreach (var s in problems.Take(60)) Console.WriteLine("   " + s);
Console.WriteLine(problems.Count == 0 ? "   RESULT: CLEAN" : $"   RESULT: {problems.Count} 处问题");

static int InsSize(Mono.Cecil.Cil.Instruction i)
{
    int sz = i.OpCode.Size;
    switch (i.OpCode.OperandType)
    {
        case Mono.Cecil.Cil.OperandType.InlineNone: break;
        case Mono.Cecil.Cil.OperandType.ShortInlineI:
        case Mono.Cecil.Cil.OperandType.ShortInlineVar:
        case Mono.Cecil.Cil.OperandType.ShortInlineBrTarget: sz += 1; break;
        case Mono.Cecil.Cil.OperandType.InlineVar: sz += 2; break;
        case Mono.Cecil.Cil.OperandType.InlineI8:
        case Mono.Cecil.Cil.OperandType.InlineR: sz += 8; break;
        case Mono.Cecil.Cil.OperandType.ShortInlineR: sz += 4; break;
        case Mono.Cecil.Cil.OperandType.InlineSwitch: sz += 4 + 4 * (((Mono.Cecil.Cil.Instruction[])i.Operand)?.Length ?? 0); break;
        default: sz += 4; break;
    }
    return sz;
}
