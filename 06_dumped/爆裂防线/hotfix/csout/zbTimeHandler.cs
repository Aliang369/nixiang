using System;

public class zbTimeHandler
{
	private long qwServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

	private long qwClientTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

	private long m_lastServerTimeChecked;

	private const int arenaResultTime = 21600;

	private long m_arenaResultTm;

	public void Init()
	{
		SetServerTime(zbServerData.sGameEnv.ServTime);
		m_lastServerTimeChecked = GetServerTime();
	}

	public long GetTimeZoneSeconds()
	{
		return (int)Math.Round(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalHours) * 3600;
	}

	public void SetServerTime(long value)
	{
		qwServerTime = value;
		qwClientTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		m_lastServerTimeChecked = value;
	}

	public long GetServerTime()
	{
		return qwServerTime + DateTimeOffset.UtcNow.ToUnixTimeSeconds() - qwClientTime;
	}

	public long GetServerMillTime()
	{
		return qwServerTime * 1000 + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - qwClientTime * 1000;
	}

	public string GetServerDateStr()
	{
		return zbUtil.FormatTimeYYYYMMDD(GetServerTime());
	}

	public string GetServerTimeStr()
	{
		long num = GetServerTime() + GetTimeZoneSeconds();
		return zbUtil.FormatTimeYYYYMMDD(num) + "  " + zbUtil.FormatTimeHHMMSS(num, displayDay: false);
	}

	public long GetRefreshTime()
	{
		return zbServerData.sGameData.RoleCommonInfo.NextRefresh;
	}

	public long GetRefreshLeftTime()
	{
		return GetRefreshTime() - GetServerTime();
	}

	public string GetRefreshTimeStr()
	{
		return zbUtil.FormatTimeHHMMSS(GetRefreshLeftTime());
	}

	private void RefreshArenaResultTm()
	{
		long serverTime = GetServerTime();
		long nextRefresh = zbServerData.sGameData.RoleCommonInfo.NextRefresh;
		if (serverTime < nextRefresh - 21600)
		{
			m_arenaResultTm = nextRefresh - 21600;
		}
		else
		{
			m_arenaResultTm = nextRefresh + 86400 - 21600;
		}
	}

	public string GetArenaRefreshTimeStr()
	{
		long num = m_arenaResultTm - GetServerTime();
		if (num < 0)
		{
			RefreshArenaResultTm();
			num = m_arenaResultTm - GetServerTime();
		}
		return zbUtil.FormatTimeHHMMSS(num);
	}

	public bool IsSubjectLegionWarTime()
	{
		DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(GetServerTime()).UtcDateTime;
		return utcDateTime.DayOfWeek switch
		{
			DayOfWeek.Saturday => true, 
			DayOfWeek.Sunday => utcDateTime.TimeOfDay < TimeSpan.FromHours(21.0), 
			_ => false, 
		};
	}

	public string GetSubjectLegionWarTime()
	{
		DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(GetServerTime()).UtcDateTime;
		if (!IsSubjectLegionWarTime())
		{
			return string.Empty;
		}
		DateTime dateTime = ((utcDateTime.DayOfWeek != DayOfWeek.Saturday) ? utcDateTime.Date.AddDays(-1.0) : utcDateTime.Date);
		DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);
		DateTimeOffset dateTimeOffset2 = dateTimeOffset.AddDays(1.0).AddHours(21.0);
		DateTime dateTime2 = dateTimeOffset.ToLocalTime().DateTime;
		DateTime dateTime3 = dateTimeOffset2.ToLocalTime().DateTime;
		return string.Format("{0}/{1}-{2}/{3}", new object[4] { dateTime2.Month, dateTime2.Day, dateTime3.Month, dateTime3.Day });
	}

	public void RefreshTimeInvoke()
	{
		long serverTime = GetServerTime();
		long refreshTime = GetRefreshTime();
		if (m_lastServerTimeChecked < refreshTime && serverTime >= refreshTime)
		{
			zbSelf.Instance.equipHandler.ResetTodayRedPointUse();
		}
		m_lastServerTimeChecked = serverTime;
	}

	public void Update()
	{
		RefreshTimeInvoke();
	}
}
