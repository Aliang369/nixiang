using System.Collections.Generic;

public class RestoreEnemyDeckAction : ExecuteAction
{
	private readonly BattleFieldTransform bft;

	private readonly object source;

	public RestoreEnemyDeckAction(BattleFieldTransform bft, object source)
	{
		this.bft = bft;
		this.source = source;
	}

	protected override void Update()
	{
		if (bft?.characterAI == null)
		{
			return;
		}
		List<Item> configuredSources = EnemyDeckReconcileRules.GetConfiguredSources(bft, includePhase2: false);
		List<Item> activeCycleItems = EnemyDeckReconcileRules.GetActiveCycleItems(bft);
		EnemyDeckReconcileRules.RestorePlan restorePlan = EnemyDeckReconcileRules.BuildRestorePlan(configuredSources, activeCycleItems);
		foreach (Item foreignItem in restorePlan.ForeignItems)
		{
			EnqueueAction(new ExileItemAction(foreignItem, foreignItem, null, ExileItemAction.ExileType.Permanent));
		}
		foreach (Item missingSource in restorePlan.MissingSources)
		{
			ItemBlackBoard blackboard = missingSource.blackboard;
			ItemContainer targetContainer = ((blackboard != null && blackboard.InitWastePile) ? bft.wastePileItemContainer : bft.drawPileItemContainer);
			EnqueueAction(new SpawnItemAction(missingSource.Copy(spawnFromDeck: true), targetContainer, withAnim: false, source));
		}
	}
}
