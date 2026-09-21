using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentInfluence : IGameEventSubscriber, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	[SaveField(null, null)]
	public EnvironmentInfluenceDef def;

	public GameObject spawnEffectInstance;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues;

	public EnvironmentInfluence(EnvironmentInfluenceDef def)
	{
		this.def = def;
		tempValues = new Dictionary<string, object>();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		return def.abilities.SelectMany((Ability ability) => ability.GetAllSubscribeGameEventType());
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return def.abilities.Any((Ability ability) => ability.CheckTriggerGameEventAble(eve));
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability ability in def.abilities)
		{
			ability.TriggerGameEvent(eve);
		}
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public List<IEffectNode> GetChildren()
	{
		return def.abilities.Cast<IEffectNode>().ToList();
	}

	public string GetDescription(GameStateInfo gs, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatEnvironmentInfluenceDescription(gs, this, out generator);
	}
}
