internal sealed class AdvanceGeneratedChapterAction : ExecuteAction
{
	private readonly int remainingAdvances;

	private readonly Item sourceItem;

	private readonly ItemTag chapterTag;

	private readonly Item targetItem;

	private readonly ItemContainer targetContainer;

	public AdvanceGeneratedChapterAction(Item targetItem, Item sourceItem, ItemContainer targetContainer, ItemTag chapterTag, int remainingAdvances)
	{
		this.targetItem = targetItem;
		this.sourceItem = sourceItem;
		this.targetContainer = targetContainer;
		this.chapterTag = chapterTag;
		this.remainingAdvances = remainingAdvances;
	}

	protected override void Update()
	{
		if (remainingAdvances <= 0 || targetItem?.owner != targetContainer)
		{
			return;
		}
		ItemContainer itemContainer = targetContainer;
		if (itemContainer != null && itemContainer.type == ItemContainer.ContainerType.Table && targetItem.HasTag(chapterTag) && (int)targetItem.maxCounter > 0 && (int)targetItem.counter < (int)targetItem.maxCounter)
		{
			GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerAddChapterResolve(targetItem, sourceItem, GameEvent.TriggerType.Real);
			if (!resolveInfo.reject)
			{
				EnqueueAction(new ChangeCounterAction(resolveInfo.targetDelta + 1, targetItem, sourceItem, withPopupText: true));
			}
			EnqueueAction(new AdvanceGeneratedChapterAction(targetItem, sourceItem, targetContainer, chapterTag, remainingAdvances - 1));
		}
	}
}
