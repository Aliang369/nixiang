public static class GlobalVariableSetupExtensions
{
	public static bool GetFlag(this GlobalVariableSetup setup)
	{
		return Bootstrap.applicationState.globalVariableManager.GetVariableFlag(setup);
	}
}
