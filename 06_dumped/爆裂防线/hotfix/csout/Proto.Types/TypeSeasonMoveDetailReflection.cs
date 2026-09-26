using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonMoveDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMoveDetailReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlNlYXNvbk1vdmVEZXRhaWwucHJvdG8SBXR5cGVzGhdlbnVtLkVT" + "ZWFzb25Gb3JjZS5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8i6AEK" + "EFNlYXNvbk1vdmVEZXRhaWwSKQoMUm9sZUJhc2VJbmZvGAEgASgLMhMudHlw" + "ZXMuUm9sZUJhc2VJbmZvEisKB0ZvcmNlSWQYAiABKA4yGi5FU2Vhc29uRm9y" + "Y2UuRVNlYXNvbkZvcmNlEg4KBlRlYW1JZBgDIAEoBRINCgVGcm9tWBgEIAEo" + "AhINCgVGcm9tWRgFIAEoAhILCgNUb1gYBiABKAISCwoDVG9ZGAcgASgCEg8K" + "B1N0YXJ0VG0YCCABKAMSEQoJQXJyaXZhbFRtGAkgASgDEhAKCE1vdmVUeXBl" + "GAogASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMoveDetail), (MessageParser)(object)SeasonMoveDetail.Parser, new string[10] { "RoleBaseInfo", "ForceId", "TeamId", "FromX", "FromY", "ToX", "ToY", "StartTm", "ArrivalTm", "MoveType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
