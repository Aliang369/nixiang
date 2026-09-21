public class WaitNode : Effect
{
	public float time;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new WaitAction(time));
		return NodeCastResult.Success;
	}
}
