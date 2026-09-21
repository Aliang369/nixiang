using UnityEngine;

public class ChecKTurnPhase : CheckNode
{
	[SerializeField]
	public TurnPhase turn;

	public override bool? Check(GameEvent eve)
	{
		BattleStateInfo battleStateInfo = eve.gameState?.battleState;
		if (battleStateInfo == null)
		{
			return null;
		}
		return battleStateInfo.turnPhase == turn;
	}
}
