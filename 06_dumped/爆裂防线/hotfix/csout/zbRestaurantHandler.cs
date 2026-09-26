using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbRestaurantHandler
{
	private zbRestaurantPieceCsvRecord[] m_records;

	private int m_maxPieceLevel;

	private zbRestaurantBuyCsvRecord[] m_buyRecords;

	public int m_offerSlotIndex = -1;

	public int m_offerTargetIndex = -1;

	private const int UnlockBatchLevel = 5;

	private int m_unlockBatchSlotCount;

	public void Init()
	{
		m_records = zbDataConfig.Instance.restaurantPieceCsvTable.GetRecords().records.Values.ToArray();
		m_maxPieceLevel = m_records.Max((zbRestaurantPieceCsvRecord r) => r.id);
		m_buyRecords = zbDataConfig.Instance.restaurantBuyCsvTable.GetRecords().records.ToArray();
		m_unlockBatchSlotCount = zbDataConfig.Instance.restaurantPieceCsvTable.GetRecordByKey(5).unlockSlot;
	}

	public int GetUnlockLevelBySlotIndex(int index)
	{
		return m_records.FirstOrDefault((zbRestaurantPieceCsvRecord r) => r.unlockSlot >= index + 1)?.id ?? 0;
	}

	private int GetOffByCount(int count)
	{
		zbRestaurantBuyCsvRecord[] buyRecords = m_buyRecords;
		foreach (zbRestaurantBuyCsvRecord zbRestaurantBuyCsvRecord2 in buyRecords)
		{
			if (count <= zbRestaurantBuyCsvRecord2.count)
			{
				return zbRestaurantBuyCsvRecord2.off;
			}
		}
		return 0;
	}

	public (long batchCost, int batchCount) GetBuyBatchCostAndCount(RestaurantDetail restaurantDetail)
	{
		AwardDetail awardDetail = restaurantDetail.SingleCost[0];
		long count = awardDetail.Count;
		long num = zbSelf.Instance.dataHandler.GetResourceCount(awardDetail.Type, (int)awardDetail.Tid);
		int num2 = restaurantDetail.SlotCountMax - restaurantDetail.SlotList.Count;
		foreach (int slot in restaurantDetail.SlotList)
		{
			if (slot == 0)
			{
				num2++;
			}
		}
		int num3 = 0;
		long num4 = 0L;
		int num5 = restaurantDetail.BuyCount;
		while (num3 < num2)
		{
			int offByCount = GetOffByCount(num5);
			long num6 = count * (10 - offByCount) / 10;
			if (num < num6)
			{
				break;
			}
			num -= num6;
			num4 += num6;
			num3++;
			num5++;
		}
		return (batchCost: num4, batchCount: num3);
	}

	public RestaurantDetail GetRestaurantDetail()
	{
		if (zbServerData.sGameData.OperateInfo.RestaurantList != null)
		{
			foreach (RestaurantDetail restaurant in zbServerData.sGameData.OperateInfo.RestaurantList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(restaurant.BaseInfo) && restaurant.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					return restaurant;
				}
			}
		}
		return null;
	}

	public bool CanUseBatch(RestaurantDetail restaurantDetail, bool needTip = false)
	{
		if (restaurantDetail.SlotCountMax >= m_unlockBatchSlotCount)
		{
			return true;
		}
		if (needTip)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11387"), 5));
		}
		return false;
	}

	public bool HasEmptySlot(RestaurantDetail restaurantDetail, bool needTip = true)
	{
		RepeatedField<int> slotList = restaurantDetail.SlotList;
		int slotCountMax = restaurantDetail.SlotCountMax;
		if (slotList.Count == slotCountMax)
		{
			if (((IEnumerable<int>)slotList).Contains(0))
			{
				return true;
			}
			if (needTip)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10281"));
			}
			return false;
		}
		if (slotList.Count < slotCountMax)
		{
			return true;
		}
		if (needTip)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10281"));
		}
		return false;
	}

	public bool HasEnoughCost(RestaurantDetail restaurantDetail, long cost, bool needTip = true)
	{
		bool num = zbSelf.Instance.itemHandler.HasEnoughProp((int)restaurantDetail.SingleCost[0].Tid, cost);
		if (!num & needTip)
		{
			zbUIMgr.Instance.ShowTip(restaurantDetail.SingleCost[0].Type, (int)restaurantDetail.SingleCost[0].Tid, cost);
		}
		return num;
	}

	public (int, int) GetCanMergeFromAndToIndex(RestaurantDetail restaurantDetail, int[] slotList)
	{
		if (restaurantDetail == null || slotList == null)
		{
			return (-1, -1);
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int item in restaurantDetail.TargetDetail.Target)
		{
			if (!dictionary.ContainsKey(item))
			{
				dictionary[item] = 0;
			}
			dictionary[item]++;
		}
		foreach (int offer in restaurantDetail.OfferList)
		{
			if (dictionary.ContainsKey(offer))
			{
				dictionary[offer]--;
			}
		}
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num = Mathf.Min(slotList.Length, restaurantDetail.SlotCountMax);
		for (int i = 0; i < num; i++)
		{
			int num2 = slotList[i];
			if (num2 <= 0 || num2 >= m_maxPieceLevel)
			{
				continue;
			}
			if (dictionary.TryGetValue(num2, out var value) && value > 0)
			{
				dictionary[num2]--;
				continue;
			}
			if (dictionary2.TryGetValue(num2, out var value2))
			{
				return (value2, i);
			}
			dictionary2[num2] = i;
		}
		return (-1, -1);
	}

	public bool CanOffer(RestaurantDetail restaurantDetail, int slotIndex)
	{
		if (restaurantDetail == null)
		{
			return false;
		}
		if (slotIndex < 0 || slotIndex >= restaurantDetail.SlotList.Count)
		{
			return false;
		}
		int num = restaurantDetail.SlotList[slotIndex];
		if (num <= 0)
		{
			return false;
		}
		return CanOfferByPieceLevel(restaurantDetail, ((IEnumerable<int>)restaurantDetail.SlotList).ToArray(), num);
	}

	public bool CanOfferByTargetIndex(RestaurantDetail restaurantDetail, int targetIndex)
	{
		if (restaurantDetail == null)
		{
			return false;
		}
		RepeatedField<int> target = restaurantDetail.TargetDetail.Target;
		if (targetIndex < 0 || targetIndex >= target.Count)
		{
			return false;
		}
		int num = target[targetIndex];
		int num2 = 0;
		foreach (int item in target)
		{
			if (item == num)
			{
				num2++;
			}
		}
		int num3 = 0;
		foreach (int offer in restaurantDetail.OfferList)
		{
			if (offer == num)
			{
				num3++;
			}
		}
		if (num3 >= num2)
		{
			return false;
		}
		int num4 = 0;
		foreach (int slot in restaurantDetail.SlotList)
		{
			if (slot == num)
			{
				num4++;
			}
		}
		return num4 > 0;
	}

	public bool IsOfferedByTargetIndex(RestaurantDetail restaurantDetail, int targetIndex)
	{
		if (restaurantDetail == null)
		{
			return false;
		}
		RepeatedField<int> target = restaurantDetail.TargetDetail.Target;
		if (targetIndex < 0 || targetIndex >= target.Count)
		{
			return false;
		}
		int num = target[targetIndex];
		int num2 = 0;
		for (int i = 0; i <= targetIndex; i++)
		{
			if (target[i] == num)
			{
				num2++;
			}
		}
		int num3 = 0;
		foreach (int offer in restaurantDetail.OfferList)
		{
			if (offer == num)
			{
				num3++;
			}
		}
		return num3 >= num2;
	}

	private bool CanOfferByPieceLevel(RestaurantDetail restaurantDetail, int[] slotList, int pieceLevel)
	{
		int num = 0;
		foreach (int item in restaurantDetail.TargetDetail.Target)
		{
			if (item == pieceLevel)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return false;
		}
		int num2 = 0;
		foreach (int offer in restaurantDetail.OfferList)
		{
			if (offer == pieceLevel)
			{
				num2++;
			}
		}
		int num3 = 0;
		for (int i = 0; i < slotList.Length; i++)
		{
			if (slotList[i] == pieceLevel)
			{
				num3++;
			}
		}
		return Mathf.Min(num3, num - num2) > 0;
	}

	public bool CanCompose(RestaurantDetail restaurantDetail)
	{
		if (restaurantDetail == null)
		{
			return false;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = Mathf.Min(restaurantDetail.SlotList.Count, restaurantDetail.SlotCountMax);
		for (int i = 0; i < num; i++)
		{
			int num2 = restaurantDetail.SlotList[i];
			if (num2 > 0 && !CanOfferByPieceLevel(restaurantDetail, ((IEnumerable<int>)restaurantDetail.SlotList).ToArray(), num2) && !dictionary.TryAdd(num2, 1))
			{
				dictionary[num2]++;
				if (dictionary[num2] >= 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanOfferAllTarget(RestaurantDetail detail, int[] slotList)
	{
		if (detail.TargetDetail.State == ECommonState.Yes)
		{
			return false;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int num in slotList)
		{
			if (num != 0)
			{
				dictionary.TryAdd(num, 0);
				dictionary[num]++;
			}
		}
		foreach (int item in detail.TargetDetail.Target)
		{
			if (!dictionary.TryGetValue(item, out var value) || value == 0)
			{
				return false;
			}
			dictionary[item]--;
		}
		return true;
	}

	public bool HasAwardNotify(RestaurantDetail detail)
	{
		if (detail.ActivateAward == EBoolState.False)
		{
			return true;
		}
		if (detail.DailyAward.Count < detail.DailyAward.Total && zbSelf.Instance.timeHandler.GetServerTime() > detail.DailyAward.NextTm)
		{
			return true;
		}
		if (detail.TargetDetail.State == ECommonState.Yes)
		{
			return true;
		}
		return false;
	}

	public bool NeedGuideFinger(RestaurantDetail detail)
	{
		for (int i = 0; i < 3; i++)
		{
			if (CanOfferByTargetIndex(detail, i))
			{
				return true;
			}
		}
		if (HasEmptySlot(detail, needTip: false) && HasEnoughCost(detail, detail.SingleCost[0].Count, needTip: false))
		{
			return true;
		}
		var (num, num2) = GetCanMergeFromAndToIndex(detail, ((IEnumerable<int>)detail.SlotList).ToArray());
		if (num >= 0)
		{
			_ = 0;
			return false;
		}
		return false;
	}

	public bool HasRedPoint()
	{
		RestaurantDetail restaurantDetail = GetRestaurantDetail();
		if (restaurantDetail != null && (HasAwardNotify(restaurantDetail) || NeedGuideFinger(restaurantDetail)))
		{
			return true;
		}
		return false;
	}
}
