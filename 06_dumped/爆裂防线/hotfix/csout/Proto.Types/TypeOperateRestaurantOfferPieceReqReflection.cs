using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateRestaurantOfferPieceReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateRestaurantOfferPieceReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVSZXN0YXVyYW50T2ZmZXJQaWVjZVJlcS5wcm90bxIF" + "dHlwZXMiUwoeT3BlcmF0ZVJlc3RhdXJhbnRPZmZlclBpZWNlUmVxEgwKBFVV" + "SUQYASABKAkSEAoIU2xvdERhdGEYAiADKAUSEQoJU2xvdEluZGV4GAMgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateRestaurantOfferPieceReq), (MessageParser)(object)OperateRestaurantOfferPieceReq.Parser, new string[3] { "UUID", "SlotData", "SlotIndex" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
