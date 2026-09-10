public abstract class InGameManager : GameManager
{
	public GameUIManager uiManager => base.gameState.UIManager;

	public virtual void Init(GameStateInfo gameState)
	{
	}
}
