using System.Collections.Generic;
using UnityEngine;

public class TargetBaseSpellCooldownValueGenerator : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector selector;

	public override int? GenerateValue(GameEvent eve)
	{
		List<Item> list = selector?.GetTargetItems(eve);
		if (list == null || list.Count != 1)
		{
			return null;
		}
		Item item = list[0];
		if (item?.BaseSpell == null)
		{
			return null;
		}
		return item.BaseSpell.cooldown;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
