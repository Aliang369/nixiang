using System;

internal class DrawSpecificItemFromPileAction : ExecuteAction
{
	private readonly BattleFieldTransform bft;

	private readonly Item mustDrawItem;

	private readonly Item sourceItem;

	private readonly Action<Item> onDrawCompleted;

	private readonly int insertIndex;

	public DrawSpecificItemFromPileAction(BattleFieldTransform bft, Item mustDrawItem, Item sourceItem, Action<Item> onDrawCompleted = null, int insertIndex = 0)
	{
		this.bft = bft;
		this.mustDrawItem = mustDrawItem;
		this.sourceItem = sourceItem;
		this.onDrawCompleted = onDrawCompleted;
		this.insertIndex = insertIndex;
		Ignore(ActionLock.Type.DrawFromPile);
	}

	protected override void Update()
	{
		if (mustDrawItem == null)
		{
			onDrawCompleted?.Invoke(null);
			FinishAction();
			return;
		}
		if (!bft.tableContainer.CanAcceptItemWithinHardLimit(mustDrawItem))
		{
			gameState.actionManager.ShowTableItemHardLimitHintOnce();
			onDrawCompleted?.Invoke(null);
			FinishAction();
			return;
		}
		EnqueueAction(new TransferItemAction(mustDrawItem, bft.tableContainer, null, TransferItemAction.TransferAnimType.Draw, () => insertIndex, onDrawCompleted));
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerDrawFromPile(mustDrawItem, sourceItem);
		});
	}
}
