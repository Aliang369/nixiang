using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SpiderContractRule", menuName = "Contract/Spider Contract Rule")]
public sealed class SpiderContractRuleDef : ContractRuleDef
{
	public enum PotionType
	{
		None,
		InsecticideOne,
		InsecticideTwo,
		InsecticideThree
	}

	public enum PotionUpgrade
	{
		Capacity,
		FullPreparation,
		Concentrated,
		Hunter,
		Thrifty,
		LargeCapacity,
		StepByStep,
		EctoplasmBoost,
		DamageBoost
	}

	[Serializable]
	public sealed class InitialSwarmRule
	{
		public EnemyAssetRef enemy;

		public ItemDefinition swarmItem;
	}

	[Serializable]
	public sealed class InitialNestActivityRule
	{
		public ContractDef contract;

		public int value;

		public int maximum;

		public int enemyLevelOffset;
	}

	private sealed class SpiderBattleRewardRule : BattleRewardRule
	{
		private sealed class SpiderPotionReplenishBonus : BattleBonusObject
		{
			private readonly Item potion;

			private readonly int amount;

			public override Sprite PreviewIcon => potion?.GetDef()?.Icon;

			public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_replenish_usage_count_arg0.FormatWith(amount);

			public SpiderPotionReplenishBonus(Item potion, int amount)
			{
				this.potion = potion;
				this.amount = amount;
			}

			protected override void TriggerImpl(GameStateInfo gameState)
			{
				gameState.actionManager.EnqueueAction(new ChangeCounterAction(amount, potion, this));
				gameState.actionManager.EnqueueAction(new ShowMultiInfoAction(new MultiInfoDisplayPanel.SetupInfo
				{
					items = new List<Item> { potion }
				}, halt: true));
				ConfirmGotBonus();
			}
		}

		private readonly SpiderContractRuleDef ruleDef;

		public SpiderBattleRewardRule(SpiderContractRuleDef ruleDef)
		{
			this.ruleDef = ruleDef;
		}

		protected override bool IsSpecialBonusMechanicEnabled(GameStateInfo gameState)
		{
			return ruleDef.enableSpecialBonusMechanic;
		}

		public override BattleBonus BuildFinalBonus(BattleRewardContext context)
		{
			BattleBonus battleBonus = base.BuildFinalBonus(context);
			if (context.finishType != FinishBattleEncounterAction.FinishType.Win)
			{
				return battleBonus;
			}
			Item potion = ruleDef.GetPotion(context.gameState);
			int battleReplenishAmount = ruleDef.GetBattleReplenishAmount(context.gameState);
			if (potion != null && battleReplenishAmount > 0)
			{
				battleBonus.bonuses.Add(new SpiderPotionReplenishBonus(potion, battleReplenishAmount));
			}
			return battleBonus;
		}
	}

	private const bool OverrideStingingArmorCounter = false;

	private const string NestActivityMaximumKey = "spider.nestActivity.maximum";

	private const string NestActivityCurrentKey = "spider.nestActivity.current";

	private const string NestActivitySuppressedKey = "spider.nestActivity.suppressed";

	private const string NestDestroyedEnemyLevelReductionKey = "spider.nestDestroyedEnemyLevelReduction";

	private const string PotionTypeKey = "spider.potion.type";

	private const string PotionUsedThisBattleKey = "spider.potion.usedThisBattle";

	private const string PotionBattleEncounterTypeKey = "spider.potion.battleEncounterType";

	private const string PotionUpgradeKeyPrefix = "spider.potion.upgrade.";

	public bool enableSpecialBonusMechanic;

	public int initialNestActivityNormal = 3;

	public int initialNestActivityHard = 4;

	public int initialNestActivityHigh = 5;

	public int nestActivityMaximumNormal = 3;

	public int nestActivityMaximumHard = 6;

	public int nestActivityMaximumHigh = 9;

	public List<InitialNestActivityRule> initialNestActivityRules = new List<InitialNestActivityRule>();

	public List<ItemDefinition> excludedLootItems = new List<ItemDefinition>();

	public List<RuneDef> excludedRunes = new List<RuneDef>();

	public List<InitialSwarmRule> initialSwarmRules = new List<InitialSwarmRule>();

	public SpiderPotionChoiceEvent initialPotionChoiceEvent;

	public SpiderPotionChoiceEvent potionUpgradeChoiceEvent;

	public ItemDefinition insecticideOne;

	public ItemDefinition insecticideTwo;

	public ItemDefinition insecticideThree;

	public override DifficultyLevel? FixedDifficulty => DifficultyLevel.NightmareD3;

