using System.Collections.Generic;
using UnityEngine;

public class InstantiateItemDefItemGenerator : NewItemGenerator
{
	[SerializeField]
	private ValueGenerator initCounterGenerator;

	[SerializeField]
	private ItemDefinition itemDef;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		if (itemDef == null)
		{
			return null;
		}
		Item item = new Item(itemDef);
		if (initCounterGenerator != null)
		{
			int? num = initCounterGenerator?.GenerateValue(eve);
			if (!num.HasValue)
			{
				return null;
			}
			item.counter = num.Value;
		}
		return new List<Item> { item };
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { initCounterGenerator };
	}
}
