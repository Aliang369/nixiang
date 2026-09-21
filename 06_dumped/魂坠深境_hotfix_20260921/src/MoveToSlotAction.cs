public class MoveToSlotAction : ExecuteAction
{
	private readonly Item item;

	private readonly object source;

	private bool started;

	private readonly int targetPos;

	public MoveToSlotAction(int targetPos, Item item, object source)
	{
		this.targetPos = targetPos;
		this.item = item;
		this.source = source;
	}

	protected override void Update()
	{
		ItemContainer itemContainer = item?.owner;
		if (itemContainer != null && !item.BlocksExternalMove(source, gameState))
		{
			int rawIndex = item.owner.GetRawIndex(item);
			int deltaPos = EternalItemRules.ClampNonEternalTargetIndex(item, targetPos) - rawIndex;
			itemContainer.MoveItem(item, deltaPos, source);
		}
	}
}
