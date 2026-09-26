using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionBossFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionBossFightStartAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxlZ2lvbkJvc3NGaWdodFN0YXJ0QWNrLnByb3RvEgV0eXBlcxoV" + "dHlwZS5CYXR0bGVJbmZvLnByb3RvGhl0eXBlLkxlZ2lvbkJvc3NJbmZvLnBy" + "b3RvIm8KF0xlZ2lvbkJvc3NGaWdodFN0YXJ0QWNrEi0KDkxlZ2lvbkJvc3NJ" + "bmZvGAEgASgLMhUudHlwZXMuTGVnaW9uQm9zc0luZm8SJQoKQmF0dGxlSW5m" + "bxgCIAEoCzIRLnR5cGVzLkJhdHRsZUluZm9CLloJLi87cHJvdG9jqgILUHJv" + "dG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleInfoReflection.Descriptor,
			TypeLegionBossInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionBossFightStartAck), (MessageParser)(object)LegionBossFightStartAck.Parser, new string[2] { "LegionBossInfo", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
