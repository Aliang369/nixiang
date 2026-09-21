public class TransformAction : ExecuteAction
{
	private readonly Item targetItem;

	private readonly Item sourceItem;

	public TransformAction(Item targetItem, Item sourceItem = null)
	{
		this.targetItem = targetItem;
		this.sourceItem = sourceItem;
	}

	protected override void Update()
	{
		if (targetItem != null && targetItem.HasTransformTemplate && !targetItem.transformActive)
		{
			targetItem.transformActive = true;
			gameState.gameEventManager.TriggerAfterTransform(sourceItem, targetItem);
		}
	}
}
