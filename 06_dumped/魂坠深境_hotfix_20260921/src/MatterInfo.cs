using System.Collections.Generic;
using UnityEngine;

public class MatterInfo : IGlobalItemDisplaySetup, IValidate
{
	[SaveField(null, null)]
	public int count;

	[SaveField(null, null)]
	public MatterDef matterDef;

	public Sprite GlobalItemCellIcon => matterDef.icon;

	public string GlobalItemTitle => matterDef.matterName;

	public int GlobalItemCount => count;

	public bool JumpToStoreButton
	{
		get
		{
			if (!(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.commonCurrencyMatter) && !(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.essenceCurrencyMatter) && !(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter) && !(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.gachaTicketMatter) && !(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.limitGachaTicketMatter) && !(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter))
			{
				return matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter;
			}
			return true;
		}
	}

	public PurchaseGroupType JumpToStorePurchaseGroupType
	{
		get
		{
			MatterDef matterDef = this.matterDef;
			if (matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.gachaTicketMatter || matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.limitGachaTicketMatter)
			{
				return PurchaseGroupType.Stable;
			}
			if (matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter)
			{
				return PurchaseGroupType.SeasonShuffle;
			}
			if (matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter)
			{
				return PurchaseGroupType.WeekShuffle;
			}
			return PurchaseGroupType.RealMoneyBuy;
		}
	}

	public int JumpToStoreSubIndex
	{
		get
		{
			if (!(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter))
			{
				return -1;
			}
			return 3;
		}
	}

	string IGlobalItemDisplaySetup.JumpToStoreButtonText
	{
		get
		{
			if (!(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter) && !(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter))
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_global_item_jump_to_store;
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_global_item_jump_to_use;
		}
	}

	PlayerCharacterDef IGlobalItemDisplaySetup.GlobalItemRefPlayerCharacter => GetAttachedPlayerCharacterDef(Bootstrap.applicationState);

	QualityLevel IGlobalItemDisplaySetup.GlobalItemQuality => matterDef.qualityLevel;

	private MatterInfo()
	{
	}

	public MatterInfo(MatterDef matterDef, int count)
	{
		this.matterDef = matterDef;
		this.count = count;
	}

	public static MatterInfo operator *(MatterInfo matterInfo, int multiplier)
	{
		if (matterInfo == null)
		{
			return null;
		}
		return new MatterInfo(matterInfo.matterDef, matterInfo.count * multiplier);
	}

	public static MatterInfo operator *(int multiplier, MatterInfo matterInfo)
	{
		if (matterInfo == null)
		{
			return null;
		}
		return new MatterInfo(matterInfo.matterDef, matterInfo.count * multiplier);
	}

	public bool IsPayable(ApplicationStateInfo applicationState)
	{
		return applicationState.matterManager.GetMatterCount(matterDef) >= count;
	}

	public static List<MatterInfo> FoldMatterInfo(IEnumerable<MatterInfo> source)
	{
		List<MatterInfo> list = new List<MatterInfo>();
		foreach (MatterInfo matterInfo in source)
		{
			MatterInfo matterInfo2 = list.Find((MatterInfo i) => i.matterDef == matterInfo.matterDef);
			if (matterInfo2 == null)
			{
				list.Add(new MatterInfo(matterInfo.matterDef, matterInfo.count));
			}
			else
			{
				matterInfo2.count += matterInfo.count;
			}
		}
		return list;
	}

	public PlayerCharacterDef GetAttachedPlayerCharacterDef(ApplicationStateInfo applicationState)
	{
		return matterDef.GetAttachedPlayerCharacterDef(applicationState);
	}

	public PlayerCharacterState GetAttachedPlayerCharacterState(ApplicationStateInfo applicationState)
	{
		return matterDef.GetAttachedPlayerCharacterState(applicationState);
	}

	public string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return matterDef.description;
	}

	public int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return applicationState.matterManager.GetMatterCount(matterDef);
	}

	string IGlobalItemDisplaySetup.GetGlobalItemStorageCountText(ApplicationStateInfo applicationState)
	{
		int? globalItemStorageCount = GetGlobalItemStorageCount(applicationState);
		if (!globalItemStorageCount.HasValue)
		{
			return null;
		}
		if (matterDef.matterType == MatterDef.MatterType.ObsessionDebris)
		{
			int num = GetAttachedPlayerCharacterState(applicationState)?.currentObsessionDebrisRequiredCount ?? ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUpgradeCount;
			return $"{globalItemStorageCount}<color=#A7A7A7>/{num}</color>";
		}
		return $"{globalItemStorageCount}";
	}

	public MatterInfo GetUpgradeObsessionLevelInfo()
	{
		return matterDef.matterType switch
		{
			MatterDef.MatterType.ObsessionDebris => new MatterInfo(matterDef, ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUpgradeCount), 
			MatterDef.MatterType.CharacterClassObsession => new MatterInfo(matterDef, 1), 
			_ => null, 
		};
	}

	public bool IsValidate()
	{
		return matterDef != null;
	}
}
