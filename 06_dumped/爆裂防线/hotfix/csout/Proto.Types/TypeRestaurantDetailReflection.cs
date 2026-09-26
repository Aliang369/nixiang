using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRestaurantDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRestaurantDetailReflection()
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlJlc3RhdXJhbnREZXRhaWwucHJvdG8SBXR5cGVzGhVlbnVtLkVC" + "b29sU3RhdGUucHJvdG8aFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aGnR5cGUu" + "T3BlcmF0ZUJhc2VJbmZvLnByb3RvGix0eXBlLk9wZXJhdGVSZXN0YXVyYW50" + "RGFpbHlBd2FyZERldGFpbC5wcm90bxoodHlwZS5PcGVyYXRlUmVzdGF1cmFu" + "dFRhcmdldERldGFpbC5wcm90bxondHlwZS5PcGVyYXRlVGFyZ2V0TmV4dEdy" + "YW5kRGV0YWlsLnByb3RvIr4DChBSZXN0YXVyYW50RGV0YWlsEigKCEJhc2VJ" + "bmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZvEhAKCEJ1eUNvdW50" + "GAIgASgFEi0KDUFjdGl2YXRlQXdhcmQYAyABKA4yFi5FQm9vbFN0YXRlLkVC" + "b29sU3RhdGUSPAoKRGFpbHlBd2FyZBgEIAEoCzIoLnR5cGVzLk9wZXJhdGVS" + "ZXN0YXVyYW50RGFpbHlBd2FyZERldGFpbBImCgpTaW5nbGVDb3N0GAUgAygL" + "MhIudHlwZXMuQXdhcmREZXRhaWwSEAoIUHJpY2VPZmYYBiABKAUSEAoIU2xv" + "dExpc3QYByADKAUSEgoKU2xvdFN5bmNUbRgIIAEoAxIUCgxTbG90Q291bnRN" + "YXgYCSABKAUSOgoMVGFyZ2V0RGV0YWlsGAogASgLMiQudHlwZXMuT3BlcmF0" + "ZVJlc3RhdXJhbnRUYXJnZXREZXRhaWwSPAoPVGFyZ2V0TmV4dEdyYW5kGAsg" + "ASgLMiMudHlwZXMuT3BlcmF0ZVRhcmdldE5leHRHcmFuZERldGFpbBIRCglP" + "ZmZlckxpc3QYDCADKAVCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[6]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateRestaurantDailyAwardDetailReflection.Descriptor,
			TypeOperateRestaurantTargetDetailReflection.Descriptor,
			TypeOperateTargetNextGrandDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RestaurantDetail), (MessageParser)(object)RestaurantDetail.Parser, new string[12]
			{
				"BaseInfo", "BuyCount", "ActivateAward", "DailyAward", "SingleCost", "PriceOff", "SlotList", "SlotSyncTm", "SlotCountMax", "TargetDetail",
				"TargetNextGrand", "OfferList"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
