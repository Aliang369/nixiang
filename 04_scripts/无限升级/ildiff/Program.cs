// ildiff — 逐方法 IL 指纹导出（用于官方热更 DLL 版本对比 / 补丁锚点存活检查）
// 用法: dotnet run -- <assembly.dll> > out.tsv
// 输出: M<TAB>类型<TAB>方法<TAB>参数签名<TAB>IL指纹<TAB>指令数
// 指纹 = 操作码序列 + 成员/字符串操作数（忽略分支目标偏移，避免重编译噪声）
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

string path = args[0];
var asm = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = false });
var sb = new StringBuilder();
int nTypes = 0, nMethods = 0;

void DumpType(TypeDefinition t)
{
    nTypes++;
    sb.Append("T\t").Append(t.FullName).Append("\tfields=").Append(t.Fields.Count)
      .Append("\tmethods=").Append(t.Methods.Count).Append('\n');
    foreach (var m in t.Methods)
    {
        nMethods++;
        string sig = string.Join(",", m.Parameters.Select(p => p.ParameterType.FullName));
        string hash = "-"; int cnt = 0;
        if (m.HasBody && m.Body != null)
        {
            cnt = m.Body.Instructions.Count;
            hash = HashIL(m);
        }
        sb.Append("M\t").Append(t.FullName).Append('\t').Append(m.Name).Append('\t')
          .Append(sig).Append('\t').Append(hash).Append('\t').Append(cnt).Append('\n');
    }
    foreach (var n in t.NestedTypes) DumpType(n);
}

static string HashIL(MethodDefinition m)
{
    using var ms = new MemoryStream();
    using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
    foreach (var ins in m.Body.Instructions)
    {
        bw.Write(ins.OpCode.Value);
        switch (ins.Operand)
        {
            case null: break;
            case Instruction: bw.Write((byte)1); break;          // 分支目标：只记"是分支"
            case string s: bw.Write(s); break;
            case MethodReference mr: bw.Write("m:" + mr.FullName); break;
            case FieldReference fr: bw.Write("f:" + fr.FullName); break;
            case TypeReference tr: bw.Write("t:" + tr.FullName); break;
            case ParameterDefinition pd: bw.Write("p:" + pd.Index); break;
            case VariableDefinition vd: bw.Write("v:" + vd.Index); break;
            default: bw.Write(ins.Operand.ToString()); break;
        }
    }
    bw.Write(m.Body.Variables.Count);
    foreach (var v in m.Body.Variables) bw.Write(v.VariableType.FullName);
    foreach (var h in m.Body.ExceptionHandlers) bw.Write(h.HandlerType.ToString());
    bw.Flush();
    return Convert.ToHexString(SHA1.HashData(ms.ToArray()))[..16];
}

foreach (var t in asm.MainModule.Types) DumpType(t);
Console.Write(sb.ToString());
Console.Error.WriteLine($"# {Path.GetFileName(path)} types={nTypes} methods={nMethods} module={asm.Name.Name} {asm.Name.Version}");
