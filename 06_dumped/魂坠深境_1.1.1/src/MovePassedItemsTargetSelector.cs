using System.Collections.Generic;

public sealed class MovePassedItemsTargetSelector : ItemTargetSelector
{
	public override List<Item> GetTargetItems(GameEvent eve)
	{
		if (!DancerMovePathResolver.TryGetPassedItems(eve, eve?.CasterItem, out var passedItems, out var _, out var _))
		{
			return new List<Item>();
		}
		return passedItems;
	}
}
