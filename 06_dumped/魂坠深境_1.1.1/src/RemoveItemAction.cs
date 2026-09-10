public class RemoveItemAction : ExecuteAction
{
	public object source;

	public Item targetItem;

	public RemoveItemAction(Item targetItem, object source)
	{
		this.targetItem = targetItem;
		this.source = source;
	}

	protected override void Update()
	{
		Item item = targetItem;
		ItemContainer container = targetItem?.owner;
		if (targetItem == null || container == null || targetItem.BlocksExternalAction(source))
		{
			return;
		}
		gameState.gameEventManager.TriggerBeforeRemoveItem(targetItem, source, container);
		EnqueueAction(new ActionAction(delegate
		{
			container.RemoveItem(item, source, clean: false);
		}));
		if (container != null && container.type == ItemContainer.ContainerType.Deck)
		{
			BattleFieldTransform ownerBft = container.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.recordManager.TrackJournalRecord(new RemoveFromDeckRecord(item));
			}
		}
	}
}
