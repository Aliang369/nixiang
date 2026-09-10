public class SkillGemReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public SkillGemDef skillGemDef;

	[SaveField(null, null)]
	public int amount;

	public SkillGemReward(SkillGemDef skillGemDef, int amount)
	{
		this.skillGemDef = skillGemDef;
		this.amount = amount;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			skillGem = new SkillGem(skillGemDef, amount)
		};
	}
}
