using UnityEngine;

public class TriggerEnemyAnimation : Effect
{
	[SerializeField]
	private EnemyAnimationType animationType;

	[SerializeField]
	private CheckNode waitForEndCheckNode;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		bool waitForCompletion = waitForEndCheckNode?.Check(eve).Value ?? false;
		BattleFieldTransform bft = eve.gameState.battleState.enemy.bft;
		eve.gameState.actionManager.EnqueueAction(new PlayPerformanceAction(eve.CasterItem, bft, animationType, waitForCompletion));
		return NodeCastResult.Success;
	}
}
