public class SetEnvironmentInfluence : Effect
{
	public EnvironmentInfluenceDef target;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new SetEnvironmentInfluenceAction(target, hint: true));
		return NodeCastResult.Success;
	}
}
