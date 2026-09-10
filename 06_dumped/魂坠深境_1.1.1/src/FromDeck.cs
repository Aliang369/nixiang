public class FromDeck : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		return item.FromDeck;
	}
}
