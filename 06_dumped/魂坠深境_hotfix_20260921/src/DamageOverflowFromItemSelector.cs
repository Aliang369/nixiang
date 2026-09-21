using System.Collections.Generic;

public class DamageOverflowFromItemSelector : ItemTargetSelector
{
	public override List<Item> GetTargetItems(GameEvent eve)
	{
		Item item = eve.damagecommand?.lastDamageInfo?.lastDamageItem;
		if (item != null)
		{
			return new List<Item> { item };
		}
		return new List<Item>();
	}
}
