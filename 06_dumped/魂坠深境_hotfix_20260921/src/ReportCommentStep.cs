using System;
using ChillyRoom.Services.ContainerOfSoul;
using TMPro;
using UnityEngine;

public sealed class ReportCommentStep : UIBase
{
	public const int MinCommentLength = 10;

	public const int MaxCommentLength = 200;

	[SerializeField]
	private TextMeshProUGUI selectedTypeText;

	[SerializeField]
	private TMP_InputField commentInput;

	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private GameButton submitButton;

	[SerializeField]
	private GameButton backButton;

	public Action<string> onSubmit;

	public Action onBack;

	private bool submitting;

	protected override void Init()
	{
		base.Init();
		commentInput.characterLimit = 200;
		commentInput.lineType = TMP_InputField.LineType.MultiLineNewline;
		if (commentInput.placeholder is TextMeshProUGUI textMeshProUGUI)
		{
			textMeshProUGUI.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_report_comment_hint;
		}
		commentInput.onValueChanged.AddListener(OnCommentChanged);
		submitButton.onClick.AddListener(OnSubmitClicked);
		backButton.onClick.AddListener(delegate
		{
			if (!submitting)
			{
				onBack?.Invoke();
			}
		});
	}

	public void Show(ForerunnerLegacyReportApiType type)
	{
		SetActiveAndDirty(active: true);
		submitting = false;
		if (selectedTypeText != null)
		{
			selectedTypeText.text = ForerunnerLegacyReportView.GetTypeText(type);
		}
		commentInput.SetTextWithoutNotify(string.Empty);
		OnCommentChanged(string.Empty);
		commentInput.ActivateInputField();
	}

	public void Hide()
	{
		SetActiveAndDirty(active: false);
	}

	public void SetSubmitting(bool value)
	{
		submitting = value;
		OnCommentChanged(commentInput.text);
	}

	private void OnSubmitClicked()
	{
		if (!submitting)
		{
			string text = commentInput.text.Trim();
			int length = text.Length;
			if (length >= 10 && length <= 200)
			{
				submitting = true;
				submitButton.interactable = false;
				onSubmit?.Invoke(text);
			}
		}
	}

	private void OnCommentChanged(string value)
	{
		int num = value?.Length ?? 0;
		if (countText != null)
		{
			countText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_report_comment_count_arg0.FormatWith(num);
		}
		submitButton.interactable = !submitting && num >= 10 && num <= 200;
	}
}
