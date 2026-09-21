public static class CharacterBlessingItemFilters
{
	public static bool IsDeckArmor(Item item)
	{
		if (item?.owner != null && item.owner.type == ItemContainer.ContainerType.Deck)
		{
			return item.DefenceLike;
		}
		return false;
	}
}
