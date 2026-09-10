using UnityEngine;

public class ChangeBftMaxHealthAction : ExecuteAction
{
	public bool baseAdd;

	public BattleFieldTransform bft;

	public int delta;

	public bool hideNfx;

	public ChangeBftMaxHealthAction(BattleFieldTransform bft, int delta, bool baseAdd, bool hideNfx = false)
	{
		this.bft = bft;
		this.delta = delta;
		this.baseAdd = baseAdd;
		this.hideNfx = hideNfx;
	}

	protected override void Update()
	{
		int num = bft.maxHealth;
		int appliedDelta = (bft.isPlayer ? Mathf.Max(delta, 1 - num) : delta);
		if (gameState.gameEventManager.TriggerBftMaxHealthChangeResolve(bft, appliedDelta, null).reject)
		{
			return;
		}
		int num2 = num + appliedDelta;
		bft.maxHealth = num2;
		int num3 = bft.health;
		if (appliedDelta > 0 && baseAdd)
		{
			num3 += appliedDelta;
			gameState.gameEventManager.TriggerBftHealthChange(bft, appliedDelta, null, null);
		}
		num3 = Mathf.Clamp(num3, 0, num2);
		bft.health = num3;
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerBftMaxHealthChange(bft, appliedDelta, null);
		});
		if (bft.isPlayer && num3 <= 0)
		{
			EnqueueAction(delegate
			{
				gameState.playerState.CheckDeath();
			});
		}
		if (appliedDelta > 0 && !hideNfx)
		{
			EnqueueAction(new PlayHealVFXAction(bft));
			EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.addMaxHealthShake));
		}
	}
}
