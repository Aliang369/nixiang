using System.Collections.Generic;

public class DamageResolveAbility : Ability
{
	public GameEvent.DealType dealType;

	public Effect effect;

	public bool fromSpellOnly;

	public bool healthDamgeOnly;

	public bool ignoreOverflow = true;

	public Damage.ResolveType resolveType;

	private const string GazeMaskId = "ca45706f694e86f49aff48ca38a49aab";

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}

	private bool CheckOverflow(GameEvent eve)
	{
		if (ignoreOverflow)
		{
			return eve.damagecommand.lastDamageInfo == null;
		}
		return true;
	}

	private bool CheckSpell(GameEvent eve)
	{
		if (fromSpellOnly)
		{
			return eve.spellInfo != null;
		}
		return true;
	}

	private bool CheckHealth(GameEvent eve)
	{
		if (healthDamgeOnly)
		{
			return eve.TargetItem == null;
		}
		return true;
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		switch (resolveType)
		{
		case Damage.ResolveType.BFT:
			yield return GameEvent.Type.GenerateDamageBFTResolve;
			break;
		case Damage.ResolveType.Pierce:
			yield return GameEvent.Type.GenerateDamagePierceResolve;
			break;
		case Damage.ResolveType.DamageValue:
			yield return GameEvent.Type.GenerateDamageDamageValueResolve;
			break;
		case Damage.ResolveType.DamageTimes:
			yield return GameEvent.Type.GenerateDamageTimesResolve;
			break;
		case Damage.ResolveType.RejectFinalDamage:
			yield return GameEvent.Type.GenerateDamageSubCounterResolve;
			break;
		}
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
		if (eve?.CasterItem?.GetDef()?.id == "ca45706f694e86f49aff48ca38a49aab" && eve.CasterItem.skin != null)
		{
			return false;
		}
		if (eve?.CasterItem?.GetDef()?.id == "ca45706f694e86f49aff48ca38a49aab" && eve.CasterItem.owner?.firstItem != eve.CasterItem)
		{
			return false;
		}
		switch (resolveType)
		{
		case Damage.ResolveType.BFT:
			if (eve.type != GameEvent.Type.GenerateDamageBFTResolve)
			{
				return false;
			}
			break;
		case Damage.ResolveType.Pierce:
			if (eve.type != GameEvent.Type.GenerateDamagePierceResolve)
			{
				return false;
			}
			break;
		case Damage.ResolveType.DamageValue:
			if (eve.type != GameEvent.Type.GenerateDamageDamageValueResolve)
			{
				return false;
			}
			break;
		case Damage.ResolveType.DamageTimes:
			if (eve.type != GameEvent.Type.GenerateDamageTimesResolve)
			{
				return false;
			}
			break;
		case Damage.ResolveType.RejectFinalDamage:
			if (eve.type != GameEvent.Type.GenerateDamageSubCounterResolve)
			{
				return false;
			}
			break;
		}
		if (eve.damageResolveType != resolveType)
		{
			return false;
		}
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.CheckDealType(dealType) && CheckOverflow(eve) && CheckSpell(eve) && CheckHealth(eve))
		{
			return true;
		}
		return false;
	}
}
