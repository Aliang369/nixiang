using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillGemWordAbility : Ability
{
	public enum EffectType
	{
		RejectPermanentExileOnce,
		DiscardRandomEnemyItemOnDurabilityClean,
		IncreaseMovedDefenceCounterPercent,
		RejectCombatExileOnce,
		IncreaseFinalCastDamageOnDurabilityClean,
		IncreaseNextCastDamageAfterLastTurnHealthDamage,
		PermanentlyIncreaseDeckDefenceCounterOnKill,
		GainTempHealthAtLowHealthOnTurnStart
	}

	public EffectType effectType;

	public int valuePerStack;

	private string StateKey => string.Format("{0}.{1}", "SkillGemWordAbility", effectType);

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int num = eve.CasterSkillWord?.stack ?? 1;
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(valuePerStack * num)
		};
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BattleStart;
		switch (effectType)
		{
		case EffectType.RejectPermanentExileOnce:
		case EffectType.RejectCombatExileOnce:
			yield return GameEvent.Type.ExileResolveInfo;
			break;
		case EffectType.DiscardRandomEnemyItemOnDurabilityClean:
			yield return GameEvent.Type.SkillDurabilityClean;
			break;
		case EffectType.IncreaseMovedDefenceCounterPercent:
			yield return GameEvent.Type.MoveSlotIndex;
			break;
		case EffectType.IncreaseFinalCastDamageOnDurabilityClean:
			yield return GameEvent.Type.GenerateDamageDamageValueResolve;
			break;
		case EffectType.IncreaseNextCastDamageAfterLastTurnHealthDamage:
			yield return GameEvent.Type.AfterSkillCast;
			yield return GameEvent.Type.GenerateDamageDamageValueResolve;
			break;
		case EffectType.PermanentlyIncreaseDeckDefenceCounterOnKill:
			yield return GameEvent.Type.AfterEnemyDefeated;
			break;
		case EffectType.GainTempHealthAtLowHealthOnTurnStart:
			yield return GameEvent.Type.PlayerTurnStart;
			break;
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.type == GameEvent.Type.BattleStart)
		{
			return true;
		}
		if (!(eve.CasterAttachObject is GameSkill gameSkill))
		{
			return false;
		}
		int result2;
		int result;
		switch (effectType)
		{
		case EffectType.RejectPermanentExileOnce:
			return eve.type == GameEvent.Type.ExileResolveInfo && eve.exileType == ExileItemAction.ExileType.Permanent && !eve.resolveInfo.reject && !GetFlag(gameSkill, StateKey);
		case EffectType.RejectCombatExileOnce:
			return eve.type == GameEvent.Type.ExileResolveInfo && eve.exileType == ExileItemAction.ExileType.Combat && !eve.resolveInfo.reject && !GetFlag(gameSkill, StateKey);
		case EffectType.DiscardRandomEnemyItemOnDurabilityClean:
			return eve.type == GameEvent.Type.SkillDurabilityClean && eve.TargetSkill == gameSkill;
		case EffectType.IncreaseMovedDefenceCounterPercent:
			return eve.type == GameEvent.Type.MoveSlotIndex && eve.GetEveObj(GameEvent.EveObjType.Source) == gameSkill && (eve.TargetItem?.Defence ?? false);
		case EffectType.IncreaseFinalCastDamageOnDurabilityClean:
			return IsDamageFromSkill(eve, gameSkill) && gameSkill.durability == 1;
		case EffectType.IncreaseNextCastDamageAfterLastTurnHealthDamage:
			return CheckRevenge(eve, gameSkill);
		case EffectType.PermanentlyIncreaseDeckDefenceCounterOnKill:
			if (eve.type == GameEvent.Type.AfterEnemyDefeated && eve.GetEveObj(GameEvent.EveObjType.Source) == gameSkill)
			{
				BattleFieldTransform targetBft = eve.TargetBft;
				if (targetBft != null && targetBft.isEnemy)
				{
					Damage damage = eve.damage;
					result2 = ((damage != null && damage.subCounter > 0) ? 1 : 0);
					goto IL_018a;
				}
			}
			result2 = 0;
			goto IL_018a;
		case EffectType.GainTempHealthAtLowHealthOnTurnStart:
			if (eve.type == GameEvent.Type.PlayerTurnStart)
			{
				BattleFieldTransform ownerBft = gameSkill.OwnerBft;
				if (ownerBft != null && ownerBft.isPlayer)
				{
					result = (((int)ownerBft.health * 2 <= (int)ownerBft.maxHealth) ? 1 : 0);
					goto IL_01c9;
				}
			}
			result = 0;
			goto IL_01c9;
		default:
			{
				return false;
			}
			IL_018a:
			return (byte)result2 != 0;
			IL_01c9:
			return (byte)result != 0;
		}
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		GameSkill gameSkill = eve.CasterAttachObject as GameSkill;
		if (eve.type == GameEvent.Type.BattleStart)
		{
			gameSkill?.tempValues.Remove(StateKey);
			gameSkill?.tempValues.Remove(StateKey + ".armed");
		}
		else
		{
			if (!CheckTriggerGameEventAble(eve) || gameSkill == null)
			{
				return;
			}
			int num = valuePerStack * eve.CasterSkillWord.stack;
			switch (effectType)
			{
			case EffectType.RejectPermanentExileOnce:
				SetFlag(gameSkill, StateKey);
				eve.resolveInfo.reject = true;
				eve.gameState.actionManager.EnqueueAction(new ExileItemAction(eve.TargetItem, eve.GetEveObj(GameEvent.EveObjType.Source)));
				break;
			case EffectType.RejectCombatExileOnce:
				SetFlag(gameSkill, StateKey);
				eve.resolveInfo.reject = true;
				eve.gameState.actionManager.EnqueueAction(new ConsumeItemAction(eve.TargetItem, ConsumeItemAction.ConsumeType.Effect));
				break;
			case EffectType.DiscardRandomEnemyItemOnDurabilityClean:
				DiscardRandomEnemyItem(eve);
				break;
			case EffectType.IncreaseMovedDefenceCounterPercent:
			{
				int delta = Mathf.Max(1, CombatValueLimit.TruncateToInt((float)((int)eve.TargetItem.counter * num) / 100f));
				eve.gameState.actionManager.EnqueueAction(new ChangeCounterAction(delta, eve.TargetItem, gameSkill));
				break;
			}
			case EffectType.IncreaseFinalCastDamageOnDurabilityClean:
				eve.resolveInfo.sourceDeltaPercent += num;
				break;
			case EffectType.IncreaseNextCastDamageAfterLastTurnHealthDamage:
				TriggerRevenge(eve, gameSkill, num);
				break;
			case EffectType.PermanentlyIncreaseDeckDefenceCounterOnKill:
			{
				foreach (Item item in from item in gameSkill.OwnerBft.deckContainer.GetAllItems()
					where item.Defence
					select item)
				{
					eve.gameState.actionManager.EnqueueAction(new ChangeCounterAction(num, item, gameSkill));
				}
				break;
			}
			case EffectType.GainTempHealthAtLowHealthOnTurnStart:
				eve.gameState.actionManager.EnqueueAction(new ChangeTempHealthAction(num, gameSkill.OwnerBft));
				break;
			}
		}
	}

	private static bool IsDamageFromSkill(GameEvent eve, GameSkill skill)
	{
		if (eve.type == GameEvent.Type.GenerateDamageDamageValueResolve)
		{
			return eve.damagecommand?.source == skill;
		}
		return false;
	}

	private bool CheckRevenge(GameEvent eve, GameSkill skill)
	{
		if (eve.type == GameEvent.Type.AfterSkillCast)
		{
			if (eve.SourceSkill == skill)
			{
				return GetFlag(skill, StateKey + ".armed");
			}
			return false;
		}
		if (!IsDamageFromSkill(eve, skill) || !HasEnemyHealthDamageLastTurn(eve))
		{
			return false;
		}
		List<Damage> damageLastRound = eve.gameState.recordManager.damageLastRound;
		if (!GetFlag(skill, StateKey + ".armed") && skill.tempValues.TryGetValue(StateKey, out var value))
		{
			return value != damageLastRound;
		}
		return true;
	}

	private void TriggerRevenge(GameEvent eve, GameSkill skill, int value)
	{
		if (eve.type == GameEvent.Type.AfterSkillCast)
		{
			skill.tempValues.Remove(StateKey + ".armed");
			return;
		}
		skill.tempValues[StateKey] = eve.gameState.recordManager.damageLastRound;
		SetFlag(skill, StateKey + ".armed");
		eve.resolveInfo.sourceDeltaPercent += value;
	}

	private static bool HasEnemyHealthDamageLastTurn(GameEvent eve)
	{
		GameStateInfo gameState = eve.gameState;
		if (gameState == null)
		{
			return false;
		}
		return gameState.recordManager?.damageLastRound?.Any(delegate(Damage damage)
		{
			if (damage != null && damage.isHealthDamage)
			{
				BattleFieldTransform targetBFT = damage.targetBFT;
				if (targetBFT != null && targetBFT.isPlayer)
				{
					return damage.SourceOwnerBft?.isEnemy ?? false;
				}
			}
			return false;
		}) == true;
	}

	private static bool GetFlag(GameSkill skill, string key)
	{
		if (skill.tempValues.TryGetValue(key, out var value))
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return false;
		}
		return false;
	}

	private static void SetFlag(GameSkill skill, string key)
	{
		skill.tempValues[key] = true;
	}

	private static void DiscardRandomEnemyItem(GameEvent eve)
	{
		List<Item> list = eve.gameState?.battleState?.EnemyTableContainer?.GetAllItems();
		if (list != null && list.Count != 0)
		{
			int index = Mathf.Min((int)(eve.gameState.randomManager.NextDouble() * (double)list.Count), list.Count - 1);
			eve.gameState.actionManager.EnqueueAction(new ConsumeItemAction(list[index], ConsumeItemAction.ConsumeType.Effect));
		}
	}
}
