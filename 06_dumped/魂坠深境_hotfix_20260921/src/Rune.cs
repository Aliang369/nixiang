using System.Collections.Generic;

public class Rune : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IAttachment, IBftOwnable
{
	[SaveField(null, null)]
	public Item attachItem;

	[SaveField(null, null)]
	public RuneDef def;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues;

	[SaveField(null, null)]
	public int value;

	public object AttachTarget => attachItem;

	public BattleFieldTransform OwnerBft => attachItem?.OwnerBft;

	public Rune(RuneDef def, int value = 1)
	{
		this.def = def;
		if (!def.stacking)
		{
			value = 1;
		}
		this.value = value;
		tempValues = new Dictionary<string, object>();
	}

	private Rune()
	{
	}

	public List<IEffectNode> GetChildren()
	{
		return ((IChildren<IEffectNode>)def).GetChildren();
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		bool result = false;
		if (def.attachAbility != null)
		{
			foreach (Ability item in def.attachAbility.OrEmptyIfNull())
			{
				if (item.CheckTriggerGameEventAble(eve))
				{
					result = true;
				}
			}
		}
		return result;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		if (def.attachAbility != null)
		{
			foreach (Ability item in def.attachAbility)
			{
				if (item == null)
				{
					continue;
				}
				foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
				{
					yield return item2;
				}
			}
		}
		yield return GameEvent.Type.AfterConsume;
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		if (def.attachAbility == null)
		{
			return;
		}
		foreach (Ability item in def.attachAbility.OrEmptyIfNull())
		{
			if (item.CheckTriggerGameEventAble(eve))
			{
				item.TriggerGameEvent(eve);
			}
		}
	}

	public Rune CopyToItem(Item item)
	{
		Rune rune = MemberwiseClone() as Rune;
		rune.tempValues = new Dictionary<string, object>();
		foreach (KeyValuePair<string, object> tempValue in tempValues)
		{
			rune.tempValues.Add(tempValue.Key, tempValue.Value);
		}
		rune.attachItem = item;
		return rune;
	}

	public string GetNameWithValue()
	{
		return (def.Name + (def.stacking ? value.ToString() : "")).ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(def.quality));
	}

	public string GetDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatRuneDescription(applicationState, this, out generator);
	}
}
