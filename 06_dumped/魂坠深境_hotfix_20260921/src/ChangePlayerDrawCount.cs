using System.Collections.Generic;

public class ChangePlayerDrawCount : Effect
{
	public ValueGenerator deltaGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int delta = ((deltaGenerator != null) ? deltaGenerator.GenerateValue(eve).Value : 0);
		Effect.EnqueueAction(eve, new ChangePlayerDrawAction(delta));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaGenerator };
	}
}
