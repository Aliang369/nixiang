using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbHolidayHandler
{
	public List<List<int>> fusionData = new List<List<int>>();

	public List<string> uuidRechargeDaily = new List<string>();

	public List<string> uuidRechargeDailySum = new List<string>();

	public List<string> uuidRechargeSum = new List<string>();

	public List<string> uuidConsumeDailySum = new List<string>();

	public List<string> uuidConsumeSum = new List<string>();

	public List<string> uuidLimitTimeHero = new List<string>();

	public List<string> uuidLimitTimeShop = new List<string>();

	public List<string> uuidLimitVipHero = new List<string>();

	public List<string> uuidLoginSum = new List<string>();

	public List<string> uuidDailyActive = new List<string>();

	public List<string> uuidBarDraw = new List<string>();

	public List<string> uuidPropExchange = new List<string>();

	public List<string> uuidScratchCard = new List<string>();

	public List<string> uuidJiZi = new List<string>();

	public List<string> uuidCipherBox = new List<string>();

	public List<string> uuidRechargeGift = new List<string>();

	public List<string> uuidTimeCatcher = new List<string>();

	public List<string> uuidRedBag = new List<string>();

	public List<string> uuidPrayLantern = new List<string>();

	public List<string> uuidResourceExchange = new List<string>();

	public List<string> uuidHolidayTask = new List<string>();

	public List<string> uuidInfiniteGift = new List<string>();

	public List<string> uuidLimitTimeRoulette = new List<string>();

	public List<string> uuidLimitTimeTreasure = new List<string>();

	public List<string> uuidLimitTimeVendor = new List<string>();

	public List<string> uuidThemeTask = new List<string>();

	public List<string> uuidZhanLingKill = new List<string>();

	private bool m_isFirst = true;

	public void Init()
	{
		uuidRechargeDaily.Clear();
		uuidRechargeDailySum.Clear();
		uuidRechargeSum.Clear();
		uuidConsumeDailySum.Clear();
		uuidConsumeSum.Clear();
		uuidLimitTimeHero.Clear();
		uuidLimitTimeShop.Clear();
		uuidLimitVipHero.Clear();
		uuidLoginSum.Clear();
		uuidDailyActive.Clear();
		uuidBarDraw.Clear();
		uuidPropExchange.Clear();
		uuidScratchCard.Clear();
		uuidJiZi.Clear();
		uuidCipherBox.Clear();
		uuidRechargeGift.Clear();
		uuidTimeCatcher.Clear();
		uuidRedBag.Clear();
		uuidPrayLantern.Clear();
		uuidResourceExchange.Clear();
		uuidHolidayTask.Clear();
		uuidInfiniteGift.Clear();
		uuidLimitTimeRoulette.Clear();
		uuidLimitTimeTreasure.Clear();
		uuidLimitTimeVendor.Clear();
		uuidThemeTask.Clear();
		uuidZhanLingKill.Clear();
	}

	public int GetHolidayCount()
	{
		int num = 0;
		if (zbServerData.sGameData.OperateInfo == null)
		{
			return -1;
		}
		Init();
		if (zbServerData.sGameData.OperateInfo.RechargeDailyList != null)
		{
			foreach (RechargeDailyDetail rechargeDaily in zbServerData.sGameData.OperateInfo.RechargeDailyList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeDaily.BaseInfo) && rechargeDaily.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidRechargeDaily.Add(rechargeDaily.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.RechargeDailySumList != null)
		{
			foreach (RechargeSumDetail rechargeDailySum in zbServerData.sGameData.OperateInfo.RechargeDailySumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeDailySum.BaseInfo) && rechargeDailySum.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidRechargeDailySum.Add(rechargeDailySum.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.RechargeSumList != null)
		{
			foreach (RechargeSumDetail rechargeSum in zbServerData.sGameData.OperateInfo.RechargeSumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeSum.BaseInfo) && rechargeSum.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidRechargeSum.Add(rechargeSum.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ConsumeDailySumList != null)
		{
			foreach (ConsumeSumDetail consumeDailySum in zbServerData.sGameData.OperateInfo.ConsumeDailySumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(consumeDailySum.BaseInfo) && consumeDailySum.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidConsumeDailySum.Add(consumeDailySum.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ConsumeSumList != null)
		{
			foreach (ConsumeSumDetail consumeSum in zbServerData.sGameData.OperateInfo.ConsumeSumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(consumeSum.BaseInfo) && consumeSum.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidConsumeSum.Add(consumeSum.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeHeroList != null)
		{
			foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeHero.BaseInfo) && limitTimeHero.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidLimitTimeHero.Add(limitTimeHero.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeShopList != null)
		{
			foreach (LimitTimeShopDetail limitTimeShop in zbServerData.sGameData.OperateInfo.LimitTimeShopList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeShop.BaseInfo) && limitTimeShop.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidLimitTimeShop.Add(limitTimeShop.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitVipHeroList != null)
		{
			foreach (LimitVipHeroDetail limitVipHero in zbServerData.sGameData.OperateInfo.LimitVipHeroList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitVipHero.BaseInfo) && limitVipHero.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidLimitVipHero.Add(limitVipHero.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LoginSumList != null)
		{
			foreach (LoginSumDetail loginSum in zbServerData.sGameData.OperateInfo.LoginSumList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(loginSum.BaseInfo) && loginSum.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidLoginSum.Add(loginSum.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.DailyActiveList != null)
		{
			foreach (DailyActiveDetail dailyActive in zbServerData.sGameData.OperateInfo.DailyActiveList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(dailyActive.BaseInfo) && dailyActive.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidDailyActive.Add(dailyActive.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.BarDrawList != null)
		{
			foreach (BarDrawDetail barDraw in zbServerData.sGameData.OperateInfo.BarDrawList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(barDraw.BaseInfo) && barDraw.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidBarDraw.Add(barDraw.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.PropExchangeList != null)
		{
			foreach (PropExchangeDetail propExchange in zbServerData.sGameData.OperateInfo.PropExchangeList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(propExchange.BaseInfo) && propExchange.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidPropExchange.Add(propExchange.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ScratchCardList != null)
		{
			foreach (ScratchCardDetail scratchCard in zbServerData.sGameData.OperateInfo.ScratchCardList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(scratchCard.BaseInfo) && scratchCard.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidScratchCard.Add(scratchCard.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.JiZiList != null)
		{
			foreach (JiZiDetail jiZi in zbServerData.sGameData.OperateInfo.JiZiList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(jiZi.BaseInfo) && jiZi.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidJiZi.Add(jiZi.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.CipherBoxList != null)
		{
			foreach (CipherBoxDetail cipherBox in zbServerData.sGameData.OperateInfo.CipherBoxList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(cipherBox.BaseInfo) && cipherBox.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidCipherBox.Add(cipherBox.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.RechargeGiftList != null)
		{
			foreach (RechargeGiftDetail rechargeGift in zbServerData.sGameData.OperateInfo.RechargeGiftList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(rechargeGift.BaseInfo) && rechargeGift.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidRechargeGift.Add(rechargeGift.BaseInfo.UUID);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeCatcherList != null)
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeCatcher.BaseInfo) && limitTimeCatcher.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidTimeCatcher.Add(limitTimeCatcher.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.BarrageCmdList != null)
		{
			foreach (BarrageCmdDetail barrageCmd in zbServerData.sGameData.OperateInfo.BarrageCmdList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(barrageCmd.BaseInfo) && barrageCmd.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidRedBag.Add(barrageCmd.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.PrayLanternList != null)
		{
			foreach (PrayLanternDetail prayLantern in zbServerData.sGameData.OperateInfo.PrayLanternList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(prayLantern.BaseInfo) && prayLantern.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidPrayLantern.Add(prayLantern.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ResourceExchangeList != null)
		{
			foreach (ResourceExchangeDetail resourceExchange in zbServerData.sGameData.OperateInfo.ResourceExchangeList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(resourceExchange.BaseInfo) && resourceExchange.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidResourceExchange.Add(resourceExchange.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.HolidayTaskList != null)
		{
			foreach (HolidayTaskDetail holidayTask in zbServerData.sGameData.OperateInfo.HolidayTaskList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(holidayTask.BaseInfo) && holidayTask.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidHolidayTask.Add(holidayTask.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.FusionInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo) && zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo.Type == EOperatePlanType.Holiday)
		{
			num++;
		}
		if (zbServerData.sGameData.OperateInfo.InfiniteGiftList != null)
		{
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(infiniteGift.BaseInfo) && infiniteGift.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidInfiniteGift.Add(infiniteGift.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeRouletteList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeRoulette in zbServerData.sGameData.OperateInfo.LimitTimeRouletteList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeRoulette.BaseInfo) && limitTimeRoulette.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidLimitTimeRoulette.Add(limitTimeRoulette.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeTreasureList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeTreasure in zbServerData.sGameData.OperateInfo.LimitTimeTreasureList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeTreasure.BaseInfo) && limitTimeTreasure.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidLimitTimeTreasure.Add(limitTimeTreasure.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeVendorList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeVendor in zbServerData.sGameData.OperateInfo.LimitTimeVendorList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(limitTimeVendor.BaseInfo) && limitTimeVendor.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidLimitTimeVendor.Add(limitTimeVendor.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ThemeTaskList != null)
		{
			foreach (ThemeTaskDetail themeTask in zbServerData.sGameData.OperateInfo.ThemeTaskList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(themeTask.BaseInfo) && themeTask.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidThemeTask.Add(themeTask.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ZhanLingKillList != null)
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(zhanLingKill.BaseInfo) && zhanLingKill.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					uuidThemeTask.Add(zhanLingKill.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (m_isFirst && zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail() != null)
		{
			m_isFirst = false;
			zbWsMsgHandler.Instance.EndlessMine_Data_Req();
		}
		return num;
	}

	public EndlessMineDetail GetEndlessMineInfo()
	{
		if (zbServerData.sGameData.OperateInfo.EndlessMineInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.EndlessMineInfo.BaseInfo) && zbServerData.sGameData.OperateInfo.EndlessMineInfo.BaseInfo.Type == EOperatePlanType.Holiday)
		{
			return zbServerData.sGameData.OperateInfo.EndlessMineInfo;
		}
		return null;
	}

	public bool HasRedPoint()
	{
		if (zbServerData.sGameData.OperateInfo == null)
		{
			return false;
		}
		List<(List<string>, Func<string, bool>)> list = new List<(List<string>, Func<string, bool>)>
		{
			(uuidRechargeDaily, zbSelf.Instance.operateHandler.HasRechargeDailyGift),
			(uuidRechargeDailySum, zbSelf.Instance.operateHandler.HasRechargeDailySumGift),
			(uuidRechargeSum, zbSelf.Instance.operateHandler.HasRechargeSumGift),
			(uuidConsumeDailySum, zbSelf.Instance.operateHandler.HasConsumeDailySumGift),
			(uuidConsumeSum, zbSelf.Instance.operateHandler.HasConsumeSumGift),
			(uuidLimitTimeShop, zbSelf.Instance.operateHandler.HasLimitTimeShopGift),
			(uuidLimitVipHero, zbSelf.Instance.operateHandler.HasLimitVipHeroGift),
			(uuidLoginSum, zbSelf.Instance.operateHandler.HasLoginSumGift),
			(uuidDailyActive, zbSelf.Instance.operateHandler.HasDailyActivieGift),
			(uuidBarDraw, zbSelf.Instance.operateHandler.HasBarDrawGift),
			(uuidPropExchange, zbSelf.Instance.operateHandler.HasExchangeGift),
			(uuidScratchCard, zbSelf.Instance.operateHandler.HasScratchCardGift),
			(uuidJiZi, zbSelf.Instance.operateHandler.HasJiZiGift),
			(uuidCipherBox, zbSelf.Instance.operateHandler.HasCipherBoxGift),
			(uuidTimeCatcher, zbSelf.Instance.operateHandler.HasTimeCatcherNotify),
			(uuidRedBag, zbSelf.Instance.operateHandler.HasBarrageCmdGift),
			(uuidPrayLantern, zbSelf.Instance.operateHandler.HasPrayLanternGift),
			(uuidResourceExchange, zbSelf.Instance.operateHandler.HasResourceExchangeGift),
			(uuidHolidayTask, zbSelf.Instance.operateHandler.HasHolidayTaskGift),
			(uuidInfiniteGift, zbSelf.Instance.operateHandler.HasInfiniteGift),
			(uuidLimitTimeRoulette, zbSelf.Instance.operateHandler.HasLimitTimeRouletteGift),
			(uuidLimitTimeTreasure, zbSelf.Instance.operateHandler.HasLimitTimeTreasure),
			(uuidLimitTimeVendor, zbSelf.Instance.operateHandler.HasLimitTimeVendor),
			(uuidThemeTask, zbSelf.Instance.operateHandler.HasThemeTaskNotify),
			(uuidZhanLingKill, zbSelf.Instance.operateHandler.HasZhanLingKillNotify)
		};
		if (zbSelf.Instance.operateHandler.HasLimitTimeHeroGift(isHoliday: true) || zbSelf.Instance.operateHandler.HasFusionGift(isHoliday: true) || zbSelf.Instance.endlessMineHandler.HasRedPoint() || zbSelf.Instance.restaurantHandler.HasRedPoint() || zbSelf.Instance.snowVeilHandler.HasRedPoint() || zbSelf.Instance.bossSubjectHandler.HasRedPoint() || zbSelf.Instance.wallSubjectHandler.HasRedPoint() || zbSelf.Instance.mechaSubjectHandler.HasRedPoint())
		{
			return true;
		}
		foreach (var (source, predicate) in list)
		{
			if (source.Any(predicate))
			{
				return true;
			}
		}
		return false;
	}

	public ThemeTaskDetail GetThemeTaskDetail()
	{
		if (zbServerData.sGameData.OperateInfo.ThemeTaskList != null && zbServerData.sGameData.OperateInfo.ThemeTaskList.Count > 0)
		{
			foreach (ThemeTaskDetail themeTask in zbServerData.sGameData.OperateInfo.ThemeTaskList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(themeTask.BaseInfo) && themeTask.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					return themeTask;
				}
			}
		}
		return null;
	}

	public void OpenHoliday()
	{
		RestaurantDetail restaurantDetail = zbSelf.Instance.restaurantHandler.GetRestaurantDetail();
		if (restaurantDetail != null)
		{
			zbUIMgr.Instance.ShowRestaurant(restaurantDetail);
			return;
		}
		if (zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail() != null)
		{
			zbWsMsgHandler.Instance.EndlessMine_Data_Req(openWnd: true);
			return;
		}
		SnowVeilDetail snowVeilDetail = zbSelf.Instance.snowVeilHandler.GetSnowVeilDetail();
		if (snowVeilDetail != null)
		{
			zbUIMgr.Instance.ShowSnowVeil(snowVeilDetail);
			return;
		}
		BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail != null)
		{
			zbUIMgr.Instance.ShowBossSubjectMain(bossSubjectDetail);
			return;
		}
		WallSubjectDetail wallSubjectDetail = zbSelf.Instance.wallSubjectHandler.GetWallSubjectDetail();
		if (wallSubjectDetail != null)
		{
			zbUIMgr.Instance.ShowWallSubject(wallSubjectDetail);
			return;
		}
		MechaSubjectDetail mechaSubjectDetail = zbSelf.Instance.mechaSubjectHandler.GetMechaSubjectDetail();
		if (mechaSubjectDetail != null)
		{
			zbUIMgr.Instance.ShowMechaSubject(mechaSubjectDetail);
		}
	}
}
