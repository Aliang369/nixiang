using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaPoolState
{
	[SaveField(null, null)]
	public GachaPoolDef def;

	[SaveField(null, null)]
	public PlayerCharacterDef wishCharacter;

	[SaveField(null, null)]
	private bool _viewed;

	[SaveField(null, null)]
	public List<GachaHistoryInfo> gachaDrawHistoryInfos;

	[NonSerialized]
	public bool gachaHistoryReady = true;

	public bool viewed
	{
		get
		{
			return _viewed;
		}
		set
		{
			if (_viewed != value)
			{
				_viewed = value;
				Bootstrap.applicationState.savingManager.SetProfileDirtyToSave();
			}
		}
	}

	public bool valid
	{
		get
		{
			DateTime utcNow = DateTime.UtcNow;
			if (!def.startDate.HasValue || def.startDate.Value.UtcDateTime <= utcNow)
			{
				if (def.endDate.HasValue)
				{
					return def.endDate.Value.UtcDateTime > utcNow;
				}
				return true;
			}
			return false;
		}
	}

	public int maxDrawCount
	{
		get
		{
			if (!def.enableGachaCountLimitRule)
			{
				return 10;
			}
			return RemainDrawTimesInThisLimitResetPeriod;
		}
	}

	public MatterInfo drawMaxCost => GetCostOfDrawX(maxDrawCount);

	public bool GrandPrizeDrawed
	{
		get
		{
			if (!def.enableGrandPrizeRule)
			{
				return false;
			}
			return gachaDrawHistoryInfos.Any((GachaHistoryInfo h) => h.contentType == GachaPoolDef.GachaContentType.Obsession && h.content == def.grandPrizeCharacter);
		}
	}

	public int RemainTimesForGrandPrize
	{
		get
		{
			if (!def.enableGrandPrizeRule || GrandPrizeDrawed)
			{
				return -1;
			}
			int count = gachaDrawHistoryInfos.Count;
			return def.grandPrizeCount - count;
		}
	}

	public int RemainDrawTimesInThisLimitResetPeriod
	{
		get
		{
			if (!def.enableGachaCountLimitRule)
			{
				return -1;
			}
			return Math.Max(0, def.gachaCountLimit - GetDrawTimeInResetTimePeriod(def.gachaCountLimitResetType));
		}
	}

	public int RemainFreeDrawTimesInThisResetPeriod
	{
		get
		{
			if (!def.enableFreeDraw)
			{
				return -1;
			}
			return Math.Max(0, def.freeDrawCount - GetDrawTimeInResetTimePeriod(def.freeDrawResetTimeType));
		}
	}

	public bool WishUsedThisWeek
	{
		get
		{
			GachaHistoryInfo gachaHistoryInfo = gachaDrawHistoryInfos.LastOrDefault((GachaHistoryInfo s) => s.wish);
			if (gachaHistoryInfo != null)
			{
				return ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriod(gachaHistoryInfo.date) == ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeekPeriod;
			}
			return false;
		}
	}

	private GachaPoolState()
	{
	}

	public GachaPoolState(GachaPoolDef def)
	{
		this.def = def;
		gachaDrawHistoryInfos = new List<GachaHistoryInfo>();
	}

	public MatterInfo GetCostOfDrawX(int count)
	{
		int poolNeedCostDrawCount = GetPoolNeedCostDrawCount(count);
		return new MatterInfo(def.singleDrawCost.matterDef, def.singleDrawCost.count * poolNeedCostDrawCount);
	}

	private int GetPoolNeedCostDrawCount(int rawDrawCount)
	{
		int num = rawDrawCount;
		if (def.enableFreeDraw && RemainFreeDrawTimesInThisResetPeriod > 0)
		{
			num -= Mathf.Min(RemainFreeDrawTimesInThisResetPeriod, num);
		}
		return num;
	}

	public int GetDrawTimeInResetTimePeriod(ResetTimeType type)
	{
		switch (type)
		{
		case ResetTimeType.None:
			return gachaDrawHistoryInfos.Count;
		case ResetTimeType.Daily:
		{
			int current = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod;
			return gachaDrawHistoryInfos.Count((GachaHistoryInfo h) => ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(h.date) == current);
		}
		case ResetTimeType.Weekly:
		{
			int currentWeek = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeekPeriod;
			return gachaDrawHistoryInfos.Count((GachaHistoryInfo h) => ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriod(h.date) == currentWeek);
		}
		case ResetTimeType.Monthly:
		{
			int currentMonth = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentMonthPeriod;
			return gachaDrawHistoryInfos.Count((GachaHistoryInfo h) => ScriptableObjectSingleton<CalendarDataManager>.instance.GetMonthPeriod(h.date) == currentMonth);
		}
		default:
			return 0;
		}
	}
}
