public class CheckTargetWorldEventChoice : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.TargetWorldEvent is HealSpringEvent healSpringEvent)
		{
			return healSpringEvent.WasUsed;
		}
		return eve.TargetWorldEventChoice != null;
	}
}
