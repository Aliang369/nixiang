public class ItemInContainerType : ItemConstraint
{
	public ItemContainer.ContainerType type;

	public override bool? CheckItemAccept(Item item)
	{
		return item.owner != null && item.owner.type == type;
	}
}
