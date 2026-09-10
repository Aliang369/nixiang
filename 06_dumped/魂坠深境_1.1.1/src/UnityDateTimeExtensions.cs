using System;

public static class UnityDateTimeExtensions
{
	public static DateTime GetMonday4AM(this DateTime dateTime)
	{
		int num = (int)(7 - dateTime.DayOfWeek) % 7;
		DateTime dateTime2 = dateTime.AddDays(num + 1);
		return new DateTime(dateTime2.Year, dateTime2.Month, dateTime2.Day, 4, 0, 0);
	}
}
