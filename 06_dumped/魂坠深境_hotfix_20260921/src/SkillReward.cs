using System.Collections.Generic;

public class SkillReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public SkillDef skillDef;

	[SaveField(null, null)]
	public int archTypeIndex;

	[SaveField(null, null)]
	public List<SkillWordDef> skillWords;

	public override ProfileSkill GlobalItemRefSkill => GetProfileSkill();

	private SkillReward()
	{
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			skill = GetProfileSkill()
		};
	}

	public ProfileSkill GetProfileSkill()
	{
		return new ProfileSkill(skillDef, archTypeIndex);
	}

	public GameSkill GetGameSkill()
	{
		return GetProfileSkill().GetGameSkill();
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(skillDef?.skillName);
	}
}
