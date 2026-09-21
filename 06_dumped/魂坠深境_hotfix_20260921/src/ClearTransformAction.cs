public class ClearTransformAction : ExecuteAction
{
	private readonly Item targetItem;

	public ClearTransformAction(Item targetItem)
	{
		this.targetItem = targetItem;
	}

	protected override void Update()
	{
		if (targetItem != null && targetItem.transformActive)
		{
			targetItem.transformActive = false;
		}
	}
}
