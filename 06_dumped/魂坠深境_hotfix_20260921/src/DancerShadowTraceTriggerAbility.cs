using System.Collections.Generic;

public sealed class DancerShadowTraceTriggerAbility : GameEventTriggerAbility
{
	private const string TriggeredSwapEventKey = "DancerShadowTraceTriggerAbility.TriggeredSwapEvent";

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve) || eve.CasterEnchant == null)
		{
			return;
		}
		if (eve.type == GameEvent.Type.AfterSwapItem)
		{
			if (!eve.nodeTempValues.TryGetValue("DancerShadowTraceTriggerAbility.TriggeredSwapEvent", out var value))
			{
				value = new HashSet<Enchant>();
				eve.nodeTempValues["DancerShadowTraceTriggerAbility.TriggeredSwapEvent"] = value;
			}
			HashSet<Enchant> hashSet = value as HashSet<Enchant>;
			if (hashSet == null)
			{
				hashSet = new HashSet<Enchant>();
				eve.nodeTempValues["DancerShadowTraceTriggerAbility.TriggeredSwapEvent"] = hashSet;
			}
			if (!hashSet.Add(eve.CasterEnchant))
			{
				return;
			}
		}
		TriggerEffect(eve, effect);
	}
}
