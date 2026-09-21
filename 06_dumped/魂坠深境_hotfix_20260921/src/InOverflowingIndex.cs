public class InOverflowingIndex : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		return item.IsOverflow;
	}
}
