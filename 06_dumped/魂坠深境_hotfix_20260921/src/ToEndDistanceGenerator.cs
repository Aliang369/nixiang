using System.Collections.Generic;
using UnityEngine;

public class ToEndDistanceGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType bftType;

	[SerializeField]
	private GameEvent.ContainerType containerType;

	[SerializeField]
	private ItemTargetSelector targetSelector;

	[SerializeField]
	private bool ignoreCost;

	public override int? GenerateValue(GameEvent eve)
	{
		Item singleItem = targetSelector.GetSingleItem(eve);
		if (singleItem == null || !singleItem.ownerSlotIndex.HasValue)
		{
			return null;
		}
		int value = singleItem.ownerSlotIndex.Value;
		ItemContainer ownerContainer = eve.GetOwnerContainer(bftType, containerType);
		if (ownerContainer == null || ownerContainer != singleItem.owner)
		{
			return null;
		}
		List<Item> allItems = ownerContainer.GetAllItems();
		int num = allItems.Count - 1;
		int num2 = num - value;
		if (ignoreCost)
		{
			for (int i = value; i < num; i++)
			{
				if (eve.gameState.playerState.playerSpellInfo.addedCosts.Contains(allItems[i]))
				{
					num2--;
				}
			}
		}
		return num2;
	}
}
