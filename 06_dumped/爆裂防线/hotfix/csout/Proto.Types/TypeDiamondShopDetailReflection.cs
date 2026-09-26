using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeDiamondShopDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDiamondShopDetailReflection()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkRpYW1vbmRTaG9wRGV0YWlsLnByb3RvEgV0eXBlcxoTZW51bS5F" + "Q0NZVHlwZS5wcm90byKPAQoRRGlhbW9uZFNob3BEZXRhaWwSDwoHR29vZHNJ" + "ZBgBIAEoBRIfCgNDY3kYAiABKA4yEi5FQ0NZVHlwZS5FQ0NZVHlwZRINCgVQ" + "cmljZRgDIAEoAhIPCgdEaWFtb25kGAQgASgFEhMKC0dpdmVEaWFtb25kGAUg" + "ASgFEhMKC0JvdWdodENvdW50GAYgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumECCYTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DiamondShopDetail), (MessageParser)(object)DiamondShopDetail.Parser, new string[6] { "GoodsId", "Ccy", "Price", "Diamond", "GiveDiamond", "BoughtCount" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
