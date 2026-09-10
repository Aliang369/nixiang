using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class BattleRewardRule
{
	public static readonly BattleRewardRule Default = new BattleRewardRule();

	public virtual BattleBonus BuildFinalBonus(BattleRewardContext context)
	{
		GameStateInfo gameState = context.gameState;
		BattleBonus battleBonus = gameState.currentEncounter.roomBonus ?? new BattleBonus();
		PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
		if (currentLocation != null && !gameState.IsIntroGame && context.finishType == FinishBattleEncounterAction.FinishType.Win)
		{
			if (currentLocation.type == PathLocation.LocationType.EliteFight)
			{
				AddEliteFightBonus(gameState, battleBonus);
			}
			if (currentLocation.type == PathLocation.LocationType.NormalFight)
			{
				AddNormalFightBonus(gameState, battleBonus);
			}
			if (currentLocation.type == PathLocation.LocationType.Boss)
			{
				AddBossFightBonus(gameState, battleBonus);
			}
		}
		return battleBonus;
	}

	public virtual void AddNormalFightBonus(GameStateInfo gameState, BattleBonus bonus)
	{
		AppendEnemyCoinAndCrystalBonus(EnemyEncounterType.Normal, gameState, bonus);
		if (!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoCardBonusAfterFight))
		{
			bonus.bonuses.Add(new GenerateCardsBonus(gameState, CardRewardType.NormalBattle));
		}
		if (IsSpecialBonusMechanicEnabled(gameState))
		{
			float normalFightSpecialBonusBaseChance = ScriptableObjectSingleton<DungeonGenerationManager>.instance.normalFightSpecialBonusBaseChance;
			if (gameState.randomManager.NextFloat() < normalFightSpecialBonusBaseChance)
			{
				bonus.bonuses.Add(new LootBonus(gameState));
			}
		}
	}

	public virtual void AddEliteFightBonus(GameStateInfo gameState, BattleBonus bonus, bool includeOptionalRewards = true)
	{
		AppendEnemyCoinAndCrystalBonus(EnemyEncounterType.Elite, gameState, bonus);
		if (!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoCardBonusAfterFight))
		{
			bonus.bonuses.Add(new GenerateCardsBonus(gameState, CardRewardType.EliteBattle));
		}
		if (includeOptionalRewards && IsSpecialBonusMechanicEnabled(gameState))
		{
			float eliteFightSpecialBonusBaseChance = ScriptableObjectSingleton<DungeonGenerationManager>.instance.eliteFightSpecialBonusBaseChance;
			if (gameState.randomManager.NextFloat() < eliteFightSpecialBonusBaseChance)
			{
				if (gameState.randomManager.NextFloat() < 0.5f)
				{
					bonus.bonuses.Add(new LootBonus(gameState));
				}
				else
				{
					Rune rune = gameState.generationManager.GenerateMintageRunes(1).FirstOrDefault();
					if (rune == null)
					{
						TraceLog.Error("[BattleRewardRule] Elite fight special bonus failed to generate mintage rune.");
					}
					else
					{
						bonus.bonuses.Add(new RuneBonus(rune));
					}
				}
			}
		}
		if (!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoBlessBonusAfterEliteFight))
		{
			int num = 1;
			num += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.EliteFightExtraBlessCountDelta);
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			if (currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.EliteVariant)
			{
				num++;
			}
			foreach (InfluenceDef item in gameState.generationManager.GenerateRandomCommonBless(num, updateRecord: true, QualityLevel.Common))
			{
				bonus.bonuses.Add(new AddInfluenceBonus(new List<Influence>
				{
					new Influence(item)
				}, gameState));
				ThinkingAnalyticsExtends.TrackExposeBonusBless(gameState, item, "精英战斗", selectable: true);
			}
		}
		if (includeOptionalRewards && gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableChanceHealBonusAfterEliteFight))
		{
			if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableEliteFightMustHealBonus))
			{
				bonus.bonuses.Add(new HealBonus(15));
			}
			else if (gameState.randomManager.NextFloat() > 0.5f)
			{
				bonus.bonuses.Add(new HealBonus(10));
			}
		}
	}

	public virtual void AddBossFightBonus(GameStateInfo gameState, BattleBonus bonus)
	{
		AppendEnemyCoinAndCrystalBonus(EnemyEncounterType.Boss, gameState, bonus);
		Rune rune = gameState.generationManager.GenerateMintageRunes(1).FirstOrDefault();
		if (rune == null)
		{
			TraceLog.Error("[BattleRewardRule] Boss fight bonus failed to generate mintage rune.");
		}
		else
		{
			bonus.bonuses.Add(new RuneBonus(rune));
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoCardBonusAfterFight))
		{
			bonus.bonuses.Add(new LootBonus(gameState));
		}
		else
		{
			bonus.bonuses.Add(new GenerateCardsBonus(gameState, CardRewardType.BossBattle));
		}
		int variableValue = gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BossFightExtraBlessCountDelta);
		for (int i = 0; i < variableValue; i++)
		{
			InfluenceDef influenceDef = gameState.generationManager.GenerateRandomCommonBless(1, updateRecord: true, QualityLevel.Common).FirstOrDefault();
			if (!(influenceDef == null))
			{
				bonus.bonuses.Add(new AddInfluenceBonus(new List<Influence>
				{
					new Influence(influenceDef)
				}, gameState));
				ThinkingAnalyticsExtends.TrackExposeBonusBless(gameState, influenceDef, "精英战斗", selectable: true);
			}
		}
	}

	protected virtual bool IsSpecialBonusMechanicEnabled(GameStateInfo gameState)
	{
		return gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSpecialBonusMechanic);
	}

	protected static void AppendEnemyCoinAndCrystalBonus(EnemyEncounterType type, GameStateInfo gameState, BattleBonus bonus)
	{
		switch (type)
		{
		case EnemyEncounterType.Elite:
		{
			BonusGenerationInfo bonusGenerationInfo2 = ScriptableObjectSingleton<DungeonGenerationManager>.instance.eliteEnemyBattleBonus.SelectRandom(gameState.randomManager.NextDouble);
			bonus.bonuses.AddRange(bonusGenerationInfo2.GenerateBonusObject(gameState));
			foreach (BattleBonusObject bonuse in bonus.bonuses)
			{
				if (bonuse is CoinBonus coinBonus2)
				{
					coinBonus2.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCoinDeltaForEliteEnemy);
					coinBonus2.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCoinDeltaAfterBattle);
				}
				if (bonuse is CrystalBonus crystalBonus2)
				{
					crystalBonus2.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCrystalDeltaForEliteEnemy);
					crystalBonus2.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCrystalDeltaAfterBattle);
				}
			}
			break;
		}
		case EnemyEncounterType.Normal:
		{
			BonusGenerationInfo bonusGenerationInfo = ScriptableObjectSingleton<DungeonGenerationManager>.instance.normalEnemyBattleBonus.SelectRandom(gameState.randomManager.NextDouble);
			bonus.bonuses.AddRange(bonusGenerationInfo.GenerateBonusObject(gameState));
			foreach (BattleBonusObject bonuse2 in bonus.bonuses)
			{
				if (bonuse2 is CoinBonus coinBonus)
				{
					coinBonus.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCoinDeltaForNormalEnemy);
					coinBonus.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCoinDeltaAfterBattle);
				}
				if (bonuse2 is CrystalBonus crystalBonus)
				{
					crystalBonus.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCrystalDeltaForNormalEnemy);
					crystalBonus.count += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCrystalDeltaAfterBattle);
				}
			}
			break;
		}
		case EnemyEncounterType.Boss:
			bonus.bonuses.Add(new CrystalBonus(6));
			bonus.bonuses.Add(new CoinBonus(6));
			break;
		}
		if (!gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillGemGet))
		{
			return;
		}
		EnemyEncounterType type2 = ((type == EnemyEncounterType.Normal && gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NormalEnemyGemBonusLikeElite)) ? EnemyEncounterType.Elite : type);
		int gemCount = GetGemCount(gameState, type2);
		if (gemCount != 0)
		{
			Dictionary<int, int> gemConfigs = GetGemConfigs(gameState, type2, gemCount);
			if (gemConfigs != null)
			{
				bonus.bonuses.Add(new GemBonus(gemConfigs));
			}
		}
	}

	private static int GetGemCount(GameStateInfo gameState, EnemyEncounterType type)
	{
		if (!ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusConfig.ContainsKey(gameState.contractState.selectedContract.difficultyLevel) || !ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusConfig[gameState.contractState.selectedContract.difficultyLevel].ContainsKey(type))
		{
			return 0;
		}
		Dictionary<int, int> gemConfig = ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusConfig[gameState.contractState.selectedContract.difficultyLevel][type];
		if (gemConfig.IsNullOrEmpty())
		{
			return 0;
		}
		return gemConfig.Keys.ToList().SelectRandomWeight((int l) => gemConfig[l]);
	}

	private static Dictionary<int, int> GetGemConfigs(GameStateInfo gameState, EnemyEncounterType type, int count)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (!ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusLevelConfig.ContainsKey(gameState.contractState.selectedContract.difficultyLevel) || !ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusLevelConfig[gameState.contractState.selectedContract.difficultyLevel].ContainsKey(type))
		{
			return dictionary;
		}
		Dictionary<int, int> dictionary2 = ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusLevelConfig[gameState.contractState.selectedContract.difficultyLevel][type];
		if (dictionary2.IsNullOrEmpty())
		{
			return dictionary;
		}
		for (int i = 0; i < count; i++)
		{
			int num = dictionary2.SelectRandomWeight();
			if (num != 0)
			{
				dictionary.Change(num, 1);
			}
		}
		return dictionary;
	}
}
