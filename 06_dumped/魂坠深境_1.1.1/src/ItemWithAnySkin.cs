public class ItemWithAnySkin : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		return item.skin != null;
	}
}
