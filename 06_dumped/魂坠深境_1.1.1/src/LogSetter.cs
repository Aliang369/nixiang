public class LogSetter : DebugPanel.Setter
{
	private const string _name = "Log";

	public override string name => "Log";

	public LogSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	public override void OnSwitchGroup()
	{
	}
}
