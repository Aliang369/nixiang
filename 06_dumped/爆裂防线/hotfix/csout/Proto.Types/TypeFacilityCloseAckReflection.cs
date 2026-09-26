using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityCloseAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityCloseAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkZhY2lsaXR5Q2xvc2VBY2sucHJvdG8SBXR5cGVzGht0eXBlLkZh" + "Y2lsaXR5U2VsZkluZm8ucHJvdG8iRQoQRmFjaWxpdHlDbG9zZUFjaxIxChBG" + "YWNpbGl0eVNlbGZJbmZvGAEgASgLMhcudHlwZXMuRmFjaWxpdHlTZWxmSW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeFacilitySelfInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityCloseAck), (MessageParser)(object)FacilityCloseAck.Parser, new string[1] { "FacilitySelfInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
