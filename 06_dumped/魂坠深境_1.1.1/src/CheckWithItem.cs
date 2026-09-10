using System.Collections.Generic;
using UnityEngine;

public class CheckWithItem : CheckNode
{
	[SerializeField]
	private ItemTargetSelector targetSelector;

	public override bool? Check(GameEvent eve)
	{
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		if (targetItems.Count == 0)
		{
			return false;
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
