using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class GameRecordInfoCell : CellUI<GlobalGameRecordManager.GameRecordPreviewInfo>
{
	[SerializeField]
	private GameButton showDetailButton;

	[SerializeField]
	private TextMeshProUGUI contractNameText;

	[SerializeField]
	private TextMeshProUGUI resultText;

	[SerializeField]
	private Image characterAvatarImage;

	[SerializeField]
	private TextMeshProUGUI timeText;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private TextMeshProUGUI rankingScoreText;

	[SerializeField]
	private Color successColor = new Color(0.961f, 0.776f, 0.459f, 1f);

	[SerializeField]
	private Color failedColor = new Color(0.659f, 0.741f, 0.914f, 1f);

	[NonSerialized]
	private FortressGameRecordPanel _fortressGameRecordPanel;

	[NonSerialized]
	private GlobalGameRecordManager.GameRecordPreviewInfo previewRecord;

	protected override void Init()
	{
		base.Init();
		_fortressGameRecordPanel = parent as FortressGameRecordPanel;
		showDetailButton.onClick.RemoveAllListeners();
		showDetailButton.onClick.AddListener(_OnShowDetailButtonClicked);
	}

	protected override void OnSetup(GlobalGameRecordManager.GameRecordPreviewInfo dailyContract)
	{
		previewRecord = dailyContract;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		resultText.text = (previewRecord.success ? ScriptableObjectSingleton<TextDataPreset>.instance.term_contract_success : ScriptableObjectSingleton<TextDataPreset>.instance.term_contract_failed);
		resultText.color = (previewRecord.success ? successColor : failedColor);
		contractNameText.text = previewRecord.contract.ContractName;
		if (previewRecord.avatar == null)
		{
			previewRecord.avatar = previewRecord.CharacterRecordIcon;
		}
		characterAvatarImage.sprite = previewRecord.avatar;
		characterAvatarImage.enabled = characterAvatarImage.sprite != null;
		timeText.text = TextDisplayHelper.GetDetailTimeSpanText(previewRecord.gameTime);
		progressText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_game_progress_arg0.FormatWith(previewRecord.progress);
		rankingScoreText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_ranking_score_arg0.FormatWith(previewRecord.rankingScore);
	}

	private void _OnShowDetailButtonClicked()
	{
		previewRecord.LoadGameRecord(CellUI<GlobalGameRecordManager.GameRecordPreviewInfo>.applicationState, delegate(GameRecord record)
		{
			_fortressGameRecordPanel.ShowRecordInfoPanel(record);
		});
	}
}
