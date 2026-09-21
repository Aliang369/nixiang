using System.Collections.Generic;

public class CheckTargetItemTransformed : CheckNode
{
	public ItemTargetSelector itemTargetSelector;

	public override bool? Check(GameEvent eve)
	{
		return (itemTargetSelector?.GetSingleItem(eve))?.transformActive;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemTargetSelector };
	}
}
