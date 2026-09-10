using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.RealName.Utils;
using UnityEngine;

public class YuanQiActivityManager : GlobalGameManager
{
	private const string AnalyticsActivityId = "元气骑士联动活动202607";

	private const int ResetProgressVersion = 1;

	[SaveField(null, null)]
	public YuanQiActivityModel Model = new YuanQiActivityModel();

	[SaveField(null, null)]
	public YuanQiActivityDef activityDef;

	[NonSerialized]
	private ApplicationStateInfo ownerApplicationState;

	[NonSerialized]
	private YuanQiActivityEventHandler eventHandler;

	[NonSerialized]
	private YuanQiActivityServerDelegate serverDelegate;

	[NonSerialized]
	private HashSet<string> reportingMissionIds;

	[NonSerialized]
	private bool fetchingState;

	[NonSerialized]
	private bool claimingReward;

	[NonSerialized]
	private bool resolveActivityDefErrorLogged;

	[NonSerialized]
	private Dictionary<string, YuanQiActivityMissionStatus> serverMissionStatus;

	public bool IsActivityOpen
	{
		get
		{
			if (TryGetActivityDef(out var def))
			{
				ApplicationStateInfo applicationStateInfo = ownerApplicationState;
				if (applicationStateInfo == null)
				{
					return false;
				}
				return applicationStateInfo.limitedTimeActivityManager?.IsOpen(def.activityId) == true;
			}
			return false;
		}
	}

	public bool IsEntranceVisible
	{
		get
		{
			if (!IsEntranceUnlocked() || !TryGetRuntimeConfig(out var config))
			{
				return false;
			}
			return GetCurrentUtcTime() < config.EndTimeUtc;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		ownerApplicationState = applicationState;
		serverDelegate = new YuanQiActivityServerDelegate(applicationState);
		reportingMissionIds = new HashSet<string>(StringComparer.Ordinal);
		serverMissionStatus = new Dictionary<string, YuanQiActivityMissionStatus>(StringComparer.Ordinal);
		TryGetActivityDef(out var _);
		if (Model == null)
		{
			Model = new YuanQiActivityModel();
		}
		Model.Initialize(GetMissionConfigs());
		TryMigrateResetProgress(applicationState);
		eventHandler?.Dispose();
		eventHandler = new YuanQiActivityEventHandler(applicationState, Model, CanCollectMissionEvent, OnMissionChanged, OnMissionReached);
		Log($"Init, missions={Model.MissionItems.Count}, reached={Model.HasRewardToClaim()}");
	}

	public override void Dispose()
	{
		eventHandler?.Dispose();
		eventHandler = null;
		serverDelegate = null;
		reportingMissionIds = null;
		serverMissionStatus = null;
		ownerApplicationState = null;
	}

	private bool TryGetRuntimeConfig(out LimitedTimeActivityRuntimeConfig config)
	{
		config = null;
		if (TryGetActivityDef(out var def))
		{
			ApplicationStateInfo applicationStateInfo = ownerApplicationState;
			if (applicationStateInfo == null)
			{
				return false;
			}
			return applicationStateInfo.limitedTimeActivityManager?.TryGetRuntimeConfig(def.activityId, out config) == true;
		}
		return false;
	}

	public TimeSpan GetRemainTime()
	{
		if (TryGetActivityDef(out var def) && ownerApplicationState?.limitedTimeActivityManager != null && ownerApplicationState.limitedTimeActivityManager.TryGetRuntimeConfig(def.activityId, out var config))
		{
			TimeSpan timeSpan = config.EndTimeUtc - GetCurrentUtcTime();
			if (!(timeSpan > TimeSpan.Zero))
			{
				return TimeSpan.Zero;
			}
			return timeSpan;
		}
		return Model.GetRemainTime();
	}

	public string GetRemainTimeText()
	{
		TimeSpan remainTime = GetRemainTime();
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_activity_countdown_time.FormatWith(remainTime.Days, remainTime.Hours, remainTime.Minutes);
	}

	public bool IsEntranceUnlocked()
	{
		ApplicationStateInfo applicationStateInfo = ownerApplicationState;
		if (applicationStateInfo == null)
		{
			return false;
		}
		return applicationStateInfo.globalVariableManager?.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel) == true;
	}

	public bool TryGetDaysUntilOpen(out int days)
	{
		days = 0;
		if (!TryGetRuntimeConfig(out var config))
		{
			return false;
		}
		DateTime currentUtcTime = GetCurrentUtcTime();
		if (currentUtcTime >= config.StartTimeUtc)
		{
			return false;
		}
		days = ToRoundUpDays(config.StartTimeUtc - currentUtcTime);
		return true;
	}

