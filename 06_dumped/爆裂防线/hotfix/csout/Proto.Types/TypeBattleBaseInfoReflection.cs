using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBattleBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBattleBaseInfoReflection()
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLkJhdHRsZUJhc2VJbmZvLnByb3RvEgV0eXBlcxoaZW51bS5FQmF0" + "dGxlRnVuY1R5cGUucHJvdG8aG2VudW0uRUJhdHRsZU1vZGVsVHlwZS5wcm90" + "byKyAgoOQmF0dGxlQmFzZUluZm8SDAoEVVVJRBgBIAEoCRIPCgdFbmVteUlk" + "GAIgASgFEg8KB1N0YXJ0VG0YAyABKAMSDQoFRW5kVG0YBCABKAMSNAoKQmF0" + "dGxlVHlwZRgFIAEoDjIgLkVCYXR0bGVGdW5jVHlwZS5FQmF0dGxlRnVuY1R5" + "cGUSNwoLQmF0dGxlTW9kZWwYBiABKA4yIi5FQmF0dGxlTW9kZWxUeXBlLkVC" + "YXR0bGVNb2RlbFR5cGUSEQoJTW9kUmFuZElkGAcgASgFEhEKCUluaXRMZXZl" + "bBgIIAEoBRIQCghNYXhMZXZlbBgJIAEoBRIRCglUaW1lTGltaXQYCiABKAUS" + "EwoLVmlzaW9uUmFuZ2UYCyABKAUSEgoKUmVmcmVzaE1vZBgMIAEoBUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEBattleFuncTypeReflection.Descriptor,
			EnumEBattleModelTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BattleBaseInfo), (MessageParser)(object)BattleBaseInfo.Parser, new string[12]
			{
				"UUID", "EnemyId", "StartTm", "EndTm", "BattleType", "BattleModel", "ModRandId", "InitLevel", "MaxLevel", "TimeLimit",
				"VisionRange", "RefreshMod"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
