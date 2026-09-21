public class ClavisActivated : GlobalTriggerObserverCondition
{
	public ClavisDef clavisDef;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return applicationState.clavisManager.IsActivated(clavisDef);
	}
}
