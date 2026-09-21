using System.Collections.Generic;
using UnityEngine;

public class AllBftDamageAbleTargetSelector : ItemTargetSelector
{
	[SerializeField]
	private GameEvent.EventBFTType bft;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> obj = eve?.GetEventBFT(bft)?.tableContainer.GetAllItems();
		List<Item> list = new List<Item>();
		foreach (Item item in obj)
		{
			if (item != null && item.DamageAble)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
