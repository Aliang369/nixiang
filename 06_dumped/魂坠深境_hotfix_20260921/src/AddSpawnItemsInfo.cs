using System.Collections.Generic;
using UnityEngine;

public class AddSpawnItemsInfo
{
	public struct AddSingleItemInfo(Item item, int deltaCounter = 0)
	{
		public Item item = item;

		public int deltaCounter = deltaCounter;

		public Item GenerateFinalItem(AddSpawnItemsInfo addInfo)
		{
			Item item = this.item.Copy();
			int finalCounter = GetFinalCounter(addInfo);
			item.counter = finalCounter;
			if (finalCounter >= 0)
			{
				item.resetInitCounter = finalCounter;
				item.blackboard?.SetValue(ItemBlackBoard.ValueType.InitCounter, finalCounter);
			}
			return item;
		}

		public int GetFinalCounter(AddSpawnItemsInfo addInfo)
		{
			ProtectedInt counter = item.counter;
			if (item.InvalidCounter)
			{
				return -1;
			}
			if (item.UsesDefenceCounterLimit)
			{
				long num = (long)(int)counter + (long)deltaCounter + addInfo.deltaCounter;
				return CombatValueLimit.CeilToIntWithUpperLimit((100f + (float)addInfo.deltaPercentCounter) / 100f * (float)num);
			}
			counter = (int)counter + deltaCounter;
			counter = (int)counter + addInfo.deltaCounter;
			return Mathf.CeilToInt((100f + (float)addInfo.deltaPercentCounter) / 100f * (float)(int)counter);
		}

		public int GetFinalDeltaCounter(AddSpawnItemsInfo addInfo)
		{
			return GetFinalCounter(addInfo) - (int)item.counter;
		}
	}

	public int deltaCounter;

	public int deltaPercentCounter;

	public List<AddSingleItemInfo> itemInfoEachTime;

	public int repeatTimes;

	public Item sourceItem;

	public SpellCastInfo sourceSpellInfo;

	public List<AddSingleItemInfo> GetAddItemInfos()
	{
		if (itemInfoEachTime == null)
		{
			return null;
		}
		List<AddSingleItemInfo> list = new List<AddSingleItemInfo>();
		for (int i = 0; i < repeatTimes; i++)
		{
			foreach (AddSingleItemInfo item in itemInfoEachTime)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<Item> GenerateFinalItems()
	{
		return GetAddItemInfos()?.ConvertAll((AddSingleItemInfo i) => i.GenerateFinalItem(this));
	}
}
