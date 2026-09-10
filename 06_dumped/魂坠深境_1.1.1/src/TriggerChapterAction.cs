public class TriggerChapterAction : ExecuteAction
{
	public Item targetItem;

	public TriggerChapterAction(Item targetItem)
	{
		this.targetItem = targetItem;
	}

	protected override void Update()
	{
		gameState.gameEventManager.TriggerTriggerChapterEffect(targetItem);
	}
}
