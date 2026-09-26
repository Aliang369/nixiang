using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldBossSaoDangAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldBossSaoDangAckReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldvcmxkQm9zc1Nhb0RhbmdBY2sucHJvdG8SBXR5cGVzGhZ0eXBl" + "LkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8aE3R5" + "cGUuUmFua0RhdGEucHJvdG8aGHR5cGUuV29ybGRCb3NzSW5mby5wcm90bxoc" + "dHlwZS5Xb3JsZEJvc3NTZWxmSW5mby5wcm90byLxAQoTV29ybGRCb3NzU2Fv" + "RGFuZ0FjaxIzChFXb3JsZEJvc3NTZWxmSW5mbxgBIAEoCzIYLnR5cGVzLldv" + "cmxkQm9zc1NlbGZJbmZvEioKEVdvcmxkQm9zc1NlbGZSYW5rGAIgASgLMg8u" + "dHlwZXMuUmFua0RhdGESKwoNV29ybGRCb3NzSW5mbxgDIAEoCzIULnR5cGVz" + "LldvcmxkQm9zc0luZm8SJQoJQXdhcmRMaXN0GAQgAygLMhIudHlwZXMuQXdh" + "cmREZXRhaWwSJQoKRGF0YUNoYW5nZRgFIAEoCzIRLnR5cGVzLkRhdGFDaGFu" + "Z2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRankDataReflection.Descriptor,
			TypeWorldBossInfoReflection.Descriptor,
			TypeWorldBossSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldBossSaoDangAck), (MessageParser)(object)WorldBossSaoDangAck.Parser, new string[5] { "WorldBossSelfInfo", "WorldBossSelfRank", "WorldBossInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
