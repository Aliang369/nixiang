public class SwapSlotItemAction : ExecuteAction
{
	private readonly Item itemA;

	private readonly Item itemB;

	private readonly Item source;

	public SwapSlotItemAction(Item itemA, Item itemB, Item source)
	{
		this.itemA = itemA;
		this.itemB = itemB;
		this.source = source;
	}

	protected override void Update()
	{
		if (itemA?.owner != null && itemB?.owner != null && itemA.owner == itemB.owner && !itemA.IsEternal() && !itemB.IsEternal())
		{
			itemA.owner.SwapItem(itemA, itemB, source);
		}
	}
}
