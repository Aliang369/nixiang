using UnityEngine;

public class SetCoinAction : ExecuteAction
{
	public int value;

	public SetCoinAction(int value)
	{
		this.value = value;
	}

	protected override void Update()
	{
		int finalValue = Mathf.Min(value, 999);
		int num = gameState.playerState.coinCount;
		int delta = finalValue - num;
		gameState.playerState.coinCount = finalValue;
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerCoinChange(finalValue, delta);
		});
	}
}
