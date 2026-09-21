using System.Collections.Generic;

public abstract class ContractRuleDef : CTScriptableObject
{
	public override string CTAssetIDPrepend => "CONRULE";

	public virtual DifficultyLevel? FixedDifficulty => null;

	public virtual bool IsDifficultyIsolated => false;

	public virtual int GetEnemyLevelIncreasePerStage(GameStateInfo gameState, int defaultValue)
	{
		return defaultValue;
	}

	public virtual int GetEnemyLevelOffset(GameStateInfo gameState)
	{
		return 0;
	}

	public virtual BattleRewardRule GetBattleRewardRule(GameStateInfo gameState)
	{
		return null;
	}

	public virtual void ApplyEnemyGeneratedItemInitialState(GameStateInfo gameState, EnemyInfoData enemyInfoData, EnemyEncounterType encounterType, Item item)
	{
	}

	public virtual bool IsEnemyInitialItemOutsideIntentLimit(GameStateInfo gameState, EnemyInfoData enemyInfoData, EnemyEncounterType encounterType, Item item)
	{
		return false;
	}

	public virtual bool CanGenerateLoot(GameStateInfo gameState, ItemDefinition itemDefinition)
	{
		return true;
	}

	public virtual bool CanGenerateRune(GameStateInfo gameState, RuneDef runeDef)
	{
		return true;
	}

	public virtual void ModifyStoreItemsForBuy(GameStateInfo gameState, List<StoreEvent.PricedItem> itemsForSell)
	{
	}

	public virtual void NotifyStoreItemBought(GameStateInfo gameState, Item item)
	{
	}

	public virtual bool CanSellItem(GameStateInfo gameState, Item item)
	{
		return true;
	}

	public virtual bool CanActivatePotentialInLevelWelcome(GameStateInfo gameState)
	{
		return true;
	}

	public virtual bool CanGenerateRiftWorldEvent(GameStateInfo gameState)
	{
		return true;
	}

	public virtual bool CanUseBossBlessAutoBalance(GameStateInfo gameState)
	{
		return true;
	}

	public virtual bool ForceHealSpringChoicesMutuallyExclusive(GameStateInfo gameState)
	{
		return false;
	}

	public virtual WorldEvent GetStartWorldEventOverride(GameStateInfo gameState)
	{
		return null;
	}
}
