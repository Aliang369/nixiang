using System;

public class MonthCheckInState
{
	[SaveField(null, null)]
	public int index;

	[SaveField(null, null)]
	public int displayIndex;

	[SaveField(null, null)]
	public bool isCollected;

	[SaveField(null, null)]
	public bool isCurrentMonth = true;

	[SaveField(null, null)]
	public bool isCardReward;

	[SaveField(null, null)]
	public bool isDoubleReward;

	public GlobalRewardSetup reward
	{
		get
		{
			if (isCardReward)
			{
				return ScriptableObjectSingleton<FortressDataManager>.instance.monthlyCardRewardSetup;
			}
			DateTime now = DateTime.Now;
			DateTime dateTime = (isCurrentMonth ? now : now.AddMonths(-1));
			int num = (isCurrentMonth ? index : displayIndex);
			if (num <= 0 || num > DateTime.DaysInMonth(dateTime.Year, dateTime.Month))
			{
				return null;
			}
			int num2 = (int)(new DateTime(dateTime.Year, dateTime.Month, num).DayOfWeek + 6) % 7;
			if (ScriptableObjectSingleton<CalendarDataManager>.instance.monthCheckInRewards.Count <= num2)
			{
				return null;
			}
			return ScriptableObjectSingleton<CalendarDataManager>.instance.monthCheckInRewards[num2];
		}
	}
}
