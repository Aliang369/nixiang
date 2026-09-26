using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventPopupConfirmAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventPopupConfirmAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLldvcmxkRHVuZ2VvbkV2ZW50UG9wdXBDb25maXJtQWNrLnByb3Rv" + "EgV0eXBlcxobdHlwZS5EdW5nZW9uQXdhcmRJbmZvLnByb3RvGhp0eXBlLkR1" + "bmdlb25CYXNlSW5mby5wcm90bxoZdHlwZS5EdW5nZW9uTWFwSW5mby5wcm90" + "byK1AQogV29ybGREdW5nZW9uRXZlbnRQb3B1cENvbmZpcm1BY2sSLwoPRHVu" + "Z2VvbkJhc2VJbmZvGAEgASgLMhYudHlwZXMuRHVuZ2VvbkJhc2VJbmZvEi0K" + "DkR1bmdlb25NYXBJbmZvGAIgASgLMhUudHlwZXMuRHVuZ2Vvbk1hcEluZm8S" + "MQoQRHVuZ2VvbkF3YXJkSW5mbxgDIAEoCzIXLnR5cGVzLkR1bmdlb25Bd2Fy" + "ZEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeDungeonAwardInfoReflection.Descriptor,
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventPopupConfirmAck), (MessageParser)(object)WorldDungeonEventPopupConfirmAck.Parser, new string[3] { "DungeonBaseInfo", "DungeonMapInfo", "DungeonAwardInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
