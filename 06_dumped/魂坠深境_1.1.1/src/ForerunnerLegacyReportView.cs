using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public sealed class ForerunnerLegacyReportView : UIBase
{
	[SerializeField]
	private ReportArchiveStep archiveStep;

	[SerializeField]
	private ReportTypeStep typeStep;

	[SerializeField]
	private ReportCommentStep commentStep;

	[SerializeField]
	private ReportSuccessStep successStep;

	private StoryNotePanel owner;

	private StoryNoteInfo noteInfo;

	private ForerunnerLegacyReportApiStateResult reportState;

	private ForerunnerLegacyReportApiType selectedType;

	private string boundGuid;

	private bool submitting;

	private bool debugSimulation;

	public void Setup(StoryNotePanel value)
	{
		owner = value;
	}

	protected override void Init()
	{
		base.Init();
		InitUI(archiveStep);
		InitUI(typeStep);
		InitUI(commentStep);
		InitUI(successStep);
		archiveStep.onConfirm = OnArchiveConfirm;
		archiveStep.onClose = CloseAll;
		typeStep.onNext = OnTypeSelected;
		typeStep.onBack = GoToArchive;
		commentStep.onSubmit = OnCommentSubmit;
		commentStep.onBack = GoToType;
		successStep.onConfirm = CloseAll;
		HideAllSteps();
		base.gameObject.SetActive(value: false);
	}

	protected override void Dispose()
	{
		base.Dispose();
		Reset();
	}

	public void Bind(StoryNoteInfo value)
	{
		noteInfo = value;
		debugSimulation = false;
		reportState = null;
		selectedType = (ForerunnerLegacyReportApiType)0;
		submitting = false;
		boundGuid = value?.forerunnerLegacy?.guid;
	}

	public void EnableDebugSimulation()
	{
		debugSimulation = true;
	}

	public void OpenArchive()
	{
		SetActiveAndDirty(active: true);
		GoToArchive();
		FetchReportState();
	}

	public void Close()
	{
		CloseAll();
	}

	public void Reset()
	{
		HideAllSteps();
		submitting = false;
		debugSimulation = false;
		base.gameObject.SetActive(value: false);
		owner?.SetLetterContentVisible(visible: true);
	}

	private void GoToArchive()
	{
		HideAllSteps();
		owner?.SetLetterContentVisible(visible: true);
		archiveStep.Show(noteInfo);
		archiveStep.SetReported(reportState?.AlreadySubmitted ?? false);
	}

	private void GoToType()
	{
		HideAllSteps();
		owner?.SetLetterContentVisible(visible: false);
		typeStep.Show();
	}

	private void GoToComment()
	{
		HideAllSteps();
		owner?.SetLetterContentVisible(visible: false);
		commentStep.Show(selectedType);
	}

	private void GoToSuccess()
	{
		HideAllSteps();
		owner?.SetLetterContentVisible(visible: false);
		successStep.Show();
		if (!successStep.HasConfirmButton)
		{
			CloseAll();
		}
	}

	private void CloseAll()
	{
		Reset();
	}

	private void HideAllSteps()
	{
		archiveStep.Hide();
		typeStep.Hide();
		commentStep.Hide();
		successStep.Hide();
	}

	private void OnArchiveConfirm()
	{
		if (reportState == null)
		{
			HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_report_loading);
		}
		else if (!reportState.CanReport)
		{
			HintText(GetUnavailableReasonText(reportState));
		}
		else
		{
			GoToType();
		}
	}

	private void OnTypeSelected(ForerunnerLegacyReportApiType type)
	{
		selectedType = type;
		GoToComment();
	}

	private void OnCommentSubmit(string comment)
	{
		if (selectedType == (ForerunnerLegacyReportApiType)0 || submitting)
		{
			return;
		}
		submitting = true;
		if (debugSimulation)
		{
			reportState = new ForerunnerLegacyReportApiStateResult
			{
				CanReport = false,
				AlreadySubmitted = true
			};
			submitting = false;
			GoToSuccess();
			return;
		}
		Bootstrap.applicationState.playerReportManager.ReportForerunnerLegacy(boundGuid, selectedType, comment, delegate
		{
			reportState = new ForerunnerLegacyReportApiStateResult
			{
				CanReport = false,
				AlreadySubmitted = true
			};
			submitting = false;
			GoToSuccess();
		}, delegate
		{
			submitting = false;
			commentStep.SetSubmitting(value: false);
		});
	}

	private void FetchReportState()
	{
		if (noteInfo?.forerunnerLegacy == null)
		{
			return;
		}
		if (debugSimulation)
		{
			reportState = new ForerunnerLegacyReportApiStateResult
			{
				CanReport = true
			};
			archiveStep.SetReported(reported: false);
			return;
		}
		if (ForerunnerPresetMessageResolver.TryGet(noteInfo.forerunnerLegacy.presetMessageId, out var _))
		{
			reportState = new ForerunnerLegacyReportApiStateResult
			{
				CanReport = false,
				UnavailableReason = ForerunnerLegacyReportApiUnavailableReason.OfficialPresetMessage
			};
			archiveStep.SetReported(reported: false);
			return;
		}
		string requestGuid = boundGuid;
		Bootstrap.applicationState.playerReportManager.GetForerunnerLegacyReportState(requestGuid, delegate(ForerunnerLegacyReportApiStateResult result)
		{
			if (base.gameObject.activeSelf && requestGuid == boundGuid)
			{
				reportState = result;
				archiveStep.SetReported(result.AlreadySubmitted);
			}
		}, delegate
		{
			if (base.gameObject.activeSelf && requestGuid == boundGuid)
			{
				reportState = new ForerunnerLegacyReportApiStateResult
				{
					CanReport = false,
					UnavailableReason = ForerunnerLegacyReportApiUnavailableReason.LegacyNotReceived
				};
			}
		});
	}

	private static void HintText(string text)
	{
		(UIBase.gameState?.UIManager?.quickHintPanel ?? UIBase.fortressState?.uiManager?.quickHintPanel)?.HintText(text);
	}

	private static string GetUnavailableReasonText(ForerunnerLegacyReportApiStateResult state)
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		if (state.AlreadySubmitted)
		{
			return instance.term_forerunner_report_submitted;
		}
		if (state.UnavailableReason == ForerunnerLegacyReportApiUnavailableReason.OfficialPresetMessage)
		{
			return instance.term_forerunner_report_official_message;
		}
		return instance.term_forerunner_report_unavailable;
	}

	public static string GetTypeText(ForerunnerLegacyReportApiType type)
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		return type switch
		{
			ForerunnerLegacyReportApiType.OffensiveOrHateful => instance.term_forerunner_report_type_offensive, 
			ForerunnerLegacyReportApiType.HarassmentOrPersonalInfo => instance.term_forerunner_report_type_harassment, 
			ForerunnerLegacyReportApiType.AdvertisingOrIrrelevant => instance.term_forerunner_report_type_advertising, 
			_ => instance.term_forerunner_report_type_other, 
		};
	}
}
