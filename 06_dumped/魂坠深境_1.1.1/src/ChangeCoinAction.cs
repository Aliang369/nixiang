using UnityEngine;

public class ChangeCoinAction : ExecuteAction
{
	public enum ChangeReason
	{
		Other,
		Store
	}

	public int delta;

	public ChangeReason reason;

	public ChangeCoinAction(int delta, ChangeReason reason = ChangeReason.Other)
	{
		this.delta = delta;
		this.reason = reason;
	}

	protected override void Update()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerCoinChangeResolve(delta);
		if (!resolveInfo.reject)
		{
			delta += resolveInfo.deltaValue;
			int variableValue = gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.CoinLowerBound);
			int num = gameState.playerState.coinCount;
			int newCoinCount = Mathf.Clamp(num + delta, variableValue, 999);
			gameState.playerState.coinCount = newCoinCount;
			delta = newCoinCount - num;
			if (delta < 0 && reason == ChangeReason.Store)
			{
				gameState.globalVariableStateInfo.SetVariableValue(InGameGlobalVariableStateInfo.BonusCoinPerLocationUntilSpend, 0);
			}
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_resources_coin_change));
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerCoinChange(newCoinCount, delta);
			});
		}
	}
}
