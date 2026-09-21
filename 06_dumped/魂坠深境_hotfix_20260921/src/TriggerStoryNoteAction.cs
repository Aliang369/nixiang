public class TriggerStoryNoteAction : GameAction
{
	private StoryNoteInfo noteInfo;

	private bool halt;

	private bool started;

	public TriggerStoryNoteAction(StoryNoteInfo noteInfo, bool halt)
	{
		this.noteInfo = noteInfo;
		this.halt = halt;
		started = false;
	}

	protected override void Update()
	{
		if (!started)
		{
			gameState.UIManager.storyNotePanel.Setup(noteInfo);
			started = true;
		}
		if (halt)
		{
			if (!gameState.UIManager.storyNotePanel.gameObject.activeSelf)
			{
				FinishAction();
			}
		}
		else
		{
			FinishAction();
		}
	}
}
