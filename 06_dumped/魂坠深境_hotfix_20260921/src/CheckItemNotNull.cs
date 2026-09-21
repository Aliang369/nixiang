using System.Collections.Generic;
using UnityEngine;

public class CheckItemNotNull : CheckNode
{
	[SerializeField]
	private ItemTargetSelector target;

	public override bool? Check(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return false;
		}
		if (targetItems.Count == 0 || targetItems.Contains(null))
		{
			return false;
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target };
	}
}
