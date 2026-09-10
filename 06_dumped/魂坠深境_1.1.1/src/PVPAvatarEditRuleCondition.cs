using System.Collections.Generic;

public abstract class PVPAvatarEditRuleCondition : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public abstract bool IsMatch(DeckPreset preset);

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}
}
