using UnityEngine;

public class ChangeAdditionalWillPowerCostAction : ExecuteAction
{
	public int delta;

	public ChangeAdditionalWillPowerCostAction(int delta)
	{
		this.delta = delta;
	}

	protected override void Update()
	{
		if (delta == 0)
		{
			return;
		}
		PlayerGameStateInfo playerState = gameState.playerState;
		int num = Mathf.Max(playerState.additionalWillPowerCost, 0);
		int num2 = Mathf.Max(num + delta, 0);
		int appliedDelta = num2 - num;
		if (appliedDelta != 0)
		{
			playerState.additionalWillPowerCost = num2;
			if (appliedDelta > 0)
			{
				playerState.SyncMissingWillPowerPenaltyInfluences();
			}
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerWillPowerChange(playerState.CurrentUsedWillPower, appliedDelta);
			});
		}
	}
}
