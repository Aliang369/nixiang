using UnityEngine;

public class ChangeCrystalAction : ExecuteAction
{
	public enum ChangeReason
	{
		Other,
		Forge
	}

	public int delta;

	public ChangeReason reason;

	public ChangeCrystalAction(int delta, ChangeReason reason = ChangeReason.Other)
	{
		this.delta = delta;
		this.reason = reason;
	}

	protected override void Update()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!gameState.gameEventManager.TriggerCrystalChangeResolve(delta).reject)
		{
			int num = gameState.playerState.crystalCount;
			int num2 = Mathf.Clamp(num + delta, 0, 999);
			gameState.playerState.crystalCount = num2;
			delta = num2 - num;
			if (delta < 0 && reason == ChangeReason.Forge)
			{
				gameState.globalVariableStateInfo.SetVariableValue(InGameGlobalVariableStateInfo.BonusCrystalPerLocationUntilSpend, 0);
			}
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_resources_crystal_change));
			gameState.gameEventManager.TriggerCrystalChange(num2, delta);
		}
	}
}
