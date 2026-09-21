using System;
using System.Collections;
using ChillyRoom.Services.ContainerOfSoul;
using DG.Tweening;
using UnityEngine;

public class ForerunnerPanel : CellUI<int>
{
	public CanvasGroup canvasGroup;

	public CanvasGroup hintPanelCanvasGroup;

	public ForerunnerHintPanel forerunnerHintPanel;

	public ForerunnerSelectPanel forerunnerSelectPanel;

	public ForerunnerLeaveNotePanel forerunnerLeaveNotePanel;

	public float fadeTime = 0.5f;

	[NonSerialized]
	public ForerunnerLegacy forerunnerLegacy;

	[NonSerialized]
	public bool finalShare;

	protected override void Init()
	{
		InitUI(forerunnerHintPanel);
		InitUI(forerunnerSelectPanel);
		InitUI(forerunnerLeaveNotePanel);
	}

	public void UploadForrunnerLegacy(bool share)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			forerunnerLegacy.guid = Guid.NewGuid().ToString();
			forerunnerLegacy.leaveTime = DateTime.UtcNow;
			new GeneralGameAsyncOperationRequestOnly<UploadForerunnerLegacyRequest>(new UploadForerunnerLegacyRequest(forerunnerLegacy, share), CellUI<int>.applicationState.ChillyRoomSDK.Forerunner.UploadShareForerunnerLegacyAsync).OnComplete(AfterUpload).ManualRetry().OnFault(OnFault)
				.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_uploading_forerunner_legacy);
		}
		void AfterUpload()
		{
			CellUI<int>.gameState.applicationState.forerunnerManager.localForerunnerLegacy = forerunnerLegacy;
			if (share)
			{
				CellUI<int>.gameState.applicationState.forerunnerManager.shareForerunnerLegacy = forerunnerLegacy;
			}
			finalShare = share;
			Finish();
		}
		static void OnFault()
		{
			CellUI<int>.gameState.UIManager.forerunnerPanel.ShowForeRunnerPanel();
		}
	}

	public void Finish()
	{
		ThinkingAnalyticsExtends.TrackForerunnerDeath(CellUI<int>.gameState, forerunnerLegacy, finalShare);
		CellUI<int>.gameState.StartCoroutine(HideCor);
	}

	public IEnumerator ShowCor()
	{
		if (CellUI<int>.gameState.dungeonMapManager.currentDungeonMap?.currentLocation != null)
		{
			base.gameObject.SetActive(value: true);
			canvasGroup.alpha = 0f;
			forerunnerLegacy = new ForerunnerLegacy
			{
				stage = CellUI<int>.gameState.contractState.stage,
				expireTime = DateTime.Now + TimeSpan.FromDays(7.0),
				sourceDraftMap = CellUI<int>.gameState.dungeonMapManager.currentDungeonMap.sourceDraftMap,
				locationPivotIndex = CellUI<int>.gameState.dungeonMapManager.currentDungeonMap.currentLocation.draftMapIndex,
				signature = CellUI<int>.applicationState.playerInfoManager.DataName
			};
			forerunnerHintPanel.Setup(forerunnerLegacy);
			forerunnerHintPanel.SetActiveAndDirty(active: true);
			if (!CellUI<int>.gameState.actionManager.IsActionLocked(ActionLock.Type.ForerunnerGiveLegacy))
			{
				CellUI<int>.gameState.actionManager.AcquireLock(ActionLock.Type.ForerunnerGiveLegacy);
			}
			yield return DOTweenModuleUI.DOFade(canvasGroup, 1f, fadeTime).WaitForCompletion();
		}
	}

	private IEnumerator HideCor()
	{
		yield return DOTweenModuleUI.DOFade(hintPanelCanvasGroup, 0f, fadeTime).WaitForCompletion();
		yield return DOTweenModuleUI.DOFade(canvasGroup, 0f, fadeTime).WaitForCompletion();
		CellUI<int>.gameState.actionManager.ReleaseLock(ActionLock.Type.ForerunnerGiveLegacy);
		base.gameObject.SetActive(value: false);
	}
}
