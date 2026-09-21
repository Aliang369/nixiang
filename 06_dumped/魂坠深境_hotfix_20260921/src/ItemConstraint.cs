using System.Collections.Generic;

public abstract class ItemConstraint : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public virtual bool? CheckItemAccept(Item item)
	{
		return item != null;
	}
}
