using System.Collections.Generic;
using UnityEngine;

public class ItemSpellCountGenerator : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector targetSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		Item singleItem = targetSelector.GetSingleItem(eve);
		if (singleItem == null)
		{
			return null;
		}
		return singleItem.itemSpells?.Count ?? 0;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
