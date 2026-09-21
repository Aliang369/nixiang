public class ItemUpgraded : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		return item?.upgraded;
	}
}
