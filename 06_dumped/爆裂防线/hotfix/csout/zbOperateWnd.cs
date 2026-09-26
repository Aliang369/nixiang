using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;
using zbType;

public class zbOperateWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private ListView m_listView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_operate.uxml");
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
	}

	public override void Refresh()
	{
		zbSelf.Instance.operateHandler.GetOperateCount();
		List<zbOperateData> list = new List<zbOperateData>();
		if (zbServerData.sGameData.OperateInfo != null)
		{
			int num = 0;
			foreach (RechargeDailyDetail rechargeDaily in zbServerData.sGameData.OperateInfo.RechargeDailyList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeDaily.BaseInfo) && rechargeDaily.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.RechargeDaily, num, rechargeDaily.BaseInfo));
					num++;
				}
			}
			int num2 = 0;
			foreach (RechargeSumDetail rechargeDailySum in zbServerData.sGameData.OperateInfo.RechargeDailySumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeDailySum.BaseInfo) && rechargeDailySum.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.RechargeDailySum, num2, rechargeDailySum.BaseInfo));
					num2++;
				}
			}
			int num3 = 0;
			foreach (RechargeSumDetail rechargeSum in zbServerData.sGameData.OperateInfo.RechargeSumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeSum.BaseInfo) && rechargeSum.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.RechargeSum, num3, rechargeSum.BaseInfo));
					num3++;
				}
			}
			int num4 = 0;
			foreach (ConsumeSumDetail consumeDailySum in zbServerData.sGameData.OperateInfo.ConsumeDailySumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(consumeDailySum.BaseInfo) && consumeDailySum.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.ConsumeDailySum, num4, consumeDailySum.BaseInfo));
					num4++;
				}
			}
			int num5 = 0;
			foreach (ConsumeSumDetail consumeSum in zbServerData.sGameData.OperateInfo.ConsumeSumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(consumeSum.BaseInfo) && consumeSum.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.ConsumeSum, num5, consumeSum.BaseInfo));
					num5++;
				}
			}
			foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeHero.BaseInfo) && limitTimeHero.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.LimitTimeHero, 0, limitTimeHero.BaseInfo));
					break;
				}
			}
			int num6 = 0;
			foreach (LimitTimeShopDetail limitTimeShop in zbServerData.sGameData.OperateInfo.LimitTimeShopList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeShop.BaseInfo) && limitTimeShop.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.LimitTimeShop, num6, limitTimeShop.BaseInfo));
					num6++;
				}
			}
			int num7 = 0;
			foreach (LoginSumDetail loginSum in zbServerData.sGameData.OperateInfo.LoginSumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(loginSum.BaseInfo) && loginSum.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.LoginSum, num7, loginSum.BaseInfo));
					num7++;
				}
			}
			int num8 = 0;
			foreach (DailyActiveDetail dailyActive in zbServerData.sGameData.OperateInfo.DailyActiveList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(dailyActive.BaseInfo) && dailyActive.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.DailyActive, num8, dailyActive.BaseInfo));
					num8++;
				}
			}
			int num9 = 0;
			foreach (BarDrawDetail barDraw in zbServerData.sGameData.OperateInfo.BarDrawList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(barDraw.BaseInfo) && barDraw.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (zbSelf.Instance.operateHandler.IsShowTime(propExchange.BaseInfo) && propExchange.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.PropExchange, num10, propExchange.BaseInfo));
					num10++;
				}
			}
			int num11 = 0;
			foreach (ScratchCardDetail scratchCard in zbServerData.sGameData.OperateInfo.ScratchCardList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(scratchCard.BaseInfo) && scratchCard.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.ScratchCard, num11, scratchCard.BaseInfo));
					num11++;
				}
			}
			int num12 = 0;
			foreach (JiZiDetail jiZi in zbServerData.sGameData.OperateInfo.JiZiList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(jiZi.BaseInfo) && jiZi.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.JiZi, num12, jiZi.BaseInfo));
					num12++;
				}
			}
			int num13 = 0;
			foreach (CipherBoxDetail cipherBox in zbServerData.sGameData.OperateInfo.CipherBoxList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(cipherBox.BaseInfo) && cipherBox.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.CipherBox, num13, cipherBox.BaseInfo));
					num13++;
				}
			}
			int num14 = 0;
			foreach (BarrageCmdDetail barrageCmd in zbServerData.sGameData.OperateInfo.BarrageCmdList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(barrageCmd.BaseInfo) && barrageCmd.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.BarrageCmd, num14, barrageCmd.BaseInfo));
					num14++;
				}
			}
			int num15 = 0;
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeCatcher.BaseInfo) && limitTimeCatcher.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.LimitTimeCatcher, num15, limitTimeCatcher.BaseInfo));
					num15++;
				}
			}
			int num16 = 0;
			foreach (HolidayTaskDetail holidayTask in zbServerData.sGameData.OperateInfo.HolidayTaskList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(holidayTask.BaseInfo) && holidayTask.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.HolidayTask, num16, holidayTask.BaseInfo));
					num16++;
				}
			}
			int num17 = 0;
			foreach (PrayLanternDetail prayLantern in zbServerData.sGameData.OperateInfo.PrayLanternList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(prayLantern.BaseInfo) && prayLantern.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.PrayLantern, num17, prayLantern.BaseInfo));
					num17++;
				}
			}
			int num18 = 0;
			foreach (ResourceExchangeDetail resourceExchange in zbServerData.sGameData.OperateInfo.ResourceExchangeList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(resourceExchange.BaseInfo) && resourceExchange.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.ResourceExchange, num18, resourceExchange.BaseInfo));
					num18++;
				}
			}
			if (zbServerData.sGameData.OperateInfo.FusionInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo) && zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo.Type != EOperatePlanType.Holiday)
			{
				list.Add(new zbOperateData(EOperateType.Fusion, 0, zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo));
			}
			int num19 = 0;
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(infiniteGift.BaseInfo) && infiniteGift.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					list.Add(new zbOperateData(EOperateType.InfiniteGift, num19, infiniteGift.BaseInfo));
					num19++;
				}
			}
			int num20 = 0;
			foreach (LimitTimeRouletteDetail limitTimeRoulette in zbServerData.sGameData.OperateInfo.LimitTimeRouletteList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeRoulette.BaseInfo) && limitTimeRoulette.BaseInfo.Type != EOperatePlanType.Holiday)
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
					if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeTreasure.BaseInfo) && limitTimeTreasure.BaseInfo.Type != EOperatePlanType.Holiday)
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
					if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeVendor.BaseInfo) && limitTimeVendor.BaseInfo.Type != EOperatePlanType.Holiday)
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
					if (zbSelf.Instance.operateHandler.IsShowTime(zhanLingKill.BaseInfo) && zhanLingKill.BaseInfo.Type != EOperatePlanType.Holiday)
					{
						list.Add(new zbOperateData(EOperateType.ZhanLingKill, num23, zhanLingKill.BaseInfo));
						num23++;
					}
				}
			}
		}
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.DailySign))
		{
			list.Add(new zbOperateData(EOperateType.DailySign));
		}
		list.Add(new zbOperateData(EOperateType.TiliBank));
		list.Add(new zbOperateData(EOperateType.Community));
		RefreshListView(list);
	}

	private void RefreshListView(List<zbOperateData> listViewDatas)
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbOperateItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbOperateItem).Refresh(listViewDatas[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = listViewDatas;
		UpdatePerSecond();
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		foreach (VisualElement item in ((VisualElement)UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null)).Children())
		{
			if (item is zbOperateItem zbOperateItem2)
			{
				zbOperateItem2.UpdatePerSecond();
			}
		}
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
