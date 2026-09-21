using System;

public class SpawnConfiguredPhase2Items : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve.TargetBft ?? eve.CasterOwner;
		CharacterAI characterAI = battleFieldTransform?.characterAI;
		if (characterAI == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item2 in characterAI.phase2Items.OrEmptyIfNull())
		{
			Item item = item2.Copy(spawnFromDeck: true);
			bool initSpawn = item.blackboard.InitSpawn;
			ItemContainer targetContainer = (initSpawn ? battleFieldTransform.tableContainer : battleFieldTransform.drawPileItemContainer);
			Effect.EnqueueAction(eve, new SpawnItemAction(item, targetContainer, withAnim: true, eve.CasterItem, initSpawn ? ((Func<int>)(() => 0)) : null));
		}
		return NodeCastResult.Success;
	}
}
