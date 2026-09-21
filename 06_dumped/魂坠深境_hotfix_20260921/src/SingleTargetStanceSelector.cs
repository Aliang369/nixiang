using System.Collections.Generic;

public class SingleTargetStanceSelector : StanceDefSelector
{
	public GameEvent.EventItemType target;

	public override List<StanceDef> GenerateStanceDefs(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return null;
		}
		List<StanceDef> stanceDefsFromTags = eventItem.GetTags().GetStanceDefsFromTags();
		if (stanceDefsFromTags == null || stanceDefsFromTags.Count == 0)
		{
			return null;
		}
		return stanceDefsFromTags;
	}

	public override StanceDef GenerateSingleStanceDef(GameEvent eve)
	{
		return GetSingleStanceDef(eve);
	}
}
