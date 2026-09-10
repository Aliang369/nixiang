public class ClavisUnlocked : GlobalTriggerObserverCondition
{
	public ClavisDef clavisDef;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return applicationState.clavisManager.IsUnlocked(clavisDef);
	}
}
