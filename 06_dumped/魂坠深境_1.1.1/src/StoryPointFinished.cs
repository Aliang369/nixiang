public class StoryPointFinished : GlobalTriggerObserverCondition
{
	public StoryPoint storyPoint;

	public override bool Check(ApplicationStateInfo applicationState)
	{
		return storyPoint.IsFinished(applicationState);
	}
}
