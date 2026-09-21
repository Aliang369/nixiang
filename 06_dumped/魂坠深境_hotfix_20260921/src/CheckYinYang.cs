using UnityEngine;

public class CheckYinYang : CheckNode
{
	[SerializeField]
	private TrigramsManager.YinYangStateType type;

	public override bool? Check(GameEvent eve)
	{
		TrigramsManager trigramsManager = eve.gameState?.trigramsManager;
		if (trigramsManager == null)
		{
			return null;
		}
		TrigramsManager.YinYangStateType currentState = trigramsManager.GetCurrentState();
		return currentState.HasFlag(type);
	}
}
