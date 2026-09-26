using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTruckTransportStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTruckTransportStartAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlRydWNrVHJhbnNwb3J0U3RhcnRBY2sucHJvdG8SBXR5cGVzGhh0" + "eXBlLlRydWNrU2VsZkluZm8ucHJvdG8iRQoWVHJ1Y2tUcmFuc3BvcnRTdGFy" + "dEFjaxIrCg1UcnVja1NlbGZJbmZvGAEgASgLMhQudHlwZXMuVHJ1Y2tTZWxm" + "SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeTruckSelfInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TruckTransportStartAck), (MessageParser)(object)TruckTransportStartAck.Parser, new string[1] { "TruckSelfInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
