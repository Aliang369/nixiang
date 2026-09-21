public class PlayAvatarDialogueData : Effect
{
	public AvatarTalkData data;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new EnumeratorAction(() => eve.gameState.UIManager.avatarDialoguePanel.PlayCurrentDialogueSectionCor(data.section)));
		return NodeCastResult.Success;
	}
}
