using System.Collections.Generic;
using UnityEngine;

public class CloneItems : NewItemGenerator
{
	[SerializeField]
	private ItemTargetSelector selector;

	public bool resetCoolDown = true;

	public bool removeEnchant;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<Item> list = selector?.GetTargetItems(eve)?.FindAll((Item it) => it != null);
		if (list.IsNullOrEmpty())
		{
			return new List<Item>();
		}
		List<Item> list2 = list.ConvertAll((Item i) => i.Copy());
		if (resetCoolDown)
		{
			list2.ForEach(delegate(Item i)
			{
				i.cooldown = 0;
			});
		}
		if (removeEnchant)
		{
			list2.ForEach(delegate(Item i)
			{
				i.RemoveAllEnchants();
			});
		}
		list2.ForEach(delegate(Item i)
		{
			i.sourceDeckItem = null;
		});
		return list2;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector };
	}
}
