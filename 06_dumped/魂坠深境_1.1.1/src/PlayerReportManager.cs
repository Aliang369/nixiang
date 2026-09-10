using System;
using ChillyRoom.Services.ContainerOfSoul;

public class PlayerReportManager : GlobalGameManager
{
	public void ReportPlayer(PlayerReportRequest request)
	{
		GeneralGameAsyncOperation<PlayerReportRequest, PlayerReportResult> operation = new GeneralGameAsyncOperation<PlayerReportRequest, PlayerReportResult>(request, base.applicationState.ChillyRoomSDK.PlayerReport.PlayerReportAsync);
		operation.OnComplete(delegate
		{
			TraceLog.Debug($"玩家举报结果: {operation.OpResult.ReportId} - {operation.OpResult.Status} - {operation.OpResult.Message}");
		}).ManualRetry().EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_submitting_player_report);
	}

	public void GetForerunnerLegacyReportState(string legacyGuid, Action<ForerunnerLegacyReportApiStateResult> onComplete, Action onFault = null)
	{
		ForerunnerLegacyReportApiStateRequest request = new ForerunnerLegacyReportApiStateRequest
		{
			ForerunnerLegacyGuid = legacyGuid
		};
		GeneralGameAsyncOperation<ForerunnerLegacyReportApiStateRequest, ForerunnerLegacyReportApiStateResult> operation = new GeneralGameAsyncOperation<ForerunnerLegacyReportApiStateRequest, ForerunnerLegacyReportApiStateResult>(request, base.applicationState.ChillyRoomSDK.PlayerReport.GetForerunnerLegacyReportStateCompatAsync);
		operation.ManualRetry().OnComplete(delegate
		{
			onComplete?.Invoke(operation.OpResult);
		}).OnFault(onFault)
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_report_state);
	}

	public void ReportForerunnerLegacy(string legacyGuid, ForerunnerLegacyReportApiType reportType, string comment, Action<ForerunnerLegacyReportApiSubmitResult> onComplete, Action onFault = null)
	{
		ForerunnerLegacyReportApiSubmitRequest request = new ForerunnerLegacyReportApiSubmitRequest
		{
			ForerunnerLegacyGuid = legacyGuid,
			ReportType = reportType,
			CommentNote = comment
		};
		GeneralGameAsyncOperation<ForerunnerLegacyReportApiSubmitRequest, ForerunnerLegacyReportApiSubmitResult> operation = new GeneralGameAsyncOperation<ForerunnerLegacyReportApiSubmitRequest, ForerunnerLegacyReportApiSubmitResult>(request, base.applicationState.ChillyRoomSDK.PlayerReport.ReportForerunnerLegacyCompatAsync);
		operation.ManualRetry().OnComplete(delegate
		{
			onComplete?.Invoke(operation.OpResult);
		}).OnFault(onFault)
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_submitting_player_report);
	}
}
