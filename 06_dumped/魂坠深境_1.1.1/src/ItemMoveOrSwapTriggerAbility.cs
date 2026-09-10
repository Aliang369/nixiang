using System.Collections.Generic;

public class ItemMoveOrSwapTriggerAbility : Ability
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
		Item casterItem = eve.CasterItem;
		if (casterItem == null)
		{
			return false;
		}
		if (eve.type == GameEvent.Type.MoveSlotIndex)
		{
			if (eve.TargetItem == casterItem)
			{
				return eve.SourceItem != casterItem;
			}
			return false;
		}
		if (eve.SourceItem != casterItem)
		{
			return eve.TargetItem == casterItem;
		}
		return true;
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
