using System;
using System.Collections.Generic;

public sealed class LimitedTimeActivityRuntimeConfig
{
	public string ActivityId { get; }

	public bool Enabled { get; }

	public IReadOnlyDictionary<string, string> Title { get; }

	public IReadOnlyDictionary<string, string> Description { get; }

	public IReadOnlyDictionary<string, string> HelpText { get; }

	public DateTime StartTimeUtc { get; }

	public DateTime EndTimeUtc { get; }

	public bool IsValid
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(ActivityId))
			{
				return StartTimeUtc < EndTimeUtc;
			}
			return false;
		}
	}

	public LimitedTimeActivityRuntimeConfig(string activityId, DateTime startTimeUtc, DateTime endTimeUtc, bool enabled = true, Dictionary<string, string> title = null, Dictionary<string, string> description = null, Dictionary<string, string> helpText = null)
	{
		Enabled = enabled;
		ActivityId = activityId;
		StartTimeUtc = NormalizeUtc(startTimeUtc);
		EndTimeUtc = NormalizeUtc(endTimeUtc);
		Title = title ?? new Dictionary<string, string>();
		Description = description ?? new Dictionary<string, string>();
		HelpText = helpText ?? new Dictionary<string, string>();
	}

	public bool Contains(DateTime utcNow)
	{
		if (!Enabled)
		{
			return false;
		}
		DateTime dateTime = NormalizeUtc(utcNow);
		if (dateTime >= StartTimeUtc)
		{
			return dateTime < EndTimeUtc;
		}
		return false;
	}

	public static DateTime NormalizeUtc(DateTime dateTime)
	{
		return dateTime.Kind switch
		{
			DateTimeKind.Utc => dateTime, 
			DateTimeKind.Local => dateTime.ToUniversalTime(), 
			_ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), 
		};
	}
}
