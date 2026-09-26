using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegendaryDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegendaryDataAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkxlZ2VuZGFyeURhdGFBY2sucHJvdG8SBXR5cGVzGhN0eXBlLkhl" + "bHBJbmZvLnByb3RvGhh0eXBlLkxlZ2VuZGFyeUluZm8ucHJvdG8iYgoQTGVn" + "ZW5kYXJ5RGF0YUFjaxIrCg1MZWdlbmRhcnlJbmZvGAEgASgLMhQudHlwZXMu" + "TGVnZW5kYXJ5SW5mbxIhCghIZWxwSW5mbxgCIAEoCzIPLnR5cGVzLkhlbHBJ" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeHelpInfoReflection.Descriptor,
			TypeLegendaryInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegendaryDataAck), (MessageParser)(object)LegendaryDataAck.Parser, new string[2] { "LegendaryInfo", "HelpInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
