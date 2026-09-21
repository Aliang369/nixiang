using System.Collections.Generic;

public class ItemAttachEnchantDefCount : ValueGenerator
{
	public ItemTargetSelector target;

	public override int? GenerateValue(GameEvent eve)
	{
		HashSet<EnchantDef> hashSet = new HashSet<EnchantDef>();
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		foreach (Item item in targetItems)
		{
			foreach (Enchant enchant in item.Enchants)
			{
				hashSet.Add(enchant.def);
			}
		}
		return hashSet.Count;
	}
}
