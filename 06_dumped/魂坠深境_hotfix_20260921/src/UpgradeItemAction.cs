public class UpgradeItemAction : ExecuteAction
{
	public Item source;

	public Item target;

	public UpgradeItemAction(Item target, Item source)
	{
		this.target = target;
		this.source = source;
	}

	protected override void Update()
	{
		if (target == null)
		{
			return;
		}
		target.Upgrade(gameState);
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerItemUpgrade(target, source);
		});
		ItemContainer owner = target.owner;
		if (owner != null && owner.type == ItemContainer.ContainerType.Deck)
		{
			BattleFieldTransform ownerBft = owner.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.recordManager.TrackJournalRecord(new UpgradeDeckItemRecord(target));
			}
		}
	}
}
