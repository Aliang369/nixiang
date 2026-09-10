public class PlayStoryPoint : Effect
{
	public StoryPoint.StoryPointNode node;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new EnumeratorAction(() => node.ExecuteCor(eve.gameState.applicationState)));
		return NodeCastResult.Success;
	}
}
