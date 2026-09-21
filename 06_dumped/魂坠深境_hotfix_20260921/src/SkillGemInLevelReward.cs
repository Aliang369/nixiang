using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class SkillGemInLevelReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public int level;

	[SaveField(null, null)]
	public int amount;

	public override string GlobalItemTitle
	{
		get
		{
			if (amount <= 1)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_random_gem_in_level_arg0.FormatWith(level);
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_random_gem_in_count_arg0_level_arg1.FormatWith(amount, level);
		}
	}

	public override string GlobalItemCellName => GlobalItemTitle;

	public override int GlobalItemCount => amount;

	public override Sprite GlobalItemCellIcon => Bootstrap.applicationState.skillGemManager.GetGemRewardIcon(level);

	public override QualityLevel GlobalItemQuality => SkillGemDef.GetLevelQuality(level);

	public override string GetRewardBrief()
	{
		return GlobalItemTitle;
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_get_random_gem_in_level.FormatWith(level, amount);
	}

	public SkillGemInLevelReward(int level, int amount)
	{
		this.level = level;
		this.amount = amount;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.RandomChest, (from g in GetGem(applicationState)
			select new GlobalRewardInfo
			{
				skillGem = g
			}).ToList());
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
