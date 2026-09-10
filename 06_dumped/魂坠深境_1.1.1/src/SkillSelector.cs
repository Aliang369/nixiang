using System.Collections.Generic;
using System.Linq;

public abstract class SkillSelector : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refSkills = new NodeDisplayInfo.Element<List<GameSkill>>(GetTargetSkills(eve))
		};
	}

	public List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<GameSkill> GetTargetSkills(GameEvent eve);

	public GameSkill GetSingleSkill(GameEvent eve)
	{
		return GetTargetSkills(eve).FirstOrDefault();
	}
}
