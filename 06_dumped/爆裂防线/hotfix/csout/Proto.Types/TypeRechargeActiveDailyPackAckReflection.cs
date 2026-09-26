using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRechargeActiveDailyPackAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeActiveDailyPackAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLlJlY2hhcmdlQWN0aXZlRGFpbHlQYWNrQWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnBy" + "b3RvGh10eXBlLlJlY2hhcmdlQWN0aXZlSW5mby5wcm90byKhAQoaUmVjaGFy" + "Z2VBY3RpdmVEYWlseVBhY2tBY2sSNQoSUmVjaGFyZ2VBY3RpdmVJbmZvGAEg" + "ASgLMhkudHlwZXMuUmVjaGFyZ2VBY3RpdmVJbmZvEiUKCUF3YXJkTGlzdBgC" + "IAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsy" + "ES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVz" + "ygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeActiveDailyPackAck), (MessageParser)(object)RechargeActiveDailyPackAck.Parser, new string[3] { "RechargeActiveInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
