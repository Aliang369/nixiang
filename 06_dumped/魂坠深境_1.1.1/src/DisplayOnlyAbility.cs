using System.Collections.Generic;
using UnityEngine;

public class DisplayOnlyAbility : Ability
{
	[SerializeField]
	private Effect effect;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { effect };
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield break;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return false;
	}
}
