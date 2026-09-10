using System;
using System.Collections;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class PleaseReviewPopupManager : GlobalGameManager
{
	[NonSerialized]
	public bool configLoaded;

	[NonSerialized]
	private bool configLoading;

	[NonSerialized]
	public int pleaseReviewPopupType;

	public override void Init(ApplicationStateInfo applicationState)
	{
		configLoading = false;
		pleaseReviewPopupType = -1;
		configLoaded = false;
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			configLoading = false;
			configLoaded = true;
			pleaseReviewPopupType = 0;
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

	public void RequestConfigIfNeeded()
	{
		if (!configLoaded && !configLoading)
		{
			configLoading = true;
			GeneralGameAsyncOperationResultOnly<PleaseReviewPopupConfigReply> op = new GeneralGameAsyncOperationResultOnly<PleaseReviewPopupConfigReply>(base.applicationState.ChillyRoomSDK.PleaseReviewPopup.GetPleaseReviewPopupConfigAsync);
			op.OnComplete(delegate
			{
				PleaseReviewPopupConfigReply opResult = op.OpResult;
				pleaseReviewPopupType = ((opResult.PleaseReviewPopupType != -1 || !opResult.PleaseReviewPopupEnabled) ? opResult.PleaseReviewPopupType : 0);
				TraceLog.Debug($"[PleaseReviewPopupManager] RequestConfigIfNeeded() - config loaded, enabled={opResult.PleaseReviewPopupEnabled}, rawType={opResult.PleaseReviewPopupType}, resolvedType={pleaseReviewPopupType}");
			}).OnOver(delegate
			{
				configLoading = false;
				configLoaded = true;
			}).HideFaultPanel()
				.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_submitting_review_feedback);
		}
	}
}
