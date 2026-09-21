using System.Collections.Generic;

public class PVPEnemyDeckEditRule : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public string description;

	public PVPAvatarEditRuleCondition condition;

	public PVPAvatarEditRuleEffect effect;

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { condition, effect };
	}

	public string GetRuleDescription(ApplicationStateInfo asi, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatEffectObjectDescription(this, description, asi, out generator);
	}
}
