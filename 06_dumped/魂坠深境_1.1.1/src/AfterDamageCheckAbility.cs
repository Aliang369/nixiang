using System.Collections.Generic;

public class AfterDamageCheckAbility : Ability
{
	public enum TargetItemTypeCondition
	{
		Any,
		Health,
		Defence
	}

	public GameEvent.DealType dealType;

	public Effect effect;

	public bool fromSpellOnly;

	public bool ignoreOverflow;

	public bool mustWithSubCounter = true;

	public TargetItemTypeCondition targetType;

	public bool withoutOverflow;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			TriggerEffect(eve, effect);
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.type == GameEvent.Type.AfterDamage && CheckTargetType(eve) && eve.CheckDealType(dealType) && CheckSpell(eve) && CheckOverflow(eve) && CheckMustSubCounter(eve) && CheckWithoutOverflow(eve))
		{
			return true;
		}
		return false;
	}

	private bool CheckWithoutOverflow(GameEvent eve)
	{
		if (withoutOverflow && eve.damage.overflowDamage != 0)
		{
			return eve.damage.targetItem == null;
		}
		return true;
	}

	private bool CheckMustSubCounter(GameEvent eve)
	{
		if (mustWithSubCounter)
		{
			return eve.damage.subCounter > 0;
		}
		return true;
	}

	private bool CheckOverflow(GameEvent eve)
	{
		if (ignoreOverflow)
		{
			return !eve.damage.isOverflowDamage;
		}
		return true;
	}

	private bool CheckSpell(GameEvent eve)
	{
		if (fromSpellOnly)
		{
			return eve.damage.IsFromSpell;
		}
		return true;
	}

	private bool CheckTargetType(GameEvent eve)
	{
		switch (targetType)
		{
		case TargetItemTypeCondition.Any:
			return true;
		case TargetItemTypeCondition.Health:
			return eve.damage.targetItem == null;
		case TargetItemTypeCondition.Defence:
			if (eve.damage.targetItem != null)
			{
				return eve.damage.targetItem.Defence;
			}
			return false;
		default:
			return false;
		}
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterDamage;
	}
}
