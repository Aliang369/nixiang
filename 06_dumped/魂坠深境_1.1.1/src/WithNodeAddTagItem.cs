using System.Collections.Generic;

public class WithNodeAddTagItem : ItemConstraint
{
	public ItemTag tag;

	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		bool value = false;
		List<SpawnItems> descendantNodeOfType = item.GetDescendantNodeOfType<SpawnItems>();
		GameEvent eve = new GameEvent(GameEvent.Type.Display, GameEvent.TriggerType.OutBattleDisplay)
		{
			CasterItem = item
		};
		foreach (SpawnItems item2 in descendantNodeOfType)
		{
			foreach (AddSpawnItemsInfo.AddSingleItemInfo item3 in item2.GetNodeDisplayInfo(eve).addItemInfo.value.itemInfoEachTime)
			{
				if (item3.item.HasTag(tag))
				{
					value = true;
				}
			}
		}
		return value;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refTags = new NodeDisplayInfo.Element<List<ItemTag>>(new List<ItemTag> { tag })
		};
	}
}
