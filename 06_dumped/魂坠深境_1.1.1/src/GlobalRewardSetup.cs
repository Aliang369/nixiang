using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class GlobalRewardSetup : IGlobalItemDisplaySetup
{
	protected TextDataPreset.GlobalItemStringConfig GlobalItemStringConfig => globalItemStringConfigs[GetType()];

	private Dictionary<Type, TextDataPreset.GlobalItemStringConfig> globalItemStringConfigs => ScriptableObjectSingleton<TextDataPreset>.instance.globalItemStringConfigs;

	public virtual Sprite GlobalItemCellIcon => ScriptableObjectSingleton<GameDataManager>.instance.itemPlaceHolderIcon;

	public virtual int GlobalItemCount => 1;

	public virtual string GlobalItemCellName => GlobalItemStringConfig.GlobalItemCellName;

	public virtual string GlobalItemTitle => GlobalItemStringConfig.GlobalItemTitle;

	public virtual QualityLevel GlobalItemQuality => GenerateRewardImpl(Bootstrap.applicationState).GlobalItemQuality;

	public virtual ProfileSkill GlobalItemRefSkill => null;

	public virtual ItemSkinDef GlobalItemRefItemSkin => null;

	public virtual ItemSkinSetDef GlobalItemSkinSetDef => null;

	public virtual Item GlobalItemRefItem => null;

	public virtual Rune GlobalItemRefRune => null;

	public virtual Influence globalItemRefInfluence => null;

	public virtual List<IGlobalItemDisplaySetup> RandomStartGameBuffs => null;

	public virtual List<IGlobalItemDisplaySetup> RandomSkillDefs => null;

	public virtual List<IGlobalItemDisplaySetup> RandomMatterDefs => null;

	public virtual PlayerCharacterDef GlobalItemRefPlayerCharacter => null;

	public virtual bool isNew => false;

	public virtual bool isHidden => false;

	public virtual bool JumpToGachaAnnouncement => false;

	public virtual bool JumpToStoreButton => false;

	public virtual string JumpToStoreButtonText => ScriptableObjectSingleton<TextDataPreset>.instance.term_global_item_jump_to_store;

	public virtual PurchaseGroupType JumpToStorePurchaseGroupType => PurchaseGroupType.RealMoneyBuy;

	public virtual int JumpToStoreSubIndex => -1;

	protected string FGlobalItemCellName(params object[] ps)
	{
		try
		{
			return string.Format(GlobalItemStringConfig.GlobalItemCellName, ps);
		}
		catch
		{
			return "";
		}
	}

	protected string FGlobalItemTitle(params object[] ps)
	{
		try
		{
			return string.Format(GlobalItemStringConfig.GlobalItemTitle, ps);
		}
		catch
		{
			return "";
		}
	}

	public virtual string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return GlobalItemStringConfig.GetGlobalItemDescription;
	}

	public virtual int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return null;
	}

	public GlobalRewardInfo GenerateReward(ApplicationStateInfo applicationState, GlobalRewardSourceType type, ICTAsset rewardSource)
	{
		return GenerateReward(applicationState, type, rewardSource.GetFullID());
	}

	public GlobalRewardInfo GenerateReward(ApplicationStateInfo applicationState, GlobalRewardSourceType sourceType, string rewardSourceId)
	{
		GlobalRewardInfo globalRewardInfo = GenerateRewardImpl(applicationState);
		globalRewardInfo.rewardSourceType = sourceType;
		globalRewardInfo.rewardSourceId = rewardSourceId;
		return globalRewardInfo;
	}

	protected abstract GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState);

	public virtual string GetRewardBrief()
	{
		return GlobalItemStringConfig.RewardBrief;
	}

	protected string FGetRewardBrief(params object[] ps)
	{
		return string.Format(GlobalItemStringConfig.RewardBrief, ps);
	}

	public virtual GlobalRewardSetup Copy()
	{
		return (GlobalRewardSetup)MemberwiseClone();
	}

	public static List<GlobalRewardSetup> CompressRewardsForDisplay(List<GlobalRewardSetup> rewards)
	{
		List<GlobalRewardSetup> list = new List<GlobalRewardSetup>();
		foreach (GlobalRewardSetup reward in rewards)
		{
			MatterReward matterReward = reward as MatterReward;
			if (matterReward != null)
			{
				GlobalRewardSetup globalRewardSetup = list.FirstOrDefault((GlobalRewardSetup r) => r is MatterReward matterReward2 && matterReward2.matterDef == matterReward.matterDef);
				if (globalRewardSetup != null)
				{
					(globalRewardSetup as MatterReward).amount += matterReward.amount;
					continue;
				}
			}
			if (reward is GachaGlobalReward gachaGlobalReward)
			{
				GlobalRewardSetup globalRewardSetup2 = list.FirstOrDefault((GlobalRewardSetup r) => r is GachaGlobalReward);
				if (globalRewardSetup2 != null)
				{
					(globalRewardSetup2 as GachaGlobalReward)._count += gachaGlobalReward._count;
					continue;
				}
			}
			if (reward is RandomLootMatterSetup randomLootMatterSetup)
			{
				GlobalRewardSetup globalRewardSetup3 = list.FirstOrDefault((GlobalRewardSetup r) => r is RandomLootMatterSetup);
				if (globalRewardSetup3 != null)
				{
					(globalRewardSetup3 as RandomLootMatterSetup).count += randomLootMatterSetup.count;
					continue;
				}
			}
			RandomItemSkinInSkinSetReward rsisr = reward as RandomItemSkinInSkinSetReward;
			if (rsisr != null)
			{
				GlobalRewardSetup globalRewardSetup4 = list.FirstOrDefault((GlobalRewardSetup r) => r is RandomItemSkinInSkinSetReward randomItemSkinInSkinSetReward && randomItemSkinInSkinSetReward.itemSkinSetDef == rsisr.itemSkinSetDef);
				if (globalRewardSetup4 != null)
				{
					(globalRewardSetup4 as RandomItemSkinInSkinSetReward).count += rsisr.count;
					continue;
				}
			}
			if (reward is RandomSkillReward randomSkillReward)
			{
				GlobalRewardSetup globalRewardSetup5 = list.FirstOrDefault((GlobalRewardSetup r) => r is RandomSkillReward);
				if (globalRewardSetup5 != null)
				{
					(globalRewardSetup5 as RandomSkillReward).count += randomSkillReward.count;
					continue;
				}
			}
			if (reward is RandomStartGameBuffReward randomStartGameBuffReward)
			{
				GlobalRewardSetup globalRewardSetup6 = list.FirstOrDefault((GlobalRewardSetup r) => r is RandomStartGameBuffReward);
				if (globalRewardSetup6 != null)
				{
					(globalRewardSetup6 as RandomStartGameBuffReward).count += randomStartGameBuffReward.count;
					continue;
				}
			}
			if (reward != null)
			{
				list.Add(reward.Copy());
			}
		}
		return list;
	}
}
