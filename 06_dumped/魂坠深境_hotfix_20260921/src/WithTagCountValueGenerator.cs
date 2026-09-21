using System.Collections.Generic;
using UnityEngine;

public class WithTagCountValueGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventBFTType bft;

	[SerializeField]
	private GameEvent.OwnerItemsType itemsType;

	[SerializeField]
	private ItemTag tag;

	public override int? GenerateValue(GameEvent eve)
	{
		List<Item> eventItems = eve.GetEventItems(bft, itemsType);
		if (eventItems == null)
		{
			return null;
		}
		int num = 0;
		foreach (Item item in eventItems)
		{
			if (item != null && item.HasTag(tag))
			{
				num++;
			}
		}
		return num;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(GenerateValue(eve)),
			refTags = new NodeDisplayInfo.Element<List<ItemTag>>(new List<ItemTag> { tag })
		};
	}
}
