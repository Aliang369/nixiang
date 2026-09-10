using UnityEngine;

public class TargetCurrentSpellCooldownValueGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventItemType target;

	public override int? GenerateValue(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null || !eventItem.HasSpell)
		{
			return null;
		}
		return eventItem.CurrentSpell.GetCoolDown(eventItem);
	}
}
