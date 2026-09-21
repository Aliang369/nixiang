public static class InGameGlobalVariableStateInfoEx
{
	public static bool GetVariableFlag(this GlobalVariableSetup setup)
	{
		if (Bootstrap.gameState != null)
		{
			return Bootstrap.gameState.globalVariableStateInfo.GetVariableFlag(setup);
		}
		if (Bootstrap.applicationState != null)
		{
			return Bootstrap.applicationState.globalVariableManager.GetVariableFlag(setup);
		}
		return false;
	}
}
