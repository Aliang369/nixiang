using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOrderDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOrderDetailReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLk9yZGVyRGV0YWlsLnByb3RvEgV0eXBlcxoTZW51bS5FQ0NZVHlw" + "ZS5wcm90bxoXZW51bS5FQ2hhbm5lbFR5cGUucHJvdG8ikQEKC09yZGVyRGV0" + "YWlsEg8KB09yZGVySWQYASABKAkSLgoKQXBwQ2hhbm5lbBgCIAEoDjIaLkVD" + "aGFubmVsVHlwZS5FQ2hhbm5lbFR5cGUSHwoDQ0NZGAMgASgOMhIuRUNDWVR5" + "cGUuRUNDWVR5cGUSEAoIQnV5Q291bnQYBCABKAUSDgoGQW1vdW50GAUgASgC" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			EnumECCYTypeReflection.Descriptor,
			EnumEChannelTypeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OrderDetail), (MessageParser)(object)OrderDetail.Parser, new string[5] { "OrderId", "AppChannel", "CCY", "BuyCount", "Amount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
