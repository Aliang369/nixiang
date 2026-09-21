using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerationManager : ScriptableObjectSingleton<DungeonGenerationManager>
{
	[Serializable]
	public class RuneRarityWeights
	{
		public int common = 50;

		public int uncommon = 35;

		public int rare = 15;
	}

	[Serializable]
	public class CardRewardRarityWeights
	{
		public int common;

		public int uncommon;

		public int rare;

		public int Sum => common + uncommon + rare;

		public CardRewardRarityWeights()
		{
		}

		public CardRewardRarityWeights(int common, int uncommon, int rare)
		{
			this.common = common;
			this.uncommon = uncommon;
			this.rare = rare;
		}

		public CardRewardRarityWeights ApplyRareOffset(int rareOffset)
		{
			return new CardRewardRarityWeights(Mathf.Max(0, common - rareOffset), uncommon, Mathf.Max(0, rare + rareOffset));
		}
	}

	[Serializable]
	public class CardRewardRarityTierConfig
	{
		public CardRewardRarityWeights normal;

		public CardRewardRarityWeights elite;

		public int rareOffsetInitial;

		public int rareOffsetCommonIncrease;

		public CardRewardRarityTierConfig()
		{
		}

		public CardRewardRarityTierConfig(CardRewardRarityWeights normal, CardRewardRarityWeights elite, int rareOffsetInitial, int rareOffsetCommonIncrease)
		{
			this.normal = normal;
			this.elite = elite;
			this.rareOffsetInitial = rareOffsetInitial;
			this.rareOffsetCommonIncrease = rareOffsetCommonIncrease;
		}
	}

	[Serializable]
	public class CardRewardRarityConfig
	{
		public const int CurrentRareOffsetConfigVersion = 1;

		public List<CardRewardRarityTierConfig> tiers = CreateDefaultTiers();

		public int rareOffsetMax = 20;

		public CardRewardRarityTierConfig GetTier(int currentStage)
		{
			EnsureDefaults();
			int index = Mathf.Clamp((Mathf.Max(1, currentStage) - 1) / 2, 0, tiers.Count - 1);
			return tiers[index];
		}

		public int ResolveRareOffset(int currentStage, int savedStage, int savedOffset, int savedConfigVersion)
		{
			CardRewardRarityTierConfig tier = GetTier(currentStage);
			if (savedConfigVersion != 1 || savedStage != currentStage)
			{
				return tier.rareOffsetInitial;
			}
			return Mathf.Clamp(savedOffset, tier.rareOffsetInitial, rareOffsetMax);
		}

		public void EnsureDefaults()
		{
			if (tiers.IsNullOrEmpty())
			{
				tiers = CreateDefaultTiers();
			}
		}

		private static List<CardRewardRarityTierConfig> CreateDefaultTiers()
		{
			return new List<CardRewardRarityTierConfig>
			{
				new CardRewardRarityTierConfig(new CardRewardRarityWeights(60, 35, 5), new CardRewardRarityWeights(50, 42, 8), -4, 1),
				new CardRewardRarityTierConfig(new CardRewardRarityWeights(50, 42, 8), new CardRewardRarityWeights(43, 43, 14), -5, 2),
				new CardRewardRarityTierConfig(new CardRewardRarityWeights(43, 43, 14), new CardRewardRarityWeights(40, 40, 20), -9, 3),
				new CardRewardRarityTierConfig(new CardRewardRarityWeights(38, 46, 16), new CardRewardRarityWeights(20, 50, 30), -4, 3)
			};
		}
	}

	[Serializable]
	public class LocationDisplayConfig
	{
		public Sprite icon;

		[LocalizeText("地点名称")]
		public string name;

		[LocalizeText("地点描述")]
		public string description;
	}

	public class ModifierConfig
	{
		public Sprite buffIcon;

		public Sprite debuffIcon;

		public Sprite unknownIcon;

		public Sprite commonWeatherIcon;

		public Color positiveEnvironmentColor;

		public Color negativeEnvironmentColor;

		public Color complexEnvironmentColor;

		public float locationPerRandomModifierCount;

		public float locationPerRandomVairantCount;

		public List<(PathLocation.LocationVariantType, float)> randomLocationVariantTypeWeightPool;

		public List<(PathLocation.LocationVariantType, float)> hardRandomLocationVariantTypeWeightPool;
	}

	[Serializable]
	public class RootBlessUpgradeConfig
	{
		public ClavisDef startUnlockClavis;

		public ClavisDef refreshUnlockClavis;

		public MatterDef offeringMatter;

		public int offeringMatterCount = 1;

		public MatterDef refreshMatter;

		public List<int> refreshPrices = new List<int> { 60, 150, 300 };

		public int adRefreshMaxCount = 1;

		public bool CheckStartUnlocked(ApplicationStateInfo applicationState)
		{
			if (startUnlockClavis != null && applicationState?.clavisManager != null)
			{
				if (!applicationState.clavisManager.IsUnlocked(startUnlockClavis))
				{
					return applicationState.clavisManager.IsActivated(startUnlockClavis);
				}
				return true;
			}
			return false;
		}

		public bool CheckRefreshClavisCanOffer(ApplicationStateInfo applicationState)
		{
			if (refreshUnlockClavis != null && applicationState?.clavisManager != null && applicationState.clavisManager.IsUnlocked(refreshUnlockClavis))
			{
				return !applicationState.clavisManager.IsActivated(refreshUnlockClavis);
			}
			return false;
		}
	}

	public BonusLocationEvent bonusLocationEvent;

	public MintageRuneStoneEvent gemBonusEvent;

	public Level cityLevel;

	public CrystalUpgradeWorldEvent crystalUpgradeEventTemplate;

	public WorldEvent afterBossWorldEvent;

	public EnterBattleEvent enterBattleEventTemplate;

	public HealSpringEvent healSpringEventTemplate;

	public RiftWorldEvent riftWorldEventTemplate;

	public RootBlessUpgradeConfig rootBlessUpgradeConfig = new RootBlessUpgradeConfig();

	[Obsolete("Root bless no longer replaces the initial bonfire at runtime.")]
	public HealSpringEvent rootBonfireHealSpringEventTemplate;

	[Obsolete("Use rootBlessUpgradeConfig.startUnlockClavis instead.")]
	public ClavisDef rootBonfireUnlockClavis;

	public ForerunnerWorldEvent forerunnerWorldEvent;

	public ForerunnerWorldEvent shareForerunnerWorldEvent;

	public StoreEvent relicStoreTemplate;

	public UpgradeRoomEvent upgradeRoomEventTemplate;

	public SelectNextStageLevel selectNextStageLevelTemplate;

	public List<EnvironmentInfluenceDef> defaultEnvironmentInfluences;

	public RoomRef upgradeRoomRef;

	public RoomRef storeRoomRef;

	public RoomRef gachaDrawRoomRef;

	public int EnemyLevelIncreasePerStage = 2;

	public int initFinishGameStage = 4;

	public int baseFinishGameStage = 6;

	public int finalFinishGameStage = 7;

	public float environmentInfluenceSpawnRatio = 0.5f;

	public Sprite defaultBonusIcon;

	public Sprite healBonusIcon;

	public Sprite addMaxHealthBonusIcon;

	public Sprite coinBonusIcon;

	public Sprite crystalBonusIcon;

	public Sprite itemBonusIcon;

	public Sprite increaseDefenceBonusIcon;

	[SerializeField]
	public List<BonusGenerationInfo> normalEnemyBattleBonus;

	[SerializeField]
	public List<BonusGenerationInfo> eliteEnemyBattleBonus;

	[SerializeField]
	public CardRewardRarityConfig cardRewardRarityConfig = new CardRewardRarityConfig();

	[SerializeField]
	public RuneRarityWeights runeRarityWeights = new RuneRarityWeights();

	public float normalFightSpecialBonusBaseChance = 0.4f;

	public float eliteFightSpecialBonusBaseChance = 1f;

	public float SpecialBonusLootWeight = 3f;

	public float SpecialBonusIncreaseDefenceWeight = 2f;

	public float SpecialBonusRuneWeight = 1f;

	public ModifierConfig pathLocationModifierDisplayConfig;

	public Dictionary<PathLocation.LocationType, LocationDisplayConfig> locationDisplayConfigs;

	public Dictionary<PathLocation.LocationVariantType, LocationDisplayConfig> locationVariantDisplayConfigs;

	public LocationDisplayConfig hideLocationDisplayConfig;

	[SerializeField]
	public Dictionary<DifficultyLevel, Dictionary<EnemyEncounterType, Dictionary<int, int>>> gemBonusConfig;

	[SerializeField]
	public Dictionary<DifficultyLevel, Dictionary<EnemyEncounterType, Dictionary<int, int>>> gemBonusLevelConfig;

	public LocationGenerator GetLocationGenerator(PathLocation.LocationType type)
	{
		return GetAllLocationGeneratorInCurrentAssembly().Find((LocationGenerator t) => t.LocationType == type);
	}

	public static List<LocationGenerator> GetAllLocationGeneratorInCurrentAssembly()
	{
		List<LocationGenerator> list = new List<LocationGenerator>();
		Type locationGeneratorType = typeof(LocationGenerator);
		foreach (Type item2 in from t in locationGeneratorType.Assembly.GetTypes()
			where t.IsSubclassOf(locationGeneratorType) && !t.IsAbstract
			select t)
		{
			if (Activator.CreateInstance(item2) is LocationGenerator item)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
