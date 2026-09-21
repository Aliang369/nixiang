using DamageNumbersPro;
using UnityEngine;

public class ChangeBftHealthAction : GameAction
{
	public Damage damage;

	public int delta;

	public object source;

	public BattleFieldTransform targetBFT;

	public bool realHealthOnly;

	public bool showPositiveFeedback;

	public ChangeBftHealthAction(BattleFieldTransform targetBFT, int delta, object source = null, Damage damage = null, bool realHealthOnly = false, bool showPositiveFeedback = false)
	{
		this.targetBFT = targetBFT;
		this.delta = delta;
		this.source = source;
		this.damage = damage;
		this.realHealthOnly = realHealthOnly;
		this.showPositiveFeedback = showPositiveFeedback;
	}

	protected override void Update()
	{
		if (targetBFT == null)
		{
			FinishAction();
			return;
		}
		int num = delta;
		if (!realHealthOnly)
		{
			int num2 = targetBFT.tempHealth;
			if (num2 > 0 && delta < 0)
			{
				int num3 = num2 + delta;
				if (num3 <= 0)
				{
					num = num3;
					targetBFT.tempHealth = 0;
				}
				else
				{
					targetBFT.tempHealth = num3;
					num = 0;
				}
			}
		}
		int num4 = targetBFT.health;
		int a = num4 + num;
		int num5 = targetBFT.maxHealth;
		a = Mathf.Min(a, num5);
		targetBFT.health = a;
		int num6 = Mathf.Max(a, 0) - num4;
		if (targetBFT.isPlayer && damage != null && num6 < 0)
		{
			gameState.recordManager.RecordPlayerRealHealthDamage(-num6);
		}
		if (showPositiveFeedback && num6 > 0)
		{
			EnqueuePositiveFeedback(num6);
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerBftHealthChange(targetBFT, delta, source, damage);
		});
		if (a <= 0)
		{
			if (ShouldResetCharacterTrialPlayerHealth())
			{
				targetBFT.health = num5;
				EnqueuePositiveFeedback(num5);
				FinishAction();
				return;
			}
			targetBFT.health = 0;
			if ((int)targetBFT.extraHealthPoint >= 0)
			{
				EnqueueAction(new DeathblowAction(targetBFT, source, damage));
			}
			EnqueuePlayerDeathCheckIfNeeded();
		}
		FinishAction();
	}

	private void EnqueuePlayerDeathCheckIfNeeded()
	{
		if (targetBFT.isPlayer)
		{
			EnqueueAction(delegate
			{
				gameState.playerState.CheckDeath();
			});
		}
	}

	private bool ShouldResetCharacterTrialPlayerHealth()
	{
		if (targetBFT.isPlayer)
		{
			return gameState.IsCharacterTrialGame;
		}
		return false;
	}

	private void EnqueuePositiveFeedback(int actualDelta)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		EnqueueAction(new PlayHealVFXAction(targetBFT));
		EnqueueAction(new PopupTextAction(GetPositiveFeedbackPivot(), actualDelta, GetHealPopupTextPrefab()));
		EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_heal));
		if (targetBFT.isPlayer)
		{
			EnqueueAction(new ControlCameraAction(ScriptableObjectSingleton<VFXDataPreset>.instance.shakeCameraConfig.healShake));
		}
	}

	private PivotManager.IWithPivot GetPositiveFeedbackPivot()
	{
		if (targetBFT.isPlayer)
		{
			return targetBFT.GetUIPivot(gameState);
		}
		if (targetBFT.isEnemy)
		{
			return targetBFT.GetHitPivot();
		}
		return null;
	}

	private DamageNumberGUI GetHealPopupTextPrefab()
	{
		if (!targetBFT.isPlayer)
		{
			return ScriptableObjectSingleton<VFXDataPreset>.instance.enemyHealPopupTextPrefab;
		}
		return ScriptableObjectSingleton<VFXDataPreset>.instance.playerHealPopupTextPrefab;
	}
}
