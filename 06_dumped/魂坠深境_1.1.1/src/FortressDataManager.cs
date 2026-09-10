using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FortressDataManager : ScriptableObjectSingleton<FortressDataManager>
{
	[Serializable]
	public class GachaHistorySharedGroup
	{
		public List<GachaPoolDef> groups;
	}

	public class SkillAddSlotMatterCost
	{
		public List<MatterInfo> matterInfos;

		public QualityLevel qualityLevel;
	}

	public class DailyGameExpBonusSetup
	{
		public enum BonusType
		{
			FinishGame,
			WinGame
		}

		public BonusType type;

		public Vector2Int range;

		public int addFixExp;

		public int addPercentExp;

		public bool IsValid(bool win, ApplicationStateInfo asi)
		{
			if (type == BonusType.FinishGame)
			{
				int finishedGameCountToday = asi.routineQuestManager.finishedGameCountToday;
				if (range.x <= finishedGameCountToday)
				{
					return finishedGameCountToday <= range.y;
				}
				return false;
			}
			if ((type == BonusType.WinGame) & win)
			{
				int winGameCountToday = asi.routineQuestManager.winGameCountToday;
				if (range.x <= winGameCountToday)
				{
					return winGameCountToday <= range.y;
				}
				return false;
			}
			return false;
		}
	}

	public class RoutineQuestPool
	{
		public string name;

		public List<RoutineQuestDef> quests;
	}

	[Serializable]
	public struct VipDebrisDropRange
	{
		public int min;

		public int max;
	}

	public int playerNickNameByteLimit;

	public MatterDef commonCurrencyMatter;

	public bool enablePVPMode;

	public string yuanQiActivityGiftCode;

	public GlobalRewardSetup monthlyCardRewardSetup;

	private MatterDef _skillWordRecycleMatter;

	private MatterDef _essenceCurrencyMatter;

	private MatterDef _creditCurrencyMatter;

	private MatterDef _storeTokenMatter;

	private MatterDef _weeklyTokenMatter;

	public MatterDef _gachaTicketMatter;

	public MatterDef _limitGachaTicketMatter;

	private MatterDef _itemSkinTokenMatter;

	public int weeklyTokenMatterUpperLimit;

	public int skillCurrencyMatterGotFromBoss;

	public List<MatterReward> initMatters;

	public int enemyMatterLootCountPerGame;

	public Sprite defaultGlobalRewardIcon;

	public int lootMattersPerGame;

	public List<StartGameBuffDef> initUnlockedStartGameBuff;

	public MatterReward questionnaireBonus;

	public int legacyShareRewardPerUpvote;

	public int vipExpBonusPercent;

	public LevelLineDef globalLevelLine;

	public GachaPoolDef baseGachaPoolDef;

	public List<GachaHistorySharedGroup> gachaHistorySharedGroups;

	public PlayerCharacterDef defaultCharacter;

	public int repeatCharacterCompensateToken;

	public int repeatCharacterObsessionDebrisCompensateCount;

	public int repeatItemSkinCompensateToken;

	public Dictionary<QualityLevel, int> gachaBaseCompensateTokenDic;

	public List<PurchasePromotion> presetPurchasePromotions;

	public int weeklyQuestSeasonExpLimit;

	public AddSlotMatrixData addSlotMatrixData;

	public List<SkillAddSlotMatterCost> skillAddSlotMatterCosts;

	public int highSeasonPassQuestExpBonusPercent;

	public int highSeasonPassWeeklyExpLimitAddPercent;

	public int highSeasonPassInstanceAddExpPercentToGotSumExp;

	public int characterSkillPresetCount;

	public int initCharacterSkillLimit;

	public int obsessionDebrisUnlockCount;

	public int obsessionDebrisUpgradeCount;

	public MatterDef _obsessionRecycleMatter;

	public int obsessionRecycleCount;

	public MatterDef obsessionDebrisRecycleMatter;

	public int obsessionDebrisRecycleCount;

	[FormerlySerializedAs("fortressRoomRef")]
	public List<RoomRef> commonRoomRefs;

	[FormerlySerializedAs("fortressRoomRef")]
	public List<RoomRef> fortressRoomRefs;

	public RoomRef defaultFortressRoomRef;

	public List<DailyGameExpBonusSetup> dailyGameExpBonusSetups;

	public ContractDef defaultContractDef;

	public ContractDef introContractDef;

	public int dailyRoutineQuestMaxCount;

	public int dailyRoutineQuestDailyGenerateCount;

	public List<RoutineQuestDef> fixedDailyQuests;

	public List<RoutineQuestPool> weeklyRoutineQuestPools;

	public RoutineQuestPool dailyRoutineQuestPool;

	public List<SkillDef> testInitSkills;

	[SerializeField]
	public Dictionary<QualityLevel, Dictionary<int, int>> SkillUpgradeQualityCommonMatterCostDic;

	[SerializeField]
	public Dictionary<QualityLevel, Dictionary<int, int>> SkillUpgradeQualityEssenceMatterCostDic;

	[SerializeField]
	public Dictionary<QualityLevel, Dictionary<int, int>> SkillUpgradeQualityLootMatterCostDic;

	[SerializeField]
	public Dictionary<QualityLevel, int> randomWordAttachSkillQualityCommonCostDic;

	public Dictionary<QualityLevel, int> randomWordAttachSkillQualityEssenceCostDic;

	public Dictionary<QualityLevel, int> randomWordAttachSkillQualityLootMatterCostDic;

	[FormerlySerializedAs("randomWordAttachWordToSkillQualityWeightDic")]
	public Dictionary<QualityLevel, Dictionary<QualityLevel, int>> randomWordAttachToQualitySkillWithQualityWordWeightDic;

	public Dictionary<QualityLevel, int> fixWordAttachWordQualityCostDic;

	public Dictionary<QualityLevel, int> deleteSkillGainRecycleMatterQualityDic;

	public Dictionary<QualityLevel, int> deleteSkillGainLootMatterQualityDic;

	public Dictionary<QualityLevel, int> skillBreakthroughMatterCount;

	public Dictionary<int, Dictionary<int, int>> gemCombineProbability;

	public Dictionary<int, int> gemCombineCost;

	public Color introAchievementColor;

	public Color progressAchievementColor;

	public Color specialAchievementColor;

	public List<IndexBookConfig> indexBookConfigs;

	public int winGameRankScore;

	public int rankScorePerDifficultyLevel;

	public int rankTimeScoreSum;

	public long rankTimeScoreMaxTimeTicks;

	public long rankTimeScoreMinTimeTicks;

	public int rankMaxPages;

	public int itemSkinMaxLevel;

	public Dictionary<QualityLevel, int> repeatItemSkinTokenCompensateDic;

	public int dailyStartGameBuffCount;

	public Dictionary<DifficultyLevel, Dictionary<int, int>> stageClassMatterConfig;

	public MatterInfo refreshDailyQuestMatter;

	public IdolSystemConfig idolSystemConfig;

	public Dictionary<DifficultyLevel, VipDebrisDropRange> vipDebrisDropNormalBoss;

	public Dictionary<DifficultyLevel, VipDebrisDropRange> vipDebrisDropFinalBoss;

	public int vipDebrisDailyDropLimit = 300;

	public MatterDef skillWordRecycleMatter
	{
		get
		{
			if (_skillWordRecycleMatter == null)
			{
				_skillWordRecycleMatter = TryGetMatterDef(MatterDef.MatterType.CommonCurrency);
			}
			return _skillWordRecycleMatter;
		}
	}

	public MatterDef essenceCurrencyMatter
	{
		get
		{
			if (_essenceCurrencyMatter == null)
			{
				_essenceCurrencyMatter = TryGetMatterDef(MatterDef.MatterType.EssenceCurrency);
			}
			return _essenceCurrencyMatter;
		}
	}

	public MatterDef creditCurrencyMatter
	{
		get
		{
			if (_creditCurrencyMatter == null)
			{
				_creditCurrencyMatter = TryGetMatterDef(MatterDef.MatterType.CreditCurrency);
			}
			return _creditCurrencyMatter;
		}
	}

	public MatterDef storeTokenMatter
	{
		get
		{
			if (_storeTokenMatter == null)
			{
				_storeTokenMatter = TryGetMatterDef(MatterDef.MatterType.StoreTokenCurrency);
			}
			return _storeTokenMatter;
		}
	}

	public MatterDef weeklyTokenMatter
	{
		get
		{
			if (_weeklyTokenMatter == null)
			{
				_weeklyTokenMatter = TryGetMatterDef(MatterDef.MatterType.WeeklyTokenCurrency);
			}
			return _weeklyTokenMatter;
		}
	}

	public MatterDef gachaTicketMatter
	{
		get
		{
			if (_gachaTicketMatter == null)
			{
				_gachaTicketMatter = TryGetMatterDef(MatterDef.MatterType.GachaTicket);
			}
			return _gachaTicketMatter;
		}
	}

	public MatterDef limitGachaTicketMatter
	{
		get
		{
			if (_limitGachaTicketMatter == null)
			{
				_limitGachaTicketMatter = TryGetMatterDef(MatterDef.MatterType.LimitGachaTicket);
			}
			return _limitGachaTicketMatter;
		}
	}

	public MatterDef itemSkinTokenMatter
	{
		get
		{
			if (_itemSkinTokenMatter == null)
			{
				_itemSkinTokenMatter = TryGetMatterDef(MatterDef.MatterType.ItemSkinToken);
			}
			return _itemSkinTokenMatter;
		}
	}

	public MatterDef ObsessionRecycleMatter
	{
		get
		{
			if (_obsessionRecycleMatter == null)
			{
				_obsessionRecycleMatter = TryGetMatterDef(MatterDef.MatterType.StoreTokenCurrency);
			}
			return _obsessionRecycleMatter;
		}
	}

	private MatterDef TryGetMatterDef(MatterDef.MatterType type)
	{
		try
		{
			return Bootstrap.applicationState.assetCatalog.matterDefCatalog.assets.Find((MatterDef m) => m.matterType == type);
		}
		catch
		{
			return null;
		}
	}
}
