using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbArenaHandler
{
	public ArenaCombatDetail[] combatList;

	public int buyLimit;

	public int buyDiamond;

	public int rankIndex;

	public int playerId;

	public bool isCombatRecord;

	public ArenaRecordDetail[] arenaRecords;

	public RankDetail rankDetail;

	public int lastRank;

	public int lastBestRank;

	public string saveShowLegionWarEffectKey = "ArenaEffect" + zbSelf.Instance.GetRoleId();

	public void Init()
	{
		JObject record = zbDataConfig.Instance.arenaBaseJsonTable.GetRecord();
		buyLimit = Extensions.Value<int>((IEnumerable<JToken>)record["buyLimit"]);
		buyDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["buyDiamond"]);
		saveShowLegionWarEffectKey = "ArenaEffect" + zbSelf.Instance.GetRoleId();
	}

	public void UpdateArenaRankTop(RankTopAck ack)
	{
		rankDetail = ((IEnumerable<RankTopDetail>)ack.RankTopList).FirstOrDefault((RankTopDetail r) => r.RankType == ERankType.Arena)?.RankInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ArenaRankTop);
	}

	public void UpdateArenaInfo(ArenaInfo arenaInfo)
	{
		lastRank = zbServerData.sGameData.ArenaInfo.Rank;
		lastBestRank = zbServerData.sGameData.ArenaInfo.BestRank;
		zbServerData.sGameData.ArenaInfo = arenaInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ArenaInfo);
	}

	public void UpdateCombatList(ArenaCombatDetail[] list)
	{
		combatList = list;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ArenaCombatList);
	}

	public int GetDailyArenaCoin(int rank)
	{
		zbArenaDailyAwardCsvRecord[] records = zbDataConfig.Instance.arenaDailyAwardCsvTable.GetRecords().records;
		foreach (zbArenaDailyAwardCsvRecord zbArenaDailyAwardCsvRecord2 in records)
		{
			if (rank <= zbArenaDailyAwardCsvRecord2.rank)
			{
				return zbArenaDailyAwardCsvRecord2.rankMoney;
			}
		}
		return 0;
	}

	public zbArenaGiftCsvRecord[] GetArenaGiftRecordsByLayer(int layer)
	{
		return (from r in zbDataConfig.Instance.arenaGiftCsvTable.GetRecords().records.Values
			where r.layer == layer
			orderby r.layer
			select r).ToArray();
	}

	public int GetArenaGiftSaleCount(int giftId)
	{
		foreach (ArenaGiftDetail gift in zbServerData.sGameData.ArenaInfo.GiftList)
		{
			if (giftId == gift.GiftId)
			{
				return gift.SaleCount;
			}
		}
		return 0;
	}

	public void NeedArenaEffect()
	{
		if (!zbGuide.Instance.IsGuiding() && !zbUtil.IsSameDay(zbGameSave.Instance.GetLong(saveShowLegionWarEffectKey, 0L), zbSelf.Instance.timeHandler.GetServerTime()))
		{
			zbWsMsgHandler.Instance.Arena_RankList_Req(openWnd: false);
		}
	}

	public void ShowArenaEffect(RankDetail[] list)
	{
		if (!zbGuide.Instance.IsGuiding() && !zbUtil.IsSameDay(zbGameSave.Instance.GetLong(saveShowLegionWarEffectKey, 0L), zbSelf.Instance.timeHandler.GetServerTime()) && list.Count() >= 3)
		{
			zbGameSave.Instance.SetLong(saveShowLegionWarEffectKey, zbSelf.Instance.timeHandler.GetServerTime());
			zbUIMgr.Instance.ShowArenaEffect(list);
		}
	}

	public bool HasFightCountNotify()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena))
		{
			return false;
		}
		return zbServerData.sGameData.ArenaInfo.DailyCount > zbServerData.sGameData.ArenaInfo.CombatCount;
	}

	public bool HasGiftNotifyById(int giftId)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena))
		{
			return false;
		}
		foreach (zbArenaGiftCsvRecord value in zbDataConfig.Instance.arenaGiftCsvTable.GetRecords().records.Values)
		{
			if (giftId == value.id && value.price == 0 && zbServerData.sGameData.ArenaInfo.BestRank <= value.layer && (value.limitNum == 0 || GetArenaGiftSaleCount(value.id) < value.limitNum))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasGiftNotify()
	{
		foreach (zbArenaGiftCsvRecord value in zbDataConfig.Instance.arenaGiftCsvTable.GetRecords().records.Values)
		{
			if (HasGiftNotifyById(value.id))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!HasFightCountNotify() && !HasGiftNotify())
		{
			return zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.ArenaRecord);
		}
		return true;
	}

	public bool NeedGuidPvpMod()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena))
		{
			return false;
		}
		PvpInfo pvpInfo = zbServerData.sGameData.PvpInfo;
		if (pvpInfo == null)
		{
			return false;
		}
		if (pvpInfo.ValidModInfo.SelectList.Count <= 0)
		{
			return pvpInfo.SetModInfo.SkillList.Count <= 0;
		}
		return false;
	}

	public bool NeedCombatResume()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena))
		{
			return false;
		}
		PvpInfo pvpInfo = zbServerData.sGameData.PvpInfo;
		if (pvpInfo != null && pvpInfo.SetModInfo.SkillList.Count > 0)
		{
			if (pvpInfo.ValidModInfo.SelectList.Count <= 0)
			{
				zbUIMgr.Instance.ShowMessageConfirm(zbUtil.Language("lang.11476"), delegate
				{
					zbUIMgr.Instance.ShowPvpModSelect();
				});
				return true;
			}
			zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11471"), OnMsgPvpConfirm, zbUtil.Language("lang.11472"), zbUtil.Language("lang.10873"));
			return true;
		}
		return false;
	}

	private void OnMsgPvpConfirm(bool isOK)
	{
		if (isOK)
		{
			zbUIMgr.Instance.ShowPvpModSelect();
		}
		else
		{
			zbWsMsgHandler.Instance.PvpSet_ModCancel_Req();
		}
	}
}
