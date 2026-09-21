using System.Collections.Generic;
using UnityEngine;

public class ItemMajorTagGenerator : ItemTagGenerator
{
	[SerializeField]
	private ItemTargetSelector targetSelector;

	public override List<ItemTag> GenerateItemTags(GameEvent eve)
	{
		Item item = targetSelector?.GetSingleItem(eve);
		if (item == null)
		{
			return new List<ItemTag>();
		}
		return new List<ItemTag> { item.majorTag };
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (targetSelector != null)
		{
			list.Add(targetSelector);
		}
		return list;
	}
}
