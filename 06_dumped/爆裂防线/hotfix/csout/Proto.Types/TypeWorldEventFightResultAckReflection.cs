using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldEventFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldEventFightResultAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLldvcmxkRXZlbnRGaWdodFJlc3VsdEFjay5wcm90bxIFdHlwZXMa" + "FnR5cGUuQXdhcmREZXRhaWwucHJvdG8aG3R5cGUuQmF0dGxlUmVzdWx0SW5m" + "by5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhR0eXBlLldvcmxkSW5m" + "by5wcm90byK8AQoYV29ybGRFdmVudEZpZ2h0UmVzdWx0QWNrEi0KDEJhdHRs" + "ZVNldHRsZRgBIAEoCzIXLnR5cGVzLkJhdHRsZVJlc3VsdEluZm8SIwoJV29y" + "bGRJbmZvGAIgASgLMhAudHlwZXMuV29ybGRJbmZvEiUKCUF3YXJkTGlzdBgD" + "IAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYBCABKAsy" + "ES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeWorldInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldEventFightResultAck), (MessageParser)(object)WorldEventFightResultAck.Parser, new string[4] { "BattleSettle", "WorldInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
