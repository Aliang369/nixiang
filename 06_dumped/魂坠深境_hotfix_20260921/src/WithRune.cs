public class WithRune : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		if (item == null)
		{
			return null;
		}
		return item.Runes.Count > 0;
	}
}
