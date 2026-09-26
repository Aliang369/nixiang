using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonAirdropEnterNtfReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonAirdropEnterNtfReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlNlYXNvbkFpcmRyb3BFbnRlck50Zi5wcm90bxIFdHlwZXMaFXR5" + "cGUuQmF0dGxlSW5mby5wcm90byKCAQoVU2Vhc29uQWlyZHJvcEVudGVyTnRm" + "Eg4KBkRyb3BJZBgBIAEoBRISCgpCYXR0bGVVdWlkGAIgASgJEiUKCkJhdHRs" + "ZUluZm8YAyABKAsyES50eXBlcy5CYXR0bGVJbmZvEgwKBFJvbGUYBCABKAUS" + "EAoIVGFyZ2V0SWQYBSABKANCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonAirdropEnterNtf), (MessageParser)(object)SeasonAirdropEnterNtf.Parser, new string[5] { "DropId", "BattleUuid", "BattleInfo", "Role", "TargetId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
