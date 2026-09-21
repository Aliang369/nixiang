public class ShowMultiInfoAction : GameAction
{
	private readonly MultiInfoDisplayPanel.SetupInfo setupInfo;

	private bool halt;

	private bool started;

	public ShowMultiInfoAction(MultiInfoDisplayPanel.SetupInfo setupInfo, bool halt = false)
	{
		this.setupInfo = setupInfo;
		this.halt = halt;
	}

	protected override void Update()
	{
		if (!started)
		{
			gameState.UIManager.multiInfoDisplayPanel.Setup(setupInfo);
			started = true;
		}
		if (!halt)
		{
			FinishAction();
		}
		else if (!gameState.UIManager.multiInfoDisplayPanel.gameObject.activeSelf)
		{
			FinishAction();
		}
	}
}
