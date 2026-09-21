using System.Collections.Generic;

public class WeekCheckInState
{
	[SaveField(null, null)]
	public FestivalDef festivalDef;

	[SaveField(null, null)]
	public int checkInDays;

	private WeekCheckInState()
	{
	}

	public WeekCheckInState(FestivalDef festivalDef)
	{
		this.festivalDef = festivalDef;
	}

	public List<FestivalCheckInDayState> GenerateAllCheckInDaysInfo()
	{
		List<FestivalCheckInDayState> list = new List<FestivalCheckInDayState>();
		for (int i = 0; i < festivalDef.dailyCheckInRewards.Count; i++)
		{
			list.Add(new FestivalCheckInDayState
			{
				week = this,
				index = i
			});
		}
		return list;
	}

	public FestivalCheckInDayState GetNextUnlockDayInfo()
	{
		if (checkInDays >= festivalDef.RewardDayCount)
		{
			return null;
		}
		return new FestivalCheckInDayState
		{
			week = this,
			index = checkInDays
		};
	}
}
