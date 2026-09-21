using System.Collections.Generic;

public class SkillPurchase : PurchaseInfo
{
	public SkillReward skillReward;

	public override string PurchaseTitleText => skillReward.skillDef.skillName;

	public override ProfileSkill GlobalItemRefSkill => new ProfileSkill(skillReward.skillDef, skillReward.archTypeIndex);

	public override QualityLevel GlobalItemQuality => skillReward.GlobalItemRefSkill.QualityLevel;

	private SkillPurchase()
	{
	}

	public SkillPurchase(SkillReward skillReward, MatterInfo cost)
	{
		this.skillReward = skillReward;
		mattercost = cost;
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		GlobalRewardInfo item = skillReward.GenerateReward(applicationState, GlobalRewardSourceType.Purchase, purchaseId);
		return new List<GlobalRewardInfo> { item };
	}
}
