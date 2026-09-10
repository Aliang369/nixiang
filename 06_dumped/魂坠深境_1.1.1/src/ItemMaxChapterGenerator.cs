using System.Collections.Generic;
using UnityEngine;

public class ItemMaxChapterGenerator : ValueGenerator
{
	[SerializeField]
	public ItemTargetSelector selector;

	public override int? GenerateValue(GameEvent eve)
	{
		Item item = selector?.GetSingleItem(eve);
		if (item == null)
		{
			return null;
		}
		return item.maxCounter;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
