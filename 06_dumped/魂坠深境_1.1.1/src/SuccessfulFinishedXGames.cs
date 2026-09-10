public class SuccessfulFinishedXGames : GlobalTriggerObserverCondition
{
	public int count;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return applicationState.recordManager.SuccessfulGameCount >= count;
	}
}
