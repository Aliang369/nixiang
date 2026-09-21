using System.Collections.Generic;
using UnityEngine;

public class MatterDef : CTScriptableObject, IAssetIcon
{
	public enum MatterType
	{
		CommonCurrency = 0,
		EssenceCurrency = 1,
		CreditCurrency = 2,
		StoreTokenCurrency = 3,
		SkillRecycleCurrency = 4,
		WeeklyTokenCurrency = 5,
		Obsession = 6,
		ObsessionDebris = 7,
		Loot = 8,
		CharacterClassObsession = 9,
		CommonObsession = 10,
		GachaTicket = 11,
		Soul = 12,
		ItemSkinToken = 13,
		LimitGachaTicket = 14,
		EnemyLootMatter = 10000,
		Other = 100000
	}

	public enum MatterDisplayGroup
	{
		Currency,
		CharacterMatter,
		Obsession,
		Chest
	}

	public static int obsessionMaxLevel = 6;

	[LocalizeText("货币/素材名字")]
	[CTSOAssetName]
	public string matterName;

	[LocalizeText("资源描述")]
	[TextArea]
	public string description;

	public Color matterTextColor;

	public Sprite icon;

	public MatterType matterType;

	public LootSetup lootSetup;

	public QualityLevel qualityLevel;

	public int lootMatterWeight;

	public override string CTAssetIDPrepend => "MT";

	public bool isCurrencyMatter => matterType switch
	{
		MatterType.CommonCurrency => true, 
		MatterType.EssenceCurrency => true, 
		MatterType.CreditCurrency => true, 
		MatterType.StoreTokenCurrency => true, 
		MatterType.SkillRecycleCurrency => true, 
		MatterType.WeeklyTokenCurrency => true, 
		MatterType.ItemSkinToken => true, 
		_ => false, 
	};

	public bool IsMatterTypeEnemyLoot => matterType == MatterType.EnemyLootMatter;

	public string NameWithColor => DisplayMatterName.ColorText(matterTextColor);

	public string DisplayMatterName
	{
		get
		{
			if (IsMatterTypeEnemyLoot)
			{
				return matterName + ScriptableObjectSingleton<TextDataPreset>.instance.term_loot_matter_append_text;
			}
			return matterName;
		}
	}

	public Sprite IconAsset => icon;

	public int GetCount(ApplicationStateInfo applicationState)
	{
		return applicationState.matterManager.GetMatterCount(this);
	}

	public PlayerCharacterDef GetAttachedPlayerCharacterDef(ApplicationStateInfo applicationState)
	{
		return GetAttachedPlayerCharacterState(applicationState)?.def;
	}

	public PlayerCharacterState GetAttachedPlayerCharacterState(ApplicationStateInfo applicationState)
	{
		return matterType switch
		{
			MatterType.Obsession => applicationState.characterManager.characterStates.Find((PlayerCharacterState c) => c.def.obsession == this), 
			MatterType.ObsessionDebris => applicationState.characterManager.characterStates.Find((PlayerCharacterState c) => c.def.obsessionDebris == this), 
			_ => null, 
		};
	}

	public List<GlobalRewardSetup> GetAllLootRewards()
	{
		if (lootSetup == null)
		{
			return null;
		}
		if (lootSetup is FixedLootSetup fixedLootSetup)
		{
			return fixedLootSetup.globalRewards;
		}
		if (lootSetup is PickLootSetup pickLootSetup)
		{
			return pickLootSetup.globalRewards;
		}
		return null;
	}

	public MatterDisplayGroup GetMatterGroup()
	{
		switch (matterType)
		{
		case MatterType.CommonCurrency:
		case MatterType.EssenceCurrency:
		case MatterType.CreditCurrency:
		case MatterType.StoreTokenCurrency:
		case MatterType.SkillRecycleCurrency:
		case MatterType.WeeklyTokenCurrency:
		case MatterType.GachaTicket:
		case MatterType.EnemyLootMatter:
			return MatterDisplayGroup.Currency;
		case MatterType.Obsession:
		case MatterType.ObsessionDebris:
		case MatterType.CharacterClassObsession:
		case MatterType.CommonObsession:
			return MatterDisplayGroup.Obsession;
		case MatterType.Loot:
			return MatterDisplayGroup.Chest;
		default:
			return MatterDisplayGroup.Currency;
		}
	}
}
