using Common.UI;

public abstract class CellUI<TSetupData> : CellUIBase<TSetupData>
{
	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public static FortressStateInfo fortressState => applicationState?.fortressState;

	public static GameStateInfo gameState => applicationState?.runningGameState;

	public static TextDataPreset textData => ScriptableObjectSingleton<TextDataPreset>.instance;
}
