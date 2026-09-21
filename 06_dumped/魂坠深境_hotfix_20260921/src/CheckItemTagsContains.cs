using System.Collections.Generic;
using UnityEngine;

public class CheckItemTagsContains : CheckNode
{
	[SerializeField]
	private ItemTagGenerator pool;

	[SerializeField]
	private ItemTagGenerator target;

	public override bool? Check(GameEvent eve)
	{
		List<ItemTag> list = pool.GenerateItemTags(eve);
		if (list == null)
		{
			return null;
		}
		List<ItemTag> list2 = target.GenerateItemTags(eve);
		if (list2 == null)
		{
			return null;
		}
		foreach (ItemTag item in list2)
		{
			if (!list.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool };
	}
}
