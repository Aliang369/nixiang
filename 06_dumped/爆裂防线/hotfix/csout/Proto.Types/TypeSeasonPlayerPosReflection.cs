using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonPlayerPosReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonPlayerPosReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlNlYXNvblBsYXllclBvcy5wcm90bxIFdHlwZXMaF2VudW0uRVNl" + "YXNvbkZvcmNlLnByb3RvGhd0eXBlLlJvbGVCYXNlSW5mby5wcm90byKVAQoP" + "U2Vhc29uUGxheWVyUG9zEikKDFJvbGVCYXNlSW5mbxgBIAEoCzITLnR5cGVz" + "LlJvbGVCYXNlSW5mbxIrCgdGb3JjZUlkGAIgASgOMhouRVNlYXNvbkZvcmNl" + "LkVTZWFzb25Gb3JjZRIOCgZUZWFtSWQYAyABKAUSDAoEUG9zWBgEIAEoAhIM" + "CgRQb3NZGAUgASgCQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJn" + "YW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonPlayerPos), (MessageParser)(object)SeasonPlayerPos.Parser, new string[5] { "RoleBaseInfo", "ForceId", "TeamId", "PosX", "PosY" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
