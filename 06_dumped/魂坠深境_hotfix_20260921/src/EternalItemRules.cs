using System;
using System.Collections.Generic;
using System.Linq;

public static class EternalItemRules
{
	public static bool IsEternal(this Item item)
	{
		if (item == null)
		{
			return false;
		}
		return item.Enchants?.Any((Enchant enchant) => enchant != null && enchant.def?.attachAbility?.Any((Ability ability) => ability is EternalAbility) == true) == true;
	}

	public static bool BlocksExternalAction(this Item item, object source)
	{
		return item.BlocksExternalAction(source, ExternalItemActionBlockType.Generic);
	}

	public static bool BlocksExternalMove(this Item item, object source, GameStateInfo gameState)
	{
		return item.BlocksExternalAction(source, ExternalItemActionBlockType.Move, gameState);
	}

	private static bool BlocksExternalAction(this Item item, object source, ExternalItemActionBlockType actionType, GameStateInfo gameState = null)
	{
		if (item == null)
		{
			return false;
		}
		foreach (Enchant item2 in item.Enchants.OrEmptyIfNull())
		{
			foreach (Ability item3 in item2?.def?.attachAbility.OrEmptyIfNull())
			{
				if (item3 is IExternalItemActionBlockerAbility externalItemActionBlockerAbility && externalItemActionBlockerAbility.BlocksExternalItemAction(item, item2, source, actionType))
				{
					externalItemActionBlockerAbility.OnExternalItemActionBlocked(gameState, item, item2, source, actionType);
					return true;
				}
			}
		}
		if (item.IsEternal())
		{
			ItemContainer owner = item.owner;
			if (owner != null && owner.type == ItemContainer.ContainerType.Table)
			{
				return source != item;
			}
		}
		return false;
	}

	public static bool BlocksExternalExile(this Item item, object source, ExileItemAction.ExileType exileType)
	{
		if (item.BlocksExternalAction(source))
		{
			return true;
		}
		if (exileType == ExileItemAction.ExileType.Permanent && item.IsEternal())
		{
			ItemContainer owner = item.owner;
			if (owner != null && owner.IsBattleContainer())
			{
				return source != item;
			}
		}
		return false;
	}

	public static bool IsActionFrontAtEnd(ItemContainer container)
	{
		if (container != null && container.type == ItemContainer.ContainerType.Table)
		{
			return container.OwnerBft?.isEnemy ?? false;
		}
		return false;
	}

	public static int GetEternalFrontCount(ItemContainer container)
	{
		if (container == null || container.type != ItemContainer.ContainerType.Table)
		{
			return 0;
		}
		List<Item> allItems = container.GetAllItems();
		if (!IsActionFrontAtEnd(container))
		{
			return allItems.TakeWhile((Item item) => item.IsEternal()).Count();
		}
		return allItems.AsEnumerable().Reverse().TakeWhile((Item item) => item.IsEternal())
			.Count();
	}

	public static int ResolveTableInsertIndex(ItemContainer container, Item item, int requestedIndex)
	{
		if (container == null || container.type != ItemContainer.ContainerType.Table || item == null)
		{
			return requestedIndex;
		}
		int itemsCount = container.ItemsCount;
		if (item.IsEternal())
		{
			if (!IsActionFrontAtEnd(container))
			{
				return 0;
			}
			return itemsCount;
		}
		if (requestedIndex < 0 || requestedIndex > itemsCount)
		{
			requestedIndex = itemsCount;
		}
		int eternalFrontCount = GetEternalFrontCount(container);
		if (!IsActionFrontAtEnd(container))
		{
			return Math.Max(requestedIndex, eternalFrontCount);
		}
		return Math.Min(requestedIndex, itemsCount - eternalFrontCount);
	}

	public static int ClampNonEternalTargetIndex(Item item, int targetIndex)
	{
		ItemContainer itemContainer = item?.owner;
		if (itemContainer == null || itemContainer.type != ItemContainer.ContainerType.Table || item.IsEternal())
		{
			return targetIndex;
		}
		int val = Math.Clamp(targetIndex, 0, itemContainer.ItemsCount - 1);
		int eternalFrontCount = GetEternalFrontCount(itemContainer);
		if (!IsActionFrontAtEnd(itemContainer))
		{
			return Math.Max(val, eternalFrontCount);
		}
		return Math.Min(val, itemContainer.ItemsCount - eternalFrontCount - 1);
	}
}
