public class SkillWithSpellConstraint : SkillConstraint
{
	public override bool? CheckSkillAccept(GameSkill skill)
	{
		return skill?.SkillSpell != null;
	}
}
