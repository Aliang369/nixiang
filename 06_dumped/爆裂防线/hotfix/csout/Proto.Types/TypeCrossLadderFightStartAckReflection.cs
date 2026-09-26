using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderFightStartAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLkNyb3NzTGFkZGVyRmlnaHRTdGFydEFjay5wcm90bxIFdHlwZXMa" + "FXR5cGUuQmF0dGxlSW5mby5wcm90byJTChhDcm9zc0xhZGRlckZpZ2h0U3Rh" + "cnRBY2sSEAoIUGxheWVySWQYASABKAUSJQoKQmF0dGxlSW5mbxgCIAEoCzIR" + "LnR5cGVzLkJhdHRsZUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderFightStartAck), (MessageParser)(object)CrossLadderFightStartAck.Parser, new string[2] { "PlayerId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
