using System.Collections.Generic;

public class PrayAbility : Ability
{
	public List<Ability> prayAbilities;

	public string prayDescription;

	public Effect prayTriggerEffect;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (Ability item in prayAbilities.OrEmptyIfNull())
		{
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability item in prayAbilities.OrEmptyIfNull())
		{
			item.TriggerGameEvent(eve);
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		list.Add(prayTriggerEffect);
		if (!prayAbilities.IsNullOrEmpty())
		{
			list.AddRange(prayAbilities);
		}
		return list;
	}
}
