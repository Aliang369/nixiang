using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyEscapeAction : ExecuteAction
{
	private CancellationTokenSource cts;

	private Task task;

	protected override void Update()
	{
		gameState.UIManager.StartCoroutine(Escape());
	}

	private IEnumerator Escape()
	{
		yield return new WaitUntil(() => gameState.actionManager.isEmpty);
		gameState.gameEventManager.TriggerBeforeEnemyEscape();
		EnqueueAction(new FinishBattleEncounterAction(FinishBattleEncounterAction.FinishType.Escape));
	}
}
