using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonAirdropParticipantReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAirdropParticipantReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLlNlYXNvbkFpcmRyb3BQYXJ0aWNpcGFudC5wcm90bxIFdHlwZXMa" + "F2VudW0uRVNlYXNvbkZvcmNlLnByb3RvGhd0eXBlLlJvbGVCYXNlSW5mby5w" + "cm90byKPAQoYU2Vhc29uQWlyZHJvcFBhcnRpY2lwYW50EikKDFJvbGVCYXNl" + "SW5mbxgBIAEoCzITLnR5cGVzLlJvbGVCYXNlSW5mbxIMCgRQcm9nGAIgASgF" + "EisKB0ZvcmNlSWQYAyABKA4yGi5FU2Vhc29uRm9yY2UuRVNlYXNvbkZvcmNl" + "Eg0KBVN0YXRlGAQgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumESeasonForceReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAirdropParticipant), (MessageParser)(object)SeasonAirdropParticipant.Parser, new string[4] { "RoleBaseInfo", "Prog", "ForceId", "State" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
