using System.Collections.Generic;

public class ForeachItemLoop : Effect
{
	public ItemTargetSelector selector;

	public Effect castNode;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = selector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		int num = (eve.CastNodeLoopIndex = 0);
		Item nodeTempItem = eve.NodeTempItem;
		foreach (Item item in list)
		{
			eve.CastNodeLoopIndex = num++;
			eve.NodeTempItem = item;
			castNode?.Cast(eve);
		}
		eve.NodeTempItem = nodeTempItem;
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector, castNode };
	}
}
