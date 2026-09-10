using System.Collections.Generic;
using UnityEngine;

public class IndexItemOfItems : ItemTargetSelector
{
	[SerializeField]
	private bool closest;

	[SerializeField]
	private ItemTargetSelector targetSelector;

	[SerializeField]
	private ValueGenerator indexGenerator;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return null;
		}
		int num = 0;
		if (indexGenerator != null)
		{
			int? num2 = indexGenerator.GenerateValue(eve);
			if (!num2.HasValue)
			{
				return null;
			}
			num = num2.Value;
		}
		if (closest)
		{
			if (num < 0)
			{
				num = 0;
			}
			if (list.Count > 0 && num >= list.Count)
			{
				num = list.Count - 1;
			}
		}
		if (list == null || num < 0 || num >= list.Count)
		{
			return new List<Item>();
		}
		return new List<Item> { list[num] };
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, indexGenerator };
	}
}
