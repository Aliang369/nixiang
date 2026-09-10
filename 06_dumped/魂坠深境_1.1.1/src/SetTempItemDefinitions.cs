using System;
using System.Collections.Generic;
using UnityEngine;

public class SetTempItemDefinitions : SetTempKey
{
	private enum Op
	{
		Set,
		Add,
		Remove,
		RemoveAll
	}

	[SerializeField]
	private Op @operator;

	[SerializeField]
	private ItemDefGenerator defGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<ItemDefinition> itemDefs = new List<ItemDefinition>();
		if (@operator != Op.RemoveAll)
		{
			itemDefs = defGenerator.GenerateItemDefs(eve);
		}
		if (type == TempValue.Type.GameGlobal)
		{
			throw new Exception("Global itemDefinition is not supported.");
		}
		if (!SetNormalTempItemDefinitions(eve, itemDefs))
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	private bool SetNormalTempItemDefinitions(GameEvent eve, List<ItemDefinition> itemDefs)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		List<ItemDefinition> list = dictionary.TryGetValue(key) as List<ItemDefinition>;
		switch (@operator)
		{
		case Op.Set:
			dictionary[key] = itemDefs;
			break;
		case Op.Add:
			if (list != null)
			{
				itemDefs.AddRange(list);
			}
			dictionary[key] = itemDefs;
			break;
		case Op.Remove:
			if (list == null)
			{
				dictionary[key] = new List<ItemDefinition>();
				break;
			}
			foreach (ItemDefinition itemDef in itemDefs)
			{
				list.Remove(itemDef);
			}
			dictionary[key] = list;
			break;
		case Op.RemoveAll:
			dictionary[key] = new List<ItemDefinition>();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { defGenerator };
	}
}
