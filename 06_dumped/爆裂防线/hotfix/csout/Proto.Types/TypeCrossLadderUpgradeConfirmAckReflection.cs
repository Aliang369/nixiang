using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderUpgradeConfirmAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderUpgradeConfirmAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLkNyb3NzTGFkZGVyVXBncmFkZUNvbmZpcm1BY2sucHJvdG8SBXR5" + "cGVzGhp0eXBlLkNyb3NzTGFkZGVySW5mby5wcm90byJgChxDcm9zc0xhZGRl" + "clVwZ3JhZGVDb25maXJtQWNrEi8KD0Nyb3NzTGFkZGVySW5mbxgBIAEoCzIW" + "LnR5cGVzLkNyb3NzTGFkZGVySW5mbxIPCgdPdmVyUGN0GAIgASgCQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeCrossLadderInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderUpgradeConfirmAck), (MessageParser)(object)CrossLadderUpgradeConfirmAck.Parser, new string[2] { "CrossLadderInfo", "OverPct" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
