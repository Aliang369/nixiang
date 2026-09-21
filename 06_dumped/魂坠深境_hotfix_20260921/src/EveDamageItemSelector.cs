using System.Collections.Generic;

public class EveDamageItemSelector : ItemTargetSelector
{
	public override List<Item> GetTargetItems(GameEvent eve)
	{
		if (eve.damage?.targetItem == null)
		{
			return null;
		}
		return new List<Item> { eve.damage.targetItem };
	}
}
