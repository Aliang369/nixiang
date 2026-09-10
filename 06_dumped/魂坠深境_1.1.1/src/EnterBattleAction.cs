using Common.UI;

public class EnterBattleAction : ExecuteAction
{
	protected override void Update()
	{
		gameState.UIManager.battlePanel.TriggerEvent(UIEvent.OnBattleStart);
		gameState.battleState.StartBattle();
	}
}
