using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultiItemTargetSelector : ItemTargetSelector
{
	[SerializeField]
	private bool removeRepeat;

	[SerializeField]
	private List<ItemTargetSelector> selectors;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		foreach (ItemTargetSelector selector in selectors)
		{
			List<Item> targetItems = selector.GetTargetItems(eve);
			if (targetItems == null)
			{
				return null;
			}
			list.AddRange(targetItems);
		}
		if (removeRepeat)
		{
			list = list.Distinct().ToList();
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (selectors != null)
		{
			list.AddRange(selectors);
		}
		return list;
	}
}
