using TMPro;

public class PVPDefenderBattleRecordCell : CellUI<PVPDefenderBattleRecord>
{
	public TextMeshProUGUI challengerNameText;

	public TextMeshProUGUI challengerPlayerIdText;

	public TextMeshProUGUI battleResultText;

	public TextMeshProUGUI battleTimeText;

	public TextMeshProUGUI damageDealtValueText;

	public GameButton inspectReplayButton;

	protected override void Init()
	{
		inspectReplayButton.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		challengerNameText.text = base.setupData.challengePlayerNickName;
		challengerPlayerIdText.text = base.setupData.challengePlayerId;
		battleResultText.text = ((base.setupData.challengerFinishType == FinishBattleEncounterAction.FinishType.Win) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_battle_lose : ScriptableObjectSingleton<TextDataPreset>.instance.term_battle_win);
		battleTimeText.text = base.setupData.battleDateTime.LocalDateTime.ToShortDateString() + " " + base.setupData.battleDateTime.LocalDateTime.ToShortTimeString();
		damageDealtValueText.text = base.setupData.dealtDamageToChallenger.ToString();
	}
}
