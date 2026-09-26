using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeActivityBubbleGiftRandAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeActivityBubbleGiftRandAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkFjdGl2aXR5QnViYmxlR2lmdFJhbmRBY2sucHJvdG8SBXR5cGVz" + "Ghd0eXBlLkFjdGl2aXR5SW5mby5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnBy" + "b3RvIm0KGUFjdGl2aXR5QnViYmxlR2lmdFJhbmRBY2sSKQoMQWN0aXZpdHlJ" + "bmZvGAEgASgLMhMudHlwZXMuQWN0aXZpdHlJbmZvEiUKCkRhdGFDaGFuZ2UY" + "AiABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeActivityInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ActivityBubbleGiftRandAck), (MessageParser)(object)ActivityBubbleGiftRandAck.Parser, new string[2] { "ActivityInfo", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
