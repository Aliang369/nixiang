using DamageNumbersPro;

public class HealAction : ExecuteAction
{
	public object source;

	public BattleFieldTransform bft;

	public int healAmount;

	public HealAction(object source, BattleFieldTransform bft, int healAmount)
	{
		this.source = source;
		this.bft = bft;
		this.healAmount = healAmount;
	}

	protected override void Update()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerHealResolve(source, bft, healAmount, GameEvent.TriggerType.Real);
		healAmount += resolveInfo.deltaValue;
		if (!resolveInfo.reject)
		{
			EnqueueAction(new ChangeBftHealthAction(bft, healAmount, source));
			EnqueueAction(new PlayHealVFXAction(bft));
			PopupText();
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_heal));
			if (bft.isPlayer)
			{
				EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.healShake));
			}
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerAfterHeal(source, bft, healAmount);
			});
		}
	}

	private void PopupText()
	{
		PivotManager.IWithPivot pivot = null;
		if (bft.isPlayer)
		{
			pivot = bft.GetUIPivot(gameState);
		}
		else if (bft.isEnemy)
		{
			pivot = bft.GetHitPivot();
		}
		DamageNumberGUI damageNumbersPrefab = (bft.isPlayer ? ScriptableObjectSingleton<VFXDataPreset>.instance.playerHealPopupTextPrefab : ScriptableObjectSingleton<VFXDataPreset>.instance.enemyHealPopupTextPrefab);
		EnqueueAction(new PopupTextAction(pivot, healAmount, damageNumbersPrefab));
	}
}
