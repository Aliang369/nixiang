using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbOperateHandler
{
	public List<List<int>> fusionData = new List<List<int>>();

	public OperatePrayLanternDrawAck prayLanternDrawAckData;

	public OperateLimitTimeRouletteDrawAck timeRouletteAckData;

	public OperateLimitTimeTreasureDrawAck timeTreasureDrawAckData;

	public OperateLimitTimeVendorDrawAck timeVendorDrawData;

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

	public List<string> uuidZhanLingKill = new List<string>();

	private readonly bool m_firstRechargeDaily;

	private readonly bool m_firstRechargeDailySum;

	private readonly bool m_firstRechargeSum;

	private readonly bool m_firstConsumeSum;

	private readonly bool m_firstLimitTimeShopList;

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
		uuidZhanLingKill.Clear();
	}

	public bool IsShowTime(OperateBaseInfo _operteBaseInfo)
	{
		if (_operteBaseInfo.StartTm == 0L && _operteBaseInfo.EndTm == 0L)
		{
			return false;
		}
		if (zbSelf.Instance.timeHandler.GetServerTime() >= _operteBaseInfo.StartTm && zbSelf.Instance.timeHandler.GetServerTime() <= _operteBaseInfo.EndTm)
		{
			return true;
		}
		return false;
	}

	public int GetOperateCount()
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
				if (IsShowTime(rechargeDaily.BaseInfo) && rechargeDaily.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(rechargeDailySum.BaseInfo) && rechargeDailySum.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(rechargeSum.BaseInfo) && rechargeSum.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(consumeDailySum.BaseInfo) && consumeDailySum.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(consumeSum.BaseInfo) && consumeSum.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(limitTimeHero.BaseInfo) && limitTimeHero.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(limitTimeShop.BaseInfo) && limitTimeShop.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(limitVipHero.BaseInfo) && limitVipHero.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(loginSum.BaseInfo) && loginSum.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(dailyActive.BaseInfo) && dailyActive.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(barDraw.BaseInfo) && barDraw.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(propExchange.BaseInfo) && propExchange.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(scratchCard.BaseInfo) && scratchCard.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(jiZi.BaseInfo) && jiZi.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(cipherBox.BaseInfo) && cipherBox.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(rechargeGift.BaseInfo) && rechargeGift.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					uuidRechargeGift.Add(rechargeGift.BaseInfo.UUID);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeCatcherList != null)
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (IsShowTime(limitTimeCatcher.BaseInfo) && limitTimeCatcher.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(barrageCmd.BaseInfo) && barrageCmd.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(prayLantern.BaseInfo) && prayLantern.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(resourceExchange.BaseInfo) && resourceExchange.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(holidayTask.BaseInfo) && holidayTask.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					uuidHolidayTask.Add(holidayTask.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.FusionInfo != null && IsShowTime(zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo) && zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo.Type != EOperatePlanType.Holiday)
		{
			num++;
		}
		if (zbServerData.sGameData.OperateInfo.InfiniteGiftList != null)
		{
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (IsShowTime(infiniteGift.BaseInfo) && infiniteGift.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(limitTimeRoulette.BaseInfo) && limitTimeRoulette.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(limitTimeTreasure.BaseInfo) && limitTimeTreasure.BaseInfo.Type != EOperatePlanType.Holiday)
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
				if (IsShowTime(limitTimeVendor.BaseInfo) && limitTimeVendor.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					uuidLimitTimeVendor.Add(limitTimeVendor.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ZhanLingKillList != null)
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (IsShowTime(zhanLingKill.BaseInfo) && zhanLingKill.BaseInfo.Type != EOperatePlanType.Holiday)
				{
					uuidZhanLingKill.Add(zhanLingKill.BaseInfo.UUID);
					num++;
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.EndlessMineInfo != null && IsShowTime(zbServerData.sGameData.OperateInfo.EndlessMineInfo.BaseInfo))
		{
			num++;
		}
		return num + 3;
	}

	public int GetBarDrawTitleRoman(BarDrawDetail gmBarDraw, bool isHoliday)
	{
		int num = 0;
		if (zbServerData.sGameData.OperateInfo.BarDrawList != null)
		{
			foreach (BarDrawDetail barDraw in zbServerData.sGameData.OperateInfo.BarDrawList)
			{
				if (isHoliday)
				{
					if (IsShowTime(barDraw.BaseInfo) && barDraw.BaseInfo.Type == EOperatePlanType.Holiday && barDraw.BarType == gmBarDraw.BarType)
					{
						_ = barDraw.BaseInfo.UUID == gmBarDraw.BaseInfo.UUID;
						num++;
					}
				}
				else if (IsShowTime(barDraw.BaseInfo) && barDraw.BaseInfo.Type != EOperatePlanType.Holiday && barDraw.BarType == gmBarDraw.BarType)
				{
					_ = barDraw.BaseInfo.UUID == gmBarDraw.BaseInfo.UUID;
					num++;
				}
			}
		}
		return num;
	}

	public RechargeDailyDetail GetRechargeDailyByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.RechargeDailyList != null)
		{
			foreach (RechargeDailyDetail rechargeDaily in zbServerData.sGameData.OperateInfo.RechargeDailyList)
			{
				if (IsShowTime(rechargeDaily.BaseInfo) && rechargeDaily.BaseInfo.UUID == uuid)
				{
					return rechargeDaily;
				}
			}
		}
		return null;
	}

	public int GetRechargeDailyDaysByUUID(string uuid)
	{
		int num = 0;
		RechargeDailyDetail rechargeDailyByUUID = GetRechargeDailyByUUID(uuid);
		if (rechargeDailyByUUID != null)
		{
			foreach (RechargeTargetDetail target in rechargeDailyByUUID.TargetList)
			{
				if (target.State >= ECommonState.Yes)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool GetRechargeDailyNotifyByUUID(string uuid)
	{
		RechargeDailyDetail rechargeDailyByUUID = GetRechargeDailyByUUID(uuid);
		if (rechargeDailyByUUID != null)
		{
			foreach (RechargeTargetDetail target in rechargeDailyByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public RechargeSumDetail GetRechargeDailySumInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.RechargeDailySumList != null)
		{
			foreach (RechargeSumDetail rechargeDailySum in zbServerData.sGameData.OperateInfo.RechargeDailySumList)
			{
				if (IsShowTime(rechargeDailySum.BaseInfo) && rechargeDailySum.BaseInfo.UUID == uuid)
				{
					return rechargeDailySum;
				}
			}
		}
		return null;
	}

	public RechargeSumDetail GetRechargeSumInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.RechargeSumList != null)
		{
			foreach (RechargeSumDetail rechargeSum in zbServerData.sGameData.OperateInfo.RechargeSumList)
			{
				if (IsShowTime(rechargeSum.BaseInfo) && rechargeSum.BaseInfo.UUID == uuid)
				{
					return rechargeSum;
				}
			}
		}
		return null;
	}

	public ConsumeSumDetail GetConsumeSumInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.ConsumeSumList != null)
		{
			foreach (ConsumeSumDetail consumeSum in zbServerData.sGameData.OperateInfo.ConsumeSumList)
			{
				if (IsShowTime(consumeSum.BaseInfo) && consumeSum.BaseInfo.UUID == uuid)
				{
					return consumeSum;
				}
			}
		}
		return null;
	}

	public ConsumeSumDetail GetConsumeDailySumInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.ConsumeDailySumList != null)
		{
			foreach (ConsumeSumDetail consumeDailySum in zbServerData.sGameData.OperateInfo.ConsumeDailySumList)
			{
				if (IsShowTime(consumeDailySum.BaseInfo) && consumeDailySum.BaseInfo.UUID == uuid)
				{
					return consumeDailySum;
				}
			}
		}
		return null;
	}

	public LimitTimeShopDetail GetTimeShopInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitTimeShopList != null)
		{
			foreach (LimitTimeShopDetail limitTimeShop in zbServerData.sGameData.OperateInfo.LimitTimeShopList)
			{
				if (IsShowTime(limitTimeShop.BaseInfo) && limitTimeShop.BaseInfo.UUID == uuid)
				{
					return limitTimeShop;
				}
			}
		}
		return null;
	}

	public LimitVipHeroDetail GetVipHeroInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitVipHeroList != null)
		{
			foreach (LimitVipHeroDetail limitVipHero in zbServerData.sGameData.OperateInfo.LimitVipHeroList)
			{
				if (IsShowTime(limitVipHero.BaseInfo) && limitVipHero.BaseInfo.UUID == uuid)
				{
					return limitVipHero;
				}
			}
		}
		return null;
	}

	public LoginSumDetail GetLoginSumInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LoginSumList != null)
		{
			foreach (LoginSumDetail loginSum in zbServerData.sGameData.OperateInfo.LoginSumList)
			{
				if (IsShowTime(loginSum.BaseInfo) && loginSum.BaseInfo.UUID == uuid)
				{
					return loginSum;
				}
			}
		}
		return null;
	}

	public DailyActiveDetail GetActiveInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.DailyActiveList != null)
		{
			foreach (DailyActiveDetail dailyActive in zbServerData.sGameData.OperateInfo.DailyActiveList)
			{
				if (IsShowTime(dailyActive.BaseInfo) && dailyActive.BaseInfo.UUID == uuid)
				{
					return dailyActive;
				}
			}
		}
		return null;
	}

	public BarDrawDetail GetBarDrawInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.BarDrawList != null)
		{
			foreach (BarDrawDetail barDraw in zbServerData.sGameData.OperateInfo.BarDrawList)
			{
				if (IsShowTime(barDraw.BaseInfo) && barDraw.BaseInfo.UUID == uuid)
				{
					return barDraw;
				}
			}
		}
		return null;
	}

	public bool HasBarrageCmdGift(string uuid)
	{
		BarrageCmdDetail redBagInfoByUUID = GetRedBagInfoByUUID(uuid);
		if (redBagInfoByUUID != null)
		{
			foreach (AwardDetail award in redBagInfoByUUID.AwardList)
			{
				_ = award;
				if (redBagInfoByUUID.LimitNum > redBagInfoByUUID.Count)
				{
					return true;
				}
			}
		}
		return false;
	}

	public LimitTimeCatcherDetail GetTimeCatcherInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitTimeCatcherList != null)
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (IsShowTime(limitTimeCatcher.BaseInfo) && limitTimeCatcher.BaseInfo.UUID == uuid)
				{
					return limitTimeCatcher;
				}
			}
		}
		return null;
	}

	public BarrageCmdDetail GetRedBagInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.BarrageCmdList != null)
		{
			foreach (BarrageCmdDetail barrageCmd in zbServerData.sGameData.OperateInfo.BarrageCmdList)
			{
				if (IsShowTime(barrageCmd.BaseInfo) && barrageCmd.BaseInfo.UUID == uuid)
				{
					return barrageCmd;
				}
			}
		}
		return null;
	}

	public PrayLanternDetail GetPrayLanternInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.PrayLanternList != null)
		{
			foreach (PrayLanternDetail prayLantern in zbServerData.sGameData.OperateInfo.PrayLanternList)
			{
				if (IsShowTime(prayLantern.BaseInfo) && prayLantern.BaseInfo.UUID == uuid)
				{
					return prayLantern;
				}
			}
		}
		return null;
	}

	public ResourceExchangeDetail GetResourceExchangeInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.ResourceExchangeList != null)
		{
			foreach (ResourceExchangeDetail resourceExchange in zbServerData.sGameData.OperateInfo.ResourceExchangeList)
			{
				if (IsShowTime(resourceExchange.BaseInfo) && resourceExchange.BaseInfo.UUID == uuid)
				{
					return resourceExchange;
				}
			}
		}
		return null;
	}

	public PropExchangeDetail GetExchangeInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.PropExchangeList != null)
		{
			foreach (PropExchangeDetail propExchange in zbServerData.sGameData.OperateInfo.PropExchangeList)
			{
				if (IsShowTime(propExchange.BaseInfo) && propExchange.BaseInfo.UUID == uuid)
				{
					return propExchange;
				}
			}
		}
		return null;
	}

	public ScratchCardDetail GetScratchCardInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.ScratchCardList != null)
		{
			foreach (ScratchCardDetail scratchCard in zbServerData.sGameData.OperateInfo.ScratchCardList)
			{
				if (IsShowTime(scratchCard.BaseInfo) && scratchCard.BaseInfo.UUID == uuid)
				{
					return scratchCard;
				}
			}
		}
		return null;
	}

	public JiZiDetail GetJiZiYouLiByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.JiZiList != null)
		{
			foreach (JiZiDetail jiZi in zbServerData.sGameData.OperateInfo.JiZiList)
			{
				if (IsShowTime(jiZi.BaseInfo) && jiZi.BaseInfo.UUID == uuid)
				{
					return jiZi;
				}
			}
		}
		return null;
	}

	public CipherBoxDetail GetCipherBoxInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.CipherBoxList != null)
		{
			foreach (CipherBoxDetail cipherBox in zbServerData.sGameData.OperateInfo.CipherBoxList)
			{
				if (IsShowTime(cipherBox.BaseInfo) && cipherBox.BaseInfo.UUID == uuid)
				{
					return cipherBox;
				}
			}
		}
		return null;
	}

	public RechargeGiftDetail GetRechargeGiftByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.RechargeGiftList != null)
		{
			foreach (RechargeGiftDetail rechargeGift in zbServerData.sGameData.OperateInfo.RechargeGiftList)
			{
				if (IsShowTime(rechargeGift.BaseInfo) && rechargeGift.BaseInfo.UUID == uuid)
				{
					return rechargeGift;
				}
			}
		}
		return null;
	}

	public LimitTimeHeroDetail GetTimeHeroInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitTimeHeroList != null)
		{
			foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
			{
				if (IsShowTime(limitTimeHero.BaseInfo) && limitTimeHero.BaseInfo.UUID == uuid)
				{
					return limitTimeHero;
				}
			}
		}
		return null;
	}

	public HolidayTaskDetail GetHolidayTaskInfoByUUID(string uuid = null)
	{
		if (zbServerData.sGameData.OperateInfo?.HolidayTaskList != null)
		{
			foreach (HolidayTaskDetail holidayTask in zbServerData.sGameData.OperateInfo.HolidayTaskList)
			{
				if (IsShowTime(holidayTask.BaseInfo))
				{
					if (uuid == null)
					{
						return holidayTask;
					}
					if (holidayTask.BaseInfo.UUID == uuid)
					{
						return holidayTask;
					}
				}
			}
		}
		return null;
	}

	public BossSubjectDetail GetBossSubjectDetailByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.BossSubjectList != null)
		{
			foreach (BossSubjectDetail bossSubject in zbServerData.sGameData.OperateInfo.BossSubjectList)
			{
				if (IsShowTime(bossSubject.BaseInfo) && bossSubject.BaseInfo.UUID == uuid)
				{
					return bossSubject;
				}
			}
		}
		return null;
	}

	public ThemeTaskDetail GetThemeTaskInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.ThemeTaskList != null)
		{
			foreach (ThemeTaskDetail themeTask in zbServerData.sGameData.OperateInfo.ThemeTaskList)
			{
				if (IsShowTime(themeTask.BaseInfo) && themeTask.BaseInfo.UUID == uuid)
				{
					return themeTask;
				}
			}
		}
		return null;
	}

	public RestaurantDetail GetRestaurantByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.RestaurantList != null)
		{
			foreach (RestaurantDetail restaurant in zbServerData.sGameData.OperateInfo.RestaurantList)
			{
				if (IsShowTime(restaurant.BaseInfo) && restaurant.BaseInfo.UUID == uuid)
				{
					return restaurant;
				}
			}
		}
		return null;
	}

	public SnowVeilDetail GetSnowVeilByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.SnowVeilList != null)
		{
			foreach (SnowVeilDetail snowVeil in zbServerData.sGameData.OperateInfo.SnowVeilList)
			{
				if (IsShowTime(snowVeil.BaseInfo) && snowVeil.BaseInfo.UUID == uuid)
				{
					return snowVeil;
				}
			}
		}
		return null;
	}

	public InfiniteGiftDetail GetInfiniteGiftInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.InfiniteGiftList != null)
		{
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (IsShowTime(infiniteGift.BaseInfo) && infiniteGift.BaseInfo.UUID == uuid)
				{
					return infiniteGift;
				}
			}
		}
		return null;
	}

	public InfiniteGiftGoodsDetail GetInfiniteGiftCurGood(string uuid)
	{
		InfiniteGiftDetail infiniteGiftInfoByUUID = GetInfiniteGiftInfoByUUID(uuid);
		if (infiniteGiftInfoByUUID != null)
		{
			foreach (InfiniteGiftGoodsDetail goods in infiniteGiftInfoByUUID.GoodsList)
			{
				if (goods.State == ECommonState.Yes)
				{
					return goods;
				}
			}
			foreach (InfiniteGiftGoodsDetail goods2 in infiniteGiftInfoByUUID.GoodsList)
			{
				if (goods2.State == ECommonState.No)
				{
					return goods2;
				}
			}
		}
		return null;
	}

	public (OperateTargetDetail lastTarget, OperateTargetDetail curTarget, bool isFinal) GetInfiniteGiftLastCurTarget(string uuid)
	{
		InfiniteGiftDetail infiniteGiftInfoByUUID = GetInfiniteGiftInfoByUUID(uuid);
		OperateTargetDetail item = null;
		if (infiniteGiftInfoByUUID != null)
		{
			foreach (OperateTargetDetail target in infiniteGiftInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return (lastTarget: item, curTarget: target, isFinal: false);
				}
				item = target;
			}
			item = null;
			foreach (OperateTargetDetail target2 in infiniteGiftInfoByUUID.TargetList)
			{
				if (target2.State == ECommonState.No)
				{
					return (lastTarget: item, curTarget: target2, isFinal: false);
				}
				item = target2;
			}
			int count = infiniteGiftInfoByUUID.TargetList.Count;
			return (lastTarget: infiniteGiftInfoByUUID.TargetList[count - 1], curTarget: item, isFinal: true);
		}
		return (lastTarget: null, curTarget: null, isFinal: false);
	}

	public (OperateTargetDetail curTarget, bool isFinal) GetFusionLastCurTarget()
	{
		if (zbServerData.sGameData.OperateInfo?.FusionInfo != null)
		{
			FusionInfo fusionInfo = zbServerData.sGameData.OperateInfo.FusionInfo;
			if (fusionInfo.FusionBase != null)
			{
				OperateTargetDetail operateTargetDetail;
				foreach (OperateTargetDetail target in fusionInfo.FusionBase.TargetList)
				{
					if (target.State == ECommonState.Yes)
					{
						return (curTarget: target, isFinal: false);
					}
					operateTargetDetail = target;
				}
				operateTargetDetail = null;
				foreach (OperateTargetDetail target2 in fusionInfo.FusionBase.TargetList)
				{
					if (target2.State == ECommonState.No)
					{
						return (curTarget: target2, isFinal: false);
					}
					operateTargetDetail = target2;
				}
				return (curTarget: operateTargetDetail, isFinal: true);
			}
		}
		return (curTarget: null, isFinal: false);
	}

	public LimitTimeRouletteDetail GetLimitTimeRouletteInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitTimeRouletteList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeRoulette in zbServerData.sGameData.OperateInfo.LimitTimeRouletteList)
			{
				if (IsShowTime(limitTimeRoulette.BaseInfo) && limitTimeRoulette.BaseInfo.UUID == uuid)
				{
					return limitTimeRoulette;
				}
			}
		}
		return null;
	}

	public LimitTimeRouletteDetail GetLimitTimeTreasureInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitTimeTreasureList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeTreasure in zbServerData.sGameData.OperateInfo.LimitTimeTreasureList)
			{
				if (IsShowTime(limitTimeTreasure.BaseInfo) && limitTimeTreasure.BaseInfo.UUID == uuid)
				{
					return limitTimeTreasure;
				}
			}
		}
		return null;
	}

	public LimitTimeRouletteDetail GetLimitTimeVendorInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitTimeVendorList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeVendor in zbServerData.sGameData.OperateInfo.LimitTimeVendorList)
			{
				if (IsShowTime(limitTimeVendor.BaseInfo) && limitTimeVendor.BaseInfo.UUID == uuid)
				{
					return limitTimeVendor;
				}
			}
		}
		return null;
	}

	public LimitTimeCatcherDetail GetLimitTimeCatcherInfoByUUID(string uuid)
	{
		if (zbServerData.sGameData.OperateInfo?.LimitTimeCatcherList != null)
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (IsShowTime(limitTimeCatcher.BaseInfo) && limitTimeCatcher.BaseInfo.UUID == uuid)
				{
					return limitTimeCatcher;
				}
			}
		}
		return null;
	}

	public ZhanLingKillDetail GetZhanLingKillDetailByUUID(string uuid = null)
	{
		if (zbServerData.sGameData.OperateInfo?.ZhanLingKillList != null)
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (IsShowTime(zhanLingKill.BaseInfo) && (string.IsNullOrEmpty(uuid) || zhanLingKill.BaseInfo.UUID == uuid))
				{
					return zhanLingKill;
				}
			}
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
			(uuidRechargeDaily, HasRechargeDailyGift),
			(uuidRechargeDailySum, HasRechargeDailySumGift),
			(uuidRechargeSum, HasRechargeSumGift),
			(uuidConsumeDailySum, HasConsumeDailySumGift),
			(uuidConsumeSum, HasConsumeSumGift),
			(uuidLimitTimeShop, HasLimitTimeShopGift),
			(uuidLimitVipHero, HasLimitVipHeroGift),
			(uuidLoginSum, HasLoginSumGift),
			(uuidDailyActive, HasDailyActivieGift),
			(uuidBarDraw, HasBarDrawGift),
			(uuidPropExchange, HasExchangeGift),
			(uuidScratchCard, HasScratchCardGift),
			(uuidJiZi, HasJiZiGift),
			(uuidCipherBox, HasCipherBoxGift),
			(uuidTimeCatcher, HasTimeCatcherNotify),
			(uuidRedBag, HasBarrageCmdGift),
			(uuidPrayLantern, HasPrayLanternGift),
			(uuidResourceExchange, HasResourceExchangeGift),
			(uuidHolidayTask, HasHolidayTaskGift),
			(uuidInfiniteGift, HasInfiniteGift),
			(uuidLimitTimeRoulette, HasLimitTimeRouletteGift),
			(uuidLimitTimeTreasure, HasLimitTimeTreasure),
			(uuidLimitTimeVendor, HasLimitTimeVendor),
			(uuidZhanLingKill, HasZhanLingKillNotify)
		};
		if (HasLimitTimeHeroGift() || HasFusionGift() || zbSelf.Instance.activityHandler.HasDailySignNotify() || HasTiliNotify())
		{
			return true;
		}
		foreach (var (list2, predicate) in list)
		{
			if (list2 != null && list2.Any(predicate))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRechargeDailyGift(string uuid)
	{
		RechargeDailyDetail rechargeDailyByUUID = GetRechargeDailyByUUID(uuid);
		if (rechargeDailyByUUID != null)
		{
			if (m_firstRechargeDaily)
			{
				return true;
			}
			foreach (RechargeTargetDetail target in rechargeDailyByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRechargeDailySumGift(string uuid)
	{
		RechargeSumDetail rechargeDailySumInfoByUUID = GetRechargeDailySumInfoByUUID(uuid);
		if (rechargeDailySumInfoByUUID != null)
		{
			if (m_firstRechargeDailySum)
			{
				return true;
			}
			foreach (RechargeTargetDetail target in rechargeDailySumInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRechargeSumGift(string uuid)
	{
		RechargeSumDetail rechargeSumInfoByUUID = GetRechargeSumInfoByUUID(uuid);
		if (rechargeSumInfoByUUID != null)
		{
			if (m_firstRechargeSum)
			{
				return true;
			}
			foreach (RechargeTargetDetail target in rechargeSumInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasConsumeSumGift(string uuid)
	{
		ConsumeSumDetail consumeSumInfoByUUID = GetConsumeSumInfoByUUID(uuid);
		if (consumeSumInfoByUUID != null)
		{
			if (m_firstConsumeSum)
			{
				return true;
			}
			foreach (ConsumeTargetDetail target in consumeSumInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasConsumeDailySumGift(string uuid)
	{
		ConsumeSumDetail consumeDailySumInfoByUUID = GetConsumeDailySumInfoByUUID(uuid);
		if (consumeDailySumInfoByUUID != null)
		{
			if (m_firstConsumeSum)
			{
				return true;
			}
			foreach (ConsumeTargetDetail target in consumeDailySumInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLimitTimeShopGift(string uuid)
	{
		if (GetTimeShopInfoByUUID(uuid) != null && m_firstLimitTimeShopList)
		{
			return true;
		}
		return false;
	}

	public bool HasLimitVipHeroGift(string uuid)
	{
		LimitVipHeroDetail vipHeroInfoByUUID = GetVipHeroInfoByUUID(uuid);
		if (vipHeroInfoByUUID != null)
		{
			foreach (OperateTargetDetail target in vipHeroInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLoginSumGift(string uuid)
	{
		LoginSumDetail loginSumInfoByUUID = GetLoginSumInfoByUUID(uuid);
		if (loginSumInfoByUUID != null)
		{
			foreach (OperateTargetDetail target in loginSumInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasDailyActivieGift(string uuid)
	{
		DailyActiveDetail activeInfoByUUID = GetActiveInfoByUUID(uuid);
		if (activeInfoByUUID != null && zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active >= 50 && activeInfoByUUID.State == ECommonState.Yes)
		{
			return true;
		}
		return false;
	}

	public bool HasBarDrawGift(string uuid)
	{
		BarDrawDetail barDrawInfoByUUID = GetBarDrawInfoByUUID(uuid);
		if (barDrawInfoByUUID != null)
		{
			foreach (OperateTargetDetail target in barDrawInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasExchangeGift(string uuid)
	{
		PropExchangeDetail exchangeInfoByUUID = GetExchangeInfoByUUID(uuid);
		if (exchangeInfoByUUID == null)
		{
			return false;
		}
		foreach (OperateTargetDetail target in exchangeInfoByUUID.TargetList)
		{
			if (target.State == ECommonState.Yes)
			{
				return true;
			}
		}
		long countByPropTid = zbSelf.Instance.itemHandler.GetCountByPropTid(exchangeInfoByUUID.PropId);
		foreach (OperatePropExchangeDetail exchange in exchangeInfoByUUID.ExchangeList)
		{
			string key = $"PropExchange{zbSelf.Instance.GetRoleId()}{exchangeInfoByUUID.BaseInfo.UUID}{exchange.Index}";
			if (zbGameSave.Instance.GetInt(key) != 2 && (exchange.LimitNum <= 0 || exchange.Count < exchange.LimitNum) && countByPropTid >= exchange.PropCost)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasScratchCardGift(string uuid)
	{
		ScratchCardDetail scratchCardInfoByUUID = GetScratchCardInfoByUUID(uuid);
		if (scratchCardInfoByUUID != null)
		{
			if (zbResourceHelper.GetPropCountByType(scratchCardInfoByUUID.PropId) > 0)
			{
				return true;
			}
			foreach (OperateScratchCardDetail target in scratchCardInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasJiZiGift(string uuid)
	{
		JiZiDetail jiZiYouLiByUUID = GetJiZiYouLiByUUID(uuid);
		if (jiZiYouLiByUUID != null)
		{
			foreach (OperateJiZiExchangeDetail exchange in jiZiYouLiByUUID.ExchangeList)
			{
				string key = $"Jizi{zbSelf.Instance.GetRoleId()}{jiZiYouLiByUUID.BaseInfo.UUID}{exchange.Index}";
				int num = zbGameSave.Instance.GetInt(key);
				if ((num != 1 && num != 0) || exchange.Count >= exchange.LimitNum)
				{
					continue;
				}
				bool flag = true;
				foreach (int prop in exchange.PropList)
				{
					if (zbSelf.Instance.itemHandler.GetCountByPropTid(prop) == 0L)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasCipherBoxGift(string uuid)
	{
		CipherBoxDetail cipherBoxInfoByUUID = GetCipherBoxInfoByUUID(uuid);
		if (cipherBoxInfoByUUID != null)
		{
			return cipherBoxInfoByUUID.Count < cipherBoxInfoByUUID.LimitNum;
		}
		return false;
	}

	public long GetDirectGiftLeftTime(string uuid)
	{
		RechargeGiftDetail rechargeGiftByUUID = GetRechargeGiftByUUID(uuid);
		if (rechargeGiftByUUID != null)
		{
			return rechargeGiftByUUID.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
		}
		return -1L;
	}

	public int GetTimeHeroCount()
	{
		int num = 0;
		if (zbServerData.sGameData.OperateInfo.LimitTimeHeroList != null)
		{
			foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
			{
				if (IsShowTime(limitTimeHero.BaseInfo))
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool HasTimeHeroBarGift(string uuid)
	{
		return zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 31, 1);
	}

	public bool HasTimeHeroScoreGift(string uuid)
	{
		LimitTimeHeroDetail timeHeroInfoByUUID = GetTimeHeroInfoByUUID(uuid);
		if (timeHeroInfoByUUID != null)
		{
			foreach (LimitTimeHeroScoreDetail score in timeHeroInfoByUUID.ScoreList)
			{
				if (HasTimeHeroScoreGiftByIndex(uuid, score.Index))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTimeHeroScoreGiftByIndex(string uuid, int index)
	{
		LimitTimeHeroDetail timeHeroInfoByUUID = GetTimeHeroInfoByUUID(uuid);
		if (zbSelf.Instance.newTagHandler.IsNotNotifyBarScore(index))
		{
			return false;
		}
		foreach (LimitTimeHeroScoreDetail score in timeHeroInfoByUUID.ScoreList)
		{
			if (index == score.Index && (score.LimitNum == 0 || score.Count < score.LimitNum) && score.Score <= timeHeroInfoByUUID.BaseData.LeftScore)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasLimitTimeHeroGift(bool isHoliday = false)
	{
		foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
		{
			if (((isHoliday && limitTimeHero.BaseInfo.Type == EOperatePlanType.Holiday) || (!isHoliday && limitTimeHero.BaseInfo.Type != EOperatePlanType.Holiday)) && IsShowTime(limitTimeHero.BaseInfo) && (HasTimeHeroBarGift(limitTimeHero.BaseInfo.UUID) || HasTimeHeroScoreGift(limitTimeHero.BaseInfo.UUID)))
			{
				return true;
			}
		}
		return false;
	}

	public int GetHolidayTaskToday(string uuid)
	{
		HolidayTaskDetail holidayTaskInfoByUUID = GetHolidayTaskInfoByUUID(uuid);
		if (holidayTaskInfoByUUID != null)
		{
			return Mathf.FloorToInt((float)(zbSelf.Instance.timeHandler.GetServerTime() - holidayTaskInfoByUUID.BaseInfo.StartTm) / 86400f);
		}
		return 0;
	}

	public bool HasTaskGiftByDay(string uuid, int day)
	{
		HolidayTaskDetail holidayTaskInfoByUUID = GetHolidayTaskInfoByUUID(uuid);
		if (holidayTaskInfoByUUID != null)
		{
			if (day > GetHolidayTaskToday(uuid))
			{
				return false;
			}
			foreach (TaskDetail task in holidayTaskInfoByUUID.TaskList)
			{
				zbHolidayTaskTaskCsvRecord recordByKey = zbDataConfig.Instance.holidayTaskTaskCsvTable.GetRecordByKey(task.TaskId);
				if (recordByKey != null && recordByKey.day == day + 1 && task.State == EActiveState.Active)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTaskGift(string uuid)
	{
		if (GetHolidayTaskInfoByUUID(uuid) != null)
		{
			for (int i = 0; i < 6; i++)
			{
				if (HasTaskGiftByDay(uuid, i))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLevelGift(string uuid)
	{
		HolidayTaskDetail holidayTaskInfoByUUID = GetHolidayTaskInfoByUUID(uuid);
		if (holidayTaskInfoByUUID != null)
		{
			int num = Math.Min(zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecords().records.Count, holidayTaskInfoByUUID.Level);
			if (num > holidayTaskInfoByUUID.LowAwardLevel)
			{
				for (int i = holidayTaskInfoByUUID.LowAwardLevel + 1; i <= num; i++)
				{
					zbHolidayTaskLevelCsvRecord recordByKey = zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecordByKey(i);
					if (recordByKey != null && recordByKey.awardLow.Length != 0)
					{
						return true;
					}
				}
			}
			if (holidayTaskInfoByUUID.ActiveState == 1 && num > holidayTaskInfoByUUID.HighAwardLevel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasScoreExchangeGiftByIndex(string uuid, int index)
	{
		HolidayTaskDetail holidayTaskInfoByUUID = GetHolidayTaskInfoByUUID(uuid);
		if ((holidayTaskInfoByUUID == null && holidayTaskInfoByUUID.ActiveState == 0) || holidayTaskInfoByUUID.Level < zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecords().records.Count)
		{
			return false;
		}
		if (zbSelf.Instance.newTagHandler.IsNotNotifyHolidayTask(index))
		{
			return false;
		}
		foreach (HolidayTaskExchangeDetail exchange in holidayTaskInfoByUUID.ExchangeList)
		{
			if (index == exchange.Index && (exchange.LimitCount == 0 || exchange.ExchangeCount < exchange.LimitCount) && (holidayTaskInfoByUUID.TotalScore - holidayTaskInfoByUUID.ExchangeScore) / exchange.ScoreCost >= 1)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasScoreExchangeGift(string uuid)
	{
		HolidayTaskDetail holidayTaskInfoByUUID = GetHolidayTaskInfoByUUID(uuid);
		if (holidayTaskInfoByUUID == null)
		{
			return false;
		}
		foreach (HolidayTaskExchangeDetail exchange in holidayTaskInfoByUUID.ExchangeList)
		{
			if (HasScoreExchangeGiftByIndex(uuid, exchange.Index))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHolidayTaskGift(string uuid)
	{
		if (!HasTaskGift(uuid) && !HasLevelGift(uuid))
		{
			return HasScoreExchangeGift(uuid);
		}
		return true;
	}

	public bool HasFusionGift(bool isHoliday = false)
	{
		if (zbServerData.sGameData.OperateInfo != null && zbServerData.sGameData.OperateInfo.FusionInfo != null)
		{
			FusionInfo fusionInfo = zbServerData.sGameData.OperateInfo.FusionInfo;
			if (!IsShowTime(fusionInfo.BaseInfo))
			{
				return false;
			}
			if (HasFusionTargetNotify(isHoliday))
			{
				return true;
			}
			if ((isHoliday && fusionInfo.BaseInfo.Type == EOperatePlanType.Holiday) || (!isHoliday && fusionInfo.BaseInfo.Type != EOperatePlanType.Holiday))
			{
				foreach (OperateTargetDetail dailyAward in fusionInfo.FusionBase.DailyAwardList)
				{
					if (fusionInfo.FusionBase.DailyBestScore >= dailyAward.Target && dailyAward.State != ECommonState.End)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasFusionTargetNotify(bool isHoliday = false)
	{
		if (zbServerData.sGameData.OperateInfo != null && zbServerData.sGameData.OperateInfo.FusionInfo != null)
		{
			FusionInfo fusionInfo = zbServerData.sGameData.OperateInfo.FusionInfo;
			if (!IsShowTime(fusionInfo.BaseInfo))
			{
				return false;
			}
			if ((isHoliday && fusionInfo.BaseInfo.Type == EOperatePlanType.Holiday) || (!isHoliday && fusionInfo.BaseInfo.Type != EOperatePlanType.Holiday))
			{
				foreach (OperateTargetDetail target in fusionInfo.FusionBase.TargetList)
				{
					if (fusionInfo.FusionBase.ActScore >= target.Target && target.State != ECommonState.End)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public (int count, int allCount) GetTargetProgress(string uuid, int floor)
	{
		int num = 0;
		int num2 = 0;
		zbPrayLanternAwardCsvTableData records = zbDataConfig.Instance.prayLanternAwardCsvTable.GetRecords();
		foreach (KeyValuePair<int, zbPrayLanternAwardCsvRecord> record in records.records)
		{
			if (record.Value.floor == floor)
			{
				num2++;
			}
		}
		PrayLanternDetail prayLanternInfoByUUID = GetPrayLanternInfoByUUID(uuid);
		if (prayLanternInfoByUUID != null)
		{
			foreach (int unlock in prayLanternInfoByUUID.UnlockList)
			{
				int num3 = -1;
				foreach (KeyValuePair<int, zbPrayLanternAwardCsvRecord> record2 in records.records)
				{
					if (record2.Value.id == unlock)
					{
						num3 = record2.Value.floor;
						break;
					}
				}
				if (num3 == floor)
				{
					num++;
				}
			}
		}
		return (count: num, allCount: num2);
	}

	public bool IsUnlock(string uuid, int id)
	{
		PrayLanternDetail prayLanternInfoByUUID = GetPrayLanternInfoByUUID(uuid);
		if (prayLanternInfoByUUID != null)
		{
			foreach (int unlock in prayLanternInfoByUUID.UnlockList)
			{
				if (unlock == id)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsFloorGray(string uuid, int _floor)
	{
		PrayLanternDetail detail = GetPrayLanternInfoByUUID(uuid);
		if (detail == null || _floor == 1)
		{
			return false;
		}
		return !(from record in zbDataConfig.Instance.prayLanternAwardCsvTable.GetRecords().records.Values
			where record.floor == _floor - 1
			select record.id).ToList().All((int id) => detail.UnlockList.Contains(id));
	}

	public int GetMaxDrawId(string uuid, int[] idList)
	{
		int num = 1;
		for (int i = 1; i <= 3; i++)
		{
			var (num2, num3) = GetTargetProgress(uuid, i);
			if (num2 >= num3)
			{
				num = Math.Min(i + 1, 3);
			}
		}
		zbPrayLanternAwardCsvTableData records = zbDataConfig.Instance.prayLanternAwardCsvTable.GetRecords();
		foreach (int num4 in idList)
		{
			int num5 = -1;
			foreach (KeyValuePair<int, zbPrayLanternAwardCsvRecord> record in records.records)
			{
				if (record.Value.id == num4)
				{
					num5 = record.Value.floor;
					break;
				}
			}
			if (num5 > num)
			{
				num = num5;
			}
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, zbPrayLanternAwardCsvRecord> record2 in records.records)
		{
			if (record2.Value.floor == num)
			{
				list.Add(record2.Value.id);
			}
		}
		int num6 = 1;
		foreach (int item in list)
		{
			if (item > num6)
			{
				num6 = item;
			}
		}
		return num6;
	}

	public UltimateAwardDetail GetUltimateAward(string uuid)
	{
		PrayLanternDetail prayLanternInfoByUUID = GetPrayLanternInfoByUUID(uuid);
		if (prayLanternInfoByUUID != null)
		{
			foreach (UltimateAwardDetail ultimateAward in prayLanternInfoByUUID.Ultimate.UltimateAwardList)
			{
				if (ultimateAward.Selected == 1)
				{
					return ultimateAward;
				}
			}
		}
		return null;
	}

	public bool HasPrayLanternGift(string uuid)
	{
		PrayLanternDetail prayLanternInfoByUUID = GetPrayLanternInfoByUUID(uuid);
		if (prayLanternInfoByUUID == null)
		{
			return false;
		}
		if (prayLanternInfoByUUID.DailyFree == EBoolState.False || prayLanternInfoByUUID.Ultimate.State == ECommonState.Yes)
		{
			return true;
		}
		foreach (OperateTargetDetail target in prayLanternInfoByUUID.TargetList)
		{
			if (target.State == ECommonState.Yes)
			{
				return true;
			}
		}
		if (prayLanternInfoByUUID.Ultimate.State == ECommonState.No && zbSelf.Instance.moneyHandler.HasEnoughMoney(34, 1L))
		{
			return true;
		}
		return false;
	}

	public bool HasResourceExchangeGift(string uuid)
	{
		ResourceExchangeDetail resourceExchangeInfoByUUID = GetResourceExchangeInfoByUUID(uuid);
		if (resourceExchangeInfoByUUID == null)
		{
			return false;
		}
		foreach (ExchangeItemDetail exchange in resourceExchangeInfoByUUID.ExchangeList)
		{
			string key = $"ResourceExchange{zbSelf.Instance.GetRoleId()}{resourceExchangeInfoByUUID.BaseInfo.UUID}{exchange.Index}";
			if (zbGameSave.Instance.GetInt(key) == 2 || (exchange.Limit > 0 && exchange.Count >= exchange.Limit))
			{
				continue;
			}
			bool flag = true;
			foreach (AwardDetail src in exchange.SrcList)
			{
				if (zbSelf.Instance.itemHandler.GetCountByItemTid((int)src.Tid) < src.Count)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasInfiniteGift(string uuid)
	{
		InfiniteGiftDetail infiniteGiftInfoByUUID = GetInfiniteGiftInfoByUUID(uuid);
		if (infiniteGiftInfoByUUID != null)
		{
			if (infiniteGiftInfoByUUID.DailyFreeState == EBoolState.False)
			{
				return true;
			}
			foreach (InfiniteGiftGoodsDetail goods in infiniteGiftInfoByUUID.GoodsList)
			{
				if (goods.State == ECommonState.Yes && goods.GoodsId == 0)
				{
					return true;
				}
			}
			foreach (OperateTargetDetail target in infiniteGiftInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLimitTimeRouletteGift(string uuid)
	{
		LimitTimeRouletteDetail limitTimeRouletteInfoByUUID = GetLimitTimeRouletteInfoByUUID(uuid);
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, 1L))
		{
			return true;
		}
		if (limitTimeRouletteInfoByUUID != null)
		{
			foreach (OperateTargetDetail target in limitTimeRouletteInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLimitTimeTreasure(string uuid)
	{
		LimitTimeRouletteDetail limitTimeTreasureInfoByUUID = GetLimitTimeTreasureInfoByUUID(uuid);
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, limitTimeTreasureInfoByUUID.BaseData.OnceCost))
		{
			return true;
		}
		if (limitTimeTreasureInfoByUUID != null)
		{
			foreach (OperateTargetDetail target in limitTimeTreasureInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasLimitTimeVendor(string uuid)
	{
		LimitTimeRouletteDetail limitTimeVendorInfoByUUID = GetLimitTimeVendorInfoByUUID(uuid);
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, 1L))
		{
			return true;
		}
		if (limitTimeVendorInfoByUUID != null)
		{
			foreach (OperateTargetDetail target in limitTimeVendorInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTimeCatcherNotify(string uuid)
	{
		LimitTimeCatcherDetail limitTimeCatcherInfoByUUID = GetLimitTimeCatcherInfoByUUID(uuid);
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(33, 1L))
		{
			return true;
		}
		if (limitTimeCatcherInfoByUUID.CoinCount > 0)
		{
			return true;
		}
		if (limitTimeCatcherInfoByUUID.AwardState == ECommonState.Yes)
		{
			return true;
		}
		return false;
	}

	public bool HasThemeTaskNotify(string uuid)
	{
		ThemeTaskDetail themeTaskInfoByUUID = GetThemeTaskInfoByUUID(uuid);
		if (themeTaskInfoByUUID != null)
		{
			foreach (TaskDetail task in themeTaskInfoByUUID.TaskList)
			{
				if (task.State == EActiveState.Active)
				{
					return true;
				}
			}
			foreach (OperateTargetDetail target in themeTaskInfoByUUID.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasZhanLingKillNotify(string uuid)
	{
		ZhanLingKillDetail zhanLingKillDetailByUUID = GetZhanLingKillDetailByUUID(uuid);
		if (zhanLingKillDetailByUUID != null)
		{
			if (zhanLingKillDetailByUUID.ZhanLingInfo.Level > zhanLingKillDetailByUUID.ZhanLingInfo.LowAward)
			{
				return true;
			}
			if (zhanLingKillDetailByUUID.ZhanLingInfo.Active == EBoolState.True && zhanLingKillDetailByUUID.ZhanLingInfo.Level > zhanLingKillDetailByUUID.ZhanLingInfo.HighAward)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTiliNotify()
	{
		if (zbServerData.sGameData.ActivityInfo != null && zbServerData.sGameData.ActivityInfo.TiliBank != null)
		{
			foreach (TiliBankDetail tili in zbServerData.sGameData.ActivityInfo.TiliBank.TiliList)
			{
				if (tili.TiliTm < zbSelf.Instance.timeHandler.GetServerTime())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRechargeGiftNotify(int _freeType)
	{
		if (zbServerData.sGameData.FreeGiftList != null)
		{
			foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
			{
				if (freeGift.FreeType == _freeType)
				{
					return freeGift.State == EBoolState.False;
				}
			}
		}
		return false;
	}

	public bool IsNovicePackPurchased()
	{
		NovicePackInfo novicePackInfo = GetNovicePackInfo();
		if (novicePackInfo != null)
		{
			return novicePackInfo.State == EBoolState.True;
		}
		return false;
	}

	public int GetNovicePackClaimedDays()
	{
		return GetNovicePackInfo()?.Count ?? 0;
	}

	public EActiveState GetNovicePackTodayState()
	{
		return GetNovicePackInfo()?.TodayState ?? EActiveState.Pending;
	}

	public bool IsNovicePackClaimableToday()
	{
		NovicePackInfo novicePackInfo = GetNovicePackInfo();
		if (novicePackInfo != null && novicePackInfo.State == EBoolState.True && novicePackInfo.TodayState == EActiveState.Active)
		{
			return novicePackInfo.Count < novicePackInfo.Total;
		}
		return false;
	}

	public bool IsNovicePackCompleted()
	{
		NovicePackInfo novicePackInfo = GetNovicePackInfo();
		if (novicePackInfo != null && novicePackInfo.State == EBoolState.True && novicePackInfo.Total > 0)
		{
			return novicePackInfo.Count >= novicePackInfo.Total;
		}
		return false;
	}

	public int GetNovicePackGoodsId()
	{
		JObject record = zbDataConfig.Instance.vipBaseJsonTable.GetRecord();
		if (!record.ContainsKey("novicePack"))
		{
			return 0;
		}
		return (int)record["novicePack"][(object)"goodsId"];
	}

	private NovicePackInfo GetNovicePackInfo()
	{
		return zbServerData.sGameData.RechargeActiveInfo?.NovicePackInfo;
	}
}