	public override bool IsDifficultyIsolated => true;

	public override int GetEnemyLevelIncreasePerStage(GameStateInfo gameState, int defaultValue)
	{
		return defaultValue * 2;
	}

	public override int GetEnemyLevelOffset(GameStateInfo gameState)
	{
		return GetConfiguredEnemyLevelOffset(gameState) - GetDestroyedNestEnemyLevelReduction(gameState);
	}

	public override BattleRewardRule GetBattleRewardRule(GameStateInfo gameState)
	{
		return new SpiderBattleRewardRule(this);
	}

	public override bool CanGenerateLoot(GameStateInfo gameState, ItemDefinition itemDefinition)
	{
		if (itemDefinition != null)
		{
			return !excludedLootItems.OrEmptyIfNull().Contains(itemDefinition);
		}
		return false;
	}

	public override void ModifyStoreItemsForBuy(GameStateInfo gameState, List<StoreEvent.PricedItem> itemsForSell)
	{
		itemsForSell?.RemoveAll((StoreEvent.PricedItem pricedItem) => FinalEndingKeyUtility.IsFinalEndingKey(pricedItem?.item));
	}

	public override bool CanSellItem(GameStateInfo gameState, Item item)
	{
		ItemDefinition itemDefinition = item?.GetDef();
		if (itemDefinition != insecticideOne && itemDefinition != insecticideTwo)
		{
			return itemDefinition != insecticideThree;
		}
		return false;
	}

	public override bool CanGenerateRune(GameStateInfo gameState, RuneDef runeDef)
	{
		if (runeDef != null)
		{
			return !excludedRunes.OrEmptyIfNull().Contains(runeDef);
		}
		return false;
	}

	public override bool CanActivatePotentialInLevelWelcome(GameStateInfo gameState)
	{
		return false;
	}

	public override bool CanGenerateRiftWorldEvent(GameStateInfo gameState)
	{
		return false;
	}

	public override bool CanUseBossBlessAutoBalance(GameStateInfo gameState)
	{
		return false;
	}

	public override bool ForceHealSpringChoicesMutuallyExclusive(GameStateInfo gameState)
	{
		return true;
	}

	public override WorldEvent GetStartWorldEventOverride(GameStateInfo gameState)
	{
		return initialPotionChoiceEvent;
	}

	public ItemDefinition GetPotionDefinition(PotionType potionType)
	{
		return potionType switch
		{
			PotionType.InsecticideOne => insecticideOne, 
			PotionType.InsecticideTwo => insecticideTwo, 
			PotionType.InsecticideThree => insecticideThree, 
			_ => null, 
		};
	}

	public PotionType GetPotionType(GameStateInfo gameState)
	{
		return (PotionType)Mathf.Clamp((gameState?.contractRuntimeStateInfo?.GetInt("spider.potion.type")).GetValueOrDefault(), 0, 3);
	}

	public bool HasPotion(GameStateInfo gameState)
	{
		return GetPotionType(gameState) != PotionType.None;
	}

	public bool IsCurrentPotion(GameStateInfo gameState, Item item)
	{
		if (item != null)
		{
			return item.GetDef() == GetPotionDefinition(GetPotionType(gameState));
		}
		return false;
	}

	public Item GetPotion(GameStateInfo gameState)
	{
		ItemDefinition potionDefinition = GetPotionDefinition(GetPotionType(gameState));
		if (potionDefinition == null)
		{
			return null;
		}
		object obj = gameState?.playerState?.DeckContainer?.GetAllItems().FirstOrDefault((Item item) => item?.GetDef() == potionDefinition);
		if (obj == null)
		{
			if (gameState == null)
			{
				return null;
			}
			PlayerGameStateInfo playerState = gameState.playerState;
			if (playerState == null)
			{
				return null;
			}
			BattleFieldTransform bft = playerState.bft;
			if (bft == null)
			{
				return null;
			}
			obj = bft.GetAllRealItems().FirstOrDefault((Item item) => item?.GetDef() == potionDefinition);
		}
		return (Item)obj;
	}

