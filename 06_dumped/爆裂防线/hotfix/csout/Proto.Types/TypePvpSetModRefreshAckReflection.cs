using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePvpSetModRefreshAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePvpSetModRefreshAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlB2cFNldE1vZFJlZnJlc2hBY2sucHJvdG8SBXR5cGVzGhV0eXBl" + "LkRhdGFDaGFuZ2UucHJvdG8aEnR5cGUuUHZwSW5mby5wcm90byJdChNQdnBT" + "ZXRNb2RSZWZyZXNoQWNrEh8KB1B2cEluZm8YASABKAsyDi50eXBlcy5QdnBJ" + "bmZvEiUKCkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePvpInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PvpSetModRefreshAck), (MessageParser)(object)PvpSetModRefreshAck.Parser, new string[2] { "PvpInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
