using System.Collections.Generic;

public class ChangePlayerCrystal : Effect
{
	public ValueGenerator deltaGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ChangeCrystalAction action = new ChangeCrystalAction(deltaGenerator.GenerateValue(eve).Value);
		Effect.EnqueueAction(eve, action);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaGenerator };
	}
}
