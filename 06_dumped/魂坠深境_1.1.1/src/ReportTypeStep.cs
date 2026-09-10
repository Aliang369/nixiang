using System;
using ChillyRoom.Services.ContainerOfSoul;
using TMPro;
using UnityEngine;

public sealed class ReportTypeStep : UIBase
{
	[SerializeField]
	private GameButton[] typeButtons;

	[SerializeField]
	private GameObject[] selectedMarks;

	[SerializeField]
	private TextMeshProUGUI[] typeLabels;

	[SerializeField]
	private GameButton nextStepButton;

	[SerializeField]
	private GameButton backButton;

	public Action<ForerunnerLegacyReportApiType> onNext;

	public Action onBack;

	private ForerunnerLegacyReportApiType selectedType;

	protected override void Init()
	{
		base.Init();
		for (int i = 0; i < typeButtons.Length; i++)
		{
			int index = i;
			typeButtons[i].onClick.AddListener(delegate
			{
				SelectType(index);
			});
		}
		nextStepButton.onClick.AddListener(OnNextClicked);
		backButton.onClick.AddListener(delegate
		{
			onBack?.Invoke();
		});
		SetupLabels();
	}

	public void Show()
	{
		SetActiveAndDirty(active: true);
		selectedType = (ForerunnerLegacyReportApiType)0;
		RefreshSelection();
	}

	public void Hide()
	{
		SetActiveAndDirty(active: false);
	}

	private void SelectType(int index)
	{
		selectedType = (ForerunnerLegacyReportApiType)(index + 1);
		RefreshSelection();
	}

	private void OnNextClicked()
	{
		if (selectedType != 0)
		{
			onNext?.Invoke(selectedType);
		}
	}

	private void RefreshSelection()
	{
		for (int i = 0; i < selectedMarks.Length; i++)
		{
			if (selectedMarks[i] != null)
			{
				selectedMarks[i].SetActive(selectedType == (ForerunnerLegacyReportApiType)(i + 1));
			}
		}
		nextStepButton.interactable = selectedType != (ForerunnerLegacyReportApiType)0;
	}

	private void SetupLabels()
	{
		if (typeLabels != null && typeLabels.Length >= 4)
		{
			TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
			typeLabels[0].text = instance.term_forerunner_report_type_offensive;
			typeLabels[1].text = instance.term_forerunner_report_type_harassment;
			typeLabels[2].text = instance.term_forerunner_report_type_advertising;
			typeLabels[3].text = instance.term_forerunner_report_type_other;
		}
	}
}
