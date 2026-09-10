using System.Collections.Generic;
using UnityEngine;

public class DeltaSlotIndexItemSelector : ItemTargetSelector
{
	[SerializeField]
	private ValueGenerator delta;

	[SerializeField]
	private bool nearest;

	[SerializeField]
	private GameEvent.EventItemType pivot;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(pivot);
		if (eventItem == null || !eventItem.ownerSlotIndex.HasValue)
		{
			return null;
		}
		if (eventItem.owner == null)
		{
			return new List<Item>();
		}
		int num = 0;
		if (delta != null)
		{
			int? num2 = delta.GenerateValue(eve);
			if (!num2.HasValue)
			{
				return null;
			}
			num = num2.Value;
		}
		int value = eventItem.ownerSlotIndex.Value;
		int num3 = value + num;
		Item item = null;
		foreach (Item allItem in eventItem.owner.GetAllItems())
		{
			if (nearest)
			{
				if (item == null)
				{
					item = allItem;
					continue;
				}
				int num4 = value;
				if (Mathf.Abs(value - num3) < Mathf.Abs(num4 - num3))
				{
					item = allItem;
				}
			}
			else if (num3 == value)
			{
				item = allItem;
			}
		}
		return new List<Item> { item };
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { delta };
	}
}
