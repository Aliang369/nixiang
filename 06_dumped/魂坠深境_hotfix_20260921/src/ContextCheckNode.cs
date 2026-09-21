using System.Collections.Generic;

public class ContextCheckNode : CheckNode
{
	public CheckNode checkNode;

	public Effect effect;

	public override bool? Check(GameEvent eve)
	{
		effect.Cast(eve);
		return checkNode.Check(eve);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { effect, checkNode };
	}
}