	public bool ShouldShowEntranceRedDot()
	{
		if (IsActivityOpen)
		{
			return Model.HasRewardToClaim();
		}
		return false;
	}

	public bool TryGetEntranceDisplayText(out string text)
	{
		text = string.Empty;
		if (!IsEntranceUnlocked() || !TryGetRuntimeConfig(out var config))
		{
			return false;
		}
		DateTime currentUtcTime = GetCurrentUtcTime();
		if (currentUtcTime >= config.EndTimeUtc)
		{
			return false;
		}
		if (currentUtcTime < config.StartTimeUtc)
		{
			int num = ToRoundUpDays(config.StartTimeUtc - currentUtcTime);
			text = ScriptableObjectSingleton<TextDataPreset>.instance.term_activity_yuanqi_entrance_open_after_arg0_days.FormatWith(num);
			return true;
		}
		int num2 = ToRoundUpDays(config.EndTimeUtc - currentUtcTime);
		text = ScriptableObjectSingleton<TextDataPreset>.instance.term_activity_yuanqi_entrance_remain_arg0_days.FormatWith(num2);
		return true;
	}

	private static int ToRoundUpDays(TimeSpan span)
	{
		if (!(span <= TimeSpan.Zero))
		{
			return (int)Math.Ceiling(span.TotalDays);
		}
		return 0;
	}

	private void ApplyServerActivityState(bool enabled, IEnumerable<YuanQiActivityMissionServerState> serverStates, long serverEndTimeTicks = 0L)
	{
		Model.SetActivityState(enabled, serverEndTimeTicks);
		ApplyServerMissionState(serverStates);
	}

	public bool TryRefreshState(bool isForeground)
	{
		GameAsyncOperation gameAsyncOperation = CreateFetchStateOperation();
		if (gameAsyncOperation == null)
		{
			return false;
		}
		GameAsyncOperationLane lane = ((!isForeground) ? GameAsyncOperationLane.Background : GameAsyncOperationLane.Foreground);
		gameAsyncOperation.EnqueueRun(lane, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_yuanqi_activity);
		return true;
	}

	private GameAsyncOperation CreateFetchStateOperation()
	{
		if (!TryGetActivityDef(out var def) || serverDelegate == null || fetchingState)
		{
			return null;
		}
		fetchingState = true;
		GetYuanQiActivityStateRequest request = new GetYuanQiActivityStateRequest
		{
			activityId = def.activityId
		};
		GeneralGameAsyncOperation<GetYuanQiActivityStateRequest, GetYuanQiActivityStateResponse> op = new GeneralGameAsyncOperation<GetYuanQiActivityStateRequest, GetYuanQiActivityStateResponse>(request, serverDelegate.GetActivityStateAsync);
		op.HideFaultPanel().OnComplete(delegate
		{
			ApplyFetchedActivityState(op.OpResult);
		}).OnFault(delegate
		{
			TraceLog.Error("[YuanQiActivityManager] Fetch state failed.");
		})
			.OnOver(delegate
			{
				fetchingState = false;
				TryReconcileCompensation();
			});
		return op;
	}

	private void ApplyServerMissionState(IEnumerable<YuanQiActivityMissionServerState> serverStates)
	{
		if (Model.ApplyServerStates(serverStates) | SyncTalentMissionProgressFromLocal())
		{
			MarkPersistedStateChanged();
		}
		RefreshFortressEntry();
	}

	private bool SyncTalentMissionProgressFromLocal()
	{
		CharacterManager characterManager = ownerApplicationState?.characterManager;
		if (characterManager == null)
		{
			return false;
		}
		bool result = false;
		foreach (YuanQiActivityMissionItem missionItem in Model.MissionItems)
		{
			if (missionItem?.Data == null || missionItem.Data.type != YuanQiActivityMissionType.TalentCount)
			{
				continue;
			}
			int unlockedTalentCount = GetUnlockedTalentCount(characterManager, missionItem.Data.characterId);
			if (missionItem.SyncProgressFromLocal(unlockedTalentCount))
			{
				result = true;
				if (missionItem.Status == YuanQiActivityMissionStatus.Reached)
				{
					OnMissionReached(missionItem, null);
				}
			}
		}
		return result;
	}

