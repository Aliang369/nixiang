using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairHelpDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairHelpDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLldvcmxkTGFpckhlbHBEYXRhQWNrLnByb3RvEgV0eXBlcxocdHlw" + "ZS5Xb3JsZExhaXJIZWxwSW5mby5wcm90byJCChRXb3JsZExhaXJIZWxwRGF0" + "YUFjaxIqCghIZWxwSW5mbxgBIAEoCzIYLnR5cGVzLldvcmxkTGFpckhlbHBJ" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldLairHelpInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairHelpDataAck), (MessageParser)(object)WorldLairHelpDataAck.Parser, new string[1] { "HelpInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
