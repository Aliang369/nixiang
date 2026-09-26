using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairRefreshAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairRefreshAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLldvcmxkTGFpclJlZnJlc2hBY2sucHJvdG8SBXR5cGVzGhx0eXBl" + "LldvcmxkTGFpckhlbHBJbmZvLnByb3RvGhh0eXBlLldvcmxkTGFpckluZm8u" + "cHJvdG8idwoTV29ybGRMYWlyUmVmcmVzaEFjaxIrCg1Xb3JsZExhaXJJbmZv" + "GAEgASgLMhQudHlwZXMuV29ybGRMYWlySW5mbxIzChFXb3JsZExhaXJIZWxw" + "TGlzdBgCIAMoCzIYLnR5cGVzLldvcmxkTGFpckhlbHBJbmZvQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeWorldLairHelpInfoReflection.Descriptor,
			TypeWorldLairInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairRefreshAck), (MessageParser)(object)WorldLairRefreshAck.Parser, new string[2] { "WorldLairInfo", "WorldLairHelpList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
