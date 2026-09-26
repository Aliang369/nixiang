using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonShopGoodsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonShopGoodsReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLlNlYXNvblNob3BHb29kcy5wcm90bxIFdHlwZXMilwEKD1NlYXNv" + "blNob3BHb29kcxIOCgZTaG9wSWQYASABKAUSDwoHUmVzVHlwZRgCIAEoBRIN" + "CgVSZXNJZBgDIAEoBRIQCghSZXNDb3VudBgEIAEoBRIRCglNb25leVR5cGUY" + "BSABKAUSDQoFUHJpY2UYBiABKAUSEAoITGltaXROdW0YByABKAUSDgoGQm91" + "Z2h0GAggASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1l" + "XFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonShopGoods), (MessageParser)(object)SeasonShopGoods.Parser, new string[8] { "ShopId", "ResType", "ResId", "ResCount", "MoneyType", "Price", "LimitNum", "Bought" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
