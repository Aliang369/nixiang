public class ItemWithMaxChapter : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		return (int)item.maxCounter > 0;
	}
}
