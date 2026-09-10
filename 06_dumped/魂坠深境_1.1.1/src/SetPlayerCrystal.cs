using System.Collections.Generic;

public class SetPlayerCrystal : Effect
{
	public ValueGenerator valueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		SetCrystalAction action = new SetCrystalAction(valueGenerator.GenerateValue(eve).Value);
		Effect.EnqueueAction(eve, action);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
