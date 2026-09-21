using UnityEngine;

public class MatterReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public int amount;

	[SaveField(null, null)]
	public MatterDef matterDef;

	public override Sprite GlobalItemCellIcon => matterDef.icon;

	public override string GlobalItemTitle => matterDef.matterName;

	public override int GlobalItemCount => amount;

	public override bool JumpToStoreButton
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

	public override PurchaseGroupType JumpToStorePurchaseGroupType
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

	public override int JumpToStoreSubIndex
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

	public override string JumpToStoreButtonText
	{
		get
		{
			if (!(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter) && !(matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter))
			{
				return base.JumpToStoreButtonText;
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_global_item_jump_to_use;
		}
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return matterDef.description;
	}

	public override int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return applicationState.matterManager.GetMatterCount(matterDef);
	}

	private MatterReward()
	{
	}

	public MatterReward(MatterDef matterDef, int amount)
	{
		this.matterDef = matterDef;
		this.amount = amount;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			matter = new MatterInfo(matterDef, amount)
		};
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(amount, matterDef?.matterName);
	}
}
