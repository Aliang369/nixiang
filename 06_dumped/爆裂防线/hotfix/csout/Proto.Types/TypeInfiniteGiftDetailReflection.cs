using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeInfiniteGiftDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeInfiniteGiftDetailReflection()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkluZmluaXRlR2lmdERldGFpbC5wcm90bxIFdHlwZXMaFWVudW0u" + "RUJvb2xTdGF0ZS5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoidHlw" + "ZS5JbmZpbml0ZUdpZnRHb29kc0RldGFpbC5wcm90bxoadHlwZS5PcGVyYXRl" + "QmFzZUluZm8ucHJvdG8aHnR5cGUuT3BlcmF0ZVRhcmdldERldGFpbC5wcm90" + "byKjAgoSSW5maW5pdGVHaWZ0RGV0YWlsEigKCEJhc2VJbmZvGAEgASgLMhYu" + "dHlwZXMuT3BlcmF0ZUJhc2VJbmZvEjEKCUdvb2RzTGlzdBgCIAMoCzIeLnR5" + "cGVzLkluZmluaXRlR2lmdEdvb2RzRGV0YWlsEi4KClRhcmdldExpc3QYAyAD" + "KAsyGi50eXBlcy5PcGVyYXRlVGFyZ2V0RGV0YWlsEhAKCEJ1eUNvdW50GAQg" + "ASgFEhIKClRvdGFsU2NvcmUYBSABKAUSKgoORGFpbHlGcmVlQXdhcmQYBiAD" + "KAsyEi50eXBlcy5Bd2FyZERldGFpbBIuCg5EYWlseUZyZWVTdGF0ZRgHIAEo" + "DjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeInfiniteGiftGoodsDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(InfiniteGiftDetail), (MessageParser)(object)InfiniteGiftDetail.Parser, new string[7] { "BaseInfo", "GoodsList", "TargetList", "BuyCount", "TotalScore", "DailyFreeAward", "DailyFreeState" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
