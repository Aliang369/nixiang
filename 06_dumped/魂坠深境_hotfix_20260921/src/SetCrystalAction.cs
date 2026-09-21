using UnityEngine;

public class SetCrystalAction : ExecuteAction
{
	public int value;

	public SetCrystalAction(int value)
	{
		this.value = value;
	}

	protected override void Update()
	{
		int num = Mathf.Min(value, 999);
		int num2 = gameState.playerState.crystalCount;
		int delta = num - num2;
		gameState.playerState.crystalCount = num;
		gameState.gameEventManager.TriggerCrystalChange(num, delta);
	}
}
