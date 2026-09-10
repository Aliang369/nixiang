using System;
using TMPro;
using UnityEngine;

public sealed class ReportArchiveStep : UIBase
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI pidText;

	[SerializeField]
	private TextMeshProUGUI hasReportText;

	[SerializeField]
	private TextMeshProUGUI hasReportTitleText;

	[SerializeField]
	private GameButton sureReportButton;

	[SerializeField]
	private GameButton closeButton;

	public Action onConfirm;

	public Action onClose;

	protected override void Init()
	{
		base.Init();
		sureReportButton.onClick.AddListener(delegate
		{
			onConfirm?.Invoke();
		});
		closeButton.onClick.AddListener(delegate
		{
			onClose?.Invoke();
		});
	}

	public void Show(StoryNoteInfo note)
	{
		SetActiveAndDirty(active: true);
		if (nameText != null)
		{
			nameText.text = note.signature;
		}
		if (pidText != null)
		{
			long num = note.forerunnerLegacy?.sourcePlayerId ?? 0;
			pidText.gameObject.SetActive(num > 0);
			pidText.text = ((num > 0) ? $"PID{num}" : string.Empty);
		}
		hasReportTitleText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_report_title_submitted;
		SetReported(reported: false);
	}

	public void SetReported(bool reported)
	{
		hasReportText.gameObject.SetActive(reported);
		if (reported)
		{
			hasReportText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_report_submitted;
		}
	}

	public void Hide()
	{
		SetActiveAndDirty(active: false);
	}
}
