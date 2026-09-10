public class FinishDifficulty : GlobalTriggerObserverCondition
{
	public DifficultyLevel level;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return applicationState.difficultyManager.IsDifficultyFinished(level);
	}
}
