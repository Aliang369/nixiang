using System.Collections.Generic;
using UnityEngine;

public class HasItemCheckNode : CheckNode
{
	[SerializeField]
	private ItemTargetSelector target;

	public override bool? Check(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		return targetItems.Count > 0;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target };
	}
}
