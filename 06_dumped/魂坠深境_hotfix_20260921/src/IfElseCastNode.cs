using System.Collections.Generic;

public class IfElseCastNode : Effect
{
	public CheckNode checkNode;

	public Effect trueNode;

	public Effect falseNode;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		bool flag = checkNode.Check(eve) == true;
		if ((trueNode != null) & flag)
		{
			trueNode.Cast(eve);
		}
		else if (falseNode != null && !flag)
		{
			falseNode.Cast(eve);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { checkNode, trueNode, falseNode };
	}
}
