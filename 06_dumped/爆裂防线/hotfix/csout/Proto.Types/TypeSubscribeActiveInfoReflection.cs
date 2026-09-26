using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSubscribeActiveInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSubscribeActiveInfoReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlN1YnNjcmliZUFjdGl2ZUluZm8ucHJvdG8SBXR5cGVzIm0KE1N1" + "YnNjcmliZUFjdGl2ZUluZm8SDwoHU3RhcnRUbRgBIAEoAxINCgVFbmRUbRgC" + "IAEoAxIRCglOZXh0RGFpbHkYAyABKAMSEAoITmV4dFdlZWsYBCABKAMSEQoJ" + "TmV4dE1vbnRoGAUgASgDQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SubscribeActiveInfo), (MessageParser)(object)SubscribeActiveInfo.Parser, new string[5] { "StartTm", "EndTm", "NextDaily", "NextWeek", "NextMonth" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
