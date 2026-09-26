using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateInfoReflection()
	{
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Expected O, but got Unknown
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLk9wZXJhdGVJbmZvLnByb3RvEgV0eXBlcxoadHlwZS5BZHNEaXJl" + "Y3REZXRhaWwucHJvdG8aGHR5cGUuQWRzR2lmdERldGFpbC5wcm90bxocdHlw" + "ZS5BZHNSb3VsZXR0ZURldGFpbC5wcm90bxoYdHlwZS5BZHNTaWduRGV0YWls" + "LnByb3RvGhx0eXBlLkFkc1poYW5MaW5nRGV0YWlsLnByb3RvGhh0eXBlLkJh" + "ckRyYXdEZXRhaWwucHJvdG8aG3R5cGUuQmFycmFnZUNtZERldGFpbC5wcm90" + "bxocdHlwZS5Cb3NzU3ViamVjdERldGFpbC5wcm90bxoadHlwZS5DaXBoZXJC" + "b3hEZXRhaWwucHJvdG8aG3R5cGUuQ29uc3VtZVN1bURldGFpbC5wcm90bxoc" + "dHlwZS5EYWlseUFjdGl2ZURldGFpbC5wcm90bxocdHlwZS5FbmRsZXNzTWlu" + "ZURldGFpbC5wcm90bxoVdHlwZS5GdXNpb25JbmZvLnByb3RvGhx0eXBlLkhv" + "bGlkYXlUYXNrRGV0YWlsLnByb3RvGh10eXBlLkluZmluaXRlR2lmdERldGFp" + "bC5wcm90bxoVdHlwZS5KaVppRGV0YWlsLnByb3RvGiF0eXBlLkxpbWl0VGlt" + "ZUNhdGNoZXJEZXRhaWwucHJvdG8aHnR5cGUuTGltaXRUaW1lSGVyb0RldGFp" + "bC5wcm90bxoidHlwZS5MaW1pdFRpbWVSb3VsZXR0ZURldGFpbC5wcm90bxoe" + "dHlwZS5MaW1pdFRpbWVTaG9wRGV0YWlsLnByb3RvGh10eXBlLkxpbWl0Vmlw" + "SGVyb0RldGFpbC5wcm90bxoZdHlwZS5Mb2dpblN1bURldGFpbC5wcm90bxod" + "dHlwZS5NZWNoYVN1YmplY3REZXRhaWwucHJvdG8aHHR5cGUuUHJheUxhbnRl" + "cm5EZXRhaWwucHJvdG8aHXR5cGUuUHJvcEV4Y2hhbmdlRGV0YWlsLnByb3Rv" + "Gh50eXBlLlJlY2hhcmdlRGFpbHlEZXRhaWwucHJvdG8aHXR5cGUuUmVjaGFy" + "Z2VHaWZ0RGV0YWlsLnByb3RvGhx0eXBlLlJlY2hhcmdlU3VtRGV0YWlsLnBy" + "b3RvGiF0eXBlLlJlc291cmNlRXhjaGFuZ2VEZXRhaWwucHJvdG8aG3R5cGUu" + "UmVzdGF1cmFudERldGFpbC5wcm90bxocdHlwZS5TY3JhdGNoQ2FyZERldGFp" + "bC5wcm90bxoZdHlwZS5Tbm93VmVpbERldGFpbC5wcm90bxofdHlwZS5UYWN0" + "aWNSb3VsZXR0ZURldGFpbC5wcm90bxoadHlwZS5UaGVtZVRhc2tEZXRhaWwu" + "cHJvdG8aHHR5cGUuV2FsbFN1YmplY3REZXRhaWwucHJvdG8aHXR5cGUuWmhh" + "bkxpbmdLaWxsRGV0YWlsLnByb3RvIpEQCgtPcGVyYXRlSW5mbxI1ChFSZWNo" + "YXJnZURhaWx5TGlzdBgBIAMoCzIaLnR5cGVzLlJlY2hhcmdlRGFpbHlEZXRh" + "aWwSNgoUUmVjaGFyZ2VEYWlseVN1bUxpc3QYAiADKAsyGC50eXBlcy5SZWNo" + "YXJnZVN1bURldGFpbBIxCg9SZWNoYXJnZVN1bUxpc3QYAyADKAsyGC50eXBl" + "cy5SZWNoYXJnZVN1bURldGFpbBIzChBSZWNoYXJnZUdpZnRMaXN0GAQgAygL" + "MhkudHlwZXMuUmVjaGFyZ2VHaWZ0RGV0YWlsEjQKE0NvbnN1bWVEYWlseVN1" + "bUxpc3QYBSADKAsyFy50eXBlcy5Db25zdW1lU3VtRGV0YWlsEi8KDkNvbnN1" + "bWVTdW1MaXN0GAYgAygLMhcudHlwZXMuQ29uc3VtZVN1bURldGFpbBI1ChFM" + "aW1pdFRpbWVIZXJvTGlzdBgHIAMoCzIaLnR5cGVzLkxpbWl0VGltZUhlcm9E" + "ZXRhaWwSNQoRTGltaXRUaW1lU2hvcExpc3QYCCADKAsyGi50eXBlcy5MaW1p" + "dFRpbWVTaG9wRGV0YWlsEjMKEExpbWl0VmlwSGVyb0xpc3QYCSADKAsyGS50" + "eXBlcy5MaW1pdFZpcEhlcm9EZXRhaWwSKwoMTG9naW5TdW1MaXN0GAogAygL" + "MhUudHlwZXMuTG9naW5TdW1EZXRhaWwSMQoPRGFpbHlBY3RpdmVMaXN0GAsg" + "AygLMhgudHlwZXMuRGFpbHlBY3RpdmVEZXRhaWwSKQoLQmFyRHJhd0xpc3QY" + "DCADKAsyFC50eXBlcy5CYXJEcmF3RGV0YWlsEjMKEFByb3BFeGNoYW5nZUxp" + "c3QYDSADKAsyGS50eXBlcy5Qcm9wRXhjaGFuZ2VEZXRhaWwSMQoPU2NyYXRj" + "aENhcmRMaXN0GA4gAygLMhgudHlwZXMuU2NyYXRjaENhcmREZXRhaWwSIwoI" + "SmlaaUxpc3QYDyADKAsyES50eXBlcy5KaVppRGV0YWlsEi0KDUNpcGhlckJv" + "eExpc3QYECADKAsyFi50eXBlcy5DaXBoZXJCb3hEZXRhaWwSLwoOQmFycmFn" + "ZUNtZExpc3QYESADKAsyFy50eXBlcy5CYXJyYWdlQ21kRGV0YWlsEjsKFExp" + "bWl0VGltZUNhdGNoZXJMaXN0GBIgAygLMh0udHlwZXMuTGltaXRUaW1lQ2F0" + "Y2hlckRldGFpbBIxCg9Ib2xpZGF5VGFza0xpc3QYEyADKAsyGC50eXBlcy5I" + "b2xpZGF5VGFza0RldGFpbBIxCg9QcmF5TGFudGVybkxpc3QYFCADKAsyGC50" + "eXBlcy5QcmF5TGFudGVybkRldGFpbBI7ChRSZXNvdXJjZUV4Y2hhbmdlTGlz" + "dBgVIAMoCzIdLnR5cGVzLlJlc291cmNlRXhjaGFuZ2VEZXRhaWwSJQoKRnVz" + "aW9uSW5mbxgWIAEoCzIRLnR5cGVzLkZ1c2lvbkluZm8SMwoQSW5maW5pdGVH" + "aWZ0TGlzdBgXIAMoCzIZLnR5cGVzLkluZmluaXRlR2lmdERldGFpbBI9ChVM" + "aW1pdFRpbWVSb3VsZXR0ZUxpc3QYGCADKAsyHi50eXBlcy5MaW1pdFRpbWVS" + "b3VsZXR0ZURldGFpbBIvCg5SZXN0YXVyYW50TGlzdBgZIAMoCzIXLnR5cGVz" + "LlJlc3RhdXJhbnREZXRhaWwSLQoNVGhlbWVUYXNrTGlzdBgaIAMoCzIWLnR5" + "cGVzLlRoZW1lVGFza0RldGFpbBIxCg9FbmRsZXNzTWluZUluZm8YGyABKAsy" + "GC50eXBlcy5FbmRsZXNzTWluZURldGFpbBIrCgxTbm93VmVpbExpc3QYHCAD" + "KAsyFS50eXBlcy5Tbm93VmVpbERldGFpbBI9ChVMaW1pdFRpbWVUcmVhc3Vy" + "ZUxpc3QYHSADKAsyHi50eXBlcy5MaW1pdFRpbWVSb3VsZXR0ZURldGFpbBI7" + "ChNMaW1pdFRpbWVWZW5kb3JMaXN0GB4gAygLMh4udHlwZXMuTGltaXRUaW1l" + "Um91bGV0dGVEZXRhaWwSMwoQWmhhbkxpbmdLaWxsTGlzdBgfIAMoCzIZLnR5" + "cGVzLlpoYW5MaW5nS2lsbERldGFpbBIxCg9Cb3NzU3ViamVjdExpc3QYICAD" + "KAsyGC50eXBlcy5Cb3NzU3ViamVjdERldGFpbBItCg1BZHNEaXJlY3RMaXN0" + "GCEgAygLMhYudHlwZXMuQWRzRGlyZWN0RGV0YWlsEikKC0Fkc0dpZnRMaXN0" + "GCIgAygLMhQudHlwZXMuQWRzR2lmdERldGFpbBIxCg9BZHNSb3VsZXR0ZUxp" + "c3QYIyADKAsyGC50eXBlcy5BZHNSb3VsZXR0ZURldGFpbBIpCgtBZHNTaWdu" + "TGlzdBgkIAMoCzIULnR5cGVzLkFkc1NpZ25EZXRhaWwSMQoPQWRzWmhhbkxp" + "bmdMaXN0GCUgAygLMhgudHlwZXMuQWRzWmhhbkxpbmdEZXRhaWwSMQoPV2Fs" + "bFN1YmplY3RMaXN0GCYgAygLMhgudHlwZXMuV2FsbFN1YmplY3REZXRhaWwS" + "MwoQTWVjaGFTdWJqZWN0TGlzdBgnIAMoCzIZLnR5cGVzLk1lY2hhU3ViamVj" + "dERldGFpbBI3ChJUYWN0aWNSb3VsZXR0ZUxpc3QYKCADKAsyGy50eXBlcy5U" + "YWN0aWNSb3VsZXR0ZURldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[36]
		{
			TypeAdsDirectDetailReflection.Descriptor,
			TypeAdsGiftDetailReflection.Descriptor,
			TypeAdsRouletteDetailReflection.Descriptor,
			TypeAdsSignDetailReflection.Descriptor,
			TypeAdsZhanLingDetailReflection.Descriptor,
			TypeBarDrawDetailReflection.Descriptor,
			TypeBarrageCmdDetailReflection.Descriptor,
			TypeBossSubjectDetailReflection.Descriptor,
			TypeCipherBoxDetailReflection.Descriptor,
			TypeConsumeSumDetailReflection.Descriptor,
			TypeDailyActiveDetailReflection.Descriptor,
			TypeEndlessMineDetailReflection.Descriptor,
			TypeFusionInfoReflection.Descriptor,
			TypeHolidayTaskDetailReflection.Descriptor,
			TypeInfiniteGiftDetailReflection.Descriptor,
			TypeJiZiDetailReflection.Descriptor,
			TypeLimitTimeCatcherDetailReflection.Descriptor,
			TypeLimitTimeHeroDetailReflection.Descriptor,
			TypeLimitTimeRouletteDetailReflection.Descriptor,
			TypeLimitTimeShopDetailReflection.Descriptor,
			TypeLimitVipHeroDetailReflection.Descriptor,
			TypeLoginSumDetailReflection.Descriptor,
			TypeMechaSubjectDetailReflection.Descriptor,
			TypePrayLanternDetailReflection.Descriptor,
			TypePropExchangeDetailReflection.Descriptor,
			TypeRechargeDailyDetailReflection.Descriptor,
			TypeRechargeGiftDetailReflection.Descriptor,
			TypeRechargeSumDetailReflection.Descriptor,
			TypeResourceExchangeDetailReflection.Descriptor,
			TypeRestaurantDetailReflection.Descriptor,
			TypeScratchCardDetailReflection.Descriptor,
			TypeSnowVeilDetailReflection.Descriptor,
			TypeTacticRouletteDetailReflection.Descriptor,
			TypeThemeTaskDetailReflection.Descriptor,
			TypeWallSubjectDetailReflection.Descriptor,
			TypeZhanLingKillDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateInfo), (MessageParser)(object)OperateInfo.Parser, new string[40]
			{
				"RechargeDailyList", "RechargeDailySumList", "RechargeSumList", "RechargeGiftList", "ConsumeDailySumList", "ConsumeSumList", "LimitTimeHeroList", "LimitTimeShopList", "LimitVipHeroList", "LoginSumList",
				"DailyActiveList", "BarDrawList", "PropExchangeList", "ScratchCardList", "JiZiList", "CipherBoxList", "BarrageCmdList", "LimitTimeCatcherList", "HolidayTaskList", "PrayLanternList",
				"ResourceExchangeList", "FusionInfo", "InfiniteGiftList", "LimitTimeRouletteList", "RestaurantList", "ThemeTaskList", "EndlessMineInfo", "SnowVeilList", "LimitTimeTreasureList", "LimitTimeVendorList",
				"ZhanLingKillList", "BossSubjectList", "AdsDirectList", "AdsGiftList", "AdsRouletteList", "AdsSignList", "AdsZhanLingList", "WallSubjectList", "MechaSubjectList", "TacticRouletteList"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
