using System;
using TMPro;
using UnityEngine;

public sealed class ReportSuccessStep : UIBase
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private GameButton confirmButton;

	public Action onConfirm;

	public bool HasConfirmButton => confirmButton != null;

	protected override void Init()
	{
		base.Init();
		if (confirmButton != null)
		{
			confirmButton.onClick.AddListener(delegate
			{
				onConfirm?.Invoke();
			});
		}
	}

	public void Show()
	{
		SetActiveAndDirty(active: true);
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		if (titleText != null)
		{
			titleText.text = instance.term_forerunner_report_success;
		}
		if (descriptionText != null)
		{
			descriptionText.text = instance.term_forerunner_report_success_description;
		}
	}

	public void Hide()
	{
		SetActiveAndDirty(active: false);
	}
}
