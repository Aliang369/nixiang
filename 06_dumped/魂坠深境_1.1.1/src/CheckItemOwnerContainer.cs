public class CheckItemOwnerContainer : CheckNode
{
	public ItemTargetSelector itemSelector;

	public ItemContainer.ContainerType containerType;

	public override bool? Check(GameEvent eve)
	{
		Item item = itemSelector?.GetSingleItem(eve);
		if (item?.owner == null)
		{
			return null;
		}
		return item.owner.type == containerType;
	}
}
