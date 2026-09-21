using System.Collections.Generic;

public class ChoiseSpells : Effect
{
	public List<Effect> nodes = new List<Effect>();

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (nodes.Count <= eve.choiceNumber || nodes[eve.choiceNumber] == null)
		{
			return NodeCastResult.Failure;
		}
		nodes[eve.choiceNumber].Cast(eve);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (nodes != null)
		{
			list.AddRange(nodes);
		}
		return list;
	}
}
