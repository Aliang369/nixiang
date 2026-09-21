using UnityEngine;

public class AddCurrentSeasonLevelExp : GlobalRewardSetup
{
	[SaveField(null, null)]
	public int addExp;

	[SaveField(null, null)]
	public bool weekQuestExpLimit = true;

	public override int GlobalItemCount => addExp;

	public override string GlobalItemCellName => FGlobalItemCellName(addExp);

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_exp_bonus;

	public override string GlobalItemTitle => FGlobalItemTitle(addExp);

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		int exp = addExp;
		if (applicationState.seasonManager.currentSeason.highseasonPassUnlocked)
		{
			exp = Mathf.RoundToInt((float)addExp * ((100f + (float)ScriptableObjectSingleton<FortressDataManager>.instance.highSeasonPassQuestExpBonusPercent) / 100f));
		}
		LevelLine.ChangeExpResult changeLevelLineExpResult = LevelLine.ChangeExpRequest.ChangeSeasonExp(exp, weekQuestExpLimit, applicationState).Generate(applicationState);
		return new GlobalRewardInfo
		{
			changeLevelLineExpResult = changeLevelLineExpResult
		};
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(addExp);
	}

	public override string GetRewardBrief()
	{
		return base.GetRewardBrief().FormatWith(addExp);
	}
}
