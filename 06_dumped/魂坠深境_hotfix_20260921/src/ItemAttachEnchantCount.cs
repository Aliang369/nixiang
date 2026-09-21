using System.Collections.Generic;

public class ItemAttachEnchantCount : ValueGenerator
{
	public bool negativeOnly;

	public bool outBattleDisplayUnknown;

	public ItemTargetSelector target;

	public override int? GenerateValue(GameEvent eve)
	{
		if (outBattleDisplayUnknown && eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		int num = 0;
		foreach (Item item in targetItems)
		{
			foreach (Enchant enchant in item.Enchants)
			{
				if (!negativeOnly || enchant.def.type == EnchantDef.EnchantType.Negative)
				{
					num += (int)enchant.value;
				}
			}
		}
		return num;
	}
}
