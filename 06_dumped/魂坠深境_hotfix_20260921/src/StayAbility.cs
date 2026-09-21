using System.Collections.Generic;

public class StayAbility : Ability
{
	public Effect addEffect;

	public Effect removeEffect;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AddItem;
		yield return GameEvent.Type.BeforeRemoveItem;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, addEffect, removeEffect };
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.type == GameEvent.Type.AddItem && eve.CasterItem == eve.TargetItem)
		{
			ItemContainer destinationContainer = eve.DestinationContainer;
			if (destinationContainer != null && destinationContainer.type == ItemContainer.ContainerType.Table)
			{
				return true;
			}
		}
		if (eve.type == GameEvent.Type.BeforeRemoveItem && eve.CasterItem == eve.TargetItem)
		{
			ItemContainer originContainer = eve.OriginContainer;
			if (originContainer != null && originContainer.type == ItemContainer.ContainerType.Table)
			{
				return true;
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
		if (eve.type == GameEvent.Type.AddItem && eve.CasterItem == eve.TargetItem)
		{
			ItemContainer destinationContainer = eve.DestinationContainer;
			if (destinationContainer != null && destinationContainer.type == ItemContainer.ContainerType.Table)
			{
				TriggerEffect(eve, addEffect);
			}
		}
		if (eve.type == GameEvent.Type.BeforeRemoveItem && eve.CasterItem == eve.TargetItem)
		{
			ItemContainer originContainer = eve.OriginContainer;
			if (originContainer != null && originContainer.type == ItemContainer.ContainerType.Table)
			{
				TriggerEffect(eve, removeEffect);
			}
		}
	}
}
