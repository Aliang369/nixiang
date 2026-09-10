using System;
using System.Collections.Generic;

public class SwapRandomIntent : Effect
{
	public bool intentChange;

	public ItemTargetSelector toSwapIntentSelector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		throw new NotImplementedException();
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { toSwapIntentSelector };
	}
}
