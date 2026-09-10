using System.Collections.Generic;

public class EmptyItemSelector : ItemTargetSelector
{
	public override List<Item> GetTargetItems(GameEvent eve)
	{
		return new List<Item>();
	}
}
