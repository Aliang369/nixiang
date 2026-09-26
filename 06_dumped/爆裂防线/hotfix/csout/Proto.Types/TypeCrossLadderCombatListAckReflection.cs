using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderCombatListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderCombatListAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkNyb3NzTGFkZGVyQ29tYmF0TGlzdEFjay5wcm90bxIFdHlwZXMa" + "GnR5cGUuQ3Jvc3NMYWRkZXJJbmZvLnByb3RvGiB0eXBlLkNyb3NzTGFkZGVy" + "UGxheWVySW5mby5wcm90byJ9ChhDcm9zc0xhZGRlckNvbWJhdExpc3RBY2sS" + "LwoPQ3Jvc3NMYWRkZXJJbmZvGAEgASgLMhYudHlwZXMuQ3Jvc3NMYWRkZXJJ" + "bmZvEjAKCkNvbWJhdExpc3QYAiADKAsyHC50eXBlcy5Dcm9zc0xhZGRlclBs" + "YXllckluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeCrossLadderInfoReflection.Descriptor,
			TypeCrossLadderPlayerInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderCombatListAck), (MessageParser)(object)CrossLadderCombatListAck.Parser, new string[2] { "CrossLadderInfo", "CombatList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
