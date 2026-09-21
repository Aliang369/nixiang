using System.Collections.Generic;
using UnityEngine;

public class CheckSameItem : CheckNode
{
	[SerializeField]
	private ItemTargetSelector a;

	[SerializeField]
	private ItemTargetSelector b;

	public override bool? Check(GameEvent eve)
	{
		List<Item> list = a?.GetTargetItems(eve);
		List<Item> list2 = b?.GetTargetItems(eve);
		if (list == null || list2 == null)
		{
			return null;
		}
		if (list.Count != list2.Count)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != list2[i])
			{
				return false;
			}
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { a, b };
	}
}
