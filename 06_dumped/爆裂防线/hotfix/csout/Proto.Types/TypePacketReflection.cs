using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypePacketReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePacketReflection()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChF0eXBlLlBhY2tldC5wcm90bxIFdHlwZXMiWQoGUGFja2V0Eg0KBU1zZ0lk" + "GAEgASgFEgsKA1ZlchgCIAEoBRIKCgJUbRgDIAEoAxILCgNTZXEYBCABKAUS" + "DAoERGF0YRgFIAEoDBIMCgRTaWduGAYgASgJQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(Packet), (MessageParser)(object)Packet.Parser, new string[6] { "MsgId", "Ver", "Tm", "Seq", "Data", "Sign" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
