public class ChangeTempHealthAction : ExecuteAction
{
	public BattleFieldTransform bft;

	public int delta;

	public bool battleEndCleanup;

	public ChangeTempHealthAction(int delta, BattleFieldTransform bft, bool battleEndCleanup = false)
	{
		this.delta = delta;
		this.bft = bft;
		this.battleEndCleanup = battleEndCleanup;
	}

	public static ChangeTempHealthAction CreateBattleEndCleanup(BattleFieldTransform bft)
	{
		return new ChangeTempHealthAction(0, bft, battleEndCleanup: true);
	}

	protected override void Update()
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if (bft == null)
		{
			return;
		}
		if (battleEndCleanup)
		{
			delta = (((int)bft.tempHealth > 0) ? (-(int)bft.tempHealth) : 0);
		}
		else
		{
			GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerChangeTempHealthResolve(delta, bft);
			if (resolveInfo.deltaValue != 0)
			{
				delta = resolveInfo.deltaValue;
			}
			if (resolveInfo.reject)
			{
				FinishAction();
				return;
			}
		}
		int requestDelta = delta;
		int num = bft.tempHealth;
		int num2 = num + delta;
		if (num2 < 0)
		{
			num2 = 0;
		}
		bft.tempHealth = num2;
		int actualDelta = num2 - num;
		if (delta > 0)
		{
			EnqueueAction(new PlayHealVFXAction(bft));
			PopupText();
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_heal));
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerBftHealthChange(bft, actualDelta, null, null, tempHealthChange: true, requestDelta, battleEndCleanup);
		});
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
		EnqueueAction(new PopupTextAction(pivot, delta, ScriptableObjectSingleton<VFXDataPreset>.instance.tempHealthPopupTextPrefab));
	}
}
