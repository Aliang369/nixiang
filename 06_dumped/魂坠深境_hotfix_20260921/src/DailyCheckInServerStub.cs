using System;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;

public sealed class DailyCheckInServerStub : IDailyCheckInServer
{
	private readonly ApplicationStateInfo applicationState;

	public DailyCheckInServerStub(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState ?? throw new ArgumentNullException("applicationState");
	}

	public async Task<DailyCheckInResult> TriggerDailyCheckInAsync()
	{
		TriggerDailyCheckInResult triggerDailyCheckInResult = await applicationState.ChillyRoomSDK.Routine.TriggerDailyCheckInAsync();
		if (triggerDailyCheckInResult == null)
		{
			return null;
		}
		return ToClient(triggerDailyCheckInResult);
	}

	private DailyCheckInResult ToClient(TriggerDailyCheckInResult networkResult)
	{
		NetworkConvertContext networkConvertContext = new NetworkConvertContext(applicationState);
		DailyCheckInResult dailyCheckInResult = networkResult.ToClient(networkConvertContext);
		if (networkResult.FestivalCheckInResult != null)
		{
			DailyCheckManager dailyCheckManager = applicationState.dailyCheckManager;
			dailyCheckManager.weekCheckState.festivalDef = networkConvertContext.FullIdToCTAsset<FestivalDef>(networkResult.FestivalCheckInResult.FestivalId);
			dailyCheckManager.weekCheckState.checkInDays = networkResult.FestivalCheckInResult.CurrentCheckInCount;
			dailyCheckInResult.festivalCheckInResult.reward.Give(applicationState);
		}
		if (networkResult.MonthDailyCheckInRewards != null)
		{
			foreach (MonthCheckInState item in applicationState.dailyCheckManager.monthCheckState)
			{
				if (item.isCurrentMonth && networkResult.MonthDailyCheckInRewards.CheckInDaysInThisMonth.Contains(item.index))
				{
					item.isCollected = true;
				}
				else
				{
					item.isCollected = false;
				}
			}
			dailyCheckInResult.monthCheckInResult.reward.Give(applicationState);
		}
		if (networkResult.MonthlyCardCheckInResult != null)
		{
			dailyCheckInResult.monthCardCheckInResult.reward.Give(applicationState);
			dailyCheckInResult.monthCardCheckInResult.checkInDoubleReward.Give(applicationState);
		}
		return dailyCheckInResult;
	}
}