	private static int GetUnlockedTalentCount(CharacterManager characterManager, string characterId)
	{
		int num = 0;
		foreach (PlayerCharacterState characterState in characterManager.characterStates)
		{
			if ((string.IsNullOrEmpty(characterId) || !(characterState?.def == null)) && (string.IsNullOrEmpty(characterId) || !(characterState.def.FullID != characterId)))
			{
				num += (characterState?.unlockedTalents?.Count).GetValueOrDefault();
			}
		}
		return num;
	}

	public bool ClaimMissionReward(string missionId)
	{
		if (claimingReward || !Model.TryGetMission(missionId, out var mission) || !mission.CanClaim)
		{
			return false;
		}
		GameAsyncOperation gameAsyncOperation = CreateClaimMissionRewardOperation(missionId);
		if (gameAsyncOperation == null)
		{
			return false;
		}
		gameAsyncOperation.ManualRetry().EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_yuanqi_activity_reward);
		return true;
	}

	private GameAsyncOperation CreateClaimMissionRewardOperation(string missionId, Action onOver = null)
	{
		if (!TryGetActivityDef(out var def) || serverDelegate == null)
		{
			return null;
		}
		claimingReward = true;
		ClaimYuanQiActivityMissionRewardRequest request = new ClaimYuanQiActivityMissionRewardRequest
		{
			activityId = def.activityId,
			missionId = missionId
		};
		GeneralGameAsyncOperation<ClaimYuanQiActivityMissionRewardRequest, ClaimYuanQiActivityMissionRewardResponse> op = new GeneralGameAsyncOperation<ClaimYuanQiActivityMissionRewardRequest, ClaimYuanQiActivityMissionRewardResponse>(request, serverDelegate.ClaimMissionRewardAsync);
		op.OnComplete(delegate
		{
			ApplyClaimResponse(missionId, op.OpResult);
		}).OnFault(delegate
		{
			TraceLog.Error("[YuanQiActivityManager] Claim failed, missionId=" + missionId + ".");
		}).OnOver(delegate
		{
			claimingReward = false;
			onOver?.Invoke();
		});
		return op;
	}

	public void JumpToMission(string missionId)
	{
		if (!Model.TryGetMission(missionId, out var mission) || !mission.CanJump)
		{
			return;
		}
		FortressUIManager fortressUIManager = ownerApplicationState?.fortressState?.uiManager;
		if (fortressUIManager == null)
		{
			return;
		}
		switch (mission.Data.jumpType)
		{
		case YuanQiActivityMissionJumpType.GiftCode:
			fortressUIManager.fortressGiftCodePanel.ShowUI();
			break;
		case YuanQiActivityMissionJumpType.StartGame:
			fortressUIManager.ShowUI(fortressUIManager.baseGameUIPanel);
			break;
		case YuanQiActivityMissionJumpType.AscenderTalent:
			fortressUIManager.characterInfoBasePanel.SetupFromMainRoom(ownerApplicationState.newGameConfigManager.currentNewGameConfig.currentCharacter.state);
			break;
		case YuanQiActivityMissionJumpType.ExternalUrl:
			if (!string.IsNullOrEmpty(mission.Data.jumpParam))
			{
				Application.OpenURL(mission.Data.jumpParam);
			}
			break;
		}
	}

	public bool DebugSetCurrentMissionReachedWithoutReport()
	{
		YuanQiActivityMissionItem yuanQiActivityMissionItem = Model.DebugCompleteCurrentMissionLocally();
		if (yuanQiActivityMissionItem == null)
		{
			return false;
		}
		MarkPersistedStateChanged();
		Log("[Debug] Force current mission reached without report: " + yuanQiActivityMissionItem.Data.missionId);
		return true;
	}

	public void RefreshFortressEntry()
	{
		FortressUIManager obj = ownerApplicationState?.fortressState?.uiManager;
		obj?.mainUIPanel?.SetDirty();
		obj?.yuanQiActivityPanel?.SetDirty();
	}

	private void OnMissionChanged()
	{
		MarkPersistedStateChanged();
		Log("Mission state changed by event");
	}

	private void OnMissionReached(YuanQiActivityMissionItem mission, string gameGuid)
	{
		if (mission?.Data == null || reportingMissionIds == null || !reportingMissionIds.Add(mission.Data.missionId))
		{
			return;
		}
		GeneralGameAsyncOperation<ReportYuanQiActivityMissionReachedRequest, ReportYuanQiActivityMissionReachedResponse> op = CreateReportMissionReachedOperation(mission, gameGuid);
		if (op == null)
		{
			reportingMissionIds.Remove(mission.Data.missionId);
			return;
		}
		op.HideFaultPanel().OnComplete(delegate
		{
			ApplyMissionReachedResponse(mission, op.OpResult);
		}).OnFault(delegate
		{
			TraceLog.Error("[YuanQiActivityManager] Report mission reached failed, missionId=" + mission.Data.missionId + ".");
		})
			.OnOver(delegate
			{
				reportingMissionIds.Remove(mission.Data.missionId);
			})
			.EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_yuanqi_activity_progress);
	}

	private GeneralGameAsyncOperation<ReportYuanQiActivityMissionReachedRequest, ReportYuanQiActivityMissionReachedResponse> CreateReportMissionReachedOperation(YuanQiActivityMissionItem mission, string gameGuid, int progressOverride = -1)
	{
		if (!TryGetActivityDef(out var def) || serverDelegate == null || mission?.Data == null)
		{
			return null;
		}
		return new GeneralGameAsyncOperation<ReportYuanQiActivityMissionReachedRequest, ReportYuanQiActivityMissionReachedResponse>(new ReportYuanQiActivityMissionReachedRequest
		{
			activityId = def.activityId,
			missionId = mission.Data.missionId,
			evidenceType = mission.Data.type.ToString(),
			progress = ((progressOverride >= 0) ? progressOverride : mission.Progress),
			clientEventTimeUtc = GetCurrentUtcTime(),
			gameGuid = gameGuid,
			characterId = mission.Data.characterId
		}, serverDelegate.ReportMissionReachedAsync);
	}

	private void ApplyFetchedActivityState(GetYuanQiActivityStateResponse response)
	{
		if (response != null)
		{
			ApplyActivityState(response);
		}
	}

	private void ApplyActivityState(GetYuanQiActivityStateResponse response)
	{
		if (response != null)
		{
			CacheServerMissionStatus(response.missions);
			long serverEndTimeTicks = response.activityEndTimeUtc?.Ticks ?? 0;
			ApplyServerActivityState(response.isOpen, response.missions, serverEndTimeTicks);
		}
	}

	private void CacheServerMissionStatus(IEnumerable<YuanQiActivityMissionServerState> serverStates)
	{
		if (serverMissionStatus == null || serverStates == null)
		{
			return;
		}
		foreach (YuanQiActivityMissionServerState serverState in serverStates)
		{
			if (serverState != null && !string.IsNullOrEmpty(serverState.missionId))
			{
				serverMissionStatus[serverState.missionId] = serverState.status;
			}
		}
	}

	private YuanQiActivityMissionStatus GetServerMissionStatus(string missionId)
	{
		if (serverMissionStatus != null && !string.IsNullOrEmpty(missionId) && serverMissionStatus.TryGetValue(missionId, out var value))
		{
			return value;
		}
		return YuanQiActivityMissionStatus.InProgress;
	}

	private void TryReconcileCompensation()
	{
		if (!IsActivityOpen || serverDelegate == null || fetchingState || claimingReward || (reportingMissionIds?.Count ?? 0) > 0)
		{
			return;
		}
		foreach (YuanQiActivityMissionItem missionItem in Model.MissionItems)
		{
			if (missionItem?.Data == null)
			{
				continue;
			}
			string missionId = missionItem.Data.missionId;
			switch (GetServerMissionStatus(missionId))
			{
			case YuanQiActivityMissionStatus.Reached:
				return;
			case YuanQiActivityMissionStatus.Claimed:
				continue;
			}
			if (missionItem.Progress >= missionItem.Data.targetProgress)
			{
				CompensateReportReached(missionItem);
			}
			return;
		}
	}

	private void CompensateReportReached(YuanQiActivityMissionItem mission)
	{
		string missionId = mission.Data.missionId;
		GeneralGameAsyncOperation<ReportYuanQiActivityMissionReachedRequest, ReportYuanQiActivityMissionReachedResponse> reportOp = CreateReportMissionReachedOperation(mission, string.Empty, mission.Data.targetProgress);
		if (reportOp == null)
		{
			TraceLog.Error("[YuanQiActivityManager] Compensation report op is null, missionId=" + missionId + ".");
			return;
		}
		reportingMissionIds?.Add(missionId);
		reportOp.HideFaultPanel().OnComplete(delegate
		{
			ApplyMissionReachedResponse(mission, reportOp.OpResult);
		}).OnFault(delegate
		{
			TraceLog.Error("[YuanQiActivityManager] Compensation report failed, missionId=" + missionId + ".");
		})
			.OnOver(delegate
			{
				reportingMissionIds?.Remove(missionId);
			})
			.EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_yuanqi_activity_progress);
	}

	private void ApplyMissionReachedResponse(YuanQiActivityMissionItem mission, ReportYuanQiActivityMissionReachedResponse response)
	{
		if (mission?.Data == null || response?.state == null)
		{
			return;
		}
		ApplyActivityState(response.state);
		List<YuanQiActivityMissionServerState> missions = response.state.missions;
		if (missions != null && missions.Any((YuanQiActivityMissionServerState state) => state != null && state.missionId == mission.Data.missionId && state.status >= YuanQiActivityMissionStatus.Reached))
		{
			List<GlobalRewardInfo> rewardInfos = (from reward in mission.Data.rewards ?? new List<MatterReward>()
				where reward != null
				select reward.GenerateReward(ownerApplicationState, GlobalRewardSourceType.Other, "元气骑士联动活动202607")).ToList();
			ThinkingAnalyticsExtends.TrackActivityTaskAction(ownerApplicationState, "元气骑士联动活动202607", mission.Data.missionId, GetAnalyticsTaskType(mission.Data.type), "achieve", rewardInfos);
		}
	}

	private void ApplyClaimResponse(string missionId, ClaimYuanQiActivityMissionRewardResponse response)
	{
		if (response == null)
		{
			return;
		}
		Model.TryGetMission(missionId, out var mission);
		ApplyActivityState(response.state);
		if (mission?.Data != null && !response.isDuplicateClaim && response.rewards != null && response.rewards.Count != 0)
		{
			List<GlobalRewardInfo> list = response.rewards.Where((GlobalRewardInfo reward) => reward != null).ToList();
			if (list.Count != 0)
			{
				GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
				globalRewardPackage.GiveAll(ownerApplicationState);
				ownerApplicationState?.fortressState?.uiManager?.SetDirty();
				ThinkingAnalyticsExtends.TrackActivityTaskAction(ownerApplicationState, "元气骑士联动活动202607", mission.Data.missionId, GetAnalyticsTaskType(mission.Data.type), "claim", list);
				ownerApplicationState?.fortressState?.uiManager?.globalRewardPackageDisplay?.Setup(globalRewardPackage);
			}
		}
	}

	private static string GetAnalyticsTaskType(YuanQiActivityMissionType missionType)
	{
		return missionType switch
		{
			YuanQiActivityMissionType.GiftCode => "code", 
			YuanQiActivityMissionType.Share => "share", 
			YuanQiActivityMissionType.TalentCount => "talent", 
			YuanQiActivityMissionType.BossKill => "boss", 
			_ => null, 
		};
	}

	private void TryMigrateResetProgress(ApplicationStateInfo applicationState)
	{
		if (Model.ResetVersion < 1)
		{
			Model.ResetAllProgress();
			Model.MarkReset(1);
			applicationState?.savingManager?.SetProfileDirtyToSave();
			Log($"Reset progress migrated to version {1}");
		}
	}

	private bool CanCollectMissionEvent()
	{
		if (IsActivityOpen)
		{
			return Model.GetCurrentMission() != null;
		}
		return false;
	}

	private void MarkPersistedStateChanged()
	{
		ownerApplicationState?.savingManager?.SetProfileDirtyToSave();
		RefreshFortressEntry();
	}

	private IReadOnlyList<YuanQiActivityMissionData> GetMissionConfigs()
	{
		if (TryGetActivityDef(out var def) && def.activityMissionConfigs != null)
		{
			return def.activityMissionConfigs;
		}
		return null;
	}

	private YuanQiActivityDef ResolveActivityDef()
	{
		return ownerApplicationState?.assetCatalog?.activityCatalog?.ValidAssets.OfType<YuanQiActivityDef>().FirstOrDefault();
	}

	private bool TryGetActivityDef(out YuanQiActivityDef def)
	{
		if (activityDef != null)
		{
			def = activityDef;
			return true;
		}
		def = ResolveActivityDef();
		if (def != null)
		{
			activityDef = def;
			return true;
		}
		if (!resolveActivityDefErrorLogged)
		{
			resolveActivityDefErrorLogged = true;
			TraceLog.Error("[YuanQiActivityManager] YuanQiActivityDef is not ready.");
		}
		return false;
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

	private static void Log(string message)
	{
		TraceLog.Debug("[YuanQiActivityManager] " + message);
	}
}
