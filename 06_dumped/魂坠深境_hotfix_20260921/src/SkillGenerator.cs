using System.Collections.Generic;

public abstract class SkillGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refSkills = new NodeDisplayInfo.Element<List<GameSkill>>(GenerateSkills(eve))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<GameSkill> GenerateSkills(GameEvent eve);
}
