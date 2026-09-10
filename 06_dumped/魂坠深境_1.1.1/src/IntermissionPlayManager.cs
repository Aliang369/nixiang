using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;

public class IntermissionPlayManager : GlobalGameManager, IFrameUpdate
{
	private static readonly TimeSpan AutomaticRefreshRetryInterval = TimeSpan.FromSeconds(30.0);

	public const string ActivityId = "#ACT#000003#";

	[SaveField(null, null)]
	private IntermissionPlayInfoCache cachedInfo;

	[NonSerialized]
	public IntermissionPlayModel Model;

	[NonSerialized]
	private IntermissionPlayDrawPresentationResult pendingDrawResult;

	[NonSerialized]
	private IntermissionPlayReclaimPresentationResult pendingReclaimResult;

	[NonSerialized]
	private GlobalRewardPackage pendingDailyTaskRewardPackage;

	[NonSerialized]
	private int pendingPaidTicketCount;

	[NonSerialized]
	private bool fetchingInfo;

	[NonSerialized]
	private bool drawing;

	[NonSerialized]
	private bool reclaiming;

	[NonSerialized]
	private bool claimingDailyTask;

	[NonSerialized]
	private IntermissionPlayDrawRequest pendingDrawRequest;

	[NonSerialized]
	private IntermissionPlayReclaimTicketsRequest pendingReclaimRequest;

	[NonSerialized]
	private bool hasFreshInfo;

	[NonSerialized]
	private DateTimeOffset automaticRefreshRetryAfterUtc;

	[NonSerialized]
	private bool runtimeWasOpen;

	[NonSerialized]
	private bool runtimeUnavailable;

	[NonSerialized]
	private DateTimeOffset synchronizedServerTimeUtc;

	[NonSerialized]
	private DateTime synchronizedLocalTimeUtc;

	public bool IsFetchingInfo => fetchingInfo;

	public bool IsDrawing => drawing;

	public bool IsReclaiming => reclaiming;

	public bool IsClaimingDailyTask => claimingDailyTask;

	public event Action StateChanged;

	public override void Init(ApplicationStateInfo applicationState)
	{
		Model = new IntermissionPlayModel();
		hasFreshInfo = false;
		automaticRefreshRetryAfterUtc = default(DateTimeOffset);
		runtimeWasOpen = false;
		runtimeUnavailable = false;
		synchronizedServerTimeUtc = default(DateTimeOffset);
		synchronizedLocalTimeUtc = default(DateTime);
		if (cachedInfo != null)
		{
			Model.ApplyInfo(cachedInfo.ToResponse("#ACT#000003#"));
		}
		applicationState.eventManager.enterFortressPanelEvent.AddListener(OnEnterFortressPanel);
		applicationState.eventManager.afterGameOverEvent.AddListener(OnAfterGameOver);
		applicationState.eventManager.purchasePayEvent.AddListener(OnPurchasePaid);
	}

	public override void Dispose()
	{
		base.applicationState.eventManager?.enterFortressPanelEvent.RemoveListener(OnEnterFortressPanel);
		base.applicationState.eventManager?.afterGameOverEvent.RemoveListener(OnAfterGameOver);
		base.applicationState.eventManager?.purchasePayEvent.RemoveListener(OnPurchasePaid);
	}

	public bool IsReady()
	{
		if (base.applicationState.limitedTimeActivityManager.IsReady && hasFreshInfo)
		{
			return Model.Info != null;
		}
		return false;
	}

	public bool IsOpen()
	{
		if (IsReady() && !runtimeUnavailable && Model.IsOpen)
		{
			return base.applicationState.limitedTimeActivityManager.IsOpen("#ACT#000003#");
		}
		return false;
	}