	public bool SelectPotion(GameStateInfo gameState, PotionType potionType)
	{
		ItemDefinition potionDefinition = GetPotionDefinition(potionType);
		if (gameState?.contractRuntimeStateInfo == null || gameState.playerState?.DeckContainer == null || potionDefinition == null || HasPotion(gameState))
		{
			return false;
		}
		gameState.contractRuntimeStateInfo.SetInt("spider.potion.type", (int)potionType);
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item>
			{
				new Item(potionDefinition)
			}
		}));
		return true;
	}

	public bool HasPotionUpgrade(GameStateInfo gameState, PotionUpgrade upgrade)
	{
		return GetPotionUpgradeCount(gameState, upgrade) > 0;
	}

	public int GetPotionUpgradeCount(GameStateInfo gameState, PotionUpgrade upgrade)
	{
		return Mathf.Max(0, (gameState?.contractRuntimeStateInfo?.GetInt(GetPotionUpgradeKey(upgrade))).GetValueOrDefault());
	}

	public bool CanApplyPotionUpgrade(GameStateInfo gameState, PotionUpgrade upgrade)
	{
		if (GetPotion(gameState) == null)
		{
			return false;
		}
		if (!IsRepeatablePotionUpgrade(upgrade) && HasPotionUpgrade(gameState, upgrade))
		{
			return false;
		}
		return upgrade switch
		{
			PotionUpgrade.EctoplasmBoost => GetPotionType(gameState) == PotionType.InsecticideTwo, 
			PotionUpgrade.DamageBoost => GetPotionType(gameState) == PotionType.InsecticideThree, 
			_ => true, 
		};
	}

	public bool ApplyPotionUpgrade(GameStateInfo gameState, PotionUpgrade upgrade)
	{
		Item potion = GetPotion(gameState);
		if (potion == null || !CanApplyPotionUpgrade(gameState, upgrade))
		{
			return false;
		}
		switch (upgrade)
		{
		case PotionUpgrade.Capacity:
			SetPotionCounters(gameState, potion, potion.counter, (int)potion.maxCounter + 3);
			break;
		case PotionUpgrade.FullPreparation:
			SetPotionCounters(gameState, potion, potion.maxCounter, potion.maxCounter);
			break;
		case PotionUpgrade.Concentrated:
			SetPotionCounters(gameState, potion, (int)potion.counter / 2, (int)potion.maxCounter / 2);
			break;
		case PotionUpgrade.LargeCapacity:
			SetPotionCounters(gameState, potion, 0, (int)potion.maxCounter + (int)potion.counter);
			break;
		}
		gameState.contractRuntimeStateInfo.ChangeInt(GetPotionUpgradeKey(upgrade), 1);
		return true;
	}

	public void ResetPotionBattleUsage(GameStateInfo gameState, EnemyEncounterType encounterType)
	{
		gameState?.contractRuntimeStateInfo?.SetInt("spider.potion.usedThisBattle", 0);
		gameState?.contractRuntimeStateInfo?.SetInt("spider.potion.battleEncounterType", (int)encounterType);
	}

	public void NotifyPotionUsed(GameStateInfo gameState)
	{
		gameState?.contractRuntimeStateInfo?.SetInt("spider.potion.usedThisBattle", 1);
	}

	public bool WasPotionUsedThisBattle(GameStateInfo gameState)
	{
		if (gameState == null)
		{
			return false;
		}
		return gameState.contractRuntimeStateInfo?.GetInt("spider.potion.usedThisBattle") > 0;
	}

	public bool WasCurrentBattleElite(GameStateInfo gameState)
	{
		if (gameState == null)
		{
			return false;
		}
		return gameState.contractRuntimeStateInfo?.GetInt("spider.potion.battleEncounterType") == 1;
	}

	public int GetPotionTempHealth(GameStateInfo gameState)
	{
		if (GetPotionType(gameState) != PotionType.InsecticideTwo)
		{
			return 0;
		}
		return 10 + 15 * GetPotionUpgradeCount(gameState, PotionUpgrade.EctoplasmBoost);
	}

	public int GetPotionDamage(GameStateInfo gameState)
	{
		if (GetPotionType(gameState) != PotionType.InsecticideThree)
		{
			return 0;
		}
		return 10 + 20 * GetPotionUpgradeCount(gameState, PotionUpgrade.DamageBoost);
	}

	public int GetNestHeartActivityReduction(GameStateInfo gameState)
	{
		if (!HasPotionUpgrade(gameState, PotionUpgrade.Concentrated))
		{
			return 1;
		}
		return 2;
	}

	public void ReplenishPotion(GameStateInfo gameState, int amount)
	{
		Item potion = GetPotion(gameState);
		if (potion != null && amount > 0)
		{
			SetPotionCounters(gameState, potion, Mathf.Min(potion.maxCounter, (int)potion.counter + amount), potion.maxCounter);
		}
	}

	public int GetBattleReplenishAmount(GameStateInfo gameState)
	{
		bool flag = WasCurrentBattleElite(gameState);
		int num = (flag ? 1 : 0);
		if (HasPotionUpgrade(gameState, PotionUpgrade.Hunter) & flag)
		{
			num++;
		}
		if (HasPotionUpgrade(gameState, PotionUpgrade.Thrifty) && !WasPotionUsedThisBattle(gameState))
		{
			num++;
		}
		return num;
	}

	public int GetCampfireReplenishAmount(GameStateInfo gameState)
	{
		Item potion = GetPotion(gameState);
		if (potion != null)
		{
			return Mathf.Max(1, Mathf.FloorToInt((float)(int)potion.maxCounter * 0.2f));
		}
		return 0;
	}

	private static bool IsRepeatablePotionUpgrade(PotionUpgrade upgrade)
	{
		if (upgrade != PotionUpgrade.Capacity && upgrade != PotionUpgrade.FullPreparation && upgrade != PotionUpgrade.LargeCapacity && upgrade != PotionUpgrade.EctoplasmBoost)
		{
			return upgrade == PotionUpgrade.DamageBoost;
		}
		return true;
	}

	private static string GetPotionUpgradeKey(PotionUpgrade upgrade)
	{
		return "spider.potion.upgrade." + upgrade;
	}

	private static void SetPotionCounters(GameStateInfo gameState, Item potion, int current, int maximum)
	{
		maximum = Mathf.Max(0, maximum);
		current = Mathf.Clamp(current, 0, maximum);
		gameState.actionManager.EnqueueAction(new SetMaxCounterAction(maximum, potion, potion));
		gameState.actionManager.EnqueueAction(new SetCounterAction(current, potion, potion));
		gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			potion.resetInitCounter = current;
			potion.resetInitMaxCounter = maximum;
		}));
	}

	public void InitializeNestActivity(GameStateInfo gameState)
	{
		if (gameState?.contractRuntimeStateInfo != null)
		{
			int num = Mathf.Max(0, GetInitialNestActivityMaximum(gameState));
			int value = Mathf.Clamp(GetInitialNestActivity(gameState), 0, num);
			gameState.contractRuntimeStateInfo.SetInt("spider.nestActivity.maximum", num);
			gameState.contractRuntimeStateInfo.SetInt("spider.nestActivity.current", value);
			gameState.contractRuntimeStateInfo.SetInt("spider.nestActivity.suppressed", 0);
		}
	}

	public int GetNestActivityMaximum(GameStateInfo gameState)
	{
		EnsureNestActivityInitialized(gameState);
		return Mathf.Max(0, (gameState?.contractRuntimeStateInfo?.GetInt("spider.nestActivity.maximum")).GetValueOrDefault());
	}

	public int GetNestActivityCurrent(GameStateInfo gameState)
	{
		int nestActivityMaximum = GetNestActivityMaximum(gameState);
		return Mathf.Clamp((gameState?.contractRuntimeStateInfo?.GetInt("spider.nestActivity.current")).GetValueOrDefault(), 0, nestActivityMaximum);
	}

	public int GetNestActivityChange(GameStateInfo gameState)
	{
		return GetNestActivityCurrent(gameState) - GetInitialNestActivity(gameState);
	}

	public int GetNestActivitySuppressed(GameStateInfo gameState)
	{
		int nestActivityMaximum = GetNestActivityMaximum(gameState);
		return Mathf.Clamp((gameState?.contractRuntimeStateInfo?.GetInt("spider.nestActivity.suppressed")).GetValueOrDefault(), 0, nestActivityMaximum);
	}

	public bool IsNestActivityAtMaximum(GameStateInfo gameState)
	{
		int nestActivityMaximum = GetNestActivityMaximum(gameState);
		if (nestActivityMaximum > 0)
		{
			return GetNestActivityCurrent(gameState) >= nestActivityMaximum;
		}
		return false;
	}

	public int ChangeNestActivity(GameStateInfo gameState, int delta)
	{
		if (gameState?.contractRuntimeStateInfo == null)
		{
			return 0;
		}
		int nestActivityMaximum = GetNestActivityMaximum(gameState);
		int num = Mathf.Clamp(GetNestActivityCurrent(gameState) + delta, 0, nestActivityMaximum);
		gameState.contractRuntimeStateInfo.SetInt("spider.nestActivity.current", num);
		gameState.contractRuntimeStateInfo.SetInt("spider.nestActivity.suppressed", nestActivityMaximum - num);
		return num;
	}

	public void ReduceFutureEnemyLevel(GameStateInfo gameState)
	{
		if (gameState?.contractRuntimeStateInfo != null)
		{
			int value = GetDestroyedNestEnemyLevelReduction(gameState) + 1;
			gameState.contractRuntimeStateInfo.SetInt("spider.nestDestroyedEnemyLevelReduction", value);
		}
	}

	public int GetNestEnemyLevelChange(GameStateInfo gameState)
	{
		return -GetDestroyedNestEnemyLevelReduction(gameState);
	}

	private static int GetDestroyedNestEnemyLevelReduction(GameStateInfo gameState)
	{
		return Mathf.Max(0, (gameState?.contractRuntimeStateInfo?.GetInt("spider.nestDestroyedEnemyLevelReduction")).GetValueOrDefault());
	}

	private void EnsureNestActivityInitialized(GameStateInfo gameState)
	{
		if (gameState?.contractRuntimeStateInfo != null && gameState.contractRuntimeStateInfo.GetInt("spider.nestActivity.maximum") <= 0)
		{
			InitializeNestActivity(gameState);
		}
	}

	public override void ApplyEnemyGeneratedItemInitialState(GameStateInfo gameState, EnemyInfoData enemyInfoData, EnemyEncounterType encounterType, Item item)
	{
		if (gameState != null && enemyInfoData != null && item != null)
		{
			ItemDefinition def = item.def;
			if ((object)def != null && def.abilities?.Any((Ability ability) => ability is SpiderWebSwarmAbility || ability is SpiderAbsorbSwarmAbility) == true)
			{
				int num = 1 + Mathf.FloorToInt((float)GetNestActivityCurrent(gameState) * 0.25f);
				item.counter = num;
				item.maxCounter = num;
				item.resetInitCounter = num;
				item.resetInitMaxCounter = num;
				item.blackboard?.SetValue(ItemBlackBoard.ValueType.InitCounter, num);
				item.blackboard?.SetValue(ItemBlackBoard.ValueType.InitMaxCounter, num);
			}
		}
	}

	public override bool IsEnemyInitialItemOutsideIntentLimit(GameStateInfo gameState, EnemyInfoData enemyInfoData, EnemyEncounterType encounterType, Item item)
	{
		if (!(enemyInfoData?.FullId != "#ENEMYINFO#000256#"))
		{
			Item item2 = item;
			if (item2 != null && item2.blackboard?.StayInTable == true)
			{
				return initialSwarmRules.OrEmptyIfNull().Any((InitialSwarmRule rule) => rule?.enemy?.CTAssetId == enemyInfoData.FullId && rule.swarmItem == item.GetDef());
			}
		}
		return false;
	}

	private int GetInitialNestActivity(GameStateInfo gameState)
	{
		ContractDef contractDef = gameState?.contractState?.selectedContract?.contractDef;
		InitialNestActivityRule initialNestActivityRule = initialNestActivityRules.OrEmptyIfNull().FirstOrDefault((InitialNestActivityRule rule) => rule?.contract == contractDef);
		if (initialNestActivityRule != null)
		{
			return initialNestActivityRule.value;
		}
		DifficultyLevel difficultyLevel = gameState?.contractState?.selectedContract?.difficultyLevel ?? DifficultyLevel.Normal;
		if (difficultyLevel <= DifficultyLevel.Normal)
		{
			return initialNestActivityNormal;
		}
		if (difficultyLevel == DifficultyLevel.Hard)
		{
			return initialNestActivityHard;
		}
		return initialNestActivityHigh;
	}

	private int GetInitialNestActivityMaximum(GameStateInfo gameState)
	{
		ContractDef contractDef = gameState?.contractState?.selectedContract?.contractDef;
		InitialNestActivityRule initialNestActivityRule = initialNestActivityRules.OrEmptyIfNull().FirstOrDefault((InitialNestActivityRule rule) => rule?.contract == contractDef);
		if (initialNestActivityRule != null && initialNestActivityRule.maximum > 0)
		{
			return initialNestActivityRule.maximum;
		}
		DifficultyLevel difficultyLevel = gameState?.contractState?.selectedContract?.difficultyLevel ?? DifficultyLevel.Normal;
		if (difficultyLevel <= DifficultyLevel.Normal)
		{
			return nestActivityMaximumNormal;
		}
		if (difficultyLevel == DifficultyLevel.Hard)
		{
			return nestActivityMaximumHard;
		}
		return nestActivityMaximumHigh;
	}

	private int GetConfiguredEnemyLevelOffset(GameStateInfo gameState)
	{
		ContractDef contractDef = gameState?.contractState?.selectedContract?.contractDef;
		return initialNestActivityRules.OrEmptyIfNull().FirstOrDefault((InitialNestActivityRule rule) => rule?.contract == contractDef)?.enemyLevelOffset ?? 0;
	}
}
