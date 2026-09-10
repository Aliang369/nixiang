public class InGameSetter(DebugPanel debugPanel) : DebugPanel.Setter(debugPanel)
{
	private int coin = 1;

	private int crystal = 1;

	private int deltaHealth = 1;

	private int deltaTempHealth = 1;

	private int progress = 1;

	private int gem = 1;

	public override string name => "局内调试";

	public override void OnSwitchGroup()
	{
	}

	private int TryParse(string str, int defaultValue = 0)
	{
		try
		{
			return int.Parse(str);
		}
		catch
		{
			return defaultValue;
		}
	}
}
