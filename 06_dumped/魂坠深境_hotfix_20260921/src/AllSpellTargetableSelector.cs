using System.Collections.Generic;

public class AllSpellTargetableSelector : ItemTargetSelector
{
	public GameEvent.EventItemType spellSource;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(spellSource);
		if (eventItem == null)
		{
			return new List<Item>();
		}
		if (!eventItem.HasSpell || eventItem.CurrentSpell.requirement?.targetCheckNode == null)
		{
			return new List<Item>();
		}
		return eventItem.CurrentSpell.requirement.GetTargetableItems(eventItem, eve.gameState);
	}
}
