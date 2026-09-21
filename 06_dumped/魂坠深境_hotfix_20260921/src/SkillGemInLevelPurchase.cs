using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class SkillGemInLevelPurchase : PurchaseInfo
{
	[SaveField(null, null)]
	public int level;

	[SaveField(null, null)]
	public int amount;

	public override bool SupportBatchPurchase => true;

	public override string PurchaseTitleText
	{
		get
		{
			if (amount <= 1)
			{
				return PurchaseInfo.textData.term_random_gem_in_level_arg0.FormatWith(level);
			}
			return PurchaseInfo.textData.term_random_gem_in_count_arg0_level_arg1.FormatWith(amount, level);
		}
	}

	public override string GlobalItemTitle => PurchaseTitleText;

	public override Sprite GlobalItemCellIcon => Bootstrap.applicationState.skillGemManager.GetGemRewardIcon(level);

	public override QualityLevel GlobalItemQuality => SkillGemDef.GetLevelQuality(level);

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return PurchaseInfo.textData.term_get_random_gem_in_level.FormatWith(level, amount);
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		for (int i = 0; i < purchaseTimes; i++)
		{
			list.AddRange(from g in GetGem(applicationState)
				select new GlobalRewardInfo
				{
					skillGem = g
				});
		}
		return list;
	}

	private List<SkillGem> GetGem(ApplicationStateInfo applicationState)
	{
		List<SkillGem> list = new List<SkillGem>();
		List<SkillGemDef> list2 = applicationState.assetCatalog.skillGemCatalog.assets.Where((SkillGemDef s) => s?.level == level).ToList();
		if (!list2.Any())
		{
			return list;
		}
		for (int num = 0; num < amount; num++)
		{
			list.Add(new SkillGem(list2.SelectRandom()));
		}
		return list;
	}
}
