using System.Collections.Generic;
using UnityEngine;

public class SelectedItemResetInitCounterValueGenerator : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector selector;

	public override int? GenerateValue(GameEvent eve)
	{
		List<Item> list = selector?.GetTargetItems(eve);
		if (list == null || list.Count != 1 || list[0] == null)
		{
			return null;
		}
		return list[0].resetInitCounter;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
