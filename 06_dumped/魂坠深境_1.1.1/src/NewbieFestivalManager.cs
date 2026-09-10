using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.ContainerOfSoul;
using Generated.ChillyRoomSdkClient;
using UnityEngine;

public class NewbieFestivalManager : GlobalGameManager
{
	private const int DefaultTotalDays = 12;

	private const string AnalyticsActivityId = "开服活动新生游艺202607";

	public const int PaidGiftPerTicket = 60;

	[SaveField(null, null)]
	public List<RoutineQuest> currentDailyTasks;

	[SaveField(null, null)]
	public List<int> drawnSlotIndexes;

	[SaveField(null, null)]
	public List<NewbieFestivalLocalDrawnSlotIndexes> localDrawnSlotIndexesByDay;

	[SaveField(null, null)]
	public string localActivityId;

	[SaveField(null, null)]
	public int localDayIndex;

	[SaveField(null, null)]
	public int paidGiftProgress;

	[SaveField(null, null)]
	public int localPaidTicketBalance;

	[NonSerialized]
	public bool fetchingInfo;

	[NonSerialized]
	public bool drawing;

	[NonSerialized]
	private bool settlingPaidTicket;

	[NonSerialized]
	private bool pendingSettlePaidTicket;

	[NonSerialized]
	private bool updatingTaskProgress;

	[NonSerialized]
	private GlobalRewardPackage pendingDrawRewardPackage;

	[NonSerialized]
	private GlobalRewardPackage pendingPaidTicketRewardPackage;

	[NonSerialized]
	private int pendingDrawnSlotIndex = -1;

	[NonSerialized]
	private HashSet<string> observedTaskGuids = new HashSet<string>();

	[NonSerialized]
	public NewbieFestivalDef currentActivity;

	[NonSerialized]
	private bool claimingTaskReward;

	public bool hasActivityData => currentActivity != null;

