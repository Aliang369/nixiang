using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class InGameGlobalVariableStateInfo : InGameManager
{
	public static GlobalVariableSetup healSpringExtraMaxHealthDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCoinDeltaAfterBattle = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCrystalDeltaAfterBattle = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCoinDeltaForNormalEnemy = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCrystalDeltaForNormalEnemy = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCoinDeltaForEliteEnemy = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCrystalDeltaForEliteEnemy = new GlobalVariableSetup();

	public static GlobalVariableSetup relicBonusCountDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreBuyItemCountDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreBuyItemCostDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreBuyItemRefreshExtraCount = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreSellItemCostDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreFreeRecycleItemCount = new GlobalVariableSetup();

	public static GlobalVariableSetup CoinLowerBound = new GlobalVariableSetup();

	public static GlobalVariableSetup CrystalUpgradePriceDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup CrystalUpgradeBlessCountDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup ItemUpgradePriceDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup EliteFightExtraBlessCountDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup BossFightExtraBlessCountDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCoinPerLocationUntilSpend = new GlobalVariableSetup();

	public static GlobalVariableSetup BonusCrystalPerLocationUntilSpend = new GlobalVariableSetup();

	public static GlobalVariableSetup MintageRuneFixedPrice = new GlobalVariableSetup();

	public static GlobalVariableSetup ClassMatterRewardMultiplier = new GlobalVariableSetup();

	public static GlobalVariableSetup GameOverExpMultiplier = new GlobalVariableSetup();

	public static GlobalVariableSetup EnemyHealthPercentDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup ForbidAddCost = new GlobalVariableSetup();

	public static GlobalVariableSetup ForbitDropItem = new GlobalVariableSetup();

	public static GlobalVariableSetup HideAllEnemyIntent = new GlobalVariableSetup();

	public static GlobalVariableSetup PlayerDefenceInvalid = new GlobalVariableSetup();

	public static GlobalVariableSetup ReverseCoolDown = new GlobalVariableSetup();

	public static GlobalVariableSetup NoPathForkLimit = new GlobalVariableSetup();

	public static GlobalVariableSetup MapJumpProgressDistanceDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup DoNotGenerateRandomHealSpring = new GlobalVariableSetup();

	public static GlobalVariableSetup GenerateOnlyOneStore = new GlobalVariableSetup();

	public static GlobalVariableSetup GenerateOnlyOneUpgrade = new GlobalVariableSetup();

	public static GlobalVariableSetup NoIncreasePriceForStoreDeleteCard = new GlobalVariableSetup();

	public static GlobalVariableSetup NoIncreasePriceForCardUpgrade = new GlobalVariableSetup();

	public static GlobalVariableSetup ExtraUncommonBonus = new GlobalVariableSetup();

	public static GlobalVariableSetup EncounterSmith = new GlobalVariableSetup();

	public static GlobalVariableSetup HealSpringCantHeal = new GlobalVariableSetup();

	public static GlobalVariableSetup HealSpringChoicesMutuallyExclusive = new GlobalVariableSetup();

	public static GlobalVariableSetup MintageRuneCantMintage = new GlobalVariableSetup();

	public static GlobalVariableSetup PrepareForExcape = new GlobalVariableSetup();

	public static GlobalVariableSetup ForeseeLocation = new GlobalVariableSetup();

	public static GlobalVariableSetup YinPowerNotConsume = new GlobalVariableSetup();

	public static GlobalVariableSetup RejectCrystalBonusAfterBattle = new GlobalVariableSetup();

	public static GlobalVariableSetup RejectCoinBonusAfterBattle = new GlobalVariableSetup();

	public static GlobalVariableSetup WithoutRuneAttachCountLimit = new GlobalVariableSetup();

	public static GlobalVariableSetup RuneAttachCountLimitDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup leaveCrystalUpgradeCrystalBonusDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup NeedCrystalCostForUnlockSkill = new GlobalVariableSetup();

	public static GlobalVariableSetup AfterBossHealHalfHealth = new GlobalVariableSetup();

	public static GlobalVariableSetup GenerateMoreFightLocation = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableEliteSecondPhaseAbility = new GlobalVariableSetup();

	public static GlobalVariableSetup HalfEnemyIntentExtend = new GlobalVariableSetup();

	public static GlobalVariableSetup AllEnemyIntentExtend = new GlobalVariableSetup();

	public static GlobalVariableSetup ExtraEnemyHealthForPhase2 = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableMoreTypeMaps = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableExtendMapLength = new GlobalVariableSetup();

	public static GlobalVariableSetup LessSpecialBonusGenerateChance = new GlobalVariableSetup();

	public static GlobalVariableSetup DeadlierNormalEnemy = new GlobalVariableSetup();

	public static GlobalVariableSetup DeadlierEliteEnemy = new GlobalVariableSetup();

	public static GlobalVariableSetup DisableForerunnerLegacySpawn = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreBuyItemEndlessRefresh = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreFinalEndingKeyPurchased = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableExtraMap = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableHardLocationVariant = new GlobalVariableSetup();

	public static GlobalVariableSetup MoreStoreRoomGeneration = new GlobalVariableSetup();

	public static GlobalVariableSetup NoBlessBonusAfterEliteFight = new GlobalVariableSetup();

	public static GlobalVariableSetup NoCardBonusAfterFight = new GlobalVariableSetup();

	public static GlobalVariableSetup DontGenerateBonusLocation = new GlobalVariableSetup();

	public static GlobalVariableSetup GetBonusCardWithRune = new GlobalVariableSetup();

	public static GlobalVariableSetup ExileOverflowItemInsteadConsume = new GlobalVariableSetup();

	public static GlobalVariableSetup NoGenerateUpgradeRoom = new GlobalVariableSetup();

	public static GlobalVariableSetup NoGenerateStore = new GlobalVariableSetup();

	public static GlobalVariableSetup OverflowConsumeOnTurnStart = new GlobalVariableSetup();

	public static GlobalVariableSetup DisableAllSkillWord = new GlobalVariableSetup();

	public static GlobalVariableSetup GenerateItemFromAllCharacters = new GlobalVariableSetup();

	public static GlobalVariableSetup BattleAutomaton = new GlobalVariableSetup();

	public static GlobalVariableSetup ShuffleAfterTurnStartDraw = new GlobalVariableSetup();

	public static GlobalVariableSetup LeftHandednessAddCost = new GlobalVariableSetup();

	public static GlobalVariableSetup OnlyActivateSkillOnSectionStart = new GlobalVariableSetup();

	public static GlobalVariableSetup OnlyCommonCardBonusAfterBattle = new GlobalVariableSetup();

	public static GlobalVariableSetup DisableActivatePotentialInHealSpring = new GlobalVariableSetup();

	public static GlobalVariableSetup SelectSkillCountDelta = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableRestExtraHeal = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableRerollRuneNoCost = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableChanceHealBonusAfterEliteFight = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableEliteFightMustLoot = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableEliteFightMustHealBonus = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableUpgradeBonusLocationGame = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableBonusLocationGameExtraRareBonus = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableSwitchBonusLocationGame = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableCrystalUpgradeLeaveGetCrystal = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableCrystalUpgradeGetMultipleUpgrade = new GlobalVariableSetup();

	public static GlobalVariableSetup NoStoreRecycleItemPriceStageIncrease = new GlobalVariableSetup();

	public static GlobalVariableSetup FixStoreBuyCardPriceToOne = new GlobalVariableSetup();

	public static GlobalVariableSetup StoreBuyItemRefreshNoCostOnce = new GlobalVariableSetup();

	public static GlobalVariableSetup ItemUpgradeFreeTimes = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableExtraLocationModifier = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableHardLevelEnvironmentInfluence = new GlobalVariableSetup();

	public static GlobalVariableSetup EnableEliteEnemyVariant = new GlobalVariableSetup();

	public static GlobalVariableSetup MintageRuneCanGenerateNegative = new GlobalVariableSetup();

	public static GlobalVariableSetup NormalEnemyGemBonusLikeElite = new GlobalVariableSetup();

	public static GlobalVariableSetup ForceAmbushFight = new GlobalVariableSetup();

	[SaveField(null, null)]
	public List<GlobalVariable> globalVariables;

	public static GlobalVariableSetup ExtraRareCardBonus => ExtraUncommonBonus;

	public static List<GlobalVariableSetup> GetAllGlobalVariableSetups()
	{
		List<GlobalVariableSetup> list = new List<GlobalVariableSetup>();
		foreach (FieldInfo item in from f in typeof(InGameGlobalVariableStateInfo).GetFields(BindingFlags.Static | BindingFlags.Public)
			where f.FieldType == typeof(GlobalVariableSetup)
			select f)
		{
			if (item.GetValue(null) is GlobalVariableSetup globalVariableSetup)
			{
				globalVariableSetup.id = item.Name;
				list.Add(globalVariableSetup);
			}
		}
		return list;
	}

	public bool GetVariableFlag(string id)
	{
		return GetVariableValue(id) > 0;
	}

	public bool GetVariableFlag(GlobalVariableSetup setup)
	{
		return GetVariableValue(setup) > 0;
	}

	public int GetVariableValue(string id)
	{
		return globalVariables.Find((GlobalVariable g) => g.id == id)?.value ?? 0;
	}

	public int GetVariableValue(GlobalVariableSetup setup)
	{
		return GetVariableValue(setup.id);
	}

	public bool WithVariable(string id)
	{
		if (globalVariables.Find((GlobalVariable g) => g.id == id) != null)
		{
			return true;
		}
		return false;
	}

	public bool WithVariable(GlobalVariableSetup setup)
	{
		return WithVariable(setup.id);
	}

	public override void Init(GameStateInfo gameState)
	{
		InitSetup();
		if (globalVariables == null)
		{
			globalVariables = new List<GlobalVariable>();
			Reset();
		}
	}

	public void Reset()
	{
		globalVariables.Clear();
		foreach (GlobalVariableSetup allGlobalVariableSetup in GetAllGlobalVariableSetups())
		{
			globalVariables.Add(new GlobalVariable
			{
				id = allGlobalVariableSetup.id,
				value = allGlobalVariableSetup.initValue
			});
		}
	}

	private static void InitSetup()
	{
		foreach (FieldInfo item in from f in typeof(InGameGlobalVariableStateInfo).GetFields(BindingFlags.Static | BindingFlags.Public)
			where f.FieldType == typeof(GlobalVariableSetup)
			select f)
		{
			(item.GetValue(null) as GlobalVariableSetup).id = item.Name;
		}
	}

	public void SetVariableValue(string id, int value)
	{
		GlobalVariable globalVariable = globalVariables.Find((GlobalVariable g) => g.id == id);
		if (globalVariable != null)
		{
			globalVariable.value = value;
			return;
		}
		globalVariable = new GlobalVariable
		{
			id = id,
			value = value
		};
		globalVariables.Add(globalVariable);
	}

	public void SetVariableValue(GlobalVariableSetup setup, int value)
	{
		SetVariableValue(setup.id, value);
	}
}
