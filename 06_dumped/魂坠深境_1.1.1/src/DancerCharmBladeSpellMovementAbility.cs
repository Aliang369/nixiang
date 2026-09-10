using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DancerCharmBladeSpellMovementAbility : GameEventTriggerAbility
{
	public const string AccumulatedDamageKey = "DancerCharmBladeSpellMovementAbility.AccumulatedDamage";

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.MoveSlotIndex;
		yield return GameEvent.Type.AfterSwapItem;
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.BeforeBattleEnd;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve?.CasterSkill == null || eve.gameState == null)
		{
			return false;
		}
		if (eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.BeforeBattleEnd)
		{
			return true;
		}
		if (eve.type != GameEvent.Type.MoveSlotIndex && eve.type != GameEvent.Type.AfterSwapItem)
		{
			return false;
		}
		GameSkill gameSkill = eve?.CasterSkill;
		if (gameSkill != null)
		{
			return DancerMovePathResolver.IsPlayerActiveMoveEvent(eve, gameSkill.OwnerBft);
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		GameSkill casterSkill = eve.CasterSkill;
		if (eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.BeforeBattleEnd)
		{
			casterSkill.tempValues?.Remove("DancerCharmBladeSpellMovementAbility.AccumulatedDamage");
			return;
		}
		int num = ((eve.type == GameEvent.Type.MoveSlotIndex) ? Mathf.Abs(eve.deltaValue) : Mathf.Abs(eve.amount - eve.amount2));
		if (num > 0)
		{
			GameSkill gameSkill = casterSkill;
			if (gameSkill.tempValues == null)
			{
				gameSkill.tempValues = new Dictionary<string, object>();
			}
			int num2 = (casterSkill.tempValues.TryGetValue("DancerCharmBladeSpellMovementAbility.AccumulatedDamage", out var value) ? Convert.ToInt32(value) : 0);
			casterSkill.tempValues["DancerCharmBladeSpellMovementAbility.AccumulatedDamage"] = num2 + num;
			eve.gameState.actionManager.EnqueueAction(new ChangeSkillCooldownAction(casterSkill, -1));
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
