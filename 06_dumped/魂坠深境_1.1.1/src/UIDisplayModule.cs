using Common.UI;

public class UIDisplayModule : MonoDisplayModule
{
	public new static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	protected new static FortressStateInfo fortressState => applicationState.fortressState;

	protected new static GameStateInfo gameState => applicationState.runningGameState;
}
