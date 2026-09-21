using System.Collections.Generic;

public class SkillConstraint : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public virtual bool? CheckSkillAccept(GameSkill skill)
	{
		return skill != null;
	}
}
