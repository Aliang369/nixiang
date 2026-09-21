using System;
using System.Collections.Generic;
using UnityEngine;

public class ChangeItemModifyValue : Effect, IOutGameEffectNode, IEffectCastNode, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public enum ModifyValueType
	{
		DeltaCD = 0,
		MaxCounter = 2,
		DeltaCost = 4,
		Counter = 9,
		RuneSlot = 10,
		UpgradeOnce = 11,
		Treasure = 12
	}

	public bool baseAdd;

	public ValueGenerator deltaValueGenerator;

	public ItemTargetSelector itemSelector;

	public ModifyValueType type;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaValueGenerator, itemSelector };
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = itemSelector?.GetTargetItems(eve);
		int deltaValue = ((deltaValueGenerator != null) ? deltaValueGenerator.GenerateValue(eve).Value : 0);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			ChangeItemValue(item, deltaValue);
		}
		return NodeCastResult.Success;
	}

	private void ChangeItemValue(Item item, int deltaValue)
	{
		switch (type)
		{
		case ModifyValueType.DeltaCD:
			item.deltaCD = (int)item.deltaCD + deltaValue;
			break;
		case ModifyValueType.MaxCounter:
			item.maxCounter = (int)item.maxCounter + deltaValue;
			if (baseAdd)
			{
				item.counter = (int)item.counter + deltaValue;
			}
			break;
		case ModifyValueType.DeltaCost:
			item.deltaCost = (int)item.deltaCost + deltaValue;
			break;
		case ModifyValueType.Counter:
			item.counter = (int)item.counter + deltaValue;
			if ((int)item.maxCounter != 0)
			{
				item.counter = Mathf.Clamp(item.counter, 0, item.maxCounter);
			}
			break;
		case ModifyValueType.RuneSlot:
			item.runeSlot += deltaValue;
			break;
		case ModifyValueType.UpgradeOnce:
			if (deltaValue > 0 && !item.upgraded)
			{
				item.Upgrade();
			}
			if (deltaValue < 0 && item.upgraded)
			{
				item.Downgrade();
			}
			break;
		case ModifyValueType.Treasure:
			item.treasureValue += deltaValue;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
