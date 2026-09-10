using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;
using ChillyRoom.Services.Core;
using TMPro;
using UnityEngine;

public class GiftCodeManager : GlobalGameManager, IPostFetchData
{
	[SaveField(null, null)]
	private ApplicationStateInfo applicationStateInfo;

	[NonSerialized]
	private bool configLoaded;

	[NonSerialized]
	private bool configLoading;

	[NonSerialized]
	private bool giftCodeEnabled;

	public bool GiftCodeEnabled => giftCodeEnabled;

	public override void Init(ApplicationStateInfo applicationState)
	{
		applicationStateInfo = applicationState;
		giftCodeEnabled = false;
		configLoading = false;
		configLoaded = !GlobalConfigManager.instance.enableNetwork;
	}

	public IEnumerator PostFetchData()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			Task task = StartGiftCodePolling(null, 1, isInit: true);
			yield return new WaitUntil(() => task.IsCompleted);
		}
	}

	public void RequestConfigIfNeeded()
	{
		if (GlobalConfigManager.instance.enableNetwork && !configLoaded && !configLoading)
		{
			configLoading = true;
			GeneralGameAsyncOperationResultOnly<GiftCodeConfigReply> op = new GeneralGameAsyncOperationResultOnly<GiftCodeConfigReply>(applicationStateInfo.ChillyRoomSDK.GiftCode.GetGiftCodeConfigAsync);
			op.OnComplete(delegate
			{
				giftCodeEnabled = op.OpResult.GiftCodeEnabled;
			}).OnOver(delegate
			{
				configLoading = false;
				configLoaded = true;
			}).HideFaultPanel()
				.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_gift_code_config);
		}
	}

	public IEnumerator LoadConfigIfNeededCor()
	{
		RequestConfigIfNeeded();
		if (GlobalConfigManager.instance.enableNetwork)
		{
			yield return new WaitUntil(() => configLoaded);
		}
	}

	public void AcceptGiftCode(string code, TextMeshProUGUI errorText)
	{
		applicationStateInfo.eventManager.activityMissionGiftCodeEvent.TriggerEvent(new YuanQiActivityMissionGiftCodeEventData(code, null));
		if (errorText != null)
		{
			errorText.text = string.Empty;
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			GeneralGameAsyncOperation<string, IResult<GiftInfo, ExchangeGiftCodeError>> op = new GeneralGameAsyncOperation<string, IResult<GiftInfo, ExchangeGiftCodeError>>(code, async (string c) => await applicationStateInfo.ChillyRoomSDK.Core.ExchangeAsync(c));
			op.OnComplete(delegate
			{
				Debug.Log("[containerofsoul] AcceptGiftCode OnComplete : " + code);
				if (op.OpResult.IsOk())
				{
					Debug.Log($"Module:GiftCode Guid :{op.OpResult.Data.Guid} ,payload:{op.OpResult.Data.PayLoad}");
					StartGiftCodePolling(errorText, 5, isInit: false);
				}
				else
				{
					if (errorText != null)
					{
						errorText.text = op.OpResult.Error.Message;
					}
					Debug.Log($"Module:GiftCode ErrorCode:{op.OpResult.Error.StatusCode},Error Message:{op.OpResult.Error.Message}");
				}
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_redeeming_gift_code);
		}
		else
		{
			Debug.Log("未联网状态下不知道怎么处理礼包码");
		}
	}

	private async Task StartGiftCodePolling(TextMeshProUGUI errorText, int retryTime, bool isInit)
	{
		WaitPanelBase.Token waitPanel = null;
		if (!isInit)
		{
			waitPanel = BootstrapExtra.instance.waitPanelBase.Show(base.textData.gameAsyncOperationText.term_gift_code);
		}
		await Process();
		if (!isInit)
		{
			BootstrapExtra.instance.waitPanelBase.Hide(waitPanel);
		}
		async Task Process()
		{
			TraceLog.Debug("[GiftCodeManager] StartGiftCodePolling()");
			int times = 0;
			try
			{
				while (times < retryTime)
				{
					if (TraceLog.EnabledLog)
					{
						TraceLog.Debug($"[GiftCodeManager] StartGiftCodePolling() start - {times}");
					}
					ClaimGiftCodeReply claimGiftCodeReply = await applicationStateInfo.ChillyRoomSDK.GiftCode.ClaimGiftCodeAsync();
					if (!claimGiftCodeReply.RewardInfos.IsNullOrEmpty())
					{
						if (TraceLog.EnabledLog)
						{
							TraceLog.Debug($"[GiftCodeManager] StartGiftCodePolling() success - {claimGiftCodeReply.RewardInfos.Count}");
						}
						List<GlobalRewardInfo> rewards = claimGiftCodeReply.RewardInfos.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(new NetworkConvertContext(applicationStateInfo))).ToList();
						DisplayGiftCodeRewards(rewards);
						return;
					}
					if (TraceLog.EnabledLog)
					{
						TraceLog.Debug($"[GiftCodeManager] StartGiftCodePolling() no result - {times}");
					}
					times++;
					await Task.Delay(1000);
				}
			}
			catch (Exception ex)
			{
				TraceLog.Error("[GiftCodeManager] StartGiftCodePolling() - e: " + ex.Message);
			}
			if (errorText != null)
			{
				errorText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_gift_code_claim_failed;
			}
		}
	}

	private void DisplayGiftCodeRewards(List<GlobalRewardInfo> rewards)
	{
		foreach (GlobalRewardInfo reward in rewards)
		{
			reward.Give(applicationStateInfo);
		}
		applicationStateInfo.fortressState.uiManager.globalRewardPackageDisplay.Setup(new GlobalRewardPackage(rewards));
	}
}
