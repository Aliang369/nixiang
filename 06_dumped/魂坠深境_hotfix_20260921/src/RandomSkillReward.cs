using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class RandomSkillReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public int count = 1;

	[SaveField(null, null)]
	public QualityLevel qualityFilter = QualityLevel.None;

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_random_skill;

	public override int GlobalItemCount => count;

	private List<SkillDef> AllSkills => ScriptableObjectSingleton<FortressDataManager>.instance.baseGachaPoolDef.baseGachaWeightContents.Keys.OfType<SkillDef>().ToList();

	public override List<IGlobalItemDisplaySetup> RandomSkillDefs => AllSkills.ConvertAll((Converter<SkillDef, IGlobalItemDisplaySetup>)((SkillDef s) => new GlobalRewardInfo
	{
		skill = new ProfileSkill(s, 0)
	}));

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		string text = ((qualityFilter == QualityLevel.None) ? "" : ScriptableObjectSingleton<TextDataPreset>.instance.GetQualityLevelText(qualityFilter, withColor: true));
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(text, count);
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		for (int i = 0; i < count; i++)
		{
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
			List<SkillDef> list2 = AllSkills;
			if (qualityFilter != QualityLevel.None)
			{
				list2 = list2.Where((SkillDef s) => s.BaseQualityLevel == qualityFilter).ToList();
			}
			SkillDef skillDef = list2.SelectRandom();
			globalRewardInfo.skill = new ProfileSkill(skillDef, skillDef.BaseQualityArchType, 0);
			list.Add(globalRewardInfo);
		}
		return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.RandomChest, list);
	}

	public RandomSkillReward(int count = 1, QualityLevel quality = QualityLevel.None)
	{
		this.count = count;
		qualityFilter = quality;
	}

	public RandomSkillReward()
	{
		count = 1;
		qualityFilter = QualityLevel.None;
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(count, (qualityFilter == QualityLevel.None) ? "" : ((object)qualityFilter));
	}
}
