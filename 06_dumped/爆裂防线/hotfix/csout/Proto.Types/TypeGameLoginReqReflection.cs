using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeGameLoginReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGameLoginReqReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLkdhbWVMb2dpblJlcS5wcm90bxIFdHlwZXMaF2VudW0uRUNoYW5u" + "ZWxUeXBlLnByb3RvGhhlbnVtLkVQbGF0Zm9ybVR5cGUucHJvdG8ingEKDEdh" + "bWVMb2dpblJlcRIuCghQbGF0Zm9ybRgBIAEoDjIcLkVQbGF0Zm9ybVR5cGUu" + "RVBsYXRmb3JtVHlwZRIuCgpBcHBDaGFubmVsGAIgASgOMhouRUNoYW5uZWxU" + "eXBlLkVDaGFubmVsVHlwZRIQCghTZXJ2ZXJJZBgDIAEoBRIMCgRHdWlkGAQg" + "ASgDEg4KBlRpY2tldBgFIAEoCUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GameLoginReq), (MessageParser)(object)GameLoginReq.Parser, new string[5] { "Platform", "AppChannel", "ServerId", "Guid", "Ticket" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
