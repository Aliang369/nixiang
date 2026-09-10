public class LoadLevelAction : GameAction
{
	public Level level;

	private int retryCount = 1;

	private bool started;

	public LoadLevelAction(Level level)
	{
		this.level = level;
	}

	protected override void Update()
	{
		if (gameState.contractState.currentLevel != null && gameState.contractState.currentLevel != level)
		{
			gameState.contractState.currentLevel.Unload(gameState.applicationState.assetCatalog);
		}
		gameState.contractState.currentLevel = level;
		FinishAction();
	}
}
