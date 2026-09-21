using System.Collections.Generic;
using UnityEngine;

public class ItemCounterGenerator : ValueGenerator
{
	[SerializeField]
	public ItemTargetSelector selector;

	public override int? GenerateValue(GameEvent eve)
	{
		int num = 0;
		List<Item> targetItems = selector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
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
		return new List<IEffectNode> { selector };
	}
}
