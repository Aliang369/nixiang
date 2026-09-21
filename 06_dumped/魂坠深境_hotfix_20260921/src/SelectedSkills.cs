using System.Collections.Generic;

public class SelectedSkills : SkillSelector
{
	public override List<GameSkill> GetTargetSkills(GameEvent eve)
	{
		List<GameSkill> list = new List<GameSkill>();
		if (eve.SelectedSkills == null)
		{
			return list;
		}
		list.AddRange(eve.SelectedSkills);
		return list;
	}
}
