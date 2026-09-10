using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CalendarDataManager", menuName = "Singleton/CalendarDataManager")]
public class CalendarDataManager : ScriptableObjectSingleton<CalendarDataManager>
{
	public class WeekShuffleContractGroup
	{
		public List<ContractDef> contracts;
	}

	public UnityDateTime resetBaseUtcTime;

	[SerializeField]
	public FestivalDef currentFestivalDef;

	[SerializeField]
	public List<FestivalDef> festivalChain;

	public List<PurchaseGroupDef> weeklyPurchaseGroupCalender;

	public List<PurchaseGroupDef> monthlyPurchaseGroupCalender;

	public List<PurchaseGroupDef> dailyStablePurchaseGroupCalender;

	public PurchaseGroupDef defaultSeasonPurchaseGroup;

	public List<PurchaseGroupDef> seasonPurchaseGroupCalender;

	public List<WeekShuffleContractGroup> weekShuffleContractCalendar;

	public List<GlobalRewardSetup> monthCheckInRewards;

	public List<ItemSkinSetBatch> skinPurchaseShuffle;

	public PurchaseGroupDef skinPurchaseGroupDef;

	public UnityDateTime currentUnityTime => new UnityDateTime(DateTime.UtcNow);

	public int CurrentDailyPeriod => GetDailyPeriod(DateTime.UtcNow);

	public int CurrentWeekPeriod => GetWeekPeriod(DateTime.UtcNow);

	public int CurrentMonthPeriod => GetMonthPeriod(DateTime.UtcNow);

	public DateTime NextDailyResetTime => resetBaseUtcTime.UtcDateTime + TimeSpan.FromDays((DateTime.UtcNow - resetBaseUtcTime.UtcDateTime).Days + 1);

	public int WeeklyPurchaseGroupCalenderLength => weeklyPurchaseGroupCalender.Count;

	public PurchaseGroupDef CurrentWeeklyPurchaseGroup => weeklyPurchaseGroupCalender[CurrentWeekPeriod % WeeklyPurchaseGroupCalenderLength];

	public int MonthlyPurchaseGroupCalenderLength => monthlyPurchaseGroupCalender.Count;

	public PurchaseGroupDef CurrentMonthlyPurchaseGroup => monthlyPurchaseGroupCalender[CurrentMonthPeriod % MonthlyPurchaseGroupCalenderLength];

	public int DailyStablePurchaseGroupCalenderLength => dailyStablePurchaseGroupCalender?.Count ?? 0;

	public PurchaseGroupDef CurrentDailyStablePurchaseGroup
	{
		get
		{
			if (DailyStablePurchaseGroupCalenderLength <= 0)
			{
				return null;
			}
			return dailyStablePurchaseGroupCalender[CurrentDailyPeriod % DailyStablePurchaseGroupCalenderLength];
		}
	}

	public PurchaseGroupDef seasonPurchaseGroup => seasonPurchaseGroupCalender.FirstOrDefault((PurchaseGroupDef p) => p.withinValidTime) ?? defaultSeasonPurchaseGroup;

	public int WeekShuffleContractCalendarLength => weekShuffleContractCalendar.Count;

	public WeekShuffleContractGroup CurrentWeekShuffleContractGroup => weekShuffleContractCalendar[CurrentWeekPeriod % WeekShuffleContractCalendarLength];

	public string IncomingWeekPurchaseGroupPreviewInfo
	{
		get
		{
			string text = "";
			for (int i = 0; i < 10; i++)
			{
				int num = CurrentWeekPeriod + i;
				PurchaseGroupDef purchaseGroupDef = weeklyPurchaseGroupCalender[num % WeeklyPurchaseGroupCalenderLength];
				UnityDateTime weekPeriodStartTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodStartTime(num);
				UnityDateTime weekPeriodEndTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime(num);
				text = text + weekPeriodStartTime.LocalTimePreview + " - " + weekPeriodEndTime.LocalTimePreview + "          " + purchaseGroupDef.GroupName + Environment.NewLine;
			}
			return text;
		}
	}

	public string IncomingMonthPurchaseGroupPreviewInfo
	{
		get
		{
			string text = "";
			for (int i = 0; i < 10; i++)
			{
				int num = CurrentMonthPeriod + i;
				PurchaseGroupDef purchaseGroupDef = monthlyPurchaseGroupCalender[num % MonthlyPurchaseGroupCalenderLength];
				UnityDateTime monthPeriodStartTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetMonthPeriodStartTime(num);
				UnityDateTime monthPeriodEndTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetMonthPeriodEndTime(num);
				text = text + monthPeriodStartTime.LocalTimePreview + " - " + monthPeriodEndTime.LocalTimePreview + "          " + purchaseGroupDef.GroupName + Environment.NewLine;
			}
			return text;
		}
	}

