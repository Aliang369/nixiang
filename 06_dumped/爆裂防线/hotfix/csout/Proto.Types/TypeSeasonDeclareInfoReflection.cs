using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonDeclareInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonDeclareInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvbkRlY2xhcmVJbmZvLnByb3RvEgV0eXBlcxoXZW51bS5F" + "U2Vhc29uRm9yY2UucHJvdG8icwoRU2Vhc29uRGVjbGFyZUluZm8SDgoGQ2l0" + "eUlkGAEgASgFEiwKCEF0a0ZvcmNlGAIgASgOMhouRVNlYXNvbkZvcmNlLkVT" + "ZWFzb25Gb3JjZRIRCglEZWNsYXJlVG0YAyABKAMSDQoFRW5kVG0YBCABKANC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESeasonForceReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonDeclareInfo), (MessageParser)(object)SeasonDeclareInfo.Parser, new string[4] { "CityId", "AtkForce", "DeclareTm", "EndTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
