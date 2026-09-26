using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbHolidayEventView
{
	private zbBaseWnd m_wndParent;

	private VisualElement m_content;

	private VisualElement m_root;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root, float height = 360f)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_root = root;
		m_content = UQueryExtensions.Q<VisualElement>(m_root, "content", (string)null);
		if ((int)Application.platform == 8)
		{
			if (height == 120f)
			{
				m_root.style.top = StyleLength.op_Implicit(72f);
			}
			else
			{
				m_root.style.top = StyleLength.op_Implicit(170f);
			}
		}
		UQueryExtensions.Q<VisualElement>(m_root, "root", (string)null).style.height = StyleLength.op_Implicit(height);
		m_content.style.height = StyleLength.op_Implicit(height);
	}

	public void Refresh(int PlanId)
	{
		//IL_0b9a: Unknown result type (might be due to invalid IL or missing references)
		zbSelf.Instance.operateHandler.GetOperateCount();
		List<zbOperateData> list = new List<zbOperateData>();
		if (zbServerData.sGameData.OperateInfo != null)
		{
			int num = 0;
			foreach (RechargeDailyDetail rechargeDaily in zbServerData.sGameData.OperateInfo.RechargeDailyList)
			{
				if (isOpen(rechargeDaily.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.RechargeDaily, num, rechargeDaily.BaseInfo));
					num++;
				}
			}
			int num2 = 0;
			foreach (RechargeSumDetail rechargeDailySum in zbServerData.sGameData.OperateInfo.RechargeDailySumList)
			{
				if (isOpen(rechargeDailySum.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.RechargeDailySum, num2, rechargeDailySum.BaseInfo));
					num2++;
				}
			}
			int num3 = 0;
			foreach (RechargeSumDetail rechargeSum in zbServerData.sGameData.OperateInfo.RechargeSumList)
			{
				if (isOpen(rechargeSum.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.RechargeSum, num3, rechargeSum.BaseInfo));
					num3++;
				}
			}
			foreach (RechargeGiftDetail rechargeGift in zbServerData.sGameData.OperateInfo.RechargeGiftList)
			{
				if (isOpen(rechargeGift.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.RechargeGift, 0, rechargeGift.BaseInfo));
					break;
				}
			}
			int num4 = 0;
			foreach (ConsumeSumDetail consumeDailySum in zbServerData.sGameData.OperateInfo.ConsumeDailySumList)
			{
				if (isOpen(consumeDailySum.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.ConsumeDailySum, num4, consumeDailySum.BaseInfo));
					num4++;
				}
			}
			int num5 = 0;
			foreach (ConsumeSumDetail consumeSum in zbServerData.sGameData.OperateInfo.ConsumeSumList)
			{
				if (isOpen(consumeSum.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.ConsumeSum, num5, consumeSum.BaseInfo));
					num5++;
				}
			}
			foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
			{
				if (isOpen(limitTimeHero.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.LimitTimeHero, 0, limitTimeHero.BaseInfo));
					break;
				}
			}
			int num6 = 0;
			foreach (LimitTimeShopDetail limitTimeShop in zbServerData.sGameData.OperateInfo.LimitTimeShopList)
			{
				if (isOpen(limitTimeShop.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.LimitTimeShop, num6, limitTimeShop.BaseInfo));
					num6++;
				}
			}
			int num7 = 0;
			foreach (LoginSumDetail loginSum in zbServerData.sGameData.OperateInfo.LoginSumList)
			{
				if (isOpen(loginSum.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.LoginSum, num7, loginSum.BaseInfo));
					num7++;
				}
			}
			int num8 = 0;
			foreach (DailyActiveDetail dailyActive in zbServerData.sGameData.OperateInfo.DailyActiveList)
			{
				if (isOpen(dailyActive.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.DailyActive, num8, dailyActive.BaseInfo));
					num8++;
				}
			}
			int num9 = 0;
			foreach (BarDrawDetail barDraw in zbServerData.sGameData.OperateInfo.BarDrawList)
			{
				if (isOpen(barDraw.BaseInfo, PlanId))
				{
					if (barDraw.BarType == EBarType.Normal)
					{
						list.Add(new zbOperateData(EOperateType.BarDrawNormal, num9, barDraw.BaseInfo));
					}
					else if (barDraw.BarType == EBarType.Special)
					{
						list.Add(new zbOperateData(EOperateType.BarDrawSpecial, num9, barDraw.BaseInfo));
					}
					num9++;
				}
			}
			int num10 = 0;
			foreach (PropExchangeDetail propExchange in zbServerData.sGameData.OperateInfo.PropExchangeList)
			{
				if (isOpen(propExchange.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.PropExchange, num10, propExchange.BaseInfo));
					num10++;
				}
			}
			int num11 = 0;
			foreach (ScratchCardDetail scratchCard in zbServerData.sGameData.OperateInfo.ScratchCardList)
			{
				if (isOpen(scratchCard.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.ScratchCard, num11, scratchCard.BaseInfo));
					num11++;
				}
			}
			int num12 = 0;
			foreach (JiZiDetail jiZi in zbServerData.sGameData.OperateInfo.JiZiList)
			{
				if (isOpen(jiZi.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.JiZi, num12, jiZi.BaseInfo));
					num12++;
				}
			}
			int num13 = 0;
			foreach (CipherBoxDetail cipherBox in zbServerData.sGameData.OperateInfo.CipherBoxList)
			{
				if (isOpen(cipherBox.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.CipherBox, num13, cipherBox.BaseInfo));
					num13++;
				}
			}
			int num14 = 0;
			foreach (BarrageCmdDetail barrageCmd in zbServerData.sGameData.OperateInfo.BarrageCmdList)
			{
				if (isOpen(barrageCmd.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.BarrageCmd, num14, barrageCmd.BaseInfo));
					num14++;
				}
			}
			int num15 = 0;
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (isOpen(limitTimeCatcher.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.LimitTimeCatcher, num15, limitTimeCatcher.BaseInfo));
					num15++;
				}
			}
			int num16 = 0;
			foreach (HolidayTaskDetail holidayTask in zbServerData.sGameData.OperateInfo.HolidayTaskList)
			{
				if (isOpen(holidayTask.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.HolidayTask, num16, holidayTask.BaseInfo));
					num16++;
				}
			}
			int num17 = 0;
			foreach (PrayLanternDetail prayLantern in zbServerData.sGameData.OperateInfo.PrayLanternList)
			{
				if (isOpen(prayLantern.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.PrayLantern, num17, prayLantern.BaseInfo));
					num17++;
				}
			}
			int num18 = 0;
			foreach (ResourceExchangeDetail resourceExchange in zbServerData.sGameData.OperateInfo.ResourceExchangeList)
			{
				if (isOpen(resourceExchange.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.ResourceExchange, num18, resourceExchange.BaseInfo));
					num18++;
				}
			}
			if (zbServerData.sGameData.OperateInfo.FusionInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo) && zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo.Type == EOperatePlanType.Holiday)
			{
				list.Add(new zbOperateData(EOperateType.Fusion, 0, zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo));
			}
			int num19 = 0;
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (isOpen(infiniteGift.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.InfiniteGift, num19, infiniteGift.BaseInfo));
					num19++;
				}
			}
			int num20 = 0;
			foreach (LimitTimeRouletteDetail limitTimeRoulette in zbServerData.sGameData.OperateInfo.LimitTimeRouletteList)
			{
				if (isOpen(limitTimeRoulette.BaseInfo, PlanId))
				{
					list.Add(new zbOperateData(EOperateType.LimitTimeRoulette, num20, limitTimeRoulette.BaseInfo));
					num20++;
				}
			}
			int num21 = 0;
			if (zbServerData.sGameData.OperateInfo.LimitTimeTreasureList != null && zbServerData.sGameData.OperateInfo.LimitTimeTreasureList.Count > 0)
			{
				foreach (LimitTimeRouletteDetail limitTimeTreasure in zbServerData.sGameData.OperateInfo.LimitTimeTreasureList)
				{
					if (isOpen(limitTimeTreasure.BaseInfo, PlanId))
					{
						list.Add(new zbOperateData(EOperateType.LimitTimeTreasure, num21, limitTimeTreasure.BaseInfo));
						num21++;
					}
				}
			}
			int num22 = 0;
			if (zbServerData.sGameData.OperateInfo.LimitTimeVendorList != null && zbServerData.sGameData.OperateInfo.LimitTimeVendorList.Count > 0)
			{
				foreach (LimitTimeRouletteDetail limitTimeVendor in zbServerData.sGameData.OperateInfo.LimitTimeVendorList)
				{
					if (isOpen(limitTimeVendor.BaseInfo, PlanId))
					{
						list.Add(new zbOperateData(EOperateType.LimitTimeVendor, num22, limitTimeVendor.BaseInfo));
						num22++;
					}
				}
			}
			int num23 = 0;
			if (zbServerData.sGameData.OperateInfo.ZhanLingKillList != null && zbServerData.sGameData.OperateInfo.ZhanLingKillList.Count > 0)
			{
				foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
				{
					if (isOpen(zhanLingKill.BaseInfo, PlanId))
					{
						list.Add(new zbOperateData(EOperateType.ZhanLingKill, num23, zhanLingKill.BaseInfo));
						num23++;
					}
				}
			}
		}
		m_content.Clear();
		foreach (zbOperateData item in list)
		{
			zbHolidayEventItem zbHolidayEventItem2 = new zbHolidayEventItem(m_wndParent);
			zbHolidayEventItem2.Init(item);
			m_content.Add((VisualElement)(object)zbHolidayEventItem2);
		}
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(list.Count <= 0));
	}

	public bool isOpen(OperateBaseInfo baseInfo, int PlanId)
	{
		if (zbSelf.Instance.operateHandler.IsShowTime(baseInfo))
		{
			return baseInfo.Type == EOperatePlanType.Holiday;
		}
		return false;
	}

	public void Update()
	{
		foreach (VisualElement item in m_content.Children())
		{
			if (item is zbHolidayEventItem zbHolidayEventItem2)
			{
				zbHolidayEventItem2.UpdatePerSecond();
			}
		}
	}
}
