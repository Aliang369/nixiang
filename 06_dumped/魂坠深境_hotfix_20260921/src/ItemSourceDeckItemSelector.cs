using System.Collections.Generic;
using UnityEngine;

public class ItemSourceDeckItemSelector : ItemTargetSelector
{
	[SerializeField]
	private ItemTargetSelector target;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		List<Item> list = new List<Item>();
		foreach (Item item in targetItems)
		{
			Item sourceDeckItem = item.sourceDeckItem;
			if (sourceDeckItem != null)
			{
				list.Add(sourceDeckItem);
			}
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target };
	}
}
