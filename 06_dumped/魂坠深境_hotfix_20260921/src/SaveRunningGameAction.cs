using System.Collections;
using System.Threading.Tasks;

public class SaveRunningGameAction : AbstractEnumeratorAction
{
	protected override IEnumerator Run()
	{
		yield return null;
		gameState.applicationState.savedGameState = gameState;
		Task saveTask = gameState.applicationState.savingManager.SaveProfileAsync();
		while (!saveTask.IsCompleted)
		{
			yield return null;
		}
		if (saveTask.IsFaulted)
		{
			TraceLog.Exception(saveTask.Exception?.GetBaseException(), "异步写入存档错误", ThinkingAnalyticsExtends.ExceptionType.SaveLoad);
		}
	}
}
