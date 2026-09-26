using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarSignUpAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarSignUpAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbldhclNpZ25VcEFjay5wcm90bxIFdHlwZXMaHHR5cGUu" + "TGVnaW9uV2FyQmFzZUluZm8ucHJvdG8aHnR5cGUuTGVnaW9uV2FyTGVnaW9u" + "SW5mby5wcm90byKCAQoSTGVnaW9uV2FyU2lnblVwQWNrEjMKEUxlZ2lvbldh" + "ckJhc2VJbmZvGAEgASgLMhgudHlwZXMuTGVnaW9uV2FyQmFzZUluZm8SNwoT" + "TGVnaW9uV2FyTGVnaW9uSW5mbxgCIAEoCzIaLnR5cGVzLkxlZ2lvbldhckxl" + "Z2lvbkluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarLegionInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarSignUpAck), (MessageParser)(object)LegionWarSignUpAck.Parser, new string[2] { "LegionWarBaseInfo", "LegionWarLegionInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
