using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessMineDigAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMineDigAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkVuZGxlc3NNaW5lRGlnQWNrLnByb3RvEgV0eXBlcxoWdHlwZS5B" + "d2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhp0eXBl" + "LkVuZGxlc3NNaW5lSW5mby5wcm90byKSAQoRRW5kbGVzc01pbmVEaWdBY2sS" + "LwoPRW5kbGVzc01pbmVJbmZvGAEgASgLMhYudHlwZXMuRW5kbGVzc01pbmVJ" + "bmZvEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUK" + "CkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeEndlessMineInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMineDigAck), (MessageParser)(object)EndlessMineDigAck.Parser, new string[3] { "EndlessMineInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
