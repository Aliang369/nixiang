using System.Collections.Generic;

public abstract class PVPAvatarEditRuleEffect : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public abstract void Apply(PVPDefenderProperty property);

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}
}
