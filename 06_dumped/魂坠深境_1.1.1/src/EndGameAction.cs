public class EndGameAction : ExecuteAction
{
	public enum EndGameType
	{
		Win,
		Death,
		GiveUp
	}

	private readonly EndGameType endGameType;

	private readonly LastGameEndReason? endReasonOverride;

	public EndGameAction(EndGameType endGameType, LastGameEndReason? endReasonOverride = null)
	{
		this.endGameType = endGameType;
		this.endReasonOverride = endReasonOverride;
	}

	protected override void Update()
	{
		bool success = endGameType == EndGameType.Win;
		if (gameState.IsCharacterTrialGame)
		{
			gameState.LoadToFortressAfterCharacterTrial(success);
			return;
		}
		if (gameState.IsIntroGame)
		{
			gameState.recordManager.GenerateGameRecord(success, finished: true);
			return;
		}
		Bootstrap.instance.audioManager.StopSustained("BgmTag");
		if (endGameType == EndGameType.Death && gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableForerunnerLegacy) && gameState.contractState.selectedContract.IsFullLongGame)
		{
			gameState.gameEventManager.TriggerShowForerunner();
			EnqueueAction(new ActionAction(delegate
			{
				gameState.UIManager.forerunnerPanel.ShowForeRunnerPanel();
			}));
		}
		EnqueueAction(new ActionAction(delegate
		{
			gameState.EndGame(success, finished: true, endReasonOverride);
		}));
	}
}
