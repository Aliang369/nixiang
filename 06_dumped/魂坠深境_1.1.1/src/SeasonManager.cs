using System;
using System.Linq;
using UnityEngine;

public class SeasonManager : GlobalGameManager
{
	[SaveField(null, null)]
	public Season currentSeason;

	[SaveField(null, null)]
	public int gotQuestExpThisWeek;

	public bool SeasonChangedDuringInit { get; private set; }

	public int FinalWeeklyQuestSeasonExpLimit
	{
		get
		{
			int num = ScriptableObjectSingleton<FortressDataManager>.instance.weeklyQuestSeasonExpLimit;
			if (currentSeason.highseasonPassUnlocked)
			{
				num = Mathf.RoundToInt((float)num * ((100f + (float)ScriptableObjectSingleton<FortressDataManager>.instance.highSeasonPassWeeklyExpLimitAddPercent) / 100f));
			}
			return num;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		SeasonChangedDuringInit = false;
		if (!CSCommon.enableNetwork)
		{
			SeasonDef seasonDef = GetCurrentSeason();
			if (currentSeason?.def != seasonDef)
			{
				currentSeason = ((seasonDef == null) ? null : new Season(seasonDef));
				gotQuestExpThisWeek = 0;
				SeasonChangedDuringInit = true;
				applicationState.savingManager.SetProfileDirtyToSave();
			}
		}
	}

	public SeasonDef GetSeasonAtUtc(DateTime utcTime)
	{
		return base.applicationState.assetCatalog.SeasonDefCatalog.ValidAssets.OrderBy((SeasonDef s) => s.endTime.UtcDateTime).FirstOrDefault((SeasonDef s) => utcTime < s.endTime.UtcDateTime);
	}

	public bool IsInSameSeason(DateTime timeA, DateTime timeB)
	{
		SeasonDef seasonAtUtc = GetSeasonAtUtc(timeA);
		if (seasonAtUtc != null)
		{
			return seasonAtUtc == GetSeasonAtUtc(timeB);
		}
		return false;
	}

	public DateTime? GetCurrentSeasonEndTimeUtc()
	{
		SeasonDef seasonAtUtc = GetSeasonAtUtc(DateTime.UtcNow);
		if (!(seasonAtUtc == null))
		{
			return seasonAtUtc.endTime.UtcDateTime;
		}
		return null;
	}

	private SeasonDef GetCurrentSeason()
	{
		return GetSeasonAtUtc(DateTime.UtcNow);
	}
}
