using System;
using Proto.Enums;
using Proto.Types;

public class zbBarHandler
{
	private int m_barNormalCost;

	private int m_barSpecialCost;

	private int m_barTreasureCost;

	private int m_barTreasurePlusCost;

	public void Init()
	{
		m_barNormalCost = (int)zbDataConfig.Instance.barBaseJsonTable.GetRecord()["normal"][(object)"barCost"];
		m_barSpecialCost = (int)zbDataConfig.Instance.barBaseJsonTable.GetRecord()["special"][(object)"barCost"];
		m_barTreasureCost = (int)zbDataConfig.Instance.barBaseJsonTable.GetRecord()["treasure"][(object)"barCost"];
		m_barTreasurePlusCost = (int)zbDataConfig.Instance.barBaseJsonTable.GetRecord()["treasurePlus"][(object)"barCost"];
	}

	public int GetBarCost(EBarType barType)
	{
		return barType switch
		{
			EBarType.Normal => m_barNormalCost, 
			EBarType.Special => m_barSpecialCost, 
			EBarType.Treasure => m_barTreasureCost, 
			EBarType.TreasurePlus => m_barTreasurePlusCost, 
			_ => 1, 
		};
	}

	public BarDetail GetBarDetail(EBarType barType)
	{
		if (zbServerData.sGameData.BarInfo != null)
		{
			foreach (BarDetail bar in zbServerData.sGameData.BarInfo.BarList)
			{
				if (bar.BarType == barType)
				{
					return bar;
				}
			}
		}
		return null;
	}

	public bool HasBarRedPointByBarType(EBarType barType, int count = 10)
	{
		BarDetail barDetail = GetBarDetail(barType);
		if (barDetail == null)
		{
			return false;
		}
		if (barType == EBarType.Normal || barType == EBarType.Special)
		{
			OperateTargetData barDrawTargetDetail = GetBarDrawTargetDetail(barType);
			if (barDrawTargetDetail != null && barDrawTargetDetail.TargetDetail.State == ECommonState.Yes)
			{
				return true;
			}
		}
		EMoneyType tid = EMoneyType.BarNormal;
		switch (barType)
		{
		case EBarType.Special:
			tid = EMoneyType.BarSpecial;
			break;
		case EBarType.Treasure:
			tid = EMoneyType.BarTreasure;
			break;
		case EBarType.TreasurePlus:
			tid = EMoneyType.BarTreasurePlus;
			break;
		}
		int num = GetBarCost(barType) * count;
		if (barDetail.DailyFree == EBoolState.True || zbSelf.Instance.moneyHandler.HasEnoughMoney((int)tid, num))
		{
			return true;
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!HasBarRedPointByBarType(EBarType.Normal) && !HasBarRedPointByBarType(EBarType.Special) && !zbSelf.Instance.operateHandler.HasLimitTimeHeroGift(isHoliday: true))
		{
			return zbSelf.Instance.operateHandler.HasLimitTimeHeroGift();
		}
		return true;
	}

	public bool HasTreasureBarRedPoint()
	{
		if (!HasBarRedPointByBarType(EBarType.Treasure, 1) && !HasBarRedPointByBarType(EBarType.TreasurePlus, 1))
		{
			return zbSelf.Instance.treasureHandler.HasFreePackRedDot();
		}
		return true;
	}

	public OperateTargetData GetBarDrawTargetDetail(EBarType barType)
	{
		OperateTargetData d = null;
		if (zbSelf.Instance.journeyThreeHandler.IsActive() && barType == EBarType.Normal && zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterThree != null)
		{
			foreach (JourneyTargetRecordDetail target in zbServerData.sGameData.JourneyInfo.ChapterThree.DrawCount.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return new OperateTargetData(TransformTargetDetail(target), zbServerData.sGameData.JourneyInfo.ChapterThree.DrawCount.Reach, "");
				}
			}
			foreach (JourneyTargetRecordDetail target2 in zbServerData.sGameData.JourneyInfo.ChapterThree.DrawCount.TargetList)
			{
				if (target2.State == ECommonState.No)
				{
					d = new OperateTargetData(TransformTargetDetail(target2), zbServerData.sGameData.JourneyInfo.ChapterThree.DrawCount.Reach, "");
					break;
				}
			}
		}
		OperateTargetData d2 = null;
		if (zbServerData.sGameData.OperateInfo.BarDrawList != null)
		{
			foreach (BarDrawDetail barDraw in zbServerData.sGameData.OperateInfo.BarDrawList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(barDraw.BaseInfo) || barDraw.BarType != barType)
				{
					continue;
				}
				foreach (OperateTargetDetail target3 in barDraw.TargetList)
				{
					if (target3.State == ECommonState.Yes)
					{
						return new OperateTargetData(target3, barDraw.Count, barDraw.BaseInfo.UUID);
					}
				}
				foreach (OperateTargetDetail target4 in barDraw.TargetList)
				{
					if (target4.State == ECommonState.No)
					{
						d2 = new OperateTargetData(target4, barDraw.Count, barDraw.BaseInfo.UUID);
						break;
					}
				}
			}
		}
		return CompareOperateTargetDatas(d, d2);
	}

	private OperateTargetData CompareOperateTargetDatas(OperateTargetData d0, OperateTargetData d1)
	{
		int num = ((d0 != null) ? (d0.TargetDetail.Target - d0.Count) : 0);
		int num2 = ((d1 != null) ? (d1.TargetDetail.Target - d1.Count) : 0);
		if (num <= 0)
		{
			num = int.MaxValue;
		}
		if (num2 <= 0)
		{
			num2 = int.MaxValue;
		}
		int num3 = Math.Min(num, num2);
		if (num3 == int.MaxValue)
		{
			return null;
		}
		if (num3 == num)
		{
			return d0;
		}
		return d1;
	}

	private OperateTargetDetail TransformTargetDetail(JourneyTargetRecordDetail target)
	{
		zbChapterThreeDrawCountCsvRecord recordByKey = zbDataConfig.Instance.chapterThreeDrawCountCsvTable.GetRecordByKey(target.Index);
		OperateTargetDetail operateTargetDetail = new OperateTargetDetail
		{
			Target = target.Target,
			Index = target.Index,
			State = target.State
		};
		int[][] awardList = recordByKey.awardList;
		foreach (int[] array in awardList)
		{
			operateTargetDetail.AwardList.Add(new AwardDetail
			{
				Type = (EResourceType)array[0],
				Tid = array[1],
				Count = array[2]
			});
		}
		return operateTargetDetail;
	}
}
