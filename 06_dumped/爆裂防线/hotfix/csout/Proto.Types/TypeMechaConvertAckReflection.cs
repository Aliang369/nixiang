using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeMechaConvertAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeMechaConvertAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLk1lY2hhQ29udmVydEFjay5wcm90bxIFdHlwZXMaFnR5cGUuQXdh" + "cmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byJfCg9NZWNo" + "YUNvbnZlcnRBY2sSJQoJQXdhcmRMaXN0GAEgAygLMhIudHlwZXMuQXdhcmRE" + "ZXRhaWwSJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(MechaConvertAck), (MessageParser)(object)MechaConvertAck.Parser, new string[2] { "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
