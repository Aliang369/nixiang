using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Influence : IGameEventSubscriber, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IBftOwnable, PivotManager.IWithPivot
{
	[SaveField(null, null)]
	public InfluenceDef def;

	[SaveField(null, null)]
	public int influenceValue;

	[SaveField(null, null)]
	public BattleFieldTransform owner;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues;

	[SaveField(null, null)]
	public int willPowerCost;

	public BattleFieldTransform OwnerBft => owner;

	public Influence(InfluenceDef def)
	{
		this.def = def;
		influenceValue = def.initValue;
		tempValues = new Dictionary<string, object>();
	}

	private Influence()
	{
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		return ((IChildren<IEffectNode>)def).GetChildren();
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		return def.abilities.SelectMany((Ability ability) => ability.GetAllSubscribeGameEventType());
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability ability in def.abilities)
		{
			ability.TriggerGameEvent(eve);
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return def.abilities.Any((Ability ability) => ability.CheckTriggerGameEventAble(eve));
	}

	public ConvertablePivot GetPivot(GameStateInfo gameState)
	{
		Transform transform = InfluenceDisplayArea.current.FirstOrDefault()?.GetInfluenceDisplay(this)?.transform;
		if (transform == null)
		{
			return null;
		}
		return new ConvertablePivot(transform, gameState.UIManager.mainCanvas);
	}

	public string GetInfluenceDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatInfluenceDescription(this, applicationState, out generator);
	}
}
