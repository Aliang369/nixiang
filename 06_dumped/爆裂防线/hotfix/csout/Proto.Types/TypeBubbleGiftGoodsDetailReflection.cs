using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeBubbleGiftGoodsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBubbleGiftGoodsDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkJ1YmJsZUdpZnRHb29kc0RldGFpbC5wcm90bxIFdHlwZXMaFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8iiwEKFUJ1YmJsZUdpZnRHb29kc0RldGFp" + "bBIKCgJJZBgBIAEoBRIPCgdTdGFydFRtGAIgASgDEg0KBUVuZFRtGAMgASgD" + "Eg0KBVByaWNlGAQgASgFEhAKCERpc2NvdW50GAUgASgFEiUKCUF3YXJkTGlz" + "dBgGIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BubbleGiftGoodsDetail), (MessageParser)(object)BubbleGiftGoodsDetail.Parser, new string[6] { "Id", "StartTm", "EndTm", "Price", "Discount", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
