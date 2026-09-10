using System.Collections.Generic;

public class CloneItemsAttachedToItems : Effect
{
	public ItemTargetSelector itemTargetSelector1;

	public ItemTargetSelector itemTargetSelector2;

	public bool cloneEnchants = true;

	public bool cloneRunes = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = itemTargetSelector1?.GetTargetItems(eve)?.FindAll((Item it) => it != null);
		List<Item> list2 = itemTargetSelector2?.GetTargetItems(eve)?.FindAll((Item it) => it != null);
		if (list.IsNullOrEmpty() || list2.IsNullOrEmpty())
		{
			return NodeCastResult.Failure;
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
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemTargetSelector1, itemTargetSelector2 };
	}
}
