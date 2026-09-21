using System.Collections.Generic;
using UnityEngine;

public class ItemCurrentDefendCounterGenerator : ValueGenerator
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
			if (item != null)
			{
				num += item.GetCurrentDefendCounter();
			}
		}
		return num;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
