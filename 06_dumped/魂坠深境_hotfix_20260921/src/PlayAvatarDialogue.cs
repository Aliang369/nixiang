public class PlayAvatarDialogue : Effect
{
	public AvatarDialogueSection section;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new EnumeratorAction(() => eve.gameState.UIManager.avatarDialoguePanel.PlayCurrentDialogueSectionCor(section)));
		return NodeCastResult.Success;
	}
}
