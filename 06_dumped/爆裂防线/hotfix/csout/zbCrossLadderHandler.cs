using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbCrossLadderHandler
{
	public int buyLimit;

	public int buyDiamond;

	public int victory3;

	public int maxLadderId;

	public int playerId;

	public CrossLadderInfo lastCrossLadderInfo;

	public bool isUpgradeAck;

	public CrossLadderFriendDetail[] crossLadderFriendRankList;

	public void Init()
	{
		JObject record = zbDataConfig.Instance.ladderBaseJsonTable.GetRecord();
		buyLimit = Extensions.Value<int>((IEnumerable<JToken>)record["buyLimit"]);
		buyDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["buyDiamond"]);
		victory3 = Extensions.Value<int>((IEnumerable<JToken>)record["score"][(object)"victory3"]);
		zbLadderCsvRecord[] array = zbDataConfig.Instance.ladderCsvTable.GetRecords().records.Values.ToArray();
		maxLadderId = ((array != null && array.Length != 0) ? array.Max((zbLadderCsvRecord r) => r.id) : 0);
	}

	public int GetBuyLimit()
	{
		if (ValidateHasCrossLadderCard())
		{
			zbVipCrossLadderCsvRecord recordByKey = zbDataConfig.Instance.vipCrossLadderCsvTable.GetRecordByKey(4);
			if (recordByKey != null)
			{
				return buyLimit + recordByKey.val;
			}
		}
		return buyLimit;
	}

	public bool IsCrossLadderOpen(bool needBubble = false)
	{
		if (zbServerData.sGameData.CrossLadderInfo != null && zbServerData.sGameData.CrossLadderInfo.OpenInfo != null)
		{
			long num = zbServerData.sGameData.CrossLadderInfo.OpenInfo.StartTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				if (needBubble)
				{
					zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10886"), zbUtil.FormatTimeSmart(num)));
				}
				return false;
			}
			if (zbSelf.Instance.timeHandler.GetServerTime() - zbServerData.sGameData.CrossLadderInfo.OpenInfo.EndTm > 0)
			{
				if (needBubble)
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10943"));
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public long GetNextOpenLeftTime()
	{
		if (!IsCrossLadderOpen() && zbServerData.sGameData.CrossLadderInfo != null && zbServerData.sGameData.CrossLadderInfo.OpenInfo != null && zbSelf.Instance.timeHandler.GetServerTime() > zbServerData.sGameData.CrossLadderInfo.OpenInfo.EndTm)
		{
			return zbServerData.sGameData.CrossLadderInfo.OpenInfo.EndTm + 140400 - zbSelf.Instance.timeHandler.GetServerTime();
		}
		return 0L;
	}

	public void UpdateCrossLadderInfo(CrossLadderInfo crossLadderInfo)
	{
		lastCrossLadderInfo = zbServerData.sGameData.CrossLadderInfo;
		zbServerData.sGameData.CrossLadderInfo = crossLadderInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_CrossLadderInfo);
	}

	public void UpdateCrossLadderFriendRankList(CrossLadderFriendDetail[] rankList)
	{
		crossLadderFriendRankList = rankList;
		SortRankList(crossLadderFriendRankList);
	}

	public static void SortRankList(CrossLadderFriendDetail[] rankList)
	{
		if (rankList != null && rankList.Length > 1)
		{
			Array.Sort(rankList, CompareRank);
		}
	}

	private static int CompareRank(CrossLadderFriendDetail a, CrossLadderFriendDetail b)
	{
		int num = b.LadderId.CompareTo(a.LadderId);
		if (num != 0)
		{
			return num;
		}
		num = b.RoleBaseInfo.TeamAttack.CompareTo(a.RoleBaseInfo.TeamAttack);
		if (num != 0)
		{
			return num;
		}
		return a.RoleBaseInfo.RoleId.CompareTo(b.RoleBaseInfo.RoleId);
	}

	public int[][] GetLadderTargetAwards(int ladderId)
	{
		zbLadderTargetAwardCsvRecord recordByKey = zbDataConfig.Instance.ladderTargetAwardCsvTable.GetRecordByKey(ladderId);
		if (recordByKey != null)
		{
			return zbServerData.sGameData.CrossLadderInfo.OpenInfo.Month switch
			{
				1 => recordByKey.month1, 
				2 => recordByKey.month2, 
				3 => recordByKey.month3, 
				4 => recordByKey.month4, 
				5 => recordByKey.month5, 
				6 => recordByKey.month6, 
				7 => recordByKey.month7, 
				8 => recordByKey.month8, 
				9 => recordByKey.month9, 
				10 => recordByKey.month10, 
				11 => recordByKey.month11, 
				12 => recordByKey.month12, 
				_ => recordByKey.month1, 
			};
		}
		return Array.Empty<int[]>();
	}

	public string GetScoreChangeDesc()
	{
		string text = zbUtil.Language("lang.10900") + ": " + lastCrossLadderInfo.AchieveInfo.Score;
		string text2 = "";
		foreach (zbLadderCsvRecord value in zbDataConfig.Instance.ladderCsvTable.GetRecords().records.Values)
		{
			if (value.score == zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score && value.score > 0)
			{
				text2 = " (MAX)";
				break;
			}
		}
		int num = zbServerData.sGameData.CrossLadderInfo.AchieveInfo.Score - lastCrossLadderInfo.AchieveInfo.Score;
		if (num == 0)
		{
			return text + " <color=#C50000>-0" + text2 + "</color>";
		}
		if (num > 0)
		{
			return text + $" <color=#2DF421>+{num}{text2}</color>";
		}
		return text + $" <color=#C50000>{num}{text2}</color>";
	}

	public bool ValidateHasCrossLadderCard()
	{
		int goodsId = zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["vipCard"][(object)"crossLadder"].Value<int>((object)"goodsId");
		VipCardShopDetail vipCardShopDetail = zbSelf.Instance.vipHandler.GetVipCardShopDetail(goodsId);
		if (vipCardShopDetail != null)
		{
			return vipCardShopDetail.ExpireTm > zbSelf.Instance.timeHandler.GetServerTime();
		}
		return false;
	}

	public VipCardShopDetail GetCrossLadderCardDetail()
	{
		int goodsId = zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["vipCard"][(object)"crossLadder"].Value<int>((object)"goodsId");
		return zbSelf.Instance.vipHandler.GetVipCardShopDetail(goodsId);
	}

	public bool HasFreePackRedDot()
	{
		foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
		{
			if (freeGift.FreeType == 7 && freeGift.State == EBoolState.False && !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.CrossLadder))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTargetAwardNotify()
	{
		if (IsCrossLadderOpen() && zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId > zbServerData.sGameData.CrossLadderInfo.SelfInfo.TargetAward && zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId >= 2)
		{
			return true;
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.CrossLadder) || !IsCrossLadderOpen())
		{
			return false;
		}
		if (zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.LadderRecord))
		{
			return true;
		}
		if (zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId < zbSelf.Instance.crossLadderHandler.maxLadderId)
		{
			if (zbServerData.sGameData.CrossLadderInfo.SelfInfo.DailyCount > zbServerData.sGameData.CrossLadderInfo.SelfInfo.CombatCount)
			{
				return true;
			}
		}
		else if (zbServerData.sGameData.CrossPeakSelfInfo.DailyCount > zbServerData.sGameData.CrossPeakSelfInfo.CombatCount)
		{
			return true;
		}
		if (!HasFreePackRedDot() && !HasTargetAwardNotify())
		{
			return zbServerData.sGameData.CrossLadderInfo.SelfInfo.SeasonAward.State == EBoolState.False;
		}
		return true;
	}
}
