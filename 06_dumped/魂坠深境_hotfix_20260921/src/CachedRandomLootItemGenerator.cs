using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CachedRandomLootItemGenerator : NewItemGenerator
{
	[SerializeField]
	private string key = "AttachItemDefs";

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		return GetOrCreateCachedLootItemDefs(eve)?.FindAll((ItemDefinition def) => def != null).ConvertAll((ItemDefinition def) => new Item(def));
	}

	private List<ItemDefinition> GetOrCreateCachedLootItemDefs(GameEvent eve)
	{
		if (eve == null)
		{
			return null;
		}
		Dictionary<string, object> dictionary = (TempValue.TryGetDictionary(eve, TempValue.Type.Caster, out var dictionary2) ? dictionary2 : eve.nodeTempValues);
		if (dictionary.TryGetValue(key, out var value))
		{
			if (value is List<ItemDefinition> result)
			{
				return result;
			}
			if (value is ItemDefinition item)
			{
				return new List<ItemDefinition> { item };
			}
			if (value is List<Item> list)
			{
				return list.ConvertAll((Item item2) => item2?.GetDef());
			}
		}
		ItemDefinition itemDefinition = (eve.gameState?.generationManager?.GenerateRandomLoot(1))?.FirstOrDefault()?.GetDef();
		if (itemDefinition == null)
		{
			return new List<ItemDefinition>();
		}
		List<ItemDefinition> list2 = new List<ItemDefinition> { itemDefinition };
		dictionary[key] = list2;
		return list2;
	}
}
