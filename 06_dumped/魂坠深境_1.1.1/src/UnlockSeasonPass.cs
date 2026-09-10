using System.Collections.Generic;
using UnityEngine;

public class UnlockSeasonPass : PurchaseInfo
{
	public SeasonDef seasonDef;

	public override string PurchaseTitleText => ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_season_pass_with_season_name_arg0.FormatWith(seasonDef.seasonName);

	public override Sprite GlobalItemCellIcon => seasonDef.icon;

	public override string GlobalItemTitle => ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_season_pass_with_season_name_arg0.FormatWith(seasonDef.seasonName);

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_season_pass_arg0_with_season_description_arg1.FormatWith(seasonDef.seasonName, seasonDef.description);
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			unlockSeasonPass = seasonDef
		};
		return new List<GlobalRewardInfo> { item };
	}

	private UnlockSeasonPass()
	{
	}

	public UnlockSeasonPass(SeasonDef season)
	{
		seasonDef = season;
	}
}
