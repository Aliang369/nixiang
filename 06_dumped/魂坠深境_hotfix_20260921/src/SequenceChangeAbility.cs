using System.Collections.Generic;
using UnityEngine;

public class SequenceChangeAbility : Ability
{
	public Effect effect;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		return new GameEvent.Type[3]
		{
			GameEvent.Type.AddItem,
			GameEvent.Type.AfterRemoveItem,
			GameEvent.Type.SlotIndexChange
		};
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			Debug.Log(eve.CasterItem?.ItemName);
			TriggerEffect(eve, effect);
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}
}
