using System.Collections.Generic;
using GameRandom;

public class EnterBattleEffect : Effect
{
	public List<EnemyAssetRef> enemyPool;

	public int enemySetupIndex = -1;

	public List<ItemDefinition> bonusItems;

	public int levelAdjustment;

	public bool forceElite;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		WorldEvent targetWorldEvent = eve.TargetWorldEvent;
		if (targetWorldEvent == null)
		{
			return NodeCastResult.Failure;
		}
		EncounterSetupInfo setupInfo = eve.gameState.currentEncounter.setupInfo;
		setupInfo.fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
		if (bonusItems != null && bonusItems.Count > 0)
		{
			setupInfo.roomBonus = new BattleBonus();
			foreach (ItemDefinition bonusItem in bonusItems)
			{
				if (bonusItem != null)
				{
					setupInfo.roomBonus.bonuses.Add(new SingleItemBonus(new Item(bonusItem)));
				}
			}
		}
		if (enemyPool != null && enemyPool.Count > 0)
		{
			setupInfo.fixedEnemyInfoData = enemyPool.SelectRandom(eve.gameState.randomManager.NextDouble).GetAssetPreData(eve.gameState.applicationState.assetCatalog);
			if (enemySetupIndex != -1 && setupInfo.fixedEnemyInfoData.setups.Count > enemySetupIndex)
			{
				setupInfo.fixedEnemySetupIndex = enemySetupIndex;
				setupInfo.fixedEnemySetup = setupInfo.fixedEnemyInfoData.setups[enemySetupIndex];
			}
		}
		setupInfo.enemyLevel += levelAdjustment;
		if (forceElite)
		{
			setupInfo.enemySpawnType = EnemyEncounterType.Elite;
		}
		targetWorldEvent.FinishedWorldEvent(exitRoom: false);
		Effect.EnqueueAction(eve, new TransitReloadRoomAction(setupInfo));
		return NodeCastResult.Success;
	}
}
