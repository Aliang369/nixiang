using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress.AchievementDisplay;
using ContainerOfSoul.Display.Fortress.DotManagers;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

public class SeasonQuestPanel : UIBase
{
	public class WeeklyQuestModule : RedDotModule
	{
		protected override bool? CheckImpl()
		{
			return RedDotModule.applicationState.routineQuestManager.currentWeekQuests.Any((RoutineQuest w) => w.GetRewardAble);
		}
	}

	public class SeasonQuestModule : RedDotModule
	{
		protected override bool? CheckImpl()
		{
			return RedDotModule.applicationState.routineQuestManager.currentSeasonQuests.Any((RoutineQuest s) => s.GetRewardAble);
		}
	}

	public RoutineQuestCell questCellPrefab;

	public RectTransform questCellParent;

	public SwitchPanel questTypeSwitchPanel;

	public SwitchTag weeklyQuestTag;

	public SwitchTag seasonQuestTag;

	public HintRedDot weeklyQuestRedDot;

	public HintRedDot seasonQuestRedDot;

	public TextMeshProUGUI[] weeklyQuestRemainTimeTexts;

	public TextMeshProUGUI[] seasonRemainTimeTexts;

	public GameButton getAllQuestRewardButton;

	private List<RoutineQuest> currentQuests;

	private CellUIPrefabPool<RoutineQuestCell, RoutineQuest> questCellPool;

	private SeasonDetailPanel parentPanel;

	protected override void Init()
	{
		InitUI(questTypeSwitchPanel);
		InitUI(weeklyQuestRedDot);
		InitUI(seasonQuestRedDot);
		parentPanel = GetUIInParents<SeasonDetailPanel>();
		GetPrefabPool(ref questCellPool, questCellPrefab);
		SwitchPanel switchPanel = questTypeSwitchPanel;
		switchPanel.onTagSwitched = (Action)Delegate.Combine(switchPanel.onTagSwitched, new Action(_OnTagSwitched));
		questTypeSwitchPanel.Select(weeklyQuestTag);
		getAllQuestRewardButton.onClick.AddListener(_OnGetAllQuestRewardButtonClicked);
		weeklyQuestRedDot.Setup(new WeeklyQuestModule());
		seasonQuestRedDot.Setup(new SeasonQuestModule());
	}

	protected override void Dispose()
	{
		SwitchPanel switchPanel = questTypeSwitchPanel;
		switchPanel.onTagSwitched = (Action)Delegate.Remove(switchPanel.onTagSwitched, new Action(_OnTagSwitched));
	}

	protected override void UpdateDisplay()
	{
		UpdateCurrentQuest();
		questCellPool.Sync(currentQuests);
		TimeSpan span = ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeekPeriod).UtcDateTime - DateTime.UtcNow;
		string weeklyQuestRemainTimeDisplayText = ScriptableObjectSingleton<TextDataPreset>.instance.term_remain_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(span));
		weeklyQuestRemainTimeTexts.ForEach(delegate(TextMeshProUGUI t)
		{
			t.text = weeklyQuestRemainTimeDisplayText;
		});
		TimeSpan span2 = UIBase.applicationState.seasonManager.currentSeason.def.endTime.UtcDateTime - DateTime.UtcNow;
		string seasonRemainTimeDisplayText = ScriptableObjectSingleton<TextDataPreset>.instance.term_remain_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(span2));
		seasonRemainTimeTexts.ForEach(delegate(TextMeshProUGUI t)
		{
			t.text = seasonRemainTimeDisplayText;
		});
		List<RoutineQuest> source = currentQuests.Where((RoutineQuest q) => q.GetRewardAble).ToList();
		getAllQuestRewardButton.gameObject.SetActive(!source.IsNullOrEmpty());
	}

	private void UpdateCurrentQuest()
	{
		if (questTypeSwitchPanel.targetShowTag == weeklyQuestTag)
		{
			currentQuests = UIBase.applicationState.routineQuestManager.currentWeekQuests;
		}
		else if (questTypeSwitchPanel.targetShowTag == seasonQuestTag)
		{
			currentQuests = UIBase.applicationState.routineQuestManager.currentSeasonQuests;
		}
		currentQuests = currentQuests.OrderBy(delegate(RoutineQuest q)
		{
			if (q.GetRewardAble)
			{
				return 0;
			}
			return (!q.AllLayerFinished) ? 100000 : (200000 + q.def.SOindex);
		}).ToList();
	}

	private void _OnGetAllQuestRewardButtonClicked()
	{
		List<RoutineQuest> toGetRewardQuest = currentQuests.Where((RoutineQuest q) => q.GetRewardAble).ToList();
		if (!toGetRewardQuest.IsNullOrEmpty() && GlobalConfigManager.instance.enableNetwork)
		{
			GetRoutineQuestBonusRequest request = new GetRoutineQuestBonusRequest(toGetRewardQuest.ToArray());
			GeneralGameAsyncOperation<GetRoutineQuestBonusRequest, GetRoutineQuestBonusResult> op = new GeneralGameAsyncOperation<GetRoutineQuestBonusRequest, GetRoutineQuestBonusResult>(request, UIBase.applicationState.ChillyRoomSDK.Routine.GetRoutineQuestBonusAsync);
			op.OnComplete(delegate
			{
				GlobalRewardPackage rewardPackage = op.OpResult.GiveRoutineQuestRewards(UIBase.applicationState);
				AfterGetReward(rewardPackage);
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_quest_reward);
		}
		void AfterGetReward(GlobalRewardPackage rewardPackage)
		{
			UIBase.fortressState.uiManager.globalRewardPackageDisplay.Setup(rewardPackage);
			int num = toGetRewardQuest.Sum((RoutineQuest q) => q.sumExp);
			if (num != 0)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_get_exp.FormatWith(num));
			}
			manager.SetDirty();
		}
	}

	private void _OnTagSwitched()
	{
		SetDirty();
		parentPanel.SetDirty();
	}
}
