public class CheckWorldEventDef : CheckNode
{
	public WorldEvent worldEvent;

	public override bool? Check(GameEvent eve)
	{
		WorldEvent targetWorldEvent = eve.TargetWorldEvent;
		return targetWorldEvent != null && targetWorldEvent.FullID == worldEvent.FullID;
	}
}
