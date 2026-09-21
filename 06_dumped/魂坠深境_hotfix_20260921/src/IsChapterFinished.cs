public class IsChapterFinished : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		return (int)item.counter == (int)item.maxCounter;
	}
}
