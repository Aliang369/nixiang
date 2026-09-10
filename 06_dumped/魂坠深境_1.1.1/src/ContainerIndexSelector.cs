using System.Collections.Generic;
using UnityEngine;

public class ContainerIndexSelector : ItemTargetSelector
{
	[SerializeField]
	private readonly bool closest = true;

	[SerializeField]
	private ValueGenerator indexGenerator;

	[SerializeField]
	private GameEvent.EventBFTType owner;

	[SerializeField]
	private GameEvent.ContainerType type;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		ItemContainer ownerContainer = eve.GetOwnerContainer(owner, type);
		if (ownerContainer == null)
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
		Item item = null;
		foreach (Item allItem in ownerContainer.GetAllItems())
		{
			if (closest)
			{
				if (item == null)
				{
					item = allItem;
					continue;
				}
				int? ownerSlotIndex = item.ownerSlotIndex;
				int? ownerSlotIndex2 = allItem.ownerSlotIndex;
				if (ownerSlotIndex.HasValue && ownerSlotIndex2.HasValue && Mathf.Abs(ownerSlotIndex.Value - num) > Mathf.Abs(ownerSlotIndex2.Value - num))
				{
					item = allItem;
				}
			}
			else if (allItem.ownerSlotIndex == num)
			{
				item = allItem;
			}
		}
		if (item == null)
		{
			return new List<Item>();
		}
		return new List<Item> { item };
	}
}
