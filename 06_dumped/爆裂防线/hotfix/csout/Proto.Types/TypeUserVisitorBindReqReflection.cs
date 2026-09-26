using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeUserVisitorBindReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeUserVisitorBindReqReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlVzZXJWaXNpdG9yQmluZFJlcS5wcm90bxIFdHlwZXMaF2VudW0u" + "RUNoYW5uZWxUeXBlLnByb3RvGhhlbnVtLkVQbGF0Zm9ybVR5cGUucHJvdG8i" + "zgEKElVzZXJWaXNpdG9yQmluZFJlcRIuCghQbGF0Zm9ybRgBIAEoDjIcLkVQ" + "bGF0Zm9ybVR5cGUuRVBsYXRmb3JtVHlwZRIuCgpBcHBDaGFubmVsGAIgASgO" + "MhouRUNoYW5uZWxUeXBlLkVDaGFubmVsVHlwZRIUCgxWaXNpdG9yVG9rZW4Y" + "AyABKAkSLwoLQmluZENoYW5uZWwYBCABKA4yGi5FQ2hhbm5lbFR5cGUuRUNo" + "YW5uZWxUeXBlEhEKCUJpbmRUb2tlbhgFIAEoCUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(UserVisitorBindReq), (MessageParser)(object)UserVisitorBindReq.Parser, new string[5] { "Platform", "AppChannel", "VisitorToken", "BindChannel", "BindToken" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
