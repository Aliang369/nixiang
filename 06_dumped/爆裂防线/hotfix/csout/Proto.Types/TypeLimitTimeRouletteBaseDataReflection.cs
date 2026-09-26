using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitTimeRouletteBaseDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeRouletteBaseDataReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxpbWl0VGltZVJvdWxldHRlQmFzZURhdGEucHJvdG8SBXR5cGVz" + "IqQBChlMaW1pdFRpbWVSb3VsZXR0ZUJhc2VEYXRhEg8KB0hlcm9UaWQYASAB" + "KAUSEAoITGltaXRWaXAYAiABKAUSEAoIT25jZUNvc3QYAyABKAUSDwoHVGVu" + "Q29zdBgEIAEoBRIRCglPbmNlTHVja3kYBSABKAUSEAoITWF4THVja3kYBiAB" + "KAUSDQoFQ291bnQYByABKAUSDQoFTHVja3kYCCABKAVCLloJLi87cHJvdG9j" + "qgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeRouletteBaseData), (MessageParser)(object)LimitTimeRouletteBaseData.Parser, new string[8] { "HeroTid", "LimitVip", "OnceCost", "TenCost", "OnceLucky", "MaxLucky", "Count", "Lucky" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
