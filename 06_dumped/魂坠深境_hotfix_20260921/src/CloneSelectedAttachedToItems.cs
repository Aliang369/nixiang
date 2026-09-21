using System.Collections.Generic;
using UnityEngine;

public class CloneSelectedAttachedToItems : NewItemGenerator
{
	[SerializeField]
	private ItemTargetSelector selector1;

	[SerializeField]
	private ItemTargetSelector selector2;

	public bool cloneEnchants = true;

	public bool cloneRunes = true;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<Item> list = selector1?.GetTargetItems(eve)?.FindAll((Item it) => it != null);
		List<Item> list2 = selector2?.GetTargetItems(eve)?.FindAll((Item it) => it != null);
		if (list.IsNullOrEmpty() || list2.IsNullOrEmpty())
		{
			return new List<Item>();
		}
		List<Enchant> list3 = new List<Enchant>();
		List<Rune> list4 = new List<Rune>();
		foreach (Item item in list)
		{
			list3.AddRange(item.Enchants);
			list4.AddRange(item.Runes);
		}
		foreach (Item item2 in list2)
		{
			if (cloneEnchants)
			{
				foreach (Enchant item3 in list3)
				{
					item2.AddEnchant(item3);
				}
			}
			if (!cloneRunes)
			{
				continue;
			}
			foreach (Rune item4 in list4)
			{
				item2.AddRune(item4);
			}
		}
		return list2;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selector1, selector2 };
	}
}
