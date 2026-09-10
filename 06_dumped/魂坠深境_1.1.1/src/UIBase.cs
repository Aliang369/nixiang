using Common.UI;

public class UIBase : Common.UI.UIBase
{
	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public static FortressStateInfo fortressState => applicationState?.fortressState;

	public static GameStateInfo gameState => applicationState?.runningGameState;

	public float uiTimeScale => applicationState?.settingManager.GetUIScale() ?? 1f;

	public TextDataPreset textData => ScriptableObjectSingleton<TextDataPreset>.instance;
}
