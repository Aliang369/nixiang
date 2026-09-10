public class ShowStartGameBuffConfigPanelAction : GameAction
{
	protected override void Update()
	{
		gameState.UIManager.startGameBuffConfigPanel.Setup();
		FinishAction();
	}
}