	public bool isEntranceUnlocked => base.applicationState.globalVariableManager?.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel) ?? false;

	public bool isAvailable
	{
		get
		{
			if (hasActivityData && base.applicationState.limitedTimeActivityManager.IsOpen(currentActivity.activityId))
			{
				return isEntranceUnlocked;
			}
			return false;
		}
	}

	public bool isEntranceVisible
	{
		get
		{
			if (hasActivityData && base.applicationState.limitedTimeActivityManager.IsOpen(currentActivity.activityId))
			{
				return isEntranceUnlocked;
			}
			return false;
		}
	}

	public int freeTicketCount => base.applicationState.matterManager.GetMatterCount(currentActivity.freeTicketMatter);

	public int paidTicketCount => base.applicationState.matterManager.GetMatterCount(currentActivity.paidTicketMatter);

	public bool isActivityOpen
	{
		get
		{
			if (hasActivityData)
			{
				return base.applicationState.limitedTimeActivityManager.IsOpen(currentActivity.activityId);
			}
			return false;
		}
	}

	public MatterDef nextDrawTicketMatter
	{
		get
		{
			if (freeTicketCount <= 0)
			{
				return currentActivity.paidTicketMatter;
			}
			return currentActivity.freeTicketMatter;
		}
	}

	public event Action StateChanged;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (currentDailyTasks == null)
		{
			currentDailyTasks = new List<RoutineQuest>();
		}
		if (drawnSlotIndexes == null)
		{
			drawnSlotIndexes = new List<int>();
		}
		if (localDrawnSlotIndexesByDay == null)
		{
			localDrawnSlotIndexesByDay = new List<NewbieFestivalLocalDrawnSlotIndexes>();
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		currentActivity = (from activity in applicationState.assetCatalog.activityCatalog.ValidAssets.OfType<NewbieFestivalDef>()
			orderby activity.startTime.UtcDateTime
			select activity).FirstOrDefault((NewbieFestivalDef activity) => applicationState.limitedTimeActivityManager.IsOpen(activity.activityId));
		if (currentActivity == null)
		{
			currentDailyTasks.Clear();
			drawnSlotIndexes.Clear();
			paidGiftProgress = 0;
			localPaidTicketBalance = 0;
			return;
		}
		int num = GetCurrentDayIndex() % currentActivity.totalDays;
		if (num > ((currentActivity.totalDays > 0) ? currentActivity.totalDays : 12))
		{
			currentActivity = null;
			localActivityId = string.Empty;
			localDayIndex = 0;
			currentDailyTasks.Clear();
			drawnSlotIndexes.Clear();
			paidGiftProgress = 0;
			localPaidTicketBalance = 0;
			return;
		}
		if (!string.Equals(localActivityId, currentActivity.activityId, StringComparison.Ordinal))
		{
			localActivityId = currentActivity.activityId;
			localDrawnSlotIndexesByDay.Clear();
			currentDailyTasks.Clear();
			paidGiftProgress = 0;
			localPaidTicketBalance = 0;
		}
		if (localDayIndex != num)
		{
			localDayIndex = num;
			currentDailyTasks.Clear();
		}
		currentDailyTasks = currentDailyTasks.Where((RoutineQuest quest) => quest?.def != null).ToList();
		drawnSlotIndexes = GetLocalDrawnSlotIndexes(num).ToList();
		RefreshLocalDailyTasks();
		InitAllDailyTaskObserve();
	}

	public int GetCurrentDayIndex()
	{
		return Mathf.FloorToInt((float)(GetCurrentUtcTime() - GetActivityRuntimeTimeUtc(startTime: true)).TotalDays) + 1;
	}

	public int GetRemainingRewardSlots()
	{
		return GetRemainingRewardSlots(GetCurrentDayIndex());
	}

	private int GetRemainingRewardSlots(int dayIndex)
	{
		NewbieFestivalDailyRewardGroup dailyRewardGroupForDayIndex = GetDailyRewardGroupForDayIndex(dayIndex);
		if (dailyRewardGroupForDayIndex == null)
		{
			return 0;
		}
		if (drawnSlotIndexes == null)
		{
			drawnSlotIndexes = new List<int>();
		}
		return Mathf.Max(0, dailyRewardGroupForDayIndex.rewardSlots.Count((NewbieFestivalRewardSlot slot) => !drawnSlotIndexes.Contains(slot.slotIndex)));
	}

	private int GetDailyDrawIndex()
	{
		return drawnSlotIndexes.Count;
	}

	private string GetRewardTier(int dayIndex, int drawnSlotIndex)
	{
		QualityLevel rewardLevel = GetDailyRewardGroupForDayIndex(dayIndex).rewardSlots.Single((NewbieFestivalRewardSlot slot) => slot.slotIndex == drawnSlotIndex).rewardLevel;
		return rewardLevel switch
		{
			QualityLevel.Epic => "epic", 
			QualityLevel.Rare => "rare", 
			QualityLevel.Uncommon => "uncommon", 
			QualityLevel.Common => "common", 
			_ => throw new ArgumentOutOfRangeException("rewardLevel", rewardLevel, null), 
		};
	}

	public bool TryGetEntranceDisplayText(out string text)
	{
		text = string.Empty;
		if (!isEntranceVisible)
		{
			return false;
		}
		string remainingTimeText = GetRemainingTimeText();
		text = remainingTimeText;
		return true;
	}

	public string GetRemainingTimeText()
	{
		if (!isActivityOpen)
		{
			return string.Empty;
		}
		TimeSpan timeSpan = GetActivityRuntimeTimeUtc(startTime: false) - GetCurrentUtcTime();
		if (timeSpan <= TimeSpan.Zero)
		{
			return string.Empty;
		}
		return GetCountdownTimeSpanText(timeSpan);
	}

	public DateTimeOffset? GetActivityEndTimeUtc()
	{
		return new DateTimeOffset(GetActivityRuntimeTimeUtc(startTime: false));
	}

	public DateTimeOffset? GetCurrentDayRefreshTimeUtc()
	{
		return new DateTimeOffset(GetActivityRuntimeTimeUtc(startTime: true).AddDays(GetCurrentDayIndex()));
	}

	public bool ShouldShowEntranceRedDot()
	{
		if (isEntranceVisible)
		{
			if (GetRemainingRewardSlots() <= 0 || freeTicketCount + paidTicketCount <= 0)
			{
				return currentDailyTasks?.Any((RoutineQuest q) => q.GetRewardAble) ?? false;
			}
			return true;
		}
		return false;
	}

	public int GetPaidGiftProgress(RoutineQuest paidTicketTask)
	{
		return paidGiftProgress;
	}

	public RoutineQuest GetTaskState(string guid)
	{
		return currentDailyTasks?.FirstOrDefault((RoutineQuest q) => q.guid == guid);
	}

	public bool ShouldRefreshDailyInfo()
	{
		if (GlobalConfigManager.instance.enableNetwork && base.applicationState.ChillyRoomSDK?.NewbieFestival != null && isActivityOpen)
		{
			if (string.Equals(localActivityId, currentActivity.activityId, StringComparison.Ordinal))
			{
				return localDayIndex != GetCurrentDayIndex();
			}
			return true;
		}
		return false;
	}

	private NewbieFestivalLocalDrawResult BuildLocalDrawResult(int dayIndex)
	{
		NewbieFestivalDailyRewardGroup dailyRewardGroupForDayIndex = GetDailyRewardGroupForDayIndex(dayIndex);
		if (dailyRewardGroupForDayIndex == null)
		{
			return null;
		}
		List<int> localDrawnSlotIndexes = GetLocalDrawnSlotIndexes(dayIndex);
		int num = SelectLocalRewardSlotIndex(dailyRewardGroupForDayIndex, localDrawnSlotIndexes);
		if (num < 0)
		{
			return null;
		}
		GlobalRewardInfo globalRewardInfo = BuildLocalSlotReward(dailyRewardGroupForDayIndex, num);
		if (globalRewardInfo == null || !ConsumeLocalTicket())
		{
			return null;
		}
		localDrawnSlotIndexes.Add(num);
		return new NewbieFestivalLocalDrawResult
		{
			DrawnSlotIndex = num,
			Reward = globalRewardInfo
		};
	}

	public NewbieFestivalDailyRewardGroup GetCurrentDailyRewardGroup()
	{
		return GetDailyRewardGroupForDayIndex(GetCurrentDayIndex());
	}

	private NewbieFestivalDailyRewardGroup GetDailyRewardGroupForDayIndex(int dayIndex)
	{
		if (dayIndex <= 0)
		{
			return null;
		}
		List<NewbieFestivalDailyRewardGroup> list = (from @group in currentActivity.dailyRewardGroups
			where @group?.rewardSlots != null && @group.rewardSlots.Count > 0
			orderby @group.dayIndex
			select @group).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		return list.FirstOrDefault((NewbieFestivalDailyRewardGroup group) => group.dayIndex == dayIndex) ?? list[(dayIndex - 1) % list.Count];
	}

	private void RefreshLocalDailyTasks()
	{
		if (currentDailyTasks == null)
		{
			currentDailyTasks = new List<RoutineQuest>();
		}
		if (currentDailyTasks.Count > 0)
		{
			return;
		}
		foreach (RoutineQuestDef dailyTask in currentActivity.dailyTasks)
		{
			if ((dailyTask.node.ExtraInfoType != RoutineQuestDef.QuestExtraInfoType.RandomOwnedCharacter || base.applicationState.characterManager.GetAllUnlockedCharacterStates().Any()) && (dailyTask.node.ExtraInfoType != RoutineQuestDef.QuestExtraInfoType.RandomOwnedSkill || base.applicationState.globalSkillManager.allSkills.Any()))
			{
				currentDailyTasks.Add(new RoutineQuest(dailyTask, base.applicationState));
			}
		}
	}

	private List<int> GetLocalDrawnSlotIndexes(int dayIndex)
	{
		if (localDrawnSlotIndexesByDay == null)
		{
			localDrawnSlotIndexesByDay = new List<NewbieFestivalLocalDrawnSlotIndexes>();
		}
		NewbieFestivalLocalDrawnSlotIndexes newbieFestivalLocalDrawnSlotIndexes = localDrawnSlotIndexesByDay.FirstOrDefault((NewbieFestivalLocalDrawnSlotIndexes r) => r.DayIndex == dayIndex);
		if (newbieFestivalLocalDrawnSlotIndexes == null)
		{
			newbieFestivalLocalDrawnSlotIndexes = new NewbieFestivalLocalDrawnSlotIndexes
			{
				DayIndex = dayIndex,
				SlotIndexes = new List<int>()
			};
			localDrawnSlotIndexesByDay.Add(newbieFestivalLocalDrawnSlotIndexes);
		}
		NewbieFestivalLocalDrawnSlotIndexes newbieFestivalLocalDrawnSlotIndexes2 = newbieFestivalLocalDrawnSlotIndexes;
		if (newbieFestivalLocalDrawnSlotIndexes2.SlotIndexes == null)
		{
			newbieFestivalLocalDrawnSlotIndexes2.SlotIndexes = new List<int>();
		}
		return newbieFestivalLocalDrawnSlotIndexes.SlotIndexes;
	}

	private bool ConsumeLocalTicket()
	{
		if (base.applicationState.matterManager.ConsumeMatter(currentActivity.freeTicketMatter, 1, "新生游艺抽奖"))
		{
			return true;
		}
		return base.applicationState.matterManager.ConsumeMatter(currentActivity.paidTicketMatter, 1, "新生游艺抽奖");
	}

	private static int SelectLocalRewardSlotIndex(NewbieFestivalDailyRewardGroup dailyRewardGroup, IReadOnlyCollection<int> drawnSlotIndexes)
	{
		List<NewbieFestivalRewardSlot> list = dailyRewardGroup.rewardSlots.Where((NewbieFestivalRewardSlot slot) => !drawnSlotIndexes.Contains(slot.slotIndex)).ToList();
		if (list.Count == 0)
		{
			return -1;
		}
		int num = list.Sum((NewbieFestivalRewardSlot slot) => Mathf.Max(0, slot.weight));
		if (num <= 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)].slotIndex;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		int num3 = 0;
		foreach (NewbieFestivalRewardSlot item in list)
		{
			num3 += Mathf.Max(0, item.weight);
			if (num2 < num3)
			{
				return item.slotIndex;
			}
		}
		return list[list.Count - 1].slotIndex;
	}

	private GlobalRewardInfo BuildLocalSlotReward(NewbieFestivalDailyRewardGroup dailyRewardGroup, int selectedSlotIndex)
	{
		return dailyRewardGroup.rewardSlots.FirstOrDefault((NewbieFestivalRewardSlot slot) => slot.slotIndex == selectedSlotIndex)?.reward.GenerateReward(base.applicationState, GlobalRewardSourceType.Other, $"{currentActivity.activityId}:{GetCurrentDayIndex()}:{selectedSlotIndex}");
	}

	private void InitAllDailyTaskObserve()
	{
		if (currentDailyTasks == null)
		{
			return;
		}
		foreach (RoutineQuest currentDailyTask in currentDailyTasks)
		{
			if (currentDailyTask != null && currentDailyTask.def?.node != null && !currentDailyTask.AllLayerFinished && !observedTaskGuids.Contains(currentDailyTask.guid) && !IsPaidTicketTask(currentDailyTask))
			{
				observedTaskGuids.Add(currentDailyTask.guid);
				currentDailyTask.def.node.InitObserve(currentDailyTask, base.applicationState);
			}
		}
	}

	private bool IsPaidTicketTask(RoutineQuest quest)
	{
		if (quest == null)
		{
			return false;
		}
		return quest.def?.rewards?.OfType<MatterReward>().Any((MatterReward reward) => reward.matterDef == currentActivity.paidTicketMatter) == true;
	}

	public bool TryConsumePendingDrawRewardPackage(out GlobalRewardPackage package)
	{
		package = pendingDrawRewardPackage;
		pendingDrawRewardPackage = null;
		pendingDrawnSlotIndex = -1;
		return package != null;
	}

	public bool TryConsumePendingPaidTicketRewardPackage(out GlobalRewardPackage package)
	{
		package = pendingPaidTicketRewardPackage;
		pendingPaidTicketRewardPackage = null;
		return package != null;
	}

	public bool TryGetPendingDrawnSlotIndex(out int slotIndex)
	{
		slotIndex = pendingDrawnSlotIndex;
		return pendingDrawnSlotIndex >= 0;
	}

	public string CurrentDayRefreshTimeCountdownText()
	{
		return FormatCountdown(ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_day_refresh, GetCurrentDayRefreshTimeUtc());
	}

	public string ActivityCountdownText()
	{
		return FormatCountdown(ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_activity_end, GetActivityEndTimeUtc());
	}

	private string FormatCountdown(string label, DateTimeOffset? targetTimeUtc)
	{
		if (!targetTimeUtc.HasValue)
		{
			return string.Empty;
		}
		TimeSpan timeSpan = targetTimeUtc.Value.UtcDateTime - GetCurrentUtcTime();
		if (!(timeSpan <= TimeSpan.Zero))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_countdown_arg0_arg1.FormatWith(label, GetCountdownTimeSpanText(timeSpan));
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_countdown_finished_arg0.FormatWith(label);
	}

	private static string GetCountdownTimeSpanText(TimeSpan span)
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		if (span.Days < 1)
		{
			return instance.term_arg0_hours.FormatWith(span.Hours) + instance.term_arg0_minutes.FormatWith(span.Minutes);
		}
		return instance.term_arg0_days.FormatWith(span.Days) + instance.term_arg0_hours.FormatWith(span.Hours) + instance.term_arg0_minutes.FormatWith(span.Minutes);
	}

	private static DateTime GetCurrentUtcTime()
	{
		try
		{
			return ServerTimePoller.Instance.GetCurrentTime().UtcDateTime;
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
			return DateTime.UtcNow;
		}
	}

	private DateTime GetActivityRuntimeTimeUtc(bool startTime)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			if (!base.applicationState.limitedTimeActivityManager.TryGetRuntimeConfig(currentActivity.activityId, out var config))
			{
				TraceLog.Error("[NewbieFestivalManager] Missing runtime config for activity: " + currentActivity.activityId);
				if (!startTime)
				{
					return DateTime.MinValue;
				}
				return DateTime.MaxValue;
			}
			if (!startTime)
			{
				return config.EndTimeUtc;
			}
			return config.StartTimeUtc;
		}
		if (!startTime)
		{
			return currentActivity.endTime.UtcDateTime;
		}
		return currentActivity.startTime.UtcDateTime;
	}

	private void SyncCurrentDailyTasks(List<RoutineQuest> questStates)
	{
		currentDailyTasks = questStates?.Where((RoutineQuest quest) => quest?.def != null).ToList() ?? new List<RoutineQuest>();
		if (currentDailyTasks.Count == 0 && currentActivity != null)
		{
			RefreshLocalDailyTasks();
		}
		InitAllDailyTaskObserve();
	}

	private void ApplyTaskState(RoutineQuest quest)
	{
		if (currentDailyTasks == null)
		{
			currentDailyTasks = new List<RoutineQuest>();
		}
		int num = currentDailyTasks.FindIndex((RoutineQuest q) => q.guid == quest.guid);
		if (num >= 0)
		{
			currentDailyTasks[num] = quest;
		}
		else
		{
			currentDailyTasks.Add(quest);
		}
		InitAllDailyTaskObserve();
	}

	public void ApplyTaskUpdate(params RoutineQuestManager.RoutineQuestUpdate[] updates)
	{
		RoutineQuestManager.RoutineQuestUpdate[] array = updates?.Where(delegate(RoutineQuestManager.RoutineQuestUpdate update)
		{
			RoutineQuest taskState2 = GetTaskState(update.questGuid);
			return taskState2 != null && !IsPaidTicketTask(taskState2);
		}).ToArray();
		if (array == null || array.Length == 0)
		{
			return;
		}
		if (GlobalConfigManager.instance.enableNetwork && base.applicationState.ChillyRoomSDK?.NewbieFestival != null)
		{
			if (updatingTaskProgress)
			{
				return;
			}
			updatingTaskProgress = true;
			NewbieFestivalTaskProgressUpdateRequest request = new NewbieFestivalTaskProgressUpdateRequest
			{
				ActivityId = currentActivity.activityId,
				AddProgreses = array.Select((RoutineQuestManager.RoutineQuestUpdate update) => new RoutineQuestStateAddProgress
				{
					QuestGuid = update.questGuid,
					AddProgress = update.addProgress
				}).ToList()
			};
			GeneralGameAsyncOperation<NewbieFestivalTaskProgressUpdateRequest, NewbieFestivalTaskProgressUpdateResponse> op = GameAsyncOperation.Get(request, base.applicationState.ChillyRoomSDK.NewbieFestival.UpdateTaskProgressAsync);
			op.HideFaultPanel().OnComplete(delegate
			{
				foreach (RoutineQuestState item in op.OpResult?.TaskStates?.OrEmptyIfNull())
				{
					RoutineQuest routineQuest = item.ToClient(new NetworkConvertContext(base.applicationState));
					int reachedLayerCount2 = GetReachedLayerCount(GetTaskState(routineQuest.guid));
					ApplyTaskState(routineQuest);
					TrackNewlyReachedLayers(routineQuest, reachedLayerCount2);
				}
				base.applicationState.savingManager?.SetProfileDirtyToSave();
				StateChanged?.Invoke();
			}).OnFault(delegate
			{
				TraceLog.Error("[NewbieFestivalManager] Update task progress failed.");
			})
				.OnOver(delegate
				{
					updatingTaskProgress = false;
				})
				.EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_quest_progress);
			return;
		}
		RoutineQuestManager.RoutineQuestUpdate[] array2 = array;
		foreach (RoutineQuestManager.RoutineQuestUpdate routineQuestUpdate in array2)
		{
			RoutineQuest taskState = GetTaskState(routineQuestUpdate.questGuid);
			if (taskState != null)
			{
				int reachedLayerCount = GetReachedLayerCount(taskState);
				taskState.progress += routineQuestUpdate.addProgress;
				if (taskState.progress > taskState.def.AllLayerSumProgress)
				{
					taskState.progress = taskState.def.AllLayerSumProgress;
				}
				TrackNewlyReachedLayers(taskState, reachedLayerCount);
			}
		}
		base.applicationState.savingManager?.SetProfileDirtyToSave();
		StateChanged?.Invoke();
	}

	public bool TryDraw()
	{
		if (!isAvailable)
		{
			return false;
		}
		if (drawing)
		{
			return false;
		}
		drawing = true;
		int drawDayIndex = GetCurrentDayIndex();
		string drawTicketType = ((freeTicketCount > 0) ? "free" : "paid");
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			NewbieFestivalLocalDrawResult newbieFestivalLocalDrawResult = BuildLocalDrawResult(drawDayIndex);
			if (newbieFestivalLocalDrawResult != null)
			{
				ApplyLocalDraw(newbieFestivalLocalDrawResult);
			}
			drawing = false;
			return newbieFestivalLocalDrawResult != null;
		}
		if (base.applicationState.ChillyRoomSDK?.NewbieFestival == null)
		{
			drawing = false;
			return false;
		}
		NewbieFestivalDrawRequest request = new NewbieFestivalDrawRequest
		{
			ActivityId = currentActivity.activityId
		};
		GeneralGameAsyncOperation<NewbieFestivalDrawRequest, NewbieFestivalDrawResponse> op = new GeneralGameAsyncOperation<NewbieFestivalDrawRequest, NewbieFestivalDrawResponse>(request, base.applicationState.ChillyRoomSDK.NewbieFestival.DrawAsync);
		op.HideFaultPanel().OnComplete(delegate
		{
			ApplyDraw(op.OpResult);
		}).OnFault(delegate
		{
			TraceLog.Error("[NewbieFestivalManager] Draw failed.");
		})
			.OnOver(delegate
			{
				drawing = false;
			})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_festival_draw);
		return true;
		void ApplyDraw(NewbieFestivalDrawResponse response)
		{
			if (response != null)
			{
				ConsumeLocalTicket();
			}
			GlobalRewardInfo globalRewardInfo = response?.Reward?.ToClient(new NetworkConvertContext(base.applicationState));
			globalRewardInfo?.Give(base.applicationState);
			pendingDrawRewardPackage = ((globalRewardInfo == null) ? null : new GlobalRewardPackage(globalRewardInfo));
			pendingDrawnSlotIndex = response?.DrawnSlotIndex ?? (-1);
			drawnSlotIndexes = response?.DrawnSlotIndexes ?? new List<int>();
			if (response != null)
			{
				int remainingRewardSlots = GetRemainingRewardSlots(drawDayIndex);
				ThinkingAnalyticsExtends.TrackNewbieFestivalDraw(base.applicationState, GetDailyDrawIndex(), drawDayIndex.ToString(), GetRewardTier(drawDayIndex, response.DrawnSlotIndex), drawTicketType, freeTicketCount, paidTicketCount, remainingRewardSlots == 0, globalRewardInfo);
			}
			base.applicationState.savingManager?.SetProfileDirtyToSave();
			StateChanged?.Invoke();
		}
		void ApplyLocalDraw(NewbieFestivalLocalDrawResult result)
		{
			result.Reward.Give(base.applicationState);
			pendingDrawRewardPackage = new GlobalRewardPackage(result.Reward);
			pendingDrawnSlotIndex = result.DrawnSlotIndex;
			drawnSlotIndexes = GetLocalDrawnSlotIndexes(drawDayIndex).ToList();
			int remainingRewardSlots = GetRemainingRewardSlots(drawDayIndex);
			ThinkingAnalyticsExtends.TrackNewbieFestivalDraw(base.applicationState, GetDailyDrawIndex(), drawDayIndex.ToString(), GetRewardTier(drawDayIndex, result.DrawnSlotIndex), drawTicketType, freeTicketCount, paidTicketCount, remainingRewardSlots == 0, result.Reward);
			base.applicationState.savingManager?.SetProfileDirtyToSave();
			StateChanged?.Invoke();
		}
	}

	public void ApplyInfo(GetNewbieFestivalInfoResponse info)
	{
		if (info == null)
		{
			currentActivity = null;
			drawnSlotIndexes = new List<int>();
			currentDailyTasks = new List<RoutineQuest>();
			paidGiftProgress = 0;
			localActivityId = string.Empty;
			localDayIndex = 0;
			base.applicationState.savingManager?.SetProfileDirtyToSave();
			StateChanged?.Invoke();
			return;
		}
		if (!string.IsNullOrEmpty(info?.ActivityId))
		{
			currentActivity = (base.applicationState?.assetCatalog?.activityCatalog?.ValidAssets)?.OfType<NewbieFestivalDef>().FirstOrDefault((NewbieFestivalDef def) => string.Equals(def.activityId, info.ActivityId, StringComparison.Ordinal));
		}
		else
		{
			currentActivity = null;
		}
		drawnSlotIndexes = info.DrawnSlotIndexes ?? new List<int>();
		paidGiftProgress = info.PaidGiftProgress;
		localActivityId = currentActivity?.activityId ?? string.Empty;
		if (base.applicationState.limitedTimeActivityManager.IsReady)
		{
			localDayIndex = ((!(currentActivity == null)) ? GetCurrentDayIndex() : 0);
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		List<RoutineQuest> questStates = info.DailyTaskStates?.Select((RoutineQuestState q) => q?.ToClient(context)).WhereNotNull().ToList() ?? new List<RoutineQuest>();
		SyncCurrentDailyTasks(questStates);
		base.applicationState.savingManager?.SetProfileDirtyToSave();
		StateChanged?.Invoke();
		SettlePaidTicket();
	}

	public bool TryRefreshDailyInfoIfNeeded()
	{
		if (fetchingInfo || !ShouldRefreshDailyInfo())
		{
			return false;
		}
		fetchingInfo = true;
		GetNewbieFestivalInfoRequest request = new GetNewbieFestivalInfoRequest
		{
			ActivityId = currentActivity.activityId
		};
		WaitPanelBase.Token waitPanel = BootstrapExtra.instance.waitPanelBase.Show(base.textData.gameAsyncOperationText.term_NewbieFestivalInfo);
		GeneralGameAsyncOperation<GetNewbieFestivalInfoRequest, GetNewbieFestivalInfoResponse> op = GameAsyncOperation.Get(request, base.applicationState.ChillyRoomSDK.NewbieFestival.GetInfoAsync);
		op.HideFaultPanel().OnComplete(delegate
		{
			ApplyInfo(op.OpResult);
		}).OnFault(delegate
		{
			TraceLog.Error("[NewbieFestivalManager] Fetch info failed.");
		})
			.OnOver(delegate
			{
				BootstrapExtra.instance.waitPanelBase.Hide(waitPanel);
				fetchingInfo = false;
				StateChanged?.Invoke();
			})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_festival);
		StateChanged?.Invoke();
		return true;
	}

	public void SettlePaidTicket()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			SettleLocalPaidTicket();
		}
		else
		{
			if (!GlobalConfigManager.instance.enableNetwork || base.crSDK?.NewbieFestival == null)
			{
				return;
			}
			if (settlingPaidTicket)
			{
				pendingSettlePaidTicket = true;
				return;
			}
			settlingPaidTicket = true;
			GeneralGameAsyncOperationResultOnly<NewbieFestivalPaidTicketSettleResponse> op = GameAsyncOperation.Get(base.crSDK.NewbieFestival.SettlePaidTicketAsync);
			op.HideFaultPanel().OnComplete(delegate
			{
				paidGiftProgress = op.OpResult.PaidGiftProgress;
				GlobalRewardInfo globalRewardInfo = op.OpResult.Reward?.ToClient(new NetworkConvertContext(base.applicationState));
				if (globalRewardInfo != null)
				{
					globalRewardInfo.Give(base.applicationState);
					SetPendingPaidTicketReward(globalRewardInfo);
					TrackPaidTicketSettlement(globalRewardInfo);
				}
				base.applicationState.savingManager?.SetProfileDirtyToSave();
				StateChanged?.Invoke();
			}).OnFault(delegate
			{
				TraceLog.Error("[NewbieFestivalManager] Settle paid ticket failed.");
			})
				.OnOver(delegate
				{
					settlingPaidTicket = false;
					if (pendingSettlePaidTicket)
					{
						pendingSettlePaidTicket = false;
						SettlePaidTicket();
					}
				})
				.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_settling_festival_ticket);
		}
	}

	public void AddLocalPaidPurchaseProgress(int realMoneyCost, int purchaseTimes)
	{
		if (!GlobalConfigManager.instance.enableNetwork && isActivityOpen && realMoneyCost > 0 && purchaseTimes > 0)
		{
			int num = realMoneyCost * purchaseTimes * 10;
			int num2 = paidGiftProgress + num;
			localPaidTicketBalance += num2 / 60;
			paidGiftProgress = num2 % 60;
			base.applicationState.savingManager?.SetProfileDirtyToSave();
			StateChanged?.Invoke();
		}
	}

	private void SettleLocalPaidTicket()
	{
		if (isActivityOpen && localPaidTicketBalance > 0)
		{
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo
			{
				matter = new MatterInfo(currentActivity.paidTicketMatter, localPaidTicketBalance),
				rewardSourceType = GlobalRewardSourceType.Other,
				rewardSourceId = currentActivity.activityId
			};
			globalRewardInfo.Give(base.applicationState);
			SetPendingPaidTicketReward(globalRewardInfo);
			TrackPaidTicketSettlement(globalRewardInfo);
			localPaidTicketBalance = 0;
			base.applicationState.savingManager?.SetProfileDirtyToSave();
			StateChanged?.Invoke();
		}
	}

	private void SetPendingPaidTicketReward(GlobalRewardInfo reward)
	{
		pendingPaidTicketRewardPackage = new GlobalRewardPackage(reward);
	}

	public void ClaimTaskReward(RoutineQuest quest, Action<GlobalRewardPackage> onComplete)
	{
		if (quest == null || !quest.GetRewardAble)
		{
			return;
		}
		if (GlobalConfigManager.instance.enableNetwork && ChillyRoomService.GetSdk()?.NewbieFestival != null)
		{
			if (claimingTaskReward)
			{
				return;
			}
			claimingTaskReward = true;
			NewbieFestivalTaskClaimRequest request = new NewbieFestivalTaskClaimRequest
			{
				ActivityId = currentActivity.activityId,
				QuestGuid = quest.guid
			};
			GeneralGameAsyncOperation<NewbieFestivalTaskClaimRequest, NewbieFestivalTaskClaimResponse> op = GameAsyncOperation.Get(request, base.applicationState.ChillyRoomSDK.NewbieFestival.ClaimTaskBonusAsync);
			op.HideFaultPanel().OnComplete(delegate
			{
				ApplyTaskState(op.OpResult.QuestState.ToClient(new NetworkConvertContext(base.applicationState)));
				GlobalRewardInfo globalRewardInfo = op.OpResult.Reward?.ToClient(new NetworkConvertContext(base.applicationState));
				if (globalRewardInfo != null)
				{
					globalRewardInfo.Give(base.applicationState);
					TrackTaskAction(quest, "claim", new List<GlobalRewardInfo> { globalRewardInfo });
					onComplete?.Invoke(new GlobalRewardPackage(globalRewardInfo));
				}
				else
				{
					onComplete?.Invoke(null);
				}
				base.applicationState.savingManager?.SetProfileDirtyToSave();
				StateChanged?.Invoke();
			}).OnFault(delegate
			{
				TraceLog.Error("[NewbieFestivalManager] Claim task reward failed.");
			})
				.OnOver(delegate
				{
					claimingTaskReward = false;
				})
				.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_festival_reward);
			return;
		}
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		while (quest.GetRewardAble)
		{
			GlobalRewardPackage questReward = quest.GetQuestReward(base.applicationState);
			TrackTaskAction(quest, "claim", questReward.rewards);
			if (!questReward.rewards.IsNullOrEmpty())
			{
				list.AddRange(questReward.rewards);
			}
		}
		base.applicationState.savingManager?.SetProfileDirtyToSave();
		StateChanged?.Invoke();
		onComplete?.Invoke((list.Count == 0) ? null : new GlobalRewardPackage(list));
	}

	private void TrackNewlyReachedLayers(RoutineQuest quest, int previousReachedLayerCount)
	{
		if (quest == null)
		{
			return;
		}
		int reachedLayerCount = GetReachedLayerCount(quest);
		for (int i = previousReachedLayerCount; i < reachedLayerCount; i++)
		{
			List<GlobalRewardInfo> rewards = quest.def.rewards.Select((GlobalRewardSetup reward) => reward.GenerateReward(base.applicationState, GlobalRewardSourceType.Other, "开服活动新生游艺202607")).ToList();
			TrackTaskAction(quest, "achieve", rewards);
		}
	}

	private void TrackPaidTicketSettlement(GlobalRewardInfo reward)
	{
		RoutineQuest routineQuest = currentDailyTasks?.SingleOrDefault(IsPaidTicketTask);
		if (routineQuest != null)
		{
			List<GlobalRewardInfo> rewards = new List<GlobalRewardInfo> { reward };
			TrackTaskAction(routineQuest, "achieve", rewards);
			TrackTaskAction(routineQuest, "claim", rewards);
		}
	}

	private void TrackTaskAction(RoutineQuest quest, string actionType, List<GlobalRewardInfo> rewards)
	{
		ThinkingAnalyticsExtends.TrackActivityTaskAction(base.applicationState, "开服活动新生游艺202607", quest.def.FullID, GetAnalyticsTaskType(quest), actionType, rewards);
	}

	private string GetAnalyticsTaskType(RoutineQuest quest)
	{
		if (IsPaidTicketTask(quest))
		{
			return "paid_ticket";
		}
		CheckNode checkNode = ((quest.def.node as RoutineQuestDef.GlobalObserveQuestNode)?.observer as GameEventObserver)?.checkNode;
		if (!(checkNode is CheckLocationType))
		{
			if (checkNode is CheckGameEndSuccess)
			{
				return "game_ticket_win";
			}
			TraceLog.Error("[NewbieFestivalManager] Unknown task type for analytics: " + quest.def.FullID);
			return "unknown";
		}
		return "game_ticket_play";
	}

	private static int GetReachedLayerCount(RoutineQuest quest)
	{
		if (quest?.def == null || quest.def.maxProgress <= 0)
		{
			return 0;
		}
		return Mathf.Clamp(quest.progress / quest.def.maxProgress, 0, quest.def.maxLayer);
	}
}
