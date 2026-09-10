using System.Collections.Generic;

public class ChangePlayerCoin : Effect
{
	public ValueGenerator deltaGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ChangeCoinAction action = new ChangeCoinAction(deltaGenerator.GenerateValue(eve).Value);
		Effect.EnqueueAction(eve, action);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaGenerator };
	}
}
