using System;

public static class DailyChallengeClock
{
	public static DateTime GetNextDailyResetUtc(DateTime utcNow, int utcOffsetHours = 8, int resetHour = 4)
	{
		DateTime dateTime = utcNow.ToUniversalTime().AddHours(utcOffsetHours);
		DateTime dateTime2 = dateTime.Date.AddHours(resetHour);
		if (dateTime >= dateTime2)
		{
			dateTime2 = dateTime2.AddDays(1.0);
		}
		return dateTime2.AddHours(-utcOffsetHours);
	}

	public static DateTime GetNextWeeklyResetUtc(DateTime utcNow, int utcOffsetHours = 8, int resetHour = 4, DayOfWeek resetDay = DayOfWeek.Monday)
	{
		DateTime dateTime = utcNow.ToUniversalTime().AddHours(utcOffsetHours);
		int num = (resetDay - dateTime.DayOfWeek + 7) % 7;
		DateTime dateTime2 = dateTime.Date.AddDays(num).AddHours(resetHour);
		if (dateTime >= dateTime2)
		{
			dateTime2 = dateTime2.AddDays(7.0);
		}
		return dateTime2.AddHours(-utcOffsetHours);
	}

	public static TimeSpan Remaining(DateTime utcTarget, DateTime utcNow)
	{
		if (!(utcTarget > utcNow))
		{
			return TimeSpan.Zero;
		}
		return utcTarget - utcNow;
	}
}
