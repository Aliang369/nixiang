using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeGameDataReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeGameDataReflection()
	{
		//IL_0888: Unknown result type (might be due to invalid IL or missing references)
		//IL_088e: Expected O, but got Unknown
		//IL_088e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChN0eXBlLkdhbWVEYXRhLnByb3RvEgV0eXBlcxoXdHlwZS5BY3Rpdml0eUlu" + "Zm8ucHJvdG8aHHR5cGUuQWR2ZXJ0aXNlbWVudEluZm8ucHJvdG8aFHR5cGUu" + "QXJlbmFJbmZvLnByb3RvGhd0eXBlLkF1eEF3YXJkSW5mby5wcm90bxoSdHlw" + "ZS5CYXJJbmZvLnByb3RvGhp0eXBlLkNyb3NzTGFkZGVySW5mby5wcm90bxoc" + "dHlwZS5Dcm9zc1BlYWtTZWxmSW5mby5wcm90bxoYdHlwZS5EYWlseVRhc2tJ" + "bmZvLnByb3RvGhZ0eXBlLkRlZmVuc2VJbmZvLnByb3RvGht0eXBlLkVsZW1l" + "bnRUcmlhbEluZm8ucHJvdG8aGXR5cGUuRWxpdGVRdWVzdEluZm8ucHJvdG8a" + "G3R5cGUuRW5kbGVzc0hvcmRlSW5mby5wcm90bxoWdHlwZS5FcXVpcERldGFp" + "bC5wcm90bxoYdHlwZS5FcXVpcFNsb3RJbmZvLnByb3RvGht0eXBlLkZhY2ls" + "aXR5U2VsZkluZm8ucHJvdG8aF3R5cGUuRm9vdGhvbGRJbmZvLnByb3RvGhl0" + "eXBlLkZyZWVHaWZ0RGV0YWlsLnByb3RvGhZ0eXBlLkZyaWVuZHNJbmZvLnBy" + "b3RvGhd0eXBlLkhhbmRib29rSW5mby5wcm90bxoTdHlwZS5IZWxwSW5mby5w" + "cm90bxoZdHlwZS5IZXJvQXdha2VuSW5mby5wcm90bxoVdHlwZS5IZXJvRGV0" + "YWlsLnByb3RvGhl0eXBlLkhlcm9Gb2xsb3dJbmZvLnByb3RvGhh0eXBlLkhl" + "cm9UcmlhbEluZm8ucHJvdG8aF3R5cGUuSG9tZWxhbmRJbmZvLnByb3RvGhN0" + "eXBlLklkbGVJbmZvLnByb3RvGhd0eXBlLkluY3ViYXRlSW5mby5wcm90bxoW" + "dHlwZS5Kb3VybmV5SW5mby5wcm90bxoYdHlwZS5MZWdlbmRhcnlJbmZvLnBy" + "b3RvGhl0eXBlLkxlZ2lvblNlbGZJbmZvLnByb3RvGhx0eXBlLkxlZ2lvbldh" + "ckJhc2VJbmZvLnByb3RvGhN0eXBlLkxvY2tJbmZvLnByb3RvGhh0eXBlLk1h" + "aW5RdWVzdEluZm8ucHJvdG8aFHR5cGUuTWVjaGFJbmZvLnByb3RvGhp0eXBl" + "Lk1pbmVXYXJCYXNlSW5mby5wcm90bxoedHlwZS5Nb25leUV4Y2hhbmdlRGV0" + "YWlsLnByb3RvGhl0eXBlLk9wZW5TZXJ2ZXJJbmZvLnByb3RvGhZ0eXBlLk9w" + "ZXJhdGVJbmZvLnByb3RvGhZ0eXBlLlBlZXJhZ2VJbmZvLnByb3RvGhh0eXBl" + "LlBsYXllckd1bkluZm8ucHJvdG8aGXR5cGUuUGxheWVyU3VpdEluZm8ucHJv" + "dG8aFXR5cGUuUHJvcERldGFpbC5wcm90bxoSdHlwZS5QdnBJbmZvLnByb3Rv" + "Gh10eXBlLlJlY2hhcmdlQWN0aXZlSW5mby5wcm90bxobdHlwZS5SZWNoYXJn" + "ZVNob3BJbmZvLnByb3RvGhl0eXBlLlJlc291cmNlRGV0YWlsLnByb3RvGhd0" + "eXBlLlJvbGVCYXNlSW5mby5wcm90bxoZdHlwZS5Sb2xlQ29tbW9uSW5mby5w" + "cm90bxoXdHlwZS5Sb2xlVGVhbUluZm8ucHJvdG8aFnR5cGUuU2VydmFudElu" + "Zm8ucHJvdG8aFXR5cGUuU2hvcERldGFpbC5wcm90bxoYdHlwZS5TdWJzY3Jp" + "YmVJbmZvLnByb3RvGhd0eXBlLlRhY3RpY0RldGFpbC5wcm90bxoXdHlwZS5U" + "cmVhc3VyZUluZm8ucHJvdG8aGHR5cGUuVHJ1Y2tTZWxmSW5mby5wcm90bxoS" + "dHlwZS5WaXBJbmZvLnByb3RvGhp0eXBlLlZvdWNoZXJTaG9wSW5mby5wcm90" + "bxoTdHlwZS5XYWxsSW5mby5wcm90bxoWdHlwZS5XYXJSb29tSW5mby5wcm90" + "bxocdHlwZS5Xb3JsZEJvc3NTZWxmSW5mby5wcm90bxoUdHlwZS5Xb3JsZElu" + "Zm8ucHJvdG8aGHR5cGUuV29ybGRMYWlySW5mby5wcm90byLmFwoIR2FtZURh" + "dGESKQoMUm9sZUJhc2VJbmZvGAEgASgLMhMudHlwZXMuUm9sZUJhc2VJbmZv" + "Ei0KDlJvbGVDb21tb25JbmZvGAIgASgLMhUudHlwZXMuUm9sZUNvbW1vbklu" + "Zm8SKQoMUm9sZVRlYW1JbmZvGAMgASgLMhMudHlwZXMuUm9sZVRlYW1JbmZv" + "EigKCU1vbmV5TGlzdBgEIAMoCzIVLnR5cGVzLlJlc291cmNlRGV0YWlsEiMK" + "CFByb3BMaXN0GAUgAygLMhEudHlwZXMuUHJvcERldGFpbBInCghJdGVtTGlz" + "dBgGIAMoCzIVLnR5cGVzLlJlc291cmNlRGV0YWlsEiUKCUVxdWlwTGlzdBgH" + "IAMoCzISLnR5cGVzLkVxdWlwRGV0YWlsEiMKCEhlcm9MaXN0GAggAygLMhEu" + "dHlwZXMuSGVyb0RldGFpbBIrCgxIZXJvRnJhZ0xpc3QYCSADKAsyFS50eXBl" + "cy5SZXNvdXJjZURldGFpbBImCgdHZW1MaXN0GAogAygLMhUudHlwZXMuUmVz" + "b3VyY2VEZXRhaWwSJgoHQm94TGlzdBgLIAMoCzIVLnR5cGVzLlJlc291cmNl" + "RGV0YWlsEiEKCExvY2tJbmZvGAwgASgLMg8udHlwZXMuTG9ja0luZm8SKQoM" + "SG9tZWxhbmRJbmZvGA0gASgLMhMudHlwZXMuSG9tZWxhbmRJbmZvEicKC1dh" + "clJvb21JbmZvGA4gASgLMhIudHlwZXMuV2FyUm9vbUluZm8SJwoLRGVmZW5z" + "ZUluZm8YDyABKAsyEi50eXBlcy5EZWZlbnNlSW5mbxIrCg1FcXVpcFNsb3RJ" + "bmZvGBAgASgLMhQudHlwZXMuRXF1aXBTbG90SW5mbxIrCg1NYWluUXVlc3RJ" + "bmZvGBEgASgLMhQudHlwZXMuTWFpblF1ZXN0SW5mbxItCg5FbGl0ZVF1ZXN0" + "SW5mbxgSIAEoCzIVLnR5cGVzLkVsaXRlUXVlc3RJbmZvEisKDUhlcm9Ucmlh" + "bEluZm8YEyABKAsyFC50eXBlcy5IZXJvVHJpYWxJbmZvEiMKCVdvcmxkSW5m" + "bxgUIAEoCzIQLnR5cGVzLldvcmxkSW5mbxInCgtQZWVyYWdlSW5mbxgVIAEo" + "CzISLnR5cGVzLlBlZXJhZ2VJbmZvEisKDURhaWx5VGFza0luZm8YFiABKAsy" + "FC50eXBlcy5EYWlseVRhc2tJbmZvEicKC0ZyaWVuZHNJbmZvGBcgASgLMhIu" + "dHlwZXMuRnJpZW5kc0luZm8SLQoOTGVnaW9uU2VsZkluZm8YGCABKAsyFS50" + "eXBlcy5MZWdpb25TZWxmSW5mbxIfCgdCYXJJbmZvGBkgASgLMg4udHlwZXMu" + "QmFySW5mbxIjCghTaG9wTGlzdBgaIAMoCzIRLnR5cGVzLlNob3BEZXRhaWwS" + "KQoMQXV4QXdhcmRJbmZvGBsgASgLMhMudHlwZXMuQXV4QXdhcmRJbmZvEjAK" + "DEV4Y2hhbmdlTGlzdBgcIAMoCzIaLnR5cGVzLk1vbmV5RXhjaGFuZ2VEZXRh" + "aWwSKwoMRnJlZUdpZnRMaXN0GB0gAygLMhUudHlwZXMuRnJlZUdpZnREZXRh" + "aWwSEAoISGVhZExpc3QYHiADKAUSFQoNSGVhZEZyYW1lTGlzdBgfIAMoBRIx" + "ChBSZWNoYXJnZVNob3BJbmZvGCAgASgLMhcudHlwZXMuUmVjaGFyZ2VTaG9w" + "SW5mbxI1ChJSZWNoYXJnZUFjdGl2ZUluZm8YISABKAsyGS50eXBlcy5SZWNo" + "YXJnZUFjdGl2ZUluZm8SHwoHVmlwSW5mbxgiIAEoCzIOLnR5cGVzLlZpcElu" + "Zm8SKwoNU3Vic2NyaWJlSW5mbxgjIAEoCzIULnR5cGVzLlN1YnNjcmliZUlu" + "Zm8SLwoPVm91Y2hlclNob3BJbmZvGCQgASgLMhYudHlwZXMuVm91Y2hlclNo" + "b3BJbmZvEikKDEFjdGl2aXR5SW5mbxglIAEoCzITLnR5cGVzLkFjdGl2aXR5" + "SW5mbxInCgtPcGVyYXRlSW5mbxgmIAEoCzISLnR5cGVzLk9wZXJhdGVJbmZv" + "EikKDEhhbmRib29rSW5mbxgnIAEoCzITLnR5cGVzLkhhbmRib29rSW5mbxIn" + "CghTdWl0SW5mbxgoIAEoCzIVLnR5cGVzLlBsYXllclN1aXRJbmZvEiUKB0d1" + "bkluZm8YKSABKAsyFC50eXBlcy5QbGF5ZXJHdW5JbmZvEi0KDk9wZW5TZXJ2" + "ZXJJbmZvGCogASgLMhUudHlwZXMuT3BlblNlcnZlckluZm8SIQoISWRsZUlu" + "Zm8YKyABKAsyDy50eXBlcy5JZGxlSW5mbxIzChFXb3JsZEJvc3NTZWxmSW5m" + "bxgsIAEoCzIYLnR5cGVzLldvcmxkQm9zc1NlbGZJbmZvEhkKEVByb2dyZXNz" + "RXZlbnRMaXN0GC0gAygFEjEKEEVsZW1lbnRUcmlhbEluZm8YLiABKAsyFy50" + "eXBlcy5FbGVtZW50VHJpYWxJbmZvEi0KDkhlcm9Bd2FrZW5JbmZvGC8gASgL" + "MhUudHlwZXMuSGVyb0F3YWtlbkluZm8SIwoJTWVjaGFJbmZvGDAgASgLMhAu" + "dHlwZXMuTWVjaGFJbmZvEjEKEEVuZGxlc3NIb3JkZUluZm8YMSABKAsyFy50" + "eXBlcy5FbmRsZXNzSG9yZGVJbmZvEicKC0pvdXJuZXlJbmZvGDIgASgLMhIu" + "dHlwZXMuSm91cm5leUluZm8SIQoIV2FsbEluZm8YMyABKAsyDy50eXBlcy5X" + "YWxsSW5mbxIrCg1MZWdlbmRhcnlJbmZvGDQgASgLMhQudHlwZXMuTGVnZW5k" + "YXJ5SW5mbxIhCghIZWxwSW5mbxg1IAEoCzIPLnR5cGVzLkhlbHBJbmZvEikK" + "DEZvb3Rob2xkSW5mbxg2IAEoCzITLnR5cGVzLkZvb3Rob2xkSW5mbxIjCglB" + "cmVuYUluZm8YNyABKAsyEC50eXBlcy5BcmVuYUluZm8SFAoMUmVtaW5kZXJM" + "aXN0GDggAygFEi8KD0Nyb3NzTGFkZGVySW5mbxg5IAEoCzIWLnR5cGVzLkNy" + "b3NzTGFkZGVySW5mbxInCghHZW5lTGlzdBg6IAMoCzIVLnR5cGVzLlJlc291" + "cmNlRGV0YWlsEikKDFRyZWFzdXJlSW5mbxg7IAEoCzITLnR5cGVzLlRyZWFz" + "dXJlSW5mbxIvCg9NaW5lV2FyQmFzZUluZm8YPCABKAsyFi50eXBlcy5NaW5l" + "V2FyQmFzZUluZm8SJwoLU2VydmFudEluZm8YPSABKAsyEi50eXBlcy5TZXJ2" + "YW50SW5mbxIpCgxJbmN1YmF0ZUluZm8YPiABKAsyEy50eXBlcy5JbmN1YmF0" + "ZUluZm8SLQoOSGVyb0ZvbGxvd0luZm8YPyABKAsyFS50eXBlcy5IZXJvRm9s" + "bG93SW5mbxIzChFDcm9zc1BlYWtTZWxmSW5mbxhAIAEoCzIYLnR5cGVzLkNy" + "b3NzUGVha1NlbGZJbmZvEisKDVRydWNrU2VsZkluZm8YQSABKAsyFC50eXBl" + "cy5UcnVja1NlbGZJbmZvEjMKEUxlZ2lvbldhckJhc2VJbmZvGEIgASgLMhgu" + "dHlwZXMuTGVnaW9uV2FyQmFzZUluZm8SMwoRQWR2ZXJ0aXNlbWVudEluZm8Y" + "QyABKAsyGC50eXBlcy5BZHZlcnRpc2VtZW50SW5mbxIxChBGYWNpbGl0eVNl" + "bGZJbmZvGEQgASgLMhcudHlwZXMuRmFjaWxpdHlTZWxmSW5mbxIrCg1Xb3Js" + "ZExhaXJJbmZvGEUgASgLMhQudHlwZXMuV29ybGRMYWlySW5mbxInCgpUYWN0" + "aWNMaXN0GEYgAygLMhMudHlwZXMuVGFjdGljRGV0YWlsEi0KDlRhY3RpY0Zy" + "YWdMaXN0GEcgAygLMhUudHlwZXMuUmVzb3VyY2VEZXRhaWwSHwoHUHZwSW5m" + "bxhIIAEoCzIOLnR5cGVzLlB2cEluZm9CLloJLi87cHJvdG9jqgILUHJvdG8u" + "VHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[62]
		{
			TypeActivityInfoReflection.Descriptor,
			TypeAdvertisementInfoReflection.Descriptor,
			TypeArenaInfoReflection.Descriptor,
			TypeAuxAwardInfoReflection.Descriptor,
			TypeBarInfoReflection.Descriptor,
			TypeCrossLadderInfoReflection.Descriptor,
			TypeCrossPeakSelfInfoReflection.Descriptor,
			TypeDailyTaskInfoReflection.Descriptor,
			TypeDefenseInfoReflection.Descriptor,
			TypeElementTrialInfoReflection.Descriptor,
			TypeEliteQuestInfoReflection.Descriptor,
			TypeEndlessHordeInfoReflection.Descriptor,
			TypeEquipDetailReflection.Descriptor,
			TypeEquipSlotInfoReflection.Descriptor,
			TypeFacilitySelfInfoReflection.Descriptor,
			TypeFootholdInfoReflection.Descriptor,
			TypeFreeGiftDetailReflection.Descriptor,
			TypeFriendsInfoReflection.Descriptor,
			TypeHandbookInfoReflection.Descriptor,
			TypeHelpInfoReflection.Descriptor,
			TypeHeroAwakenInfoReflection.Descriptor,
			TypeHeroDetailReflection.Descriptor,
			TypeHeroFollowInfoReflection.Descriptor,
			TypeHeroTrialInfoReflection.Descriptor,
			TypeHomelandInfoReflection.Descriptor,
			TypeIdleInfoReflection.Descriptor,
			TypeIncubateInfoReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor,
			TypeLegendaryInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor,
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLockInfoReflection.Descriptor,
			TypeMainQuestInfoReflection.Descriptor,
			TypeMechaInfoReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMoneyExchangeDetailReflection.Descriptor,
			TypeOpenServerInfoReflection.Descriptor,
			TypeOperateInfoReflection.Descriptor,
			TypePeerageInfoReflection.Descriptor,
			TypePlayerGunInfoReflection.Descriptor,
			TypePlayerSuitInfoReflection.Descriptor,
			TypePropDetailReflection.Descriptor,
			TypePvpInfoReflection.Descriptor,
			TypeRechargeActiveInfoReflection.Descriptor,
			TypeRechargeShopInfoReflection.Descriptor,
			TypeResourceDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor,
			TypeRoleCommonInfoReflection.Descriptor,
			TypeRoleTeamInfoReflection.Descriptor,
			TypeServantInfoReflection.Descriptor,
			TypeShopDetailReflection.Descriptor,
			TypeSubscribeInfoReflection.Descriptor,
			TypeTacticDetailReflection.Descriptor,
			TypeTreasureInfoReflection.Descriptor,
			TypeTruckSelfInfoReflection.Descriptor,
			TypeVipInfoReflection.Descriptor,
			TypeVoucherShopInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor,
			TypeWarRoomInfoReflection.Descriptor,
			TypeWorldBossSelfInfoReflection.Descriptor,
			TypeWorldInfoReflection.Descriptor,
			TypeWorldLairInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(GameData), (MessageParser)(object)GameData.Parser, new string[72]
			{
				"RoleBaseInfo", "RoleCommonInfo", "RoleTeamInfo", "MoneyList", "PropList", "ItemList", "EquipList", "HeroList", "HeroFragList", "GemList",
				"BoxList", "LockInfo", "HomelandInfo", "WarRoomInfo", "DefenseInfo", "EquipSlotInfo", "MainQuestInfo", "EliteQuestInfo", "HeroTrialInfo", "WorldInfo",
				"PeerageInfo", "DailyTaskInfo", "FriendsInfo", "LegionSelfInfo", "BarInfo", "ShopList", "AuxAwardInfo", "ExchangeList", "FreeGiftList", "HeadList",
				"HeadFrameList", "RechargeShopInfo", "RechargeActiveInfo", "VipInfo", "SubscribeInfo", "VoucherShopInfo", "ActivityInfo", "OperateInfo", "HandbookInfo", "SuitInfo",
				"GunInfo", "OpenServerInfo", "IdleInfo", "WorldBossSelfInfo", "ProgressEventList", "ElementTrialInfo", "HeroAwakenInfo", "MechaInfo", "EndlessHordeInfo", "JourneyInfo",
				"WallInfo", "LegendaryInfo", "HelpInfo", "FootholdInfo", "ArenaInfo", "ReminderList", "CrossLadderInfo", "GeneList", "TreasureInfo", "MineWarBaseInfo",
				"ServantInfo", "IncubateInfo", "HeroFollowInfo", "CrossPeakSelfInfo", "TruckSelfInfo", "LegionWarBaseInfo", "AdvertisementInfo", "FacilitySelfInfo", "WorldLairInfo", "TacticList",
				"TacticFragList", "PvpInfo"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
