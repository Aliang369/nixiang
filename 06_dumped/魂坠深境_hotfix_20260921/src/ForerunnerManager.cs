using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class ForerunnerManager : GlobalGameManager
{
	private static readonly TimeSpan ProfileDataRefreshInterval = TimeSpan.FromSeconds(30.0);

	[SaveField(null, null)]
	public ForerunnerLegacy localForerunnerLegacy;

	[SaveField(null, null)]
	public ForerunnerLegacy shareForerunnerLegacy;

	[SaveField(null, null)]
	public List<ForerunnerLegacy> gotForerunnerLegacyHistory;

	[SaveField(null, null)]
	public List<ForerunnerLegacy> leaveForerunnerLegacyHistory;

	[SaveField(null, null)]
	public HashSet<string> upvoteLegacyHistory;

	[SaveField(null, null)]
	public HashSet<string> downvoteLegacyHistory;

	public bool forerunnerProfileDataLoaded;

	public bool forerunnerProfileDataLoading;

	private DateTime lastForerunnerProfileDataRequestTimeUtc;

	private Action<bool> pendingProfileDataRequestCallbacks;

	private Action<bool> queuedProfileDataRefreshCallbacks;

	private bool refreshProfileDataAfterCurrentRequest;

	private int profileDataRequestVersion;

	private int minimumPendingRewardProfileVersion;

	public int pendingShareRewardUpvoteCount { get; private set; }

	public bool HasPendingShareReward => pendingShareRewardUpvoteCount > 0;

	public int PendingShareRewardMatterCount => pendingShareRewardUpvoteCount * ScriptableObjectSingleton<FortressDataManager>.instance.legacyShareRewardPerUpvote;

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (gotForerunnerLegacyHistory == null)
		{
			gotForerunnerLegacyHistory = new List<ForerunnerLegacy>();
		}
		if (leaveForerunnerLegacyHistory == null)
		{
			leaveForerunnerLegacyHistory = new List<ForerunnerLegacy>();
		}
		if (upvoteLegacyHistory == null)
		{
			upvoteLegacyHistory = new HashSet<string>();
		}
		if (downvoteLegacyHistory == null)
		{
			downvoteLegacyHistory = new HashSet<string>();
		}
		forerunnerProfileDataLoaded = false;
		forerunnerProfileDataLoading = false;
		pendingShareRewardUpvoteCount = 0;
		lastForerunnerProfileDataRequestTimeUtc = DateTime.MinValue;
		pendingProfileDataRequestCallbacks = null;
		queuedProfileDataRefreshCallbacks = null;
		refreshProfileDataAfterCurrentRequest = false;
		profileDataRequestVersion = 0;
		minimumPendingRewardProfileVersion = 0;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		gotForerunnerLegacyHistory = ScriptableObjectSingleton<MockNetworkDataManager>.instance.testGotHistoryDataSetups.Select((MockNetworkDataManager.MockShareForerunnerDataSetup d) => d.ToData()).ToList();
		gotForerunnerLegacyHistory.AddRange(ScriptableObjectSingleton<MockNetworkDataManager>.instance.GeneratePresetMessageTestData());
		leaveForerunnerLegacyHistory = ScriptableObjectSingleton<MockNetworkDataManager>.instance.testLeaveHistoryDataSetups.Select((MockNetworkDataManager.MockShareForerunnerDataSetup d) => d.ToData()).ToList();
		for (int num = 0; num < gotForerunnerLegacyHistory.Count; num++)
		{
			if (num % 2 == 0)
			{
				upvoteLegacyHistory.Add(gotForerunnerLegacyHistory[num].guid);
			}
			else
			{
				downvoteLegacyHistory.Add(gotForerunnerLegacyHistory[num].guid);
			}
		}
		forerunnerProfileDataLoaded = true;
	}

	public ForerunnerLegacy GenerateDefaultForerunnerLegacy()
	{
		ForerunnerLegacy forerunnerLegacy = GenerateStoryForerunnerLegacy(ScriptableObjectSingleton<GuideData>.instance.initForerunnerNote);
		forerunnerLegacy.SetSkill(new GameSkill(new ProfileSkill(ScriptableObjectSingleton<GuideData>.instance.initForerunnerSkill, 0)));
		return forerunnerLegacy;
	}

	public ForerunnerLegacy GenerateLevelFallbackForerunnerLegacy(SkillDef fallbackSkill, int stage, DraftMapRef sourceDraftMap, string fallbackNote)
	{
		int num = fallbackSkill?.skillArchetypes?.FindIndex((SkillDef.SkillArchetype archetype) => archetype.qualityLevel == QualityLevel.Rare) ?? (-1);
		if (num < 0)
		{
			Debug.LogError($"Level {stage} forerunner fallback skill is missing a Rare archetype.");
			return GenerateDefaultForerunnerLegacy();
		}
		ForerunnerLegacy forerunnerLegacy = new ForerunnerLegacy();
		forerunnerLegacy.stage = stage;
		forerunnerLegacy.sourceDraftMap = sourceDraftMap;
		forerunnerLegacy.expireTime = DateTime.Now + TimeSpan.FromDays(9999.0);
		forerunnerLegacy.note = fallbackNote ?? string.Empty;
		forerunnerLegacy.signature = base.applicationState.playerInfoManager.DataName;
		forerunnerLegacy.storyOnly = true;
		forerunnerLegacy.SetSkill(new GameSkill(new ProfileSkill(fallbackSkill, num)));
		return forerunnerLegacy;
	}

	public ForerunnerLegacy GenerateStoryForerunnerLegacy(string note, Item item = null)
	{
		ForerunnerLegacy forerunnerLegacy = new ForerunnerLegacy();
		forerunnerLegacy.stage = ScriptableObjectSingleton<GuideData>.instance.initForerunnerStage;
		forerunnerLegacy.expireTime = DateTime.Now + TimeSpan.FromDays(9999.0);
		forerunnerLegacy.note = note;
		forerunnerLegacy.signature = base.applicationState.playerInfoManager.DataName;
		forerunnerLegacy.storyOnly = true;
		forerunnerLegacy.SetItem(item);
		return forerunnerLegacy;
	}

	public void CleanLocalLegacy()
	{
		ThinkingAnalyticsExtends.TrackStopKeepForerunnerLegacy(base.applicationState, localForerunnerLegacy);
		if (GlobalConfigManager.instance.enableNetwork)
		{
			StopKeepForerunnerLegacyRequest stopKeepForerunnerLegacyRequest = new StopKeepForerunnerLegacyRequest();
			stopKeepForerunnerLegacyRequest.Shared = false;
			new GeneralGameAsyncOperationRequestOnly<StopKeepForerunnerLegacyRequest>(stopKeepForerunnerLegacyRequest, base.applicationState.ChillyRoomSDK.Forerunner.StopKeepForerunnerLegacyAsync).OnComplete(delegate
			{
				localForerunnerLegacy = null;
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_updating_forerunner_legacy);
		}
		localForerunnerLegacy = null;
	}

	public void CleanShareLegacy()
	{
		ThinkingAnalyticsExtends.TrackStopKeepForerunnerLegacy(base.applicationState, shareForerunnerLegacy);
		if (GlobalConfigManager.instance.enableNetwork)
		{
			StopKeepForerunnerLegacyRequest stopKeepForerunnerLegacyRequest = new StopKeepForerunnerLegacyRequest();
			stopKeepForerunnerLegacyRequest.Shared = true;
			new GeneralGameAsyncOperationRequestOnly<StopKeepForerunnerLegacyRequest>(stopKeepForerunnerLegacyRequest, base.applicationState.ChillyRoomSDK.Forerunner.StopKeepForerunnerLegacyAsync).OnComplete(delegate
			{
				shareForerunnerLegacy = null;
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_updating_forerunner_legacy);
		}
		shareForerunnerLegacy = null;
	}

	public bool IsLegacyUpvoteByPlayer(ForerunnerLegacy legacy)
	{
		return upvoteLegacyHistory.Contains(legacy.guid);
	}

	public bool IsLegacyDownvoteByPlayer(ForerunnerLegacy legacy)
	{
		return downvoteLegacyHistory.Contains(legacy.guid);
	}

	public void RequestForerunnerProfileDataIfNeeded(Action<bool> completed = null)
	{
		RequestForerunnerProfileData(forceRefresh: false, completed);
	}

	public void RefreshForerunnerProfileData(Action<bool> completed = null)
	{
		RequestForerunnerProfileData(forceRefresh: true, completed);
	}

	public bool RefreshForerunnerProfileDataIfStale(Action<bool> completed = null)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return false;
		}
		if (forerunnerProfileDataLoading)
		{
			pendingProfileDataRequestCallbacks = (Action<bool>)Delegate.Combine(pendingProfileDataRequestCallbacks, completed);
			return true;
		}
		if (DateTime.UtcNow - lastForerunnerProfileDataRequestTimeUtc < ProfileDataRefreshInterval)
		{
			return false;
		}
		RequestForerunnerProfileData(forceRefresh: true, completed);
		return true;
	}

	private void RequestForerunnerProfileData(bool forceRefresh, Action<bool> completed)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			completed?.Invoke(obj: true);
			return;
		}
		if (!forceRefresh && forerunnerProfileDataLoaded)
		{
			completed?.Invoke(obj: true);
			return;
		}
		if (forerunnerProfileDataLoading)
		{
			if (forceRefresh)
			{
				refreshProfileDataAfterCurrentRequest = true;
				queuedProfileDataRefreshCallbacks = (Action<bool>)Delegate.Combine(queuedProfileDataRefreshCallbacks, completed);
			}
			else
			{
				pendingProfileDataRequestCallbacks = (Action<bool>)Delegate.Combine(pendingProfileDataRequestCallbacks, completed);
			}
			return;
		}
		pendingProfileDataRequestCallbacks = (Action<bool>)Delegate.Combine(pendingProfileDataRequestCallbacks, completed);
		int requestVersion = ++profileDataRequestVersion;
		GeneralGameAsyncOperationResultOnly<ForerunnerProfileData> op = new GeneralGameAsyncOperationResultOnly<ForerunnerProfileData>(base.applicationState.ChillyRoomSDK.Forerunner.FetchForerunnerProfileDataAsync);
		forerunnerProfileDataLoading = true;
		lastForerunnerProfileDataRequestTimeUtc = DateTime.UtcNow;
		bool requestSucceeded = false;
		op.OnComplete(delegate
		{
			FetchForerunnerProfileData(op.OpResult, requestVersion);
			forerunnerProfileDataLoaded = true;
			requestSucceeded = true;
		}).OnOver(delegate
		{
			forerunnerProfileDataLoading = false;
			Action<bool> action = pendingProfileDataRequestCallbacks;
			pendingProfileDataRequestCallbacks = null;
			bool flag = refreshProfileDataAfterCurrentRequest;
			Action<bool> completed2 = queuedProfileDataRefreshCallbacks;
			refreshProfileDataAfterCurrentRequest = false;
			queuedProfileDataRefreshCallbacks = null;
			try
			{
				action?.Invoke(requestSucceeded);
			}
			finally
			{
				if (flag)
				{
					RequestForerunnerProfileData(forceRefresh: true, completed2);
				}
			}
		});
		op.HideFaultPanel();
		op.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_forerunner_data);
		void FetchForerunnerProfileData(ForerunnerProfileData profileData, int completedRequestVersion)
		{
			NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
			leaveForerunnerLegacyHistory = profileData.ShareForerunnerLegacyHistory?.Select((ChillyRoom.Services.ContainerOfSoul.ForerunnerLegacy s) => s.ToClient(context)).WhereNotNull().ToList() ?? new List<ForerunnerLegacy>();
			gotForerunnerLegacyHistory = profileData.GotForerunnerLegacyHistory?.Select((ChillyRoom.Services.ContainerOfSoul.ForerunnerLegacy s) => s.ToClient(context)).WhereNotNull().ToList() ?? new List<ForerunnerLegacy>();
			if (completedRequestVersion >= minimumPendingRewardProfileVersion)
			{
				pendingShareRewardUpvoteCount = Math.Max(0, profileData.PendingShareRewardUpvoteCount);
			}
		}
	}

	public void ClearPendingShareReward()
	{
		pendingShareRewardUpvoteCount = 0;
		minimumPendingRewardProfileVersion = profileDataRequestVersion + 1;
	}
}
