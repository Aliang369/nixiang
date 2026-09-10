public class StanceManager : InGameManager
{
	public enum StanceType
	{
		Tiger,
		Crane,
		Snake,
		Leopard,
		Dragon
	}

	[SaveField(null, null)]
	public Stance currentStance;

	[SaveField(null, null)]
	public int stanceMaxValue;

	public bool NeedRepeatTrigger { get; set; }

	public override void Init(GameStateInfo gameState)
	{
		gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, _OnBattleStart);
		gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, _OnBattleBeforeBattleEnd);
	}

	public override void Dispose()
	{
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, _OnBattleStart);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, _OnBattleBeforeBattleEnd);
	}

	private void _OnBattleStart(GameEvent.Type type, GameEvent eve)
	{
		int stanceBaseMaxValue = ScriptableObjectSingleton<GameDataManager>.instance.stanceBaseMaxValue;
		currentStance = null;
		base.gameState.UIManager.stanceDisplay.cachedStance = null;
		base.gameState.actionManager.EnqueueAction(new SetMaxStanceAction(stanceBaseMaxValue));
		base.gameState.UIManager.stanceDisplay.SetDirty();
	}

	private void _OnBattleBeforeBattleEnd(GameEvent.Type type, GameEvent eve)
	{
		currentStance = null;
		base.gameState.UIManager.stanceDisplay.cachedStance = null;
		base.gameState.UIManager.stanceDisplay.SetDirty();
	}
}
