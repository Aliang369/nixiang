using System;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCrownSwarmAbility : Ability
{
	public int attackPerCounter = 1;

	[Obsolete("Legacy field kept so old serialized assets do not break.")]
	public int tempHealthPerCounter = 1;

	[NonSerialized]
	private int damageFromLastTurnTempHealth;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.EnemyTurnStart;
		yield return GameEvent.Type.EnemyTurnEnd;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve))
		{
			BattleFieldTransform casterOwner = eve.CasterOwner;
			if (casterOwner != null && casterOwner.isEnemy && !(SpiderContractEnemyAbilityUtility.GetSpiderRule(eve.gameState) == null))
			{
				if (eve.type != GameEvent.Type.EnemyTurnStart)
				{
					return eve.type == GameEvent.Type.EnemyTurnEnd;
				}
				return true;
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		BattleFieldTransform owner = eve.CasterOwner;
		if (eve.type == GameEvent.Type.EnemyTurnStart && damageFromLastTurnTempHealth > 0)
		{
			eve.gameState.actionManager.EnqueueAction(new DamageAction(new DamageCommand
			{
				baseDamage = damageFromLastTurnTempHealth,
				source = eve.Caster,
				targetBFT = eve.gameState.playerState.bft,
				overflow = true
			}));
		}
		if (eve.type == GameEvent.Type.EnemyTurnEnd)
		{
			int nestActivity = SpiderContractEnemyAbilityUtility.GetNestActivity(eve.gameState);
			eve.gameState.actionManager.EnqueueAction(new ChangeTempHealthAction(GetPhase2TempHealth(nestActivity, GetOwner(eve), eve.gameState), owner));
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				damageFromLastTurnTempHealth = owner.tempHealth;
			}));
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int phase2TempHealth = GetPhase2TempHealth(SpiderContractEnemyAbilityUtility.GetNestActivity(eve?.gameState), GetOwner(eve), eve?.gameState);
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(SpiderContractEnemyAbilityUtility.FormatActivityValueWithLevel(phase2TempHealth, eve?.gameState))
		};
	}

	private static int GetPhase2TempHealth(int activity, BattleFieldTransform owner, GameStateInfo gameState)
	{
		CharacterAI characterAI = owner?.characterAI;
		if (characterAI == null)
		{
			return 0;
		}
		int num = characterAI.secondPhaseMaxHealth;
		if (gameState != null && gameState.globalVariableStateInfo?.GetVariableFlag(InGameGlobalVariableStateInfo.ExtraEnemyHealthForPhase2) == true)
		{
			num = Mathf.FloorToInt((float)num * 1.25f);
		}
		int num2 = characterAI.firstPhaseMaxHealth + num;
		return Mathf.FloorToInt((5f + Mathf.Pow(activity, 1.55f)) * (float)num2 / 200f);
	}

	private static BattleFieldTransform GetOwner(GameEvent eve)
	{
		object obj = eve?.CasterOwner;
		if (obj == null)
		{
			CharacterAI obj2 = eve?.CasterAttachObject as CharacterAI;
			if (obj2 == null)
			{
				return null;
			}
			obj = obj2.bft;
		}
		return (BattleFieldTransform)obj;
	}
}
