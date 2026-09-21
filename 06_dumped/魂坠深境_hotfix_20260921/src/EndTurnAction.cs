public class EndTurnAction : ExecuteAction
{
	public enum Type
	{
		Player,
		Enemy
	}

	public Type type;

	public bool outControlPhaseEffect;

	public EndTurnAction(Type type, bool outControlPhaseEffect = false)
	{
		this.type = type;
		this.outControlPhaseEffect = outControlPhaseEffect;
	}

	protected override void Update()
	{
		switch (type)
		{
		case Type.Player:
			if (outControlPhaseEffect || gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase)
			{
				gameState.battleState.endPlayerTurnFlag = true;
			}
			break;
		case Type.Enemy:
			if (outControlPhaseEffect || gameState.battleState.turnPhase == TurnPhase.EnemyMovePhase)
			{
				gameState.battleState.endEnemyTurnFlag = true;
			}
			break;
		}
	}
}
