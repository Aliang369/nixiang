using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFootholdConvertAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFootholdConvertAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkZvb3Rob2xkQ29udmVydEFjay5wcm90bxIFdHlwZXMaFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byJiChJG" + "b290aG9sZENvbnZlcnRBY2sSJQoJQXdhcmRMaXN0GAEgAygLMhIudHlwZXMu" + "QXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFD" + "aGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FootholdConvertAck), (MessageParser)(object)FootholdConvertAck.Parser, new string[2] { "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
