public class PlayCutScene : Effect
{
	public CutsceneRef cutsceneRef;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		PlayCutsceneAction action = new PlayCutsceneAction(cutsceneRef, eve.gameState.applicationState);
		Effect.EnqueueAction(eve, action);
		return NodeCastResult.Success;
	}
}
