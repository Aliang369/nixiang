public class MoveHeadToCasterRightAction : ExecuteAction
{
	private readonly Item casterItem;

	public MoveHeadToCasterRightAction(Item casterItem)
	{
		this.casterItem = casterItem;
	}

	protected override void Update()
	{
		ItemContainer itemContainer = casterItem?.owner;
		if (itemContainer == null || itemContainer.type != ItemContainer.ContainerType.Table)
		{
			return;
		}
		int rawIndex = itemContainer.GetRawIndex(casterItem);
		if (rawIndex > 0)
		{
			Item item = itemContainer.items[rawIndex - 1];
			if (item != null && item.owner == itemContainer)
			{
				item.owner.MoveItem(item, 1, casterItem);
			}
		}
	}
}
