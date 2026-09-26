using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatScrollNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatScrollNtfReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkNoYXRTY3JvbGxOdGYucHJvdG8SBXR5cGVzGhZlbnVtLkVTY3Jv" + "bGxUeXBlLnByb3RvGhh0eXBlLkNvbnRlbnREZXRhaWwucHJvdG8aF3R5cGUu" + "Um9sZUJhc2VJbmZvLnByb3RvIqEBCg1DaGF0U2Nyb2xsTnRmEiwKClNjcm9s" + "bFR5cGUYASABKA4yGC5FU2Nyb2xsVHlwZS5FU2Nyb2xsVHlwZRIQCghTZW5k" + "ZXJJZBgCIAEoBRIpCgxSb2xlQmFzZUluZm8YAyABKAsyEy50eXBlcy5Sb2xl" + "QmFzZUluZm8SJQoHQ29udGVudBgEIAEoCzIULnR5cGVzLkNvbnRlbnREZXRh" + "aWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEScrollTypeReflection.Descriptor,
			TypeContentDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatScrollNtf), (MessageParser)(object)ChatScrollNtf.Parser, new string[4] { "ScrollType", "SenderId", "RoleBaseInfo", "Content" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
