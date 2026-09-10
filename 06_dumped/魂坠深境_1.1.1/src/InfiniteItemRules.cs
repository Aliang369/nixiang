using System.Linq;

public static class InfiniteItemRules
{
	public static bool IsInfinite(this Item item)
	{
		if (item == null)
		{
			return false;
		}
		return item.GetDef()?.abilities?.Any((Ability ability) => ability is InfiniteAbility) == true;
	}

	public static bool IsPrimaryInfiniteSource(this Item item)
	{
		ItemContainer itemContainer = item?.OwnerBft?.tableContainer;
		if (itemContainer == null || item.owner != itemContainer)
		{
			return false;
		}
		return itemContainer.GetAllItems(orderReverse: true).FirstOrDefault((Item candidate) => candidate.IsInfinite()) == item;
	}
}
