using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePayCreateOrderAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePayCreateOrderAckReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlBheUNyZWF0ZU9yZGVyQWNrLnByb3RvEgV0eXBlcxoTZW51bS5F" + "Q0NZVHlwZS5wcm90byKGAQoRUGF5Q3JlYXRlT3JkZXJBY2sSDwoHT3JkZXJJ" + "ZBgBIAEoCRIPCgdHb29kc0lkGAIgASgFEh8KA0NDWRgDIAEoDjISLkVDQ1lU" + "eXBlLkVDQ1lUeXBlEg0KBVByaWNlGAQgASgCEg8KB09yZGVyVG0YBSABKAMS" + "DgoGVmVyaWZ5GAYgASgJQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECCYTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PayCreateOrderAck), (MessageParser)(object)PayCreateOrderAck.Parser, new string[6] { "OrderId", "GoodsId", "CCY", "Price", "OrderTm", "Verify" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
