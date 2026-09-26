using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeEndlessMineActivateAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEndlessMineActivateAwardReqReflection()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLkVuZGxlc3NNaW5lQWN0aXZhdGVBd2FyZFJlcS5wcm90bxIFdHlw" + "ZXMiHQobRW5kbGVzc01pbmVBY3RpdmF0ZUF3YXJkUmVxQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EndlessMineActivateAwardReq), (MessageParser)(object)EndlessMineActivateAwardReq.Parser, (string[])null, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
