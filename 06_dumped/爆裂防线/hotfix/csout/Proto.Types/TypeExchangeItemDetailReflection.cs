using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeExchangeItemDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeExchangeItemDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkV4Y2hhbmdlSXRlbURldGFpbC5wcm90bxIFdHlwZXMaFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8iiwEKEkV4Y2hhbmdlSXRlbURldGFpbBINCgVJ" + "bmRleBgBIAEoBRINCgVMaW1pdBgCIAEoBRINCgVDb3VudBgDIAEoBRIjCgdT" + "cmNMaXN0GAQgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSIwoHRHN0TGlzdBgF" + "IAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ExchangeItemDetail), (MessageParser)(object)ExchangeItemDetail.Parser, new string[5] { "Index", "Limit", "Count", "SrcList", "DstList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
