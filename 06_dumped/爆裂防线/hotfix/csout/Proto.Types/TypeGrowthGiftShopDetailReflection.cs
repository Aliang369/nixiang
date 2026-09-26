using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeGrowthGiftShopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGrowthGiftShopDetailReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLkdyb3d0aEdpZnRTaG9wRGV0YWlsLnByb3RvEgV0eXBlcxoTZW51" + "bS5FQ0NZVHlwZS5wcm90byJpChRHcm93dGhHaWZ0U2hvcERldGFpbBIPCgdH" + "b29kc0lkGAEgASgFEh8KA0NjeRgCIAEoDjISLkVDQ1lUeXBlLkVDQ1lUeXBl" + "Eg0KBVByaWNlGAMgASgCEhAKCEV4cGlyZVRtGAQgASgDQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECCYTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GrowthGiftShopDetail), (MessageParser)(object)GrowthGiftShopDetail.Parser, new string[4] { "GoodsId", "Ccy", "Price", "ExpireTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
