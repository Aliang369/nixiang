using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeUserLoginAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeUserLoginAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlVzZXJMb2dpbkFjay5wcm90bxIFdHlwZXMaF2VudW0uRUNoYW5u" + "ZWxUeXBlLnByb3RvInYKDFVzZXJMb2dpbkFjaxIwCgxMb2dpbkNoYW5uZWwY" + "ASABKA4yGi5FQ2hhbm5lbFR5cGUuRUNoYW5uZWxUeXBlEgwKBEd1aWQYAiAB" + "KAMSDgoGVGlja2V0GAMgASgJEhYKDkxvZ2luQWNjb3VudElEGAQgASgJQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEChannelTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(UserLoginAck), (MessageParser)(object)UserLoginAck.Parser, new string[4] { "LoginChannel", "Guid", "Ticket", "LoginAccountID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
