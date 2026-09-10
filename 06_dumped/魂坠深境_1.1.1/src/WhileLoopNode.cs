using System.Collections.Generic;

public class WhileLoopNode : Effect
{
	public CheckNode checker;

	private bool doFirst;

	public Effect effect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (doFirst)
		{
			effect.Cast(eve);
		}
		CastAction(eve);
		return NodeCastResult.Success;
	}

	private void CastAction(GameEvent eve)
	{
		Effect.EnqueueAction(action: new ActionAction(delegate
		{
			if (checker != null && checker.Check(eve) == true)
			{
				effect.Cast(eve);
				CastAction(eve);
			}
		}), eve: eve);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { checker, effect };
	}
}
