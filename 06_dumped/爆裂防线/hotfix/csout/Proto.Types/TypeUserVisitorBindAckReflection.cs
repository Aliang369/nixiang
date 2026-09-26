using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeUserVisitorBindAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeUserVisitorBindAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlVzZXJWaXNpdG9yQmluZEFjay5wcm90bxIFdHlwZXMaF2VudW0u" + "RUNoYW5uZWxUeXBlLnByb3RvImQKElVzZXJWaXNpdG9yQmluZEFjaxIwCgxM" + "b2dpbkNoYW5uZWwYASABKA4yGi5FQ2hhbm5lbFR5cGUuRUNoYW5uZWxUeXBl" + "EgwKBEd1aWQYAiABKAMSDgoGVGlja2V0GAMgASgJQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChannelTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(UserVisitorBindAck), (MessageParser)(object)UserVisitorBindAck.Parser, new string[3] { "LoginChannel", "Guid", "Ticket" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
