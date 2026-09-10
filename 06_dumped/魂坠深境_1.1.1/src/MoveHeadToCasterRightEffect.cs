using System.Collections.Generic;

public class MoveHeadToCasterRightEffect : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item casterItem = eve.CasterItem;
		if (casterItem == null || casterItem.owner?.type != ItemContainer.ContainerType.Table)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new MoveHeadToCasterRightAction(eve.CasterItem));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
