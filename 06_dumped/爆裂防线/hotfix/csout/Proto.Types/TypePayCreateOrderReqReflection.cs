using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePayCreateOrderReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePayCreateOrderReqReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlBheUNyZWF0ZU9yZGVyUmVxLnByb3RvEgV0eXBlcxoXZW51bS5F" + "Q2hhbm5lbFR5cGUucHJvdG8aGGVudW0uRVBsYXRmb3JtVHlwZS5wcm90byKE" + "AQoRUGF5Q3JlYXRlT3JkZXJSZXESLgoIUGxhdGZvcm0YASABKA4yHC5FUGxh" + "dGZvcm1UeXBlLkVQbGF0Zm9ybVR5cGUSLgoKQXBwQ2hhbm5lbBgCIAEoDjIa" + "LkVDaGFubmVsVHlwZS5FQ2hhbm5lbFR5cGUSDwoHR29vZHNJZBgDIAEoBUIu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumEChannelTypeReflection.Descriptor,
			EnumEPlatformTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PayCreateOrderReq), (MessageParser)(object)PayCreateOrderReq.Parser, new string[3] { "Platform", "AppChannel", "GoodsId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
