using System.Collections.Generic;

public class SetPlayerCoin : Effect
{
	public ValueGenerator valueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		SetCoinAction action = new SetCoinAction(valueGenerator.GenerateValue(eve).Value);
		Effect.EnqueueAction(eve, action);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
