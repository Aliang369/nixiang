using System.Collections.Generic;

public class MultiCondition : GlobalTriggerObserverCondition
{
	public List<GlobalTriggerObserverCondition> conditions;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return conditions.TrueForAll((GlobalTriggerObserverCondition c) => c.Check(applicationState));
	}
}
