using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeChatSendReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeChatSendReqReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLkNoYXRTZW5kUmVxLnByb3RvEgV0eXBlcxoUZW51bS5FQ2hhdFR5" + "cGUucHJvdG8iWgoLQ2hhdFNlbmRSZXESJgoIQ2hhdFR5cGUYASABKA4yFC5F" + "Q2hhdFR5cGUuRUNoYXRUeXBlEhIKCkNoYXRUYXJnZXQYAiABKAMSDwoHQ29u" + "dGVudBgDIAEoCUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChatTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ChatSendReq), (MessageParser)(object)ChatSendReq.Parser, new string[3] { "ChatType", "ChatTarget", "Content" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
