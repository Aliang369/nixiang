using Common.UI;

public abstract class UIManager : Common.UI.UIManager
{
	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public static FortressStateInfo fortressState => applicationState?.fortressState;

	public static GameStateInfo gameState => applicationState?.runningGameState;
}
