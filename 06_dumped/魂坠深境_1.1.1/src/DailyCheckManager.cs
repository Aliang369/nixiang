using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyCheckManager : GlobalGameManager
{
	[SaveField(null, null)]
	public WeekCheckInState weekCheckState;

	[SaveField(null, null)]
	public List<MonthCheckInState> monthCheckState;

	[NonSerialized]
	private DailyCheckInServerDelegate dailyCheckInServer;

	private DailyCheckInServerDelegate DailyCheckInServer
	{
		get
		{
			if (dailyCheckInServer == null)
			{
				dailyCheckInServer = new DailyCheckInServerDelegate(base.applicationState);
			}
			return dailyCheckInServer;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		InitializeDailyCheckIn();
		InitializeMonthCheckIn();
	}

	public IEnumerator EnterFortressDailyCheckInCor(Action beforeVisibleInterruption)
	{
		if (!base.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableDailyCheckIn))
		{
			yield break;
		}
		FortressUIManager uiManager = base.applicationState.fortressState.uiManager;
		MonthCheckPanel monthCheckPanel = uiManager.monthCheckPanel;
		GeneralGameAsyncOperationResultOnly<DailyCheckInResult> op = new GeneralGameAsyncOperationResultOnly<DailyCheckInResult>(DailyCheckInServer.TriggerDailyCheckInAsync);
		op.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_daily_check_in);
		yield return new WaitUntil(() => op.isOver);
		if (!op.isComplete)
		{
			TraceLog.Error("[DailyCheckManager] EnterFortressDailyCheckInCor() failed - " + op.errorCode);
			yield break;
		}
		DailyCheckInResult checkInResult = op.OpResult;
		if (checkInResult == null)
		{
			TraceLog.Error("[DailyCheckManager] EnterFortressDailyCheckInCor() checkInResult == null");
			yield break;
		}
		base.applicationState.eventManager.dailyCheckInEvent.TriggerEvent(checkInResult);
		if (checkInResult.festivalCheckInResult != null)
		{
			beforeVisibleInterruption?.Invoke();
			FestivalCheckInPanel festivalCheckInPanel = SelectFestivalPanel(checkInResult.festivalCheckInResult.festivalDef);
			yield return festivalCheckInPanel.AutoGive(checkInResult.festivalCheckInResult);
			ThinkingAnalyticsExtends.TrackFestivalCheckIn(base.applicationState, checkInResult.festivalCheckInResult);
		}
		if (checkInResult.monthCheckInResult != null)
		{
			beforeVisibleInterruption?.Invoke();
			yield return monthCheckPanel.AutoGive(checkInResult.monthCheckInResult, checkInResult.monthCardCheckInResult);
			ThinkingAnalyticsExtends.TrackMonthCheckIn(base.applicationState, checkInResult.monthCheckInResult);
		}
		if (checkInResult.forerunnerShareRewardResult != null)
		{
			GlobalRewardPackage globalRewardPackage = checkInResult.forerunnerShareRewardResult.TryGiveRewards(base.applicationState);
			base.applicationState.forerunnerManager.ClearPendingShareReward();
			base.fortressState.uiManager.mainUIPanel.SetDirty();
			if (globalRewardPackage != null)
			{
				beforeVisibleInterruption?.Invoke();
				base.fortressState.uiManager.globalRewardPackageDisplay.Setup(globalRewardPackage);
			}
			base.applicationState.forerunnerManager.RefreshForerunnerProfileData(delegate
			{
				base.fortressState.uiManager.mainUIPanel.SetDirty();
			});
		}
		yield return new WaitUntil(() => !uiManager.festivalCheckInPanel.gameObject.activeInHierarchy && !uiManager.carnivalCheckInPanel.gameObject.activeInHierarchy && !monthCheckPanel.gameObject.activeInHierarchy);
	}

	private void InitializeDailyCheckIn()
	{
		if (weekCheckState == null)
		{
			List<FestivalDef> festivalChain = ScriptableObjectSingleton<CalendarDataManager>.instance.festivalChain;
			FestivalDef festivalDef = ((festivalChain != null && festivalChain.Count > 0 && festivalChain[0] != null) ? festivalChain[0] : ScriptableObjectSingleton<CalendarDataManager>.instance.currentFestivalDef);
			if (festivalDef != null)
			{
				weekCheckState = new WeekCheckInState(festivalDef);
			}
		}
	}

	public FestivalDef GetDisplayFestival()
	{
		if (weekCheckState?.festivalDef != null && weekCheckState.checkInDays < weekCheckState.festivalDef.RewardDayCount)
		{
			return weekCheckState.festivalDef;
		}
		return FindNextOpenFestival();
	}

	private FestivalDef FindNextOpenFestival()
	{
		List<FestivalDef> festivalChain = ScriptableObjectSingleton<CalendarDataManager>.instance.festivalChain;
		if (festivalChain == null)
		{
			return null;
		}
		for (int i = ((weekCheckState?.festivalDef == null) ? (-1) : festivalChain.IndexOf(weekCheckState.festivalDef)) + 1; i < festivalChain.Count; i++)
		{
			FestivalDef festivalDef = festivalChain[i];
			if (!(festivalDef == null))
			{
				string text = festivalDef.limitedTimeActivity?.FullID;
				if (string.IsNullOrEmpty(text) || base.applicationState.limitedTimeActivityManager.IsOpen(text))
				{
					return festivalDef;
				}
			}
		}
		return null;
	}

	public void TryAdvanceFestivalChain()
	{
		if (!(weekCheckState?.festivalDef != null) || weekCheckState.checkInDays >= weekCheckState.festivalDef.RewardDayCount)
		{
			FestivalDef festivalDef = FindNextOpenFestival();
			if (festivalDef != null)
			{
				weekCheckState = new WeekCheckInState(festivalDef);
			}
		}
	}

	public FestivalCheckInPanel SelectFestivalPanel(FestivalDef festivalDef)
	{
		FortressUIManager uiManager = base.applicationState.fortressState.uiManager;
		if (!(festivalDef != null) || festivalDef.panelType != FestivalDef.PanelType.Carnival)
		{
			return uiManager.festivalCheckInPanel;
		}
		return uiManager.carnivalCheckInPanel;
	}

	private void InitializeMonthCheckIn()
	{
		List<MonthCheckInState> source = monthCheckState ?? new List<MonthCheckInState>();
		monthCheckState = new List<MonthCheckInState>();
		DateTime now = DateTime.Now;
		DateTime dateTime = new DateTime(now.Year, now.Month, 1);
		DateTime dateTime2 = dateTime.AddMonths(-1);
		int num = DateTime.DaysInMonth(dateTime2.Year, dateTime2.Month);
		for (int num2 = (int)(dateTime.DayOfWeek + 6) % 7; num2 > 0; num2--)
		{
			monthCheckState.Add(new MonthCheckInState
			{
				index = 0,
				displayIndex = num - num2 + 1,
				isCollected = false,
				isCurrentMonth = false
			});
		}
		int num3 = Mathf.Min(DateTime.DaysInMonth(now.Year, now.Month), ScriptableObjectSingleton<CalendarDataManager>.instance.monthCheckInRewards.Count);
		int date;
		for (date = 1; date <= num3; date++)
		{
			MonthCheckInState monthCheckInState = source.FirstOrDefault((MonthCheckInState state) => state.index == date && (state.isCurrentMonth || state.displayIndex == 0));
			monthCheckState.Add(new MonthCheckInState
			{
				index = date,
				displayIndex = date,
				isCollected = (monthCheckInState?.isCollected ?? false),
				isCurrentMonth = true,
				isCardReward = false
			});
		}
	}

	public MonthCheckInState GetCurrentMonthCheckState(int day)
	{
		return monthCheckState?.FirstOrDefault((MonthCheckInState state) => state.isCurrentMonth && state.index == day);
	}
}
