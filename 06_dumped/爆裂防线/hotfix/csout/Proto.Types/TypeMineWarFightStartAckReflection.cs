using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMineWarFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMineWarFightStartAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLk1pbmVXYXJGaWdodFN0YXJ0QWNrLnByb3RvEgV0eXBlcxoVdHlw" + "ZS5CYXR0bGVJbmZvLnByb3RvGhp0eXBlLk1pbmVXYXJCYXNlSW5mby5wcm90" + "byKRAQoUTWluZVdhckZpZ2h0U3RhcnRBY2sSDwoHUG9pbnRJZBgBIAEoBRIQ" + "CghQbGF5ZXJJZBgCIAEoBRIlCgpCYXR0bGVJbmZvGAMgASgLMhEudHlwZXMu" + "QmF0dGxlSW5mbxIvCg9NaW5lV2FyQmFzZUluZm8YBCABKAsyFi50eXBlcy5N" + "aW5lV2FyQmFzZUluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeBattleInfoReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MineWarFightStartAck), (MessageParser)(object)MineWarFightStartAck.Parser, new string[4] { "PointId", "PlayerId", "BattleInfo", "MineWarBaseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
