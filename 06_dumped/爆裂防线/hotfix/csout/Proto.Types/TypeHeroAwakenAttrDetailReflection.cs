using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroAwakenAttrDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroAwakenAttrDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkhlcm9Bd2FrZW5BdHRyRGV0YWlsLnByb3RvEgV0eXBlcxoZZW51" + "bS5FQXR0clNraWxsVHlwZS5wcm90byKIAQoUSGVyb0F3YWtlbkF0dHJEZXRh" + "aWwSLAoEVHlwZRgBIAEoDjIeLkVBdHRyU2tpbGxUeXBlLkVBdHRyU2tpbGxU" + "eXBlEg0KBVZhbHVlGAIgASgCEg4KBkdyb3d0aBgDIAEoAhISCgpDdXJyZW50" + "TWF4GAQgASgCEg8KB05leHRNYXgYBSABKAJCLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEAttrSkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroAwakenAttrDetail), (MessageParser)(object)HeroAwakenAttrDetail.Parser, new string[5] { "Type", "Value", "Growth", "CurrentMax", "NextMax" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
