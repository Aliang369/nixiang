using System.Collections.Generic;
using UnityEngine;

public class SetTempItems : SetTempKey
{
	[SerializeField]
	private ItemTargetSelector targetSelector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		if (targetItems.IsNullOrEmpty())
		{
			return NodeCastResult.Failure;
		}
		if (!SetNormalTempItems(eve, targetItems))
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	private bool SetNormalTempItems(GameEvent eve, List<Item> items)
	{
		TempValue.GetDictionary(eve, type)[key] = items;
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
