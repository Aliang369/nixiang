using System.Collections;

public class MainStateManager : InGameManager
{
	[SaveField(null, null)]
	private new GameStateInfo gameState;

	public override void Init(GameStateInfo gameState)
	{
		this.gameState = gameState;
	}

	public override void Dispose()
	{
	}

	public IEnumerator ShowCurrentRoomPanel()
	{
		if (gameState.currentEncounter.triggerEvent.displayType == WorldEventDisplayType.Battle)
		{
			yield return gameState.UIManager.battlePanel.ShowPanel(anim: true);
		}
		else
		{
			yield return gameState.UIManager.currentEventPanel.ShowPanel();
		}
	}

	public IEnumerator HideCurrentRoomPanel()
	{
		if (gameState.currentEncounter.triggerEvent.displayType == WorldEventDisplayType.Battle)
		{
			yield return gameState.UIManager.battlePanel.HidePanel();
		}
		else
		{
			yield return gameState.UIManager.currentEventPanel.HidePanel();
		}
	}
}
