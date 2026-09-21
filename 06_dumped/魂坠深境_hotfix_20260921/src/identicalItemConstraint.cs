using System;
using System.Collections.Generic;

public class identicalItemConstraint : ItemConstraint
{
	public ItemDefinition item;

	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		return item.GetDef() == this.item;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		if (item == null)
		{
			throw new NullReferenceException();
		}
		return new NodeDisplayInfo
		{
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(new List<ItemDefinition> { item })
		};
	}
}
