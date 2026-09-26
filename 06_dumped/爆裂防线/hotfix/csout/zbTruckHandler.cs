using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbTruckHandler
{
	public int dailyTransportBuyLimit;

	public int dailyTransportBuyDiamond;

	public int dailyRobBuyLimit;

	public long dailyRobBuyDiamond;

	public int unlockDiamond;

	public int unlockGoodsId;

	public int refreshLimit;

	public JArray refreshDiamond;

	public TruckMatchDetail[] MatchList;

	public int currTransportId;

	public int currPlayerId;

	public TruckTransportInfo[] TruckTransportInfoList = new TruckTransportInfo[0];

	public TruckRobRecordDetail[] RobRecordList;

	public bool bFromBalance;

	public bool bBestPrize;

	public TrainDetail currRobTrain;

	public bool openTaker;

	public TrainInfo trainInfo { get; set; }

	public void Init()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		JObject record = zbDataConfig.Instance.truckBaseJsonTable.GetRecord();
		dailyTransportBuyLimit = Extensions.Value<int>((IEnumerable<JToken>)record["dailyTransportBuyLimit"]);
		dailyTransportBuyDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["dailyTransportBuyDiamond"]);
		dailyRobBuyLimit = Extensions.Value<int>((IEnumerable<JToken>)record["dailyRobBuyLimit"]);
		dailyRobBuyDiamond = Extensions.Value<long>((IEnumerable<JToken>)record["dailyRobBuyDiamond"]);
		refreshLimit = Extensions.Value<int>((IEnumerable<JToken>)record["truckRefresh"][(object)"refreshNum"]);
		refreshDiamond = (JArray)record["truckRefresh"][(object)"refreshDiamond"];
		unlockDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["truckUnlockDiamond"]);
	}

	public int GetRefreshDiamond(int refreshCount)
	{
		if (((JContainer)zbSelf.Instance.truckHandler.refreshDiamond).Count <= refreshCount)
		{
			return Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.truckHandler.refreshDiamond[((JContainer)zbSelf.Instance.truckHandler.refreshDiamond).Count - 1]);
		}
		return Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.truckHandler.refreshDiamond[refreshCount]);
	}

	public int GetTransportRefreshLimit()
	{
		return refreshLimit;
	}

	public int GetTransportCombatLimit()
	{
		return zbServerData.sGameData.TruckSelfInfo.TransportLimit;
	}

	public int GetTransportBuyLimit()
	{
		return dailyTransportBuyLimit;
	}

	public int GetRobCombatLimit()
	{
		return zbServerData.sGameData.TruckSelfInfo.RobLimit;
	}

	public int GetRobBuyLimit()
	{
		return dailyRobBuyLimit;
	}

	public bool IsTruckOpen()
	{
		if (zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.StartTm > 0 && zbSelf.Instance.timeHandler.GetServerTime() >= zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.StartTm)
		{
			return true;
		}
		return false;
	}

	public TruckTransportDetail GetTruckRobDetail(int TransportId)
	{
		TruckMatchDetail[] matchList = MatchList;
		foreach (TruckMatchDetail truckMatchDetail in matchList)
		{
			if (truckMatchDetail.TransportInfo.TransportId == TransportId)
			{
				return truckMatchDetail.TransportInfo;
			}
		}
		return null;
	}

	public TruckDetail GetTruckDetail(int truckId)
	{
		foreach (TruckDetail truck in zbServerData.sGameData.TruckSelfInfo.TruckList)
		{
			if (truck.TruckId == truckId)
			{
				return truck;
			}
		}
		return null;
	}

	public void UpdateTruckSelfInfo(TruckSelfInfo _TruckSelfInfo)
	{
		zbServerData.sGameData.TruckSelfInfo = _TruckSelfInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_TruckSelfInfo);
	}

	public VipCardShopDetail GetCrossLadderCardDetail()
	{
		int goodsId = (int)zbDataConfig.Instance.truckBaseJsonTable.GetRecord()["truckSlot"][(object)"unlockGoodsId"];
		return zbSelf.Instance.vipHandler.GetVipCardShopDetail(goodsId);
	}

	public void RefreshItems(zbItem[] m_items, AwardDetail[] awardList, int[] robList)
	{
		int num = 0;
		foreach (AwardDetail awardDetail in awardList)
		{
			zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
			m_items[num].Refresh(data);
			m_items[num].SetGray(robList.Contains(num));
			num++;
		}
	}

	public bool SelfTrainRedPoint()
	{
		if (zbSelf.Instance.truckHandler.trainInfo == null)
		{
			return false;
		}
		if (zbSelf.Instance.truckHandler.trainInfo.SelfTrain.TransportId == 0 && zbSelf.Instance.moneyHandler.HasEnoughMoney(38, 1L))
		{
			return true;
		}
		if (zbSelf.Instance.truckHandler.trainInfo.SelfTrain.TransportId > 0 && zbSelf.Instance.truckHandler.trainInfo.SelfTrain.ArriveTm <= zbSelf.Instance.timeHandler.GetServerTime())
		{
			return true;
		}
		return false;
	}

	public bool CanTruckGetAward()
	{
		foreach (TruckDetail truck in zbServerData.sGameData.TruckSelfInfo.TruckList)
		{
			if (truck.TransportInfo.TransportId > 0 && truck.TransportInfo.ArriveTm <= zbSelf.Instance.timeHandler.GetServerTime())
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveTruckListTransport()
	{
		foreach (TruckDetail truck in zbServerData.sGameData.TruckSelfInfo.TruckList)
		{
			if (truck.TransportInfo.TransportId == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveTruckTransport()
	{
		if (zbServerData.sGameData.TruckSelfInfo.TransportLimit <= zbServerData.sGameData.TruckSelfInfo.TransportUsed)
		{
			return false;
		}
		return HaveTruckListTransport();
	}

	public bool HaveTruckRuning()
	{
		foreach (TruckDetail truck in zbServerData.sGameData.TruckSelfInfo.TruckList)
		{
			if (truck.TransportInfo.TransportId > 0 && truck.TransportInfo.ArriveTm > zbSelf.Instance.timeHandler.GetServerTime())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFreePackRedDot()
	{
		foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
		{
			if (freeGift.FreeType == 10 && freeGift.State == EBoolState.False && !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Truck))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint(bool isArenaMain = true)
	{
		if (!IsTruckOpen() || zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Truck))
		{
			return false;
		}
		if (isArenaMain && zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			return false;
		}
		if (zbSelf.Instance.journeyTwoHandler.HasZhanLingNotify())
		{
			return true;
		}
		if (!CanTruckGetAward() && !HaveTruckTransport() && !HasFreePackRedDot())
		{
			return zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.TruckRecord);
		}
		return true;
	}
}
