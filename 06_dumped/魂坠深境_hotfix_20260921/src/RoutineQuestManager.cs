using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using GameRandom;
using UnityEngine;

public class RoutineQuestManager : GlobalGameManager, IFrameUpdate
{
	public class RoutineQuestUpdate
	{
		[SaveField(null, null)]
		public string questGuid;

		[SaveField(null, null)]
		public int addProgress;
	}

	[SaveField(null, null)]
	public List<RoutineQuest> currentSeasonQuests;

	[SaveField(null, null)]
	public List<RoutineQuest> currentWeekQuests;

	[SaveField(null, null)]
	public List<RoutineQuest> currentDailyQuests;

	[SaveField(null, null)]
	public int winGameCountToday;

	[SaveField(null, null)]
	public int finishedGameCountToday;

	[SaveField(null, null)]
	public List<RoutineQuestUpdate> cachedRoutineQuestUpdates;

	[SaveField(null, null)]
	public int questReplaceCountToday;

	private RoutineQuestProgressBuffer networkProgressBuffer = new RoutineQuestProgressBuffer();

	public List<RoutineQuest> AllRoutineQuests => currentSeasonQuests.Concat(currentWeekQuests).Concat(currentDailyQuests).ToList();

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (networkProgressBuffer == null)
		{
			networkProgressBuffer = new RoutineQuestProgressBuffer();
		}
		if (cachedRoutineQuestUpdates == null)
		{
			cachedRoutineQuestUpdates = new List<RoutineQuestUpdate>();
		}
		if (applicationState.seasonManager.SeasonChangedDuringInit)
		{
			ResetForSeasonChange();
		}
		else if (currentWeekQuests == null || currentDailyQuests == null)
		{
			ResetQuest();
		}
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeGameEnd, _OnBeforeGameEnd);
	}

	public override void Dispose()
	{
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeGameEnd, _OnBeforeGameEnd);
	}

	private void ResetForSeasonChange()
	{
		cachedRoutineQuestUpdates.Clear();
		ClearPendingNetworkProgress("SeasonChanged");
		ResetQuest();
	}

	public void AddCachedRoutineQuestUpdate(RoutineQuest quest, int addProgress)
	{
		RoutineQuestUpdate routineQuestUpdate = cachedRoutineQuestUpdates.FirstOrDefault((RoutineQuestUpdate c) => c.questGuid == quest.guid);
		if (routineQuestUpdate != null)
		{
			routineQuestUpdate.addProgress += addProgress;
			return;
		}
		cachedRoutineQuestUpdates.Add(new RoutineQuestUpdate
		{
			questGuid = quest.guid,
			addProgress = addProgress
		});
	}

	public void ApplyRoutineQuestUpdate(params RoutineQuestUpdate[] updates)
	{
		if (updates.IsNullOrEmpty())
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (RoutineQuestUpdate routineQuestUpdate in updates)
		{
			if (routineQuestUpdate != null)
			{
				RoutineQuest questState = GetQuestState(routineQuestUpdate.questGuid);
				if (!(questState?.def == null) && !questState.AllLayerFinished)
				{
					networkProgressBuffer.Queue(routineQuestUpdate.questGuid, routineQuestUpdate.addProgress, questState.progress, questState.def.AllLayerSumProgress, realtimeSinceStartup);
				}
			}
		}
	}

	public void FrameUpdate()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!networkProgressBuffer.CanFlush(realtimeSinceStartup))
		{
			return;
		}
		Dictionary<string, int> sentUpdates = networkProgressBuffer.BeginFlush(ValidateBufferedProgress);
		if (sentUpdates.Count == 0)
		{
			return;
		}
		int generation = networkProgressBuffer.Generation;
		RoutineQuestUpdate[] array = sentUpdates.Select((KeyValuePair<string, int> pair) => new RoutineQuestUpdate
		{
			questGuid = pair.Key,
			addProgress = pair.Value
		}).ToArray();
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug($"[RoutineQuestProgressBuffer] Flush quests={array.Length}, totalDelta={array.Sum((RoutineQuestUpdate update) => update.addProgress)}");
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			new GeneralGameAsyncOperationRequestOnly<AddRoutineQuestStateProgressRequest>(new AddRoutineQuestStateProgressRequest(array), base.applicationState.ChillyRoomSDK.Routine.AddRoutineQuestStateProgressAsync).OnComplete(delegate
			{
				ApplySentProgress(sentUpdates, generation);
			}).OnFault(delegate
			{
				HandleProgressRequestFault(generation);
			}).HideFaultPanel()
				.DoesNotBlockLoading()
				.EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_quest_progress);
		}
		else
		{
			ApplySentProgress(sentUpdates, generation);
		}
	}

	public void ClearPendingNetworkProgress(string reason = null)
	{
		if (networkProgressBuffer == null)
		{
			networkProgressBuffer = new RoutineQuestProgressBuffer();
		}
		networkProgressBuffer.Clear();
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug(string.Format("[RoutineQuestProgressBuffer] Clear reason={0} generation={1}", reason ?? "Unknown", networkProgressBuffer.Generation));
		}
	}

	private int ValidateBufferedProgress(string questGuid, int bufferedProgress)
	{
		RoutineQuest questState = GetQuestState(questGuid);
		if (questState?.def == null || questState.AllLayerFinished)
		{
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[RoutineQuestProgressBuffer] Drop stale quest guid=" + questGuid);
			}
			return 0;
		}
		return Mathf.Clamp(bufferedProgress, 0, Mathf.Max(0, questState.def.AllLayerSumProgress - questState.progress));
	}

	private void ApplySentProgress(IReadOnlyDictionary<string, int> sentUpdates, int generation)
	{
		if (generation != networkProgressBuffer.Generation)
		{
			return;
		}
		try
		{
			foreach (KeyValuePair<string, int> sentUpdate in sentUpdates)
			{
				RoutineQuest questState = GetQuestState(sentUpdate.Key);
				if (!(questState?.def == null))
				{
					questState.progress += sentUpdate.Value;
					if (questState.progress > questState.def.AllLayerSumProgress)
					{
						questState.progress = questState.def.AllLayerSumProgress;
					}
				}
			}
		}
		finally
		{
			networkProgressBuffer.CompleteSuccess(generation, Time.realtimeSinceStartup);
		}
	}

	private void HandleProgressRequestFault(int generation)
	{
		(int, float)? tuple = networkProgressBuffer.Fail(generation, Time.realtimeSinceStartup);
		if (tuple.HasValue && TraceLog.EnabledLog)
		{
			TraceLog.Warn($"[RoutineQuestProgressBuffer] Retry step={tuple.Value.Item1}, delay={tuple.Value.Item2}s, quests={networkProgressBuffer.PendingCount}");
		}
	}

	public void InitAllRoutineQuestObserve()
	{
		foreach (RoutineQuest allRoutineQuest in AllRoutineQuests)
		{
			if (allRoutineQuest?.def?.node != null && !allRoutineQuest.AllLayerFinished)
			{
				allRoutineQuest.def.node.InitObserve(allRoutineQuest, base.applicationState);
			}
		}
	}

	public void ResetQuest()
	{
		currentSeasonQuests = new List<RoutineQuest>();
		if (base.applicationState.seasonManager.currentSeason != null)
		{
			foreach (RoutineQuestDef questDef in base.applicationState.seasonManager.currentSeason.def.seasonRoutineQuests)
			{
				if (questDef.GenerateAble(base.applicationState) && currentSeasonQuests.All((RoutineQuest q) => q.def != questDef))
				{
					currentSeasonQuests.Add(new RoutineQuest(questDef, base.applicationState));
				}
			}
		}
		currentWeekQuests = new List<RoutineQuest>();
		foreach (FortressDataManager.RoutineQuestPool weeklyRoutineQuestPool in ScriptableObjectSingleton<FortressDataManager>.instance.weeklyRoutineQuestPools)
		{
			List<RoutineQuestDef> list = weeklyRoutineQuestPool.quests.Where((RoutineQuestDef q) => currentWeekQuests.All((RoutineQuest cq) => cq.def != q) && q.GenerateAble(base.applicationState)).ToList();
			if (list.Count != 0)
			{
				RoutineQuestDef def = list.SelectRandom();
				currentWeekQuests.Add(new RoutineQuest(def, base.applicationState));
			}
		}
		if (currentDailyQuests == null)
		{
			currentDailyQuests = new List<RoutineQuest>();
		}
		foreach (RoutineQuestDef fixedQuestDef in ScriptableObjectSingleton<FortressDataManager>.instance.fixedDailyQuests)
		{
			if (fixedQuestDef.GenerateAble(base.applicationState) && currentDailyQuests.All((RoutineQuest cq) => cq.def != fixedQuestDef))
			{
				currentDailyQuests.Add(new RoutineQuest(fixedQuestDef, base.applicationState));
			}
		}
		int num = ScriptableObjectSingleton<FortressDataManager>.instance.dailyRoutineQuestMaxCount - currentDailyQuests.Count;
		if (num > 0)
		{
			List<RoutineQuestDef> list2 = ScriptableObjectSingleton<FortressDataManager>.instance.dailyRoutineQuestPool.quests.Where((RoutineQuestDef q) => currentDailyQuests.All((RoutineQuest cq) => cq.def != q) && q.GenerateAble(base.applicationState)).ToList();
			if (list2.Count >= num)
			{
				foreach (RoutineQuestDef item in list2.SelectRandomCountUnique(num))
				{
					currentDailyQuests.Add(new RoutineQuest(item, base.applicationState));
				}
			}
		}
		base.applicationState.savingManager.SetProfileDirtyToSave();
		base.applicationState.eventManager.RoutineQuestRefreshedEvent.TriggerEvent();
	}

	public RoutineQuest GetQuestState(string guid)
	{
		object obj = currentSeasonQuests?.FirstOrDefault((RoutineQuest quest) => quest.guid == guid);
		if (obj == null)
		{
			obj = currentWeekQuests?.FirstOrDefault((RoutineQuest quest) => quest.guid == guid);
			if (obj == null)
			{
				List<RoutineQuest> list = currentDailyQuests;
				if (list == null)
				{
					return null;
				}
				obj = list.FirstOrDefault((RoutineQuest quest) => quest.guid == guid);
			}
		}
		return (RoutineQuest)obj;
	}

	private void _OnBeforeGameEnd(GameEvent.Type type, GameEvent eve)
	{
		if (!eve.isAfterIntroFirstGame)
		{
			if (eve.succesGame)
			{
				winGameCountToday++;
			}
			finishedGameCountToday++;
		}
		ApplyRoutineQuestUpdate(cachedRoutineQuestUpdates.ToArray());
		cachedRoutineQuestUpdates.Clear();
	}

	public MatterInfo RefreshMatter()
	{
		return ScriptableObjectSingleton<FortressDataManager>.instance.refreshDailyQuestMatter;
	}

	public bool CanRefresh()
	{
		return base.applicationState.matterManager.HasEnoughMatter(ScriptableObjectSingleton<FortressDataManager>.instance.refreshDailyQuestMatter);
	}

	public void GetReward(RoutineQuest quest, Action<GlobalRewardPackage> onComplete)
	{
		if (!quest.GetRewardAble)
		{
			return;
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			GetRoutineQuestBonusRequest request = new GetRoutineQuestBonusRequest(quest);
			GeneralGameAsyncOperation<GetRoutineQuestBonusRequest, GetRoutineQuestBonusResult> op = new GeneralGameAsyncOperation<GetRoutineQuestBonusRequest, GetRoutineQuestBonusResult>(request, base.applicationState.ChillyRoomSDK.Routine.GetRoutineQuestBonusAsync);
			op.OnComplete(delegate
			{
				GlobalRewardPackage rewardPackage = op.OpResult.GiveRoutineQuestRewards(base.applicationState);
				AfterGetReward(rewardPackage);
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_quest_reward);
		}
		else
		{
			GlobalRewardPackage questReward = quest.GetQuestReward(base.applicationState);
			AfterGetReward(questReward);
		}
		void AfterGetReward(GlobalRewardPackage rewardPackage)
		{
			if (quest.AllLayerFinished)
			{
				currentDailyQuests.Remove(quest);
			}
			onComplete?.Invoke(rewardPackage);
		}
	}

	public void ReplaceQuest(RoutineQuest quest, Action<RoutineQuest> onComplete)
	{
		if (!base.applicationState.matterManager.HasEnoughMatter(ScriptableObjectSingleton<FortressDataManager>.instance.refreshDailyQuestMatter))
		{
			return;
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			ReplaceDailyQuestRequest request = new ReplaceDailyQuestRequest
			{
				ToReplaceQuestGuid = quest.guid
			};
			GeneralGameAsyncOperation<ReplaceDailyQuestRequest, ReplaceDailyQuestResult> op = GameAsyncOperation.Get(request, base.applicationState.ChillyRoomSDK.Routine.ReplaceDailyQuestAsync);
			op.OnComplete(delegate
			{
				if (op.OpResult.NewDailyRoutineQuestState != null)
				{
					NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
					RoutineQuest newQuest = op.OpResult.NewDailyRoutineQuestState.ToClient(context);
					AfterReplaceQuest(newQuest, op.OpResult.Matter.ToClient(context));
				}
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_refreshing_quest);
			return;
		}
		RoutineQuestDef def = ScriptableObjectSingleton<FortressDataManager>.instance.dailyRoutineQuestPool.quests.Where((RoutineQuestDef qd) => base.applicationState.routineQuestManager.currentDailyQuests.All((RoutineQuest q) => q.def != qd)).ToList().SelectRandom();
		AfterReplaceQuest(new RoutineQuest(def, base.applicationState), ScriptableObjectSingleton<FortressDataManager>.instance.refreshDailyQuestMatter);
		void AfterReplaceQuest(RoutineQuest newQuest, MatterInfo matterInfo)
		{
			RoutineQuest oldQuest = quest;
			newQuest.def.node.InitObserve(newQuest, base.applicationState);
			questReplaceCountToday++;
			int index = currentDailyQuests.IndexOf(quest);
			currentDailyQuests.Remove(quest);
			currentDailyQuests.Insert(index, newQuest);
			base.applicationState.matterManager.ConsumeMatter(matterInfo, "刷新任务");
			ThinkingAnalyticsExtends.TrackFortressReplaceDailyQuest(base.applicationState, oldQuest, newQuest);
			onComplete?.Invoke(newQuest);
		}
	}
}
