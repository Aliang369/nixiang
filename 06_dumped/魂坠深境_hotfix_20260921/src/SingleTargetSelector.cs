using System.Collections.Generic;

public class SingleTargetSelector : ItemTargetSelector
{
	public GameEvent.EventItemType target;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return null;
		}
		return new List<Item> { eventItem };
	}
}
