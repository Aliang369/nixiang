using UnityEngine;

public class ItemSlotIndexGenerator : ValueGenerator
{
	[SerializeField]
	private readonly bool addone;

	[SerializeField]
	private GameEvent.EventItemType itemType;

	[SerializeField]
	private readonly bool onlyInTable = true;

	[SerializeField]
	private ItemTargetSelector targetSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		Item item = ((targetSelector == null) ? eve.GetEventItem(itemType) : targetSelector?.GetSingleItem(eve));
		if (item == null || !item.ownerSlotIndex.HasValue)
		{
			return null;
		}
		if (onlyInTable && (item.owner == null || item.owner.type != ItemContainer.ContainerType.Table))
		{
			return null;
		}
		return item.ownerSlotIndex + (addone ? 1 : 0);
	}
}
