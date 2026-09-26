using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderPlayerInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderPlayerInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkNyb3NzTGFkZGVyUGxheWVySW5mby5wcm90bxIFdHlwZXMaIXR5" + "cGUuQ3Jvc3NMYWRkZXJBY2hpZXZlSW5mby5wcm90bxoXdHlwZS5Sb2xlQmFz" + "ZUluZm8ucHJvdG8idgoVQ3Jvc3NMYWRkZXJQbGF5ZXJJbmZvEikKDFJvbGVC" + "YXNlSW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxIyCgtBY2hpZXZl" + "SW5mbxgCIAEoCzIdLnR5cGVzLkNyb3NzTGFkZGVyQWNoaWV2ZUluZm9CLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeCrossLadderAchieveInfoReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderPlayerInfo), (MessageParser)(object)CrossLadderPlayerInfo.Parser, new string[2] { "RoleBaseInfo", "AchieveInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
