using System.Collections.Generic;

public sealed class PlayerActiveMoveTriggerAbility : Ability
{
	public Effect effect;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.MoveSlotIndex;
		yield return GameEvent.Type.AfterSwapItem;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		return DancerMovePathResolver.IsPlayerActiveMoveEvent(eve, eve.CasterOwner);
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			TriggerEffect(eve, effect);
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}
}
