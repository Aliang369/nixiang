using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkhlcm9Bd2FrZW5EZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVT" + "a2lsbFR5cGUucHJvdG8aGXR5cGUuR2VuZVNsb3REZXRhaWwucHJvdG8aH3R5" + "cGUuSGVyb0F3YWtlbkF0dHJEZXRhaWwucHJvdG8iqgEKEEhlcm9Bd2FrZW5E" + "ZXRhaWwSKQoJU2tpbGxUeXBlGAEgASgOMhYuRVNraWxsVHlwZS5FU2tpbGxU" + "eXBlEg0KBUxldmVsGAIgASgFEi8KCkF0dHJEZXRhaWwYAyABKAsyGy50eXBl" + "cy5IZXJvQXdha2VuQXR0ckRldGFpbBIrCgxHZW5lU2xvdExpc3QYBCADKAsy" + "FS50eXBlcy5HZW5lU2xvdERldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumESkillTypeReflection.Descriptor,
			TypeGeneSlotDetailReflection.Descriptor,
			TypeHeroAwakenAttrDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenDetail), (MessageParser)(object)HeroAwakenDetail.Parser, new string[4] { "SkillType", "Level", "AttrDetail", "GeneSlotList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
