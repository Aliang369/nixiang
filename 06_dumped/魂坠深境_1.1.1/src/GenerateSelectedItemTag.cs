using System.Collections.Generic;

public class GenerateSelectedItemTag : ItemTagGenerator
{
	public override List<ItemTag> GenerateItemTags(GameEvent eve)
	{
		List<ItemTag> list = new List<ItemTag>();
		foreach (Item selectedItem in eve.SelectedItems)
		{
			list.AddRange(selectedItem.GetTags());
		}
		return list;
	}
}
