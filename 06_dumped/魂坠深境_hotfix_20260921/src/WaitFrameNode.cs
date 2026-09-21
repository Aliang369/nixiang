public class WaitFrameNode : Effect
{
	public int frame;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new WaitFrameAction(frame));
		return NodeCastResult.Success;
	}
}
