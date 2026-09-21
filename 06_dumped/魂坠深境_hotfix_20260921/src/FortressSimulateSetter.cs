public class FortressSimulateSetter(DebugPanel debugPanel) : DebugPanel.Setter(debugPanel)
{
	private int finishStage = 7;

	public override string name => "外部系统模拟";

	public override void OnSwitchGroup()
	{
	}

	private void SetAllDirty()
	{
		if (DebugPanel.Setter.applicationState.fortressState?.uiManager != null)
		{
			DebugPanel.Setter.applicationState.fortressState.uiManager.SetDirty();
		}
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
