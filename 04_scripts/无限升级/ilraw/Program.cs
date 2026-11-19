using Mono.Cecil;
using Cil = Mono.Cecil.Cil;
using Emit = System.Reflection.Emit;

// 只读：原始字节级 IL 校验（不信任 Cecil 的写回结果）
var path = args[0];
var bytes = File.ReadAllBytes(path);
var mod = AssemblyDefinition.ReadAssembly(path).MainModule;

// ---- 手写 PE 段表（RVA→文件偏移）
int peOff = BitConverter.ToInt32(bytes, 0x3C);
int nSec = BitConverter.ToInt16(bytes, peOff + 6);
int optSize = BitConverter.ToInt16(bytes, peOff + 20);
int secOff = peOff + 24 + optSize;
var secs = new List<(uint va, uint vsize, uint raw, uint rawSize)>();
for (int i = 0; i < nSec; i++)
{
    int o = secOff + i * 40;
    secs.Add((BitConverter.ToUInt32(bytes, o + 12), BitConverter.ToUInt32(bytes, o + 8),
              BitConverter.ToUInt32(bytes, o + 20), BitConverter.ToUInt32(bytes, o + 16)));
}
int Rva2Off(int rva)
{
    foreach (var s in secs)
        if ((uint)rva >= s.va && (uint)rva < s.va + Math.Max(s.vsize, s.rawSize))
            return (int)((uint)rva - s.va + s.raw);
    return -1;
}

var one = new Dictionary<byte, Emit.OpCode>();
var two = new Dictionary<byte, Emit.OpCode>();
foreach (var f in typeof(Emit.OpCodes).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
{
    var oc = (Emit.OpCode)f.GetValue(null)!;
    if (oc.Size == 1) one[(byte)oc.Value] = oc; else two[(byte)(oc.Value & 0xff)] = oc;
}

if (args.Length > 2 && args[1] == "ms")
{
    var pat = args[2];
    foreach (var t2 in mod.Types)
        foreach (var m2 in t2.Methods)
        {
            if (!m2.HasBody) continue;
            if (!(t2.Name + "::" + m2.Name).Contains(pat)) continue;
            Console.WriteLine($"  {t2.Name}::{m2.Name}  MaxStackSize={m2.Body.MaxStackSize}  codeSize={m2.Body.CodeSize}  instr={m2.Body.Instructions.Count}");
        }
    return;
}
IEnumerable<TypeDefinition> All(TypeDefinition t) { yield return t; foreach (var n in t.NestedTypes) foreach (var x in All(n)) yield return x; }

int badDecode = 0, badBranch = 0, badEh = 0, shortOver = 0, methods = 0;

foreach (var t in mod.Types.SelectMany(All))
    foreach (var m in t.Methods)
    {
        if (!m.HasBody) continue;
        int off = Rva2Off((int)m.RVA);
        if (off < 0) continue;
        int b0 = bytes[off], codeSize, headerSize;
        if ((b0 & 3) == 2) { headerSize = 1; codeSize = b0 >> 2; }
        else
        {
            int flags = BitConverter.ToUInt16(bytes, off);
            headerSize = (flags >> 12) * 4;
            codeSize = BitConverter.ToInt32(bytes, off + 4);
            if (headerSize < 12) headerSize = 12;
        }
        int start = off + headerSize;
        if (codeSize <= 0 || start + codeSize > bytes.Length) continue;
        methods++;

        var bounds = new HashSet<int>();
        var brs = new List<(int at, int rel, bool sh, int abs)>();
        var sws = new List<(int at, int[] abs)>();
        int p = 0; bool ok = true;
        while (p < codeSize)
        {
            bounds.Add(p);
            int ip = start + p;
            Emit.OpCode oc;
            if (bytes[ip] == 0xFE) { if (!two.TryGetValue(bytes[ip + 1], out oc)) { ok = false; break; } }
            else if (!one.TryGetValue(bytes[ip], out oc)) { ok = false; break; }
            int sz = oc.Size;
            switch (oc.OperandType)
            {
                case Emit.OperandType.InlineNone: break;
                case Emit.OperandType.ShortInlineBrTarget:
                    { sbyte d = (sbyte)bytes[ip + sz]; brs.Add((p, d, true, p + sz + 1 + d)); sz += 1; break; }
                case Emit.OperandType.ShortInlineI:
                case Emit.OperandType.ShortInlineVar: sz += 1; break;
                case Emit.OperandType.InlineVar: sz += 2; break;
                case Emit.OperandType.InlineBrTarget:
                    { int d = BitConverter.ToInt32(bytes, ip + sz); brs.Add((p, d, false, p + sz + 4 + d)); sz += 4; break; }
                case Emit.OperandType.InlineSwitch:
                    {
                        int n = BitConverter.ToInt32(bytes, ip + sz);
                        int baseOff = p + sz + 4 + n * 4;
                        var ts = new int[n];
                        for (int k = 0; k < n; k++) ts[k] = baseOff + BitConverter.ToInt32(bytes, ip + sz + 4 + k * 4);
                        sws.Add((p, ts)); sz += 4 + n * 4; break;
                    }
                case Emit.OperandType.InlineI8:
                case Emit.OperandType.InlineR: sz += 8; break;
                case Emit.OperandType.ShortInlineR: sz += 4; break;
                default: sz += 4; break;
            }
            p += sz;
        }
        if (!ok || p != codeSize) { badDecode++; Console.WriteLine($"[DECODE] {m.FullName} 解码未对齐 p={p}/{codeSize}"); continue; }

        foreach (var (at, rel, sh, abs) in brs)
        {
            if (abs != codeSize && !bounds.Contains(abs))
            { badBranch++; Console.WriteLine($"[BR] {m.FullName} IL_{at:X4} -> IL_{abs:X4} 不在指令边界 (codeSize={codeSize})"); }
            if (sh && (rel < -128 || rel > 127)) { shortOver++; Console.WriteLine($"[SHORT] {m.FullName} IL_{at:X4} rel={rel} 超短跳范围"); }
        }
        foreach (var (at, ts) in sws)
            foreach (var abs in ts)
                if (abs != codeSize && !bounds.Contains(abs))
                { badBranch++; Console.WriteLine($"[SWITCH] {m.FullName} IL_{at:X4} -> IL_{abs:X4} 不在指令边界"); }

        foreach (var eh in m.Body.ExceptionHandlers)
        {
            var checks = new (string, int)[] {
                ("tryStart", eh.TryStart?.Offset ?? -1), ("tryEnd", eh.TryEnd?.Offset ?? codeSize),
                ("hStart", eh.HandlerStart?.Offset ?? -1), ("hEnd", eh.HandlerEnd?.Offset ?? codeSize) };
            foreach (var (nm, v) in checks)
            {
                if (v < 0 || v == codeSize) continue;
                if (!bounds.Contains(v)) { badEh++; Console.WriteLine($"[EH] {m.FullName} {nm}=IL_{v:X4} 不在指令边界"); }
            }
        }
    }

Console.WriteLine($"== methods={methods} decodeBad={badDecode} brBad={badBranch} ehBad={badEh} shortOver={shortOver}");
Console.WriteLine(badDecode + badBranch + badEh + shortOver == 0 ? "RESULT: CLEAN" : "RESULT: 有问题");