	public bool IsEntranceUnlocked()
	{
		return base.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel);
	}

	public bool IsEntranceVisible()
	{
		if (IsEntranceUnlocked() && !runtimeUnavailable)
		{
			return base.applicationState.limitedTimeActivityManager.IsOpen("#ACT#000003#");
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (IsOpen() && IsEntranceUnlocked())
		{
			if (Model.DailyTaskCompletedCount <= Model.DailyTicketGrantedCount)
			{
				if (Model.RewardSlots.Any((IntermissionPlayRewardSlotDto rewardSlot) => !rewardSlot.IsDrawn))
				{
					if (Model.DailyTicketCount <= 0)
					{
						return Model.CarryTicketCount > 0;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public TimeSpan GetActivityRemaining()
	{
		if (!IsReady())
		{
			return TimeSpan.Zero;
		}
		TimeSpan timeSpan = Model.ActivityEndTimeUtc - GetCurrentUtcTime();
		if (!(timeSpan > TimeSpan.Zero))
		{
			return TimeSpan.Zero;
		}
		return timeSpan;
	}

	public TimeSpan GetEntranceActivityRemaining()
	{
		if (!base.applicationState.limitedTimeActivityManager.TryGetRuntimeConfig("#ACT#000003#", out var config))
		{
			return TimeSpan.Zero;
		}
		TimeSpan timeSpan = config.EndTimeUtc - GetCurrentUtcTime().UtcDateTime;
		if (!(timeSpan > TimeSpan.Zero))
		{
			return TimeSpan.Zero;
		}
		return timeSpan;
	}

	public string GetEntranceActivityCountdownText()
	{
		TimeSpan entranceActivityRemaining = GetEntranceActivityRemaining();
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_intermission_play_entrance_duration_arg0_days.FormatWith(ToRoundUpDays(entranceActivityRemaining));
	}

	public TimeSpan GetNextRefreshRemaining()
	{
		if (!IsReady())
		{
			return TimeSpan.Zero;
		}
		TimeSpan timeSpan = Model.NextRefreshTimeUtc - GetCurrentUtcTime();
		if (!(timeSpan > TimeSpan.Zero))
		{
			return TimeSpan.Zero;
		}
		return timeSpan;
	}

	public string GetLocalizedDescription()
	{
		return GetLocalizedRuntimeText(Model.Description);
	}

	public string GetLocalizedHelpText()
	{
		return GetLocalizedRuntimeText(Model.HelpText);
	}

	public string GetActivityCountdownText()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_countdown_arg0_arg1.FormatWith(ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_activity_end, FormatCountdown(GetActivityRemaining()));
	}

	public string GetDailyRefreshCountdownText()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_countdown_arg0_arg1.FormatWith(ScriptableObjectSingleton<TextDataPreset>.instance.term_newbie_festival_day_refresh, FormatCountdown(GetNextRefreshRemaining()));
	}

	public bool CanClaimDailyTask()
	{
		if (IsOpen() && Model.DailyTaskCompletedCount > Model.DailyTicketGrantedCount && !fetchingInfo && !drawing && !reclaiming)
		{
			return !claimingDailyTask;
		}
		return false;
	}

	public bool TryFetchInfo(bool forceRefresh)
	{
		if (fetchingInfo || drawing || reclaiming || claimingDailyTask || pendingDrawRequest != null || pendingReclaimRequest != null || !GlobalConfigManager.instance.enableNetwork || !base.applicationState.limitedTimeActivityManager.IsOpen("#ACT#000003#") || base.applicationState.ChillyRoomSDK?.IntermissionPlay == null || (!forceRefresh && hasFreshInfo))
		{
			return false;
		}
		GetIntermissionPlayInfoRequest request = new GetIntermissionPlayInfoRequest
		{
			ActivityId = "#ACT#000003#"
		};
		fetchingInfo = true;
		GeneralGameAsyncOperation<GetIntermissionPlayInfoRequest, GetIntermissionPlayInfoResponse> operation = GameAsyncOperation.Get(request, base.applicationState.ChillyRoomSDK.IntermissionPlay.GetInfoAsync);
		operation.HideFaultPanel().OnComplete(delegate
		{
			int num = (hasFreshInfo ? Model.CarryTicketCount : (cachedInfo?.carryTicketCount ?? 0));
			TraceLog.TryDebug(() => "[IntermissionPlayManager] Fetch info succeeded: " + TraceLog.ToJsonString(operation.OpResult));
			Model.ApplyInfo(operation.OpResult);
			int num2 = Model.CarryTicketCount - num;
			if (num2 > 0)
			{
				pendingPaidTicketCount += num2;
			}
			cachedInfo = IntermissionPlayInfoCache.FromResponse(Model.Info);
			hasFreshInfo = true;
			runtimeUnavailable = false;
			automaticRefreshRetryAfterUtc = default(DateTimeOffset);
			base.applicationState.savingManager.SetProfileDirtyToSave();
			StateChanged?.Invoke();
		}).OnFault(delegate
		{
			if (IsActivityUnavailableFailure(operation.errorCode))
			{
				runtimeUnavailable = true;
			}
			TraceLog.Error("[IntermissionPlayManager] Fetch info failed. ErrorCode=" + operation.errorCode + ", TraceId=" + operation.traceId);
		})
			.OnOver(delegate
			{
				fetchingInfo = false;
				StateChanged?.Invoke();
			})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_network_communicating);
		StateChanged?.Invoke();
		return true;
	}

	public bool TryClaimDailyTask()
	{
		if (!CanClaimDailyTask() || pendingDrawRequest != null || pendingReclaimRequest != null || base.applicationState.ChillyRoomSDK?.IntermissionPlay == null)
		{
			return false;
		}
		claimingDailyTask = true;
		GeneralGameAsyncOperationResultOnly<IntermissionPlayClaimDailyTaskResponse> operation = GameAsyncOperation.Get(base.applicationState.ChillyRoomSDK.IntermissionPlay.ClaimDailyTaskAsync);
		operation.HideFaultPanel().OnComplete(delegate
		{
			Model.ApplyDailyTaskClaim(operation.OpResult);
			GlobalRewardInfo globalRewardInfo = operation.OpResult.Reward.ToClient(new NetworkConvertContext(base.applicationState));
			globalRewardInfo.Give(base.applicationState);
			pendingDailyTaskRewardPackage = new GlobalRewardPackage(globalRewardInfo);
			cachedInfo = IntermissionPlayInfoCache.FromResponse(Model.Info);
			base.applicationState.savingManager.SetProfileDirtyToSave();
			StateChanged?.Invoke();
		}).OnFault(delegate
		{
			if (IsActivityUnavailableFailure(operation.errorCode))
			{
				runtimeUnavailable = true;
			}
			TraceLog.Error("[IntermissionPlayManager] Claim daily task failed.");
		})
			.OnOver(delegate
			{
				claimingDailyTask = false;
				StateChanged?.Invoke();
			})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_festival_reward);
		StateChanged?.Invoke();
		return true;
	}

	public bool TryDrawCard(int cardIndex)
	{
		if (fetchingInfo || drawing || reclaiming || claimingDailyTask || pendingReclaimRequest != null || !IsOpen() || cardIndex < 1 || cardIndex > 6 || Model.OpenedCards.Any((IntermissionPlayOpenedCardDto openedCard) => openedCard.CardIndex == cardIndex) || Model.DailyTicketCount + Model.CarryTicketCount <= 0)
		{
			return false;
		}
		if (pendingDrawRequest != null && pendingDrawRequest.CardIndex != cardIndex)
		{
			return false;
		}
		if (pendingDrawRequest == null)
		{
			pendingDrawRequest = new IntermissionPlayDrawRequest
			{
				ActivityId = "#ACT#000003#",
				RequestId = Guid.NewGuid(),
				CardIndex = cardIndex
			};
		}
		ThinkingAnalyticsExtends.TrackIntermissionPlayDrawClick(base.applicationState, Model.CurrentDayIndex, pendingDrawRequest.RequestId.ToString(), pendingDrawRequest.CardIndex);
		drawing = true;
		bool shouldRefreshInfo = false;
		GeneralGameAsyncOperation<IntermissionPlayDrawRequest, IntermissionPlayDrawResponse> operation = GameAsyncOperation.Get(pendingDrawRequest, base.applicationState.ChillyRoomSDK.IntermissionPlay.DrawAsync);
		operation.HideFaultPanel().OnComplete(delegate
		{
			Model.ApplyDraw(operation.OpResult);
			GlobalRewardInfo globalRewardInfo = operation.OpResult.RewardView.ToClient(new NetworkConvertContext(base.applicationState));
			ThinkingAnalyticsExtends.TrackIntermissionPlayDrawResult(base.applicationState, Model.CurrentDayIndex, operation.OpResult.RequestId.ToString(), operation.OpResult.CardIndex, operation.OpResult.SlotId, operation.OpResult.TicketType.ToString(), globalRewardInfo, operation.OpResult.RemainingSlotCount);
			if (operation.OpResult.RemainingSlotCount == 0)
			{
				ThinkingAnalyticsExtends.TrackIntermissionPlayPoolEmpty(base.applicationState, Model.CurrentDayIndex, operation.OpResult.RequestId.ToString());
			}
			globalRewardInfo.Give(base.applicationState);
			pendingDrawResult = new IntermissionPlayDrawPresentationResult(operation.OpResult.CardIndex, operation.OpResult.SlotId, operation.OpResult.TicketType, globalRewardInfo);
			cachedInfo = IntermissionPlayInfoCache.FromResponse(Model.Info);
			base.applicationState.savingManager.SetProfileDirtyToSave();
			pendingDrawRequest = null;
			shouldRefreshInfo = true;
			StateChanged?.Invoke();
		}).OnFault(delegate
		{
			ThinkingAnalyticsExtends.TrackIntermissionPlayDrawFailure(base.applicationState, Model.CurrentDayIndex, pendingDrawRequest.RequestId.ToString(), pendingDrawRequest.CardIndex, operation.errorCode);
			if (IsActivityUnavailableFailure(operation.errorCode))
			{
				runtimeUnavailable = true;
			}
			if (IsDefiniteActivityFailure(operation.errorCode))
			{
				pendingDrawRequest = null;
			}
			TraceLog.Error("[IntermissionPlayManager] Draw failed.");
		})
			.OnOver(delegate
			{
				drawing = false;
				StateChanged?.Invoke();
				if (shouldRefreshInfo)
				{
					TryFetchInfo(forceRefresh: true);
				}
			})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_network_communicating);
		StateChanged?.Invoke();
		return true;
	}

	public bool TryReclaimTickets(int count)
	{
		if (fetchingInfo || drawing || reclaiming || claimingDailyTask || pendingDrawRequest != null || !IsOpen() || count <= 0 || count > Model.OtherworldTicketICount)
		{
			return false;
		}
		if (pendingReclaimRequest != null && pendingReclaimRequest.Count != count)
		{
			return false;
		}
		if (pendingReclaimRequest == null)
		{
			pendingReclaimRequest = new IntermissionPlayReclaimTicketsRequest
			{
				RequestId = Guid.NewGuid(),
				TicketType = IntermissionPlayTicketType.OtherworldTicketI,
				Count = count
			};
		}
		ThinkingAnalyticsExtends.TrackIntermissionPlayReclaimConfirm(base.applicationState, Model.CurrentDayIndex, pendingReclaimRequest.RequestId.ToString(), pendingReclaimRequest.TicketType.ToString(), pendingReclaimRequest.Count, pendingReclaimRequest.Count * 10);
		reclaiming = true;
		bool shouldRefreshInfo = false;
		GeneralGameAsyncOperation<IntermissionPlayReclaimTicketsRequest, IntermissionPlayReclaimTicketsResponse> operation = GameAsyncOperation.Get(pendingReclaimRequest, base.applicationState.ChillyRoomSDK.IntermissionPlay.ReclaimTicketsAsync);
		operation.HideFaultPanel().OnComplete(delegate
		{
			Model.ApplyReclaim(operation.OpResult);
			ThinkingAnalyticsExtends.TrackIntermissionPlayReclaimResult(base.applicationState, Model.CurrentDayIndex, operation.OpResult.RequestId.ToString(), operation.OpResult.TicketType.ToString(), operation.OpResult.ReclaimedCount, operation.OpResult.GiftCount);
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo
			{
				matter = new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter, operation.OpResult.GiftCount),
				rewardSourceType = GlobalRewardSourceType.Other,
				rewardSourceId = "#ACT#000003#"
			};
			globalRewardInfo.Give(base.applicationState);
			pendingReclaimResult = new IntermissionPlayReclaimPresentationResult(operation.OpResult.TicketType, operation.OpResult.ReclaimedCount, operation.OpResult.GiftCount, globalRewardInfo);
			cachedInfo = IntermissionPlayInfoCache.FromResponse(Model.Info);
			base.applicationState.savingManager.SetProfileDirtyToSave();
			pendingReclaimRequest = null;
			shouldRefreshInfo = true;
			StateChanged?.Invoke();
		}).OnFault(delegate
		{
			ThinkingAnalyticsExtends.TrackIntermissionPlayReclaimFailure(base.applicationState, Model.CurrentDayIndex, pendingReclaimRequest.RequestId.ToString(), pendingReclaimRequest.TicketType.ToString(), pendingReclaimRequest.Count, operation.errorCode);
			if (IsActivityUnavailableFailure(operation.errorCode))
			{
				runtimeUnavailable = true;
			}
			if (IsDefiniteActivityFailure(operation.errorCode))
			{
				pendingReclaimRequest = null;
			}
			TraceLog.Error("[IntermissionPlayManager] Reclaim tickets failed.");
		})
			.OnOver(delegate
			{
				reclaiming = false;
				StateChanged?.Invoke();
				if (shouldRefreshInfo)
				{
					TryFetchInfo(forceRefresh: true);
				}
			})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_network_communicating);
		StateChanged?.Invoke();
		return true;
	}

	public bool TryConsumePendingDrawResult(out IntermissionPlayDrawPresentationResult result)
	{
		result = pendingDrawResult;
		pendingDrawResult = null;
		return result != null;
	}

	public bool TryConsumePendingReclaimResult(out IntermissionPlayReclaimPresentationResult result)
	{
		result = pendingReclaimResult;
		pendingReclaimResult = null;
		return result != null;
	}

	public bool TryConsumePendingDailyTaskRewardPackage(out GlobalRewardPackage package)
	{
		package = pendingDailyTaskRewardPackage;
		pendingDailyTaskRewardPackage = null;
		return package != null;
	}

	public bool TryConsumePendingPaidTicketCount(out int count)
	{
		count = pendingPaidTicketCount;
		pendingPaidTicketCount = 0;
		return count > 0;
	}

	public void FrameUpdate()
	{
		bool flag = base.applicationState.limitedTimeActivityManager.IsOpen("#ACT#000003#");
		if (runtimeWasOpen && !flag)
		{
			StateChanged?.Invoke();
		}
		runtimeWasOpen = flag;
		if (flag && hasFreshInfo && !(Model.NextRefreshTimeUtc == default(DateTimeOffset)))
		{
			DateTimeOffset currentUtcTime = GetCurrentUtcTime();
			if (!(currentUtcTime < Model.NextRefreshTimeUtc) && !(currentUtcTime < automaticRefreshRetryAfterUtc))
			{
				automaticRefreshRetryAfterUtc = currentUtcTime + AutomaticRefreshRetryInterval;
				TryFetchInfo(forceRefresh: true);
			}
		}
	}

	private void OnEnterFortressPanel()
	{
		TryFetchInfo(forceRefresh: false);
	}

	private void OnAfterGameOver(GameRecord record)
	{
		if (record != null && record.success)
		{
			TryFetchInfo(forceRefresh: true);
		}
	}

	private void OnPurchasePaid(int _)
	{
		TryFetchInfo(forceRefresh: true);
	}

	private DateTimeOffset GetCurrentUtcTime()
	{
		try
		{
			DateTimeOffset currentTime = ServerTimePoller.Instance.GetCurrentTime();
			if (synchronizedServerTimeUtc == default(DateTimeOffset) || currentTime != synchronizedServerTimeUtc)
			{
				synchronizedServerTimeUtc = currentTime;
				synchronizedLocalTimeUtc = DateTime.UtcNow;
			}
			return synchronizedServerTimeUtc + (DateTime.UtcNow - synchronizedLocalTimeUtc);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
			return DateTimeOffset.UtcNow;
		}
	}

	private static string GetLocalizedRuntimeText(IReadOnlyDictionary<string, string> texts)
	{
		if (texts == null)
		{
			return string.Empty;
		}
		LocalizationManager.LanguageCode currentLanguage = ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage;
		string key = ((currentLanguage == LocalizationManager.LanguageCode.ChineseSimplified) ? "zh-CN" : LocalizationManager.cultureDic.GetValueOrDefault(currentLanguage, LocalizationManager.cultureDic[LocalizationManager.LanguageCode.English]));
		if (texts.TryGetValue(key, out var value))
		{
			return value;
		}
		if (!texts.TryGetValue("en", out value))
		{
			return string.Empty;
		}
		return value;
	}

	private static string FormatCountdown(TimeSpan remaining)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_intermission_play_duration_arg0_arg1_arg2_arg3.FormatWith(remaining.Days, remaining.Hours.ToString("D2"), remaining.Minutes.ToString("D2"), remaining.Seconds.ToString("D2"));
	}

	private static int ToRoundUpDays(TimeSpan span)
	{
		if (!(span <= TimeSpan.Zero))
		{
			return (int)Math.Ceiling(span.TotalDays);
		}
		return 0;
	}

	private static bool IsDefiniteActivityFailure(string errorCode)
	{
		if (!int.TryParse(errorCode, out var result))
		{
			return false;
		}
		if (result >= 118001 && result <= 118012)
		{
			return result != 118009;
		}
		return false;
	}

	private static bool IsActivityUnavailableFailure(string errorCode)
	{
		if (!(errorCode == "118001"))
		{
			return errorCode == "118002";
		}
		return true;
	}
}
