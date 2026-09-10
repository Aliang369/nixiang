public class FortressSetter : DebugPanel.Setter
{
	public override string name => "外部系统";

	public FortressSetter(DebugPanel debugPanel)
		: base(debugPanel)
	{
	}

	public override void OnSwitchGroup()
	{
	}
}
