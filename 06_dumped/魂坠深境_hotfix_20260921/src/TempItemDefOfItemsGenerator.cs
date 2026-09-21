using System.Collections.Generic;
using UnityEngine;

public class TempItemDefOfItemsGenerator : ItemDefGenerator
{
	[SerializeField]
	private ItemTargetSelector targetSelector;

	private string key;

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (Item item in targetItems)
		{
			Dictionary<string, object> tempValues = item.tempValues;
			if (tempValues != null && tempValues.TryGetValue(key) is List<ItemDefinition> collection)
			{
				list.AddRange(collection);
			}
		}
		return list;
	}
}
