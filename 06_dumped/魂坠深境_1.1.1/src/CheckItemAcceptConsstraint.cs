using System.Collections.Generic;

public class CheckItemAcceptConsstraint : CheckNode
{
	public ItemConstraint constraint;

	public GameEvent.EventItemType target;

	public override bool? Check(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null || constraint == null)
		{
			return false;
		}
		return constraint.CheckItemAccept(eventItem);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { constraint };
	}
}
