using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using zbType;

public class zbShopHandler
{
	public zbItemData getWayItem;

	public void Init()
	{
	}

	public void UpdateShopList(ShopDetail[] shopList)
	{
		zbServerData.sGameData.ShopList.Clear();
		zbServerData.sGameData.ShopList.AddRange((IEnumerable<ShopDetail>)shopList);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Shop);
	}

	public bool IsShopOpen(int tabIndex)
	{
		EFuncType funcTypeByTab = zbSelf.Instance.shopHandler.GetFuncTypeByTab(tabIndex);
		if (!zbSelf.Instance.funHandler.IsLockNoTip(funcTypeByTab))
		{
			EShopType eShopType = (EShopType)(0 + tabIndex + 1);
			foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
			{
				if (shop.ShopType == eShopType && shop.SlotList.Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public EGetWayType GetGetWayTypeByShopType(EShopType shopType)
	{
		return shopType switch
		{
			EShopType.Shop1 => EGetWayType.Shop1, 
			EShopType.Shop2 => EGetWayType.Shop2, 
			EShopType.Shop3 => EGetWayType.Shop3, 
			EShopType.Shop4 => EGetWayType.Shop4, 
			EShopType.Shop5 => EGetWayType.Shop5, 
			_ => EGetWayType.Shop1, 
		};
	}

	public EFuncType GetFuncTypeByShopType(EShopType shopType)
	{
		return shopType switch
		{
			EShopType.Shop1 => EFuncType.Shop1, 
			EShopType.Shop2 => EFuncType.Shop2, 
			EShopType.Shop3 => EFuncType.Shop3, 
			EShopType.Shop4 => EFuncType.Shop4, 
			EShopType.Shop5 => EFuncType.Shop5, 
			_ => EFuncType.Shop1, 
		};
	}

	public EFuncType GetFuncTypeByTab(int shopTab)
	{
		return shopTab switch
		{
			0 => EFuncType.Shop1, 
			1 => EFuncType.Shop2, 
			2 => EFuncType.Shop3, 
			3 => EFuncType.Shop4, 
			4 => EFuncType.Shop5, 
			_ => EFuncType.Shop1, 
		};
	}

	public int GetOffByItemId(int shopIndex, EShopType shopType)
	{
		int result = 0;
		switch (shopType)
		{
		case EShopType.Shop1:
			result = zbDataConfig.Instance.shop1CsvTable.GetRecordByKey(shopIndex).off;
			break;
		case EShopType.Shop2:
			result = zbDataConfig.Instance.shop2CsvTable.GetRecordByKey(shopIndex).off;
			break;
		case EShopType.Shop3:
			result = zbDataConfig.Instance.shop3CsvTable.GetRecordByKey(shopIndex).off;
			break;
		case EShopType.Shop4:
			result = zbDataConfig.Instance.shop4CsvTable.GetRecordByKey(shopIndex).off;
			break;
		case EShopType.Shop5:
			result = zbDataConfig.Instance.shop5CsvTable.GetRecordByKey(shopIndex).off;
			break;
		}
		return result;
	}

	public int GetEffectByItemId(int shopIndex, EShopType shopType)
	{
		int result = 0;
		switch (shopType)
		{
		case EShopType.Shop1:
			result = zbDataConfig.Instance.shop1CsvTable.GetRecordByKey(shopIndex).effect;
			break;
		case EShopType.Shop2:
			result = zbDataConfig.Instance.shop2CsvTable.GetRecordByKey(shopIndex).effect;
			break;
		case EShopType.Shop3:
			result = zbDataConfig.Instance.shop3CsvTable.GetRecordByKey(shopIndex).effect;
			break;
		case EShopType.Shop4:
			result = zbDataConfig.Instance.shop4CsvTable.GetRecordByKey(shopIndex).effect;
			break;
		case EShopType.Shop5:
			result = zbDataConfig.Instance.shop5CsvTable.GetRecordByKey(shopIndex).effect;
			break;
		}
		return result;
	}

	public ShopDetail GetSortedShopDetail(ShopDetail shopDetail, EShopType shopType)
	{
		ShopDetail shopDetail2 = shopDetail.Clone();
		List<ShopSlotDetail> list = null;
		switch (shopType)
		{
		case EShopType.Shop1:
			list = ((IEnumerable<ShopSlotDetail>)shopDetail2.SlotList).OrderBy((ShopSlotDetail s) => zbDataConfig.Instance.shop1CsvTable.GetRecordByKey(s.Index).sort).ToList();
			break;
		case EShopType.Shop2:
			list = ((IEnumerable<ShopSlotDetail>)shopDetail2.SlotList).OrderBy((ShopSlotDetail s) => zbDataConfig.Instance.shop2CsvTable.GetRecordByKey(s.Index).sort).ToList();
			break;
		case EShopType.Shop3:
			list = ((IEnumerable<ShopSlotDetail>)shopDetail2.SlotList).OrderBy((ShopSlotDetail s) => zbDataConfig.Instance.shop3CsvTable.GetRecordByKey(s.Index).sort).ToList();
			break;
		case EShopType.Shop4:
			list = ((IEnumerable<ShopSlotDetail>)shopDetail2.SlotList).OrderBy((ShopSlotDetail s) => zbDataConfig.Instance.shop4CsvTable.GetRecordByKey(s.Index).sort).ToList();
			break;
		case EShopType.Shop5:
			list = ((IEnumerable<ShopSlotDetail>)shopDetail2.SlotList).OrderBy((ShopSlotDetail s) => zbDataConfig.Instance.shop5CsvTable.GetRecordByKey(s.Index).sort).ToList();
			break;
		}
		shopDetail2.SlotList.Clear();
		shopDetail2.SlotList.AddRange((IEnumerable<ShopSlotDetail>)list);
		return shopDetail2;
	}

	public bool HasNewPoint()
	{
		for (int i = 0; i < 4; i++)
		{
			if (zbSelf.Instance.newTagHandler.isNewFunc(GetFuncTypeByTab(i)))
			{
				return true;
			}
		}
		return false;
	}

	public OperateConsumTargetData GetConsumeTargetDetail()
	{
		OperateConsumTargetData d = null;
		if (zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			foreach (JourneyTargetRecordDetail item in zbServerData.sGameData.JourneyInfo?.ChapterTwo?.DiamondCost?.TargetList)
			{
				if (item.State == ECommonState.Yes)
				{
					return new OperateConsumTargetData(TransformTargetDetail(item), zbServerData.sGameData.JourneyInfo.ChapterTwo.DiamondCost.Reach, "", EOperateType.ConsumeSum);
				}
			}
			foreach (JourneyTargetRecordDetail item2 in zbServerData.sGameData.JourneyInfo?.ChapterTwo?.DiamondCost?.TargetList)
			{
				if (item2.State == ECommonState.No)
				{
					d = new OperateConsumTargetData(TransformTargetDetail(item2), zbServerData.sGameData.JourneyInfo.ChapterTwo.DiamondCost.Reach, "", EOperateType.ConsumeSum);
					break;
				}
			}
		}
		OperateConsumTargetData d2 = null;
		if (zbServerData.sGameData.OperateInfo.ConsumeDailySumList != null)
		{
			foreach (ConsumeSumDetail consumeDailySum in zbServerData.sGameData.OperateInfo.ConsumeDailySumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(consumeDailySum.BaseInfo))
				{
					continue;
				}
				foreach (ConsumeTargetDetail target in consumeDailySum.TargetList)
				{
					if (target.State == ECommonState.Yes)
					{
						return new OperateConsumTargetData(target, consumeDailySum.Count, consumeDailySum.BaseInfo.UUID, EOperateType.ConsumeDailySum);
					}
				}
				foreach (ConsumeTargetDetail target2 in consumeDailySum.TargetList)
				{
					if (target2.State == ECommonState.No)
					{
						d2 = new OperateConsumTargetData(target2, consumeDailySum.Count, consumeDailySum.BaseInfo.UUID, EOperateType.ConsumeDailySum);
						break;
					}
				}
			}
		}
		OperateConsumTargetData d3 = null;
		if (zbServerData.sGameData.OperateInfo.ConsumeSumList != null)
		{
			foreach (ConsumeSumDetail consumeSum in zbServerData.sGameData.OperateInfo.ConsumeSumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(consumeSum.BaseInfo))
				{
					continue;
				}
				foreach (ConsumeTargetDetail target3 in consumeSum.TargetList)
				{
					if (target3.State == ECommonState.Yes)
					{
						return new OperateConsumTargetData(target3, consumeSum.Count, consumeSum.BaseInfo.UUID, EOperateType.ConsumeSum);
					}
				}
				foreach (ConsumeTargetDetail target4 in consumeSum.TargetList)
				{
					if (target4.State == ECommonState.No)
					{
						d3 = new OperateConsumTargetData(target4, consumeSum.Count, consumeSum.BaseInfo.UUID, EOperateType.ConsumeSum);
						break;
					}
				}
			}
		}
		return CompareOperateConsumTargetDatas(d, d2, d3);
	}

	private OperateConsumTargetData CompareOperateConsumTargetDatas(OperateConsumTargetData d0, OperateConsumTargetData d1, OperateConsumTargetData d2)
	{
		int num = ((d0 != null) ? (d0.TargetDetail.Target - d0.Count) : 0);
		int num2 = ((d1 != null) ? (d1.TargetDetail.Target - d1.Count) : 0);
		int num3 = ((d2 != null) ? (d2.TargetDetail.Target - d2.Count) : 0);
		if (num <= 0)
		{
			num = int.MaxValue;
		}
		if (num2 <= 0)
		{
			num2 = int.MaxValue;
		}
		if (num3 <= 0)
		{
			num3 = int.MaxValue;
		}
		int num4 = Math.Min(num, Math.Min(num2, num3));
		if (num4 == int.MaxValue)
		{
			return null;
		}
		if (num4 == num)
		{
			return d0;
		}
		if (num4 == num2)
		{
			return d1;
		}
		return d2;
	}

	private ConsumeTargetDetail TransformTargetDetail(JourneyTargetRecordDetail target)
	{
		zbChapterTwoDiamondCostCsvRecord recordByKey = zbDataConfig.Instance.chapterTwoDiamondCostCsvTable.GetRecordByKey(target.Index);
		ConsumeTargetDetail consumeTargetDetail = new ConsumeTargetDetail
		{
			Target = target.Target,
			Index = target.Index,
			State = target.State
		};
		int[][] awardList = recordByKey.awardList;
		foreach (int[] array in awardList)
		{
			consumeTargetDetail.AwardList.Add(new AwardDetail
			{
				Type = (EResourceType)array[0],
				Tid = array[1],
				Count = array[2]
			});
		}
		return consumeTargetDetail;
	}

	public bool HasConsumeTargetDetailNotify()
	{
		OperateConsumTargetData consumeTargetDetail = GetConsumeTargetDetail();
		if (consumeTargetDetail != null && consumeTargetDetail.TargetDetail.State == ECommonState.Yes)
		{
			return true;
		}
		return false;
	}

	public bool HasPuTongNotify()
	{
		foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
		{
			if (shop.ShopType != EShopType.Shop1)
			{
				continue;
			}
			foreach (ShopSlotDetail slot in shop.SlotList)
			{
				if (slot.Price == 0 && slot.Count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}
}
