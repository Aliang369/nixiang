using System.Collections.Generic;

public class DialogController
{
	public class DialogModule
	{
		public DialogController dialogController;

		public DialoguePlayer dialoguePlayer => dialogController.dialoguePlayer;

		public ApplicationStateInfo applicationState => dialogController.applicationState;

		public GameStateInfo gameState => applicationState.runningGameState;

		protected bool isPlaying => gameState.UIManager.dialoguePanel.dialogueBubblePool.activeCount != 0;

		public void Init()
		{
			OnInit();
		}

		protected virtual void OnInit()
		{
		}

		public void FrameUpdate()
		{
			OnFrameUpdate();
		}

		protected virtual void OnFrameUpdate()
		{
		}
	}

	public DialoguePlayer dialoguePlayer;

	public ApplicationStateInfo applicationState;

	private List<DialogModule> modules;

	public bool enable;

	public void Init(DialoguePlayer dialoguePlayer, ApplicationStateInfo applicationState)
	{
		this.dialoguePlayer = dialoguePlayer;
		this.applicationState = applicationState;
		modules = new List<DialogModule>();
	}

	public void FrameUpdate()
	{
		if (!enable)
		{
			return;
		}
		foreach (DialogModule module in modules)
		{
			module.FrameUpdate();
		}
	}

	public void AddModule(DialogModule module)
	{
		module.dialogController = this;
		modules.Add(module);
		module.Init();
	}
}
