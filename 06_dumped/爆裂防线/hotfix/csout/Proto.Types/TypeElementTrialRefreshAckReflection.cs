using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeElementTrialRefreshAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeElementTrialRefreshAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkVsZW1lbnRUcmlhbFJlZnJlc2hBY2sucHJvdG8SBXR5cGVzGht0" + "eXBlLkVsZW1lbnRUcmlhbEluZm8ucHJvdG8iSwoWRWxlbWVudFRyaWFsUmVm" + "cmVzaEFjaxIxChBFbGVtZW50VHJpYWxJbmZvGAEgASgLMhcudHlwZXMuRWxl" + "bWVudFRyaWFsSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeElementTrialInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ElementTrialRefreshAck), (MessageParser)(object)ElementTrialRefreshAck.Parser, new string[1] { "ElementTrialInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
