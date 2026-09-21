public class WithStackedRune : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		if (item == null)
		{
			return null;
		}
		return item.Runes.Find((Rune r) => r.def.stacking) != null;
	}
}
