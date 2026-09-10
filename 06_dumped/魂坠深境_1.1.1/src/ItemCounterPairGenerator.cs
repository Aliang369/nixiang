using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemCounterPairGenerator : NewItemGenerator
{
	public struct ItemCounterPair
	{
		public ItemDefinition item;

		public int counter;
	}

	[SerializeField]
	private List<ItemCounterPair> itemCounterPairs;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		foreach (ItemCounterPair itemCounterPair in itemCounterPairs)
		{
			if (itemCounterPair.item == null)
			{
				throw new NullReferenceException("pair.item == null");
			}
			Item item = new Item(itemCounterPair.item);
			item.counter = itemCounterPair.counter;
			list.Add(item);
		}
		return list;
	}
}
