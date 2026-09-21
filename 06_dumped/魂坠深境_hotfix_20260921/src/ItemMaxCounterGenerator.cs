using UnityEngine;

public class ItemMaxCounterGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventItemType target;

	public override int? GenerateValue(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return null;
		}
		return eventItem.maxCounter;
	}
}
