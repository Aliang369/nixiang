using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionSelfInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionSelfInfoReflection()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkxlZ2lvblNlbGZJbmZvLnByb3RvEgV0eXBlcxoZdHlwZS5MZWdp" + "b25TZWxmQm9zcy5wcm90bxobdHlwZS5MZWdpb25TZWxmRGlubmVyLnByb3Rv" + "Ght0eXBlLkxlZ2lvblNlbGZEb25hdGUucHJvdG8ixgEKDkxlZ2lvblNlbGZJ" + "bmZvEhAKCExlZ2lvbklkGAEgASgFEg4KBkpvaW5UbRgCIAEoBRInCgZEb25h" + "dGUYAyABKAsyFy50eXBlcy5MZWdpb25TZWxmRG9uYXRlEisKCkRpbm5lckxp" + "c3QYBCADKAsyFy50eXBlcy5MZWdpb25TZWxmRGlubmVyEhMKC1Jld2FyZENv" + "dW50GAUgASgFEicKCEJvc3NJbmZvGAYgASgLMhUudHlwZXMuTGVnaW9uU2Vs" + "ZkJvc3NCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeLegionSelfBossReflection.Descriptor,
			TypeLegionSelfDinnerReflection.Descriptor,
			TypeLegionSelfDonateReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionSelfInfo), (MessageParser)(object)LegionSelfInfo.Parser, new string[6] { "LegionId", "JoinTm", "Donate", "DinnerList", "RewardCount", "BossInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
