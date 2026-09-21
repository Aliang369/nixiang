public class ItemDefinitionTagConstraint : ItemTagConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		if (itemTag == null)
		{
			return true;
		}
		if (item == null || item.GetDef() == null)
		{
			return false;
		}
		return item.GetDef().HasTag(itemTag);
	}
}
