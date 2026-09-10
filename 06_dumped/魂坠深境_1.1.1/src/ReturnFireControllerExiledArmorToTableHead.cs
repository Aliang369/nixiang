public class ReturnFireControllerExiledArmorToTableHead : FireControllerExileEffect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item casterItem = eve.CasterItem;
		ItemContainer itemContainer = casterItem?.OwnerBft?.tableContainer;
		if (casterItem == null || itemContainer == null)
		{
			return NodeCastResult.Success;
		}
		if ((int)casterItem.counter > 0)
		{
			Effect.EnqueueAction(eve, new ChangeCounterAction(casterItem.counter, casterItem, eve.CasterItem, withPopupText: true));
		}
		if (casterItem.owner != itemContainer || casterItem.ownerSlotIndex != 0)
		{
			Effect.EnqueueAction(eve, new TransferItemAction(casterItem, itemContainer, eve.CasterItem, TransferItemAction.TransferAnimType.Transfer, () => 0));
		}
		return NodeCastResult.Success;
	}
}
