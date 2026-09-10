using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GlobalRewardInfo : IGlobalItemDisplaySetup
{
	public enum RewardNestedType
	{
		None,
		RandomChest,
		GachaDraw,
		UnlockHighSeason
	}

	public enum GlobalRewardType
	{
		Unknown = -1,
		MatterReward,
		SkillReward,
		UnlockCharacter,
		UnlockItemSkin,
		GetStartGameBuff,
		UnlockCTSO,
		UnlockSeasonPass,
		SetGlobalVariable,
		NestedReward,
		UnlockPermanentVIP,
		UnlockMonthlyCard,
		UnlockPVPEnemyAvatar,
		UnlockPVPDeckComp,
		ChangeLevelLineExp,
		UnlockHighSeasonPass,
		SkillGem,
		SoulSacrificeReward,
		ForgetShrinePoint
	}

	[SaveField(null, null)]
	public MatterInfo matter;

	[SaveField(null, null)]
	public ProfileSkill skill;

	[SaveField(null, null)]
	public ItemSkinDef unlockItemSkin;

	[SaveField(null, null)]
	public StartGameBuff getStartGameBuff;

	[SaveField(null, null)]
	public List<CTScriptableObject> unlockCTSOs;

	[SaveField(null, null)]
	public SeasonDef unlockSeasonPass;

	[SaveField(null, null)]
	public SeasonDef unlockHighSeasonPass;

	[SaveField(null, null)]
	public string setGlobalVariableFlag;

	[SaveField(null, null)]
	public bool unlockPermanentVIP;

	[SaveField(null, null)]
	public bool unlockMonthlyCard;

	[SaveField(null, null)]
	public DeckPresetCompDef unlockPVPDeckPresetComp;

	[SaveField(null, null)]
	public EnemyInfoData unlockPVPEnemyAvatar;

	[SaveField(null, null)]
	public LevelLine.ChangeExpResult changeLevelLineExpResult;

	[SaveField(null, null)]
	public RewardNestedType nestedType;

	[SaveField(null, null)]
	public List<GlobalRewardInfo> nestedRewards;

	[SaveField(null, null)]
	public int baseTokenCompensate;

	[SaveField(null, null)]
	public GlobalRewardSourceType rewardSourceType;

	[SaveField(null, null)]
	public string rewardSourceId;

	[SaveField(null, null)]
	public SkillGem skillGem;

	[SaveField(null, null)]
	public bool forgetShinePoint;

	[SaveField(null, null)]
	public CharacterClassDef characterClassDef;

	[SaveField(null, null)]
	public GlobalRewardGiveAppendInfo giveAppendInfo;

	public GachaPoolDef.GachaContentType gachaDisplayContentType;

	public CTScriptableObject gachaDisplayContent;

	public QualityLevel gachaDisplayQualityLevel = QualityLevel.None;

	public CTScriptableObject mainUnlockCTSOs
	{
		get
		{
			if (unlockCTSOs == null || unlockCTSOs.Count == 0 || unlockCTSOs[0] == null)
			{
				return null;
			}
			return unlockCTSOs[0];
		}
	}

	public int SumTokenCompensate => baseTokenCompensate + (giveAppendInfo?.repeatTokenCompensate ?? 0);

	public int SumItemSkinTokenCompensate => giveAppendInfo?.repeatItemSkinTokenCompensate ?? 0;

	private bool HasGachaDisplayContent
	{
		get
		{
			if (gachaDisplayContent != null)
			{
				return gachaDisplayContentType == GachaPoolDef.GachaContentType.Obsession;
			}
			return false;
		}
	}

	bool IGlobalItemDisplaySetup.isNew
	{
		get
		{
			if (giveAppendInfo == null)
			{
				giveAppendInfo = new GlobalRewardGiveAppendInfo
				{
					isNewGive = CheckIsNew()
				};
			}
			return giveAppendInfo.isNewGive;
		}
	}

	private ApplicationStateInfo applicationState => Bootstrap.instance.CurrentApplicationState;

	public bool isHidden
	{
		get
		{
			return RewardType switch
			{
				GlobalRewardType.UnlockCTSO => UnlockCTSOIsHidden(), 
				GlobalRewardType.SetGlobalVariable => true, 
				GlobalRewardType.UnlockPermanentVIP => true, 
				GlobalRewardType.ChangeLevelLineExp => true, 
				GlobalRewardType.UnlockSeasonPass => true, 
				GlobalRewardType.UnlockHighSeasonPass => true, 
				GlobalRewardType.UnlockMonthlyCard => true, 
				_ => false, 
			};
			bool UnlockCTSOIsHidden()
			{
				if (mainUnlockCTSOs == null)
				{
					return true;
				}
				if (mainUnlockCTSOs is ItemDefinition)
				{
					return false;
				}
				if (mainUnlockCTSOs is RuneDef)
				{
					return false;
				}
				if (mainUnlockCTSOs is InfluenceDef)
				{
					return false;
				}
				return true;
			}
		}
	}

	public MatterInfo compensate => new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter, SumTokenCompensate);

	public MatterInfo itemSkinTokenCompensate => new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.itemSkinTokenMatter, SumItemSkinTokenCompensate);

	public MatterInfo soul
	{
		get
		{
			if (refCharacter != null)
			{
				return new MatterInfo(refCharacter.attachClass.soulMatter, giveAppendInfo.soul);
			}
			return null;
		}
	}

	public QualityLevel GlobalItemQuality
	{
		get
		{
			if (HasGachaDisplayContent && gachaDisplayQualityLevel != QualityLevel.None)
			{
				return gachaDisplayQualityLevel;
			}
			if (skill != null)
			{
				return skill.QualityLevel;
			}
			if (matter != null)
			{
				MatterDef.MatterType matterType = matter.matterDef.matterType;
				if (matterType == MatterDef.MatterType.Obsession || matterType == MatterDef.MatterType.CharacterClassObsession)
				{
					return QualityLevel.Epic;
				}
				if (matter.matterDef.matterType == MatterDef.MatterType.ObsessionDebris)
				{
					return QualityLevel.Uncommon;
				}
				return ((IGlobalItemDisplaySetup)matter).GlobalItemQuality;
			}
			if (unlockItemSkin != null)
			{
				return unlockItemSkin.quality;
			}
			if (getStartGameBuff != null)
			{
				return getStartGameBuff.def.quality;
			}
			if (skillGem != null)
			{
				return SkillGemDef.GetLevelQuality(skillGem.def.level);
			}
			if (forgetShinePoint)
			{
				return QualityLevel.Rare;
			}
			return QualityLevel.None;
		}
	}

	public GlobalRewardType RewardType
	{
		get
		{
			if (matter != null)
			{
				return GlobalRewardType.MatterReward;
			}
			if (skill != null)
			{
				return GlobalRewardType.SkillReward;
			}
			if (unlockItemSkin != null)
			{
				return GlobalRewardType.UnlockItemSkin;
			}
			if (getStartGameBuff != null)
			{
				return GlobalRewardType.GetStartGameBuff;
			}
			if (unlockCTSOs != null)
			{
				return GlobalRewardType.UnlockCTSO;
			}
			if (unlockSeasonPass != null)
			{
				return GlobalRewardType.UnlockSeasonPass;
			}
			if (unlockHighSeasonPass != null)
			{
				return GlobalRewardType.UnlockHighSeasonPass;
			}
			if (!setGlobalVariableFlag.IsNullOrEmpty())
			{
				return GlobalRewardType.SetGlobalVariable;
			}
			if (unlockPermanentVIP)
			{
				return GlobalRewardType.UnlockPermanentVIP;
			}
			if (unlockMonthlyCard)
			{
				return GlobalRewardType.UnlockMonthlyCard;
			}
			if (unlockPVPEnemyAvatar != null)
			{
				return GlobalRewardType.UnlockPVPEnemyAvatar;
			}
			if (unlockPVPDeckPresetComp != null)
			{
				return GlobalRewardType.UnlockPVPDeckComp;
			}
			if (changeLevelLineExpResult != null)
			{
				return GlobalRewardType.ChangeLevelLineExp;
			}
			if (nestedRewards != null)
			{
				return GlobalRewardType.NestedReward;
			}
			if (skillGem != null)
			{
				return GlobalRewardType.SkillGem;
			}
			if (forgetShinePoint)
			{
				return GlobalRewardType.ForgetShrinePoint;
			}
			return GlobalRewardType.Unknown;
		}
	}

	Sprite IGlobalItemDisplaySetup.GlobalItemCellIcon => GetGlobalItemCellIcon();

	int IGlobalItemDisplaySetup.GlobalItemCount
	{
		get
		{
			if (HasGachaDisplayContent)
			{
				return 1;
			}
			if (matter != null)
			{
				return matter.count;
			}
			if (skillGem != null)
			{
				return skillGem.amount;
			}
			return 1;
		}
	}

	string IGlobalItemDisplaySetup.GlobalItemCellName
	{
		get
		{
			if (HasGachaDisplayContent)
			{
				return GetGachaDisplayName();
			}
			if (skill != null)
			{
				return skill.Name;
			}
			if (unlockItemSkin != null)
			{
				return stringConfig.GlobalItemTitle.FormatWith(unlockItemSkin.attachItem.itemName, unlockItemSkin.attachSkinSet.skinSetName);
			}
			if (matter != null)
			{
				return matter.matterDef.matterName;
			}
			if (getStartGameBuff != null)
			{
				return getStartGameBuff.def.buffName;
			}
			if (unlockSeasonPass != null)
			{
				return stringConfig.GlobalItemCellName;
			}
			if (unlockPVPDeckPresetComp != null)
			{
				return stringConfig.GlobalItemTitle.FormatWith(unlockPVPDeckPresetComp.DisplayName);
			}
			if (unlockPVPEnemyAvatar != null)
			{
				return stringConfig.GlobalItemTitle.FormatWith(unlockPVPEnemyAvatar.name);
			}
			if (skillGem != null)
			{
				return skillGem.def.skillGemName;
			}
			if (forgetShinePoint)
			{
				return ForgetShrinePointReward.GetName(giveAppendInfo?.appendCharacterClass);
			}
			if (stringConfig != null)
			{
				return stringConfig.GlobalItemTitle;
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_global_reward_other;
		}
	}

	string IGlobalItemDisplaySetup.GlobalItemTitle
	{
		get
		{
			if (HasGachaDisplayContent)
			{
				return GetGachaDisplayName();
			}
			if (matter != null)
			{
				return matter.matterDef.matterName;
			}
			if (unlockItemSkin != null)
			{
				return stringConfig.GlobalItemTitle.FormatWith(unlockItemSkin.attachItem.itemName, unlockItemSkin.attachSkinSet.skinSetName);
			}
			if (getStartGameBuff != null)
			{
				return getStartGameBuff.def.buffName;
			}
			if (unlockSeasonPass != null)
			{
				return stringConfig.GlobalItemTitle.FormatWith(unlockSeasonPass.seasonName);
			}
			if (unlockPVPDeckPresetComp != null)
			{
				return stringConfig.GlobalItemTitle.FormatWith(unlockPVPDeckPresetComp.DisplayName);
			}
			if (unlockPVPEnemyAvatar != null)
			{
				return stringConfig.GlobalItemTitle.FormatWith(unlockPVPEnemyAvatar.name);
			}
			if (skillGem != null)
			{
				return skillGem.def.skillGemName;
			}
			if (forgetShinePoint)
			{
				return ForgetShrinePointReward.GetRewardTitle(giveAppendInfo?.appendCharacterClass);
			}
			if (stringConfig != null)
			{
				return stringConfig.GlobalItemTitle;
			}
			if (mainUnlockCTSOs != null)
			{
				if (mainUnlockCTSOs is ItemDefinition itemDefinition)
				{
					return stringConfig.GlobalItemTitle.FormatWith(itemDefinition.itemName);
				}
				if (mainUnlockCTSOs is RuneDef runeDef)
				{
					return stringConfig.GlobalItemTitle.FormatWith(runeDef.Name);
				}
				if (mainUnlockCTSOs is InfluenceDef influenceDef)
				{
					return stringConfig.GlobalItemTitle.FormatWith(influenceDef.InfluenceName);
				}
			}
			return "";
		}
	}

	protected TextDataPreset.GlobalItemStringConfig stringConfig
	{
		get
		{
			try
			{
				return globalItemStringConfigs[GetCorrespondRewardSetupType()];
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return null;
			}
		}
	}

	private Dictionary<Type, TextDataPreset.GlobalItemStringConfig> globalItemStringConfigs => ScriptableObjectSingleton<TextDataPreset>.instance.globalItemStringConfigs;

	ProfileSkill IGlobalItemDisplaySetup.GlobalItemRefSkill => skill;

	SkillGem IGlobalItemDisplaySetup.GlobalSkillGem => skillGem;

	Item IGlobalItemDisplaySetup.GlobalItemRefItem
	{
		get
		{
			if (unlockCTSOs != null && unlockCTSOs.Count > 0)
			{
				ItemDefinition itemDefinition = unlockCTSOs.Find((CTScriptableObject u) => u is ItemDefinition) as ItemDefinition;
				if (itemDefinition != null)
				{
					return new Item(itemDefinition);
				}
			}
			return null;
		}
	}

	Rune IGlobalItemDisplaySetup.GlobalItemRefRune
	{
		get
		{
			if (unlockCTSOs != null && unlockCTSOs.Count > 0)
			{
				RuneDef runeDef = unlockCTSOs.Find((CTScriptableObject u) => u is RuneDef) as RuneDef;
				if (runeDef != null)
				{
					return new Rune(runeDef);
				}
			}
			return null;
		}
	}

	string IGlobalItemDisplaySetup.JumpToStoreButtonText
	{
		get
		{
			if (!(matter?.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter) && !(matter?.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter))
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_global_item_jump_to_store;
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_global_item_jump_to_use;
		}
	}

	ItemSkinDef IGlobalItemDisplaySetup.GlobalItemRefItemSkin => unlockItemSkin;

	PlayerCharacterDef IGlobalItemDisplaySetup.GlobalItemRefPlayerCharacter
	{
		get
		{
			if (HasGachaDisplayContent && gachaDisplayContent is PlayerCharacterDef result)
			{
				return result;
			}
			MatterDef.MatterType? matterType = matter?.matterDef.matterType;
			bool flag;
			if (matterType.HasValue)
			{
				MatterDef.MatterType valueOrDefault = matterType.GetValueOrDefault();
				if ((uint)(valueOrDefault - 6) <= 1u)
				{
					flag = true;
					goto IL_005c;
				}
			}
			flag = false;
			goto IL_005c;
			IL_005c:
			if (flag)
			{
				return matter.GetAttachedPlayerCharacterDef(Bootstrap.applicationState);
			}
			return null;
		}
	}

	public PlayerCharacterDef refCharacter => ((IGlobalItemDisplaySetup)this).GlobalItemRefPlayerCharacter;

	public bool ShowGachaCharacterObsessionDisplay
	{
		get
		{
			if (!HasGachaDisplayContent || gachaDisplayContentType != GachaPoolDef.GachaContentType.Obsession)
			{
				MatterInfo matterInfo = matter;
				if (matterInfo == null)
				{
					return false;
				}
				return matterInfo.matterDef?.matterType == MatterDef.MatterType.Obsession;
			}
			return true;
		}
	}

	public string RewardTargetId => RewardType switch
	{
		GlobalRewardType.MatterReward => matter.matterDef.FullID, 
		GlobalRewardType.SkillReward => skill.def.FullID, 
		GlobalRewardType.UnlockCharacter => refCharacter.FullID, 
		GlobalRewardType.UnlockItemSkin => unlockItemSkin.FullID, 
		GlobalRewardType.GetStartGameBuff => getStartGameBuff.def.FullID, 
		GlobalRewardType.UnlockSeasonPass => unlockSeasonPass.FullID, 
		GlobalRewardType.UnlockHighSeasonPass => unlockHighSeasonPass.FullID, 
		GlobalRewardType.SetGlobalVariable => setGlobalVariableFlag, 
		GlobalRewardType.UnlockPVPDeckComp => unlockPVPDeckPresetComp.TargetAssetID, 
		GlobalRewardType.UnlockPVPEnemyAvatar => unlockPVPEnemyAvatar.FullId, 
		GlobalRewardType.SkillGem => skillGem.def.FullID, 
		_ => "", 
	};

	public string gachaTypeString
	{
		get
		{
			if (RewardType == GlobalRewardType.SkillReward)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill;
			}
			if (RewardType == GlobalRewardType.UnlockCharacter)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession;
			}
			if (RewardType == GlobalRewardType.UnlockItemSkin)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin;
			}
			if (RewardType == GlobalRewardType.MatterReward)
			{
				if (HasGachaDisplayContent && gachaDisplayContentType == GachaPoolDef.GachaContentType.Obsession)
				{
					return ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession;
				}
				MatterInfo matterInfo = matter;
				if (matterInfo != null && matterInfo.matterDef?.matterType == MatterDef.MatterType.ObsessionDebris)
				{
					return FormatObsessionDebrisWithCount(matter.count);
				}
				MatterInfo matterInfo2 = matter;
				if (matterInfo2 != null && matterInfo2.matterDef?.matterType == MatterDef.MatterType.Obsession)
				{
					return ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession;
				}
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_matter;
			}
			return "";
		}
	}

	private Sprite GetGlobalItemCellIcon()
	{
		if (HasGachaDisplayContent && gachaDisplayContent is PlayerCharacterDef characterDef)
		{
			return GetGachaDisplayCharacterIcon(characterDef);
		}
		if (HasGachaDisplayContent && gachaDisplayContent is IAssetIcon assetIcon)
		{
			return assetIcon.IconAsset;
		}
		if (skill != null)
		{
			return skill.Icon;
		}
		if (unlockItemSkin != null)
		{
			return unlockItemSkin.globalItemIcon;
		}
		if (matter != null)
		{
			return matter.matterDef.icon;
		}
		if (getStartGameBuff != null)
		{
			return getStartGameBuff.def.icon;
		}
		if (unlockPVPDeckPresetComp != null)
		{
			return unlockPVPDeckPresetComp.DisplayIcon;
		}
		if (unlockPVPEnemyAvatar != null)
		{
			return unlockPVPEnemyAvatar.thumbnail;
		}
		if (mainUnlockCTSOs != null && mainUnlockCTSOs is IAssetIcon assetIcon2)
		{
			return assetIcon2.IconAsset;
		}
		if (skillGem != null)
		{
			return skillGem.def.GemSprite;
		}
		if (forgetShinePoint)
		{
			return ForgetShrinePointReward.GetIcon(characterClassDef ?? giveAppendInfo?.appendCharacterClass);
		}
		return null;
	}

	private Sprite GetGachaDisplayCharacterIcon(PlayerCharacterDef characterDef)
	{
		if (gachaDisplayContentType == GachaPoolDef.GachaContentType.Obsession)
		{
			if (characterDef.obsession?.icon != null)
			{
				return characterDef.obsession.icon;
			}
			if (characterDef.globalItemIcon != null)
			{
				return characterDef.globalItemIcon;
			}
		}
		return characterDef.IconAsset;
	}

	string IGlobalItemDisplaySetup.GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		if (matter != null)
		{
			return matter.matterDef.description;
		}
		if (unlockItemSkin != null)
		{
			return stringConfig.GetGlobalItemDescription.FormatWith(unlockItemSkin.attachItem.itemName, unlockItemSkin.attachSkinSet.skinSetName, unlockItemSkin.attachSkinSet.description);
		}
		if (unlockSeasonPass != null)
		{
			return stringConfig.GetGlobalItemDescription.FormatWith(unlockSeasonPass.seasonName, unlockSeasonPass.description);
		}
		if (getStartGameBuff != null)
		{
			return TextDataPreset.FormatStartGameBuffDescription(getStartGameBuff, applicationState, withHintAndExpireData: true, out generator);
		}
		if (unlockPVPDeckPresetComp != null)
		{
			return stringConfig.GetGlobalItemDescription.FormatWith(unlockPVPDeckPresetComp.DisplayName);
		}
		if (unlockPVPEnemyAvatar != null)
		{
			return stringConfig.GetGlobalItemDescription.FormatWith(unlockPVPEnemyAvatar.name);
		}
		if (skillGem != null)
		{
			return skillGem.def.GetDescription(applicationState, null);
		}
		if (forgetShinePoint)
		{
			return ForgetShrinePointReward.GetDescription(giveAppendInfo?.appendCharacterClass);
		}
		if (stringConfig != null)
		{
			return stringConfig.GetGlobalItemDescription;
		}
		return "";
	}

	private Type GetCorrespondRewardSetupType()
	{
		return RewardType switch
		{
			GlobalRewardType.MatterReward => typeof(MatterReward), 
			GlobalRewardType.SkillReward => typeof(SkillReward), 
			GlobalRewardType.UnlockCharacter => typeof(UnlockCharacter), 
			GlobalRewardType.UnlockItemSkin => typeof(UnlockItemSkin), 
			GlobalRewardType.GetStartGameBuff => typeof(StartGameBuffReward), 
			GlobalRewardType.UnlockCTSO => typeof(UnlockCTSOGlobalReward), 
			GlobalRewardType.UnlockSeasonPass => typeof(UnlockSeasonPassSetup), 
			GlobalRewardType.SetGlobalVariable => typeof(SetGlobalVariableGlobalReward), 
			GlobalRewardType.NestedReward => typeof(GachaGlobalReward), 
			GlobalRewardType.UnlockPermanentVIP => typeof(UnlockPermanentVIPSetup), 
			GlobalRewardType.UnlockMonthlyCard => typeof(UnlockMonthlyCardSetup), 
			GlobalRewardType.UnlockPVPDeckComp => typeof(UnlockPVPCompReward), 
			GlobalRewardType.UnlockPVPEnemyAvatar => typeof(UnlockPVPEnemyAvatarReward), 
			GlobalRewardType.SkillGem => typeof(SkillGemReward), 
			GlobalRewardType.ForgetShrinePoint => typeof(ForgetShrinePointReward), 
			_ => typeof(MatterReward), 
		};
	}

	int? IGlobalItemDisplaySetup.GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		if (matter != null)
		{
			return applicationState.matterManager.GetMatterCount(matter.matterDef);
		}
		return null;
	}

	public void SetupGachaDisplayContent(GachaPoolDef.GachaContentType contentType, CTScriptableObject content, QualityLevel qualityLevel)
	{
		gachaDisplayContentType = contentType;
		gachaDisplayContent = content;
		gachaDisplayQualityLevel = qualityLevel;
	}

	private string GetGachaDisplayName()
	{
		if (gachaDisplayContent is PlayerCharacterDef playerCharacterDef)
		{
			if (gachaDisplayContentType == GachaPoolDef.GachaContentType.Obsession)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession_of_character_arg0.FormatWith(playerCharacterDef.characterName);
			}
			return playerCharacterDef.characterName;
		}
		if (gachaDisplayContent is SkillDef skillDef)
		{
			return skillDef.skillName;
		}
		if (gachaDisplayContent is IGlobalItemDisplaySetup globalItemDisplaySetup)
		{
			return globalItemDisplaySetup.GlobalItemCellName;
		}
		if (!(gachaDisplayContent != null))
		{
			return "";
		}
		return gachaDisplayContent.name;
	}

	public string GenerateUnlockCTSODescription()
	{
		if (unlockCTSOs == null)
		{
			TraceLog.Error("unlockCTSOs为空");
			return "";
		}
		return UnlockCTSOGlobalReward.GetUnlockCTSODetail(unlockCTSOs);
	}

	public GlobalRewardInfo()
	{
	}

	public GlobalRewardInfo(RewardNestedType nestedType, IEnumerable<GlobalRewardInfo> nestedRewards)
	{
		this.nestedType = nestedType;
		this.nestedRewards = nestedRewards.ToList();
	}

	public void Give(ApplicationStateInfo applicationState)
	{
		if (giveAppendInfo == null)
		{
			giveAppendInfo = new GlobalRewardGiveAppendInfo
			{
				isNewGive = CheckIsNew()
			};
		}
		TrySetupConvertObsessionToUnlockCharacterAppendInfo(applicationState);
		if (matter != null)
		{
			TrySetupRepeatedObsessionConvertToDebrisAppendInfo(applicationState);
			if (matter.matterDef.matterType == MatterDef.MatterType.Obsession && giveAppendInfo.convertObsessionToUnlockCharacter)
			{
				PlayerCharacterState attachedPlayerCharacterState = matter.matterDef.GetAttachedPlayerCharacterState(applicationState);
				if (giveAppendInfo.isObsessionDebris)
				{
					attachedPlayerCharacterState.Unlock("执念碎片");
				}
				else
				{
					attachedPlayerCharacterState.Unlock("执念");
				}
			}
			if (matter.count > 0)
			{
				applicationState.matterManager.AddMatter(matter.matterDef, matter.count, rewardSourceType.ToString());
			}
		}
		if (skill != null)
		{
			applicationState.globalSkillManager.AddProfileSkill(skill);
		}
		if (unlockItemSkin != null)
		{
			if (applicationState.itemSkinManager.IsItemSkinUnlocked(unlockItemSkin) || giveAppendInfo.repeated)
			{
				giveAppendInfo.repeated = true;
				giveAppendInfo.isNewGive = false;
				giveAppendInfo.repeatTokenCompensate = 0;
				giveAppendInfo.repeatItemSkinTokenCompensate = GetRepeatedItemSkinTokenCompensate();
			}
			else
			{
				applicationState.itemSkinManager.AddItemSkin(unlockItemSkin);
			}
		}
		if (getStartGameBuff != null)
		{
			applicationState.startGameBuffManager.AddStartGameBuff(getStartGameBuff);
		}
		if (unlockCTSOs != null)
		{
			foreach (CTScriptableObject unlockCTSO in unlockCTSOs)
			{
				applicationState.globalUnlockManager.UnlockResource(unlockCTSO);
				applicationState.discoverManager.Discover(unlockCTSO);
			}
		}
		if (unlockPermanentVIP)
		{
			bool vip = applicationState.playerInfoManager.vip;
			applicationState.playerInfoManager.vip = true;
			if (!vip)
			{
				string s = ScriptableObjectSingleton<TextDataPreset>.instance.term_new_unlock_arg0.FormatWith(ScriptableObjectSingleton<TextDataPreset>.instance.term_PermanentVIPPurchase);
				(applicationState.runningGameState?.UIManager?.quickHintPanel ?? applicationState.fortressState?.uiManager?.quickHintPanel)?.HintText(s);
			}
		}
		if (unlockMonthlyCard)
		{
			int dailyPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(DateTime.UtcNow);
			int num = Mathf.Clamp(applicationState.playerInfoManager.monthlyCardExpireDayPeriod - dailyPeriod, 0, int.MaxValue);
			applicationState.playerInfoManager.monthlyCardExpireDayPeriod = dailyPeriod + num + 30;
		}
		if (!setGlobalVariableFlag.IsNullOrEmpty())
		{
			applicationState.globalVariableManager.SetVariableValue(setGlobalVariableFlag, 1);
		}
		if (unlockSeasonPass != null)
		{
			applicationState.seasonManager.currentSeason.UnlockSeasonPass(applicationState);
		}
		if (unlockHighSeasonPass != null)
		{
			applicationState.seasonManager.currentSeason.UnlockHighSeasonPass(applicationState);
		}
		if (nestedRewards != null)
		{
			nestedRewards.ForEach(delegate(GlobalRewardInfo r)
			{
				r.Give(applicationState);
			});
		}
		if (SumTokenCompensate > 0)
		{
			applicationState.matterManager.AddMatter(ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter, SumTokenCompensate, "奖励补偿");
		}
		if (SumItemSkinTokenCompensate > 0)
		{
			applicationState.matterManager.AddMatter(ScriptableObjectSingleton<FortressDataManager>.instance.itemSkinTokenMatter, SumItemSkinTokenCompensate, "重复卡牌形态补偿");
		}
		if (unlockPVPEnemyAvatar != null)
		{
			applicationState.pvpModeManager.UnlockEnemyAvatar(unlockPVPEnemyAvatar);
		}
		if (unlockPVPDeckPresetComp != null)
		{
			applicationState.pvpModeManager.AddPVPDeckPresetComp(unlockPVPDeckPresetComp);
		}
		if (changeLevelLineExpResult != null)
		{
			changeLevelLineExpResult.Excute(applicationState);
		}
		if (skillGem != null)
		{
			applicationState.skillGemManager.AddGem(skillGem, skillGem.amount);
		}
		ThinkingAnalyticsExtends.TrackGetGlobalRewardInfo(applicationState, this);
		applicationState.eventManager.ReceiveRewardEvent.TriggerEvent(this);
	}

	private int GetRepeatedItemSkinTokenCompensate()
	{
		Dictionary<QualityLevel, int> repeatItemSkinTokenCompensateDic = ScriptableObjectSingleton<FortressDataManager>.instance.repeatItemSkinTokenCompensateDic;
		if (repeatItemSkinTokenCompensateDic != null && repeatItemSkinTokenCompensateDic.TryGetValue(unlockItemSkin.quality, out var value))
		{
			return value;
		}
		TraceLog.Error($"卡牌形态品质{unlockItemSkin.quality}未配置异界尘埃补偿");
		return 0;
	}

	private void TrySetupConvertObsessionToUnlockCharacterAppendInfo(ApplicationStateInfo applicationState)
	{
		if (matter == null || matter.matterDef.matterType != MatterDef.MatterType.Obsession)
		{
			return;
		}
		PlayerCharacterState attachedPlayerCharacterState = matter.matterDef.GetAttachedPlayerCharacterState(applicationState);
		if (giveAppendInfo.convertObsessionToUnlockCharacter)
		{
			giveAppendInfo.convertObsessionToUnlockCharacter = true;
			if (!HasGachaDisplayContent)
			{
				SetupGachaDisplayContent(GachaPoolDef.GachaContentType.Obsession, attachedPlayerCharacterState.def, QualityLevel.Epic);
			}
			if (giveAppendInfo.soul <= 0)
			{
				giveAppendInfo.soul = ScriptableObjectSingleton<FortressDataManager>.instance.idolSystemConfig.newCharacterReward;
			}
			if (matter.count > 0)
			{
				matter.count--;
			}
		}
	}

	private void TrySetupRepeatedObsessionConvertToDebrisAppendInfo(ApplicationStateInfo applicationState)
	{
		if (GlobalConfigManager.instance.enableNetwork || matter == null || matter.matterDef.matterType != MatterDef.MatterType.Obsession || giveAppendInfo.convertObsessionToUnlockCharacter)
		{
			return;
		}
		PlayerCharacterState attachedPlayerCharacterState = matter.matterDef.GetAttachedPlayerCharacterState(applicationState);
		if (attachedPlayerCharacterState != null && !attachedPlayerCharacterState.locked)
		{
			int repeatCharacterObsessionDebrisCompensateCount = ScriptableObjectSingleton<FortressDataManager>.instance.repeatCharacterObsessionDebrisCompensateCount;
			if (repeatCharacterObsessionDebrisCompensateCount > 0)
			{
				giveAppendInfo.repeated = true;
				giveAppendInfo.isNewGive = false;
				giveAppendInfo.repeatTokenCompensate = ScriptableObjectSingleton<FortressDataManager>.instance.repeatCharacterCompensateToken;
				giveAppendInfo.convertObsessionToDebris = true;
				SetupGachaDisplayContent(GachaPoolDef.GachaContentType.Obsession, attachedPlayerCharacterState.def, QualityLevel.Epic);
				matter = new MatterInfo(attachedPlayerCharacterState.def.obsessionDebris, repeatCharacterObsessionDebrisCompensateCount);
			}
		}
	}

	public static List<GlobalRewardInfo> UnfoldNestedReward(List<GlobalRewardInfo> source)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		foreach (GlobalRewardInfo item in source)
		{
			if (item.nestedRewards.IsNullOrEmpty())
			{
				list.Add(item);
			}
			else
			{
				list.AddRange(item.nestedRewards);
			}
		}
		return list;
	}

	public static List<GlobalRewardInfo> CompressRewardsForDisplay(List<GlobalRewardInfo> source)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		foreach (GlobalRewardInfo reward in source)
		{
			if (reward.RewardType == GlobalRewardType.MatterReward && !reward.HasGachaDisplayContent)
			{
				GlobalRewardInfo globalRewardInfo = list.Find((GlobalRewardInfo r) => r.matter != null && !r.HasGachaDisplayContent && r.matter.matterDef == reward.matter.matterDef);
				if (globalRewardInfo != null)
				{
					globalRewardInfo.matter.count += reward.matter.count;
					continue;
				}
			}
			list.Add(reward);
		}
		return list;
	}

	private static string FormatObsessionDebrisWithCount(int count)
	{
		string term_obsessionDebris = ScriptableObjectSingleton<TextDataPreset>.instance.term_obsessionDebris;
		if (term_obsessionDebris.Contains("{0}"))
		{
			return term_obsessionDebris.FormatWith(count);
		}
		return TextDisplayHelper.FormatNameWithCount(term_obsessionDebris, count);
	}

	public bool CheckIsNew()
	{
		return RewardType switch
		{
			GlobalRewardType.MatterReward => IsNew(matter.matterDef), 
			GlobalRewardType.SkillReward => IsNew(skill.def), 
			GlobalRewardType.UnlockItemSkin => IsNew(unlockItemSkin), 
			GlobalRewardType.GetStartGameBuff => IsNew(getStartGameBuff.def), 
			_ => false, 
		};
		bool IsNew(ICTAsset asset)
		{
			List<ICTAsset> gotObject = applicationState.globalUnlockManager.gotObject;
			if (gotObject == null)
			{
				return false;
			}
			return !gotObject.Contains(asset);
		}
	}

	public override string ToString()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(matter);
			stringBuilder.Append(skill);
			stringBuilder.Append(refCharacter);
			stringBuilder.Append(unlockItemSkin);
			stringBuilder.Append(getStartGameBuff);
			stringBuilder.Append(unlockCTSOs);
			stringBuilder.Append(unlockSeasonPass);
			stringBuilder.Append(setGlobalVariableFlag);
			stringBuilder.Append(unlockPermanentVIP);
			stringBuilder.Append(unlockMonthlyCard);
			stringBuilder.Append(nestedType);
			stringBuilder.Append(nestedRewards);
			stringBuilder.Append(baseTokenCompensate);
			stringBuilder.Append(rewardSourceType);
			stringBuilder.Append(rewardSourceId);
			stringBuilder.Append(giveAppendInfo);
			return stringBuilder.ToString();
		}
		catch
		{
			return "";
		}
	}
}
