using GameRandom;

public class ChangeRandomIntent : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer drawPileItemContainer = eve.CasterOwner.drawPileItemContainer;
		ItemContainer tableContainer = eve.CasterOwner.tableContainer;
		Item item = drawPileItemContainer.GetAllItems().SelectRandom(eve.gameState.randomManager.NextDouble);
		if (item != null)
		{
			Effect.EnqueueAction(eve, new TransferItemAction(item, tableContainer, eve.CasterItem, TransferItemAction.TransferAnimType.Transfer));
		}
		Effect.EnqueueAction(eve, new TransferItemAction(eve.CasterItem, drawPileItemContainer, eve.CasterItem, TransferItemAction.TransferAnimType.Transfer));
		return NodeCastResult.Success;
	}
}
