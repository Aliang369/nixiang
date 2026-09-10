using System.Collections.Generic;

public class Enchant : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IAttachment, IBftOwnable
{
	[SaveField(null, null)]
	public bool afterTurnEnd;

	[SaveField(null, null)]
	public Item attachItem;

	[SaveField(null, null)]
	public EnchantDef def;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues;

	[SaveField(null, null)]
	public ProtectedInt value;

	public object AttachTarget => attachItem;

	public BattleFieldTransform OwnerBft => attachItem?.OwnerBft;

	private Enchant()
	{
	}

	public Enchant(EnchantDef enchant, int value = 1)
	{
		def = enchant;
		if (!def.stacking)
		{
			value = 1;
		}
		this.value = value;
		tempValues = new Dictionary<string, object>();
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
		if (AreAfterTriggerChapterEffectsSuppressed(eve) || def.attachAbility == null)
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

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (AreAfterTriggerChapterEffectsSuppressed(eve))
		{
			return false;
		}
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

	private bool AreAfterTriggerChapterEffectsSuppressed(GameEvent eve)
	{
		if (eve != null && eve.type == GameEvent.Type.AfterTriggerChapter && eve.TargetItem == attachItem)
		{
			GameStateInfo gameState = eve.gameState;
			if (gameState == null)
			{
				return false;
			}
			return gameState.actionManager?.AreAfterTriggerChapterEffectsSuppressed(attachItem) == true;
		}
		return false;
	}

	public Enchant CopyToItem(Item item)
	{
		Enchant enchant = MemberwiseClone() as Enchant;
		enchant.tempValues = new Dictionary<string, object>();
		foreach (KeyValuePair<string, object> tempValue in tempValues)
		{
			enchant.tempValues.Add(tempValue.Key, tempValue.Value);
		}
		enchant.attachItem = item;
		return enchant;
	}

	public string GetDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatEnchantDescription(applicationState, this, out generator);
	}
}
