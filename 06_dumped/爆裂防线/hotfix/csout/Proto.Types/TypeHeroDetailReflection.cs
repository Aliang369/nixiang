using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroDetailReflection()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkhlcm9EZXRhaWwucHJvdG8SBXR5cGVzGhNlbnVtLkVRdWFsaXR5" + "LnByb3RvGhZlbnVtLkVSYXJpdHlUeXBlLnByb3RvIroBCgpIZXJvRGV0YWls" + "Eg4KBkhlcm9JZBgBIAEoCRIPCgdIZXJvVGlkGAIgASgFEhAKCEF2YXRhcklk" + "GAMgASgFEiMKB1F1YWxpdHkYBCABKA4yEi5FUXVhbGl0eS5FUXVhbGl0eRIT" + "CgtRdWFsaXR5U3RhchgFIAEoBRIoCgZSYXJpdHkYBiABKA4yGC5FUmFyaXR5" + "VHlwZS5FUmFyaXR5VHlwZRIVCg1SYXJpdHlVcGdyYWRlGAcgASgFQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEQualityReflection.Descriptor,
			EnumERarityTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroDetail), (MessageParser)(object)HeroDetail.Parser, new string[7] { "HeroId", "HeroTid", "AvatarId", "Quality", "QualityStar", "Rarity", "RarityUpgrade" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
