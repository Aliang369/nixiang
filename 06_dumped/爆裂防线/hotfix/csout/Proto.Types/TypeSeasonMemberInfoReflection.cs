using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonMemberInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMemberInfoReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlNlYXNvbk1lbWJlckluZm8ucHJvdG8SBXR5cGVzGhhlbnVtLkVT" + "ZWFzb25PZmZpY2UucHJvdG8aF3R5cGUuUm9sZUJhc2VJbmZvLnByb3RvIooB" + "ChBTZWFzb25NZW1iZXJJbmZvEikKDFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5" + "cGVzLlJvbGVCYXNlSW5mbxIsCgZPZmZpY2UYAiABKA4yHC5FU2Vhc29uT2Zm" + "aWNlLkVTZWFzb25PZmZpY2USDQoFUG93ZXIYAyABKAMSDgoGT25saW5lGAQg" + "ASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonOfficeReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMemberInfo), (MessageParser)(object)SeasonMemberInfo.Parser, new string[4] { "RoleBaseInfo", "Office", "Power", "Online" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
