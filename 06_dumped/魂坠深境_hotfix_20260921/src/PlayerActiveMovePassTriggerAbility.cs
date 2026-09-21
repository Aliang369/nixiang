using System.Collections.Generic;

public sealed class PlayerActiveMovePassTriggerAbility : Ability
{
	public Effect effect;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.MoveSlotIndex;
		yield return GameEvent.Type.AfterSwapItem;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve) || !DancerMovePathResolver.IsPlayerActiveMoveEvent(eve, eve.CasterOwner))
		{
			return false;
		}
		Item casterItem = eve.CasterItem;
		if (casterItem == null || casterItem.owner?.type != ItemContainer.ContainerType.Table)
		{
			return false;
		}
		int afterIndex;
		int beforeIndex;
		if (eve.type == GameEvent.Type.MoveSlotIndex)
		{
			if (eve.TargetItem != casterItem && DancerMovePathResolver.TryGetPassedItems(eve, eve.TargetItem, out var passedItems, out afterIndex, out beforeIndex))
			{
				return passedItems.Contains(casterItem);
			}
			return false;
		}
		if (eve.SourceItem == casterItem || eve.TargetItem == casterItem)
		{
			return false;
		}
		if (!DancerMovePathResolver.TryGetPassedItems(eve, eve.SourceItem, out var passedItems2, out beforeIndex, out afterIndex) || !passedItems2.Contains(casterItem))
		{
			if (DancerMovePathResolver.TryGetPassedItems(eve, eve.TargetItem, out var passedItems3, out afterIndex, out beforeIndex))
			{
				return passedItems3.Contains(casterItem);
			}
			return false;
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
