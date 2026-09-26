using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRechargeGoodsBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRechargeGoodsBaseInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLlJlY2hhcmdlR29vZHNCYXNlSW5mby5wcm90bxIFdHlwZXMaE2Vu" + "dW0uRUNDWVR5cGUucHJvdG8iWAoVUmVjaGFyZ2VHb29kc0Jhc2VJbmZvEg8K" + "B0dvb2RzSWQYASABKAUSHwoDQ2N5GAIgASgOMhIuRUNDWVR5cGUuRUNDWVR5" + "cGUSDQoFUHJpY2UYAyABKAJCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECCYTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RechargeGoodsBaseInfo), (MessageParser)(object)RechargeGoodsBaseInfo.Parser, new string[3] { "GoodsId", "Ccy", "Price" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
