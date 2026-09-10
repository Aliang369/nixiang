using TMPro;

public class CurrentTurnCountDisplay : UIBase
{
	private TextMeshProUGUI tmp;

	private void Start()
	{
		tmp = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if (UIBase.gameState.battleState.isInBattle)
		{
			tmp.enabled = true;
			tmp.text = string.Format(ScriptableObjectSingleton<TextDataPreset>.instance.term_turn_count_arg0, UIBase.gameState.battleState.turnCount);
		}
		else
		{
			tmp.enabled = false;
		}
	}
}
