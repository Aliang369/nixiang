using System;
using System.Collections;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class DataMigrationManager : GlobalGameManager
{
	private const int RejectCacheValidSeconds = 604800;

	private const string RejectCacheKeyPrefix = "DataMigration.Rejected.";

	[NonSerialized]
	public bool statusLoaded;

	[NonSerialized]
	private bool statusLoading;

	[NonSerialized]
	public PlayerDataMigrationStatus status;

	private string RejectCacheKey => "DataMigration.Rejected." + Bootstrap.instance.CurrentAccountID;

	public override void Init(ApplicationStateInfo applicationState)
	{
		statusLoaded = false;
		statusLoading = false;
		status = PlayerDataMigrationStatus.NoAnomaly;
	}

	public void RequestStatusIfNeeded()
	{
		if (!GlobalConfigManager.instance.enableNetwork || statusLoaded || statusLoading)
		{
			return;
		}
		if (HasValidRejectCache())
		{
			statusLoaded = true;
			return;
		}
		statusLoading = true;
		GeneralGameAsyncOperationResultOnly<PlayerMigrationStatusDTO> op = new GeneralGameAsyncOperationResultOnly<PlayerMigrationStatusDTO>(base.applicationState.ChillyRoomSDK.DataOptimization.CheckCrossRegionStatusAsync);
		op.OnComplete(delegate
		{
			status = op.OpResult.Status;
		}).OnOver(delegate
		{
			statusLoading = false;
			statusLoaded = true;
		}).HideFaultPanel()
			.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_checking_data_migration);
	}

	public IEnumerator TryTriggerMigration()
	{
		return TryTriggerMigration(null);
	}

	public IEnumerator TryTriggerMigration(Action beforeVisibleInterruption)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			yield return new WaitUntil(() => statusLoaded);
			if (status == PlayerDataMigrationStatus.Ready && !HasValidRejectCache())
			{
				yield return TriggerMigrationPopupCor(beforeVisibleInterruption);
			}
		}
	}

	private IEnumerator TriggerMigrationPopupCor(Action beforeVisibleInterruption = null)
	{
		SecondConfirmPanel secondConfirmPanel = base.fortressState?.uiManager?.commonSecondConfirmPanel;
		if (!(secondConfirmPanel == null))
		{
			beforeVisibleInterruption?.Invoke();
			bool closed = false;
			TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
			secondConfirmPanel.Setup(new SecondConfirmInfo
			{
				description = instance.term_data_migration_confirm_description,
				confirmChoiceText = instance.term_data_migration_confirm_button,
				cancelChoiceText = instance.term_data_migration_cancel_button,
				confirmCallback = delegate
				{
					ConfirmMigration();
					closed = true;
				},
				cancelCallback = delegate
				{
					WriteRejectCache();
					closed = true;
				}
			});
			yield return new WaitUntil(() => closed);
		}
	}

	private void ConfirmMigration()
	{
		GeneralGameAsyncOperationResultOnly<PlayerMigrationResultDTO> op = new GeneralGameAsyncOperationResultOnly<PlayerMigrationResultDTO>(base.applicationState.ChillyRoomSDK.DataOptimization.ConfirmMigrationAsync);
		op.OnComplete(delegate
		{
			PlayerMigrationResultDTO opResult = op.OpResult;
			QuickHintPanel quickHintPanel = base.fortressState?.uiManager?.quickHintPanel;
			TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
			if (!opResult.Success)
			{
				quickHintPanel?.HintText(string.IsNullOrEmpty(opResult.Reason) ? instance.term_data_migration_failed_fallback : opResult.Reason);
			}
			else
			{
				status = opResult.Status;
				ClearRejectCache();
				base.applicationState.playerInfoManager.UpdateUserProfile();
				quickHintPanel?.HintText(instance.term_data_migration_success);
			}
		}).EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_migrating_data);
	}

	private bool HasValidRejectCache()
	{
		string text = PlayerPrefs.GetString(RejectCacheKey, string.Empty);
		if (string.IsNullOrEmpty(text) || !long.TryParse(text, out var result))
		{
			return false;
		}
		long num = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - result;
		if (num >= 0)
		{
			return num < 604800;
		}
		return false;
	}

	private void WriteRejectCache()
	{
		PlayerPrefs.SetString(RejectCacheKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
		PlayerPrefs.Save();
	}

	private void ClearRejectCache()
	{
		PlayerPrefs.DeleteKey(RejectCacheKey);
		PlayerPrefs.Save();
	}
}
