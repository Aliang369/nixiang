using System.Collections.Generic;

public class GenerateItemTag : ItemTagGenerator
{
	public GameEvent.EventItemType targetType;

	public override List<ItemTag> GenerateItemTags(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(targetType);
		if (eventItem == null)
		{
			return new List<ItemTag>();
		}
		return new List<ItemTag>(eventItem.GetTags());
	}
}
