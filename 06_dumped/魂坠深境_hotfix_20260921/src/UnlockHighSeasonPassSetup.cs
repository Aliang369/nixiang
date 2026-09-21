using System.Collections.Generic;

public class UnlockHighSeasonPassSetup : GlobalRewardSetup
{
	public SeasonDef seasonDef;

	public override bool isHidden => true;

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			unlockHighSeasonPass = seasonDef
		};
		list.Add(item);
		LevelLine.ChangeExpResult changeLevelLineExpResult = LevelLine.ChangeExpRequest.ChangeSeasonExp((int)((float)applicationState.seasonManager.currentSeason.seasonLeveLine.SumGotExp * ((float)ScriptableObjectSingleton<FortressDataManager>.instance.highSeasonPassInstanceAddExpPercentToGotSumExp / 100f)), fromQuest: false, applicationState).Generate(applicationState);
		GlobalRewardInfo item2 = new GlobalRewardInfo
		{
			changeLevelLineExpResult = changeLevelLineExpResult
		};
		list.Add(item2);
		return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.UnlockHighSeason, list);
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(seasonDef?.seasonName);
	}
}
