public class DowngradeItemAction : ExecuteAction
{
	public Item source;

	public Item target;

	public DowngradeItemAction(Item target, Item source)
	{
		this.target = target;
		this.source = source;
	}

	protected override void Update()
	{
		if (target.upgradeLevel > 0)
		{
			target.Downgrade(gameState);
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerItemDowngrade(target, source);
			});
		}
	}
}
