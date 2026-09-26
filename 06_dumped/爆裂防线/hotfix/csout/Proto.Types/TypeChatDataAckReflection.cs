using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatDataAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLkNoYXREYXRhQWNrLnByb3RvEgV0eXBlcxoUZW51bS5FQ2hhdFR5" + "cGUucHJvdG8aFXR5cGUuQ2hhdERldGFpbC5wcm90byJaCgtDaGF0RGF0YUFj" + "axImCghDaGF0VHlwZRgBIAEoDjIULkVDaGF0VHlwZS5FQ2hhdFR5cGUSIwoI" + "Q2hhdExpc3QYAiADKAsyES50eXBlcy5DaGF0RGV0YWlsQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEChatTypeReflection.Descriptor,
			TypeChatDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatDataAck), (MessageParser)(object)ChatDataAck.Parser, new string[2] { "ChatType", "ChatList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
