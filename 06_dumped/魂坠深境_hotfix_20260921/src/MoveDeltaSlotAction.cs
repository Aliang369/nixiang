public class MoveDeltaSlotAction : ExecuteAction
{
	private readonly int deltaPos;

	private readonly Item item;

	private readonly object source;

	public MoveDeltaSlotAction(int deltaPos, Item item, object source)
	{
		this.deltaPos = deltaPos;
		this.item = item;
		this.source = source;
	}

	protected override void Update()
	{
		ItemContainer itemContainer = item?.owner;
		if (itemContainer != null && !item.BlocksExternalMove(source, gameState))
		{
			int rawIndex = itemContainer.GetRawIndex(item);
			int num = EternalItemRules.ClampNonEternalTargetIndex(item, rawIndex + deltaPos);
			itemContainer.MoveItem(item, num - rawIndex, source);
		}
	}
}
