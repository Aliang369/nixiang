public class CheckIsRealResolve : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.triggerType == GameEvent.TriggerType.Real;
	}
}
