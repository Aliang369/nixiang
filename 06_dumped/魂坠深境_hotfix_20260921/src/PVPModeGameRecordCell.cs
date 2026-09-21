using System;
using System.Linq;
using ContainerOfSoul.Display.Fortress;
using TMPro;
using UnityEngine;

public class PVPModeGameRecordCell : CellUI<GlobalGameRecordManager.GameRecordPreviewInfo>
{
	public TextMeshProUGUI characterNameText;

	public TextMeshProUGUI resultText;

	public MatterCellGroup rewardMatterCellGroup;

	public TextMeshProUGUI finishTimeText;

	public GameButton inspectButton;

	[NonSerialized]
	public GlobalGameRecordManager.GameRecordPreviewInfo gameRecord;

	[NonSerialized]
	public PVPModeMainPanel parentPVPModeMainPanel;

	protected override void Init()
	{
		InitUI(rewardMatterCellGroup);
		parentPVPModeMainPanel = GetUIInParents<PVPModeMainPanel>();
		inspectButton.onClick.AddListener(delegate
		{
			gameRecord.LoadGameRecord(CellUI<GlobalGameRecordManager.GameRecordPreviewInfo>.applicationState, delegate(GameRecord record)
			{
				parentPVPModeMainPanel.recordInfoPanel.Setup(record);
				parentPVPModeMainPanel.recordInfoPanel.SetActiveAndDirty(active: true);
			});
		});
	}

	protected override void OnSetup(GlobalGameRecordManager.GameRecordPreviewInfo setupData)
	{
		gameRecord = setupData;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (gameRecord.pvpChanllengeRecord != null)
		{
			int num = gameRecord.pvpChanllengeRecord.defenderBattleRecords.Count((PVPDefenderBattleRecord r) => r.challengerFinishType == FinishBattleEncounterAction.FinishType.Win);
			characterNameText.text = gameRecord.character.characterName;
			resultText.text = (gameRecord.success ? ScriptableObjectSingleton<TextDataPreset>.instance.term_contract_success.ColorText(Color.green) : ScriptableObjectSingleton<TextDataPreset>.instance.term_defeat_pvp_defender_count_arg0_per_arg1_max.FormatWith(num, ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpDefenderPerGame).ColorText(Color.red));
			rewardMatterCellGroup.Setup(gameRecord.pvpChanllengeRecord.GetRewardMatterRewards(CellUI<GlobalGameRecordManager.GameRecordPreviewInfo>.applicationState));
			finishTimeText.text = TextDisplayHelper.GetDetailTimeSpanText(gameRecord.gameTime);
		}
	}
}
