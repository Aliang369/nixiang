using System.Collections.Generic;

public class AddItemResolveAbility : Ability
{
	public GameEvent.DealType dealType;

	public Effect effect;

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
		if (eve.type != GameEvent.Type.AddItemCounterResolveInfo)
		{
			return false;
		}
		if (!eve.CheckDealType(dealType))
		{
			return false;
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AddItemCounterResolveInfo;
	}
}
