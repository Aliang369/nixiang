using System.Collections.Generic;

public class SkillCount : ValueGenerator
{
	public SkillSelector skillSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		return skillSelector.GetTargetSkills(eve)?.Count;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { skillSelector };
	}
}
