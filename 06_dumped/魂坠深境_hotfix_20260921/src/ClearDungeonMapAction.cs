public class ClearDungeonMapAction : ExecuteAction
{
	protected override void Update()
	{
		gameState.UIManager.dungeonMapPanel.Unset();
		gameState.UIManager.dungeonMapPanel_Preview.Unset();
	}
}
