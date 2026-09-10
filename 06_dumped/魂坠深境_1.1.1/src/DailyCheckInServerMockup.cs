using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class DailyCheckInServerMockup : IDailyCheckInServer
{
	private readonly ApplicationStateInfo applicationState;

	public DailyCheckInServerMockup(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState ?? throw new ArgumentNullException("applicationState");
	}

	public Task<DailyCheckInResult> TriggerDailyCheckInAsync()
	{
		return Task.FromResult(new DailyCheckInResult
		{
			festivalCheckInResult = GenerateFestivalCheckInResult(),
			monthCheckInResult = GenerateMonthCheckInResult(),
			monthCardCheckInResult = GenerateMonthCardCheckInResult()
		});
	}

	private FestivalCheckInResult GenerateFestivalCheckInResult()
	{
		WeekCheckInState weekCheckState = applicationState.dailyCheckManager.weekCheckState;
		if (weekCheckState == null || weekCheckState.GetNextUnlockDayInfo() == null)
		{
			return null;
		}
		FestivalCheckInDayState nextUnlockDayInfo = weekCheckState.GetNextUnlockDayInfo();
		return new FestivalCheckInResult
		{
			festivalDef = weekCheckState.festivalDef,
			alreadyCheckInCount = nextUnlockDayInfo.index + 1,
			reward = nextUnlockDayInfo.GiveReward(applicationState)
		};
	}

	private MonthCheckInResult GenerateMonthCheckInResult()
	{
		DailyCheckManager dailyCheckManager = applicationState.dailyCheckManager;
		List<MonthCheckInState> monthCheckState = dailyCheckManager.monthCheckState;
		MonthCheckInResult monthCheckInResult = new MonthCheckInResult
		{
			checkInDaysThisMonth = monthCheckState.FindAll((MonthCheckInState r) => r.isCurrentMonth && r.isCollected).ConvertAll((MonthCheckInState r) => r.index)
		};
		MonthCheckInState currentMonthCheckState = dailyCheckManager.GetCurrentMonthCheckState(DateTime.Now.Day);
		if (currentMonthCheckState == null || currentMonthCheckState.isCollected)
		{
			return null;
		}
		currentMonthCheckState.isCollected = true;
		GlobalRewardInfo globalRewardInfo = currentMonthCheckState.reward.GenerateReward(applicationState, GlobalRewardSourceType.MonthDailyCheckInReward, "");
		globalRewardInfo.Give(applicationState);
		monthCheckInResult.reward = globalRewardInfo;
		return monthCheckInResult;
	}

	private MonthCardCheckInResult GenerateMonthCardCheckInResult()
	{
		if (!applicationState.playerInfoManager.IsMonthlyCardActive)
		{
			return null;
		}
		DailyCheckManager dailyCheckManager = applicationState.dailyCheckManager;
		MonthCardCheckInResult monthCardCheckInResult = new MonthCardCheckInResult
		{
			reward = ScriptableObjectSingleton<FortressDataManager>.instance.monthlyCardRewardSetup.GenerateReward(applicationState, GlobalRewardSourceType.MonthlyCardCheckIn, "")
		};
		MonthCheckInState currentMonthCheckState = dailyCheckManager.GetCurrentMonthCheckState(DateTime.Now.Day);
		if (currentMonthCheckState == null)
		{
			return null;
		}
		GlobalRewardInfo checkInDoubleReward = currentMonthCheckState.reward.GenerateReward(applicationState, GlobalRewardSourceType.MonthlyCardCheckIn, "");
		monthCardCheckInResult.checkInDoubleReward = checkInDoubleReward;
		monthCardCheckInResult.reward.Give(applicationState);
		monthCardCheckInResult.checkInDoubleReward.Give(applicationState);
		return monthCardCheckInResult;
	}
}
