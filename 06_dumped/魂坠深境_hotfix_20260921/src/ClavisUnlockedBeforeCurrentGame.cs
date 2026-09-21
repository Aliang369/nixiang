public class ClavisUnlockedBeforeCurrentGame : GlobalTriggerObserverCondition
{
	public ClavisDef clavisDef;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return applicationState.clavisManager.IsUnlockedBeforeCurrentGame(clavisDef);
	}
}
