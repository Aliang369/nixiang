using System.Collections.Generic;

public static class DancerMovePathResolver
{
	public static bool IsPlayerActiveMoveEvent(GameEvent eve, BattleFieldTransform owner)
	{
		if (eve == null || owner == null || (eve.type != GameEvent.Type.MoveSlotIndex && eve.type != GameEvent.Type.AfterSwapItem))
		{
			return false;
		}
		RealSpellCastInfo realSpellCastInfo = eve.gameState?.playerState?.playerSpellInfo;
		if (realSpellCastInfo?.spellItem == null || realSpellCastInfo.casterType != SpellCastInfo.CasterType.Player || realSpellCastInfo.castState != SpellCastInfo.State.Casting || realSpellCastInfo.isTriggerCast)
		{
			return false;
		}
		if (((eve.type == GameEvent.Type.AfterSwapItem) ? eve.SwapSourceItem : eve.SourceItem) == realSpellCastInfo.spellItem && realSpellCastInfo.spellItem.OwnerBft == owner)
		{
			return eve.CasterOwner == owner;
		}
		return false;
	}

	public static bool TryGetPassedItems(GameEvent eve, Item movedItem, out List<Item> passedItems, out int beforeIndex, out int afterIndex)
	{
		passedItems = new List<Item>();
		beforeIndex = -1;
		afterIndex = -1;
		if (eve == null || movedItem == null)
		{
			return false;
		}
		if (eve.type == GameEvent.Type.MoveSlotIndex)
		{
			if (eve.TargetItem != movedItem)
			{
				return false;
			}
			beforeIndex = eve.amount - eve.deltaValue;
			afterIndex = eve.amount;
			return TryResolveDirectMovePath(movedItem.owner, movedItem, beforeIndex, afterIndex, passedItems);
		}
		if (eve.type != GameEvent.Type.AfterSwapItem || eve.OriginContainer == null)
		{
			return false;
		}
		Item destinationItem;
		if (eve.SourceItem == movedItem)
		{
			beforeIndex = eve.amount;
			afterIndex = eve.amount2;
			destinationItem = eve.TargetItem;
		}
		else
		{
			if (eve.TargetItem != movedItem)
			{
				return false;
			}
			beforeIndex = eve.amount2;
			afterIndex = eve.amount;
			destinationItem = eve.SourceItem;
		}
		return TryResolveSwapPath(eve.OriginContainer, movedItem, destinationItem, beforeIndex, afterIndex, passedItems);
	}

	private static bool TryResolveDirectMovePath(ItemContainer container, Item movedItem, int beforeIndex, int afterIndex, List<Item> passedItems)
	{
		if (!IndicesAreValid(container, beforeIndex, afterIndex) || container.items[afterIndex] != movedItem)
		{
			return false;
		}
		if (afterIndex > beforeIndex)
		{
			for (int i = beforeIndex; i < afterIndex; i++)
			{
				AddPassedItem(passedItems, container.items[i], movedItem);
			}
		}
		else
		{
			for (int num = beforeIndex; num > afterIndex; num--)
			{
				AddPassedItem(passedItems, container.items[num], movedItem);
			}
		}
		return true;
	}

	private static bool TryResolveSwapPath(ItemContainer container, Item movedItem, Item destinationItem, int beforeIndex, int afterIndex, List<Item> passedItems)
	{
		if (destinationItem == null || !IndicesAreValid(container, beforeIndex, afterIndex) || container.items[afterIndex] != movedItem || container.items[beforeIndex] != destinationItem)
		{
			return false;
		}
		if (afterIndex > beforeIndex)
		{
			for (int i = beforeIndex + 1; i < afterIndex; i++)
			{
				AddPassedItem(passedItems, container.items[i], movedItem);
			}
		}
		else
		{
			for (int num = beforeIndex - 1; num > afterIndex; num--)
			{
				AddPassedItem(passedItems, container.items[num], movedItem);
			}
		}
		AddPassedItem(passedItems, destinationItem, movedItem);
		return true;
	}

	private static bool IndicesAreValid(ItemContainer container, int beforeIndex, int afterIndex)
	{
		if (container?.items != null && beforeIndex >= 0 && beforeIndex < container.items.Count && afterIndex >= 0 && afterIndex < container.items.Count)
		{
			return beforeIndex != afterIndex;
		}
		return false;
	}

	private static void AddPassedItem(List<Item> passedItems, Item item, Item movedItem)
	{
		if (item != null && item != movedItem)
		{
			passedItems.Add(item);
		}
	}
}
