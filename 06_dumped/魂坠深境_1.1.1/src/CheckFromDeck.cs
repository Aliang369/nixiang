using System.Collections.Generic;
using UnityEngine;

public class CheckFromDeck : CheckNode
{
	[SerializeField]
	private ItemTargetSelector target;

	public override bool? Check(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		foreach (Item item in targetItems)
		{
			if (!item.FromDeck)
			{
				return false;
			}
		}
		return true;
	}
}
