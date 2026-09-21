using System.Collections.Generic;

public class CheckTargetItemUpgraded : CheckNode
{
	public ItemTargetSelector itemTargetSelector;

	public override bool? Check(GameEvent eve)
	{
		return (itemTargetSelector?.GetSingleItem(eve))?.upgraded ?? false;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemTargetSelector };
	}
}
