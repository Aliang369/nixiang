using System.Collections.Generic;

public sealed class DancerAscendingPassEnchantAbility : Ability
{
	public ChangeEnchant effect;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.MoveSlotIndex;
		yield return GameEvent.Type.AfterSwapItem;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve) && DancerMovePathResolver.TryGetPassedItems(eve, eve.CasterItem, out var passedItems, out var beforeIndex, out var afterIndex) && afterIndex > beforeIndex)
		{
			return passedItems.Count > 0;
		}
		return false;
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
