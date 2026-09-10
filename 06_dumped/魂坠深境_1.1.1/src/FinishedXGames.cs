public class FinishedXGames : GlobalTriggerObserverCondition
{
	public int count;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return applicationState.gameStatisticsManager.gameCount >= count;
	}
}
