using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeShopSlotDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeShopSlotDetailReflection()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlNob3BTbG90RGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5FTW9u" + "ZXlUeXBlLnByb3RvGhdlbnVtLkVSZWZyZXNoVHlwZS5wcm90bxoWdHlwZS5B" + "d2FyZERldGFpbC5wcm90byLOAQoOU2hvcFNsb3REZXRhaWwSDQoFSW5kZXgY" + "ASABKAUSJAoIU2xvdEl0ZW0YAiABKAsyEi50eXBlcy5Bd2FyZERldGFpbBIp" + "CglNb25leVR5cGUYAyABKA4yFi5FTW9uZXlUeXBlLkVNb25leVR5cGUSDQoF" + "UHJpY2UYBCABKAUSLwoLUmVmcmVzaFR5cGUYBSABKA4yGi5FUmVmcmVzaFR5" + "cGUuRVJlZnJlc2hUeXBlEg0KBUxpbWl0GAYgASgFEg0KBUNvdW50GAcgASgF" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEMoneyTypeReflection.Descriptor,
			EnumERefreshTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ShopSlotDetail), (MessageParser)(object)ShopSlotDetail.Parser, new string[7] { "Index", "SlotItem", "MoneyType", "Price", "RefreshType", "Limit", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
