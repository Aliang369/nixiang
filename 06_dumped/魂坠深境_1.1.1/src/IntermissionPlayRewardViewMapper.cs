using System;
using ChillyRoom.Services.ContainerOfSoul;

public static class IntermissionPlayRewardViewMapper
{
	public static IGlobalItemDisplaySetup ToDisplaySetup(this IntermissionPlayRewardViewDto rewardView, ApplicationStateInfo applicationState)
	{
		string type = rewardView.Type;
		if (!(type == "matter"))
		{
			if (type == "skillGemLevel")
			{
				if (!int.TryParse(rewardView.Id, out var result))
				{
					throw new InvalidOperationException("幕间游艺宝石等级非法: " + rewardView.Id);
				}
				return new SkillGemInLevelReward(result, rewardView.Count);
			}
			throw new InvalidOperationException("幕间游艺奖励预览类型未实现: " + rewardView.Type);
		}
		return new MatterReward(applicationState.assetCatalog.FullIdToCTAsset<MatterDef>(rewardView.Id) ?? throw new InvalidOperationException("幕间游艺奖励物品不存在: " + rewardView.Id), rewardView.Count);
	}
}
