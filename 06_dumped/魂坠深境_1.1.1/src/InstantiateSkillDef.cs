using System.Collections.Generic;

public class InstantiateSkillDef : SkillGenerator
{
	public SkillDef skillDef;

	public int archTypeIndex;

	public int level;

	public List<SkillWordDef> words;

	public override List<GameSkill> GenerateSkills(GameEvent eve)
	{
		GameSkill item = new GameSkill(new ProfileSkill(skillDef, archTypeIndex));
		return new List<GameSkill> { item };
	}
}
