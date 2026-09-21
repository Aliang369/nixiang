using System.Collections.Generic;
using UnityEngine;

public class NeighbourItemSelector : ItemTargetSelector
{
	[SerializeField]
	private ItemTargetSelector target;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		List<Item> list = new List<Item>();
		foreach (Item item in targetItems)
		{
			int? index = item.ownerSlotIndex;
			if (index.HasValue && targetItems.Find((Item i) => i.ownerSlotIndex.HasValue && Mathf.Abs(i.ownerSlotIndex.Value - index.Value) == 1) != null)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target };
	}
}
