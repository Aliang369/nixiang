public class FestivalCheckInDayState
{
	[SaveField(null, null)]
	public WeekCheckInState week;

	[SaveField(null, null)]
	public int index;

	public bool IsGot => week.checkInDays > index;

	public GlobalRewardSetup Reward => week.festivalDef.dailyCheckInRewards[index];

	public GlobalRewardInfo GiveReward(ApplicationStateInfo applicationState)
	{
		if (week.checkInDays != index)
		{
			return null;
		}
		GlobalRewardInfo globalRewardInfo = Reward.GenerateReward(applicationState, GlobalRewardSourceType.FestivalDailyCheckReward, week.festivalDef);
		globalRewardInfo.Give(applicationState);
		week.checkInDays++;
		return globalRewardInfo;
	}
}
