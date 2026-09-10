public class DuplicateAddedPlayerDeckItem : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = eve?.TargetItem;
		ItemContainer itemContainer = eve?.DestinationContainer;
		if (item == null || itemContainer == null)
		{
			return NodeCastResult.Failure;
		}
		object eveObj = eve.GetEveObj(GameEvent.EveObjType.Source);
		if (eveObj == this || eveObj is DifficultyInfo)
		{
			return NodeCastResult.Failure;
		}
		if (itemContainer == eve.gameState?.playerState?.DeckContainer)
		{
			GameStateInfo gameState = eve.gameState;
			if (gameState == null || gameState.battleState?.isInBattle != true)
			{
				Item target = item.Copy();
				Effect.EnqueueAction(eve, new SpawnItemAction(target, itemContainer, withAnim: true, this));
				return NodeCastResult.Success;
			}
		}
		return NodeCastResult.Failure;
	}
}
