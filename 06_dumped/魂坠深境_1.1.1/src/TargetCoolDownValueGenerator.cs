using System.Collections.Generic;
using UnityEngine;

public class TargetCoolDownValueGenerator : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector selector;

	public override int? GenerateValue(GameEvent eve)
	{
		Item item = selector?.GetSingleItem(eve);
		if (item == null)
		{
			return null;
		}
		return item.cooldown;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
