using System.Collections.Generic;
using UnityEngine;

public class ItemsSumCounter : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector itemSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		List<Item> targetItems = itemSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		int num = 0;
		foreach (Item item in targetItems)
		{
			if (item.withCounter)
			{
				num += (int)item.counter;
			}
		}
		return num;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemSelector };
	}
}
