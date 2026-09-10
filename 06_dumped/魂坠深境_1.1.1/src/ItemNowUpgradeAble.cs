public class ItemNowUpgradeAble : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		return item?.Upgradeable;
	}
}
