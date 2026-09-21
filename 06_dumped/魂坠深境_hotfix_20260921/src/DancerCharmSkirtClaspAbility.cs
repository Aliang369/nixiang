using System.Collections.Generic;

public sealed class DancerCharmSkirtClaspAbility : Ability
{
	public ValueGenerator armorValueGenerator;

	public int strength;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.SlotIndexChange;
		yield return GameEvent.Type.PlayerTurnEnd;
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.BeforeBattleEnd;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve) || eve?.CasterSkill == null)
		{
			return false;
		}
		if (eve.type == GameEvent.Type.PlayerTurnEnd || eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.BeforeBattleEnd)
		{
			return true;
		}
		if (eve.type != GameEvent.Type.SlotIndexChange || eve.deltaValue == 0)
		{
			return false;
		}
		Item recordedTarget = GetRecordedTarget(eve.CasterSkill);
		if (recordedTarget != null && eve.TargetItem == recordedTarget)
		{
			BattleFieldTransform ownerBft = recordedTarget.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				ItemContainer owner = recordedTarget.owner;
				if (owner != null && owner.type == ItemContainer.ContainerType.Table)
				{
					return recordedTarget.ownerSlotIndex.HasValue;
				}
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
		GameSkill casterSkill = eve.CasterSkill;
		if (eve.type == GameEvent.Type.PlayerTurnEnd || eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.BeforeBattleEnd)
		{
			casterSkill.tempValues?.Remove("DancerCharmSkirtClasp.TargetArmor");
			return;
		}
		Item recordedTarget = GetRecordedTarget(casterSkill);
		int? num = armorValueGenerator?.GenerateValue(eve);
		if (!num.HasValue)
		{
			int skillPropertyValue = casterSkill.GetSkillPropertyValue(SkillDef.SkillPropertyType.Strength);
			num = ((skillPropertyValue != 0) ? skillPropertyValue : strength);
		}
		if (recordedTarget != null && num.Value > 0)
		{
			int counter = (recordedTarget.UsesDefenceCounterLimit ? CombatValueLimit.Clamp((long)(int)recordedTarget.resetInitCounter + (long)num.Value) : ((int)recordedTarget.resetInitCounter + num.Value));
			eve.gameState.actionManager.EnqueueAction(new ChangeResetInitCounterAction(counter, recordedTarget));
			eve.gameState.actionManager.EnqueueAction(new ChangeCounterAction(num.Value, recordedTarget, casterSkill, withPopupText: true));
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, armorValueGenerator };
	}

	private static Item GetRecordedTarget(GameSkill skill)
	{
		if (skill?.tempValues == null || !skill.tempValues.TryGetValue("DancerCharmSkirtClasp.TargetArmor", out var value))
		{
			return null;
		}
		return value as Item;
	}
}
