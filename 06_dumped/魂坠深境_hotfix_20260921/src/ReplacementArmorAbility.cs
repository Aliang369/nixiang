using System.Collections.Generic;

public class ReplacementArmorAbility : Ability
{
	public bool treatAsDefence = true;

	public bool breakItemWhenArmorDepleted;

	public int GetReplacementArmor(Item item, Enchant enchant)
	{
		if (enchant == null)
		{
			return 0;
		}
		if ((int)enchant.value <= 0)
		{
			return 0;
		}
		return enchant.value;
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

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode };
	}
}
