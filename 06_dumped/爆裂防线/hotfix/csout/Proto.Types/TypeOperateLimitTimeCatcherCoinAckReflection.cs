using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateLimitTimeCatcherCoinAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateLimitTimeCatcherCoinAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVMaW1pdFRpbWVDYXRjaGVyQ29pbkFjay5wcm90bxIF" + "dHlwZXMaFXR5cGUuRGF0YUNoYW5nZS5wcm90bxohdHlwZS5MaW1pdFRpbWVD" + "YXRjaGVyRGV0YWlsLnByb3RvIoQBCh5PcGVyYXRlTGltaXRUaW1lQ2F0Y2hl" + "ckNvaW5BY2sSOwoUTGltaXRUaW1lQ2F0Y2hlckxpc3QYASADKAsyHS50eXBl" + "cy5MaW1pdFRpbWVDYXRjaGVyRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAiABKAsy" + "ES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypeLimitTimeCatcherDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateLimitTimeCatcherCoinAck), (MessageParser)(object)OperateLimitTimeCatcherCoinAck.Parser, new string[2] { "LimitTimeCatcherList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
