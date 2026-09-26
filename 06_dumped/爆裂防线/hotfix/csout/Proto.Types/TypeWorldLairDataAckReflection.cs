using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairDataAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLldvcmxkTGFpckRhdGFBY2sucHJvdG8SBXR5cGVzGhx0eXBlLldv" + "cmxkTGFpckhlbHBJbmZvLnByb3RvGhh0eXBlLldvcmxkTGFpckluZm8ucHJv" + "dG8idAoQV29ybGRMYWlyRGF0YUFjaxIrCg1Xb3JsZExhaXJJbmZvGAEgASgL" + "MhQudHlwZXMuV29ybGRMYWlySW5mbxIzChFXb3JsZExhaXJIZWxwTGlzdBgC" + "IAMoCzIYLnR5cGVzLldvcmxkTGFpckhlbHBJbmZvQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeWorldLairHelpInfoReflection.Descriptor,
			TypeWorldLairInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairDataAck), (MessageParser)(object)WorldLairDataAck.Parser, new string[2] { "WorldLairInfo", "WorldLairHelpList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
