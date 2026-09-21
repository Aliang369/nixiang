public sealed class BattleRewardContext
{
	public readonly GameStateInfo gameState;

	public readonly FinishBattleEncounterAction.FinishType finishType;

	public BattleRewardContext(GameStateInfo gameState, FinishBattleEncounterAction.FinishType finishType)
	{
		this.gameState = gameState;
		this.finishType = finishType;
	}
}
