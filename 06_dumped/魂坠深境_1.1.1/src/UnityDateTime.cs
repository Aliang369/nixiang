using System;
using JetBrains.Annotations;

[Serializable]
public struct UnityDateTime
{
	[SaveField(null, null)]
	public int year;

	[SaveField(null, null)]
	public int month;

	[SaveField(null, null)]
	public int day;

	[SaveField(null, null)]
	public int hour;

	[SaveField(null, null)]
	public int minute;

	[CanBeNull]
	public string LocalTimePreview => UtcDateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

	public DateTime UtcDateTime
	{
		get
		{
			try
			{
				return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
			}
			catch (Exception)
			{
				return DateTime.UnixEpoch;
			}
		}
	}

	public DateTime LocalDateTime => UtcDateTime.ToLocalTime();

	public UnityDateTime(long ticks)
	{
		DateTime dateTime = new DateTime(ticks);
		year = dateTime.Year;
		month = dateTime.Month;
		day = dateTime.Day;
		hour = dateTime.Hour;
		minute = dateTime.Minute;
	}

	public UnityDateTime(DateTime DateTime)
	{
		year = DateTime.Year;
		month = DateTime.Month;
		day = DateTime.Day;
		hour = DateTime.Hour;
		minute = DateTime.Minute;
	}
}
