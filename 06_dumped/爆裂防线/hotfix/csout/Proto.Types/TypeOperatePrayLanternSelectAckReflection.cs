using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperatePrayLanternSelectAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperatePrayLanternSelectAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVQcmF5TGFudGVyblNlbGVjdEFjay5wcm90bxIFdHlw" + "ZXMaFXR5cGUuRGF0YUNoYW5nZS5wcm90bxocdHlwZS5QcmF5TGFudGVybkRl" + "dGFpbC5wcm90byJ3ChtPcGVyYXRlUHJheUxhbnRlcm5TZWxlY3RBY2sSMQoP" + "UHJheUxhbnRlcm5MaXN0GAEgAygLMhgudHlwZXMuUHJheUxhbnRlcm5EZXRh" + "aWwSJQoKRGF0YUNoYW5nZRgCIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDataChangeReflection.Descriptor,
			TypePrayLanternDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperatePrayLanternSelectAck), (MessageParser)(object)OperatePrayLanternSelectAck.Parser, new string[2] { "PrayLanternList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