	public string IncomingDailyStablePurchaseGroupPreviewInfo
	{
		get
		{
			if (dailyStablePurchaseGroupCalender == null || dailyStablePurchaseGroupCalender.Count == 0)
			{
				return string.Empty;
			}
			string text = "";
			for (int i = 0; i < 10; i++)
			{
				int num = CurrentDailyPeriod + i;
				PurchaseGroupDef purchaseGroupDef = dailyStablePurchaseGroupCalender[num % DailyStablePurchaseGroupCalenderLength];
				UnityDateTime dayPeriodStartTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetDayPeriodStartTime(num);
				UnityDateTime dayPeriodEndTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetDayPeriodEndTime(num);
				text = text + dayPeriodStartTime.LocalTimePreview + " - " + dayPeriodEndTime.LocalTimePreview + "          " + purchaseGroupDef.GroupName + Environment.NewLine;
			}
			return text;
		}
	}

	public string IncomingWeeklyContractPreviewInfo
	{
		get
		{
			string text = "";
			for (int i = 0; i < 10; i++)
			{
				int num = CurrentWeekPeriod + i;
				WeekShuffleContractGroup weekShuffleContractGroup = weekShuffleContractCalendar[num % WeekShuffleContractCalendarLength];
				UnityDateTime weekPeriodStartTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodStartTime(num);
				UnityDateTime weekPeriodEndTime = ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime(num);
				text = text + weekPeriodStartTime.LocalTimePreview + " - " + weekPeriodEndTime.LocalTimePreview + "          " + weekShuffleContractGroup.contracts.Select((ContractDef c) => c.Name).Aggregate((string a, string b) => a + "," + b) + Environment.NewLine;
			}
			return text;
		}
	}

	public int GetCustomPeriod(DateTime time, int customDays)
	{
		if (customDays <= 0)
		{
			return 0;
		}
		return (time - resetBaseUtcTime.UtcDateTime).Days / customDays;
	}

	public int GetCustomPeriod(long ticks, int customDays)
	{
		return GetCustomPeriod(new DateTime(ticks), customDays);
	}

	public int GetDailyPeriod(DateTime time)
	{
		return (time - resetBaseUtcTime.UtcDateTime).Days;
	}

	public int GetDailyPeriod(long ticks)
	{
		return GetDailyPeriod(new DateTime(ticks));
	}

	public int GetWeekPeriod(DateTime time)
	{
		return (time - resetBaseUtcTime.UtcDateTime).Days / 7;
	}

	public int GetWeekPeriod(long ticks)
	{
		return GetWeekPeriod(new DateTime(ticks));
	}

	public int GetMonthPeriod(DateTime time)
	{
		return GetNaturalMonthPeriod(time);
	}

	public int GetMonthPeriod(long ticks)
	{
		return GetMonthPeriod(new DateTime(ticks));
	}

	public UnityDateTime GetDayPeriodStartTime(int dayPeriod)
	{
		return new UnityDateTime(resetBaseUtcTime.UtcDateTime + TimeSpan.FromDays(dayPeriod));
	}

	public UnityDateTime GetDayPeriodEndTime(int dayPeriod)
	{
		return GetDayPeriodStartTime(dayPeriod + 1);
	}

	public UnityDateTime GetDayPeriodEndTime()
	{
		return GetDayPeriodEndTime(CurrentDailyPeriod);
	}

	public UnityDateTime GetWeekPeriodStartTime(int weekPeriod)
	{
		return new UnityDateTime(resetBaseUtcTime.UtcDateTime + TimeSpan.FromDays(weekPeriod * 7));
	}

	public UnityDateTime GetWeekPeriodEndTime(int weekPeriod)
	{
		return GetWeekPeriodStartTime(weekPeriod + 1);
	}

	public UnityDateTime GetWeekPeriodEndTime()
	{
		return GetWeekPeriodEndTime(CurrentWeekPeriod);
	}

	public UnityDateTime GetMonthPeriodStartTime(int monthPeriod)
	{
		return new UnityDateTime(GetNaturalMonthPeriodStartTime(monthPeriod));
	}

	public UnityDateTime GetMonthPeriodEndTime(int monthPeriod)
	{
		return GetMonthPeriodStartTime(monthPeriod + 1);
	}

	public UnityDateTime GetMonthPeriodEndTime()
	{
		return GetMonthPeriodEndTime(CurrentMonthPeriod);
	}

	public UnityDateTime GetCustomPeriodStartTime(int customPeriod, int customDays)
	{
		return new UnityDateTime(resetBaseUtcTime.UtcDateTime + TimeSpan.FromDays(customPeriod * customDays));
	}

	public UnityDateTime GetCustomPeriodEndTime(int customPeriod, int customDays)
	{
		return GetCustomPeriodStartTime(customPeriod + 1, customDays);
	}

	public UnityDateTime GetCustomPeriodEndTime(int customDays)
	{
		return GetCustomPeriodEndTime(GetCustomPeriod(DateTime.UtcNow, customDays), customDays);
	}

	private static int GetNaturalMonthPeriod(DateTime time)
	{
		return time.Year * 12 + time.Month - 1;
	}

	private static DateTime GetNaturalMonthPeriodStartTime(int monthPeriod)
	{
		int year = monthPeriod / 12;
		int month = monthPeriod % 12 + 1;
		return new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
	}
}
