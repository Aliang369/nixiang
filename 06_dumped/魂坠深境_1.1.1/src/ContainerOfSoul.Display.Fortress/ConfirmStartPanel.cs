using System;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress;

public class ConfirmStartPanel : UIBase
{
	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton cancelButton;

	[NonSerialized]
	public Action beforeConfirm;

	protected override void Init()
	{
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
	}

	private void AbandonCurrentGame()
	{
		NewGameConfigData finishedRunConfig = UIBase.applicationState.savedGameState?.newGameConfigData;
		if (UIBase.applicationState.savedGameState != null)
		{
			try
			{
				ThinkingAnalyticsExtends.TrackGameOver(UIBase.applicationState.savedGameState, UIBase.applicationState.savedGameState.recordManager.GenerateGameRecord(success: false, finished: false));
			}
			catch (Exception e)
			{
				TraceLog.Exception(e);
			}
			UIBase.applicationState.RecordLastGameEndReason(LastGameEndReason.ReturnToLobby);
		}
		UIBase.applicationState.savedGameState = null;
		UIBase.applicationState.newGameConfigManager.ResetAfterLimitedDeploymentRun(finishedRunConfig);
		UIBase.applicationState.savingManager.SetProfileDirtyToSave();
	}

	private void _OnConfirmButtonClicked()
	{
		beforeConfirm?.Invoke();
		beforeConfirm = null;
		AbandonCurrentGame();
		UIBase.fortressState.uiManager.ShowUI(UIBase.fortressState.uiManager.baseGameUIPanel);
		base.gameObject.SetActive(value: false);
	}

	private void _OnCancelButtonClicked()
	{
		base.gameObject.SetActive(value: false);
	}
}
