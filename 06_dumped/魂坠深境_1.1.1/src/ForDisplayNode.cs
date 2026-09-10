using System.Collections.Generic;

public class ForDisplayNode : Effect
{
	public Effect displayNode;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { displayNode };
	}
}
