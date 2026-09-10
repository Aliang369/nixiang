using System.Collections.Generic;

public class HintItem : Effect
{
	public ItemTargetSelector selector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		foreach (Item item in (selector?.GetTargetItems(eve)).OrEmptyIfNull())
		{
			HintItemAction action = new HintItemAction(item);
			Effect.EnqueueAction(eve, action);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
