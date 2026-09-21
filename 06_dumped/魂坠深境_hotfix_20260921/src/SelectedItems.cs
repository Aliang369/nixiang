using System.Collections.Generic;

public class SelectedItems : ItemTargetSelector
{
	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		if (eve.SelectedItems == null)
		{
			return list;
		}
		list.AddRange(eve.SelectedItems);
		return list;
	}
}
