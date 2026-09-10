using System.Collections.Generic;
using UnityEngine;

public class ItemChapterGenerator : ValueGenerator
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
		if ((int)item.maxCounter == 0)
		{
			return item.counter;
		}
		return Mathf.Clamp(item.counter, 0, item.maxCounter);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
