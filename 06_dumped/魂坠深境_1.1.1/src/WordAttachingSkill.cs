using System.Collections.Generic;

public class WordAttachingSkill : SkillSelector
{
	public override List<GameSkill> GetTargetSkills(GameEvent eve)
	{
		if (eve.CasterAttachObject is GameSkill item)
		{
			return new List<GameSkill> { item };
		}
		return new List<GameSkill>();
	}
}
