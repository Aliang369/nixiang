using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePayCreateOrderSignAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePayCreateOrderSignAckReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlBheUNyZWF0ZU9yZGVyU2lnbkFjay5wcm90bxIFdHlwZXMaE2Vu" + "dW0uRUNDWVR5cGUucHJvdG8imQEKFVBheUNyZWF0ZU9yZGVyU2lnbkFjaxIP" + "CgdPcmRlcklkGAEgASgJEg8KB0dvb2RzSWQYAiABKAUSHwoDQ0NZGAMgASgO" + "MhIuRUNDWVR5cGUuRUNDWVR5cGUSDQoFUHJpY2UYBCABKAISDwoHT3JkZXJU" + "bRgFIAEoAxIOCgZWZXJpZnkYBiABKAkSDQoFRXh0cmEYByABKAlCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECCYTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PayCreateOrderSignAck), (MessageParser)(object)PayCreateOrderSignAck.Parser, new string[7] { "OrderId", "GoodsId", "CCY", "Price", "OrderTm", "Verify", "Extra" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
