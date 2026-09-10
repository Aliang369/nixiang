using System.Collections.Generic;

public class ItemTagConstraint : ItemConstraint
{
	public ItemTag itemTag;

	public override bool? CheckItemAccept(Item item)
	{
		if (itemTag == null)
		{
			return true;
		}
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		return item.HasTag(itemTag);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refTags = new NodeDisplayInfo.Element<List<ItemTag>>(new List<ItemTag> { itemTag })
		};
	}
}
