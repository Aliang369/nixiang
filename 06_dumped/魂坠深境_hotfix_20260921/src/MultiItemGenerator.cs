using System.Collections.Generic;
using UnityEngine;

public class MultiItemGenerator : NewItemGenerator
{
	[SerializeField]
	private List<NewItemGenerator> generators;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		foreach (NewItemGenerator generator in generators)
		{
			List<Item> list2 = generator.GenerateNewItems(eve);
			if (list2 == null)
			{
				return null;
			}
			list.AddRange(list2);
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (generators != null)
		{
			list.AddRange(generators);
		}
		return list;
	}
}
