public class AnalyticsObserverManager : GlobalGameManager
{
	[SaveField(null, null)]
	private new ApplicationStateInfo applicationState;

	[SaveField(null, null)]
	private bool firstGame;

	[SaveField(null, null)]
	private bool firstWin;

	public override void Init(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterGameEnd, _OnGameEnd);
	}

	private void _OnGameEnd(GameEvent.Type type, GameEvent eve)
	{
		bool flag = false;
		if (!firstGame)
		{
			firstGame = true;
			flag = true;
			ThinkingAnalyticsExtends.TrackFortressProgressNode(eve.gameState.applicationState, "完成第一局游戏");
		}
		if (!firstWin && eve.succesGame)
		{
			firstWin = true;
			flag = true;
			ThinkingAnalyticsExtends.TrackFortressProgressNode(eve.gameState.applicationState, "第一局游戏胜利");
		}
		if (flag)
		{
			applicationState.savingManager.SaveProfile();
		}
	}

	public override void Dispose()
	{
		applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterGameEnd, _OnGameEnd);
	}
}
