using System.Collections.Generic;

public class DurabilityOfSkill : ValueGenerator
{
	public SkillSelector ss;

	public override int? GenerateValue(GameEvent eve)
	{
		int num = 0;
		List<GameSkill> list = ss?.GetTargetSkills(eve);
		if (list.IsNullOrEmpty())
		{
			return null;
		}
		foreach (GameSkill item in list)
		{
			num += item.durability;
		}
		return num;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { ss };
	}
}
