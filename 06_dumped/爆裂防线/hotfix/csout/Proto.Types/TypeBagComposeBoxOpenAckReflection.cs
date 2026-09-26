using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBagComposeBoxOpenAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBagComposeBoxOpenAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkJhZ0NvbXBvc2VCb3hPcGVuQWNrLnByb3RvEgV0eXBlcxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvImQK" + "FEJhZ0NvbXBvc2VCb3hPcGVuQWNrEiUKCUF3YXJkTGlzdBgBIAMoCzISLnR5" + "cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAiABKAsyES50eXBlcy5E" + "YXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BagComposeBoxOpenAck), (MessageParser)(object)BagComposeBoxOpenAck.Parser, new string[2] { "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
