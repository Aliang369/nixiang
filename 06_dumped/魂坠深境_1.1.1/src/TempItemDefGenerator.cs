using System;
using System.Collections.Generic;
using UnityEngine;

public class TempItemDefGenerator : ItemDefGenerator
{
	[SerializeField]
	private TempValue.Type type;

	private string key;

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		if (type == TempValue.Type.GameGlobal)
		{
			throw new Exception("Global itemDefinition is not supported.");
		}
		return GeneratorNormalItemDefs(eve);
	}

	private List<ItemDefinition> GeneratorNormalItemDefs(GameEvent eve)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue(key) is List<ItemDefinition> result)
		{
			return result;
		}
		return null;
	}
}
