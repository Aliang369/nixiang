using System.Collections.Generic;
using System.Linq;

public static class EnemyDeckReconcileRules
{
	public sealed class RestorePlan
	{
		public List<Item> ForeignItems { get; } = new List<Item>();

		public List<Item> MissingSources { get; } = new List<Item>();
	}

	public static List<Item> GetConfiguredSources(BattleFieldTransform bft, bool includePhase2)
	{
		List<Item> list = new List<Item>();
		if (bft?.deckContainer != null)
		{
			list.AddRange(bft.deckContainer.GetAllItems());
		}
		if (includePhase2 && bft?.characterAI?.phase2Items != null)
		{
			list.AddRange(bft.characterAI.phase2Items);
		}
		return list.Where((Item item) => item != null).Distinct().ToList();
	}

	public static List<Item> GetActiveCycleItems(BattleFieldTransform bft)
	{
		List<Item> list = new List<Item>();
		AddContainerItems(bft?.tableContainer, list);
		AddContainerItems(bft?.drawPileItemContainer, list);
		AddContainerItems(bft?.wastePileItemContainer, list);
		return list;
	}

	public static bool IsRuntimeFromSource(Item runtimeItem, Item sourceItem)
	{
		if (runtimeItem != null && sourceItem != null)
		{
			if (runtimeItem != sourceItem)
			{
				return runtimeItem.sourceDeckItem == sourceItem;
			}
			return true;
		}
		return false;
	}

	public static bool IsRuntimeFromAnySource(Item runtimeItem, IEnumerable<Item> sourceItems)
	{
		return sourceItems?.Any((Item source) => IsRuntimeFromSource(runtimeItem, source)) ?? false;
	}

	public static List<Item> GetMissingReplenishableSources(IEnumerable<Item> sourceItems, IEnumerable<Item> activeItems)
	{
		List<Item> active = activeItems?.Where((Item item) => item != null).ToList() ?? new List<Item>();
		return sourceItems?.Where(delegate(Item source)
		{
			if (source != null)
			{
				ItemBlackBoard blackboard = source.blackboard;
				if (blackboard == null)
				{
					return true;
				}
				return !blackboard.StayInTable;
			}
			return false;
		}).Distinct().Where((Item source) => active.All((Item runtime) => !IsRuntimeFromSource(runtime, source)))
			.ToList() ?? new List<Item>();
	}

	public static RestorePlan BuildRestorePlan(IEnumerable<Item> sourceItems, IEnumerable<Item> activeItems)
	{
		List<Item> sources = sourceItems?.Where((Item item) => item != null).Distinct().ToList() ?? new List<Item>();
		List<Item> list = activeItems?.Where((Item item) => item != null).ToList() ?? new List<Item>();
		RestorePlan restorePlan = new RestorePlan();
		restorePlan.ForeignItems.AddRange(list.Where((Item item) => !IsRuntimeFromAnySource(item, sources)));
		restorePlan.MissingSources.AddRange(GetMissingReplenishableSources(sources, list));
		return restorePlan;
	}

	private static void AddContainerItems(ItemContainer container, ICollection<Item> destination)
	{
		if (container == null)
		{
			return;
		}
		foreach (Item allItem in container.GetAllItems())
		{
			destination.Add(allItem);
		}
	}
}
