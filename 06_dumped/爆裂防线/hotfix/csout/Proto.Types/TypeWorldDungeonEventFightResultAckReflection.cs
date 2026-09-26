using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldDungeonEventFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldDungeonEventFightResultAckReflection()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLldvcmxkRHVuZ2VvbkV2ZW50RmlnaHRSZXN1bHRBY2sucHJvdG8S" + "BXR5cGVzGht0eXBlLkJhdHRsZVJlc3VsdEluZm8ucHJvdG8aG3R5cGUuRHVu" + "Z2VvbkF3YXJkSW5mby5wcm90bxoadHlwZS5EdW5nZW9uQmFzZUluZm8ucHJv" + "dG8aGXR5cGUuRHVuZ2Vvbk1hcEluZm8ucHJvdG8aFHR5cGUuV29ybGRJbmZv" + "LnByb3RvIogCCh9Xb3JsZER1bmdlb25FdmVudEZpZ2h0UmVzdWx0QWNrEi0K" + "DEJhdHRsZVNldHRsZRgBIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm8S" + "LwoPRHVuZ2VvbkJhc2VJbmZvGAIgASgLMhYudHlwZXMuRHVuZ2VvbkJhc2VJ" + "bmZvEi0KDkR1bmdlb25NYXBJbmZvGAMgASgLMhUudHlwZXMuRHVuZ2Vvbk1h" + "cEluZm8SMQoQRHVuZ2VvbkF3YXJkSW5mbxgEIAEoCzIXLnR5cGVzLkR1bmdl" + "b25Bd2FyZEluZm8SIwoJV29ybGRJbmZvGAUgASgLMhAudHlwZXMuV29ybGRJ" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeBattleResultInfoReflection.Descriptor,
			TypeDungeonAwardInfoReflection.Descriptor,
			TypeDungeonBaseInfoReflection.Descriptor,
			TypeDungeonMapInfoReflection.Descriptor,
			TypeWorldInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldDungeonEventFightResultAck), (MessageParser)(object)WorldDungeonEventFightResultAck.Parser, new string[5] { "BattleSettle", "DungeonBaseInfo", "DungeonMapInfo", "DungeonAwardInfo", "WorldInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
