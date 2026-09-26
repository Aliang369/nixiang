using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeElementTrialDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialDataAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLkVsZW1lbnRUcmlhbERhdGFBY2sucHJvdG8SBXR5cGVzGht0eXBl" + "LkVsZW1lbnRUcmlhbEluZm8ucHJvdG8iSAoTRWxlbWVudFRyaWFsRGF0YUFj" + "axIxChBFbGVtZW50VHJpYWxJbmZvGAEgASgLMhcudHlwZXMuRWxlbWVudFRy" + "aWFsSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeElementTrialInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialDataAck), (MessageParser)(object)ElementTrialDataAck.Parser, new string[1] { "ElementTrialInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
