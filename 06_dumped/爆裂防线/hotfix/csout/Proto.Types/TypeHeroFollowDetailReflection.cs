using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeHeroFollowDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroFollowDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkhlcm9Gb2xsb3dEZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVT" + "a2lsbFR5cGUucHJvdG8iUgoQSGVyb0ZvbGxvd0RldGFpbBIpCglTa2lsbFR5" + "cGUYASABKA4yFi5FU2tpbGxUeXBlLkVTa2lsbFR5cGUSEwoLRm9sbG93TGV2" + "ZWwYAiABKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroFollowDetail), (MessageParser)(object)HeroFollowDetail.Parser, new string[2] { "SkillType", "FollowLevel" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
