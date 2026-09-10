using System.Collections.Generic;
using UnityEngine;

public class ItemWithCounterGenerator : NewItemGenerator
{
	public struct ItemCounter
	{
		public int counter;

		public ItemDefinition def;
	}

	[SerializeField]
	private List<ItemCounter> itemCounters;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		if (itemCounters == null)
		{
			return list;
		}
		foreach (ItemCounter itemCounter in itemCounters)
		{
			if (!(itemCounter.def == null))
			{
				Item item = new Item(itemCounter.def);
				item.counter = itemCounter.counter;
				list.Add(item);
			}
		}
		return list;
	}
}
