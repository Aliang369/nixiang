public class WithAddItemsNode : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		return item.GetDescendantNodeOfType<SpawnItems>().Count > 0;
	}
}
