using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenUpgradeReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenUpgradeReqReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkhlcm9Bd2FrZW5VcGdyYWRlUmVxLnByb3RvEgV0eXBlcxoVZW51" + "bS5FQm9vbFN0YXRlLnByb3RvGhVlbnVtLkVTa2lsbFR5cGUucHJvdG8ifAoU" + "SGVyb0F3YWtlblVwZ3JhZGVSZXESKQoJU2tpbGxUeXBlGAEgASgOMhYuRVNr" + "aWxsVHlwZS5FU2tpbGxUeXBlEg0KBUNvdW50GAIgASgFEioKClVzZURpYW1v" + "bmQYAyABKA4yFi5FQm9vbFN0YXRlLkVCb29sU3RhdGVCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumESkillTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenUpgradeReq), (MessageParser)(object)HeroAwakenUpgradeReq.Parser, new string[3] { "SkillType", "Count", "UseDiamond" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
