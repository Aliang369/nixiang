using System;
using System.Collections.Generic;
using System.Linq;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.WebSockets;
using Best.WebSockets.Implementations;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Proto.Enums;
using Proto.Messages;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbWsMsgHandler : MonoBehaviour
{
	private bool openArenaWnd;

	private bool m_showGuide;

	private bool openArenaRankWnd;

	private int tempBoxTid;

	private int tempSelectCount;

	private EBarType m_barType;

	private EFuncType UnLockType;

	private bool needFirst;

	private bool openCrossLadderWnd;

	private bool openCrossLadderRankWnd;

	private int lastLadderId;

	private bool openCrossPeakWnd;

	private bool openEndlessHorde;

	private bool openEndlessMineWnd;

	private bool m_openFacilityWnd;

	private bool m_needExploreAnim;

	private bool m_openCombatWnd;

	private bool m_openLockedChest;

	private int m_openFootholdWnd = -1;

	private int curStarFootholdId;

	private bool openMonster;

	private int openMonsterTab;

	private int heroTid;

	private int nextMod;

	private ESkillType openHoleSkillType;

	private ESkillType refreshHoleSkillType;

	private bool m_openIdleWnd;

	private bool openLegendary;

	private bool m_bOpenLegionWnd;

	private bool m_bOpenLegionMembersWnd;

	private bool m_autoReqPointInfo;

	private bool m_bOpenMailWnd;

	private int tempMechaId;

	private int resetMechaId;

	private bool openOperate;

	private bool openHolidayTaskWnd;

	private int tempExp;

	private bool openRankWnd;

	private bool openSeasonAirdropWnd;

	public string upgradeQualityServantId;

	public ServantDetail upgradeQualityServantDetail;

	public EBoolState servantIsLock;

	private int curStarSuitId;

	private int curStarGunId;

	private bool openTreasureWnd;

	private int openTreasureTabIndex;

	private bool openTruckMainWnd;

	private bool openTruckRecordWnd;

	private int openTruckTabIndex;

	private int openTruckId;

	private bool openFromTask;

	private bool openWall;

	private int openWallTabIndex;

	private int curStarWallId;

	private bool openWorld;

	private bool openWorldBoss;

	private bool m_openChallengeWnd;

	public static zbWsMsgHandler Instance;

	private Dictionary<Message, Action<ByteString>> m_messageCallbacks = new Dictionary<Message, Action<ByteString>>();

	private WebSocket m_webSocket;

	[NonSerialized]
	public string m_wsUrl;

	private int m_SequenceId = 1;

	private bool m_needCreateAndConnectSocket;

	private float m_needCreateAndConnectSocketTime = -1f;

	public void Activity_Data_Req()
	{
		ActivityDataReq req = new ActivityDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ActivityDataReq);
	}

	public void Activity_Data_Ack(ByteString response)
	{
		ActivityDataAck activityDataAck = ActivityDataAck.Parser.ParseFrom(response);
		if (activityDataAck != null)
		{
			zbServerData.sGameData.ActivityInfo = activityDataAck.ActivityInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Activity_Sign_Req()
	{
		ActivitySignReq req = new ActivitySignReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ActivitySignReq);
	}

	public void Activity_Sign_Ack(ByteString response)
	{
		ActivitySignAck activitySignAck = ActivitySignAck.Parser.ParseFrom(response);
		if (activitySignAck != null)
		{
			zbServerData.sGameData.ActivityInfo = activitySignAck.ActivityInfo;
			zbSelf.Instance.dataHandler.DataChange(activitySignAck.DataChange, ((IEnumerable<AwardDetail>)activitySignAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Activity_SignTargetAward_Req()
	{
		ActivitySignTargetAwardReq req = new ActivitySignTargetAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ActivitySignTargetAwardReq);
	}

	public void Activity_SignTargetAward_Ack(ByteString response)
	{
		ActivitySignTargetAwardAck activitySignTargetAwardAck = ActivitySignTargetAwardAck.Parser.ParseFrom(response);
		if (activitySignTargetAwardAck != null)
		{
			zbServerData.sGameData.ActivityInfo = activitySignTargetAwardAck.ActivityInfo;
			zbSelf.Instance.dataHandler.DataChange(activitySignTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)activitySignTargetAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Activity_TiliBankAward_Req(long tiliTm)
	{
		ActivityTiliBankAwardReq activityTiliBankAwardReq = new ActivityTiliBankAwardReq();
		activityTiliBankAwardReq.TiliTm = tiliTm;
		Instance.WsPackMsg((IMessage)(object)activityTiliBankAwardReq, Message.ActivityTiliBankAwardReq);
	}

	public void Activity_TiliBankAward_Ack(ByteString response)
	{
		ActivityTiliBankAwardAck activityTiliBankAwardAck = ActivityTiliBankAwardAck.Parser.ParseFrom(response);
		if (activityTiliBankAwardAck != null)
		{
			zbServerData.sGameData.ActivityInfo = activityTiliBankAwardAck.ActivityInfo;
			zbSelf.Instance.dataHandler.DataChange(activityTiliBankAwardAck.DataChange, ((IEnumerable<AwardDetail>)activityTiliBankAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Activity_TiliBankAwardAll_Req()
	{
		ActivityTiliBankAwardAllReq req = new ActivityTiliBankAwardAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ActivityTiliBankAwardAllReq);
	}

	public void Activity_TiliBankAwardAll_Ack(ByteString response)
	{
		ActivityTiliBankAwardAllAck activityTiliBankAwardAllAck = ActivityTiliBankAwardAllAck.Parser.ParseFrom(response);
		if (activityTiliBankAwardAllAck != null)
		{
			zbServerData.sGameData.ActivityInfo = activityTiliBankAwardAllAck.ActivityInfo;
			zbSelf.Instance.dataHandler.DataChange(activityTiliBankAwardAllAck.DataChange, ((IEnumerable<AwardDetail>)activityTiliBankAwardAllAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Activity_BubbleGiftRand_Req()
	{
		ActivityBubbleGiftRandReq req = new ActivityBubbleGiftRandReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ActivityBubbleGiftRandReq);
	}

	public void Activity_BubbleGiftRand_Ack(ByteString response)
	{
		ActivityBubbleGiftRandAck activityBubbleGiftRandAck = ActivityBubbleGiftRandAck.Parser.ParseFrom(response);
		if (activityBubbleGiftRandAck != null)
		{
			zbServerData.sGameData.ActivityInfo = activityBubbleGiftRandAck.ActivityInfo;
			zbSelf.Instance.dataHandler.DataChange(activityBubbleGiftRandAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Activity_BubbleGiftBuy_Req(int id)
	{
		ActivityBubbleGiftBuyReq activityBubbleGiftBuyReq = new ActivityBubbleGiftBuyReq();
		activityBubbleGiftBuyReq.Id = id;
		Instance.WsPackMsg((IMessage)(object)activityBubbleGiftBuyReq, Message.ActivityBubbleGiftBuyReq);
	}

	public void Activity_BubbleGiftBuy_Ack(ByteString response)
	{
		ActivityBubbleGiftBuyAck activityBubbleGiftBuyAck = ActivityBubbleGiftBuyAck.Parser.ParseFrom(response);
		if (activityBubbleGiftBuyAck != null)
		{
			zbServerData.sGameData.ActivityInfo = activityBubbleGiftBuyAck.ActivityInfo;
			zbSelf.Instance.dataHandler.DataChange(activityBubbleGiftBuyAck.DataChange, ((IEnumerable<AwardDetail>)activityBubbleGiftBuyAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Arena_Data_Req(bool openWnd = false, bool showGuide = false)
	{
		ArenaDataReq req = new ArenaDataReq();
		openArenaWnd = openWnd;
		m_showGuide = showGuide;
		Instance.WsPackMsg((IMessage)(object)req, Message.ArenaDataReq);
	}

	public void Arena_Data_Ack(ByteString response)
	{
		ArenaDataAck arenaDataAck = ArenaDataAck.Parser.ParseFrom(response);
		if (arenaDataAck == null)
		{
			return;
		}
		zbSelf.Instance.arenaHandler.UpdateArenaInfo(arenaDataAck.ArenaInfo);
		if (!openArenaWnd)
		{
			return;
		}
		zbUIMgr.Instance.ShowArena();
		if (m_showGuide)
		{
			zbArenaWnd zbArenaWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Arena) as zbArenaWnd;
			if ((Object)(object)zbArenaWnd2 != (Object)null)
			{
				zbArenaWnd2.ShowGuide();
			}
		}
	}

	public void Arena_CombatList_Req()
	{
		ArenaCombatListReq req = new ArenaCombatListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ArenaCombatListReq);
	}

	public void Arena_CombatList_Ack(ByteString response)
	{
		ArenaCombatListAck arenaCombatListAck = ArenaCombatListAck.Parser.ParseFrom(response);
		if (arenaCombatListAck != null)
		{
			zbSelf.Instance.arenaHandler.UpdateArenaInfo(arenaCombatListAck.ArenaInfo);
			zbSelf.Instance.arenaHandler.UpdateCombatList(((IEnumerable<ArenaCombatDetail>)arenaCombatListAck.CombatList).ToArray());
		}
	}

	public void Arena_FightStart_Req(int PlayerId, int Rank)
	{
		ArenaFightStartReq arenaFightStartReq = new ArenaFightStartReq();
		arenaFightStartReq.PlayerId = PlayerId;
		arenaFightStartReq.Rank = Rank;
		Instance.WsPackMsg((IMessage)(object)arenaFightStartReq, Message.ArenaFightStartReq);
	}

	public void Arena_FightStart_Ack(ByteString response)
	{
		ArenaFightStartAck arenaFightStartAck = ArenaFightStartAck.Parser.ParseFrom(response);
		if (arenaFightStartAck != null)
		{
			zbUtil.Debug2LogFile("Arena_FightStart_Ack.txt", arenaFightStartAck);
			zbSelf.Instance.arenaHandler.playerId = arenaFightStartAck.PlayerId;
			zbSelf.Instance.arenaHandler.rankIndex = arenaFightStartAck.Rank;
			zbSelf.Instance.combatHandler.StartShowCombat(arenaFightStartAck.BattleInfo);
		}
	}

	public void Arena_FightResult_Req(int PlayerId, int Rank, BattleResultInfo BattleResult)
	{
		ArenaFightResultReq arenaFightResultReq = new ArenaFightResultReq();
		arenaFightResultReq.PlayerId = PlayerId;
		arenaFightResultReq.Rank = Rank;
		arenaFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)arenaFightResultReq, Message.ArenaFightResultReq);
	}

	public void Arena_FightResult_Ack(ByteString response)
	{
		ArenaFightResultAck arenaFightResultAck = ArenaFightResultAck.Parser.ParseFrom(response);
		if (arenaFightResultAck != null)
		{
			zbUtil.Debug2LogFile("Arena_FightResult_Ack.txt", arenaFightResultAck);
			zbSelf.Instance.arenaHandler.UpdateArenaInfo(arenaFightResultAck.ArenaInfo);
			zbCombatSystem.Instance.battleAwardList = arenaFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(arenaFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(arenaFightResultAck.BattleSettle);
		}
	}

	public void Arena_BuyCombat_Req(int Count)
	{
		ArenaBuyCombatReq arenaBuyCombatReq = new ArenaBuyCombatReq();
		arenaBuyCombatReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)arenaBuyCombatReq, Message.ArenaBuyCombatReq);
	}

	public void Arena_BuyCombat_Ack(ByteString response)
	{
		ArenaBuyCombatAck arenaBuyCombatAck = ArenaBuyCombatAck.Parser.ParseFrom(response);
		if (arenaBuyCombatAck != null)
		{
			zbSelf.Instance.arenaHandler.UpdateArenaInfo(arenaBuyCombatAck.ArenaInfo);
			zbSelf.Instance.dataHandler.DataChange(arenaBuyCombatAck.DataChange);
		}
	}

	public void Arena_SaoDang_Req()
	{
		ArenaSaoDangReq req = new ArenaSaoDangReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ArenaSaoDangReq);
	}

	public void Arena_SaoDang_Ack(ByteString response)
	{
		ArenaSaoDangAck arenaSaoDangAck = ArenaSaoDangAck.Parser.ParseFrom(response);
		if (arenaSaoDangAck != null)
		{
			zbSelf.Instance.arenaHandler.UpdateArenaInfo(arenaSaoDangAck.ArenaInfo);
			zbSelf.Instance.dataHandler.DataChange(arenaSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)arenaSaoDangAck.AwardList).ToArray());
		}
	}

	public void Arena_RecordList_Req()
	{
		ArenaRecordListReq req = new ArenaRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ArenaRecordListReq);
	}

	public void Arena_RecordList_Ack(ByteString response)
	{
		ArenaRecordListAck arenaRecordListAck = ArenaRecordListAck.Parser.ParseFrom(response);
		if (arenaRecordListAck != null)
		{
			zbSelf.Instance.arenaHandler.arenaRecords = ((IEnumerable<ArenaRecordDetail>)arenaRecordListAck.RecordList).ToArray();
			zbUIMgr.Instance.ShowArenaRecordWnd(((IEnumerable<ArenaRecordDetail>)arenaRecordListAck.RecordList).ToArray());
		}
	}

	public void Arena_BattleResult_Req(string UUID)
	{
		ArenaBattleResultReq arenaBattleResultReq = new ArenaBattleResultReq();
		arenaBattleResultReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)arenaBattleResultReq, Message.ArenaBattleResultReq);
	}

	public void Arena_BattleResult_Ack(ByteString response)
	{
		ArenaBattleResultAck arenaBattleResultAck = ArenaBattleResultAck.Parser.ParseFrom(response);
		if (arenaBattleResultAck != null)
		{
			zbSelf.Instance.arenaHandler.isCombatRecord = true;
			zbSelf.Instance.combatHandler.StartShowCombat(arenaBattleResultAck.BattleInfo, 0, null, arenaBattleResultAck.BattleSettle);
		}
	}

	public void Arena_RankList_Req(bool openWnd)
	{
		openArenaRankWnd = openWnd;
		ArenaRankListReq req = new ArenaRankListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ArenaRankListReq);
	}

	public void Arena_RankList_Ack(ByteString response)
	{
		ArenaRankListAck arenaRankListAck = ArenaRankListAck.Parser.ParseFrom(response);
		if (arenaRankListAck != null)
		{
			zbSelf.Instance.rankHandler.arenaRankListAck = arenaRankListAck;
			if (openArenaRankWnd)
			{
				openArenaRankWnd = false;
				zbUIMgr.Instance.ShowRank(1);
			}
			else if (!zbGuide.Instance.IsGuiding())
			{
				zbSelf.Instance.arenaHandler.ShowArenaEffect(((IEnumerable<RankDetail>)arenaRankListAck.RankList).ToArray());
			}
		}
	}

	public void Arena_GiftBuy_Req(int GiftId)
	{
		ArenaGiftBuyReq arenaGiftBuyReq = new ArenaGiftBuyReq();
		arenaGiftBuyReq.GiftId = GiftId;
		Instance.WsPackMsg((IMessage)(object)arenaGiftBuyReq, Message.ArenaGiftBuyReq);
	}

	public void Arena_GiftBuy_Ack(ByteString response)
	{
		ArenaGiftBuyAck arenaGiftBuyAck = ArenaGiftBuyAck.Parser.ParseFrom(response);
		if (arenaGiftBuyAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(arenaGiftBuyAck.DataChange, ((IEnumerable<AwardDetail>)arenaGiftBuyAck.AwardList).ToArray());
			zbSelf.Instance.arenaHandler.UpdateArenaInfo(arenaGiftBuyAck.ArenaInfo);
		}
	}

	public void Bag_ItemSale_Req(AwardDetail[] SaleList)
	{
		BagItemSaleReq bagItemSaleReq = new BagItemSaleReq();
		bagItemSaleReq.SaleList.Clear();
		bagItemSaleReq.SaleList.AddRange((IEnumerable<AwardDetail>)SaleList);
		Instance.WsPackMsg((IMessage)(object)bagItemSaleReq, Message.BagItemSaleReq);
	}

	public void Bag_ItemSale_Ack(ByteString response)
	{
		BagItemSaleAck bagItemSaleAck = BagItemSaleAck.Parser.ParseFrom(response);
		if (bagItemSaleAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(bagItemSaleAck.DataChange, ((IEnumerable<AwardDetail>)bagItemSaleAck.AwardList).ToArray());
		}
	}

	public void Bag_LimitItemSale_Req()
	{
		BagLimitItemSaleReq req = new BagLimitItemSaleReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.BagLimitItemSaleReq);
	}

	public void Bag_LimitItemSale_Ack(ByteString response)
	{
		BagLimitItemSaleAck bagLimitItemSaleAck = BagLimitItemSaleAck.Parser.ParseFrom(response);
		if (bagLimitItemSaleAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(bagLimitItemSaleAck.DataChange, ((IEnumerable<AwardDetail>)bagLimitItemSaleAck.AwardList).ToArray());
		}
	}

	public void Bag_ItemCompose_Req(int ItemTid, int Count)
	{
		BagItemComposeReq bagItemComposeReq = new BagItemComposeReq();
		bagItemComposeReq.ItemTid = ItemTid;
		bagItemComposeReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)bagItemComposeReq, Message.BagItemComposeReq);
	}

	public void Bag_ItemCompose_Ack(ByteString response)
	{
		BagItemComposeAck bagItemComposeAck = BagItemComposeAck.Parser.ParseFrom(response);
		if (bagItemComposeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(bagItemComposeAck.DataChange, ((IEnumerable<AwardDetail>)bagItemComposeAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Bag_ItemCompose, bagItemComposeAck);
		}
	}

	public void Bag_RandBoxOpen_Req(int BoxTid, int Count)
	{
		BagRandBoxOpenReq bagRandBoxOpenReq = new BagRandBoxOpenReq();
		bagRandBoxOpenReq.BoxTid = BoxTid;
		bagRandBoxOpenReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)bagRandBoxOpenReq, Message.BagRandBoxOpenReq);
	}

	public void Bag_RandBoxOpen_Ack(ByteString response)
	{
		BagRandBoxOpenAck bagRandBoxOpenAck = BagRandBoxOpenAck.Parser.ParseFrom(response);
		if (bagRandBoxOpenAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(bagRandBoxOpenAck.DataChange, ((IEnumerable<AwardDetail>)bagRandBoxOpenAck.AwardList).ToArray());
		}
	}

	public void Bag_SelectBoxView_Req(int BoxTid)
	{
		BagSelectBoxViewReq bagSelectBoxViewReq = new BagSelectBoxViewReq();
		bagSelectBoxViewReq.BoxTid = BoxTid;
		tempBoxTid = BoxTid;
		Instance.WsPackMsg((IMessage)(object)bagSelectBoxViewReq, Message.BagSelectBoxViewReq);
	}

	public void Bag_SelectBoxView_Ack(ByteString response)
	{
		BagSelectBoxViewAck bagSelectBoxViewAck = BagSelectBoxViewAck.Parser.ParseFrom(response);
		if (bagSelectBoxViewAck != null)
		{
			tempSelectCount = bagSelectBoxViewAck.SelectList.Count;
			zbUIMgr.Instance.ShowBoxOpen(((IEnumerable<AwardDetail>)bagSelectBoxViewAck.SelectList).ToArray(), tempBoxTid);
		}
	}

	public int GetLastSelectBoxOptionCount()
	{
		return tempSelectCount;
	}

	public void Bag_SelectBoxOpen_Req(int BoxTid, SelectDetail[] OpenList)
	{
		BagSelectBoxOpenReq bagSelectBoxOpenReq = new BagSelectBoxOpenReq();
		bagSelectBoxOpenReq.BoxTid = BoxTid;
		bagSelectBoxOpenReq.OpenList.Clear();
		bagSelectBoxOpenReq.OpenList.AddRange((IEnumerable<SelectDetail>)OpenList);
		Instance.WsPackMsg((IMessage)(object)bagSelectBoxOpenReq, Message.BagSelectBoxOpenReq);
	}

	public void Bag_SelectBoxOpen_Ack(ByteString response)
	{
		BagSelectBoxOpenAck bagSelectBoxOpenAck = BagSelectBoxOpenAck.Parser.ParseFrom(response);
		if (bagSelectBoxOpenAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(bagSelectBoxOpenAck.DataChange, ((IEnumerable<AwardDetail>)bagSelectBoxOpenAck.AwardList).ToArray());
		}
	}

	public void Bag_ComposeBoxOpen_Req(int BoxTid, int Count)
	{
		BagComposeBoxOpenReq bagComposeBoxOpenReq = new BagComposeBoxOpenReq();
		bagComposeBoxOpenReq.BoxTid = BoxTid;
		bagComposeBoxOpenReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)bagComposeBoxOpenReq, Message.BagComposeBoxOpenReq);
	}

	public void Bag_ComposeBoxOpen_Ack(ByteString response)
	{
		BagComposeBoxOpenAck bagComposeBoxOpenAck = BagComposeBoxOpenAck.Parser.ParseFrom(response);
		if (bagComposeBoxOpenAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(bagComposeBoxOpenAck.DataChange, ((IEnumerable<AwardDetail>)bagComposeBoxOpenAck.AwardList).ToArray());
		}
	}

	public void Bag_GeneComposeCalc_Req(EGeneType GeneType, EGeneShape GeneShape, int composeIndex)
	{
		BagGeneComposeCalcReq bagGeneComposeCalcReq = new BagGeneComposeCalcReq();
		bagGeneComposeCalcReq.GeneType = GeneType;
		bagGeneComposeCalcReq.GeneShape = GeneShape;
		zbSelf.Instance.geneHandler.composeIndex = composeIndex;
		Instance.WsPackMsg((IMessage)(object)bagGeneComposeCalcReq, Message.BagGeneComposeCalcReq);
	}

	public void Bag_GeneComposeCalc_Ack(ByteString response)
	{
		BagGeneComposeCalcAck bagGeneComposeCalcAck = BagGeneComposeCalcAck.Parser.ParseFrom(response);
		if (bagGeneComposeCalcAck != null)
		{
			zbUIMgr.Instance.ShowGeneGeneComposeCalc(((IEnumerable<AwardDetail>)bagGeneComposeCalcAck.AwardList).ToArray(), zbSelf.Instance.geneHandler.composeIndex);
			zbSelf.Instance.geneHandler.composeIndex = -1;
		}
	}

	public void Bag_GeneCompose_Req(BagSelectDetail[] SelectList, int composeIndex)
	{
		BagGeneComposeReq bagGeneComposeReq = new BagGeneComposeReq();
		bagGeneComposeReq.SelectList.Clear();
		bagGeneComposeReq.SelectList.AddRange((IEnumerable<BagSelectDetail>)SelectList);
		zbSelf.Instance.geneHandler.composeIndex = composeIndex;
		Instance.WsPackMsg((IMessage)(object)bagGeneComposeReq, Message.BagGeneComposeReq);
	}

	public void Bag_GeneCompose_Ack(ByteString response)
	{
		BagGeneComposeAck bagGeneComposeAck = BagGeneComposeAck.Parser.ParseFrom(response);
		if (bagGeneComposeAck != null)
		{
			if (bagGeneComposeAck.AwardList.Count > 0)
			{
				AwardDetail awardDetail = bagGeneComposeAck.AwardList[0];
				zbSelf.Instance.geneHandler.composeAckGeneTid = (int)awardDetail.Tid;
			}
			if (zbSelf.Instance.geneHandler.composeIndex == -1)
			{
				zbSelf.Instance.dataHandler.DataChange(bagGeneComposeAck.DataChange);
			}
			else
			{
				zbSelf.Instance.dataHandler.DataChange(bagGeneComposeAck.DataChange, ((IEnumerable<AwardDetail>)bagGeneComposeAck.AwardList).ToArray());
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Bag_GeneCompose_Ack, bagGeneComposeAck);
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		}
	}

	public void Bar_Data_Req()
	{
		BarDataReq req = new BarDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.BarDataReq);
	}

	public void Bar_Data_Ack(ByteString response)
	{
		BarDataAck barDataAck = BarDataAck.Parser.ParseFrom(response);
		if (barDataAck != null)
		{
			zbServerData.sGameData.BarInfo = barDataAck.BarInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Bar);
		}
	}

	public void Bar_Draw_Req(EBarType BarType, int Count)
	{
		BarDrawReq barDrawReq = new BarDrawReq();
		barDrawReq.BarType = BarType;
		barDrawReq.Count = Count;
		m_barType = BarType;
		Instance.WsPackMsg((IMessage)(object)barDrawReq, Message.BarDrawReq);
		switch (BarType)
		{
		case EBarType.Normal:
			zbProgressEventHelper.Instance.SetProgressTrack(616);
			break;
		case EBarType.Special:
			zbProgressEventHelper.Instance.SetProgressTrack(617);
			break;
		}
	}

	public void Bar_Draw_Ack(ByteString response)
	{
		BarDrawAck barDrawAck = BarDrawAck.Parser.ParseFrom(response);
		if (barDrawAck != null)
		{
			if (m_barType == EBarType.Treasure || m_barType == EBarType.TreasurePlus)
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Treasure_Bar_Draw_Ack, barDrawAck);
				return;
			}
			zbServerData.sGameData.BarInfo = barDrawAck.BarInfo;
			zbUIMgr.Instance.ShowBar(m_barType, ((IEnumerable<AwardDetail>)barDrawAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Bar);
			zbSelf.Instance.dataHandler.DataChange(barDrawAck.DataChange);
			zbAudioMgr.Instance.PlayAudio("audio/ui/bar.ogg");
		}
	}

	public void Bar_TargetAward_Req(EBarType BarType)
	{
		BarTargetAwardReq barTargetAwardReq = new BarTargetAwardReq();
		barTargetAwardReq.BarType = BarType;
		m_barType = BarType;
		Instance.WsPackMsg((IMessage)(object)barTargetAwardReq, Message.BarTargetAwardReq);
	}

	public void Bar_TargetAward_Ack(ByteString response)
	{
		BarTargetAwardAck barTargetAwardAck = BarTargetAwardAck.Parser.ParseFrom(response);
		if (barTargetAwardAck != null)
		{
			zbServerData.sGameData.BarInfo = barTargetAwardAck.BarInfo;
			zbSelf.Instance.dataHandler.DataChange(barTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)barTargetAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Bar);
		}
	}

	public void Operate_BossSubData_Req()
	{
		OperateBossSubDataReq req = new OperateBossSubDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateBossSubDataReq);
	}

	public void Operate_BossSubData_Ack(ByteString response)
	{
		OperateBossSubDataAck operateBossSubDataAck = OperateBossSubDataAck.Parser.ParseFrom(response);
		if (operateBossSubDataAck != null)
		{
			zbServerData.sGameData.OperateInfo.BossSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.BossSubjectList.AddRange((IEnumerable<BossSubjectDetail>)operateBossSubDataAck.BossSubjectList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_BossSubject_Ack);
		}
	}

	public void Operate_BossSubFightStart_Req(string uuid, int BossLevel)
	{
		OperateBossSubFightStartReq operateBossSubFightStartReq = new OperateBossSubFightStartReq();
		operateBossSubFightStartReq.UUID = uuid;
		operateBossSubFightStartReq.BossLevel = BossLevel;
		Instance.WsPackMsg((IMessage)(object)operateBossSubFightStartReq, Message.OperateBossSubFightStartReq);
	}

	public void Operate_BossSubFightStart_Ack(ByteString response)
	{
		OperateBossSubFightStartAck operateBossSubFightStartAck = OperateBossSubFightStartAck.Parser.ParseFrom(response);
		if (operateBossSubFightStartAck != null)
		{
			zbSelf.Instance.bossSubjectHandler.currUUID = operateBossSubFightStartAck.UUID;
			zbSelf.Instance.bossSubjectHandler.currBossLevel = operateBossSubFightStartAck.BossLevel;
			zbSelf.Instance.bossSubjectHandler.currDamageAdd = operateBossSubFightStartAck.DamageAdd;
			zbSelf.Instance.combatHandler.StartShowCombat(operateBossSubFightStartAck.BattleInfo);
		}
	}

	public void Operate_BossSubFightResult_Req(string uuid, int BossLevel, BattleResultInfo battleResult)
	{
		OperateBossSubFightResultReq operateBossSubFightResultReq = new OperateBossSubFightResultReq();
		operateBossSubFightResultReq.UUID = uuid;
		operateBossSubFightResultReq.BossLevel = BossLevel;
		operateBossSubFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)operateBossSubFightResultReq, Message.OperateBossSubFightResultReq);
	}

	public void Operate_BossSubFightResult_Ack(ByteString response)
	{
		OperateBossSubFightResultAck operateBossSubFightResultAck = OperateBossSubFightResultAck.Parser.ParseFrom(response);
		if (operateBossSubFightResultAck != null)
		{
			zbSelf.Instance.bossSubjectHandler.getScore = operateBossSubFightResultAck.Score;
			zbServerData.sGameData.OperateInfo.BossSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.BossSubjectList.AddRange((IEnumerable<BossSubjectDetail>)operateBossSubFightResultAck.BossSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateBossSubFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = operateBossSubFightResultAck.AwardList;
			((zbCombatBalanceWnd)zbCombatSystem.Instance.SetBanlanceData(operateBossSubFightResultAck.BattleSettle)).ShowNewRecord(EBoolState.False);
		}
	}

	public void Operate_BossSubZhanLingAward_Req(string uuid)
	{
		OperateBossSubZhanLingAwardReq operateBossSubZhanLingAwardReq = new OperateBossSubZhanLingAwardReq();
		operateBossSubZhanLingAwardReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateBossSubZhanLingAwardReq, Message.OperateBossSubZhanLingAwardReq);
	}

	public void Operate_BossSubZhanLingAward_Ack(ByteString response)
	{
		OperateBossSubZhanLingAwardAck operateBossSubZhanLingAwardAck = OperateBossSubZhanLingAwardAck.Parser.ParseFrom(response);
		if (operateBossSubZhanLingAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.BossSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.BossSubjectList.AddRange((IEnumerable<BossSubjectDetail>)operateBossSubZhanLingAwardAck.BossSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateBossSubZhanLingAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateBossSubZhanLingAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_BossSubject_Ack);
		}
	}

	public void Operate_BossSubRoulette_Req(string uuid, int count)
	{
		OperateBossSubRouletteReq operateBossSubRouletteReq = new OperateBossSubRouletteReq();
		operateBossSubRouletteReq.UUID = uuid;
		operateBossSubRouletteReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateBossSubRouletteReq, Message.OperateBossSubRouletteReq);
	}

	public void Operate_BossSubRoulette_Ack(ByteString response)
	{
		OperateBossSubRouletteAck operateBossSubRouletteAck = OperateBossSubRouletteAck.Parser.ParseFrom(response);
		if (operateBossSubRouletteAck != null)
		{
			zbServerData.sGameData.OperateInfo.BossSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.BossSubjectList.AddRange((IEnumerable<BossSubjectDetail>)operateBossSubRouletteAck.BossSubjectList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_BossSubRoulette_Ack, operateBossSubRouletteAck);
		}
	}

	public void Operate_BossSubRouletteTarget_Req(string uuid)
	{
		OperateBossSubRouletteTargetReq operateBossSubRouletteTargetReq = new OperateBossSubRouletteTargetReq();
		operateBossSubRouletteTargetReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateBossSubRouletteTargetReq, Message.OperateBossSubRouletteTargetReq);
	}

	public void Operate_BossSubRouletteTarget_Ack(ByteString response)
	{
		OperateBossSubRouletteTargetAck operateBossSubRouletteTargetAck = OperateBossSubRouletteTargetAck.Parser.ParseFrom(response);
		if (operateBossSubRouletteTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.BossSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.BossSubjectList.AddRange((IEnumerable<BossSubjectDetail>)operateBossSubRouletteTargetAck.BossSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateBossSubRouletteTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateBossSubRouletteTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_BossSubTaskAward_Req(string uuid, ETaskType TaskType)
	{
		OperateBossSubTaskAwardReq operateBossSubTaskAwardReq = new OperateBossSubTaskAwardReq();
		operateBossSubTaskAwardReq.UUID = uuid;
		operateBossSubTaskAwardReq.TaskType = TaskType;
		Instance.WsPackMsg((IMessage)(object)operateBossSubTaskAwardReq, Message.OperateBossSubTaskAwardReq);
	}

	public void Operate_BossSubTaskAward_Ack(ByteString response)
	{
		OperateBossSubTaskAwardAck operateBossSubTaskAwardAck = OperateBossSubTaskAwardAck.Parser.ParseFrom(response);
		if (operateBossSubTaskAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.BossSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.BossSubjectList.AddRange((IEnumerable<BossSubjectDetail>)operateBossSubTaskAwardAck.BossSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateBossSubTaskAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateBossSubTaskAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_BossSubject_Ack);
		}
	}

	public void Operate_BossSubSaoDang_Req(string uuid)
	{
		OperateBossSubSaoDangReq operateBossSubSaoDangReq = new OperateBossSubSaoDangReq();
		operateBossSubSaoDangReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateBossSubSaoDangReq, Message.OperateBossSubSaoDangReq);
	}

	public void Operate_BossSubSaoDang_Ack(ByteString response)
	{
		OperateBossSubSaoDangAck operateBossSubSaoDangAck = OperateBossSubSaoDangAck.Parser.ParseFrom(response);
		if (operateBossSubSaoDangAck != null)
		{
			zbServerData.sGameData.OperateInfo.BossSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.BossSubjectList.AddRange((IEnumerable<BossSubjectDetail>)operateBossSubSaoDangAck.BossSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateBossSubSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)operateBossSubSaoDangAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_BossSubject_Ack);
		}
	}

	public void Chat_Send_Req(EChatType chatType, int chatTarget, string content)
	{
		ChatSendReq chatSendReq = new ChatSendReq();
		chatSendReq.ChatType = chatType;
		chatSendReq.ChatTarget = chatTarget;
		chatSendReq.Content = content;
		ChatDetail[] details = new ChatDetail[1]
		{
			new ChatDetail
			{
				RoleBaseInfo = zbServerData.sGameData.RoleBaseInfo,
				ChatType = chatType,
				ChatTm = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				Action = new ChatActionDetail
				{
					BasicData = new ChatActionBasicData
					{
						Text = new ContentDetail
						{
							Label = content
						}
					}
				}
			}
		};
		zbSelf.Instance.chatHandler.SetCurrentMsg(details, chatType);
		Instance.WsPackMsg((IMessage)(object)chatSendReq, Message.ChatSendReq);
	}

	public void Chat_Send_Ack(ByteString response)
	{
		ChatSendAck chatSendAck = ChatSendAck.Parser.ParseFrom(response);
		if (chatSendAck != null)
		{
			zbSelf.Instance.chatHandler.UpdateStateList(((IEnumerable<ChatState>)chatSendAck.StateList).ToArray());
			zbSelf.Instance.chatHandler.UpdateCurrentMsg();
		}
	}

	public void Chat_Data_Req(EChatType chatType, int start)
	{
		ChatDataReq chatDataReq = new ChatDataReq();
		chatDataReq.ChatType = chatType;
		chatDataReq.Start = start;
		Instance.WsPackMsg((IMessage)(object)chatDataReq, Message.ChatDataReq);
	}

	public void Chat_Data_Ack(ByteString response)
	{
		ChatDataAck chatDataAck = ChatDataAck.Parser.ParseFrom(response);
		if (chatDataAck != null)
		{
			switch (chatDataAck.ChatType)
			{
			case EChatType.Eworld:
				zbSelf.Instance.chatHandler.UpdateWorldDetails(((IEnumerable<ChatDetail>)chatDataAck.ChatList).ToArray());
				break;
			case EChatType.Elegion:
				zbSelf.Instance.chatHandler.UpdateLegionDetails(((IEnumerable<ChatDetail>)chatDataAck.ChatList).ToArray());
				break;
			case EChatType.Eserver:
			case EChatType.Eprivate:
			case EChatType.Esystem:
				break;
			}
		}
	}

	public void Chat_ActionShare_Req(EChatActionType actionType, ChatActionParams actionParams)
	{
		ChatActionShareReq chatActionShareReq = new ChatActionShareReq();
		chatActionShareReq.ActionType = actionType;
		chatActionShareReq.ActionParams = actionParams;
		Instance.WsPackMsg((IMessage)(object)chatActionShareReq, Message.ChatActionShareReq);
	}

	public void Chat_ActionShare_Ack(ByteString response)
	{
		ChatActionShareAck.Parser.ParseFrom(response);
	}

	public void Chat_ActionData_Req(EChatActionType actionType, string uuid)
	{
		ChatActionDataReq chatActionDataReq = new ChatActionDataReq();
		chatActionDataReq.ActionType = actionType;
		chatActionDataReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)chatActionDataReq, Message.ChatActionDataReq);
	}

	public void Chat_ActionData_Ack(ByteString response)
	{
		ChatActionDataAck.Parser.ParseFrom(response);
	}

	public void Chat_Scroll_Ntf(ByteString response)
	{
		ChatScrollNtf chatScrollNtf = ChatScrollNtf.Parser.ParseFrom(response);
		if (chatScrollNtf != null && chatScrollNtf.ScrollType == EScrollType.Escroll)
		{
			zbUIMgr.Instance.PushScrollBubble(zbUtil.GetContentString(chatScrollNtf.Content));
		}
	}

	public void Chat_Receive_Ntf(ByteString response)
	{
		ChatReceiveNtf chatReceiveNtf = ChatReceiveNtf.Parser.ParseFrom(response);
		if (chatReceiveNtf != null)
		{
			ChatDetail[] array = ((IEnumerable<ChatDetail>)chatReceiveNtf.ReceiveList).ToArray();
			switch ((array.Count() > 0) ? array[0].ChatType : EChatType.EmptyValue)
			{
			case EChatType.Eworld:
				zbSelf.Instance.chatHandler.UpdateWorldDetailsByRecive(array);
				break;
			case EChatType.Elegion:
				zbSelf.Instance.chatHandler.UpdateLegionDetailsByRecive(array);
				break;
			}
		}
	}

	public void SyncMsg(Message messageId)
	{
		switch (messageId)
		{
		case Message.NoticeListReq:
			zbLoginUtil.NoticListReq(open: true);
			break;
		case Message.FriendsRecommendReq:
			Friends_Recommend_Req();
			break;
		case Message.FriendsListReq:
			Friends_List_Req();
			break;
		case Message.FriendsApplyListReq:
			Friends_ApplyList_Req();
			break;
		case Message.FriendsApplyPassAllReq:
			Friends_ApplyPassAll_Req();
			break;
		case Message.FriendsApplyRefuseAllReq:
			Friends_ApplyRefuseAll_Req();
			break;
		case Message.FriendsActionGiveAllReq:
			Friends_ActionGiveAll_Req();
			break;
		case Message.FriendsActionCollectAllReq:
			Friends_ActionCollectAll_Req();
			break;
		case Message.MailGetListReq:
			Mail_GetList_Req();
			break;
		case Message.BagLimitItemSaleReq:
			Bag_LimitItemSale_Req();
			break;
		case Message.BarDataReq:
			Bar_Data_Req();
			break;
		case Message.GemComposePreviewReq:
			Gem_ComposePreview_Req();
			break;
		case Message.GemComposeReq:
			Gem_Compose_Req();
			break;
		case Message.LegionRecommendReq:
			Legion_Recommend_Req();
			break;
		case Message.LegionJoinQuickReq:
			Legion_JoinQuick_Req();
			break;
		case Message.LegionClearJoinCdReq:
			Legion_ClearJoinCD_Req();
			break;
		case Message.LegionQuitReq:
			Legion_Quit_Req();
			break;
		case Message.LegionDataReq:
			Legion_Data_Req();
			break;
		case Message.LegionMembersReq:
			Legion_Members_Req();
			break;
		case Message.LegionApplyListReq:
			Legion_ApplyList_Req();
			break;
		case Message.LegionApplyPassAllReq:
			Legion_ApplyPassAll_Req();
			break;
		case Message.LegionApplyRefuseAllReq:
			Legion_ApplyRefuseAll_Req();
			break;
		case Message.LegionLogListReq:
			Legion_LogList_Req();
			break;
		case Message.LegionBossRankListReq:
			LegionBoss_RankList_Req();
			break;
		case Message.LegionBossRecordListReq:
			LegionBoss_RecordList_Req();
			break;
		case Message.GameKickReq:
			Game_Kick_Req();
			break;
		case Message.GameLoginReq:
			Game_Login_Req();
			break;
		case Message.RoleCreateReq:
			Role_Create_Req();
			break;
		case Message.RoleReadyReq:
			Role_Ready_Req();
			break;
		case Message.GameLoginOutReq:
			Game_LoginOut_Req();
			break;
		case Message.HeartBeatReq:
			HeartBeat_Req();
			break;
		case Message.MailGetAllAwardReq:
			Mail_GetAllAward_Req();
			break;
		case Message.MainQuestDataReq:
			MainQuest_Data_Req();
			break;
		case Message.EliteQuestDataReq:
			EliteQuest_Data_Req();
			break;
		case Message.EliteQuestHelpRecordListReq:
			EliteQuest_HelpRecordList_Req();
			break;
		case Message.HomelandV2DataReq:
			HomelandV2_Data_Req();
			break;
		case Message.PeerageDataReq:
			Peerage_Data_Req();
			break;
		case Message.DailyTaskDataReq:
			DailyTask_Data_Req();
			break;
		case Message.OperateDataReq:
			Operate_Data_Req();
			break;
		case Message.OperateThemeTaskDataReq:
			Operate_ThemeTaskData_Req();
			break;
		case Message.HandbookDataReq:
			Handbook_Data_Req();
			break;
		case Message.RoleDataReq:
			Role_Data_Req();
			break;
		case Message.RechargeShopGrowthGiftReq:
			RechargeShop_GrowthGift_Req();
			break;
		case Message.ActivityDataReq:
			Activity_Data_Req();
			break;
		case Message.RechargeActiveDataReq:
			RechargeActive_Data_Req();
			break;
		case Message.WorldDataReq:
			World_Data_Req();
			break;
		case Message.WorldBossDataReq:
			WorldBoss_Data_Req();
			break;
		case Message.WorldBossRecordListReq:
			WorldBoss_RecordList_Req();
			break;
		case Message.HeroTrialDataReq:
			HeroTrial_Data_Req();
			break;
		case Message.PaySuccessOrderReq:
			Pay_SuccessOrder_Req();
			break;
		case Message.RankTopReq:
			Rank_Top_Req();
			break;
		case Message.CrossPeakTopReq:
			CrossPeak_Top_Req();
			break;
		case Message.OpenServerRankTopReq:
			OpenServer_RankTop_Req();
			break;
		case Message.IdleDataReq:
			Idle_Data_Req();
			break;
		case Message.WorldLairDataReq:
			WorldLair_Data_Req();
			break;
		case Message.ElementTrialDataReq:
			ElementTrial_Data_Req();
			break;
		case Message.MechaDataReq:
			Mecha_Data_Req();
			break;
		case Message.JourneyDataReq:
			Journey_Data_Req();
			break;
		case Message.WallDataReq:
			Wall_Data_Req();
			break;
		case Message.FootholdDataReq:
			Foothold_Data_Req();
			break;
		case Message.ArenaDataReq:
			Arena_Data_Req();
			break;
		case Message.ArenaCombatListReq:
			Arena_CombatList_Req();
			break;
		case Message.AuxReminderListReq:
			Aux_ReminderList_Req();
			break;
		case Message.CrossLadderDataReq:
			CrossLadder_Data_Req();
			break;
		case Message.CrossPeakDataReq:
			CrossPeak_Data_Req();
			break;
		case Message.TreasureDataReq:
			Treasure_Data_Req();
			break;
		case Message.OperateHolidayTaskDataReq:
			Operate_HolidayTaskData_Req();
			break;
		case Message.ServantDataReq:
			Servant_Data_Req();
			break;
		case Message.IncubateDataReq:
			Incubate_Data_Req();
			break;
		case Message.OperateZhanLingDataReq:
			Operate_ZhanLingData_Req();
			break;
		case Message.EndlessMineDataReq:
			EndlessMine_Data_Req();
			break;
		case Message.TruckMatchListReq:
			Truck_MatchList_Req();
			break;
		case Message.TruckDataReq:
			Truck_Data_Req();
			break;
		case Message.LegionWarDataReq:
			LegionWar_Data_Req();
			break;
		case Message.LegionWarEnterPointReq:
			zbSelf.Instance.legionWarHandler.CallEnterPointReq();
			break;
		case Message.FacilityDataReq:
			Facility_Data_Req();
			break;
		case Message.OperateBossSubDataReq:
			Operate_BossSubData_Req();
			break;
		case Message.OperateWallSubjectDataReq:
			Operate_WallSubjectData_Req();
			break;
		case Message.OperateMechaSubjectDataReq:
			Operate_MechaSubjectData_Req();
			break;
		case Message.FacilityViewReq:
			if (zbSelf.Instance.facilityHandler.facilityMapInfo != null && zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo != null)
			{
				Facility_View_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId);
			}
			break;
		case Message.TrainDataReq:
			Train_Data_Req();
			break;
		case Message.TrainTransportTakeReq:
			Train_TransportTake_Req();
			break;
		}
	}

	public void Reset_RoleName_Req(string RoleName)
	{
		ResetRoleNameReq resetRoleNameReq = new ResetRoleNameReq();
		resetRoleNameReq.RoleName = RoleName;
		Instance.WsPackMsg((IMessage)(object)resetRoleNameReq, Message.ResetRoleNameReq);
	}

	public void Reset_RoleName_Ack(ByteString response)
	{
		ResetRoleNameAck resetRoleNameAck = ResetRoleNameAck.Parser.ParseFrom(response);
		if (resetRoleNameAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(resetRoleNameAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_RoleName_Changed);
		}
	}

	public void Rewards_RedeemCode_Req(string code)
	{
		RewardsRedeemCodeReq rewardsRedeemCodeReq = new RewardsRedeemCodeReq();
		rewardsRedeemCodeReq.Code = code;
		Instance.WsPackMsg((IMessage)(object)rewardsRedeemCodeReq, Message.RewardsRedeemCodeReq);
	}

	public void Rewards_RedeemCode_Ack(ByteString response)
	{
		RewardsRedeemCodeAck rewardsRedeemCodeAck = RewardsRedeemCodeAck.Parser.ParseFrom(response);
		if (rewardsRedeemCodeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(rewardsRedeemCodeAck.DataChange, ((IEnumerable<AwardDetail>)rewardsRedeemCodeAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Rewards_RedeemCode);
		}
	}

	public void Bag_HeadFrameActive_Req(int PropTid)
	{
		BagHeadFrameActiveReq bagHeadFrameActiveReq = new BagHeadFrameActiveReq();
		bagHeadFrameActiveReq.PropTid = PropTid;
		Instance.WsPackMsg((IMessage)(object)bagHeadFrameActiveReq, Message.BagHeadFrameActiveReq);
	}

	public void Bag_HeadFrameActive_Ack(ByteString response)
	{
		BagHeadFrameActiveAck bagHeadFrameActiveAck = BagHeadFrameActiveAck.Parser.ParseFrom(response);
		if (bagHeadFrameActiveAck != null)
		{
			zbServerData.sGameData.HeadFrameList.Clear();
			zbServerData.sGameData.HeadFrameList.AddRange((IEnumerable<int>)bagHeadFrameActiveAck.HeadFrameList);
			zbSelf.Instance.dataHandler.DataChange(bagHeadFrameActiveAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Bag_HeadFrameActive);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Head_Data);
		}
	}

	public void Aux_FuncUnlockAward_Req(int[] selectList)
	{
		AuxFuncUnlockAwardReq auxFuncUnlockAwardReq = new AuxFuncUnlockAwardReq();
		auxFuncUnlockAwardReq.SelectList.Clear();
		auxFuncUnlockAwardReq.SelectList.AddRange((IEnumerable<int>)selectList);
		Instance.WsPackMsg((IMessage)(object)auxFuncUnlockAwardReq, Message.AuxFuncUnlockAwardReq);
	}

	public void Aux_FuncUnlockAward_Ack(ByteString response)
	{
		AuxFuncUnlockAwardAck auxFuncUnlockAwardAck = AuxFuncUnlockAwardAck.Parser.ParseFrom(response);
		if (auxFuncUnlockAwardAck != null)
		{
			zbServerData.sGameData.AuxAwardInfo = auxFuncUnlockAwardAck.AuxAwardInfo;
			zbSelf.Instance.dataHandler.DataChange(auxFuncUnlockAwardAck.DataChange);
			zbSelf.Instance.dataHandler.DataChange(auxFuncUnlockAwardAck.DataChange, ((IEnumerable<AwardDetail>)auxFuncUnlockAwardAck.AwardList).ToArray());
			if (zbUIMgr.Instance.IsShow(EWndId.FuncPreview))
			{
				(zbUIMgr.Instance.GetWndById(EWndId.FuncPreview) as zbFuncPreviewWnd).RefreshCustom();
			}
		}
	}

	public void Aux_ExchangeMoney_Req(EMoneyType eMoneyType, int Count)
	{
		AuxExchangeMoneyReq auxExchangeMoneyReq = new AuxExchangeMoneyReq();
		auxExchangeMoneyReq.MoneyType = eMoneyType;
		auxExchangeMoneyReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)auxExchangeMoneyReq, Message.AuxExchangeMoneyReq);
	}

	public void Aux_ExchangeMoney_Ack(ByteString response)
	{
		AuxExchangeMoneyAck auxExchangeMoneyAck = AuxExchangeMoneyAck.Parser.ParseFrom(response);
		if (auxExchangeMoneyAck != null)
		{
			zbServerData.sGameData.ExchangeList.Clear();
			zbServerData.sGameData.ExchangeList.Add((IEnumerable<MoneyExchangeDetail>)auxExchangeMoneyAck.ExchangeList);
			zbSelf.Instance.dataHandler.DataChange(auxExchangeMoneyAck.DataChange, ((IEnumerable<AwardDetail>)auxExchangeMoneyAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Aux_ExchangeMoney);
		}
	}

	public void Aux_ExchangeHalf_Req(EMoneyType MoneyType)
	{
		AuxExchangeHalfReq auxExchangeHalfReq = new AuxExchangeHalfReq();
		auxExchangeHalfReq.MoneyType = MoneyType;
		Instance.WsPackMsg((IMessage)(object)auxExchangeHalfReq, Message.AuxExchangeHalfReq);
	}

	public void Aux_ExchangeHalf_Ack(ByteString response)
	{
		AuxExchangeHalfAck auxExchangeHalfAck = AuxExchangeHalfAck.Parser.ParseFrom(response);
		if (auxExchangeHalfAck != null)
		{
			zbServerData.sGameData.ExchangeList.Clear();
			zbServerData.sGameData.ExchangeList.AddRange((IEnumerable<MoneyExchangeDetail>)auxExchangeHalfAck.ExchangeList);
			zbSelf.Instance.dataHandler.DataChange(auxExchangeHalfAck.DataChange, ((IEnumerable<AwardDetail>)auxExchangeHalfAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Aux_ExchangeMoney);
		}
	}

	public void Aux_ResourceQuickBuy_Req(EResourceType Type, long Tid, int GroupCount)
	{
		AuxResourceQuickBuyReq auxResourceQuickBuyReq = new AuxResourceQuickBuyReq();
		auxResourceQuickBuyReq.Type = Type;
		auxResourceQuickBuyReq.Tid = Tid;
		auxResourceQuickBuyReq.GroupCount = GroupCount;
		Instance.WsPackMsg((IMessage)(object)auxResourceQuickBuyReq, Message.AuxResourceQuickBuyReq);
	}

	public void Aux_ResourceQuickBuy_Ack(ByteString response)
	{
		AuxResourceQuickBuyAck auxResourceQuickBuyAck = AuxResourceQuickBuyAck.Parser.ParseFrom(response);
		if (auxResourceQuickBuyAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(auxResourceQuickBuyAck.DataChange, ((IEnumerable<AwardDetail>)auxResourceQuickBuyAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Aux_ExchangeMoney);
		}
	}

	public void Reset_HeadId_Req(int headId)
	{
		ResetHeadIdReq resetHeadIdReq = new ResetHeadIdReq();
		resetHeadIdReq.HeadId = headId;
		Instance.WsPackMsg((IMessage)(object)resetHeadIdReq, Message.ResetHeadIdReq);
	}

	public void Reset_HeadId_Ack(ByteString response)
	{
		ResetHeadIdAck resetHeadIdAck = ResetHeadIdAck.Parser.ParseFrom(response);
		if (resetHeadIdAck != null)
		{
			zbSelf.Instance.UpdateRoleBaseInfo(resetHeadIdAck.RoleBaseInfo);
		}
	}

	public void Reset_HeadFrameId_Req(int headFrameId)
	{
		ResetHeadFrameIdReq resetHeadFrameIdReq = new ResetHeadFrameIdReq();
		resetHeadFrameIdReq.HeadFrameId = headFrameId;
		Instance.WsPackMsg((IMessage)(object)resetHeadFrameIdReq, Message.ResetHeadFrameIdReq);
	}

	public void Reset_HeadFrameId_Ack(ByteString response)
	{
		ResetHeadFrameIdAck resetHeadFrameIdAck = ResetHeadFrameIdAck.Parser.ParseFrom(response);
		if (resetHeadFrameIdAck != null)
		{
			zbSelf.Instance.UpdateRoleBaseInfo(resetHeadFrameIdAck.RoleBaseInfo);
		}
	}

	public void MessageSync_Ntf(ByteString response)
	{
		MessageSyncNtf messageSyncNtf = MessageSyncNtf.Parser.ParseFrom(response);
		if (messageSyncNtf == null)
		{
			return;
		}
		foreach (int id in messageSyncNtf.IdList)
		{
			SyncMsg((Message)id);
		}
	}

	public void Role_Data_Req()
	{
		RoleDataReq req = new RoleDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.RoleDataReq);
	}

	public void Role_Data_Ack(ByteString response)
	{
		RoleDataAck roleDataAck = RoleDataAck.Parser.ParseFrom(response);
		if (roleDataAck != null)
		{
			zbServerData.sGameData = roleDataAck.GameData;
		}
	}

	public void Role_TipsInfo_Req(int roleId)
	{
		if (!zbUtil.IsNpc(roleId))
		{
			RoleTipsInfoReq roleTipsInfoReq = new RoleTipsInfoReq();
			roleTipsInfoReq.PlayerId = roleId;
			Instance.WsPackMsg((IMessage)(object)roleTipsInfoReq, Message.RoleTipsInfoReq);
		}
	}

	public void Role_TipsInfo_Ack(ByteString response)
	{
		RoleTipsInfoAck roleTipsInfoAck = RoleTipsInfoAck.Parser.ParseFrom(response);
		if (roleTipsInfoAck != null)
		{
			zbUIMgr.Instance.ShowPlayerInfo(roleTipsInfoAck.TipsInfo);
		}
	}

	public void Role_TipsDetail_Req(int roleId)
	{
		if (!zbUtil.IsNpc(roleId))
		{
			RoleTipsDetailReq roleTipsDetailReq = new RoleTipsDetailReq();
			roleTipsDetailReq.PlayerId = roleId;
			Instance.WsPackMsg((IMessage)(object)roleTipsDetailReq, Message.RoleTipsDetailReq);
		}
	}

	public void Role_TipsDetail_Ack(ByteString response)
	{
		RoleTipsDetailAck roleTipsDetailAck = RoleTipsDetailAck.Parser.ParseFrom(response);
		if (roleTipsDetailAck != null)
		{
			zbUIMgr.Instance.ShowPlayerTipsDetailWnd(roleTipsDetailAck.TipsDetail);
		}
	}

	public void Role_MiniTipsInfo_Req(int roleId)
	{
		RoleMiniTipsInfoReq roleMiniTipsInfoReq = new RoleMiniTipsInfoReq();
		roleMiniTipsInfoReq.PlayerId = roleId;
		Instance.WsPackMsg((IMessage)(object)roleMiniTipsInfoReq, Message.RoleMiniTipsInfoReq);
	}

	public void Role_MiniTipsInfo_Ack(ByteString response)
	{
		RoleMiniTipsInfoAck roleMiniTipsInfoAck = RoleMiniTipsInfoAck.Parser.ParseFrom(response);
		if (roleMiniTipsInfoAck != null)
		{
			zbUIMgr.Instance.ShowPlayerInfoMiniTip(roleMiniTipsInfoAck.MiniTipsInfo);
		}
	}

	public void Role_PvpTipsInfo_Req(int roleId)
	{
		if (zbSelf.Instance.guardHandler.pvpTipsInfo != null && zbSelf.Instance.guardHandler.pvpTipsInfo.RoleBaseInfo.RoleId == roleId)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_PvpTipsInfo);
			return;
		}
		RolePvpTipsInfoReq rolePvpTipsInfoReq = new RolePvpTipsInfoReq();
		rolePvpTipsInfoReq.PlayerId = roleId;
		Instance.WsPackMsg((IMessage)(object)rolePvpTipsInfoReq, Message.RolePvpTipsInfoReq);
	}

	public void Role_PvpTipsInfo_Ack(ByteString response)
	{
		RolePvpTipsInfoAck rolePvpTipsInfoAck = RolePvpTipsInfoAck.Parser.ParseFrom(response);
		if (rolePvpTipsInfoAck != null)
		{
			zbSelf.Instance.guardHandler.pvpTipsInfo = rolePvpTipsInfoAck.PvpTipsInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_PvpTipsInfo);
		}
	}

	public void Role_FightStart_Req(int PlayerId)
	{
		RoleFightStartReq roleFightStartReq = new RoleFightStartReq();
		roleFightStartReq.PlayerId = PlayerId;
		zbSelf.Instance.combatHandler.roleFightPlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)roleFightStartReq, Message.RoleFightStartReq);
	}

	public void Role_FightStart_Ack(ByteString response)
	{
		RoleFightStartAck roleFightStartAck = RoleFightStartAck.Parser.ParseFrom(response);
		if (roleFightStartAck != null)
		{
			zbUtil.Debug2LogFile("Role_FightStart_Ack.txt", roleFightStartAck);
			zbSelf.Instance.combatHandler.StartShowCombat(roleFightStartAck.BattleInfo);
		}
	}

	public void Role_FightResult_Req(int PlayerId, BattleResultInfo BattleResult)
	{
		RoleFightResultReq roleFightResultReq = new RoleFightResultReq();
		roleFightResultReq.PlayerId = PlayerId;
		roleFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)roleFightResultReq, Message.RoleFightResultReq);
	}

	public void Role_FightResult_Ack(ByteString response)
	{
		RoleFightResultAck roleFightResultAck = RoleFightResultAck.Parser.ParseFrom(response);
		if (roleFightResultAck != null)
		{
			zbUtil.Debug2LogFile("Role_FightResult_Ack.txt", roleFightResultAck);
			zbCombatSystem.Instance.SetBanlanceData(roleFightResultAck.BattleSettle);
		}
	}

	public void Aux_FreeGiftAward_Req(int ferrGiftType)
	{
		AuxFreeGiftAwardReq auxFreeGiftAwardReq = new AuxFreeGiftAwardReq();
		auxFreeGiftAwardReq.FreeType = ferrGiftType;
		Instance.WsPackMsg((IMessage)(object)auxFreeGiftAwardReq, Message.AuxFreeGiftAwardReq);
	}

	public void Aux_FreeGiftAward_Ack(ByteString response)
	{
		AuxFreeGiftAwardAck auxFreeGiftAwardAck = AuxFreeGiftAwardAck.Parser.ParseFrom(response);
		if (auxFreeGiftAwardAck != null)
		{
			zbServerData.sGameData.FreeGiftList.Clear();
			zbServerData.sGameData.FreeGiftList.AddRange((IEnumerable<FreeGiftDetail>)auxFreeGiftAwardAck.FreeGiftList);
			zbSelf.Instance.dataHandler.DataChange(auxFreeGiftAwardAck.DataChange, ((IEnumerable<AwardDetail>)auxFreeGiftAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Aux_FreeGiftAward);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Aux_PlayerSuggest_Req(EBattleFuncType battleType, int levelId, bool _needFirst = false)
	{
		AuxPlayerSuggestReq auxPlayerSuggestReq = new AuxPlayerSuggestReq();
		auxPlayerSuggestReq.BattleType = battleType;
		auxPlayerSuggestReq.LevelId = levelId;
		needFirst = _needFirst;
		Instance.WsPackMsg((IMessage)(object)auxPlayerSuggestReq, Message.AuxPlayerSuggestReq);
	}

	public void Aux_PlayerSuggest_Ack(ByteString response)
	{
		AuxPlayerSuggestAck auxPlayerSuggestAck = AuxPlayerSuggestAck.Parser.ParseFrom(response);
		if (auxPlayerSuggestAck != null)
		{
			if (zbUIMgr.Instance.IsShow(EWndId.Combat) && needFirst)
			{
				zbSelf.Instance.playerSuggestHandler.playerSuggestAck = auxPlayerSuggestAck;
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Aux_PlayerSuggest);
			}
			else if (auxPlayerSuggestAck.SuggestList.Count > 0)
			{
				zbUIMgr.Instance.ShowPlayerSuggest(((IEnumerable<PlayerSuggestDetail>)auxPlayerSuggestAck.SuggestList).ToArray());
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10657"));
			}
		}
		needFirst = false;
	}

	public void Aux_ProgressTrack_Req(int EventType)
	{
		AuxProgressTrackReq auxProgressTrackReq = new AuxProgressTrackReq();
		auxProgressTrackReq.EventType = EventType;
		Instance.WsPackMsg((IMessage)(object)auxProgressTrackReq, Message.AuxProgressTrackReq, needTip: false);
	}

	public void Aux_ProgressTrack_Ack(ByteString response)
	{
		AuxProgressTrackAck auxProgressTrackAck = AuxProgressTrackAck.Parser.ParseFrom(response);
		if (auxProgressTrackAck != null)
		{
			zbServerData.sGameData.ProgressEventList.Clear();
			zbServerData.sGameData.ProgressEventList.AddRange((IEnumerable<int>)auxProgressTrackAck.ProgressEventList);
		}
	}

	public void Aux_ReminderList_Req()
	{
		AuxReminderListReq req = new AuxReminderListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.AuxReminderListReq, needTip: false);
	}

	public void Aux_ReminderList_Ack(ByteString response)
	{
		AuxReminderListAck auxReminderListAck = AuxReminderListAck.Parser.ParseFrom(response);
		if (auxReminderListAck != null)
		{
			zbServerData.sGameData.ReminderList.Clear();
			zbServerData.sGameData.ReminderList.AddRange((IEnumerable<int>)auxReminderListAck.ReminderList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ReminderList);
		}
	}

	public void Aux_ResourceConvert_Req(int Id, int Count)
	{
		AuxResourceConvertReq auxResourceConvertReq = new AuxResourceConvertReq();
		auxResourceConvertReq.Id = Id;
		auxResourceConvertReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)auxResourceConvertReq, Message.AuxResourceConvertReq, needTip: false);
	}

	public void Aux_ResourceConvert_Ack(ByteString response)
	{
		AuxResourceConvertAck auxResourceConvertAck = AuxResourceConvertAck.Parser.ParseFrom(response);
		if (auxResourceConvertAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(auxResourceConvertAck.DataChange, ((IEnumerable<AwardDetail>)auxResourceConvertAck.AwardList).ToArray());
		}
	}

	public void PushSet_Data_Req()
	{
		PushSetDataReq req = new PushSetDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PushSetDataReq);
	}

	public void PushSet_Data_Ack(ByteString response)
	{
		PushSetDataAck pushSetDataAck = PushSetDataAck.Parser.ParseFrom(response);
		if (pushSetDataAck != null)
		{
			zbSelf.Instance.notifySetHandler.UpdatePushSetInfo(pushSetDataAck.PushSetInfo);
		}
	}

	public void PushSet_Close_Req(EPushType[] pushList)
	{
		PushSetCloseReq pushSetCloseReq = new PushSetCloseReq();
		pushSetCloseReq.CloseList.Clear();
		pushSetCloseReq.CloseList.AddRange((IEnumerable<EPushType>)pushList);
		Instance.WsPackMsg((IMessage)(object)pushSetCloseReq, Message.PushSetCloseReq);
	}

	public void PushSet_Close_Ack(ByteString response)
	{
		PushSetCloseAck pushSetCloseAck = PushSetCloseAck.Parser.ParseFrom(response);
		if (pushSetCloseAck != null)
		{
			zbSelf.Instance.notifySetHandler.UpdatePushSetInfo(pushSetCloseAck.PushSetInfo);
		}
	}

	public void CrossLadder_Data_Req(bool openWnd = false)
	{
		CrossLadderDataReq req = new CrossLadderDataReq();
		openCrossLadderWnd = openWnd;
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderDataReq);
	}

	public void CrossLadder_Data_Ack(ByteString response)
	{
		CrossLadderDataAck crossLadderDataAck = CrossLadderDataAck.Parser.ParseFrom(response);
		if (crossLadderDataAck == null)
		{
			return;
		}
		zbSelf.Instance.crossLadderHandler.UpdateCrossLadderInfo(crossLadderDataAck.CrossLadderInfo);
		if (openCrossLadderWnd && zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen(needBubble: true))
		{
			CrossLadderInfo crossLadderInfo = zbServerData.sGameData.CrossLadderInfo;
			if (crossLadderInfo != null && crossLadderInfo.SelfInfo?.SeasonAward.State == EBoolState.False)
			{
				zbUIMgr.Instance.ShowCrossLadderSeasonInit();
			}
			else
			{
				zbUIMgr.Instance.ShowCrossLadder();
			}
		}
	}

	public void CrossLadder_SeasonInit_Req()
	{
		CrossLadderSeasonInitReq req = new CrossLadderSeasonInitReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderSeasonInitReq);
	}

	public void CrossLadder_SeasonInit_Ack(ByteString response)
	{
		CrossLadderSeasonInitAck crossLadderSeasonInitAck = CrossLadderSeasonInitAck.Parser.ParseFrom(response);
		if (crossLadderSeasonInitAck != null)
		{
			zbSelf.Instance.crossLadderHandler.UpdateCrossLadderInfo(crossLadderSeasonInitAck.CrossLadderInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_CrossLadder_SeasonInit_Ack, crossLadderSeasonInitAck);
		}
	}

	public void CrossLadder_CombatList_Req()
	{
		CrossLadderCombatListReq req = new CrossLadderCombatListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderCombatListReq);
	}

	public void CrossLadder_CombatList_Ack(ByteString response)
	{
		CrossLadderCombatListAck crossLadderCombatListAck = CrossLadderCombatListAck.Parser.ParseFrom(response);
		if (crossLadderCombatListAck != null)
		{
			zbUIMgr.Instance.ShowCrossLadderMatchSuccess(((IEnumerable<CrossLadderPlayerInfo>)crossLadderCombatListAck.CombatList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_CrossLadder_CombatList_Ack);
		}
	}

	public void CrossLadder_FightStart_Req(int PlayerId)
	{
		CrossLadderFightStartReq crossLadderFightStartReq = new CrossLadderFightStartReq();
		crossLadderFightStartReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)crossLadderFightStartReq, Message.CrossLadderFightStartReq);
	}

	public void CrossLadder_FightStart_Ack(ByteString response)
	{
		CrossLadderFightStartAck crossLadderFightStartAck = CrossLadderFightStartAck.Parser.ParseFrom(response);
		if (crossLadderFightStartAck != null)
		{
			zbSelf.Instance.crossLadderHandler.playerId = crossLadderFightStartAck.PlayerId;
			zbSelf.Instance.combatHandler.StartShowCombat(crossLadderFightStartAck.BattleInfo);
		}
	}

	public void CrossLadder_FightResult_Req(int PlayerId, BattleResultInfo BattleResult)
	{
		CrossLadderFightResultReq crossLadderFightResultReq = new CrossLadderFightResultReq();
		crossLadderFightResultReq.PlayerId = PlayerId;
		crossLadderFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)crossLadderFightResultReq, Message.CrossLadderFightResultReq);
	}

	public void CrossLadder_FightResult_Ack(ByteString response)
	{
		CrossLadderFightResultAck crossLadderFightResultAck = CrossLadderFightResultAck.Parser.ParseFrom(response);
		if (crossLadderFightResultAck != null)
		{
			zbUtil.Debug2LogFile("CrossLadder_FightResult_Ack.txt", crossLadderFightResultAck);
			zbSelf.Instance.crossLadderHandler.UpdateCrossLadderInfo(crossLadderFightResultAck.CrossLadderInfo);
			zbCombatSystem.Instance.battleAwardList = crossLadderFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(crossLadderFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(crossLadderFightResultAck.BattleSettle);
		}
	}

	public void CrossLadder_BuyCombat_Req(int Count)
	{
		CrossLadderBuyCombatReq crossLadderBuyCombatReq = new CrossLadderBuyCombatReq();
		crossLadderBuyCombatReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)crossLadderBuyCombatReq, Message.CrossLadderBuyCombatReq);
	}

	public void CrossLadder_BuyCombat_Ack(ByteString response)
	{
		CrossLadderBuyCombatAck crossLadderBuyCombatAck = CrossLadderBuyCombatAck.Parser.ParseFrom(response);
		if (crossLadderBuyCombatAck != null)
		{
			zbSelf.Instance.crossLadderHandler.UpdateCrossLadderInfo(crossLadderBuyCombatAck.CrossLadderInfo);
			zbSelf.Instance.dataHandler.DataChange(crossLadderBuyCombatAck.DataChange);
		}
	}

	public void CrossLadder_UpgradeFightStart_Req()
	{
		CrossLadderUpgradeFightStartReq req = new CrossLadderUpgradeFightStartReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderUpgradeFightStartReq);
	}

	public void CrossLadder_UpgradeFightStart_Ack(ByteString response)
	{
		CrossLadderUpgradeFightStartAck crossLadderUpgradeFightStartAck = CrossLadderUpgradeFightStartAck.Parser.ParseFrom(response);
		if (crossLadderUpgradeFightStartAck != null)
		{
			zbUtil.Debug2LogFile("CrossLadder_UpgradeFightStart_Ack.txt", crossLadderUpgradeFightStartAck);
			zbSelf.Instance.crossLadderHandler.playerId = (int)crossLadderUpgradeFightStartAck.BattleInfo.DefenderTeamInfo.MasterTeamInfo.RoleBaseInfo.RoleId;
			zbSelf.Instance.combatHandler.StartShowCombat(crossLadderUpgradeFightStartAck.BattleInfo);
		}
	}

	public void CrossLadder_UpgradeFightResult_Req(int PlayerId, BattleResultInfo BattleResult)
	{
		CrossLadderUpgradeFightResultReq crossLadderUpgradeFightResultReq = new CrossLadderUpgradeFightResultReq();
		crossLadderUpgradeFightResultReq.PlayerId = PlayerId;
		crossLadderUpgradeFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)crossLadderUpgradeFightResultReq, Message.CrossLadderUpgradeFightResultReq);
	}

	public void CrossLadder_UpgradeFightResult_Ack(ByteString response)
	{
		CrossLadderUpgradeFightResultAck crossLadderUpgradeFightResultAck = CrossLadderUpgradeFightResultAck.Parser.ParseFrom(response);
		if (crossLadderUpgradeFightResultAck != null)
		{
			zbUtil.Debug2LogFile("CrossLadder_UpgradeFightResult_Ack.txt", crossLadderUpgradeFightResultAck);
			zbSelf.Instance.crossLadderHandler.isUpgradeAck = true;
			zbSelf.Instance.crossLadderHandler.UpdateCrossLadderInfo(crossLadderUpgradeFightResultAck.CrossLadderInfo);
			zbCombatSystem.Instance.SetBanlanceData(crossLadderUpgradeFightResultAck.BattleSettle);
		}
	}

	public void CrossLadder_UpgradeConfirm_Req()
	{
		CrossLadderUpgradeConfirmReq req = new CrossLadderUpgradeConfirmReq();
		lastLadderId = zbServerData.sGameData.CrossLadderInfo.AchieveInfo.LadderId;
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderUpgradeConfirmReq);
	}

	public void CrossLadder_UpgradeConfirm_Ack(ByteString response)
	{
		CrossLadderUpgradeConfirmAck crossLadderUpgradeConfirmAck = CrossLadderUpgradeConfirmAck.Parser.ParseFrom(response);
		if (crossLadderUpgradeConfirmAck != null)
		{
			zbSelf.Instance.crossLadderHandler.UpdateCrossLadderInfo(crossLadderUpgradeConfirmAck.CrossLadderInfo);
			if (lastLadderId < crossLadderUpgradeConfirmAck.CrossLadderInfo.AchieveInfo.LadderId)
			{
				zbUIMgr.Instance.ShowCrossLadderUpgrade(crossLadderUpgradeConfirmAck.OverPct);
			}
			else
			{
				zbUIMgr.Instance.ShowCrossLadderUpgradeFail();
			}
		}
	}

	public void CrossLadder_TargetAward_Req()
	{
		CrossLadderTargetAwardReq req = new CrossLadderTargetAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderTargetAwardReq);
	}

	public void CrossLadder_TargetAward_Ack(ByteString response)
	{
		CrossLadderTargetAwardAck crossLadderTargetAwardAck = CrossLadderTargetAwardAck.Parser.ParseFrom(response);
		if (crossLadderTargetAwardAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(crossLadderTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)crossLadderTargetAwardAck.AwardList).ToArray());
			zbSelf.Instance.crossLadderHandler.UpdateCrossLadderInfo(crossLadderTargetAwardAck.CrossLadderInfo);
		}
	}

	public void CrossLadder_RecordList_Req()
	{
		CrossLadderRecordListReq req = new CrossLadderRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderRecordListReq);
	}

	public void CrossLadder_RecordList_Ack(ByteString response)
	{
		CrossLadderRecordListAck crossLadderRecordListAck = CrossLadderRecordListAck.Parser.ParseFrom(response);
		if (crossLadderRecordListAck != null)
		{
			zbUIMgr.Instance.ShowCrossLadderRecord(((IEnumerable<CrossLadderRecordDetail>)crossLadderRecordListAck.RecordList).ToArray());
		}
	}

	public void CrossLadder_FriendList_Req(bool openRank = false)
	{
		CrossLadderFriendListReq req = new CrossLadderFriendListReq();
		openCrossLadderRankWnd = openRank;
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossLadderFriendListReq);
	}

	public void CrossLadder_FriendList_Ack(ByteString response)
	{
		CrossLadderFriendListAck crossLadderFriendListAck = CrossLadderFriendListAck.Parser.ParseFrom(response);
		if (crossLadderFriendListAck != null)
		{
			zbSelf.Instance.crossLadderHandler.UpdateCrossLadderFriendRankList(((IEnumerable<CrossLadderFriendDetail>)crossLadderFriendListAck.FriendList).ToArray());
			if (openCrossLadderRankWnd)
			{
				zbUIMgr.Instance.ShowCrossLadderRank();
			}
		}
	}

	public void CrossPeak_Data_Req(bool openWnd = false)
	{
		CrossPeakDataReq req = new CrossPeakDataReq();
		openCrossPeakWnd = openWnd;
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossPeakDataReq);
	}

	public void CrossPeak_Data_Ack(ByteString response)
	{
		CrossPeakDataAck crossPeakDataAck = CrossPeakDataAck.Parser.ParseFrom(response);
		if (crossPeakDataAck != null)
		{
			zbSelf.Instance.crossPeakHandler.UpdateCrossPeakInfo(crossPeakDataAck.CrossPeakSelfInfo, crossPeakDataAck.CrossPeakRankInfo);
			zbSelf.Instance.crossPeakHandler.UpdateRankList(((IEnumerable<CrossPeakRankDetail>)crossPeakDataAck.RankList).ToArray());
			if (openCrossPeakWnd)
			{
				zbUIMgr.Instance.ShowCrossPeak();
			}
		}
	}

	public void CrossPeak_FightStart_Req(int PlayerId, int rank)
	{
		CrossPeakFightStartReq crossPeakFightStartReq = new CrossPeakFightStartReq();
		crossPeakFightStartReq.PlayerId = PlayerId;
		crossPeakFightStartReq.Rank = rank;
		Instance.WsPackMsg((IMessage)(object)crossPeakFightStartReq, Message.CrossPeakFightStartReq);
	}

	public void CrossPeak_FightStart_Ack(ByteString response)
	{
		CrossPeakFightStartAck crossPeakFightStartAck = CrossPeakFightStartAck.Parser.ParseFrom(response);
		if (crossPeakFightStartAck != null)
		{
			zbUtil.Debug2LogFile("CrossPeak_FightStart_Ack.txt", crossPeakFightStartAck);
			zbSelf.Instance.crossPeakHandler.currPlayerId = (int)crossPeakFightStartAck.BattleInfo.DefenderTeamInfo.MasterTeamInfo.RoleBaseInfo.RoleId;
			zbSelf.Instance.crossPeakHandler.currRank = crossPeakFightStartAck.Rank;
			zbSelf.Instance.combatHandler.StartShowCombat(crossPeakFightStartAck.BattleInfo);
		}
	}

	public void CrossPeak_FightResult_Req(int PlayerId, int rank, BattleResultInfo BattleResult)
	{
		CrossPeakFightResultReq crossPeakFightResultReq = new CrossPeakFightResultReq();
		crossPeakFightResultReq.PlayerId = PlayerId;
		crossPeakFightResultReq.Rank = rank;
		crossPeakFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)crossPeakFightResultReq, Message.CrossPeakFightResultReq);
	}

	public void CrossPeak_FightResult_Ack(ByteString response)
	{
		CrossPeakFightResultAck crossPeakFightResultAck = CrossPeakFightResultAck.Parser.ParseFrom(response);
		if (crossPeakFightResultAck != null)
		{
			zbUtil.Debug2LogFile("CrossPeak_FightResult_Ack.txt", crossPeakFightResultAck);
			zbSelf.Instance.crossPeakHandler.settleRankInfo = crossPeakFightResultAck.SettleRankInfo;
			zbSelf.Instance.crossPeakHandler.UpdateCrossPeakInfo(crossPeakFightResultAck.CrossPeakSelfInfo, null);
			zbCombatSystem.Instance.battleAwardList = crossPeakFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(crossPeakFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(crossPeakFightResultAck.BattleSettle);
		}
	}

	public void CrossPeak_SaoDang_Req(int PlayerId, int rank)
	{
		CrossPeakSaoDangReq crossPeakSaoDangReq = new CrossPeakSaoDangReq();
		crossPeakSaoDangReq.PlayerId = PlayerId;
		crossPeakSaoDangReq.Rank = rank;
		Instance.WsPackMsg((IMessage)(object)crossPeakSaoDangReq, Message.CrossPeakSaoDangReq);
	}

	public void CrossPeak_SaoDang_Ack(ByteString response)
	{
		CrossPeakSaoDangAck crossPeakSaoDangAck = CrossPeakSaoDangAck.Parser.ParseFrom(response);
		if (crossPeakSaoDangAck != null)
		{
			zbSelf.Instance.crossPeakHandler.UpdateCrossPeakInfo(crossPeakSaoDangAck.CrossPeakSelfInfo, null);
			zbSelf.Instance.dataHandler.DataChange(crossPeakSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)crossPeakSaoDangAck.AwardList).ToArray());
		}
	}

	public void CrossPeak_RecordList_Req()
	{
		CrossPeakRecordListReq req = new CrossPeakRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossPeakRecordListReq);
	}

	public void CrossPeak_RecordList_Ack(ByteString response)
	{
		CrossPeakRecordListAck crossPeakRecordListAck = CrossPeakRecordListAck.Parser.ParseFrom(response);
		if (crossPeakRecordListAck == null)
		{
			return;
		}
		zbSelf.Instance.truckHandler.RobRecordList = new TruckRobRecordDetail[((IEnumerable<CrossPeakRecordDetail>)crossPeakRecordListAck.RecordList).Count()];
		zbUIMgr.Instance.ShowCrossPeakRecord(((IEnumerable<CrossPeakRecordDetail>)crossPeakRecordListAck.RecordList).ToArray());
		int num = 0;
		foreach (CrossPeakRecordDetail record in crossPeakRecordListAck.RecordList)
		{
			zbSelf.Instance.truckHandler.RobRecordList[num].UUID = record.UUID;
			zbSelf.Instance.truckHandler.RobRecordList[num].FightType = record.FightType;
			zbSelf.Instance.truckHandler.RobRecordList[num].ResultScore = record.ResultScore;
			zbSelf.Instance.truckHandler.RobRecordList[num].RecordTm = record.RecordTm;
			zbSelf.Instance.truckHandler.RobRecordList[num].RoleBaseInfo = record.RoleBaseInfo;
			zbSelf.Instance.truckHandler.RobRecordList[num].UUID = record.UUID;
		}
	}

	public void CrossPeak_Top_Req()
	{
		CrossPeakTopReq req = new CrossPeakTopReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossPeakTopReq);
	}

	public void CrossPeak_Top_Ack(ByteString response)
	{
		CrossPeakTopAck crossPeakTopAck = CrossPeakTopAck.Parser.ParseFrom(response);
		if (crossPeakTopAck != null)
		{
			zbSelf.Instance.crossPeakHandler.UpdateCrossPeakTop(crossPeakTopAck.RoleBaseInfo);
		}
	}

	public void CrossPeak_BuyCombat_Req(int Count)
	{
		CrossPeakBuyCombatReq crossPeakBuyCombatReq = new CrossPeakBuyCombatReq();
		crossPeakBuyCombatReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)crossPeakBuyCombatReq, Message.CrossPeakBuyCombatReq);
	}

	public void CrossPeak_BuyCombat_Ack(ByteString response)
	{
		CrossPeakBuyCombatAck crossPeakBuyCombatAck = CrossPeakBuyCombatAck.Parser.ParseFrom(response);
		if (crossPeakBuyCombatAck != null)
		{
			zbSelf.Instance.crossPeakHandler.UpdateCrossPeakInfo(crossPeakBuyCombatAck.CrossPeakSelfInfo, null);
			zbSelf.Instance.dataHandler.DataChange(crossPeakBuyCombatAck.DataChange);
		}
	}

	public void EliteQuest_Data_Req()
	{
		EliteQuestDataReq req = new EliteQuestDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.EliteQuestDataReq);
	}

	public void EliteQuest_Data_Ack(ByteString response)
	{
		EliteQuestDataAck eliteQuestDataAck = EliteQuestDataAck.Parser.ParseFrom(response);
		if (eliteQuestDataAck != null)
		{
			zbServerData.sGameData.EliteQuestInfo = eliteQuestDataAck.EliteQuestInfo;
			zbServerData.sGameData.HelpInfo = eliteQuestDataAck.HelpInfo;
		}
	}

	public void EliteQuest_FightStart_Req(int LevelId, long m_joinRoleId, int fightCount)
	{
		EliteQuestFightStartReq eliteQuestFightStartReq = new EliteQuestFightStartReq();
		eliteQuestFightStartReq.LevelId = LevelId;
		eliteQuestFightStartReq.PlayerId = m_joinRoleId;
		eliteQuestFightStartReq.Count = fightCount;
		Instance.WsPackMsg((IMessage)(object)eliteQuestFightStartReq, Message.EliteQuestFightStartReq);
	}

	public void EliteQuest_FightStart_Ack(ByteString response)
	{
		EliteQuestFightStartAck eliteQuestFightStartAck = EliteQuestFightStartAck.Parser.ParseFrom(response);
		if (eliteQuestFightStartAck != null)
		{
			zbUtil.Debug2LogFile("EliteQuest_FightStart.txt", eliteQuestFightStartAck);
			zbSelf.Instance.combatHandler.StartShowCombat(eliteQuestFightStartAck.BattleInfo);
		}
	}

	public void EliteQuest_FightResult_Req(int LevelId, BattleResultInfo BattleResult, int fightCount, long playerId = 0L)
	{
		EliteQuestFightResultReq eliteQuestFightResultReq = new EliteQuestFightResultReq();
		eliteQuestFightResultReq.LevelId = LevelId;
		eliteQuestFightResultReq.BattleResult = BattleResult;
		eliteQuestFightResultReq.Count = fightCount;
		eliteQuestFightResultReq.PlayerId = playerId;
		Instance.WsPackMsg((IMessage)(object)eliteQuestFightResultReq, Message.EliteQuestFightResultReq);
	}

	public void EliteQuest_FightResult_Ack(ByteString response)
	{
		EliteQuestFightResultAck eliteQuestFightResultAck = EliteQuestFightResultAck.Parser.ParseFrom(response);
		if (eliteQuestFightResultAck != null)
		{
			zbUtil.Debug2LogFile("EliteQuest_FightResult_Ack.txt", eliteQuestFightResultAck);
			zbServerData.sGameData.HelpInfo = eliteQuestFightResultAck.HelpInfo;
			zbServerData.sGameData.EliteQuestInfo = eliteQuestFightResultAck.EliteQuestInfo;
			zbSelf.Instance.dataHandler.DataChange(eliteQuestFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = eliteQuestFightResultAck.AwardList;
			((zbCombatBalanceWnd)zbCombatSystem.Instance.SetBanlanceData(eliteQuestFightResultAck.BattleSettle)).ShowNewRecord(eliteQuestFightResultAck.BestScore);
		}
	}

	public void EliteQuest_BuyCombat_Req(int Count)
	{
		EliteQuestBuyCombatReq eliteQuestBuyCombatReq = new EliteQuestBuyCombatReq();
		eliteQuestBuyCombatReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)eliteQuestBuyCombatReq, Message.EliteQuestBuyCombatReq);
	}

	public void EliteQuest_BuyCombat_Ack(ByteString response)
	{
		EliteQuestBuyCombatAck eliteQuestBuyCombatAck = EliteQuestBuyCombatAck.Parser.ParseFrom(response);
		if (eliteQuestBuyCombatAck != null)
		{
			zbServerData.sGameData.EliteQuestInfo = eliteQuestBuyCombatAck.EliteQuestInfo;
			zbSelf.Instance.dataHandler.DataChange(eliteQuestBuyCombatAck.DataChange);
		}
	}

	public void EliteQuest_ScoreAward_Req(int levelId)
	{
		EliteQuestScoreAwardReq eliteQuestScoreAwardReq = new EliteQuestScoreAwardReq();
		eliteQuestScoreAwardReq.LevelId = levelId;
		Instance.WsPackMsg((IMessage)(object)eliteQuestScoreAwardReq, Message.EliteQuestScoreAwardReq);
	}

	public void EliteQuest_ScoreAward_Ack(ByteString response)
	{
		EliteQuestScoreAwardAck eliteQuestScoreAwardAck = EliteQuestScoreAwardAck.Parser.ParseFrom(response);
		if (eliteQuestScoreAwardAck != null)
		{
			zbServerData.sGameData.EliteQuestInfo = eliteQuestScoreAwardAck.EliteQuestInfo;
			if (zbUIMgr.Instance.IsShow(EWndId.MainQuestReceiveAward))
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Quest_Receive, eliteQuestScoreAwardAck);
				return;
			}
			zbSelf.Instance.dataHandler.DataChange(eliteQuestScoreAwardAck.DataChange, ((IEnumerable<AwardDetail>)eliteQuestScoreAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Quest_Refresh);
		}
	}

	public void EliteQuest_HelpRecordList_Req()
	{
		EliteQuestHelpRecordListReq req = new EliteQuestHelpRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.EliteQuestHelpRecordListReq);
	}

	public void EliteQuest_HelpRecordList_Ack(ByteString response)
	{
		EliteQuestHelpRecordListAck eliteQuestHelpRecordListAck = EliteQuestHelpRecordListAck.Parser.ParseFrom(response);
		if (eliteQuestHelpRecordListAck != null)
		{
			zbUIMgr.Instance.ShowEliteQuestThankLetter(((IEnumerable<HelpRecordDetail>)eliteQuestHelpRecordListAck.RecordList).ToArray());
		}
	}

	public void EliteQuest_HelpAward_Req(int recordId)
	{
		EliteQuestHelpAwardReq eliteQuestHelpAwardReq = new EliteQuestHelpAwardReq();
		eliteQuestHelpAwardReq.RecordId = recordId;
		Instance.WsPackMsg((IMessage)(object)eliteQuestHelpAwardReq, Message.EliteQuestHelpAwardReq);
	}

	public void EliteQuest_HelpAward_Ack(ByteString response)
	{
		EliteQuestHelpAwardAck eliteQuestHelpAwardAck = EliteQuestHelpAwardAck.Parser.ParseFrom(response);
		if (eliteQuestHelpAwardAck != null)
		{
			zbServerData.sGameData.HelpInfo = eliteQuestHelpAwardAck.HelpInfo;
			zbSelf.Instance.dataHandler.DataChange(eliteQuestHelpAwardAck.DataChange, ((IEnumerable<AwardDetail>)eliteQuestHelpAwardAck.AwardList).ToArray());
			zbSelf.Instance.eliteQuestHandler.eliteQuestHelpRecords = ((IEnumerable<HelpRecordDetail>)eliteQuestHelpAwardAck.RecordList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_QuestHelp_Award);
		}
	}

	public void EliteQuest_HelpAwardFast_Req()
	{
		EliteQuestHelpAwardFastReq req = new EliteQuestHelpAwardFastReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.EliteQuestHelpAwardFastReq);
	}

	public void EliteQuest_HelpAwardFast_Ack(ByteString response)
	{
		EliteQuestHelpAwardFastAck eliteQuestHelpAwardFastAck = EliteQuestHelpAwardFastAck.Parser.ParseFrom(response);
		if (eliteQuestHelpAwardFastAck != null)
		{
			zbServerData.sGameData.HelpInfo = eliteQuestHelpAwardFastAck.HelpInfo;
			zbSelf.Instance.dataHandler.DataChange(eliteQuestHelpAwardFastAck.DataChange, ((IEnumerable<AwardDetail>)eliteQuestHelpAwardFastAck.AwardList).ToArray());
			zbSelf.Instance.eliteQuestHandler.eliteQuestHelpRecords = ((IEnumerable<HelpRecordDetail>)eliteQuestHelpAwardFastAck.RecordList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_QuestHelp_Award);
		}
	}

	public void EndlessHorde_Data_Req(bool openWnd = false)
	{
		EndlessHordeDataReq req = new EndlessHordeDataReq();
		openEndlessHorde = openWnd;
		Instance.WsPackMsg((IMessage)(object)req, Message.EndlessHordeDataReq);
	}

	public void EndlessHorde_Data_Ack(ByteString response)
	{
		EndlessHordeDataAck endlessHordeDataAck = EndlessHordeDataAck.Parser.ParseFrom(response);
		if (endlessHordeDataAck != null)
		{
			zbSelf.Instance.endlessHordeHandler.UpdateEndlessHordeInfo(endlessHordeDataAck.EndlessHordeInfo);
			if (openEndlessHorde)
			{
				zbUIMgr.Instance.ShowEndLessHorde();
			}
		}
	}

	public void EndlessHorde_FightStart_Req(int LevelId)
	{
		EndlessHordeFightStartReq endlessHordeFightStartReq = new EndlessHordeFightStartReq();
		endlessHordeFightStartReq.LevelId = LevelId;
		Instance.WsPackMsg((IMessage)(object)endlessHordeFightStartReq, Message.EndlessHordeFightStartReq);
	}

	public void EndlessHorde_FightStart_Ack(ByteString response)
	{
		EndlessHordeFightStartAck endlessHordeFightStartAck = EndlessHordeFightStartAck.Parser.ParseFrom(response);
		if (endlessHordeFightStartAck != null)
		{
			zbSelf.Instance.combatHandler.StartShowCombat(endlessHordeFightStartAck.BattleInfo);
		}
	}

	public void EndlessHorde_FightForward_Req(int LevelId, BattleResultInfo BattleResult, int PassDistance)
	{
		EndlessHordeFightForwardReq endlessHordeFightForwardReq = new EndlessHordeFightForwardReq();
		endlessHordeFightForwardReq.LevelId = LevelId;
		endlessHordeFightForwardReq.BattleResult = BattleResult;
		endlessHordeFightForwardReq.PassDistance = PassDistance;
		Instance.WsPackMsg((IMessage)(object)endlessHordeFightForwardReq, Message.EndlessHordeFightForwardReq);
	}

	public void EndlessHorde_FightForward_Ack(ByteString response)
	{
		EndlessHordeFightForwardAck endlessHordeFightForwardAck = EndlessHordeFightForwardAck.Parser.ParseFrom(response);
		if (endlessHordeFightForwardAck != null)
		{
			zbSelf.Instance.endlessHordeHandler.UpdateEndlessHordeInfo(endlessHordeFightForwardAck.EndlessHordeInfo);
			zbCombatSystem.Instance.restoreData = null;
			zbCombatSystem.Instance.battleInfo = endlessHordeFightForwardAck.NextBattleInfo;
			zbCombatSystem.Instance.CaclEndlessHorde();
			zbCombatSystem.Instance.combatData.monsterSpawner.SpawnWaves();
			zbCombatSystem.Instance.m_isFightForward = false;
			zbUIMgr.Instance.ShowEndlessHordeFightForward();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_FightForward_EndlessHorde);
		}
	}

	public void EndlessHorde_FightResult_Req(int LevelId, BattleResultInfo BattleResult, int PassDistance)
	{
		EndlessHordeFightResultReq endlessHordeFightResultReq = new EndlessHordeFightResultReq();
		endlessHordeFightResultReq.LevelId = LevelId;
		endlessHordeFightResultReq.BattleResult = BattleResult;
		endlessHordeFightResultReq.PassDistance = PassDistance;
		Instance.WsPackMsg((IMessage)(object)endlessHordeFightResultReq, Message.EndlessHordeFightResultReq);
	}

	public void EndlessHorde_FightResult_Ack(ByteString response)
	{
		EndlessHordeFightResultAck endlessHordeFightResultAck = EndlessHordeFightResultAck.Parser.ParseFrom(response);
		if (endlessHordeFightResultAck != null)
		{
			zbSelf.Instance.endlessHordeHandler.UpdateEndlessHordeInfo(endlessHordeFightResultAck.EndlessHordeInfo);
			zbCombatSystem.Instance.battleAwardList = endlessHordeFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(endlessHordeFightResultAck.DataChange);
			_ = (zbCombatBalanceWnd)zbCombatSystem.Instance.SetBanlanceData(endlessHordeFightResultAck.BattleSettle);
		}
	}

	public void EndlessHorde_BuyCombat_Req(int Count)
	{
		EndlessHordeBuyCombatReq endlessHordeBuyCombatReq = new EndlessHordeBuyCombatReq();
		endlessHordeBuyCombatReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)endlessHordeBuyCombatReq, Message.EndlessHordeBuyCombatReq);
	}

	public void EndlessHorde_BuyCombat_Ack(ByteString response)
	{
		EndlessHordeBuyCombatAck endlessHordeBuyCombatAck = EndlessHordeBuyCombatAck.Parser.ParseFrom(response);
		if (endlessHordeBuyCombatAck != null)
		{
			zbSelf.Instance.endlessHordeHandler.UpdateEndlessHordeInfo(endlessHordeBuyCombatAck.EndlessHordeInfo);
			zbSelf.Instance.dataHandler.DataChange(endlessHordeBuyCombatAck.DataChange);
		}
	}

	public void EndlessHorde_SaoDang_Req(int Count)
	{
		EndlessHordeSaoDangReq endlessHordeSaoDangReq = new EndlessHordeSaoDangReq();
		endlessHordeSaoDangReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)endlessHordeSaoDangReq, Message.EndlessHordeSaoDangReq);
	}

	public void EndlessHorde_SaoDang_Ack(ByteString response)
	{
		EndlessHordeSaoDangAck endlessHordeSaoDangAck = EndlessHordeSaoDangAck.Parser.ParseFrom(response);
		if (endlessHordeSaoDangAck != null)
		{
			zbSelf.Instance.endlessHordeHandler.UpdateEndlessHordeInfo(endlessHordeSaoDangAck.EndlessHordeInfo);
			zbSelf.Instance.dataHandler.DataChange(endlessHordeSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)endlessHordeSaoDangAck.AwardList).ToArray());
		}
	}

	public void RechargeActive_ZhanLingEndlessAward_Req(int Distance)
	{
		RechargeActiveZhanLingEndlessAwardReq rechargeActiveZhanLingEndlessAwardReq = new RechargeActiveZhanLingEndlessAwardReq();
		rechargeActiveZhanLingEndlessAwardReq.Distance = Distance;
		Instance.WsPackMsg((IMessage)(object)rechargeActiveZhanLingEndlessAwardReq, Message.RechargeActiveZhanLingEndlessAwardReq);
	}

	public void RechargeActive_ZhanLingEndlessAward_Ack(ByteString response)
	{
		RechargeActiveZhanLingEndlessAwardAck rechargeActiveZhanLingEndlessAwardAck = RechargeActiveZhanLingEndlessAwardAck.Parser.ParseFrom(response);
		if (rechargeActiveZhanLingEndlessAwardAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveZhanLingEndlessAwardAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeActiveZhanLingEndlessAwardAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveZhanLingEndlessAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_EndlessHorde);
		}
	}

	public void EndlessMine_Data_Req(bool openWnd = false)
	{
		EndlessMineDataReq req = new EndlessMineDataReq();
		openEndlessMineWnd = openWnd;
		Instance.WsPackMsg((IMessage)(object)req, Message.EndlessMineDataReq);
	}

	public void EndlessMine_Data_Ack(ByteString response)
	{
		EndlessMineDataAck endlessMineDataAck = EndlessMineDataAck.Parser.ParseFrom(response);
		if (endlessMineDataAck == null)
		{
			return;
		}
		zbSelf.Instance.endlessMineHandler.UpdateEndlessMineInfo(endlessMineDataAck.EndlessMineInfo);
		if (openEndlessMineWnd)
		{
			if (endlessMineDataAck.EndlessMineInfo.EndlessMineBaseInfo.ActivateAward == EBoolState.False)
			{
				zbUIMgr.Instance.ShowEndlessMineActive();
			}
			else
			{
				zbUIMgr.Instance.ShowEndlessMine();
			}
		}
	}

	public void EndlessMine_ActivateAward_Req()
	{
		EndlessMineActivateAwardReq req = new EndlessMineActivateAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.EndlessMineActivateAwardReq);
	}

	public void EndlessMine_ActivateAward_Ack(ByteString response)
	{
		EndlessMineActivateAwardAck endlessMineActivateAwardAck = EndlessMineActivateAwardAck.Parser.ParseFrom(response);
		if (endlessMineActivateAwardAck != null)
		{
			zbSelf.Instance.endlessMineHandler.UpdateEndlessMineBaseInfo(endlessMineActivateAwardAck.EndlessMineBaseInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EndlessMineActivateAward_Ack, endlessMineActivateAwardAck);
		}
	}

	public void EndlessMine_Dig_Req(int index)
	{
		EndlessMineDigReq endlessMineDigReq = new EndlessMineDigReq();
		endlessMineDigReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)endlessMineDigReq, Message.EndlessMineDigReq);
	}

	public void EndlessMine_Dig_Ack(ByteString response)
	{
		EndlessMineDigAck endlessMineDigAck = EndlessMineDigAck.Parser.ParseFrom(response);
		if (endlessMineDigAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(endlessMineDigAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EndlessMine_GetProps_Ack, ((IEnumerable<AwardDetail>)endlessMineDigAck.AwardList).ToArray());
			zbSelf.Instance.endlessMineHandler.UpdateEndlessMineInfo(endlessMineDigAck.EndlessMineInfo);
		}
	}

	public void EndlessMine_Drill_Req(int index)
	{
		EndlessMineDrillReq endlessMineDrillReq = new EndlessMineDrillReq();
		endlessMineDrillReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)endlessMineDrillReq, Message.EndlessMineDrillReq);
	}

	public void EndlessMine_Drill_Ack(ByteString response)
	{
		EndlessMineDrillAck endlessMineDrillAck = EndlessMineDrillAck.Parser.ParseFrom(response);
		if (endlessMineDrillAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(endlessMineDrillAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EndlessMine_GetProps_Ack, ((IEnumerable<AwardDetail>)endlessMineDrillAck.AwardList).ToArray());
			zbSelf.Instance.endlessMineHandler.UpdateEndlessMineInfo(endlessMineDrillAck.EndlessMineInfo);
		}
	}

	public void EndlessMine_Bomb_Req(int index)
	{
		EndlessMineBombReq endlessMineBombReq = new EndlessMineBombReq();
		endlessMineBombReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)endlessMineBombReq, Message.EndlessMineBombReq);
	}

	public void EndlessMine_Bomb_Ack(ByteString response)
	{
		EndlessMineBombAck endlessMineBombAck = EndlessMineBombAck.Parser.ParseFrom(response);
		if (endlessMineBombAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(endlessMineBombAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EndlessMine_GetProps_Ack, ((IEnumerable<AwardDetail>)endlessMineBombAck.AwardList).ToArray());
			zbSelf.Instance.endlessMineHandler.UpdateEndlessMineInfo(endlessMineBombAck.EndlessMineInfo);
		}
	}

	public void EndlessMine_PropBuy_Req(int propId)
	{
		EndlessMinePropBuyReq endlessMinePropBuyReq = new EndlessMinePropBuyReq();
		endlessMinePropBuyReq.PropId = propId;
		Instance.WsPackMsg((IMessage)(object)endlessMinePropBuyReq, Message.EndlessMinePropBuyReq);
	}

	public void EndlessMine_PropBuy_Ack(ByteString response)
	{
		EndlessMinePropBuyAck endlessMinePropBuyAck = EndlessMinePropBuyAck.Parser.ParseFrom(response);
		if (endlessMinePropBuyAck != null)
		{
			zbSelf.Instance.endlessMineHandler.UpdateEndlessMineBaseInfo(endlessMinePropBuyAck.EndlessMineBaseInfo);
			zbSelf.Instance.dataHandler.DataChange(endlessMinePropBuyAck.DataChange, ((IEnumerable<AwardDetail>)endlessMinePropBuyAck.AwardList).ToArray());
		}
	}

	public void EndlessMine_ExchangeAward_Req()
	{
		EndlessMineExchangeAwardReq req = new EndlessMineExchangeAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.EndlessMineExchangeAwardReq);
	}

	public void EndlessMine_ExchangeAward_Ack(ByteString response)
	{
		EndlessMineExchangeAwardAck endlessMineExchangeAwardAck = EndlessMineExchangeAwardAck.Parser.ParseFrom(response);
		if (endlessMineExchangeAwardAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EndlessMine_ExchangeAward_Ack, endlessMineExchangeAwardAck);
		}
	}

	public void EndlessMine_TargetAward_Req(int index)
	{
		EndlessMineTargetAwardReq endlessMineTargetAwardReq = new EndlessMineTargetAwardReq();
		endlessMineTargetAwardReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)endlessMineTargetAwardReq, Message.EndlessMineTargetAwardReq);
	}

	public void EndlessMine_TargetAward_Ack(ByteString response)
	{
		EndlessMineTargetAwardAck endlessMineTargetAwardAck = EndlessMineTargetAwardAck.Parser.ParseFrom(response);
		if (endlessMineTargetAwardAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(endlessMineTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)endlessMineTargetAwardAck.AwardList).ToArray());
			zbSelf.Instance.endlessMineHandler.UpdateEndlessMineBaseInfo(endlessMineTargetAwardAck.EndlessMineBaseInfo);
		}
	}

	public void EquipSlot_LevelUpgrade_Req(EEquipType EquipType)
	{
		EquipSlotLevelUpgradeReq equipSlotLevelUpgradeReq = new EquipSlotLevelUpgradeReq();
		equipSlotLevelUpgradeReq.EquipType = EquipType;
		Instance.WsPackMsg((IMessage)(object)equipSlotLevelUpgradeReq, Message.EquipSlotLevelUpgradeReq);
	}

	public void EquipSlot_LevelUpgrade_Ack(ByteString response)
	{
		EquipSlotLevelUpgradeAck equipSlotLevelUpgradeAck = EquipSlotLevelUpgradeAck.Parser.ParseFrom(response);
		if (equipSlotLevelUpgradeAck != null)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_LEVELUP);
			zbSelf.Instance.dataHandler.DataChange(equipSlotLevelUpgradeAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotLevelUpgradeAck.EquipSlotInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EquipSlotUpgrade_Ack);
		}
	}

	public void EquipSlot_LevelUpgradeAll_Req()
	{
		EquipSlotLevelUpgradeAllReq req = new EquipSlotLevelUpgradeAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.EquipSlotLevelUpgradeAllReq);
	}

	public void EquipSlot_LevelUpgradeAll_Ack(ByteString response)
	{
		EquipSlotLevelUpgradeAllAck equipSlotLevelUpgradeAllAck = EquipSlotLevelUpgradeAllAck.Parser.ParseFrom(response);
		if (equipSlotLevelUpgradeAllAck != null)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_LEVELUP);
			zbSelf.Instance.dataHandler.DataChange(equipSlotLevelUpgradeAllAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotLevelUpgradeAllAck.EquipSlotInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EquipSlotUpgrade_Ack);
		}
	}

	public void EquipSlot_EquipSet_Req(EEquipType EquipType, string EquipId)
	{
		EquipSlotEquipSetReq equipSlotEquipSetReq = new EquipSlotEquipSetReq();
		equipSlotEquipSetReq.EquipType = EquipType;
		equipSlotEquipSetReq.EquipId = EquipId;
		Instance.WsPackMsg((IMessage)(object)equipSlotEquipSetReq, Message.EquipSlotEquipSetReq);
	}

	public void EquipSlot_EquipSet_Ack(ByteString response)
	{
		EquipSlotEquipSetAck equipSlotEquipSetAck = EquipSlotEquipSetAck.Parser.ParseFrom(response);
		if (equipSlotEquipSetAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(equipSlotEquipSetAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotEquipSetAck.EquipSlotInfo);
		}
	}

	public void EquipSlot_EquipUpgrade_Req(EEquipType EquipType)
	{
		EquipSlotEquipUpgradeReq equipSlotEquipUpgradeReq = new EquipSlotEquipUpgradeReq();
		equipSlotEquipUpgradeReq.EquipType = EquipType;
		Instance.WsPackMsg((IMessage)(object)equipSlotEquipUpgradeReq, Message.EquipSlotEquipUpgradeReq);
	}

	public void EquipSlot_EquipUpgrade_Ack(ByteString response)
	{
		EquipSlotEquipUpgradeAck equipSlotEquipUpgradeAck = EquipSlotEquipUpgradeAck.Parser.ParseFrom(response);
		if (equipSlotEquipUpgradeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(equipSlotEquipUpgradeAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotEquipUpgradeAck.EquipSlotInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EquipSlotUpgrade_Ack);
		}
	}

	public void EquipSlot_EquipLock_Req(EEquipType EquipType, EBoolState Lock)
	{
		EquipSlotEquipLockReq equipSlotEquipLockReq = new EquipSlotEquipLockReq();
		equipSlotEquipLockReq.EquipType = EquipType;
		equipSlotEquipLockReq.Lock = Lock;
		Instance.WsPackMsg((IMessage)(object)equipSlotEquipLockReq, Message.EquipSlotEquipLockReq);
	}

	public void EquipSlot_EquipLock_Ack(ByteString response)
	{
		EquipSlotEquipLockAck equipSlotEquipLockAck = EquipSlotEquipLockAck.Parser.ParseFrom(response);
		if (equipSlotEquipLockAck != null)
		{
			zbServerData.sGameData.EquipSlotInfo = equipSlotEquipLockAck.EquipSlotInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_EquipLock_Ack);
		}
	}

	public void EquipSlot_GemSet_Req(int GemPlanId, EEquipType EquipType, int GemTid)
	{
		EquipSlotGemSetReq equipSlotGemSetReq = new EquipSlotGemSetReq();
		equipSlotGemSetReq.GemPlanId = GemPlanId;
		equipSlotGemSetReq.EquipType = EquipType;
		equipSlotGemSetReq.GemTid = GemTid;
		Instance.WsPackMsg((IMessage)(object)equipSlotGemSetReq, Message.EquipSlotGemSetReq);
	}

	public void EquipSlot_GemSet_Ack(ByteString response)
	{
		EquipSlotGemSetAck equipSlotGemSetAck = EquipSlotGemSetAck.Parser.ParseFrom(response);
		if (equipSlotGemSetAck != null)
		{
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_success.ogg");
			zbSelf.Instance.dataHandler.DataChange(equipSlotGemSetAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotGemSetAck.EquipSlotInfo);
		}
	}

	public void EquipSlot_GemReplace_Req(int GemPlanId, EEquipType EquipType, int Index, int GemTid)
	{
		EquipSlotGemReplaceReq equipSlotGemReplaceReq = new EquipSlotGemReplaceReq();
		equipSlotGemReplaceReq.GemPlanId = GemPlanId;
		equipSlotGemReplaceReq.EquipType = EquipType;
		equipSlotGemReplaceReq.Index = Index;
		equipSlotGemReplaceReq.GemTid = GemTid;
		Instance.WsPackMsg((IMessage)(object)equipSlotGemReplaceReq, Message.EquipSlotGemReplaceReq);
	}

	public void EquipSlot_GemReplace_Ack(ByteString response)
	{
		EquipSlotGemReplaceAck equipSlotGemReplaceAck = EquipSlotGemReplaceAck.Parser.ParseFrom(response);
		if (equipSlotGemReplaceAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(equipSlotGemReplaceAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotGemReplaceAck.EquipSlotInfo);
		}
	}

	public void EquipSlot_GemRemove_Req(int GemPlanId, EEquipType EquipType, int Index)
	{
		EquipSlotGemRemoveReq equipSlotGemRemoveReq = new EquipSlotGemRemoveReq();
		equipSlotGemRemoveReq.GemPlanId = GemPlanId;
		equipSlotGemRemoveReq.EquipType = EquipType;
		equipSlotGemRemoveReq.Index = Index;
		Instance.WsPackMsg((IMessage)(object)equipSlotGemRemoveReq, Message.EquipSlotGemRemoveReq);
	}

	public void EquipSlot_GemRemove_Ack(ByteString response)
	{
		EquipSlotGemRemoveAck equipSlotGemRemoveAck = EquipSlotGemRemoveAck.Parser.ParseFrom(response);
		if (equipSlotGemRemoveAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(equipSlotGemRemoveAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotGemRemoveAck.EquipSlotInfo);
		}
	}

	public void EquipSlot_GemPlan_Req(int GemPlanId)
	{
		EquipSlotGemPlanReq equipSlotGemPlanReq = new EquipSlotGemPlanReq();
		equipSlotGemPlanReq.GemPlanId = GemPlanId;
		Instance.WsPackMsg((IMessage)(object)equipSlotGemPlanReq, Message.EquipSlotGemPlanReq);
	}

	public void EquipSlot_GemPlan_Ack(ByteString response)
	{
		EquipSlotGemPlanAck equipSlotGemPlanAck = EquipSlotGemPlanAck.Parser.ParseFrom(response);
		if (equipSlotGemPlanAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(equipSlotGemPlanAck.DataChange);
			zbSelf.Instance.equipHandler.UpdateEquipSlotInfo(equipSlotGemPlanAck.EquipSlotInfo);
		}
	}

	public void Equip_Decompose_Req(string[] SelectList)
	{
		EquipDecomposeReq equipDecomposeReq = new EquipDecomposeReq();
		equipDecomposeReq.SelectList.Clear();
		equipDecomposeReq.SelectList.AddRange((IEnumerable<string>)SelectList);
		Instance.WsPackMsg((IMessage)(object)equipDecomposeReq, Message.EquipDecomposeReq);
	}

	public void Equip_Decompose_Ack(ByteString response)
	{
		EquipDecomposeAck equipDecomposeAck = EquipDecomposeAck.Parser.ParseFrom(response);
		if (equipDecomposeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(equipDecomposeAck.DataChange, ((IEnumerable<AwardDetail>)equipDecomposeAck.AwardList).ToArray());
		}
	}

	public void Equip_Lock_Req(string EquipId, EBoolState Lock)
	{
		EquipLockReq equipLockReq = new EquipLockReq();
		equipLockReq.EquipId = EquipId;
		equipLockReq.Lock = Lock;
		Instance.WsPackMsg((IMessage)(object)equipLockReq, Message.EquipLockReq);
	}

	public void Equip_Lock_Ack(ByteString response)
	{
		EquipLockAck equipLockAck = EquipLockAck.Parser.ParseFrom(response);
		if (equipLockAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(equipLockAck.DataChange);
		}
	}

	public void Gem_ComposePreview_Req()
	{
		GemComposePreviewReq req = new GemComposePreviewReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.GemComposePreviewReq);
	}

	public void Gem_ComposePreview_Ack(ByteString response)
	{
		GemComposePreviewAck gemComposePreviewAck = GemComposePreviewAck.Parser.ParseFrom(response);
		if (gemComposePreviewAck != null)
		{
			if (gemComposePreviewAck.PreviewList.Count > 0)
			{
				zbUIMgr.Instance.ShowGemCompose(((IEnumerable<ResourceDetail>)gemComposePreviewAck.PreviewList).ToArray());
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10063"));
			}
		}
	}

	public void Gem_Compose_Req()
	{
		GemComposeReq req = new GemComposeReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.GemComposeReq);
	}

	public void Gem_Compose_Ack(ByteString response)
	{
		GemComposeAck gemComposeAck = GemComposeAck.Parser.ParseFrom(response);
		if (gemComposeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(gemComposeAck.DataChange, ((IEnumerable<AwardDetail>)gemComposeAck.AwardList).ToArray());
		}
	}

	public void Gem_Lock_Req(int GemTid, EBoolState Lock)
	{
		GemLockReq gemLockReq = new GemLockReq();
		gemLockReq.GemTid = GemTid;
		gemLockReq.Lock = Lock;
		Instance.WsPackMsg((IMessage)(object)gemLockReq, Message.GemLockReq);
	}

	public void Gem_Lock_Ack(ByteString response)
	{
		GemLockAck gemLockAck = GemLockAck.Parser.ParseFrom(response);
		if (gemLockAck != null)
		{
			zbServerData.sGameData.LockInfo = gemLockAck.LockInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gem);
		}
	}

	public void Gem_Decompose_Req(int GemTid)
	{
		GemDecomposeReq gemDecomposeReq = new GemDecomposeReq();
		gemDecomposeReq.GemTid = GemTid;
		Instance.WsPackMsg((IMessage)(object)gemDecomposeReq, Message.GemDecomposeReq);
	}

	public void Gem_Decompose_Ack(ByteString response)
	{
		GemDecomposeAck gemDecomposeAck = GemDecomposeAck.Parser.ParseFrom(response);
		if (gemDecomposeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(gemDecomposeAck.DataChange, ((IEnumerable<AwardDetail>)gemDecomposeAck.AwardList).ToArray());
		}
	}

	public void Gem_Cast_Req(int GemTid)
	{
		GemCastReq gemCastReq = new GemCastReq();
		gemCastReq.GemTid = GemTid;
		Instance.WsPackMsg((IMessage)(object)gemCastReq, Message.GemCastReq);
	}

	public void Gem_Cast_Ack(ByteString response)
	{
		GemCastAck gemCastAck = GemCastAck.Parser.ParseFrom(response);
		if (gemCastAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(gemCastAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_GemCast_Ack, gemCastAck);
		}
	}

	public void Facility_Data_Req()
	{
		FacilityDataReq req = new FacilityDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FacilityDataReq);
	}

	public void Facility_Data_Ack(ByteString response)
	{
		FacilityDataAck facilityDataAck = FacilityDataAck.Parser.ParseFrom(response);
		if (facilityDataAck != null)
		{
			zbServerData.sGameData.FacilitySelfInfo = facilityDataAck.FacilitySelfInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
		}
	}

	public void Facility_Recommend_Req()
	{
		FacilityRecommendReq req = new FacilityRecommendReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FacilityRecommendReq);
	}

	public void Facility_Recommend_Ack(ByteString response)
	{
		FacilityRecommendAck facilityRecommendAck = FacilityRecommendAck.Parser.ParseFrom(response);
		if (facilityRecommendAck != null)
		{
			zbSelf.Instance.facilityHandler.recommendList = ((IEnumerable<FacilityBaseInfo>)facilityRecommendAck.RecommendList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
		}
	}

	public void Facility_Create_Req(int type)
	{
		FacilityCreateReq facilityCreateReq = new FacilityCreateReq();
		facilityCreateReq.Type = type;
		Instance.WsPackMsg((IMessage)(object)facilityCreateReq, Message.FacilityCreateReq);
	}

	public void Facility_Create_Ack(ByteString response)
	{
		FacilityCreateAck facilityCreateAck = FacilityCreateAck.Parser.ParseFrom(response);
		if (facilityCreateAck != null)
		{
			zbServerData.sGameData.FacilitySelfInfo = facilityCreateAck.FacilitySelfInfo;
			zbSelf.Instance.facilityHandler.facilityMapInfo = facilityCreateAck.FacilityMapInfo;
			zbSelf.Instance.dataHandler.DataChange(facilityCreateAck.DataChange);
			zbUIMgr.Instance.ShowFacility();
		}
	}

	public void Facility_Close_Req(int facilityId)
	{
		FacilityCloseReq facilityCloseReq = new FacilityCloseReq();
		facilityCloseReq.FacilityId = facilityId;
		Instance.WsPackMsg((IMessage)(object)facilityCloseReq, Message.FacilityCloseReq);
	}

	public void Facility_Close_Ack(ByteString response)
	{
		FacilityCloseAck facilityCloseAck = FacilityCloseAck.Parser.ParseFrom(response);
		if (facilityCloseAck != null)
		{
			zbServerData.sGameData.FacilitySelfInfo = facilityCloseAck.FacilitySelfInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
			zbUIMgr.Instance.HideWnd(EWndId.Facility);
			zbUIMgr.Instance.HideWnd(EWndId.FacilityResult);
			zbSelf.Instance.facilityHandler.facilityMapInfo = null;
		}
	}

	public void Facility_View_Req(int facilityId, bool openFacilityWnd = false)
	{
		FacilityViewReq facilityViewReq = new FacilityViewReq();
		facilityViewReq.FacilityId = facilityId;
		m_openFacilityWnd = openFacilityWnd;
		Instance.WsPackMsg((IMessage)(object)facilityViewReq, Message.FacilityViewReq);
	}

	public void Facility_View_Ack(ByteString response)
	{
		FacilityViewAck facilityViewAck = FacilityViewAck.Parser.ParseFrom(response);
		if (facilityViewAck != null)
		{
			zbSelf.Instance.facilityHandler.facilityMapInfo = facilityViewAck.FacilityMapInfo;
			if (m_openFacilityWnd)
			{
				zbUIMgr.Instance.ShowFacility();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
			}
		}
	}

	public void Facility_RoomPreview_Req(int facilityId, int roomId)
	{
		FacilityRoomPreviewReq facilityRoomPreviewReq = new FacilityRoomPreviewReq();
		facilityRoomPreviewReq.FacilityId = facilityId;
		facilityRoomPreviewReq.RoomId = roomId;
		Instance.WsPackMsg((IMessage)(object)facilityRoomPreviewReq, Message.FacilityRoomPreviewReq);
	}

	public void Facility_RoomPreview_Ack(ByteString response)
	{
		FacilityRoomPreviewAck ack = FacilityRoomPreviewAck.Parser.ParseFrom(response);
		if (ack == null)
		{
			return;
		}
		zbSelf.Instance.facilityHandler.UpdateFacilityRoomDetail(ack.RoomDetail);
		if (ack.RoomDetail.RoomType == EFacilityRoomType.Monster)
		{
			zbUIMgr.Instance.ShowFacilityEventMonster(ack.RoomDetail, ack.RobotBaseInfo);
		}
		else if (ack.RoomDetail.RoomType == EFacilityRoomType.Shop)
		{
			long num = ack.RoomDetail.ExplorerExpire - zbSelf.Instance.timeHandler.GetServerTime();
			if (ack.RoomDetail.ExplorerRole != zbSelf.Instance.GetRoleId() && num > 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11276"), zbUtil.FormatTimeMMSS(num)));
				return;
			}
			string message = string.Format(zbUtil.Language("lang.11273"), $"<color=red>{ack.ShopLeftInfo.TotalLeft}</color>");
			zbUIMgr.Instance.ShowFacilityMessage(zbUtil.Language("lang.10051"), message, ack.RoomDetail.RoomState == EFacilityRoomState.Ready, delegate(bool isOK)
			{
				if (isOK)
				{
					Facility_EventTrigger_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, ack.RoomDetail.RoomId);
				}
			}, 0L);
		}
		else if (ack.RoomDetail.RoomType == EFacilityRoomType.Rolling)
		{
			Facility_EventTrigger_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, ack.RoomDetail.RoomId);
		}
		else if (ack.RoomDetail.RoomType == EFacilityRoomType.LockedChest)
		{
			if (ack.RoomDetail.RoomState == EFacilityRoomState.Completed)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11279"));
				return;
			}
			long num2 = ack.RoomDetail.ExplorerExpire - zbSelf.Instance.timeHandler.GetServerTime();
			if (ack.RoomDetail.ExplorerRole != zbSelf.Instance.GetRoleId() && num2 > 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11277"), zbUtil.FormatTimeMMSS(num2)));
			}
			else
			{
				zbUIMgr.Instance.ShowFacilityEventChest(ack.RoomDetail);
			}
		}
		else
		{
			if (ack.RoomDetail.RoomType != EFacilityRoomType.ExitStair)
			{
				return;
			}
			AwardDetail award = ((ack.RoomDetail.RoomState == EFacilityRoomState.Ready) ? ack.RoomDetail.RoomAward : null);
			zbUIMgr.Instance.ShowFacilityMessage(zbUtil.Language("lang.11269"), zbUtil.Language("lang.11270"), ack.RoomDetail.RoomState == EFacilityRoomState.Ready, delegate(bool isOK)
			{
				if (isOK)
				{
					Instance.Facility_EventTrigger_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, ack.RoomDetail.RoomId);
				}
			}, 0L, award);
		}
	}

	public void Facility_EventTrigger_Req(int facilityId, int roomId, bool needExploreAnim = false, bool openCombatWnd = false, bool openLockedChest = false)
	{
		zbSelf.Instance.facilityHandler.ResetFacilityTrigger();
		FacilityEventTriggerReq facilityEventTriggerReq = new FacilityEventTriggerReq();
		facilityEventTriggerReq.FacilityId = facilityId;
		facilityEventTriggerReq.RoomId = roomId;
		m_needExploreAnim = needExploreAnim;
		m_openCombatWnd = openCombatWnd;
		m_openLockedChest = openLockedChest;
		Instance.WsPackMsg((IMessage)(object)facilityEventTriggerReq, Message.FacilityEventTriggerReq);
	}

	public void Facility_EventCommon_Ack(ByteString response)
	{
		FacilityEventCommonAck facilityEventCommonAck = FacilityEventCommonAck.Parser.ParseFrom(response);
		if (facilityEventCommonAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(facilityEventCommonAck.DataChange);
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityEventCommonAck.FacilityMapInfo);
			if (m_needExploreAnim)
			{
				zbSelf.Instance.facilityHandler.UpdateFacilityTrigger(facilityEventCommonAck.RoomDetail, ((IEnumerable<AwardDetail>)facilityEventCommonAck.AwardList).ToArray());
			}
			else
			{
				zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)facilityEventCommonAck.AwardList).ToArray());
			}
		}
	}

	public void Facility_EventLockedChest_Ack(ByteString response)
	{
		FacilityEventLockedChestAck facilityEventLockedChestAck = FacilityEventLockedChestAck.Parser.ParseFrom(response);
		if (facilityEventLockedChestAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(facilityEventLockedChestAck.DataChange);
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityEventLockedChestAck.FacilityMapInfo);
			if (m_needExploreAnim)
			{
				zbSelf.Instance.facilityHandler.UpdateFacilityTrigger(facilityEventLockedChestAck.RoomDetail, ((IEnumerable<AwardDetail>)facilityEventLockedChestAck.AwardList).ToArray());
			}
			else if (m_openLockedChest)
			{
				Facility_OpenChest_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, facilityEventLockedChestAck.RoomDetail.RoomId);
			}
			else
			{
				zbUIMgr.Instance.ShowFacilityEventChest(facilityEventLockedChestAck.RoomDetail);
			}
		}
	}

	public void Facility_EventMonster_Ack(ByteString response)
	{
		FacilityEventMonsterAck facilityEventMonsterAck = FacilityEventMonsterAck.Parser.ParseFrom(response);
		if (facilityEventMonsterAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(facilityEventMonsterAck.DataChange);
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityEventMonsterAck.FacilityMapInfo);
			if (m_needExploreAnim)
			{
				zbSelf.Instance.facilityHandler.UpdateFacilityTrigger(facilityEventMonsterAck.RoomDetail, ((IEnumerable<AwardDetail>)facilityEventMonsterAck.AwardList).ToArray(), null, facilityEventMonsterAck);
			}
			else if (m_openCombatWnd)
			{
				zbSelf.Instance.combatHandler.StartShowCombat(facilityEventMonsterAck.BattleInfo);
			}
			else
			{
				zbUIMgr.Instance.ShowFacilityEventMonster(facilityEventMonsterAck.RoomDetail, facilityEventMonsterAck.RobotBaseInfo, facilityEventMonsterAck.BattleInfo);
			}
		}
	}

	public void Facility_EventShop_Ack(ByteString response)
	{
		FacilityEventShopAck facilityEventShopAck = FacilityEventShopAck.Parser.ParseFrom(response);
		if (facilityEventShopAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(facilityEventShopAck.DataChange);
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityEventShopAck.FacilityMapInfo);
			if (m_needExploreAnim)
			{
				zbSelf.Instance.facilityHandler.UpdateFacilityTrigger(facilityEventShopAck.RoomDetail, ((IEnumerable<AwardDetail>)facilityEventShopAck.AwardList).ToArray(), facilityEventShopAck.FacilityShopInfo);
			}
			else
			{
				zbUIMgr.Instance.ShowFacilityEventShop(facilityEventShopAck.RoomDetail, facilityEventShopAck.FacilityShopInfo);
			}
		}
	}

	public void Facility_EventRolling_Ack(ByteString response)
	{
		FacilityEventRollingAck facilityEventRollingAck = FacilityEventRollingAck.Parser.ParseFrom(response);
		if (facilityEventRollingAck != null)
		{
			zbServerData.sGameData.FacilitySelfInfo.Rolling = facilityEventRollingAck.Rolling;
			zbSelf.Instance.dataHandler.DataChange(facilityEventRollingAck.DataChange);
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityEventRollingAck.FacilityMapInfo);
			if (m_needExploreAnim)
			{
				zbSelf.Instance.facilityHandler.UpdateFacilityTrigger(facilityEventRollingAck.RoomDetail, ((IEnumerable<AwardDetail>)facilityEventRollingAck.AwardList).ToArray());
			}
			else
			{
				zbUIMgr.Instance.ShowFacilityEventRolling(facilityEventRollingAck.RoomDetail);
			}
		}
	}

	public void Facility_EventStair_Ack(ByteString response)
	{
		FacilityEventStairAck facilityEventStairAck = FacilityEventStairAck.Parser.ParseFrom(response);
		if (facilityEventStairAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(facilityEventStairAck.DataChange);
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityEventStairAck.FacilityMapInfo);
			if (m_needExploreAnim)
			{
				zbSelf.Instance.facilityHandler.UpdateFacilityTrigger(facilityEventStairAck.RoomDetail, ((IEnumerable<AwardDetail>)facilityEventStairAck.AwardList).ToArray());
				return;
			}
			Facility_StairTeleport_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, facilityEventStairAck.RoomDetail.RoomId);
			zbUIMgr.Instance.ShowAward(((IEnumerable<AwardDetail>)facilityEventStairAck.AwardList).ToArray());
		}
	}

	public void Facility_StairTeleport_Req(int facilityId, int roomId)
	{
		FacilityStairTeleportReq facilityStairTeleportReq = new FacilityStairTeleportReq();
		facilityStairTeleportReq.FacilityId = facilityId;
		facilityStairTeleportReq.RoomId = roomId;
		Instance.WsPackMsg((IMessage)(object)facilityStairTeleportReq, Message.FacilityStairTeleportReq);
	}

	public void Facility_StairTeleport_Ack(ByteString response)
	{
		FacilityStairTeleportAck facilityStairTeleportAck = FacilityStairTeleportAck.Parser.ParseFrom(response);
		if (facilityStairTeleportAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_FacilityStairTeleport_Ack, facilityStairTeleportAck);
		}
	}

	public void Facility_OpenChest_Req(int facilityId, int roomId)
	{
		FacilityOpenChestReq facilityOpenChestReq = new FacilityOpenChestReq();
		facilityOpenChestReq.FacilityId = facilityId;
		facilityOpenChestReq.RoomId = roomId;
		Instance.WsPackMsg((IMessage)(object)facilityOpenChestReq, Message.FacilityOpenChestReq);
	}

	public void Facility_OpenChest_Ack(ByteString response)
	{
		FacilityOpenChestAck facilityOpenChestAck = FacilityOpenChestAck.Parser.ParseFrom(response);
		if (facilityOpenChestAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(facilityOpenChestAck.DataChange);
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityOpenChestAck.FacilityMapInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_FacilityOpenChest_Ack, facilityOpenChestAck);
		}
	}

	public void Facility_FightResult_Req(int facilityId, int roomId, BattleResultInfo battleResult)
	{
		FacilityFightResultReq facilityFightResultReq = new FacilityFightResultReq();
		facilityFightResultReq.FacilityId = facilityId;
		facilityFightResultReq.RoomId = roomId;
		facilityFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)facilityFightResultReq, Message.FacilityFightResultReq);
	}

	public void Facility_FightResult_Ack(ByteString response)
	{
		FacilityFightResultAck facilityFightResultAck = FacilityFightResultAck.Parser.ParseFrom(response);
		if (facilityFightResultAck != null)
		{
			zbSelf.Instance.facilityHandler.UpdateFacilityMapInfo(facilityFightResultAck.FacilityMapInfo);
			zbCombatSystem.Instance.battleAwardList = facilityFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(facilityFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(facilityFightResultAck.BattleSettle);
		}
	}

	public void Facility_Rolling_Req(int facilityId, int roomId)
	{
		FacilityRollingReq facilityRollingReq = new FacilityRollingReq();
		facilityRollingReq.FacilityId = facilityId;
		facilityRollingReq.RoomId = roomId;
		Instance.WsPackMsg((IMessage)(object)facilityRollingReq, Message.FacilityRollingReq);
	}

	public void Facility_Rolling_Ack(ByteString response)
	{
		FacilityRollingAck facilityRollingAck = FacilityRollingAck.Parser.ParseFrom(response);
		if (facilityRollingAck != null)
		{
			zbServerData.sGameData.FacilitySelfInfo.Rolling = facilityRollingAck.Rolling;
			zbSelf.Instance.dataHandler.DataChange(facilityRollingAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_FacilityRolling_Ack, facilityRollingAck);
		}
	}

	public void Facility_RollingEnd_Req(int facilityId, int roomId)
	{
		FacilityRollingEndReq facilityRollingEndReq = new FacilityRollingEndReq();
		facilityRollingEndReq.FacilityId = facilityId;
		facilityRollingEndReq.RoomId = roomId;
		Instance.WsPackMsg((IMessage)(object)facilityRollingEndReq, Message.FacilityRollingEndReq);
	}

	public void Facility_RollingEnd_Ack(ByteString response)
	{
		FacilityRollingEndAck facilityRollingEndAck = FacilityRollingEndAck.Parser.ParseFrom(response);
		if (facilityRollingEndAck != null)
		{
			zbSelf.Instance.facilityHandler.facilityMapInfo = facilityRollingEndAck.FacilityMapInfo;
			zbServerData.sGameData.FacilitySelfInfo.Rolling = facilityRollingEndAck.Rolling;
			zbSelf.Instance.dataHandler.DataChange(facilityRollingEndAck.DataChange, ((IEnumerable<AwardDetail>)facilityRollingEndAck.AwardList).ToArray());
			zbUIMgr.Instance.HideWnd(EWndId.FacilityEventRolling);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
		}
	}

	public void Facility_ShopBuy_Req(int facilityId, int roomId, int shopSlot, int buyCount)
	{
		FacilityShopBuyReq facilityShopBuyReq = new FacilityShopBuyReq();
		facilityShopBuyReq.FacilityId = facilityId;
		facilityShopBuyReq.RoomId = roomId;
		facilityShopBuyReq.ShopSlot = shopSlot;
		facilityShopBuyReq.BuyCount = buyCount;
		Instance.WsPackMsg((IMessage)(object)facilityShopBuyReq, Message.FacilityShopBuyReq);
	}

	public void Facility_ShopBuy_Ack(ByteString response)
	{
		FacilityShopBuyAck facilityShopBuyAck = FacilityShopBuyAck.Parser.ParseFrom(response);
		if (facilityShopBuyAck != null)
		{
			zbSelf.Instance.facilityHandler.facilityMapInfo = facilityShopBuyAck.FacilityMapInfo;
			zbSelf.Instance.dataHandler.DataChange(facilityShopBuyAck.DataChange, ((IEnumerable<AwardDetail>)facilityShopBuyAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_FacilityShopBuy_Ack, facilityShopBuyAck);
		}
	}

	public void Facility_Quit_Req(int facilityId)
	{
		FacilityQuitReq facilityQuitReq = new FacilityQuitReq();
		facilityQuitReq.FacilityId = facilityId;
		Instance.WsPackMsg((IMessage)(object)facilityQuitReq, Message.FacilityQuitReq);
	}

	public void Facility_Quit_Ack(ByteString response)
	{
		FacilityQuitAck facilityQuitAck = FacilityQuitAck.Parser.ParseFrom(response);
		if (facilityQuitAck != null)
		{
			zbServerData.sGameData.FacilitySelfInfo = facilityQuitAck.FacilitySelfInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
		}
	}

	public void Foothold_Data_Req(int index = -1)
	{
		m_openFootholdWnd = index;
		FootholdDataReq req = new FootholdDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FootholdDataReq);
	}

	public void Foothold_Data_Ack(ByteString response)
	{
		FootholdDataAck footholdDataAck = FootholdDataAck.Parser.ParseFrom(response);
		if (footholdDataAck != null)
		{
			zbSelf.Instance.guardHandler.UpdateFootholdInfo(footholdDataAck.FootholdInfo);
			if (m_openFootholdWnd != -1)
			{
				zbUIMgr.Instance.ShowFoothold(m_openFootholdWnd);
			}
		}
	}

	public void Foothold_Active_Req(int FootholdId)
	{
		FootholdActiveReq footholdActiveReq = new FootholdActiveReq();
		footholdActiveReq.FootholdId = FootholdId;
		Instance.WsPackMsg((IMessage)(object)footholdActiveReq, Message.FootholdActiveReq);
	}

	public void Foothold_Active_Ack(ByteString response)
	{
		FootholdActiveAck footholdActiveAck = FootholdActiveAck.Parser.ParseFrom(response);
		if (footholdActiveAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(footholdActiveAck.DataChange);
			zbSelf.Instance.guardHandler.UpdateFootholdInfo(footholdActiveAck.FootholdInfo);
			zbUIMgr.Instance.ShowFootholdGet(footholdActiveAck.FootholdId);
		}
	}

	public void Foothold_Set_Req(int Index, int FootholdId)
	{
		FootholdSetReq footholdSetReq = new FootholdSetReq();
		footholdSetReq.Index = Index;
		footholdSetReq.FootholdId = FootholdId;
		Instance.WsPackMsg((IMessage)(object)footholdSetReq, Message.FootholdSetReq);
	}

	public void Foothold_Set_Ack(ByteString response)
	{
		FootholdSetAck footholdSetAck = FootholdSetAck.Parser.ParseFrom(response);
		if (footholdSetAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(footholdSetAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_PvpSet_ModSubmit);
		}
	}

	public void Foothold_UpgradeStar_Req(int FootholdId)
	{
		FootholdUpgradeStarReq footholdUpgradeStarReq = new FootholdUpgradeStarReq();
		footholdUpgradeStarReq.FootholdId = FootholdId;
		curStarFootholdId = FootholdId;
		Instance.WsPackMsg((IMessage)(object)footholdUpgradeStarReq, Message.FootholdUpgradeStarReq);
	}

	public void Foothold_UpgradeStar_Ack(ByteString response)
	{
		FootholdUpgradeStarAck footholdUpgradeStarAck = FootholdUpgradeStarAck.Parser.ParseFrom(response);
		if (footholdUpgradeStarAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(footholdUpgradeStarAck.DataChange);
			zbSelf.Instance.guardHandler.UpdateFootholdInfo(footholdUpgradeStarAck.FootholdInfo);
			zbUIMgr.Instance.ShowFootholdStarSuccess(curStarFootholdId);
		}
	}

	public void Foothold_Convert_Req(int[] SelectList)
	{
		FootholdConvertReq footholdConvertReq = new FootholdConvertReq();
		footholdConvertReq.SelectList.Clear();
		footholdConvertReq.SelectList.AddRange((IEnumerable<int>)SelectList);
		Instance.WsPackMsg((IMessage)(object)footholdConvertReq, Message.FootholdConvertReq);
	}

	public void Foothold_Convert_Ack(ByteString response)
	{
		FootholdConvertAck footholdConvertAck = FootholdConvertAck.Parser.ParseFrom(response);
		if (footholdConvertAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(footholdConvertAck.DataChange, ((IEnumerable<AwardDetail>)footholdConvertAck.AwardList).ToArray());
		}
	}

	public void Friends_List_Req()
	{
		FriendsListReq req = new FriendsListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FriendsListReq);
	}

	public void Friends_List_Ack(ByteString response)
	{
		FriendsListAck friendsListAck = FriendsListAck.Parser.ParseFrom(response);
		if (friendsListAck != null)
		{
			zbSelf.Instance.friendHandler.UpdateFriendsList(friendsListAck.FriendsList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_Recommend_Req()
	{
		FriendsRecommendReq req = new FriendsRecommendReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FriendsRecommendReq);
	}

	public void Friends_Recommend_Ack(ByteString response)
	{
		FriendsRecommendAck friendsRecommendAck = FriendsRecommendAck.Parser.ParseFrom(response);
		if (friendsRecommendAck != null)
		{
			zbSelf.Instance.friendHandler.UpdateRecommendList(friendsRecommendAck.RecommendList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_Search_Req(string Content)
	{
		FriendsSearchReq friendsSearchReq = new FriendsSearchReq();
		friendsSearchReq.Content = Content;
		Instance.WsPackMsg((IMessage)(object)friendsSearchReq, Message.FriendsSearchReq);
	}

	public void Friends_Search_Ack(ByteString response)
	{
		FriendsSearchAck friendsSearchAck = FriendsSearchAck.Parser.ParseFrom(response);
		if (friendsSearchAck != null)
		{
			zbSelf.Instance.friendHandler.UpdateSearchList(friendsSearchAck.SearchList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Search);
		}
	}

	public void Friends_Delete_Req(int PlayerId)
	{
		FriendsDeleteReq friendsDeleteReq = new FriendsDeleteReq();
		friendsDeleteReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)friendsDeleteReq, Message.FriendsDeleteReq);
	}

	public void Friends_Delete_Ack(ByteString response)
	{
		FriendsDeleteAck friendsDeleteAck = FriendsDeleteAck.Parser.ParseFrom(response);
		if (friendsDeleteAck != null)
		{
			zbServerData.sGameData.FriendsInfo = friendsDeleteAck.FriendsInfo;
			zbSelf.Instance.friendHandler.DeleteFriendDetailById(friendsDeleteAck.PlayerId);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Delete);
		}
	}

	public void Friends_Apply_Req(int PlayerId)
	{
		FriendsApplyReq friendsApplyReq = new FriendsApplyReq();
		friendsApplyReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)friendsApplyReq, Message.FriendsApplyReq);
	}

	public void Friends_Apply_Ack(ByteString response)
	{
		FriendsApplyAck.Parser.ParseFrom(response);
	}

	public void Friends_ApplyBatch_Req(int[] IdList)
	{
		FriendsApplyBatchReq friendsApplyBatchReq = new FriendsApplyBatchReq();
		friendsApplyBatchReq.IdList.Add((IEnumerable<int>)IdList);
		Instance.WsPackMsg((IMessage)(object)friendsApplyBatchReq, Message.FriendsApplyBatchReq);
	}

	public void Friends_ApplyBatch_Ack(ByteString response)
	{
		if (FriendsApplyBatchAck.Parser.ParseFrom(response) != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10064"));
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ApplyList_Req()
	{
		FriendsApplyListReq req = new FriendsApplyListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FriendsApplyListReq);
	}

	public void Friends_ApplyList_Ack(ByteString response)
	{
		FriendsApplyListAck friendsApplyListAck = FriendsApplyListAck.Parser.ParseFrom(response);
		if (friendsApplyListAck != null)
		{
			zbSelf.Instance.friendHandler.UpdateApplyList(friendsApplyListAck.ApplyList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ApplyPass_Req(int PlayerId)
	{
		FriendsApplyPassReq friendsApplyPassReq = new FriendsApplyPassReq();
		friendsApplyPassReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)friendsApplyPassReq, Message.FriendsApplyPassReq);
	}

	public void Friends_ApplyPass_Ack(ByteString response)
	{
		FriendsApplyPassAck friendsApplyPassAck = FriendsApplyPassAck.Parser.ParseFrom(response);
		if (friendsApplyPassAck != null)
		{
			zbSelf.Instance.friendHandler.AddOneFriendsDetail(friendsApplyPassAck.FriendsDetail);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ApplyPassAll_Req()
	{
		FriendsApplyPassAllReq req = new FriendsApplyPassAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FriendsApplyPassAllReq);
	}

	public void Friends_ApplyPassAll_Ack(ByteString response)
	{
		FriendsApplyPassAllAck friendsApplyPassAllAck = FriendsApplyPassAllAck.Parser.ParseFrom(response);
		if (friendsApplyPassAllAck != null)
		{
			zbSelf.Instance.friendHandler.UpdateApplyList(friendsApplyPassAllAck.ApplyList);
			zbSelf.Instance.friendHandler.UpdateFriendsList(friendsApplyPassAllAck.FriendsList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ApplyRefuse_Req(int PlayerId)
	{
		FriendsApplyRefuseReq friendsApplyRefuseReq = new FriendsApplyRefuseReq();
		friendsApplyRefuseReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)friendsApplyRefuseReq, Message.FriendsApplyRefuseReq);
	}

	public void Friends_ApplyRefuse_Ack(ByteString response)
	{
		FriendsApplyRefuseAck friendsApplyRefuseAck = FriendsApplyRefuseAck.Parser.ParseFrom(response);
		if (friendsApplyRefuseAck != null)
		{
			zbSelf.Instance.friendHandler.DeleteFriendsApplyListById(friendsApplyRefuseAck.PlayerId);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ApplyRefuseAll_Req()
	{
		FriendsApplyRefuseAllReq req = new FriendsApplyRefuseAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FriendsApplyRefuseAllReq);
	}

	public void Friends_ApplyRefuseAll_Ack(ByteString response)
	{
		FriendsApplyRefuseAllAck friendsApplyRefuseAllAck = FriendsApplyRefuseAllAck.Parser.ParseFrom(response);
		if (friendsApplyRefuseAllAck != null)
		{
			zbSelf.Instance.friendHandler.UpdateApplyList(friendsApplyRefuseAllAck.ApplyList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ActionGive_Req(int PlayerId)
	{
		FriendsActionGiveReq friendsActionGiveReq = new FriendsActionGiveReq();
		friendsActionGiveReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)friendsActionGiveReq, Message.FriendsActionGiveReq);
	}

	public void Friends_ActionGive_Ack(ByteString response)
	{
		FriendsActionGiveAck friendsActionGiveAck = FriendsActionGiveAck.Parser.ParseFrom(response);
		if (friendsActionGiveAck != null)
		{
			zbSelf.Instance.friendHandler.UpdateOneFriendDetail(friendsActionGiveAck.FriendsDetail);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ActionCollect_Req(int PlayerId)
	{
		FriendsActionCollectReq friendsActionCollectReq = new FriendsActionCollectReq();
		friendsActionCollectReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)friendsActionCollectReq, Message.FriendsActionCollectReq);
	}

	public void Friends_ActionCollect_Ack(ByteString response)
	{
		FriendsActionCollectAck friendsActionCollectAck = FriendsActionCollectAck.Parser.ParseFrom(response);
		if (friendsActionCollectAck != null)
		{
			zbServerData.sGameData.FriendsInfo = friendsActionCollectAck.FriendsInfo;
			zbSelf.Instance.friendHandler.UpdateOneFriendDetail(friendsActionCollectAck.FriendsDetail);
			zbSelf.Instance.dataHandler.DataChange(friendsActionCollectAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ActionGiveAll_Req()
	{
		FriendsActionGiveAllReq req = new FriendsActionGiveAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FriendsActionGiveAllReq);
	}

	public void Friends_ActionGiveAll_Ack(ByteString response)
	{
		FriendsActionGiveAllAck friendsActionGiveAllAck = FriendsActionGiveAllAck.Parser.ParseFrom(response);
		if (friendsActionGiveAllAck != null)
		{
			zbServerData.sGameData.FriendsInfo = friendsActionGiveAllAck.FriendsInfo;
			zbSelf.Instance.friendHandler.UpdateFriendsList(friendsActionGiveAllAck.FriendsList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void Friends_ActionCollectAll_Req()
	{
		FriendsActionCollectAllReq req = new FriendsActionCollectAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.FriendsActionCollectAllReq);
	}

	public void Friends_ActionCollectAll_Ack(ByteString response)
	{
		FriendsActionCollectAllAck friendsActionCollectAllAck = FriendsActionCollectAllAck.Parser.ParseFrom(response);
		if (friendsActionCollectAllAck != null)
		{
			zbServerData.sGameData.FriendsInfo = friendsActionCollectAllAck.FriendsInfo;
			zbSelf.Instance.friendHandler.UpdateFriendsList(friendsActionCollectAllAck.FriendsList);
			zbSelf.Instance.dataHandler.DataChange(friendsActionCollectAllAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Friends_Refresh);
		}
	}

	public void RechargeShop_GrowthGift_Req()
	{
		RechargeShopGrowthGiftReq req = new RechargeShopGrowthGiftReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.RechargeShopGrowthGiftReq);
	}

	public void RechargeShop_GrowthGift_Ack(ByteString response)
	{
		RechargeShopGrowthGiftAck rechargeShopGrowthGiftAck = RechargeShopGrowthGiftAck.Parser.ParseFrom(response);
		if (rechargeShopGrowthGiftAck == null)
		{
			return;
		}
		if (zbServerData.sGameData.RechargeShopInfo != null && zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop != null)
		{
			int count = zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop.Count;
			if (rechargeShopGrowthGiftAck.GrowthGiftShop.Count > count)
			{
				zbSelf.Instance.growthGiftHandler.isNewGrowPack = true;
			}
			zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop.Clear();
			zbServerData.sGameData.RechargeShopInfo.GrowthGiftShop.AddRange((IEnumerable<GrowthGiftShopDetail>)rechargeShopGrowthGiftAck.GrowthGiftShop);
		}
		zbSelf.Instance.growthGiftHandler.tmpRechargeShopGrowthGiftAck = rechargeShopGrowthGiftAck;
		zbSelf.Instance.growthGiftHandler.hasNotify = true;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_GrowthGift);
	}

	public void RechargeShop_VipCardAward_Req(int GoodsId)
	{
		RechargeShopVipCardAwardReq rechargeShopVipCardAwardReq = new RechargeShopVipCardAwardReq();
		rechargeShopVipCardAwardReq.GoodsId = GoodsId;
		Instance.WsPackMsg((IMessage)(object)rechargeShopVipCardAwardReq, Message.RechargeShopVipCardAwardReq);
	}

	public void RechargeShop_VipCardAward_Ack(ByteString response)
	{
		RechargeShopVipCardAwardAck rechargeShopVipCardAwardAck = RechargeShopVipCardAwardAck.Parser.ParseFrom(response);
		if (rechargeShopVipCardAwardAck != null)
		{
			zbServerData.sGameData.RechargeShopInfo = rechargeShopVipCardAwardAck.RechargeShopInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeShopVipCardAwardAck.DataChange, ((IEnumerable<AwardDetail>)rechargeShopVipCardAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void RechargeActive_Data_Req()
	{
		RechargeActiveDataReq req = new RechargeActiveDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.RechargeActiveDataReq);
	}

	public void RechargeActive_Data_Ack(ByteString response)
	{
		RechargeActiveDataAck rechargeActiveDataAck = RechargeActiveDataAck.Parser.ParseFrom(response);
		if (rechargeActiveDataAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveDataAck.RechargeActiveInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void RechargeActive_NovicePack_Req()
	{
		RechargeActiveNovicePackReq req = new RechargeActiveNovicePackReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.RechargeActiveNovicePackReq);
	}

	public void RechargeActive_NovicePack_Ack(ByteString response)
	{
		RechargeActiveNovicePackAck rechargeActiveNovicePackAck = RechargeActiveNovicePackAck.Parser.ParseFrom(response);
		if (rechargeActiveNovicePackAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveNovicePackAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeActiveNovicePackAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveNovicePackAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void RechargeActive_NovicePackExtra_Req(int goodsId)
	{
		RechargeActiveNovicePackExtraReq rechargeActiveNovicePackExtraReq = new RechargeActiveNovicePackExtraReq();
		rechargeActiveNovicePackExtraReq.GoodsId = goodsId;
		Instance.WsPackMsg((IMessage)(object)rechargeActiveNovicePackExtraReq, Message.RechargeActiveNovicePackExtraReq);
	}

	public void RechargeActive_NovicePackExtra_Ack(ByteString response)
	{
		RechargeActiveNovicePackExtraAck rechargeActiveNovicePackExtraAck = RechargeActiveNovicePackExtraAck.Parser.ParseFrom(response);
		if (rechargeActiveNovicePackExtraAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveNovicePackExtraAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeActiveNovicePackExtraAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveNovicePackExtraAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void Handbook_Data_Req(bool isOpen = false, int tab = 0)
	{
		openMonster = isOpen;
		openMonsterTab = tab;
		HandbookDataReq req = new HandbookDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.HandbookDataReq);
	}

	public void Handbook_Data_Ack(ByteString response)
	{
		HandbookDataAck handbookDataAck = HandbookDataAck.Parser.ParseFrom(response);
		if (handbookDataAck != null)
		{
			zbServerData.sGameData.HandbookInfo = handbookDataAck.HandbookInfo;
			if (openMonster)
			{
				zbUIMgr.Instance.ShowEncyclopedia(openMonsterTab);
				openMonsterTab = 0;
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Handbook);
			}
		}
	}

	public void Handbook_MonsterActive_Req(int handbookId)
	{
		HandbookMonsterActiveReq handbookMonsterActiveReq = new HandbookMonsterActiveReq();
		handbookMonsterActiveReq.HandbookId = handbookId;
		Instance.WsPackMsg((IMessage)(object)handbookMonsterActiveReq, Message.HandbookMonsterActiveReq);
	}

	public void Handbook_MonsterActive_Ack(ByteString response)
	{
		HandbookMonsterActiveAck handbookMonsterActiveAck = HandbookMonsterActiveAck.Parser.ParseFrom(response);
		if (handbookMonsterActiveAck != null)
		{
			zbServerData.sGameData.HandbookInfo = handbookMonsterActiveAck.HandbookInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_MonsterActive_Ack);
			zbSelf.Instance.dataHandler.DataChange(handbookMonsterActiveAck.DataChange, ((IEnumerable<AwardDetail>)handbookMonsterActiveAck.AwardList).ToArray());
		}
	}

	public void Handbook_MonsterUpgrade_Req(int handbookId)
	{
		HandbookMonsterUpgradeReq handbookMonsterUpgradeReq = new HandbookMonsterUpgradeReq();
		handbookMonsterUpgradeReq.HandbookId = handbookId;
		Instance.WsPackMsg((IMessage)(object)handbookMonsterUpgradeReq, Message.HandbookMonsterUpgradeReq);
	}

	public void Handbook_MonsterUpgrade_Ack(ByteString response)
	{
		HandbookMonsterUpgradeAck handbookMonsterUpgradeAck = HandbookMonsterUpgradeAck.Parser.ParseFrom(response);
		if (handbookMonsterUpgradeAck != null)
		{
			zbServerData.sGameData.HandbookInfo = handbookMonsterUpgradeAck.HandbookInfo;
			zbSelf.Instance.dataHandler.DataChange(handbookMonsterUpgradeAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Handbook);
		}
	}

	public void Handbook_ModActiveAward_Req(int handbookId)
	{
		HandbookModActiveAwardReq handbookModActiveAwardReq = new HandbookModActiveAwardReq();
		handbookModActiveAwardReq.HandbookId = handbookId;
		Instance.WsPackMsg((IMessage)(object)handbookModActiveAwardReq, Message.HandbookModActiveAwardReq);
	}

	public void Handbook_ModActiveAward_Ack(ByteString response)
	{
		HandbookModActiveAwardAck handbookModActiveAwardAck = HandbookModActiveAwardAck.Parser.ParseFrom(response);
		if (handbookModActiveAwardAck != null)
		{
			zbServerData.sGameData.HandbookInfo = handbookModActiveAwardAck.HandbookInfo;
			zbSelf.Instance.dataHandler.DataChange(handbookModActiveAwardAck.DataChange, ((IEnumerable<AwardDetail>)handbookModActiveAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_ModActive_Ack);
		}
	}

	public void Handbook_ModTypeTargetAward_Req(ESkillType skillType)
	{
		HandbookModTypeTargetAwardReq handbookModTypeTargetAwardReq = new HandbookModTypeTargetAwardReq();
		handbookModTypeTargetAwardReq.SkillType = skillType;
		Instance.WsPackMsg((IMessage)(object)handbookModTypeTargetAwardReq, Message.HandbookModTypeTargetAwardReq);
	}

	public void Handbook_ModTypeTargetAward_Ack(ByteString response)
	{
		HandbookModTypeTargetAwardAck handbookModTypeTargetAwardAck = HandbookModTypeTargetAwardAck.Parser.ParseFrom(response);
		if (handbookModTypeTargetAwardAck != null)
		{
			zbServerData.sGameData.HandbookInfo = handbookModTypeTargetAwardAck.HandbookInfo;
			zbSelf.Instance.dataHandler.DataChange(handbookModTypeTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)handbookModTypeTargetAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Handbook);
		}
	}

	public void Handbook_ModTargetAward_Req()
	{
		HandbookModTargetAwardReq req = new HandbookModTargetAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.HandbookModTargetAwardReq);
	}

	public void Handbook_ModTargetAward_Ack(ByteString response)
	{
		HandbookModTargetAwardAck handbookModTargetAwardAck = HandbookModTargetAwardAck.Parser.ParseFrom(response);
		if (handbookModTargetAwardAck != null)
		{
			zbServerData.sGameData.HandbookInfo = handbookModTargetAwardAck.HandbookInfo;
			zbSelf.Instance.dataHandler.DataChange(handbookModTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)handbookModTargetAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Handbook);
		}
	}

	public void Hero_Compose_Req(int HeroTid)
	{
		HeroComposeReq heroComposeReq = new HeroComposeReq();
		heroComposeReq.HeroTid = HeroTid;
		heroTid = HeroTid;
		Instance.WsPackMsg((IMessage)(object)heroComposeReq, Message.HeroComposeReq);
	}

	public void Hero_Compose_Ack(ByteString response)
	{
		HeroComposeAck heroComposeAck = HeroComposeAck.Parser.ParseFrom(response);
		if (heroComposeAck != null)
		{
			zbUIMgr.Instance.ShowHeroGet(new int[1] { heroTid });
			zbSelf.Instance.dataHandler.DataChange(heroComposeAck.DataChange);
		}
	}

	public void Hero_QualityUpgrade_Req(int HeroTid, int NextMod)
	{
		HeroQualityUpgradeReq heroQualityUpgradeReq = new HeroQualityUpgradeReq();
		heroQualityUpgradeReq.HeroTid = HeroTid;
		heroTid = HeroTid;
		nextMod = NextMod;
		Instance.WsPackMsg((IMessage)(object)heroQualityUpgradeReq, Message.HeroQualityUpgradeReq);
	}

	public void Hero_QualityUpgrade_Ack(ByteString response)
	{
		HeroQualityUpgradeAck heroQualityUpgradeAck = HeroQualityUpgradeAck.Parser.ParseFrom(response);
		if (heroQualityUpgradeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(heroQualityUpgradeAck.DataChange);
			bool isUpgrade = heroQualityUpgradeAck.OldQuality != heroQualityUpgradeAck.NewQuality;
			zbUIMgr.Instance.ShowHeroStarSuccess(heroTid, nextMod, isUpgrade, heroQualityUpgradeAck.OldQualityStar, heroQualityUpgradeAck.NewQualityStar);
		}
	}

	public void Hero_FragDecompose_Req(BagSelectDetail[] SelectList)
	{
		HeroFragDecomposeReq heroFragDecomposeReq = new HeroFragDecomposeReq();
		heroFragDecomposeReq.SelectList.Clear();
		heroFragDecomposeReq.SelectList.AddRange((IEnumerable<BagSelectDetail>)SelectList);
		Instance.WsPackMsg((IMessage)(object)heroFragDecomposeReq, Message.HeroFragDecomposeReq);
	}

	public void Hero_FragDecompose_Ack(ByteString response)
	{
		HeroFragDecomposeAck heroFragDecomposeAck = HeroFragDecomposeAck.Parser.ParseFrom(response);
		if (heroFragDecomposeAck != null)
		{
			zbSelf.Instance.heroHandler.heroFragDecomposeAck = heroFragDecomposeAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HeroFragDecompose_Ack);
		}
	}

	public void Hero_RarityUpgrade_Req(int HeroTid, BagSelectDetail[] SelectList)
	{
		HeroRarityUpgradeReq heroRarityUpgradeReq = new HeroRarityUpgradeReq();
		heroRarityUpgradeReq.HeroTid = HeroTid;
		heroRarityUpgradeReq.SelectList.Clear();
		heroRarityUpgradeReq.SelectList.AddRange((IEnumerable<BagSelectDetail>)SelectList);
		Instance.WsPackMsg((IMessage)(object)heroRarityUpgradeReq, Message.HeroRarityUpgradeReq);
	}

	public void Hero_RarityUpgrade_Ack(ByteString response)
	{
		HeroRarityUpgradeAck heroRarityUpgradeAck = HeroRarityUpgradeAck.Parser.ParseFrom(response);
		if (heroRarityUpgradeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(heroRarityUpgradeAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HeroRarityUpgrade_Ack);
		}
	}

	public void HeroAwaken_Upgrade_Req(ESkillType SkillType, int Count, EBoolState UseDiamond)
	{
		HeroAwakenUpgradeReq heroAwakenUpgradeReq = new HeroAwakenUpgradeReq();
		heroAwakenUpgradeReq.SkillType = SkillType;
		heroAwakenUpgradeReq.Count = Count;
		heroAwakenUpgradeReq.UseDiamond = UseDiamond;
		Instance.WsPackMsg((IMessage)(object)heroAwakenUpgradeReq, Message.HeroAwakenUpgradeReq);
	}

	public void HeroAwaken_Upgrade_Ack(ByteString response)
	{
		HeroAwakenUpgradeAck heroAwakenUpgradeAck = HeroAwakenUpgradeAck.Parser.ParseFrom(response);
		if (heroAwakenUpgradeAck != null)
		{
			zbServerData.sGameData.HeroAwakenInfo = heroAwakenUpgradeAck.HeroAwakenInfo;
			zbSelf.Instance.dataHandler.DataChange(heroAwakenUpgradeAck.DataChange);
		}
	}

	public void HeroAwaken_Advance_Req(ESkillType SkillType)
	{
		HeroAwakenAdvanceReq heroAwakenAdvanceReq = new HeroAwakenAdvanceReq();
		heroAwakenAdvanceReq.SkillType = SkillType;
		Instance.WsPackMsg((IMessage)(object)heroAwakenAdvanceReq, Message.HeroAwakenAdvanceReq);
	}

	public void HeroAwaken_Advance_Ack(ByteString response)
	{
		HeroAwakenAdvanceAck heroAwakenAdvanceAck = HeroAwakenAdvanceAck.Parser.ParseFrom(response);
		if (heroAwakenAdvanceAck != null)
		{
			zbServerData.sGameData.HeroAwakenInfo = heroAwakenAdvanceAck.HeroAwakenInfo;
			zbSelf.Instance.dataHandler.DataChange(heroAwakenAdvanceAck.DataChange);
		}
	}

	public void HeroAwaken_GeneOpenHole_Req(ESkillType SkillType)
	{
		HeroAwakenGeneOpenHoleReq heroAwakenGeneOpenHoleReq = new HeroAwakenGeneOpenHoleReq();
		heroAwakenGeneOpenHoleReq.SkillType = SkillType;
		openHoleSkillType = SkillType;
		Instance.WsPackMsg((IMessage)(object)heroAwakenGeneOpenHoleReq, Message.HeroAwakenGeneOpenHoleReq);
	}

	public void HeroAwaken_GeneOpenHole_Ack(ByteString response)
	{
		HeroAwakenGeneOpenHoleAck heroAwakenGeneOpenHoleAck = HeroAwakenGeneOpenHoleAck.Parser.ParseFrom(response);
		if (heroAwakenGeneOpenHoleAck != null)
		{
			zbSelf.Instance.geneHandler.CheckNewOpenGeneSlot(zbServerData.sGameData.HeroAwakenInfo, heroAwakenGeneOpenHoleAck.HeroAwakenInfo, openHoleSkillType);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HeroAwaken_GeneOpenHole_Ack, heroAwakenGeneOpenHoleAck);
		}
	}

	public void HeroAwaken_GeneRefreshHole_Req(ESkillType SkillType)
	{
		HeroAwakenGeneRefreshHoleReq heroAwakenGeneRefreshHoleReq = new HeroAwakenGeneRefreshHoleReq();
		heroAwakenGeneRefreshHoleReq.SkillType = SkillType;
		refreshHoleSkillType = SkillType;
		Instance.WsPackMsg((IMessage)(object)heroAwakenGeneRefreshHoleReq, Message.HeroAwakenGeneRefreshHoleReq);
	}

	public void HeroAwaken_GeneRefreshHole_Ack(ByteString response)
	{
		HeroAwakenGeneRefreshHoleAck heroAwakenGeneRefreshHoleAck = HeroAwakenGeneRefreshHoleAck.Parser.ParseFrom(response);
		if (heroAwakenGeneRefreshHoleAck != null)
		{
			zbServerData.sGameData.HeroAwakenInfo = heroAwakenGeneRefreshHoleAck.HeroAwakenInfo;
			zbSelf.Instance.dataHandler.DataChange(heroAwakenGeneRefreshHoleAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gene);
			if (zbSelf.Instance.geneHandler.IsAllSlotSameShape(refreshHoleSkillType))
			{
				zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
			}
		}
	}

	public void HeroAwaken_GeneSet_Req(ESkillType SkillType, int Index, int GeneTid)
	{
		HeroAwakenGeneSetReq heroAwakenGeneSetReq = new HeroAwakenGeneSetReq();
		heroAwakenGeneSetReq.SkillType = SkillType;
		heroAwakenGeneSetReq.Index = Index;
		heroAwakenGeneSetReq.GeneTid = GeneTid;
		Instance.WsPackMsg((IMessage)(object)heroAwakenGeneSetReq, Message.HeroAwakenGeneSetReq);
	}

	public void HeroAwaken_GeneSet_Ack(ByteString response)
	{
		HeroAwakenGeneSetAck heroAwakenGeneSetAck = HeroAwakenGeneSetAck.Parser.ParseFrom(response);
		if (heroAwakenGeneSetAck != null)
		{
			zbServerData.sGameData.HeroAwakenInfo = heroAwakenGeneSetAck.HeroAwakenInfo;
			zbSelf.Instance.dataHandler.DataChange(heroAwakenGeneSetAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gene);
		}
	}

	public void HeroAwaken_GeneRemove_Req(ESkillType SkillType, int Index)
	{
		HeroAwakenGeneRemoveReq heroAwakenGeneRemoveReq = new HeroAwakenGeneRemoveReq();
		heroAwakenGeneRemoveReq.SkillType = SkillType;
		heroAwakenGeneRemoveReq.Index = Index;
		Instance.WsPackMsg((IMessage)(object)heroAwakenGeneRemoveReq, Message.HeroAwakenGeneRemoveReq);
	}

	public void HeroAwaken_GeneRemove_Ack(ByteString response)
	{
		HeroAwakenGeneRemoveAck heroAwakenGeneRemoveAck = HeroAwakenGeneRemoveAck.Parser.ParseFrom(response);
		if (heroAwakenGeneRemoveAck != null)
		{
			zbServerData.sGameData.HeroAwakenInfo = heroAwakenGeneRemoveAck.HeroAwakenInfo;
			zbSelf.Instance.dataHandler.DataChange(heroAwakenGeneRemoveAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gene);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HeroAwaken_GeneRemove_Ack);
		}
	}

	public void HeroAwaken_GeneCompose_Req(ESkillType SkillType, int Index, int[] GeneList)
	{
		HeroAwakenGeneComposeReq heroAwakenGeneComposeReq = new HeroAwakenGeneComposeReq();
		heroAwakenGeneComposeReq.SkillType = SkillType;
		heroAwakenGeneComposeReq.Index = Index;
		heroAwakenGeneComposeReq.GeneList.Clear();
		heroAwakenGeneComposeReq.GeneList.AddRange((IEnumerable<int>)GeneList);
		Instance.WsPackMsg((IMessage)(object)heroAwakenGeneComposeReq, Message.HeroAwakenGeneComposeReq);
	}

	public void HeroAwaken_GeneCompose_Ack(ByteString response)
	{
		HeroAwakenGeneComposeAck heroAwakenGeneComposeAck = HeroAwakenGeneComposeAck.Parser.ParseFrom(response);
		if (heroAwakenGeneComposeAck != null)
		{
			zbServerData.sGameData.HeroAwakenInfo = heroAwakenGeneComposeAck.HeroAwakenInfo;
			zbSelf.Instance.dataHandler.DataChange(heroAwakenGeneComposeAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DataChange_Gene);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HeroAwaken_GeneCompose_Ack, heroAwakenGeneComposeAck);
		}
	}

	public void HeroFollow_Upgrade_Req(ESkillType SkillType)
	{
		HeroFollowUpgradeReq heroFollowUpgradeReq = new HeroFollowUpgradeReq();
		heroFollowUpgradeReq.SkillType = SkillType;
		Instance.WsPackMsg((IMessage)(object)heroFollowUpgradeReq, Message.HeroFollowUpgradeReq);
	}

	public void HeroFollow_Upgrade_Ack(ByteString response)
	{
		HeroFollowUpgradeAck heroFollowUpgradeAck = HeroFollowUpgradeAck.Parser.ParseFrom(response);
		if (heroFollowUpgradeAck != null)
		{
			zbServerData.sGameData.HeroFollowInfo = heroFollowUpgradeAck.HeroFollowInfo;
			zbSelf.Instance.dataHandler.DataChange(heroFollowUpgradeAck.DataChange);
		}
	}

	public void HomelandV2_Data_Req()
	{
		HomelandV2DataReq req = new HomelandV2DataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.HomelandV2DataReq);
	}

	public void HomelandV2_Data_Ack(ByteString response)
	{
		HomelandV2DataAck homelandV2DataAck = HomelandV2DataAck.Parser.ParseFrom(response);
		if (homelandV2DataAck != null)
		{
			zbServerData.sGameData.DefenseInfo = homelandV2DataAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2DataAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2DataAck.HomelandInfo);
		}
	}

	public void HomelandV2_DataSync_Req()
	{
		HomelandV2DataSyncReq req = new HomelandV2DataSyncReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.HomelandV2DataSyncReq);
	}

	public void HomelandV2_DataSync_Ack(ByteString response)
	{
		HomelandV2DataSyncAck homelandV2DataSyncAck = HomelandV2DataSyncAck.Parser.ParseFrom(response);
		if (homelandV2DataSyncAck != null)
		{
			zbSelf.Instance.worldHandler.worldResIdleSettleSecond = homelandV2DataSyncAck.ResIdleInfo.SettleSecond;
		}
	}

	public void HomelandV2_FloorUnlock_Req(int Floor)
	{
		HomelandV2FloorUnlockReq homelandV2FloorUnlockReq = new HomelandV2FloorUnlockReq();
		homelandV2FloorUnlockReq.Floor = Floor;
		Instance.WsPackMsg((IMessage)(object)homelandV2FloorUnlockReq, Message.HomelandV2FloorUnlockReq);
	}

	public void HomelandV2_FloorUnlock_Ack(ByteString response)
	{
		HomelandV2FloorUnlockAck homelandV2FloorUnlockAck = HomelandV2FloorUnlockAck.Parser.ParseFrom(response);
		if (homelandV2FloorUnlockAck != null)
		{
			zbServerData.sGameData.DefenseInfo = homelandV2FloorUnlockAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2FloorUnlockAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2FloorUnlockAck.HomelandInfo);
		}
	}

	public void HomelandV2_RoomCreate_Req(int TypeId, int x, int y)
	{
		HomelandV2RoomCreateReq homelandV2RoomCreateReq = new HomelandV2RoomCreateReq();
		homelandV2RoomCreateReq.TypeId = TypeId;
		homelandV2RoomCreateReq.X = x;
		homelandV2RoomCreateReq.Y = y;
		Instance.WsPackMsg((IMessage)(object)homelandV2RoomCreateReq, Message.HomelandV2RoomCreateReq);
	}

	public void HomelandV2_RoomCreate_Ack(ByteString response)
	{
		HomelandV2RoomCreateAck homelandV2RoomCreateAck = HomelandV2RoomCreateAck.Parser.ParseFrom(response);
		if (homelandV2RoomCreateAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(homelandV2RoomCreateAck.DataChange);
			zbServerData.sGameData.DefenseInfo = homelandV2RoomCreateAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2RoomCreateAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomCreateAck.HomelandInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HomelandRoomCreate_Ack);
		}
	}

	public void HomelandV2_RoomRepair_Req(int RoomId)
	{
		HomelandV2RoomRepairReq homelandV2RoomRepairReq = new HomelandV2RoomRepairReq();
		homelandV2RoomRepairReq.RoomId = RoomId;
		Instance.WsPackMsg((IMessage)(object)homelandV2RoomRepairReq, Message.HomelandV2RoomRepairReq);
	}

	public void HomelandV2_RoomRepair_Ack(ByteString response)
	{
		HomelandV2RoomRepairAck homelandV2RoomRepairAck = HomelandV2RoomRepairAck.Parser.ParseFrom(response);
		if (homelandV2RoomRepairAck != null)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
			zbSelf.Instance.dataHandler.DataChange(homelandV2RoomRepairAck.DataChange);
			zbServerData.sGameData.DefenseInfo = homelandV2RoomRepairAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2RoomRepairAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomRepairAck.HomelandInfo);
		}
	}

	public void HomelandV2_RoomUpgrade_Req(int RoomId)
	{
		HomelandV2RoomUpgradeReq homelandV2RoomUpgradeReq = new HomelandV2RoomUpgradeReq();
		homelandV2RoomUpgradeReq.RoomId = RoomId;
		Instance.WsPackMsg((IMessage)(object)homelandV2RoomUpgradeReq, Message.HomelandV2RoomUpgradeReq);
	}

	public void HomelandV2_RoomUpgrade_Ack(ByteString response)
	{
		HomelandV2RoomUpgradeAck homelandV2RoomUpgradeAck = HomelandV2RoomUpgradeAck.Parser.ParseFrom(response);
		if (homelandV2RoomUpgradeAck != null)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
			zbSelf.Instance.dataHandler.DataChange(homelandV2RoomUpgradeAck.DataChange);
			zbServerData.sGameData.DefenseInfo = homelandV2RoomUpgradeAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2RoomUpgradeAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomUpgradeAck.HomelandInfo);
			zbUIMgr.Instance.ShowCommonLevelUp();
		}
	}

	public void HomelandV2_RoomUpgradeFast_Req()
	{
		HomelandV2RoomUpgradeFastReq req = new HomelandV2RoomUpgradeFastReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.HomelandV2RoomUpgradeFastReq);
	}

	public void HomelandV2_RoomUpgradeFast_Ack(ByteString response)
	{
		HomelandV2RoomUpgradeFastAck homelandV2RoomUpgradeFastAck = HomelandV2RoomUpgradeFastAck.Parser.ParseFrom(response);
		if (homelandV2RoomUpgradeFastAck != null)
		{
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
			zbSelf.Instance.dataHandler.DataChange(homelandV2RoomUpgradeFastAck.DataChange);
			zbServerData.sGameData.DefenseInfo = homelandV2RoomUpgradeFastAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2RoomUpgradeFastAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomUpgradeFastAck.HomelandInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_FastUpdate_Room);
			zbUIMgr.Instance.ShowCommonLevelUp();
		}
	}

	public void HomelandV2_RoomDestroy_Req(int roomId)
	{
		HomelandV2RoomDestroyReq homelandV2RoomDestroyReq = new HomelandV2RoomDestroyReq();
		homelandV2RoomDestroyReq.RoomId = roomId;
		zbSelf.Instance.homelandHandler.DestroyRoomId = roomId;
		Instance.WsPackMsg((IMessage)(object)homelandV2RoomDestroyReq, Message.HomelandV2RoomDestroyReq);
	}

	public void HomelandV2_RoomDestroy_Ack(ByteString response)
	{
		HomelandV2RoomDestroyAck homelandV2RoomDestroyAck = HomelandV2RoomDestroyAck.Parser.ParseFrom(response);
		if (homelandV2RoomDestroyAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(homelandV2RoomDestroyAck.DataChange, ((IEnumerable<AwardDetail>)homelandV2RoomDestroyAck.AwardList).ToArray());
			zbServerData.sGameData.DefenseInfo = homelandV2RoomDestroyAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2RoomDestroyAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomDestroyAck.HomelandInfo);
		}
	}

	public void HomelandV2_RoomMove_Req(int roomId, int x, int y)
	{
		HomelandV2RoomMoveReq homelandV2RoomMoveReq = new HomelandV2RoomMoveReq();
		homelandV2RoomMoveReq.RoomId = roomId;
		homelandV2RoomMoveReq.X = x;
		homelandV2RoomMoveReq.Y = y;
		Instance.WsPackMsg((IMessage)(object)homelandV2RoomMoveReq, Message.HomelandV2RoomMoveReq);
	}

	public void HomelandV2_RoomMove_Ack(ByteString response)
	{
		HomelandV2RoomMoveAck homelandV2RoomMoveAck = HomelandV2RoomMoveAck.Parser.ParseFrom(response);
		if (homelandV2RoomMoveAck != null)
		{
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomMoveAck.HomelandInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HomelandRoomEdit_Ack);
		}
	}

	public void HomelandV2_RoomCompose_Req(int[] ComposeList, int X, int Y)
	{
		HomelandV2RoomComposeReq homelandV2RoomComposeReq = new HomelandV2RoomComposeReq();
		homelandV2RoomComposeReq.ComposeList.Clear();
		homelandV2RoomComposeReq.ComposeList.AddRange((IEnumerable<int>)ComposeList);
		homelandV2RoomComposeReq.X = X;
		homelandV2RoomComposeReq.Y = Y;
		Instance.WsPackMsg((IMessage)(object)homelandV2RoomComposeReq, Message.HomelandV2RoomComposeReq);
	}

	public void HomelandV2_RoomCompose_Ack(ByteString response)
	{
		HomelandV2RoomComposeAck homelandV2RoomComposeAck = HomelandV2RoomComposeAck.Parser.ParseFrom(response);
		if (homelandV2RoomComposeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(homelandV2RoomComposeAck.DataChange, ((IEnumerable<AwardDetail>)homelandV2RoomComposeAck.AwardList).ToArray());
			zbServerData.sGameData.DefenseInfo = homelandV2RoomComposeAck.DefenseInfo;
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(homelandV2RoomComposeAck.WarRoomInfo);
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomComposeAck.HomelandInfo);
		}
	}

	public void HomelandV2_RoomEdit_Req(HomelandRoomEditDetail[] RoomList)
	{
		HomelandV2RoomEditReq homelandV2RoomEditReq = new HomelandV2RoomEditReq();
		homelandV2RoomEditReq.RoomList.Clear();
		homelandV2RoomEditReq.RoomList.AddRange((IEnumerable<HomelandRoomEditDetail>)RoomList);
		Instance.WsPackMsg((IMessage)(object)homelandV2RoomEditReq, Message.HomelandV2RoomEditReq);
	}

	public void HomelandV2_RoomEdit_Ack(ByteString response)
	{
		HomelandV2RoomEditAck homelandV2RoomEditAck = HomelandV2RoomEditAck.Parser.ParseFrom(response);
		if (homelandV2RoomEditAck != null)
		{
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2RoomEditAck.HomelandInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HomelandRoomEdit_Ack);
		}
	}

	public void WarRoom_HeroSet_Req(ESkillType Skilltype, int HeroTid)
	{
		zbSelf.Instance.heroHandler.heroSetTid = HeroTid;
		WarRoomHeroSetReq warRoomHeroSetReq = new WarRoomHeroSetReq();
		warRoomHeroSetReq.SkillType = Skilltype;
		warRoomHeroSetReq.HeroTid = HeroTid;
		Instance.WsPackMsg((IMessage)(object)warRoomHeroSetReq, Message.WarRoomHeroSetReq);
	}

	public void WarRoom_HeroSet_Ack(ByteString response)
	{
		WarRoomHeroSetAck warRoomHeroSetAck = WarRoomHeroSetAck.Parser.ParseFrom(response);
		if (warRoomHeroSetAck != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10477"));
			zbSelf.Instance.homelandHandler.UpdateWarRoomInfo(warRoomHeroSetAck.WarRoomInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_HomelandInfo);
		}
	}

	public void HomelandV2_ReplaceWorker_Req(int workerId)
	{
		HomelandV2ReplaceWorkerReq homelandV2ReplaceWorkerReq = new HomelandV2ReplaceWorkerReq();
		homelandV2ReplaceWorkerReq.WorkerId = workerId;
		Instance.WsPackMsg((IMessage)(object)homelandV2ReplaceWorkerReq, Message.HomelandV2ReplaceWorkerReq);
	}

	public void HomelandV2_ReplaceWorker_Ack(ByteString response)
	{
		HomelandV2ReplaceWorkerAck homelandV2ReplaceWorkerAck = HomelandV2ReplaceWorkerAck.Parser.ParseFrom(response);
		if (homelandV2ReplaceWorkerAck != null)
		{
			zbSelf.Instance.homelandHandler.replaceList.Clear();
			zbSelf.Instance.homelandHandler.replaceList.AddRange((IEnumerable<WorkerDetail>)homelandV2ReplaceWorkerAck.ReplaceList);
			zbSelf.Instance.dataHandler.DataChange(homelandV2ReplaceWorkerAck.DataChange);
			if (!zbUIMgr.Instance.IsShow(EWndId.RoomWorkerRefresh))
			{
				zbUIMgr.Instance.ShowRoomWorkerRefresh(homelandV2ReplaceWorkerAck.WorkerId);
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorkerReplace_Refresh);
			}
		}
	}

	public void HomelandV2_ReplaceWorkerSelect_Req(int workerId, int selectId)
	{
		HomelandV2ReplaceWorkerSelectReq homelandV2ReplaceWorkerSelectReq = new HomelandV2ReplaceWorkerSelectReq();
		homelandV2ReplaceWorkerSelectReq.WorkerId = workerId;
		homelandV2ReplaceWorkerSelectReq.SelectId = selectId;
		Instance.WsPackMsg((IMessage)(object)homelandV2ReplaceWorkerSelectReq, Message.HomelandV2ReplaceWorkerSelectReq);
	}

	public void HomelandV2_ReplaceWorkerSelect_Ack(ByteString response)
	{
		HomelandV2ReplaceWorkerSelectAck homelandV2ReplaceWorkerSelectAck = HomelandV2ReplaceWorkerSelectAck.Parser.ParseFrom(response);
		if (homelandV2ReplaceWorkerSelectAck != null)
		{
			zbSelf.Instance.homelandHandler.UpdateHomelandInfo(homelandV2ReplaceWorkerSelectAck.HomelandInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_ReplaceWorkerSelect_Ack);
		}
	}

	public void Idle_Data_Req(bool open = false)
	{
		m_openIdleWnd = open;
		IdleDataReq req = new IdleDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.IdleDataReq);
	}

	public void Idle_Data_Ack(ByteString response)
	{
		IdleDataAck idleDataAck = IdleDataAck.Parser.ParseFrom(response);
		if (idleDataAck != null)
		{
			zbServerData.sGameData.IdleInfo = idleDataAck.IdleInfo;
			if (m_openIdleWnd)
			{
				zbUIMgr.Instance.ShowWallIdle();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_IdleInfo);
			}
		}
	}

	public void Idle_Gain_Req()
	{
		IdleGainReq req = new IdleGainReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.IdleGainReq);
	}

	public void Idle_Gain_Ack(ByteString response)
	{
		IdleGainAck idleGainAck = IdleGainAck.Parser.ParseFrom(response);
		if (idleGainAck != null)
		{
			zbServerData.sGameData.IdleInfo = idleGainAck.IdleInfo;
			zbSelf.Instance.dataHandler.DataChange(idleGainAck.DataChange, ((IEnumerable<AwardDetail>)idleGainAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_IdleInfo);
		}
	}

	public void Idle_FastGain_Req(int count = 1)
	{
		IdleFastGainReq idleFastGainReq = new IdleFastGainReq();
		idleFastGainReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)idleFastGainReq, Message.IdleFastGainReq);
	}

	public void Idle_FastGain_Ack(ByteString response)
	{
		IdleFastGainAck idleFastGainAck = IdleFastGainAck.Parser.ParseFrom(response);
		if (idleFastGainAck != null)
		{
			zbServerData.sGameData.IdleInfo = idleFastGainAck.IdleInfo;
			zbSelf.Instance.dataHandler.DataChange(idleFastGainAck.DataChange, ((IEnumerable<AwardDetail>)idleFastGainAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_IdleInfo);
		}
	}

	public void Journey_Data_Req()
	{
		JourneyDataReq req = new JourneyDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.JourneyDataReq);
	}

	public void Journey_Data_Ack(ByteString response)
	{
		JourneyDataAck journeyDataAck = JourneyDataAck.Parser.ParseFrom(response);
		if (journeyDataAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyDataAck.JourneyInfo);
		}
	}

	public void Journey_ChapterOneBarDraw_Req(int count)
	{
		JourneyChapterOneBarDrawReq journeyChapterOneBarDrawReq = new JourneyChapterOneBarDrawReq();
		journeyChapterOneBarDrawReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)journeyChapterOneBarDrawReq, Message.JourneyChapterOneBarDrawReq);
	}

	public void Journey_ChapterOneBarDraw_Ack(ByteString response)
	{
		JourneyChapterOneBarDrawAck journeyChapterOneBarDrawAck = JourneyChapterOneBarDrawAck.Parser.ParseFrom(response);
		if (journeyChapterOneBarDrawAck != null)
		{
			zbServerData.sGameData.JourneyInfo = journeyChapterOneBarDrawAck.JourneyInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Journey_ChapterOneBarDraw_Ack, journeyChapterOneBarDrawAck);
		}
	}

	public void Journey_ChapterOneBarTarget_Req()
	{
		JourneyChapterOneBarTargetReq req = new JourneyChapterOneBarTargetReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.JourneyChapterOneBarTargetReq);
	}

	public void Journey_ChapterOneBarTarget_Ack(ByteString response)
	{
		JourneyChapterOneBarTargetAck journeyChapterOneBarTargetAck = JourneyChapterOneBarTargetAck.Parser.ParseFrom(response);
		if (journeyChapterOneBarTargetAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterOneBarTargetAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterOneBarTargetAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterOneBarTargetAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterOneSign_Req(int day)
	{
		JourneyChapterOneSignReq journeyChapterOneSignReq = new JourneyChapterOneSignReq();
		journeyChapterOneSignReq.Day = day;
		Instance.WsPackMsg((IMessage)(object)journeyChapterOneSignReq, Message.JourneyChapterOneSignReq);
	}

	public void Journey_ChapterOneSign_Ack(ByteString response)
	{
		JourneyChapterOneSignAck journeyChapterOneSignAck = JourneyChapterOneSignAck.Parser.ParseFrom(response);
		if (journeyChapterOneSignAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(journeyChapterOneSignAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterOneSignAck.AwardList).ToArray());
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterOneSignAck.JourneyInfo);
		}
	}

	public void Journey_ChapterOneRechargeSum_Req(int index)
	{
		JourneyChapterOneRechargeSumReq journeyChapterOneRechargeSumReq = new JourneyChapterOneRechargeSumReq();
		journeyChapterOneRechargeSumReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)journeyChapterOneRechargeSumReq, Message.JourneyChapterOneRechargeSumReq);
	}

	public void Journey_ChapterOneRechargeSum_Ack(ByteString response)
	{
		JourneyChapterOneRechargeSumAck journeyChapterOneRechargeSumAck = JourneyChapterOneRechargeSumAck.Parser.ParseFrom(response);
		if (journeyChapterOneRechargeSumAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(journeyChapterOneRechargeSumAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterOneRechargeSumAck.AwardList).ToArray());
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterOneRechargeSumAck.JourneyInfo);
		}
	}

	public void Journey_ChapterOneExchange_Req(int id, int count)
	{
		JourneyChapterOneExchangeReq journeyChapterOneExchangeReq = new JourneyChapterOneExchangeReq();
		journeyChapterOneExchangeReq.Id = id;
		journeyChapterOneExchangeReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)journeyChapterOneExchangeReq, Message.JourneyChapterOneExchangeReq);
	}

	public void Journey_ChapterOneExchange_Ack(ByteString response)
	{
		JourneyChapterOneExchangeAck journeyChapterOneExchangeAck = JourneyChapterOneExchangeAck.Parser.ParseFrom(response);
		if (journeyChapterOneExchangeAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterOneExchangeAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterOneExchangeAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterOneExchangeAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterOneZhanLingAward_Req(int level)
	{
		JourneyChapterOneZhanLingAwardReq journeyChapterOneZhanLingAwardReq = new JourneyChapterOneZhanLingAwardReq();
		journeyChapterOneZhanLingAwardReq.Level = level;
		Instance.WsPackMsg((IMessage)(object)journeyChapterOneZhanLingAwardReq, Message.JourneyChapterOneZhanLingAwardReq);
	}

	public void Journey_ChapterOneZhanLingAward_Ack(ByteString response)
	{
		JourneyChapterOneZhanLingAwardAck journeyChapterOneZhanLingAwardAck = JourneyChapterOneZhanLingAwardAck.Parser.ParseFrom(response);
		if (journeyChapterOneZhanLingAwardAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterOneZhanLingAwardAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterOneZhanLingAwardAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterOneZhanLingAwardAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterThreeBarDraw_Req(int count)
	{
		JourneyChapterThreeBarDrawReq journeyChapterThreeBarDrawReq = new JourneyChapterThreeBarDrawReq();
		journeyChapterThreeBarDrawReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)journeyChapterThreeBarDrawReq, Message.JourneyChapterThreeBarDrawReq);
	}

	public void Journey_ChapterThreeBarDraw_Ack(ByteString response)
	{
		JourneyChapterThreeBarDrawAck journeyChapterThreeBarDrawAck = JourneyChapterThreeBarDrawAck.Parser.ParseFrom(response);
		if (journeyChapterThreeBarDrawAck != null)
		{
			zbServerData.sGameData.JourneyInfo = journeyChapterThreeBarDrawAck.JourneyInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Journey_ChapterThreeBarDraw_Ack, journeyChapterThreeBarDrawAck);
		}
	}

	public void Journey_ChapterThreeBarTarget_Req()
	{
		JourneyChapterThreeBarTargetReq req = new JourneyChapterThreeBarTargetReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.JourneyChapterThreeBarTargetReq);
	}

	public void Journey_ChapterThreeBarTarget_Ack(ByteString response)
	{
		JourneyChapterThreeBarTargetAck journeyChapterThreeBarTargetAck = JourneyChapterThreeBarTargetAck.Parser.ParseFrom(response);
		if (journeyChapterThreeBarTargetAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterThreeBarTargetAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterThreeBarTargetAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterThreeBarTargetAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterThreeSign_Req(int day)
	{
		JourneyChapterThreeSignReq journeyChapterThreeSignReq = new JourneyChapterThreeSignReq();
		journeyChapterThreeSignReq.Day = day;
		Instance.WsPackMsg((IMessage)(object)journeyChapterThreeSignReq, Message.JourneyChapterThreeSignReq);
	}

	public void Journey_ChapterThreeSign_Ack(ByteString response)
	{
		JourneyChapterThreeSignAck journeyChapterThreeSignAck = JourneyChapterThreeSignAck.Parser.ParseFrom(response);
		if (journeyChapterThreeSignAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(journeyChapterThreeSignAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterThreeSignAck.AwardList).ToArray());
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterThreeSignAck.JourneyInfo);
		}
	}

	public void Journey_ChapterThreeExchange_Req(int id, int count)
	{
		JourneyChapterThreeExchangeReq journeyChapterThreeExchangeReq = new JourneyChapterThreeExchangeReq();
		journeyChapterThreeExchangeReq.Id = id;
		journeyChapterThreeExchangeReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)journeyChapterThreeExchangeReq, Message.JourneyChapterThreeExchangeReq);
	}

	public void Journey_ChapterThreeExchange_Ack(ByteString response)
	{
		JourneyChapterThreeExchangeAck journeyChapterThreeExchangeAck = JourneyChapterThreeExchangeAck.Parser.ParseFrom(response);
		if (journeyChapterThreeExchangeAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterThreeExchangeAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterThreeExchangeAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterThreeExchangeAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterThreeDrawCount_Req(int index)
	{
		JourneyChapterThreeDrawCountReq journeyChapterThreeDrawCountReq = new JourneyChapterThreeDrawCountReq();
		journeyChapterThreeDrawCountReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)journeyChapterThreeDrawCountReq, Message.JourneyChapterThreeDrawCountReq);
	}

	public void Journey_ChapterThreeDrawCount_Ack(ByteString response)
	{
		JourneyChapterThreeDrawCountAck journeyChapterThreeDrawCountAck = JourneyChapterThreeDrawCountAck.Parser.ParseFrom(response);
		if (journeyChapterThreeDrawCountAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(journeyChapterThreeDrawCountAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterThreeDrawCountAck.AwardList).ToArray());
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterThreeDrawCountAck.JourneyInfo);
		}
	}

	public void Journey_ChapterThreeZhanLingAward_Req(int level)
	{
		JourneyChapterThreeZhanLingAwardReq journeyChapterThreeZhanLingAwardReq = new JourneyChapterThreeZhanLingAwardReq();
		journeyChapterThreeZhanLingAwardReq.Level = level;
		Instance.WsPackMsg((IMessage)(object)journeyChapterThreeZhanLingAwardReq, Message.JourneyChapterThreeZhanLingAwardReq);
	}

	public void Journey_ChapterThreeZhanLingAward_Ack(ByteString response)
	{
		JourneyChapterThreeZhanLingAwardAck journeyChapterThreeZhanLingAwardAck = JourneyChapterThreeZhanLingAwardAck.Parser.ParseFrom(response);
		if (journeyChapterThreeZhanLingAwardAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterThreeZhanLingAwardAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterThreeZhanLingAwardAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterThreeZhanLingAwardAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterTwoBarDraw_Req(int count)
	{
		JourneyChapterTwoBarDrawReq journeyChapterTwoBarDrawReq = new JourneyChapterTwoBarDrawReq();
		journeyChapterTwoBarDrawReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)journeyChapterTwoBarDrawReq, Message.JourneyChapterTwoBarDrawReq);
	}

	public void Journey_ChapterTwoBarDraw_Ack(ByteString response)
	{
		JourneyChapterTwoBarDrawAck journeyChapterTwoBarDrawAck = JourneyChapterTwoBarDrawAck.Parser.ParseFrom(response);
		if (journeyChapterTwoBarDrawAck != null)
		{
			zbServerData.sGameData.JourneyInfo = journeyChapterTwoBarDrawAck.JourneyInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Journey_ChapterTwoBarDraw_Ack, journeyChapterTwoBarDrawAck);
		}
	}

	public void Journey_ChapterTwoBarTarget_Req()
	{
		JourneyChapterTwoBarTargetReq req = new JourneyChapterTwoBarTargetReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.JourneyChapterTwoBarTargetReq);
	}

	public void Journey_ChapterTwoBarTarget_Ack(ByteString response)
	{
		JourneyChapterTwoBarTargetAck journeyChapterTwoBarTargetAck = JourneyChapterTwoBarTargetAck.Parser.ParseFrom(response);
		if (journeyChapterTwoBarTargetAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterTwoBarTargetAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterTwoBarTargetAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterTwoBarTargetAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterTwoActive_Req(int active)
	{
		JourneyChapterTwoActiveReq journeyChapterTwoActiveReq = new JourneyChapterTwoActiveReq();
		journeyChapterTwoActiveReq.Active = active;
		Instance.WsPackMsg((IMessage)(object)journeyChapterTwoActiveReq, Message.JourneyChapterTwoActiveReq);
	}

	public void Journey_ChapterTwoActive_Ack(ByteString response)
	{
		JourneyChapterTwoActiveAck journeyChapterTwoActiveAck = JourneyChapterTwoActiveAck.Parser.ParseFrom(response);
		if (journeyChapterTwoActiveAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(journeyChapterTwoActiveAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterTwoActiveAck.AwardList).ToArray());
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterTwoActiveAck.JourneyInfo);
		}
	}

	public void Journey_ChapterTwoDiamondCost_Req(int index)
	{
		JourneyChapterTwoDiamondCostReq journeyChapterTwoDiamondCostReq = new JourneyChapterTwoDiamondCostReq();
		journeyChapterTwoDiamondCostReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)journeyChapterTwoDiamondCostReq, Message.JourneyChapterTwoDiamondCostReq);
	}

	public void Journey_ChapterTwoDiamondCost_Ack(ByteString response)
	{
		JourneyChapterTwoDiamondCostAck journeyChapterTwoDiamondCostAck = JourneyChapterTwoDiamondCostAck.Parser.ParseFrom(response);
		if (journeyChapterTwoDiamondCostAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(journeyChapterTwoDiamondCostAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterTwoDiamondCostAck.AwardList).ToArray());
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterTwoDiamondCostAck.JourneyInfo);
		}
	}

	public void Journey_ChapterTwoExchange_Req(int id, int count)
	{
		JourneyChapterTwoExchangeReq journeyChapterTwoExchangeReq = new JourneyChapterTwoExchangeReq();
		journeyChapterTwoExchangeReq.Id = id;
		journeyChapterTwoExchangeReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)journeyChapterTwoExchangeReq, Message.JourneyChapterTwoExchangeReq);
	}

	public void Journey_ChapterTwoExchange_Ack(ByteString response)
	{
		JourneyChapterTwoExchangeAck journeyChapterTwoExchangeAck = JourneyChapterTwoExchangeAck.Parser.ParseFrom(response);
		if (journeyChapterTwoExchangeAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterTwoExchangeAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterTwoExchangeAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterTwoExchangeAck.AwardList).ToArray());
		}
	}

	public void Journey_ChapterTwoZhanLingAward_Req(int level)
	{
		JourneyChapterTwoZhanLingAwardReq journeyChapterTwoZhanLingAwardReq = new JourneyChapterTwoZhanLingAwardReq();
		journeyChapterTwoZhanLingAwardReq.Level = level;
		Instance.WsPackMsg((IMessage)(object)journeyChapterTwoZhanLingAwardReq, Message.JourneyChapterTwoZhanLingAwardReq);
	}

	public void Journey_ChapterTwoZhanLingAward_Ack(ByteString response)
	{
		JourneyChapterTwoZhanLingAwardAck journeyChapterTwoZhanLingAwardAck = JourneyChapterTwoZhanLingAwardAck.Parser.ParseFrom(response);
		if (journeyChapterTwoZhanLingAwardAck != null)
		{
			zbSelf.Instance.journeyOneHandler.UpdateJourneyInfo(journeyChapterTwoZhanLingAwardAck.JourneyInfo);
			zbSelf.Instance.dataHandler.DataChange(journeyChapterTwoZhanLingAwardAck.DataChange, ((IEnumerable<AwardDetail>)journeyChapterTwoZhanLingAwardAck.AwardList).ToArray());
		}
	}

	public void Legendary_Data_Req(bool open = false)
	{
		LegendaryDataReq req = new LegendaryDataReq();
		openLegendary = open;
		Instance.WsPackMsg((IMessage)(object)req, Message.LegendaryDataReq);
	}

	public void Legendary_Data_Ack(ByteString response)
	{
		LegendaryDataAck legendaryDataAck = LegendaryDataAck.Parser.ParseFrom(response);
		if (legendaryDataAck != null)
		{
			zbServerData.sGameData.HelpInfo = legendaryDataAck.HelpInfo;
			zbSelf.Instance.legendaryHandler.UpdateLegendaryInfo(legendaryDataAck.LegendaryInfo);
			if (openLegendary)
			{
				zbUIMgr.Instance.ShowLegendary();
			}
		}
	}

	public void Legendary_FightStart_Req(long PlayerId)
	{
		LegendaryFightStartReq legendaryFightStartReq = new LegendaryFightStartReq();
		legendaryFightStartReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)legendaryFightStartReq, Message.LegendaryFightStartReq);
	}

	public void Legendary_FightStart_Ack(ByteString response)
	{
		LegendaryFightStartAck legendaryFightStartAck = LegendaryFightStartAck.Parser.ParseFrom(response);
		if (legendaryFightStartAck != null)
		{
			zbSelf.Instance.combatHandler.StartShowCombat(legendaryFightStartAck.BattleInfo);
		}
	}

	public void Legendary_FightResult_Req(int LevelId, long PlayerId, BattleResultInfo BattleResult)
	{
		LegendaryFightResultReq legendaryFightResultReq = new LegendaryFightResultReq();
		legendaryFightResultReq.LevelId = LevelId;
		legendaryFightResultReq.PlayerId = PlayerId;
		legendaryFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)legendaryFightResultReq, Message.LegendaryFightResultReq);
	}

	public void Legendary_FightResult_Ack(ByteString response)
	{
		LegendaryFightResultAck legendaryFightResultAck = LegendaryFightResultAck.Parser.ParseFrom(response);
		if (legendaryFightResultAck != null)
		{
			zbServerData.sGameData.HelpInfo = legendaryFightResultAck.HelpInfo;
			zbSelf.Instance.legendaryHandler.UpdateLegendaryInfo(legendaryFightResultAck.LegendaryInfo);
			zbCombatSystem.Instance.battleAwardList = legendaryFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(legendaryFightResultAck.DataChange);
			_ = (zbCombatBalanceWnd)zbCombatSystem.Instance.SetBanlanceData(legendaryFightResultAck.BattleSettle);
		}
	}

	public void RechargeActive_ZhanLingLegendary_Req(int PassLevel)
	{
		RechargeActiveZhanLingLegendaryReq rechargeActiveZhanLingLegendaryReq = new RechargeActiveZhanLingLegendaryReq();
		rechargeActiveZhanLingLegendaryReq.PassLevel = PassLevel;
		Instance.WsPackMsg((IMessage)(object)rechargeActiveZhanLingLegendaryReq, Message.RechargeActiveZhanLingLegendaryReq);
	}

	public void RechargeActive_ZhanLingLegendary_Ack(ByteString response)
	{
		RechargeActiveZhanLingLegendaryAck rechargeActiveZhanLingLegendaryAck = RechargeActiveZhanLingLegendaryAck.Parser.ParseFrom(response);
		if (rechargeActiveZhanLingLegendaryAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveZhanLingLegendaryAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeActiveZhanLingLegendaryAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveZhanLingLegendaryAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegendaryInfo);
		}
	}

	public void Legion_Recommend_Req()
	{
		LegionRecommendReq req = new LegionRecommendReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionRecommendReq);
	}

	public void Legion_Recommend_Ack(ByteString response)
	{
		LegionRecommendAck legionRecommendAck = LegionRecommendAck.Parser.ParseFrom(response);
		if (legionRecommendAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionRecommend(legionRecommendAck.RecommendList);
		}
	}

	public void Legion_Search_Req(string content)
	{
		LegionSearchReq legionSearchReq = new LegionSearchReq();
		legionSearchReq.Content = content;
		Instance.WsPackMsg((IMessage)(object)legionSearchReq, Message.LegionSearchReq);
	}

	public void Legion_Search_Ack(ByteString response)
	{
		LegionSearchAck legionSearchAck = LegionSearchAck.Parser.ParseFrom(response);
		if (legionSearchAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionSearchList(legionSearchAck.SearchList);
		}
	}

	public void Legion_TipsInfo_Req(int id)
	{
		LegionTipsInfoReq legionTipsInfoReq = new LegionTipsInfoReq();
		legionTipsInfoReq.LegionId = id;
		Instance.WsPackMsg((IMessage)(object)legionTipsInfoReq, Message.LegionTipsInfoReq);
	}

	public void Legion_TipsInfo_Ack(ByteString response)
	{
		LegionTipsInfoAck legionTipsInfoAck = LegionTipsInfoAck.Parser.ParseFrom(response);
		if (legionTipsInfoAck != null)
		{
			zbUIMgr.Instance.ShowLegionTip(legionTipsInfoAck.LegionTipsInfo);
		}
	}

	public void Legion_Create_Req(int Icon, string LegionName, int joinLv1, EBoolState Confirm)
	{
		LegionCreateReq legionCreateReq = new LegionCreateReq();
		legionCreateReq.Icon = Icon;
		legionCreateReq.LegionName = LegionName;
		legionCreateReq.JoinLvl = joinLv1;
		legionCreateReq.Confirm = Confirm;
		Instance.WsPackMsg((IMessage)(object)legionCreateReq, Message.LegionCreateReq);
	}

	public void Legion_Create_Ack(ByteString response)
	{
		LegionCreateAck legionCreateAck = LegionCreateAck.Parser.ParseFrom(response);
		if (legionCreateAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionCreateAck.LegionSelfInfo;
			zbSelf.Instance.dataHandler.DataChange(legionCreateAck.DataChange);
			zbSelf.Instance.legionHandler.UpdateLegionData(legionCreateAck.LegionInfo);
			zbUIMgr.Instance.HideWnd(EWndId.LegionCreate);
			zbUIMgr.Instance.HideWnd(EWndId.LegionRecommendList);
			zbLinkHelper.OpenLegion();
		}
	}

	public void Legion_Apply_Req(int id)
	{
		LegionApplyReq legionApplyReq = new LegionApplyReq();
		legionApplyReq.LegionId = id;
		Instance.WsPackMsg((IMessage)(object)legionApplyReq, Message.LegionApplyReq);
	}

	public void Legion_Apply_Ack(ByteString response)
	{
		if (LegionApplyAck.Parser.ParseFrom(response) != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10065"));
		}
	}

	public void Legion_JoinQuick_Req()
	{
		LegionJoinQuickReq req = new LegionJoinQuickReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionJoinQuickReq);
	}

	public void Legion_JoinQuick_Ack(ByteString response)
	{
		LegionJoinQuickAck legionJoinQuickAck = LegionJoinQuickAck.Parser.ParseFrom(response);
		if (legionJoinQuickAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionJoinQuickAck.LegionSelfInfo;
			if (zbServerData.sGameData.LegionSelfInfo.LegionId == 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10065"));
				return;
			}
			zbSelf.Instance.legionHandler.UpdateLegionData(legionJoinQuickAck.LegionInfo);
			zbUIMgr.Instance.HideWnd(EWndId.LegionRecommendList);
			zbLinkHelper.OpenLegion();
		}
	}

	public void Legion_ClearJoinCD_Req()
	{
		LegionClearJoinCDReq req = new LegionClearJoinCDReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionClearJoinCdReq);
	}

	public void Legion_ClearJoinCD_Ack(ByteString response)
	{
		LegionClearJoinCDAck legionClearJoinCDAck = LegionClearJoinCDAck.Parser.ParseFrom(response);
		if (legionClearJoinCDAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(legionClearJoinCDAck.DataChange);
			zbServerData.sGameData.LegionSelfInfo = legionClearJoinCDAck.LegionSelfInfo;
			zbUIMgr.Instance.HideWnd(EWndId.LegionClearCd);
		}
	}

	public void Legion_Quit_Req()
	{
		LegionQuitReq req = new LegionQuitReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionQuitReq);
	}

	public void Legion_Quit_Ack(ByteString response)
	{
		LegionQuitAck legionQuitAck = LegionQuitAck.Parser.ParseFrom(response);
		if (legionQuitAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionQuitAck.LegionSelfInfo;
			zbSelf.Instance.legionHandler.UpdateLegionData(null);
			zbSelf.Instance.legionHandler.memberList.Clear();
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10066"));
			zbUIMgr.Instance.HideWnd(EWndId.LegionLeave);
			zbUIMgr.Instance.HideWnd(EWndId.LegionOther);
			zbUIMgr.Instance.HideWnd(EWndId.Legion);
			zbUIMgr.Instance.ShowMainLevel();
		}
	}

	public void Legion_Data_Req(bool bOpenLegionWnd = false)
	{
		m_bOpenLegionWnd = bOpenLegionWnd;
		LegionDataReq req = new LegionDataReq();
		zbSelf.Instance.legionWarHandler.CanReq = false;
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionDataReq);
	}

	public void Legion_Data_Ack(ByteString response)
	{
		LegionDataAck legionDataAck = LegionDataAck.Parser.ParseFrom(response);
		if (legionDataAck == null)
		{
			return;
		}
		zbServerData.sGameData.LegionSelfInfo = legionDataAck.LegionSelfInfo;
		zbSelf.Instance.legionHandler.UpdateLegionData(legionDataAck.LegionInfo);
		zbSelf.Instance.legionWarHandler.CanReq = true;
		if (!m_bOpenLegionWnd && !zbUIMgr.Instance.IsShow(EWndId.LegionRecommendList))
		{
			return;
		}
		if (zbServerData.sGameData.LegionSelfInfo.LegionId > 0)
		{
			if (zbUIMgr.Instance.IsShow(EWndId.LegionRecommendList))
			{
				zbUIMgr.Instance.HideWnd(EWndId.LegionRecommendList);
				zbUIMgr.Instance.HideWnd(EWndId.LegionTip);
			}
			zbUIMgr.Instance.ShowLegion();
		}
		m_bOpenLegionWnd = false;
	}

	public void Legion_Members_Req(bool bOpenLegionMembersWnd = false)
	{
		m_bOpenLegionMembersWnd = bOpenLegionMembersWnd;
		LegionMembersReq req = new LegionMembersReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionMembersReq);
	}

	public void Legion_Members_Ack(ByteString response)
	{
		LegionMembersAck legionMembersAck = LegionMembersAck.Parser.ParseFrom(response);
		if (legionMembersAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionMembers(legionMembersAck.MemberList);
			if (!zbUIMgr.Instance.IsShow(EWndId.LegionDetail) && m_bOpenLegionMembersWnd)
			{
				zbUIMgr.Instance.ShowLegionMembers();
			}
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Members);
			m_bOpenLegionMembersWnd = false;
		}
	}

	public void Legion_ApplyList_Req()
	{
		LegionApplyListReq req = new LegionApplyListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionApplyListReq);
	}

	public void Legion_ApplyList_Ack(ByteString response)
	{
		LegionApplyListAck legionApplyListAck = LegionApplyListAck.Parser.ParseFrom(response);
		if (legionApplyListAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionApplyList(legionApplyListAck.ApplyList);
		}
	}

	public void Legion_ApplyPass_Req(int playerId)
	{
		LegionApplyPassReq legionApplyPassReq = new LegionApplyPassReq();
		legionApplyPassReq.PlayerId = playerId;
		Instance.WsPackMsg((IMessage)(object)legionApplyPassReq, Message.LegionApplyPassReq);
	}

	public void Legion_ApplyPass_Ack(ByteString response)
	{
		LegionApplyPassAck legionApplyPassAck = LegionApplyPassAck.Parser.ParseFrom(response);
		if (legionApplyPassAck != null)
		{
			zbSelf.Instance.legionHandler.RemoveLegionApply(legionApplyPassAck.PlayerId);
			zbSelf.Instance.legionHandler.UpdateLegionBaseInfo(legionApplyPassAck.LegionBaseInfo);
		}
	}

	public void Legion_ApplyPassAll_Req()
	{
		LegionApplyPassAllReq req = new LegionApplyPassAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionApplyPassAllReq);
	}

	public void Legion_ApplyPassAll_Ack(ByteString response)
	{
		LegionApplyPassAllAck legionApplyPassAllAck = LegionApplyPassAllAck.Parser.ParseFrom(response);
		if (legionApplyPassAllAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionApplyList(legionApplyPassAllAck.ApplyList);
			zbSelf.Instance.legionHandler.UpdateLegionBaseInfo(legionApplyPassAllAck.LegionBaseInfo);
		}
	}

	public void Legion_ApplyRefuse_Req(int playerId)
	{
		LegionApplyRefuseReq legionApplyRefuseReq = new LegionApplyRefuseReq();
		legionApplyRefuseReq.PlayerId = playerId;
		Instance.WsPackMsg((IMessage)(object)legionApplyRefuseReq, Message.LegionApplyRefuseReq);
	}

	public void Legion_ApplyRefuse_Ack(ByteString response)
	{
		LegionApplyRefuseAck legionApplyRefuseAck = LegionApplyRefuseAck.Parser.ParseFrom(response);
		if (legionApplyRefuseAck != null)
		{
			zbSelf.Instance.legionHandler.RemoveLegionApply(legionApplyRefuseAck.PlayerId);
		}
	}

	public void Legion_ApplyRefuseAll_Req()
	{
		LegionApplyRefuseAllReq req = new LegionApplyRefuseAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionApplyRefuseAllReq);
	}

	public void Legion_ApplyRefuseAll_Ack(ByteString response)
	{
		LegionApplyRefuseAllAck legionApplyRefuseAllAck = LegionApplyRefuseAllAck.Parser.ParseFrom(response);
		if (legionApplyRefuseAllAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionApplyList(legionApplyRefuseAllAck.ApplyList);
		}
	}

	public void Legion_Kick_Req(int playerId)
	{
		LegionKickReq legionKickReq = new LegionKickReq();
		legionKickReq.PlayerId = playerId;
		Instance.WsPackMsg((IMessage)(object)legionKickReq, Message.LegionKickReq);
	}

	public void Legion_Kick_Ack(ByteString response)
	{
		LegionKickAck legionKickAck = LegionKickAck.Parser.ParseFrom(response);
		if (legionKickAck != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10067"));
			zbUIMgr.Instance.HideWnd(EWndId.LegionPosition);
			zbSelf.Instance.legionHandler.UpdateLegionData(legionKickAck.LegionInfo);
			zbSelf.Instance.legionHandler.RemoveLegionMember(legionKickAck.PlayerId);
		}
	}

	public void Legion_NameChange_Req(string legionName)
	{
		LegionNameChangeReq legionNameChangeReq = new LegionNameChangeReq();
		legionNameChangeReq.LegionName = legionName;
		Instance.WsPackMsg((IMessage)(object)legionNameChangeReq, Message.LegionNameChangeReq);
	}

	public void Legion_NameChange_Ack(ByteString response)
	{
		LegionNameChangeAck legionNameChangeAck = LegionNameChangeAck.Parser.ParseFrom(response);
		zbSelf.Instance.dataHandler.DataChange(legionNameChangeAck.DataChange);
		zbSelf.Instance.legionHandler.UpdateLegionBaseInfo(legionNameChangeAck.LegionBaseInfo);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_ChangeInfo);
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11229"));
	}

	public void Legion_IconChange_Req(int icon)
	{
		LegionIconChangeReq legionIconChangeReq = new LegionIconChangeReq();
		legionIconChangeReq.Icon = icon;
		Instance.WsPackMsg((IMessage)(object)legionIconChangeReq, Message.LegionIconChangeReq);
	}

	public void Legion_IconChange_Ack(ByteString response)
	{
		LegionIconChangeAck legionIconChangeAck = LegionIconChangeAck.Parser.ParseFrom(response);
		zbSelf.Instance.dataHandler.DataChange(legionIconChangeAck.DataChange);
		zbSelf.Instance.legionHandler.UpdateLegionBaseInfo(legionIconChangeAck.LegionBaseInfo);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_ChangeInfo);
		zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11230"));
	}

	public void Legion_SignChange_Req(string legionSign)
	{
		LegionSignChangeReq legionSignChangeReq = new LegionSignChangeReq();
		legionSignChangeReq.LegionSign = legionSign;
		Instance.WsPackMsg((IMessage)(object)legionSignChangeReq, Message.LegionSignChangeReq);
	}

	public void Legion_SignChange_Ack(ByteString response)
	{
		LegionSignChangeAck legionSignChangeAck = LegionSignChangeAck.Parser.ParseFrom(response);
		if (legionSignChangeAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionData(legionSignChangeAck.LegionInfo);
		}
	}

	public void Legion_JoinSet_Req(int joinLv1, EBoolState confirm)
	{
		LegionJoinSetReq legionJoinSetReq = new LegionJoinSetReq();
		legionJoinSetReq.JoinLvl = joinLv1;
		legionJoinSetReq.Confirm = confirm;
		Instance.WsPackMsg((IMessage)(object)legionJoinSetReq, Message.LegionJoinSetReq);
	}

	public void Legion_JoinSet_Ack(ByteString response)
	{
		LegionJoinSetAck legionJoinSetAck = LegionJoinSetAck.Parser.ParseFrom(response);
		if (legionJoinSetAck != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10068"));
			zbSelf.Instance.legionHandler.UpdateLegionBaseInfo(legionJoinSetAck.LegionBaseInfo);
			zbUIMgr.Instance.HideWnd(EWndId.LegionLimited);
		}
	}

	public void Legion_OfficialSet_Req(int playerId, ELegionOfficial official)
	{
		LegionOfficialSetReq legionOfficialSetReq = new LegionOfficialSetReq();
		legionOfficialSetReq.PlayerId = playerId;
		legionOfficialSetReq.Official = official;
		Instance.WsPackMsg((IMessage)(object)legionOfficialSetReq, Message.LegionOfficialSetReq);
	}

	public void Legion_OfficialSet_Ack(ByteString response)
	{
		LegionOfficialSetAck legionOfficialSetAck = LegionOfficialSetAck.Parser.ParseFrom(response);
		if (legionOfficialSetAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionData(legionOfficialSetAck.LegionInfo);
			zbUIMgr.Instance.HideWnd(EWndId.LegionPosition);
		}
	}

	public void Legion_LogList_Req()
	{
		LegionLogListReq req = new LegionLogListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionLogListReq);
	}

	public void Legion_LogList_Ack(ByteString response)
	{
		LegionLogListAck legionLogListAck = LegionLogListAck.Parser.ParseFrom(response);
		if (legionLogListAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionLog(legionLogListAck.LogList);
		}
	}

	public void LegionAux_Donate_Req(EMoneyType type)
	{
		LegionAuxDonateReq legionAuxDonateReq = new LegionAuxDonateReq();
		legionAuxDonateReq.Type = type;
		Instance.WsPackMsg((IMessage)(object)legionAuxDonateReq, Message.LegionAuxDonateReq);
	}

	public void LegionAux_Donate_Ack(ByteString response)
	{
		LegionAuxDonateAck legionAuxDonateAck = LegionAuxDonateAck.Parser.ParseFrom(response);
		if (legionAuxDonateAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionAuxDonateAck.LegionSelfInfo;
			zbSelf.Instance.legionHandler.UpdateLegionData(legionAuxDonateAck.LegionInfo);
			zbSelf.Instance.legionHandler.UpdateAuxDonateAwardList(legionAuxDonateAck.AwardList);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10069"));
			zbSelf.Instance.dataHandler.DataChange(legionAuxDonateAck.DataChange, ((IEnumerable<AwardDetail>)legionAuxDonateAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Donate);
		}
	}

	public void LegionAux_DonateTarget_Req(int target)
	{
		LegionAuxDonateTargetReq legionAuxDonateTargetReq = new LegionAuxDonateTargetReq();
		legionAuxDonateTargetReq.Target = target;
		Instance.WsPackMsg((IMessage)(object)legionAuxDonateTargetReq, Message.LegionAuxDonateTargetReq);
	}

	public void LegionAux_DonateTarget_Ack(ByteString response)
	{
		LegionAuxDonateTargetAck legionAuxDonateTargetAck = LegionAuxDonateTargetAck.Parser.ParseFrom(response);
		if (legionAuxDonateTargetAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionAuxDonateTargetAck.LegionSelfInfo;
			zbSelf.Instance.dataHandler.DataChange(legionAuxDonateTargetAck.DataChange, ((IEnumerable<AwardDetail>)legionAuxDonateTargetAck.AwardList).ToArray());
			zbSelf.Instance.legionHandler.UpdateAuxDonateTargetAwardList(legionAuxDonateTargetAck.AwardList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Donate);
		}
	}

	public void LegionAux_Dinner_Req(int index, EMoneyType type)
	{
		LegionAuxDinnerReq legionAuxDinnerReq = new LegionAuxDinnerReq();
		legionAuxDinnerReq.Index = index;
		legionAuxDinnerReq.Type = type;
		Instance.WsPackMsg((IMessage)(object)legionAuxDinnerReq, Message.LegionAuxDinnerReq);
	}

	public void LegionAux_Dinner_Ack(ByteString response)
	{
		LegionAuxDinnerAck legionAuxDinnerAck = LegionAuxDinnerAck.Parser.ParseFrom(response);
		if (legionAuxDinnerAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionAuxDinnerAck.LegionSelfInfo;
			zbSelf.Instance.dataHandler.DataChange(legionAuxDinnerAck.DataChange, ((IEnumerable<AwardDetail>)legionAuxDinnerAck.AwardList).ToArray());
		}
	}

	public void LegionAux_Reward_Req(int playerId, int rewardId)
	{
		LegionAuxRewardReq legionAuxRewardReq = new LegionAuxRewardReq();
		legionAuxRewardReq.PlayerId = playerId;
		legionAuxRewardReq.RewardId = rewardId;
		Instance.WsPackMsg((IMessage)(object)legionAuxRewardReq, Message.LegionAuxRewardReq);
	}

	public void LegionAuxReward_Ack(ByteString response)
	{
		LegionAuxRewardAck legionAuxRewardAck = LegionAuxRewardAck.Parser.ParseFrom(response);
		if (legionAuxRewardAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionAuxRewardAck.LegionSelfInfo;
			zbSelf.Instance.legionHandler.UpdateLegionData(legionAuxRewardAck.LegionInfo);
			zbSelf.Instance.dataHandler.DataChange(legionAuxRewardAck.DataChange);
		}
	}

	public void LegionBoss_FightStart_Req(int bossId)
	{
		LegionBossFightStartReq legionBossFightStartReq = new LegionBossFightStartReq();
		legionBossFightStartReq.BossId = bossId;
		Instance.WsPackMsg((IMessage)(object)legionBossFightStartReq, Message.LegionBossFightStartReq);
	}

	public void LegionBoss_FightStart_Ack(ByteString response)
	{
		LegionBossFightStartAck legionBossFightStartAck = LegionBossFightStartAck.Parser.ParseFrom(response);
		if (legionBossFightStartAck != null)
		{
			zbSelf.Instance.combatHandler.StartShowCombat(legionBossFightStartAck.BattleInfo);
		}
	}

	public void LegionBoss_FightResult_Req(int bossId, BattleResultInfo battleResult)
	{
		LegionBossFightResultReq legionBossFightResultReq = new LegionBossFightResultReq();
		legionBossFightResultReq.BossId = bossId;
		legionBossFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)legionBossFightResultReq, Message.LegionBossFightResultReq);
	}

	public void LegionBoss_FightResult_Ack(ByteString response)
	{
		LegionBossFightResultAck legionBossFightResultAck = LegionBossFightResultAck.Parser.ParseFrom(response);
		if (legionBossFightResultAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(legionBossFightResultAck.DataChange);
			zbServerData.sGameData.LegionSelfInfo = legionBossFightResultAck.LegionSelfInfo;
			zbSelf.Instance.legionHandler.UpdateLegionBaseInfo(legionBossFightResultAck.LegionBaseInfo);
			zbSelf.Instance.legionHandler.UpdateLegionBossInfo(legionBossFightResultAck.LegionBossInfo);
			zbCombatSystem.Instance.battleAwardList = legionBossFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(legionBossFightResultAck.BattleSettle);
		}
	}

	public void LegionBoss_SaoDang_Req(int bossId)
	{
		LegionBossSaoDangReq legionBossSaoDangReq = new LegionBossSaoDangReq();
		legionBossSaoDangReq.BossId = bossId;
		Instance.WsPackMsg((IMessage)(object)legionBossSaoDangReq, Message.LegionBossSaoDangReq);
	}

	public void LegionBoss_SaoDang_Ack(ByteString response)
	{
		LegionBossSaoDangAck legionBossSaoDangAck = LegionBossSaoDangAck.Parser.ParseFrom(response);
		if (legionBossSaoDangAck != null)
		{
			zbSelf.Instance.legionHandler.UpdateLegionBaseInfo(legionBossSaoDangAck.LegionBaseInfo);
			zbSelf.Instance.legionHandler.UpdateLegionBossInfo(legionBossSaoDangAck.LegionBossInfo);
			zbServerData.sGameData.LegionSelfInfo = legionBossSaoDangAck.LegionSelfInfo;
			zbSelf.Instance.dataHandler.DataChange(legionBossSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)legionBossSaoDangAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_BossAward);
		}
	}

	public void LegionBoss_PassAward_Req(int bossId)
	{
		LegionBossPassAwardReq legionBossPassAwardReq = new LegionBossPassAwardReq();
		legionBossPassAwardReq.BossId = bossId;
		Instance.WsPackMsg((IMessage)(object)legionBossPassAwardReq, Message.LegionBossPassAwardReq);
	}

	public void LegionBoss_PassAward_Ack(ByteString response)
	{
		LegionBossPassAwardAck legionBossPassAwardAck = LegionBossPassAwardAck.Parser.ParseFrom(response);
		if (legionBossPassAwardAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionBossPassAwardAck.LegionSelfInfo;
			zbSelf.Instance.dataHandler.DataChange(legionBossPassAwardAck.DataChange, ((IEnumerable<AwardDetail>)legionBossPassAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_BossAward);
		}
	}

	public void LegionBoss_BuyCombat_Req(int count)
	{
		LegionBossBuyCombatReq legionBossBuyCombatReq = new LegionBossBuyCombatReq();
		legionBossBuyCombatReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)legionBossBuyCombatReq, Message.LegionBossBuyCombatReq);
	}

	public void LegionBoss_BuyCombat_Ack(ByteString response)
	{
		LegionBossBuyCombatAck legionBossBuyCombatAck = LegionBossBuyCombatAck.Parser.ParseFrom(response);
		if (legionBossBuyCombatAck != null)
		{
			zbServerData.sGameData.LegionSelfInfo = legionBossBuyCombatAck.LegionSelfInfo;
			zbSelf.Instance.dataHandler.DataChange(legionBossBuyCombatAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_BossAward);
		}
	}

	public void LegionBoss_RankList_Req()
	{
		LegionBossRankListReq req = new LegionBossRankListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionBossRankListReq);
	}

	public void LegionBoss_RankList_Ack(ByteString response)
	{
		LegionBossRankListAck legionBossRankListAck = LegionBossRankListAck.Parser.ParseFrom(response);
		if (legionBossRankListAck != null)
		{
			zbUIMgr.Instance.ShowLegionBossRank(((IEnumerable<RankDetail>)legionBossRankListAck.RankList).ToArray());
		}
	}

	public void LegionBoss_RecordList_Req()
	{
		LegionBossRecordListReq req = new LegionBossRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionBossRecordListReq);
	}

	public void LegionBoss_RecordList_Ack(ByteString response)
	{
		LegionBossRecordListAck legionBossRecordListAck = LegionBossRecordListAck.Parser.ParseFrom(response);
		if (legionBossRecordListAck != null)
		{
			zbUIMgr.Instance.ShowLegionBossRecord(((IEnumerable<LegionBossKillRecordDetail>)legionBossRecordListAck.RecordList).ToArray());
		}
	}

	public void LegionWar_Data_Req()
	{
		LegionWarDataReq req = new LegionWarDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarDataReq);
	}

	public void LegionWar_Data_Ack(ByteString response)
	{
		LegionWarDataAck legionWarDataAck = LegionWarDataAck.Parser.ParseFrom(response);
		if (legionWarDataAck == null)
		{
			return;
		}
		zbSelf.Instance.legionWarHandler.m_legionWarState = legionWarDataAck.WarState;
		zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarDataAck.LegionWarLegionInfo;
		zbSelf.Instance.legionWarHandler.UpdateLegionWarBaseInfo(legionWarDataAck.LegionWarBaseInfo);
		if (zbSelf.Instance.legionWarHandler.m_legionWarState >= ELegionWarState.Finished && zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId != ELegionWarForce.EmptyValue)
		{
			int num = zbGameSave.Instance.GetInt(zbSelf.Instance.legionWarHandler.saveSettleStr);
			if (zbServerData.sGameData.LegionWarBaseInfo.Season > num)
			{
				Instance.LegionWar_SettlePlayerInfo_Req(1, openWnd: true);
			}
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarData);
	}

	public void LegionWar_SignUp_Req()
	{
		LegionWarSignUpReq req = new LegionWarSignUpReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarSignUpReq);
	}

	public void LegionWar_SignUp_Ack(ByteString response)
	{
		LegionWarSignUpAck legionWarSignUpAck = LegionWarSignUpAck.Parser.ParseFrom(response);
		if (legionWarSignUpAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarSignUpAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.UpdateLegionWarBaseInfo(legionWarSignUpAck.LegionWarBaseInfo);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11187"));
			LegionWar_SignUpList_Req(0);
		}
	}

	public void LegionWar_SignUpList_Req(int page)
	{
		LegionWarSignUpListReq legionWarSignUpListReq = new LegionWarSignUpListReq();
		legionWarSignUpListReq.Page = page;
		Instance.WsPackMsg((IMessage)(object)legionWarSignUpListReq, Message.LegionWarSignUpListReq);
	}

	public void LegionWar_SignUpList_Ack(ByteString response)
	{
		LegionWarSignUpListAck legionWarSignUpListAck = LegionWarSignUpListAck.Parser.ParseFrom(response);
		if (legionWarSignUpListAck != null)
		{
			List<LegionBaseInfo> b = ((IEnumerable<LegionBaseInfo>)legionWarSignUpListAck.LegionList).ToList();
			zbSelf.Instance.legionWarHandler.MergeToA(zbSelf.Instance.legionWarHandler.LegionWarSignUpList, b);
			zbLegionWarApplyWnd obj = zbUIMgr.Instance.GetWndById(EWndId.LegionWarApply) as zbLegionWarApplyWnd;
			obj.UpdateView();
			obj.isLoading = false;
		}
	}

	public void LegionWar_EnterWar_Req(bool openLegionWar = false)
	{
		zbSelf.Instance.legionWarHandler.openLegionWar = openLegionWar;
		LegionWarEnterWarReq req = new LegionWarEnterWarReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarEnterWarReq);
	}

	public void LegionWar_EnterWar_Ack(ByteString response)
	{
		LegionWarEnterWarAck legionWarEnterWarAck = LegionWarEnterWarAck.Parser.ParseFrom(response);
		if (legionWarEnterWarAck != null)
		{
			zbSelf.Instance.legionWarHandler.UpdateLegionWarBaseInfo(legionWarEnterWarAck.LegionWarBaseInfo);
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarEnterWarAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarEnterWarAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarEnterWarAck.LegionWarMapInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo = legionWarEnterWarAck.LegionWarSelfTeamInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarEnter);
			if (zbSelf.Instance.legionWarHandler.openLegionWar)
			{
				zbUIMgr.Instance.ShowLegionWar();
			}
			zbSelf.Instance.legionWarHandler.openLegionWar = false;
		}
	}

	public void LegionWar_MapInfo_Req()
	{
		LegionWarMapInfoReq req = new LegionWarMapInfoReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarMapInfoReq);
	}

	public void LegionWar_MapInfo_Ack(ByteString response)
	{
		LegionWarMapInfoAck legionWarMapInfoAck = LegionWarMapInfoAck.Parser.ParseFrom(response);
		if (legionWarMapInfoAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarMapInfoAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarMapInfoAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarMapInfoAck.LegionWarMapInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMapInfo);
		}
	}

	public void LegionWar_MapInfo_Ntf(ByteString response)
	{
		LegionWarMapInfoNtf legionWarMapInfoNtf = LegionWarMapInfoNtf.Parser.ParseFrom(response);
		if (legionWarMapInfoNtf != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarMapInfoNtf.LegionWarMapInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMapInfo);
		}
	}

	public void LegionWar_MapMove_Ntf(ByteString response)
	{
		LegionWarMapMoveNtf legionWarMapMoveNtf = LegionWarMapMoveNtf.Parser.ParseFrom(response);
		if (legionWarMapMoveNtf != null)
		{
			zbSelf.Instance.legionWarHandler.HeroMoveList = ((IEnumerable<LegionWarMoveDetail>)legionWarMapMoveNtf.MoveList).ToList();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMove);
		}
	}

	public void LegionWar_PointInfo_Req(int pointId, Vector3? vector3 = null)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		zbSelf.Instance.legionWarHandler.tipVector3 = (Vector3)(((_003F?)vector3) ?? Vector3.zero);
		LegionWarPointInfoReq legionWarPointInfoReq = new LegionWarPointInfoReq();
		legionWarPointInfoReq.PointId = pointId;
		Instance.WsPackMsg((IMessage)(object)legionWarPointInfoReq, Message.LegionWarPointInfoReq);
	}

	public void LegionWar_PointInfo_Ack(ByteString response)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		LegionWarPointInfoAck legionWarPointInfoAck = LegionWarPointInfoAck.Parser.ParseFrom(response);
		if (legionWarPointInfoAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarPointStateDetail = legionWarPointInfoAck.PointState;
			zbLegionWarPointTipWnd zbLegionWarPointTipWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegionWarPointTip) as zbLegionWarPointTipWnd;
			if (zbUIMgr.Instance.IsShow(EWndId.LegionWarPointTip))
			{
				zbLegionWarPointTipWnd2.RefreshCustom(legionWarPointInfoAck.PointState, Vector2.op_Implicit(zbSelf.Instance.legionWarHandler.tipVector3));
			}
		}
	}

	public void LegionWar_EnterPoint_Req(int pointId, bool openLegionWarFight = false)
	{
		zbSelf.Instance.legionWarHandler.openLegionWarFight = openLegionWarFight;
		LegionWarEnterPointReq legionWarEnterPointReq = new LegionWarEnterPointReq();
		legionWarEnterPointReq.PointId = pointId;
		Instance.WsPackMsg((IMessage)(object)legionWarEnterPointReq, Message.LegionWarEnterPointReq);
	}

	public void LegionWar_EnterPoint_Ack(ByteString response)
	{
		LegionWarEnterPointAck legionWarEnterPointAck = LegionWarEnterPointAck.Parser.ParseFrom(response);
		if (legionWarEnterPointAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarEnterPointAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarEnterPointAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarPointInfo = legionWarEnterPointAck.LegionWarPointInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarPointInfo);
			if (zbSelf.Instance.legionWarHandler.openLegionWarFight)
			{
				zbSelf.Instance.legionWarHandler.openLegionWarFight = false;
				zbUIMgr.Instance.ShowLegionWarFight();
			}
		}
	}

	public void LegionWar_Rest_Req()
	{
		LegionWarRestReq req = new LegionWarRestReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarRestReq);
	}

	public void LegionWar_Rest_Ack(ByteString response)
	{
		LegionWarRestAck legionWarRestAck = LegionWarRestAck.Parser.ParseFrom(response);
		if (legionWarRestAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarRestAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarRestAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarRestAck.LegionWarMapInfo;
			zbServerData.sGameData.LegionWarBaseInfo = legionWarRestAck.LegionWarBaseInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarRest);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMapInfo);
		}
	}

	public void LegionWar_Goto_Req(int PointId)
	{
		LegionWarGotoReq legionWarGotoReq = new LegionWarGotoReq();
		legionWarGotoReq.PointId = PointId;
		Instance.WsPackMsg((IMessage)(object)legionWarGotoReq, Message.LegionWarGotoReq);
	}

	public void LegionWar_Goto_Ack(ByteString response)
	{
		LegionWarGotoAck legionWarGotoAck = LegionWarGotoAck.Parser.ParseFrom(response);
		if (legionWarGotoAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarGotoAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarGotoAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarGotoAck.LegionWarMapInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMapInfo);
		}
	}

	public void LegionWar_PointAward_Req(int PointId)
	{
		LegionWarPointAwardReq legionWarPointAwardReq = new LegionWarPointAwardReq();
		legionWarPointAwardReq.PointId = PointId;
		Instance.WsPackMsg((IMessage)(object)legionWarPointAwardReq, Message.LegionWarPointAwardReq);
	}

	public void LegionWar_PointAward_Ack(ByteString response)
	{
		LegionWarPointAwardAck legionWarPointAwardAck = LegionWarPointAwardAck.Parser.ParseFrom(response);
		if (legionWarPointAwardAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarPointAwardAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarPointAwardAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarPointAwardAck.LegionWarMapInfo;
			zbServerData.sGameData.LegionWarBaseInfo = legionWarPointAwardAck.LegionWarBaseInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMapInfo);
			zbSelf.Instance.dataHandler.DataChange(legionWarPointAwardAck.DataChange, ((IEnumerable<AwardDetail>)legionWarPointAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarPointAward);
		}
	}

	public void LegionWar_MarkPoint_Req(int PointId)
	{
		LegionWarMarkPointReq legionWarMarkPointReq = new LegionWarMarkPointReq();
		legionWarMarkPointReq.PointId = PointId;
		Instance.WsPackMsg((IMessage)(object)legionWarMarkPointReq, Message.LegionWarMarkPointReq);
	}

	public void LegionWar_MarkPoint_Ack(ByteString response)
	{
		LegionWarMarkPointAck legionWarMarkPointAck = LegionWarMarkPointAck.Parser.ParseFrom(response);
		if (legionWarMarkPointAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarMarkPointAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarMarkPointAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarMarkPointAck.LegionWarMapInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMapInfo);
		}
	}

	public void LegionWar_FightStart_Req()
	{
		LegionWarFightStartReq req = new LegionWarFightStartReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarFightStartReq);
	}

	public void LegionWar_FightStart_Ack(ByteString response)
	{
		LegionWarFightStartAck legionWarFightStartAck = LegionWarFightStartAck.Parser.ParseFrom(response);
		if (legionWarFightStartAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarFightStartAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.currPlayerId = legionWarFightStartAck.PlayerId;
			zbSelf.Instance.legionWarHandler.currActionPoints = legionWarFightStartAck.ActionPoints;
			zbUtil.Debug2LogFile("LegionWar_FightStart_Ack.txt", legionWarFightStartAck);
			zbSelf.Instance.combatHandler.StartShowCombat(legionWarFightStartAck.BattleInfo);
		}
	}

	public void LegionWar_FightResult_Req(int playerId, BattleResultInfo battleResult)
	{
		LegionWarFightResultReq legionWarFightResultReq = new LegionWarFightResultReq();
		legionWarFightResultReq.PlayerId = playerId;
		legionWarFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)legionWarFightResultReq, Message.LegionWarFightResultReq);
	}

	public void LegionWar_FightResult_Ack(ByteString response)
	{
		LegionWarFightResultAck legionWarFightResultAck = LegionWarFightResultAck.Parser.ParseFrom(response);
		if (legionWarFightResultAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(legionWarFightResultAck.DataChange);
			zbSelf.Instance.legionWarHandler.legionWarLegionInfo = legionWarFightResultAck.LegionWarLegionInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfInfo = legionWarFightResultAck.LegionWarSelfInfo;
			zbSelf.Instance.legionWarHandler.legionWarPointInfo = legionWarFightResultAck.LegionWarPointInfo;
			zbSelf.Instance.legionWarHandler.UpdateLegionWarBaseInfo(legionWarFightResultAck.LegionWarBaseInfo);
			zbCombatSystem.Instance.battleAwardList = legionWarFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(legionWarFightResultAck.BattleSettle);
		}
	}

	public void LegionWar_FightCancel_Req()
	{
		LegionWarFightCancelReq req = new LegionWarFightCancelReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarFightCancelReq);
	}

	public void LegionWar_FightCancel_Ack(ByteString response)
	{
		LegionWarFightCancelAck legionWarFightCancelAck = LegionWarFightCancelAck.Parser.ParseFrom(response);
		if (legionWarFightCancelAck != null)
		{
			zbSelf.Instance.legionWarHandler.UpdateLegionWarBaseInfo(legionWarFightCancelAck.LegionWarBaseInfo);
		}
	}

	public void LegionWar_LegionRankList_Req()
	{
		LegionWarLegionRankListReq req = new LegionWarLegionRankListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarLegionRankListReq);
	}

	public void LegionWar_LegionRankList_Ack(ByteString response)
	{
		LegionWarLegionRankListAck legionWarLegionRankListAck = LegionWarLegionRankListAck.Parser.ParseFrom(response);
		if (legionWarLegionRankListAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarLegionRankListAck = legionWarLegionRankListAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarRankInfo);
		}
	}

	public void LegionWar_PlayerRankList_Req()
	{
		LegionWarPlayerRankListReq req = new LegionWarPlayerRankListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarPlayerRankListReq);
	}

	public void LegionWar_PlayerRankList_Ack(ByteString response)
	{
		LegionWarPlayerRankListAck legionWarPlayerRankListAck = LegionWarPlayerRankListAck.Parser.ParseFrom(response);
		if (legionWarPlayerRankListAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck = legionWarPlayerRankListAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarRankInfo);
		}
	}

	public void LegionWar_WatchMapInfo_Req(int eLegionZone, bool openLegionWar = false)
	{
		zbSelf.Instance.legionWarHandler.openLegionWar = openLegionWar;
		LegionWarWatchMapInfoReq legionWarWatchMapInfoReq = new LegionWarWatchMapInfoReq();
		legionWarWatchMapInfoReq.WarZone = eLegionZone;
		Instance.WsPackMsg((IMessage)(object)legionWarWatchMapInfoReq, Message.LegionWarWatchMapInfoReq);
	}

	public void LegionWar_WatchMapInfo_Ack(ByteString response)
	{
		LegionWarWatchMapInfoAck legionWarWatchMapInfoAck = LegionWarWatchMapInfoAck.Parser.ParseFrom(response);
		if (legionWarWatchMapInfoAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarMapInfo = legionWarWatchMapInfoAck.LegionWarMapInfo;
			if (zbSelf.Instance.legionWarHandler.openLegionWar)
			{
				zbUIMgr.Instance.ShowLegionWarWatch();
			}
			zbSelf.Instance.legionWarHandler.openLegionWar = false;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarMapInfo);
		}
	}

	public void LegionWar_ModSelect_Req(int selectId)
	{
		LegionWarModSelectReq legionWarModSelectReq = new LegionWarModSelectReq();
		legionWarModSelectReq.SelectId = selectId;
		Instance.WsPackMsg((IMessage)(object)legionWarModSelectReq, Message.LegionWarModSelectReq);
	}

	public void LegionWar_ModSelect_Ack(ByteString response)
	{
		LegionWarModSelectAck legionWarModSelectAck = LegionWarModSelectAck.Parser.ParseFrom(response);
		if (legionWarModSelectAck != null)
		{
			zbServerData.sGameData.LegionWarBaseInfo = legionWarModSelectAck.LegionWarBaseInfo;
			zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo = legionWarModSelectAck.LegionWarSelfTeamInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarModSelect);
		}
	}

	public void LegionWar_ZhanLingLevel_Req()
	{
		LegionWarZhanLingLevelReq req = new LegionWarZhanLingLevelReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarZhanLingLevelReq);
	}

	public void LegionWar_ZhanLingLevel_Ack(ByteString response)
	{
		LegionWarZhanLingLevelAck legionWarZhanLingLevelAck = LegionWarZhanLingLevelAck.Parser.ParseFrom(response);
		if (legionWarZhanLingLevelAck != null)
		{
			_ = zbSelf.Instance.legionWarHandler.PreZhanlingInfo;
			_ = legionWarZhanLingLevelAck.LegionWarBaseInfo.LegionWarZhanLingInfo.HighActive;
			_ = legionWarZhanLingLevelAck.LegionWarBaseInfo.LegionWarZhanLingInfo;
			zbSelf.Instance.legionWarHandler.CheckHighZhanling(zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo, legionWarZhanLingLevelAck.LegionWarBaseInfo.LegionWarZhanLingInfo);
			zbSelf.Instance.legionWarHandler.UpdateLegionWarBaseInfo(legionWarZhanLingLevelAck.LegionWarBaseInfo);
			zbSelf.Instance.legionWarHandler.legionWarSelfTeamInfo = legionWarZhanLingLevelAck.LegionWarSelfTeamInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarZhanlingLevelUp);
		}
	}

	public void LegionWar_ZhanLingAward_Req()
	{
		LegionWarZhanLingAwardReq req = new LegionWarZhanLingAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarZhanLingAwardReq);
	}

	public void LegionWar_ZhanLingAward_Ack(ByteString response)
	{
		LegionWarZhanLingAwardAck legionWarZhanLingAwardAck = LegionWarZhanLingAwardAck.Parser.ParseFrom(response);
		if (legionWarZhanLingAwardAck != null)
		{
			zbSelf.Instance.legionWarHandler.UpdateLegionWarBaseInfo(legionWarZhanLingAwardAck.LegionWarBaseInfo);
			zbSelf.Instance.dataHandler.DataChange(legionWarZhanLingAwardAck.DataChange, ((IEnumerable<AwardDetail>)legionWarZhanLingAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarData);
		}
	}

	public void LegionWar_SettleLegionInfo_Req(int LegionZone = 0, bool openWnd = false)
	{
		zbSelf.Instance.legionWarHandler.openLegionWarSettle = openWnd;
		LegionWarSettleLegionInfoReq legionWarSettleLegionInfoReq = new LegionWarSettleLegionInfoReq();
		legionWarSettleLegionInfoReq.SettleZone = LegionZone;
		Instance.WsPackMsg((IMessage)(object)legionWarSettleLegionInfoReq, Message.LegionWarSettleLegionInfoReq);
	}

	public void LegionWar_SettleLegionInfo_Ack(ByteString response)
	{
		LegionWarSettleLegionInfoAck legionWarSettleLegionInfoAck = LegionWarSettleLegionInfoAck.Parser.ParseFrom(response);
		if (legionWarSettleLegionInfoAck == null)
		{
			return;
		}
		zbSelf.Instance.legionWarHandler.maxZone = legionWarSettleLegionInfoAck.MaxZone;
		zbSelf.Instance.legionWarHandler.currSettleZone = legionWarSettleLegionInfoAck.SettleZone;
		zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck = legionWarSettleLegionInfoAck;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarSettleInfo);
		if (!zbSelf.Instance.legionWarHandler.openLegionWarSettle)
		{
			return;
		}
		zbSelf.Instance.legionWarHandler.openLegionWarSettle = false;
		if (legionWarSettleLegionInfoAck.SettleTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11233"));
			return;
		}
		zbSelf.Instance.legionWarHandler.getLegionWarOpenInfo();
		if (zbSelf.Instance.legionWarHandler.m_legionWarState >= ELegionWarState.Finished && zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId != ELegionWarForce.EmptyValue)
		{
			int num = zbGameSave.Instance.GetInt(zbSelf.Instance.legionWarHandler.saveSettleStr);
			if (zbServerData.sGameData.LegionWarBaseInfo.Season > num)
			{
				zbGameSave.Instance.SetInt(zbSelf.Instance.legionWarHandler.saveSettleStr, zbServerData.sGameData.LegionWarBaseInfo.Season);
				int redScoreCount = 0;
				int blueScoreCount = 0;
				foreach (LegionWarSettleLegionInfo force in legionWarSettleLegionInfoAck.ForceList)
				{
					if (force.ForceId == ELegionWarForce.Red)
					{
						redScoreCount = force.ScoreCount;
					}
					else
					{
						blueScoreCount = force.ScoreCount;
					}
				}
				zbUIMgr.Instance.ShowLegionWarSettlePreview(legionWarSettleLegionInfoAck.WinForceId, redScoreCount, blueScoreCount);
			}
			else
			{
				zbUIMgr.Instance.ShowLegionWarSettle();
			}
		}
		else
		{
			zbUIMgr.Instance.ShowLegionWarSettle();
		}
	}

	public void LegionWar_SettlePlayerInfo_Req(int LegionZone = 0, bool openWnd = false)
	{
		zbSelf.Instance.legionWarHandler.openLegionWarSettle = openWnd;
		LegionWarSettlePlayerInfoReq legionWarSettlePlayerInfoReq = new LegionWarSettlePlayerInfoReq();
		legionWarSettlePlayerInfoReq.SettleZone = LegionZone;
		Instance.WsPackMsg((IMessage)(object)legionWarSettlePlayerInfoReq, Message.LegionWarSettlePlayerInfoReq);
	}

	public void LegionWar_SettlePlayerInfo_Ack(ByteString response)
	{
		LegionWarSettlePlayerInfoAck legionWarSettlePlayerInfoAck = LegionWarSettlePlayerInfoAck.Parser.ParseFrom(response);
		if (legionWarSettlePlayerInfoAck == null)
		{
			return;
		}
		zbSelf.Instance.legionWarHandler.maxZone = legionWarSettlePlayerInfoAck.MaxZone;
		zbSelf.Instance.legionWarHandler.currSettleZone = legionWarSettlePlayerInfoAck.SettleZone;
		zbSelf.Instance.legionWarHandler.legionWarSettlePlayerInfoAck = legionWarSettlePlayerInfoAck;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegionWarSettleInfo);
		if (!zbSelf.Instance.legionWarHandler.openLegionWarSettle)
		{
			return;
		}
		zbSelf.Instance.legionWarHandler.openLegionWarSettle = false;
		if (legionWarSettlePlayerInfoAck.SettleTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11233"));
			return;
		}
		zbSelf.Instance.legionWarHandler.getLegionWarOpenInfo();
		if (zbSelf.Instance.legionWarHandler.m_legionWarState >= ELegionWarState.Finished && zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionForceId != ELegionWarForce.EmptyValue)
		{
			int num = zbGameSave.Instance.GetInt(zbSelf.Instance.legionWarHandler.saveSettleStr);
			if (zbServerData.sGameData.LegionWarBaseInfo.Season > num)
			{
				zbGameSave.Instance.SetInt(zbSelf.Instance.legionWarHandler.saveSettleStr, zbServerData.sGameData.LegionWarBaseInfo.Season);
				int redScoreCount = 0;
				int blueScoreCount = 0;
				foreach (LegionWarSettlePlayerInfo force in legionWarSettlePlayerInfoAck.ForceList)
				{
					if (force.ForceId == ELegionWarForce.Red)
					{
						redScoreCount = force.ScoreCount;
					}
					else
					{
						blueScoreCount = force.ScoreCount;
					}
				}
				zbUIMgr.Instance.ShowLegionWarSettlePreview(legionWarSettlePlayerInfoAck.WinForceId, redScoreCount, blueScoreCount);
			}
			else
			{
				zbUIMgr.Instance.ShowLegionWarSettle();
				zbSelf.Instance.legionWarHandler.ShowLegionWarEffect(legionWarSettlePlayerInfoAck);
			}
		}
		else
		{
			zbUIMgr.Instance.ShowLegionWarSettle();
			zbSelf.Instance.legionWarHandler.ShowLegionWarEffect(legionWarSettlePlayerInfoAck);
		}
	}

	public void LegionWar_TaskAward_Req(ELegionWarTaskType taskType)
	{
		LegionWarTaskAwardReq legionWarTaskAwardReq = new LegionWarTaskAwardReq();
		legionWarTaskAwardReq.TaskType = taskType;
		Instance.WsPackMsg((IMessage)(object)legionWarTaskAwardReq, Message.LegionWarTaskAwardReq);
	}

	public void LegionWar_TaskAward_Ack(ByteString response)
	{
		LegionWarTaskAwardAck legionWarTaskAwardAck = LegionWarTaskAwardAck.Parser.ParseFrom(response);
		if (legionWarTaskAwardAck != null)
		{
			zbServerData.sGameData.LegionWarBaseInfo = legionWarTaskAwardAck.LegionWarBaseInfo;
			zbSelf.Instance.dataHandler.DataChange(legionWarTaskAwardAck.DataChange, ((IEnumerable<AwardDetail>)legionWarTaskAwardAck.AwardList).ToArray());
			zbLegionWarTaskWnd zbLegionWarTaskWnd2 = zbUIMgr.Instance.GetWndById(EWndId.LegionWarTask) as zbLegionWarTaskWnd;
			if ((Object)(object)zbLegionWarTaskWnd2 != (Object)null)
			{
				zbLegionWarTaskWnd2.Refresh();
			}
		}
	}

	public void LegionWar_RecordList_Req()
	{
		LegionWarRecordListReq req = new LegionWarRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.LegionWarRecordListReq);
	}

	public void LegionWar_RecordList_Ack(ByteString response)
	{
		LegionWarRecordListAck legionWarRecordListAck = LegionWarRecordListAck.Parser.ParseFrom(response);
		if (legionWarRecordListAck != null)
		{
			zbSelf.Instance.legionWarHandler.legionWarRecordDetails = ((IEnumerable<LegionWarRecordDetail>)legionWarRecordListAck.RecordList).ToArray();
			zbUIMgr.Instance.ShowLegionWarLog();
		}
	}

	public void Game_Kick_Req()
	{
		GameKickReq gameKickReq = new GameKickReq();
		gameKickReq.Guid = zbLoginUtil.guid;
		gameKickReq.ServerId = zbLoginUtil.loginServerId;
		gameKickReq.Ticket = zbLoginUtil.ticket;
		Instance.WsPackMsg((IMessage)(object)gameKickReq, Message.GameKickReq);
	}

	public void Game_Kick_Ack(ByteString response)
	{
		if (GameKickAck.Parser.ParseFrom(response) != null)
		{
			Instance.Game_Login_Req();
		}
	}

	public void Game_Kick_Ntf(ByteString response)
	{
		if (GameKickNtf.Parser.ParseFrom(response) != null)
		{
			zbUIMgr.Instance.BubbleInfo("Account logged in elsewhere");
		}
	}

	public void Game_Login_Req()
	{
		GameLoginReq gameLoginReq = new GameLoginReq();
		gameLoginReq.AppChannel = zbLoginUtil.appChannel;
		gameLoginReq.Platform = zbLoginUtil.platForm;
		gameLoginReq.ServerId = zbLoginUtil.loginServerId;
		gameLoginReq.Guid = zbLoginUtil.guid;
		gameLoginReq.Ticket = zbLoginUtil.ticket;
		Instance.WsPackMsg((IMessage)(object)gameLoginReq, Message.GameLoginReq);
	}

	public void Game_Login_Ack(ByteString response)
	{
		GameLoginAck gameLoginAck = GameLoginAck.Parser.ParseFrom(response);
		if (gameLoginAck != null)
		{
			zbServerData.sGameEnv = gameLoginAck.GameEnv;
			zbServerData.sRoleState = gameLoginAck.RoleState;
			if (gameLoginAck.RoleState.State == ERoleState.Init)
			{
				Role_Create_Req();
			}
			else
			{
				Role_Ready_Req();
			}
		}
	}

	public void Role_Create_Req()
	{
		RoleCreateReq roleCreateReq = new RoleCreateReq();
		roleCreateReq.AvatarId = 0;
		roleCreateReq.HeadId = 0;
		roleCreateReq.RoleName = "";
		Instance.WsPackMsg((IMessage)(object)roleCreateReq, Message.RoleCreateReq);
	}

	public void Role_Create_Ack(ByteString response)
	{
		RoleCreateAck roleCreateAck = RoleCreateAck.Parser.ParseFrom(response);
		if (roleCreateAck != null)
		{
			zbServerData.sRoleState = roleCreateAck.RoleState;
			Role_Ready_Req();
		}
	}

	public void Role_Ready_Req()
	{
		RoleReadyReq req = new RoleReadyReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.RoleReadyReq);
	}

	public void Role_Ready_Ack(ByteString response)
	{
		RoleReadyAck roleReadyAck = RoleReadyAck.Parser.ParseFrom(response);
		if (roleReadyAck != null)
		{
			zbServerData.sGameData = roleReadyAck.GameData;
			zbGameStateMachine.Instance.SetState(EGameState.Main, immediately: true);
			zbSelf.Instance.dataHandler.lastHeroList = ((IEnumerable<HeroDetail>)zbServerData.sGameData.HeroList).ToArray();
			zbSelf.Instance.worldHandler.worldResIdleSettleSecond = roleReadyAck.GameData.WorldInfo.ResIdleInfo.SettleSecond;
			Role_ReadyEnd_Req();
		}
	}

	public void Role_ReadyEnd_Req()
	{
		Instance.WsPackMsg(null, Message.RoleReadyEndReq);
	}

	public void Game_LoginOut_Req()
	{
		RoleReadyReq req = new RoleReadyReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.GameLoginOutReq);
	}

	public void Game_LoginOut_Ack(ByteString response)
	{
		zbGameStateMachine.Instance.SetState(EGameState.Login);
	}

	public void HeartBeat_Req()
	{
		Instance.WsPackMsg(null, Message.HeartBeatReq, needTip: false);
	}

	public void HeartBeat_Ack(ByteString response)
	{
		zbWsHeartStateMachine.Instance.GetState()?.OnNotifyHeartAck();
		HeartBeatAck heartBeatAck = HeartBeatAck.Parser.ParseFrom(response);
		if (heartBeatAck != null && zbGameStateMachine.Instance.GetStateId() == EGameState.Main)
		{
			zbServerData.sGameEnv = heartBeatAck.GameEnv;
			zbSelf.Instance.timeHandler.SetServerTime(zbServerData.sGameEnv.ServTime);
		}
	}

	private void Error_Ack(ByteString response)
	{
		zbLoginUtil.ErrMsgHandle(ErrorCodeAck.Parser.ParseFrom(response));
	}

	public void Abnormal_ClientReport_Req(EFuncType FuncType, float OriginValue, float ModifyValue, string ClientVersion, string ReportDesc)
	{
		AbnormalClientReportReq abnormalClientReportReq = new AbnormalClientReportReq();
		abnormalClientReportReq.FuncType = FuncType;
		abnormalClientReportReq.OriginValue = OriginValue;
		abnormalClientReportReq.ModifyValue = ModifyValue;
		abnormalClientReportReq.ClientVersion = ClientVersion;
		abnormalClientReportReq.ReportDesc = ReportDesc;
		Instance.WsPackMsg((IMessage)(object)abnormalClientReportReq, Message.AbnormalClientReportReq);
	}

	public void Abnormal_ClientReport_Ack(ByteString response)
	{
		AbnormalClientReportAck.Parser.ParseFrom(response);
	}

	public void Mail_GetList_Req(bool bOpenMailWnd = false)
	{
		m_bOpenMailWnd = bOpenMailWnd;
		MailGetListReq mailGetListReq = new MailGetListReq();
		mailGetListReq.Lang = zbLanguage.Instance.GetLanguageType();
		Instance.WsPackMsg((IMessage)(object)mailGetListReq, Message.MailGetListReq);
	}

	public void Mail_GetList_Ack(ByteString response)
	{
		MailGetListAck mailGetListAck = MailGetListAck.Parser.ParseFrom(response);
		if (mailGetListAck != null)
		{
			zbSelf.Instance.mailHandler.UpdateMailList(mailGetListAck.MailList);
			if (m_bOpenMailWnd)
			{
				m_bOpenMailWnd = false;
				zbUIMgr.Instance.ShowMail();
			}
		}
	}

	public void Mail_Read_Req(long MailId)
	{
		MailReadReq mailReadReq = new MailReadReq();
		mailReadReq.Lang = zbLanguage.Instance.GetLanguageType();
		mailReadReq.MailId = MailId;
		Instance.WsPackMsg((IMessage)(object)mailReadReq, Message.MailReadReq);
	}

	public void Mail_Read_Ack(ByteString response)
	{
		MailReadAck mailReadAck = MailReadAck.Parser.ParseFrom(response);
		if (mailReadAck != null)
		{
			zbSelf.Instance.mailHandler.UpdateMailRead(mailReadAck.Mail, mailReadAck.Content);
		}
	}

	public void Mail_GetAward_Req(long MailId)
	{
		MailGetAwardReq mailGetAwardReq = new MailGetAwardReq();
		mailGetAwardReq.Lang = zbLanguage.Instance.GetLanguageType();
		mailGetAwardReq.MailId = MailId;
		Instance.WsPackMsg((IMessage)(object)mailGetAwardReq, Message.MailGetAwardReq);
	}

	public void Mail_GetAward_Ack(ByteString response)
	{
		MailGetAwardAck mailGetAwardAck = MailGetAwardAck.Parser.ParseFrom(response);
		if (mailGetAwardAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mailGetAwardAck.DataChange, ((IEnumerable<AwardDetail>)mailGetAwardAck.AwardList).ToArray());
			zbSelf.Instance.mailHandler.UpdateMailDetail(mailGetAwardAck.Mail);
		}
	}

	public void Mail_Delete_Req(long[] IdList)
	{
		MailDeleteReq mailDeleteReq = new MailDeleteReq();
		mailDeleteReq.IdList.Add((IEnumerable<long>)IdList);
		Instance.WsPackMsg((IMessage)(object)mailDeleteReq, Message.MailDeleteReq);
	}

	public void Mail_Delete_Ack(ByteString response)
	{
		MailDeleteAck mailDeleteAck = MailDeleteAck.Parser.ParseFrom(response);
		if (mailDeleteAck != null)
		{
			zbSelf.Instance.mailHandler.DeleteMailList(mailDeleteAck.IdList);
		}
	}

	public void Mail_GetAllAward_Req()
	{
		MailGetAllAwardReq mailGetAllAwardReq = new MailGetAllAwardReq();
		mailGetAllAwardReq.Lang = zbLanguage.Instance.GetLanguageType();
		Instance.WsPackMsg((IMessage)(object)mailGetAllAwardReq, Message.MailGetAllAwardReq);
	}

	public void Mail_GetAllAward_Ack(ByteString response)
	{
		MailGetAllAwardAck mailGetAllAwardAck = MailGetAllAwardAck.Parser.ParseFrom(response);
		if (mailGetAllAwardAck != null)
		{
			zbSelf.Instance.mailHandler.UpdateMailList(mailGetAllAwardAck.MailList);
			zbSelf.Instance.dataHandler.DataChange(mailGetAllAwardAck.DataChange, ((IEnumerable<AwardDetail>)mailGetAllAwardAck.AwardList).ToArray());
		}
	}

	public void MainQuest_Data_Req()
	{
		MainQuestDataReq req = new MainQuestDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.MainQuestDataReq);
	}

	public void MainQuest_Data_Ack(ByteString response)
	{
		MainQuestDataAck mainQuestDataAck = MainQuestDataAck.Parser.ParseFrom(response);
		if (mainQuestDataAck != null)
		{
			zbServerData.sGameData.MainQuestInfo = mainQuestDataAck.MainQuestInfo;
		}
	}

	public void MainQuest_FightStart_Req(int LevelId)
	{
		MainQuestFightStartReq mainQuestFightStartReq = new MainQuestFightStartReq();
		mainQuestFightStartReq.LevelId = LevelId;
		Instance.WsPackMsg((IMessage)(object)mainQuestFightStartReq, Message.MainQuestFightStartReq);
	}

	public void MainQuest_FightStart_Ack(ByteString response)
	{
		MainQuestFightStartAck mainQuestFightStartAck = MainQuestFightStartAck.Parser.ParseFrom(response);
		if (mainQuestFightStartAck != null)
		{
			zbUtil.Debug2LogFile("MainQuest_FightStart.txt", mainQuestFightStartAck);
			zbSelf.Instance.combatHandler.StartShowCombat(mainQuestFightStartAck.BattleInfo);
			if (mainQuestFightStartAck.LevelId == 1)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(609);
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Quest_Level1);
			}
			else if (mainQuestFightStartAck.LevelId == 2)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(610);
			}
			else if (mainQuestFightStartAck.LevelId == 3)
			{
				zbProgressEventHelper.Instance.SetProgressTrack(611);
			}
		}
	}

	public void MainQuest_FightResult_Req(int LevelId, BattleResultInfo BattleResult)
	{
		if (LevelId == 2 && !zbGameSave.Instance.GetBool("FirstLevel2Result" + zbSelf.Instance.GetRoleId()))
		{
			zbSelf.Instance.mainQuestHandler.isLevel2FirstResult = true;
			zbGameSave.Instance.SetBool("FirstLevel2Result" + zbSelf.Instance.GetRoleId(), value: true);
		}
		MainQuestFightResultReq mainQuestFightResultReq = new MainQuestFightResultReq();
		mainQuestFightResultReq.LevelId = LevelId;
		mainQuestFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)mainQuestFightResultReq, Message.MainQuestFightResultReq);
	}

	public void MainQuest_FightResult_Ack(ByteString response)
	{
		MainQuestFightResultAck mainQuestFightResultAck = MainQuestFightResultAck.Parser.ParseFrom(response);
		if (mainQuestFightResultAck != null)
		{
			zbUtil.Debug2LogFile("MainQuest_FightResult_Ack.txt", mainQuestFightResultAck);
			zbServerData.sGameData.MainQuestInfo = mainQuestFightResultAck.MainQuestInfo;
			zbSelf.Instance.dataHandler.DataChange(mainQuestFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = mainQuestFightResultAck.AwardList;
			((zbCombatBalanceWnd)zbCombatSystem.Instance.SetBanlanceData(mainQuestFightResultAck.BattleSettle)).ShowNewRecord(mainQuestFightResultAck.BestScore);
		}
	}

	public void MainQuest_ScoreAward_Req(int levelId)
	{
		MainQuestScoreAwardReq mainQuestScoreAwardReq = new MainQuestScoreAwardReq();
		mainQuestScoreAwardReq.LevelId = levelId;
		Instance.WsPackMsg((IMessage)(object)mainQuestScoreAwardReq, Message.MainQuestScoreAwardReq);
	}

	public void MainQuest_ScoreAward_Ack(ByteString response)
	{
		MainQuestScoreAwardAck mainQuestScoreAwardAck = MainQuestScoreAwardAck.Parser.ParseFrom(response);
		if (mainQuestScoreAwardAck != null)
		{
			zbServerData.sGameData.MainQuestInfo = mainQuestScoreAwardAck.MainQuestInfo;
			if (zbUIMgr.Instance.IsShow(EWndId.MainQuestReceiveAward))
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Quest_Receive, mainQuestScoreAwardAck);
				return;
			}
			zbSelf.Instance.dataHandler.DataChange(mainQuestScoreAwardAck.DataChange, ((IEnumerable<AwardDetail>)mainQuestScoreAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Quest_Refresh);
		}
	}

	public void Mecha_Data_Req()
	{
		MechaDataReq req = new MechaDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.MechaDataReq);
	}

	public void Mecha_Data_Ack(ByteString response)
	{
		MechaDataAck mechaDataAck = MechaDataAck.Parser.ParseFrom(response);
		if (mechaDataAck != null)
		{
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaDataAck.MechaInfo);
		}
	}

	public void Mecha_Active_Req(int MechaId)
	{
		MechaActiveReq mechaActiveReq = new MechaActiveReq();
		mechaActiveReq.MechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaActiveReq, Message.MechaActiveReq);
	}

	public void Mecha_Active_Ack(ByteString response)
	{
		MechaActiveAck mechaActiveAck = MechaActiveAck.Parser.ParseFrom(response);
		if (mechaActiveAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaActiveAck.DataChange);
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaActiveAck.MechaInfo);
			zbUIMgr.Instance.ShowMechaGet(mechaActiveAck.MechaId);
			if (zbServerData.sGameData.MechaInfo.MechaList.Count > 0 && zbSelf.Instance.GetMechaId() <= 0)
			{
				Instance.Mecha_Set_Req(mechaActiveAck.MechaId);
			}
		}
	}

	public void Mecha_Set_Req(int MechaId)
	{
		zbSelf.Instance.mechaHandler.mechaSetId = MechaId;
		MechaSetReq mechaSetReq = new MechaSetReq();
		mechaSetReq.MechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaSetReq, Message.MechaSetReq);
	}

	public void Mecha_Set_Ack(ByteString response)
	{
		MechaSetAck mechaSetAck = MechaSetAck.Parser.ParseFrom(response);
		if (mechaSetAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaSetAck.DataChange);
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaSetAck.MechaInfo);
		}
	}

	public void Mecha_UpgradeLevel_Req(int MechaId)
	{
		MechaUpgradeLevelReq mechaUpgradeLevelReq = new MechaUpgradeLevelReq();
		mechaUpgradeLevelReq.MechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaUpgradeLevelReq, Message.MechaUpgradeLevelReq);
	}

	public void Mecha_UpgradeLevel_Ack(ByteString response)
	{
		MechaUpgradeLevelAck mechaUpgradeLevelAck = MechaUpgradeLevelAck.Parser.ParseFrom(response);
		if (mechaUpgradeLevelAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaUpgradeLevelAck.DataChange);
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaUpgradeLevelAck.MechaInfo);
		}
	}

	public void Mecha_UpgradeStar_Req(int MechaId)
	{
		tempMechaId = MechaId;
		MechaUpgradeStarReq mechaUpgradeStarReq = new MechaUpgradeStarReq();
		mechaUpgradeStarReq.MechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaUpgradeStarReq, Message.MechaUpgradeStarReq);
	}

	public void Mecha_UpgradeStar_Ack(ByteString response)
	{
		MechaUpgradeStarAck mechaUpgradeStarAck = MechaUpgradeStarAck.Parser.ParseFrom(response);
		if (mechaUpgradeStarAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaUpgradeStarAck.DataChange);
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaUpgradeStarAck.MechaInfo);
			zbUIMgr.Instance.ShowMechaStarSuccess(tempMechaId);
			tempMechaId = 0;
		}
	}

	public void Mecha_UpgradeSkill_Req(int MechaId, int Index)
	{
		MechaUpgradeSkillReq mechaUpgradeSkillReq = new MechaUpgradeSkillReq();
		mechaUpgradeSkillReq.MechaId = MechaId;
		mechaUpgradeSkillReq.Index = Index;
		Instance.WsPackMsg((IMessage)(object)mechaUpgradeSkillReq, Message.MechaUpgradeSkillReq);
	}

	public void Mecha_UpgradeSkill_Ack(ByteString response)
	{
		MechaUpgradeSkillAck mechaUpgradeSkillAck = MechaUpgradeSkillAck.Parser.ParseFrom(response);
		if (mechaUpgradeSkillAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaUpgradeSkillAck.DataChange);
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaUpgradeSkillAck.MechaInfo);
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
			zbUIMgr.Instance.ShowCommonLevelUp();
		}
	}

	public void Mecha_Convert_Req(int[] SelectList)
	{
		MechaConvertReq mechaConvertReq = new MechaConvertReq();
		mechaConvertReq.SelectList.Clear();
		mechaConvertReq.SelectList.AddRange((IEnumerable<int>)SelectList);
		Instance.WsPackMsg((IMessage)(object)mechaConvertReq, Message.MechaConvertReq);
	}

	public void Mecha_Convert_Ack(ByteString response)
	{
		MechaConvertAck mechaConvertAck = MechaConvertAck.Parser.ParseFrom(response);
		if (mechaConvertAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaConvertAck.DataChange, ((IEnumerable<AwardDetail>)mechaConvertAck.AwardList).ToArray());
		}
	}

	public void Mecha_ResetCalc_Req(int MechaId)
	{
		MechaResetCalcReq mechaResetCalcReq = new MechaResetCalcReq();
		mechaResetCalcReq.MechaId = MechaId;
		resetMechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaResetCalcReq, Message.MechaResetCalcReq);
	}

	public void Mecha_ResetCalc_Ack(ByteString response)
	{
		MechaResetCalcAck mechaResetCalcAck = MechaResetCalcAck.Parser.ParseFrom(response);
		if (mechaResetCalcAck != null)
		{
			zbUIMgr.Instance.ShowMechaReset(resetMechaId, ((IEnumerable<AwardDetail>)mechaResetCalcAck.AwardList).ToArray());
		}
	}

	public void Mecha_Reset_Req(int MechaId)
	{
		MechaResetReq mechaResetReq = new MechaResetReq();
		mechaResetReq.MechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaResetReq, Message.MechaResetReq);
	}

	public void Mecha_Reset_Ack(ByteString response)
	{
		MechaResetAck mechaResetAck = MechaResetAck.Parser.ParseFrom(response);
		if (mechaResetAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaResetAck.DataChange, ((IEnumerable<AwardDetail>)mechaResetAck.AwardList).ToArray());
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaResetAck.MechaInfo);
		}
	}

	public void Mecha_FastUpgradeLevelCalc_Req(int MechaId)
	{
		MechaFastUpgradeLevelCalcReq mechaFastUpgradeLevelCalcReq = new MechaFastUpgradeLevelCalcReq();
		mechaFastUpgradeLevelCalcReq.MechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaFastUpgradeLevelCalcReq, Message.MechaFastUpgradeLevelCalcReq);
	}

	public void Mecha_FastUpgradeLevelCalc_Ack(ByteString response)
	{
		MechaFastUpgradeLevelCalcAck mechaFastUpgradeLevelCalcAck = MechaFastUpgradeLevelCalcAck.Parser.ParseFrom(response);
		if (mechaFastUpgradeLevelCalcAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_MechaFastUpgradeLevelCalc_Ack, mechaFastUpgradeLevelCalcAck);
		}
	}

	public void Mecha_FastUpgradeLevel_Req(int MechaId)
	{
		MechaFastUpgradeLevelReq mechaFastUpgradeLevelReq = new MechaFastUpgradeLevelReq();
		mechaFastUpgradeLevelReq.MechaId = MechaId;
		Instance.WsPackMsg((IMessage)(object)mechaFastUpgradeLevelReq, Message.MechaFastUpgradeLevelReq);
	}

	public void Mecha_FastUpgradeLevel_Ack(ByteString response)
	{
		MechaFastUpgradeLevelAck mechaFastUpgradeLevelAck = MechaFastUpgradeLevelAck.Parser.ParseFrom(response);
		if (mechaFastUpgradeLevelAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mechaFastUpgradeLevelAck.DataChange);
			zbSelf.Instance.mechaHandler.UpdateMechaInfo(mechaFastUpgradeLevelAck.MechaInfo);
		}
	}

	public void Operate_MechaSubjectData_Req()
	{
		OperateMechaSubjectDataReq req = new OperateMechaSubjectDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateMechaSubjectDataReq);
	}

	public void Operate_MechaSubjectData_Ack(ByteString response)
	{
		OperateMechaSubjectDataAck operateMechaSubjectDataAck = OperateMechaSubjectDataAck.Parser.ParseFrom(response);
		if (operateMechaSubjectDataAck != null)
		{
			zbServerData.sGameData.OperateInfo.MechaSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.MechaSubjectList.AddRange((IEnumerable<MechaSubjectDetail>)operateMechaSubjectDataAck.MechaSubjectList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MechaSubject);
		}
	}

	public void Operate_MechaSubjectFightStart_Req(string UUID, int LevelId)
	{
		OperateMechaSubjectFightStartReq operateMechaSubjectFightStartReq = new OperateMechaSubjectFightStartReq();
		operateMechaSubjectFightStartReq.UUID = UUID;
		operateMechaSubjectFightStartReq.LevelId = LevelId;
		Instance.WsPackMsg((IMessage)(object)operateMechaSubjectFightStartReq, Message.OperateMechaSubjectFightStartReq);
	}

	public void Operate_MechaSubjectFightStart_Ack(ByteString response)
	{
		OperateMechaSubjectFightStartAck operateMechaSubjectFightStartAck = OperateMechaSubjectFightStartAck.Parser.ParseFrom(response);
		if (operateMechaSubjectFightStartAck != null)
		{
			zbSelf.Instance.mechaSubjectHandler.currUUID = operateMechaSubjectFightStartAck.UUID;
			zbSelf.Instance.mechaSubjectHandler.currLevelId = operateMechaSubjectFightStartAck.LevelId;
			zbSelf.Instance.combatHandler.StartShowCombat(operateMechaSubjectFightStartAck.BattleInfo);
		}
	}

	public void Operate_MechaSubjectFightResult_Req(string UUID, int LevelId, BattleResultInfo battleResult)
	{
		OperateMechaSubjectFightResultReq operateMechaSubjectFightResultReq = new OperateMechaSubjectFightResultReq();
		operateMechaSubjectFightResultReq.UUID = UUID;
		operateMechaSubjectFightResultReq.LevelId = LevelId;
		operateMechaSubjectFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)operateMechaSubjectFightResultReq, Message.OperateMechaSubjectFightResultReq);
	}

	public void Operate_MechaSubjectFightResult_Ack(ByteString response)
	{
		OperateMechaSubjectFightResultAck operateMechaSubjectFightResultAck = OperateMechaSubjectFightResultAck.Parser.ParseFrom(response);
		if (operateMechaSubjectFightResultAck != null)
		{
			zbServerData.sGameData.OperateInfo.MechaSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.MechaSubjectList.AddRange((IEnumerable<MechaSubjectDetail>)operateMechaSubjectFightResultAck.MechaSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateMechaSubjectFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = operateMechaSubjectFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(operateMechaSubjectFightResultAck.BattleSettle);
		}
	}

	public void Operate_MechaSubjectChapterAward_Req(string UUID, int ChapterId)
	{
		OperateMechaSubjectChapterAwardReq operateMechaSubjectChapterAwardReq = new OperateMechaSubjectChapterAwardReq();
		operateMechaSubjectChapterAwardReq.UUID = UUID;
		operateMechaSubjectChapterAwardReq.ChapterId = ChapterId;
		Instance.WsPackMsg((IMessage)(object)operateMechaSubjectChapterAwardReq, Message.OperateMechaSubjectChapterAwardReq);
	}

	public void Operate_MechaSubjectChapterAward_Ack(ByteString response)
	{
		OperateMechaSubjectChapterAwardAck operateMechaSubjectChapterAwardAck = OperateMechaSubjectChapterAwardAck.Parser.ParseFrom(response);
		if (operateMechaSubjectChapterAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.MechaSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.MechaSubjectList.AddRange((IEnumerable<MechaSubjectDetail>)operateMechaSubjectChapterAwardAck.MechaSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateMechaSubjectChapterAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateMechaSubjectChapterAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MechaSubject);
		}
	}

	public void Operate_MechaSubjectBarDraw_Req(string UUID, int Count)
	{
		OperateMechaSubjectBarDrawReq operateMechaSubjectBarDrawReq = new OperateMechaSubjectBarDrawReq();
		operateMechaSubjectBarDrawReq.UUID = UUID;
		operateMechaSubjectBarDrawReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)operateMechaSubjectBarDrawReq, Message.OperateMechaSubjectBarDrawReq);
	}

	public void Operate_MechaSubjectBarDraw_Ack(ByteString response)
	{
		OperateMechaSubjectBarDrawAck operateMechaSubjectBarDrawAck = OperateMechaSubjectBarDrawAck.Parser.ParseFrom(response);
		if (operateMechaSubjectBarDrawAck != null)
		{
			zbServerData.sGameData.OperateInfo.MechaSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.MechaSubjectList.AddRange((IEnumerable<MechaSubjectDetail>)operateMechaSubjectBarDrawAck.MechaSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateMechaSubjectBarDrawAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_MechaSubjectBarDraw_Ack, operateMechaSubjectBarDrawAck);
		}
	}

	public void Operate_MechaSubjectZhanLingAward_Req(string UUID)
	{
		OperateMechaSubjectZhanLingAwardReq operateMechaSubjectZhanLingAwardReq = new OperateMechaSubjectZhanLingAwardReq();
		operateMechaSubjectZhanLingAwardReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)operateMechaSubjectZhanLingAwardReq, Message.OperateMechaSubjectZhanLingAwardReq);
	}

	public void Operate_MechaSubjectZhanLingAward_Ack(ByteString response)
	{
		OperateMechaSubjectZhanLingAwardAck operateMechaSubjectZhanLingAwardAck = OperateMechaSubjectZhanLingAwardAck.Parser.ParseFrom(response);
		if (operateMechaSubjectZhanLingAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.MechaSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.MechaSubjectList.AddRange((IEnumerable<MechaSubjectDetail>)operateMechaSubjectZhanLingAwardAck.MechaSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateMechaSubjectZhanLingAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateMechaSubjectZhanLingAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MechaSubject);
		}
	}

	public void Operate_MechaSubjectRoulette_Req(string UUID, int Count)
	{
		OperateMechaSubjectRouletteReq operateMechaSubjectRouletteReq = new OperateMechaSubjectRouletteReq();
		operateMechaSubjectRouletteReq.UUID = UUID;
		operateMechaSubjectRouletteReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)operateMechaSubjectRouletteReq, Message.OperateMechaSubjectRouletteReq);
	}

	public void Operate_MechaSubjectRoulette_Ack(ByteString response)
	{
		OperateMechaSubjectRouletteAck operateMechaSubjectRouletteAck = OperateMechaSubjectRouletteAck.Parser.ParseFrom(response);
		if (operateMechaSubjectRouletteAck != null)
		{
			zbServerData.sGameData.OperateInfo.MechaSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.MechaSubjectList.AddRange((IEnumerable<MechaSubjectDetail>)operateMechaSubjectRouletteAck.MechaSubjectList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_MechaSubjectRoulette_Ack, operateMechaSubjectRouletteAck);
		}
	}

	public void Operate_MechaSubjectRouletteTarget_Req(string UUID)
	{
		OperateMechaSubjectRouletteTargetReq operateMechaSubjectRouletteTargetReq = new OperateMechaSubjectRouletteTargetReq();
		operateMechaSubjectRouletteTargetReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)operateMechaSubjectRouletteTargetReq, Message.OperateMechaSubjectRouletteTargetReq);
	}

	public void Operate_MechaSubjectRouletteTarget_Ack(ByteString response)
	{
		OperateMechaSubjectRouletteTargetAck operateMechaSubjectRouletteTargetAck = OperateMechaSubjectRouletteTargetAck.Parser.ParseFrom(response);
		if (operateMechaSubjectRouletteTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.MechaSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.MechaSubjectList.AddRange((IEnumerable<MechaSubjectDetail>)operateMechaSubjectRouletteTargetAck.MechaSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateMechaSubjectRouletteTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateMechaSubjectRouletteTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MechaSubject);
		}
	}

	public void MineWar_Data_Req(bool openWnd = false)
	{
		zbSelf.Instance.mineWarHandler.openWnd = openWnd;
		MineWarDataReq req = new MineWarDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.MineWarDataReq);
	}

	public void MineWar_Data_Ack(ByteString response)
	{
		MineWarDataAck mineWarDataAck = MineWarDataAck.Parser.ParseFrom(response);
		if (mineWarDataAck != null)
		{
			if (mineWarDataAck.ErrorCode != EErrorCode.EmptyValue)
			{
				zbUIMgr.Instance.BubbleErrorCodeId(mineWarDataAck.ErrorCode);
			}
			zbSelf.Instance.mineWarHandler.currServerId = zbSelf.Instance.GetServerId();
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarDataAck.MineWarBaseInfo, mineWarDataAck.MineWarMapInfo);
		}
		if (zbSelf.Instance.mineWarHandler.openWnd)
		{
			zbUIMgr.Instance.ShowMineWar();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MineWarBaseInfo);
		}
		zbSelf.Instance.mineWarHandler.openWnd = false;
	}

	public void MineWar_EmptyPoint_Req(int PointId)
	{
		MineWarEmptyPointReq mineWarEmptyPointReq = new MineWarEmptyPointReq();
		mineWarEmptyPointReq.PointId = PointId;
		Instance.WsPackMsg((IMessage)(object)mineWarEmptyPointReq, Message.MineWarEmptyPointReq);
	}

	public void MineWar_EmptyPoint_Ack(ByteString response)
	{
		MineWarEmptyPointAck mineWarEmptyPointAck = MineWarEmptyPointAck.Parser.ParseFrom(response);
		if (mineWarEmptyPointAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mineWarEmptyPointAck.DataChange, ((IEnumerable<AwardDetail>)mineWarEmptyPointAck.AwardList).ToArray());
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10962"));
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarEmptyPointAck.MineWarBaseInfo, mineWarEmptyPointAck.MineWarMapInfo);
		}
	}

	public void MineWar_PointData_Req(int PointId, bool openEnemyWnd = false)
	{
		MineWarPointDataReq mineWarPointDataReq = new MineWarPointDataReq();
		mineWarPointDataReq.PointId = PointId;
		zbSelf.Instance.mineWarHandler.openEnemyWnd = openEnemyWnd;
		Instance.WsPackMsg((IMessage)(object)mineWarPointDataReq, Message.MineWarPointDataReq);
	}

	public void MineWar_PointData_Ack(ByteString response)
	{
		MineWarPointDataAck mineWarPointDataAck = MineWarPointDataAck.Parser.ParseFrom(response);
		if (mineWarPointDataAck != null)
		{
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarPointDataAck.MineWarBaseInfo, null);
			if (zbSelf.Instance.mineWarHandler.openEnemyWnd)
			{
				zbUIMgr.Instance.ShowMineWarEnemy(mineWarPointDataAck, null);
			}
			else
			{
				zbUIMgr.Instance.ShowMineWarDetail(mineWarPointDataAck, null);
			}
		}
	}

	public void MineWar_FightStart_Req(int PointId, int PlayerId)
	{
		MineWarFightStartReq mineWarFightStartReq = new MineWarFightStartReq();
		mineWarFightStartReq.PlayerId = PlayerId;
		mineWarFightStartReq.PointId = PointId;
		Instance.WsPackMsg((IMessage)(object)mineWarFightStartReq, Message.MineWarFightStartReq);
	}

	public void MineWar_FightStart_Ack(ByteString response)
	{
		MineWarFightStartAck mineWarFightStartAck = MineWarFightStartAck.Parser.ParseFrom(response);
		if (mineWarFightStartAck != null)
		{
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarFightStartAck.MineWarBaseInfo, null);
			zbSelf.Instance.mineWarHandler.currPointId = mineWarFightStartAck.PointId;
			zbSelf.Instance.mineWarHandler.currPlayerId = mineWarFightStartAck.PlayerId;
			zbUtil.Debug2LogFile("MineWar_FightStart_Ack.txt", mineWarFightStartAck);
			zbSelf.Instance.combatHandler.StartShowCombat(mineWarFightStartAck.BattleInfo);
		}
	}

	public void MineWar_FightResult_Req(int PointId, int PlayerId, BattleResultInfo BattleResult)
	{
		MineWarFightResultReq mineWarFightResultReq = new MineWarFightResultReq();
		mineWarFightResultReq.PlayerId = PlayerId;
		mineWarFightResultReq.PointId = PointId;
		mineWarFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)mineWarFightResultReq, Message.MineWarFightResultReq);
	}

	public void MineWar_FightResult_Ack(ByteString response)
	{
		MineWarFightResultAck mineWarFightResultAck = MineWarFightResultAck.Parser.ParseFrom(response);
		if (mineWarFightResultAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mineWarFightResultAck.DataChange);
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarFightResultAck.MineWarBaseInfo, mineWarFightResultAck.MineWarMapInfo);
			zbCombatSystem.Instance.battleAwardList = mineWarFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(mineWarFightResultAck.BattleSettle);
			if (mineWarFightResultAck.BattleSettle.ResultScore > EBattleResultScore.Fail)
			{
				zbSelf.Instance.mineWarHandler.changeInfo.changePointId = zbSelf.Instance.mineWarHandler.currPointId;
				zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId = zbSelf.Instance.mineWarHandler.currPlayerId;
				zbSelf.Instance.mineWarHandler.changeInfo.changeUUID = mineWarFightResultAck.BattleSettle.UUID;
			}
			else
			{
				zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId = 0;
			}
		}
	}

	public void MineWar_OccupyPoint_Req(int PointId, int PlayerId, string UUID)
	{
		MineWarOccupyPointReq mineWarOccupyPointReq = new MineWarOccupyPointReq();
		mineWarOccupyPointReq.PlayerId = PlayerId;
		mineWarOccupyPointReq.PointId = PointId;
		mineWarOccupyPointReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)mineWarOccupyPointReq, Message.MineWarOccupyPointReq);
	}

	public void MineWar_OccupyPoint_Ack(ByteString response)
	{
		MineWarOccupyPointAck mineWarOccupyPointAck = MineWarOccupyPointAck.Parser.ParseFrom(response);
		if (mineWarOccupyPointAck != null)
		{
			zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId = 0;
			zbSelf.Instance.dataHandler.DataChange(mineWarOccupyPointAck.DataChange);
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarOccupyPointAck.MineWarBaseInfo, mineWarOccupyPointAck.MineWarMapInfo);
			if (mineWarOccupyPointAck.AwardList.Count > 0)
			{
				zbUIMgr.Instance.ShowMineWarAwardWnd(((IEnumerable<AwardDetail>)mineWarOccupyPointAck.AwardList).ToArray());
			}
		}
	}

	public void MineWar_BuyCombat_Req(int Count)
	{
		MineWarBuyCombatReq mineWarBuyCombatReq = new MineWarBuyCombatReq();
		mineWarBuyCombatReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)mineWarBuyCombatReq, Message.MineWarBuyCombatReq);
	}

	public void MineWar_BuyCombat_Ack(ByteString response)
	{
		MineWarBuyCombatAck mineWarBuyCombatAck = MineWarBuyCombatAck.Parser.ParseFrom(response);
		if (mineWarBuyCombatAck != null)
		{
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarBuyCombatAck.MineWarBaseInfo, null);
			zbSelf.Instance.dataHandler.DataChange(mineWarBuyCombatAck.DataChange);
		}
	}

	public void MineWar_IdleAward_Req(int PointId)
	{
		MineWarIdleAwardReq mineWarIdleAwardReq = new MineWarIdleAwardReq();
		mineWarIdleAwardReq.PointId = PointId;
		Instance.WsPackMsg((IMessage)(object)mineWarIdleAwardReq, Message.MineWarIdleAwardReq);
	}

	public void MineWar_IdleAward_Ack(ByteString response)
	{
		MineWarIdleAwardAck mineWarIdleAwardAck = MineWarIdleAwardAck.Parser.ParseFrom(response);
		if (mineWarIdleAwardAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mineWarIdleAwardAck.DataChange, ((IEnumerable<AwardDetail>)mineWarIdleAwardAck.AwardList).ToArray());
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarIdleAwardAck.MineWarBaseInfo, mineWarIdleAwardAck.MineWarMapInfo);
		}
	}

	public void MineWar_RecordList_Req()
	{
		MineWarRecordListReq req = new MineWarRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.MineWarRecordListReq);
	}

	public void MineWar_RecordList_Ack(ByteString response)
	{
		MineWarRecordListAck mineWarRecordListAck = MineWarRecordListAck.Parser.ParseFrom(response);
		if (mineWarRecordListAck != null)
		{
			zbSelf.Instance.mineWarHandler.mineWarRecordList = ((IEnumerable<MineWarRecordDetail>)mineWarRecordListAck.RecordList).ToArray();
			zbSelf.Instance.mineWarHandler.mineWarRecordTime = zbSelf.Instance.timeHandler.GetServerTime();
		}
		zbMineWarRecordWnd zbMineWarRecordWnd2 = zbUIMgr.Instance.GetWndById(EWndId.MineWarRecord) as zbMineWarRecordWnd;
		if ((Object)(object)zbMineWarRecordWnd2 == (Object)null || !zbMineWarRecordWnd2.IsShow())
		{
			zbUIMgr.Instance.ShowMineWarRecordWnd();
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MineWarRecordList);
	}

	public void MineWar_RecordSettle_Req()
	{
		MineWarRecordSettleReq req = new MineWarRecordSettleReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.MineWarRecordSettleReq);
	}

	public void MineWar_RecordSettle_Ack(ByteString response)
	{
		MineWarRecordSettleAck mineWarRecordSettleAck = MineWarRecordSettleAck.Parser.ParseFrom(response);
		if (mineWarRecordSettleAck != null)
		{
			zbSelf.Instance.mineWarHandler.mineWarRecordList = ((IEnumerable<MineWarRecordDetail>)mineWarRecordSettleAck.RecordList).ToArray();
			zbSelf.Instance.mineWarHandler.mineWarRecordTime = zbSelf.Instance.timeHandler.GetServerTime();
			zbSelf.Instance.dataHandler.DataChange(mineWarRecordSettleAck.DataChange, ((IEnumerable<AwardDetail>)mineWarRecordSettleAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MineWarRecordList);
		}
	}

	public void MineWar_SaoDang_Req(int PointId)
	{
		MineWarSaoDangReq mineWarSaoDangReq = new MineWarSaoDangReq();
		mineWarSaoDangReq.PointId = PointId;
		Instance.WsPackMsg((IMessage)(object)mineWarSaoDangReq, Message.MineWarSaoDangReq);
	}

	public void MineWar_SaoDang_Ack(ByteString response)
	{
		MineWarSaoDangAck mineWarSaoDangAck = MineWarSaoDangAck.Parser.ParseFrom(response);
		if (mineWarSaoDangAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(mineWarSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)mineWarSaoDangAck.AwardList).ToArray());
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(mineWarSaoDangAck.MineWarBaseInfo, null);
		}
	}

	public void CrossMine_ServerList_Req()
	{
		CrossMineServerListReq req = new CrossMineServerListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossMineServerListReq);
	}

	public void CrossMine_ServerList_Ack(ByteString response)
	{
		CrossMineServerListAck crossMineServerListAck = CrossMineServerListAck.Parser.ParseFrom(response);
		if (crossMineServerListAck != null)
		{
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(crossMineServerListAck.MineWarBaseInfo, null);
			zbSelf.Instance.mineWarHandler.crossMineServerList = ((IEnumerable<CrossMineServerDetail>)crossMineServerListAck.CrossMineServerList).ToArray();
			zbUIMgr.Instance.ShowMineWarServer(crossMineServerListAck);
		}
	}

	public void CrossMine_Data_Req(int ServerId, bool openWnd = false)
	{
		zbSelf.Instance.mineWarHandler.openWnd = openWnd;
		CrossMineDataReq crossMineDataReq = new CrossMineDataReq();
		crossMineDataReq.ServerId = ServerId;
		Instance.WsPackMsg((IMessage)(object)crossMineDataReq, Message.CrossMineDataReq);
	}

	public void CrossMine_Data_Ack(ByteString response)
	{
		CrossMineDataAck crossMineDataAck = CrossMineDataAck.Parser.ParseFrom(response);
		if (crossMineDataAck != null)
		{
			if (crossMineDataAck.ErrorCode != EErrorCode.EmptyValue)
			{
				zbUIMgr.Instance.BubbleErrorCodeId(crossMineDataAck.ErrorCode);
			}
			zbSelf.Instance.mineWarHandler.currServerId = crossMineDataAck.ServerId;
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(crossMineDataAck.MineWarBaseInfo, crossMineDataAck.CrossMineWarMapInfo, isCross: true);
		}
		if (zbSelf.Instance.mineWarHandler.openWnd)
		{
			zbUIMgr.Instance.ShowMineWar();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MineWarBaseInfo);
		}
		zbSelf.Instance.mineWarHandler.openWnd = false;
	}

	public void CrossMine_PointData_Req(int PointId, int ServerId, bool openEnemyWnd = false)
	{
		CrossMinePointDataReq crossMinePointDataReq = new CrossMinePointDataReq();
		crossMinePointDataReq.PointId = PointId;
		crossMinePointDataReq.ServerId = ServerId;
		zbSelf.Instance.mineWarHandler.openEnemyWnd = openEnemyWnd;
		Instance.WsPackMsg((IMessage)(object)crossMinePointDataReq, Message.CrossMinePointDataReq);
	}

	public void CrossMine_PointData_Ack(ByteString response)
	{
		CrossMinePointDataAck crossMinePointDataAck = CrossMinePointDataAck.Parser.ParseFrom(response);
		if (crossMinePointDataAck != null)
		{
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(crossMinePointDataAck.MineWarBaseInfo, null);
			if (zbSelf.Instance.mineWarHandler.openEnemyWnd)
			{
				zbUIMgr.Instance.ShowMineWarEnemy(null, crossMinePointDataAck);
			}
			else
			{
				zbUIMgr.Instance.ShowMineWarDetail(null, crossMinePointDataAck);
			}
		}
	}

	public void CrossMine_DestroyEmptyPoint_Req(int PointId, int ServerId)
	{
		CrossMineDestroyEmptyPointReq crossMineDestroyEmptyPointReq = new CrossMineDestroyEmptyPointReq();
		crossMineDestroyEmptyPointReq.PointId = PointId;
		crossMineDestroyEmptyPointReq.ServerId = ServerId;
		Instance.WsPackMsg((IMessage)(object)crossMineDestroyEmptyPointReq, Message.CrossMineDestroyEmptyPointReq);
	}

	public void CrossMine_DestroyEmptyPoint_Ack(ByteString response)
	{
		CrossMineDestroyEmptyPointAck crossMineDestroyEmptyPointAck = CrossMineDestroyEmptyPointAck.Parser.ParseFrom(response);
		if (crossMineDestroyEmptyPointAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(crossMineDestroyEmptyPointAck.DataChange, ((IEnumerable<AwardDetail>)crossMineDestroyEmptyPointAck.AwardList).ToArray());
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(crossMineDestroyEmptyPointAck.MineWarBaseInfo, crossMineDestroyEmptyPointAck.CrossMineWarMapInfo, isCross: true);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10987"));
		}
	}

	public void CrossMine_FightStart_Req(int PointId, int PlayerId, int ServerId)
	{
		CrossMineFightStartReq crossMineFightStartReq = new CrossMineFightStartReq();
		crossMineFightStartReq.PointId = PointId;
		crossMineFightStartReq.PlayerId = PlayerId;
		crossMineFightStartReq.ServerId = ServerId;
		Instance.WsPackMsg((IMessage)(object)crossMineFightStartReq, Message.CrossMineFightStartReq);
	}

	public void CrossMine_FightStart_Ack(ByteString response)
	{
		CrossMineFightStartAck crossMineFightStartAck = CrossMineFightStartAck.Parser.ParseFrom(response);
		if (crossMineFightStartAck != null)
		{
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(crossMineFightStartAck.MineWarBaseInfo, null);
			zbSelf.Instance.mineWarHandler.currPointId = crossMineFightStartAck.PointId;
			zbSelf.Instance.mineWarHandler.currPlayerId = crossMineFightStartAck.PlayerId;
			zbSelf.Instance.mineWarHandler.currServerId = crossMineFightStartAck.ServerId;
			zbUtil.Debug2LogFile("CrossMine_FightStart_Ack.txt", crossMineFightStartAck);
			zbSelf.Instance.combatHandler.StartShowCombat(crossMineFightStartAck.BattleInfo);
		}
	}

	public void CrossMine_FightResult_Req(int PointId, int PlayerId, int ServerId, BattleResultInfo BattleResult)
	{
		CrossMineFightResultReq crossMineFightResultReq = new CrossMineFightResultReq();
		crossMineFightResultReq.PlayerId = PlayerId;
		crossMineFightResultReq.PointId = PointId;
		crossMineFightResultReq.ServerId = ServerId;
		crossMineFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)crossMineFightResultReq, Message.CrossMineFightResultReq);
	}

	public void CrossMine_FightResult_Ack(ByteString response)
	{
		CrossMineFightResultAck crossMineFightResultAck = CrossMineFightResultAck.Parser.ParseFrom(response);
		if (crossMineFightResultAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(crossMineFightResultAck.DataChange);
			zbSelf.Instance.mineWarHandler.currServerId = crossMineFightResultAck.ServerId;
			zbSelf.Instance.mineWarHandler.UpdateMineWarInfo(crossMineFightResultAck.MineWarBaseInfo, crossMineFightResultAck.CrossMineWarMapInfo, isCross: true);
			zbCombatSystem.Instance.battleAwardList = crossMineFightResultAck.AwardList;
			_ = (zbCombatBalanceWnd)zbCombatSystem.Instance.SetBanlanceData(crossMineFightResultAck.BattleSettle);
			if (crossMineFightResultAck.BattleSettle.ResultScore > EBattleResultScore.Fail)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10987"));
			}
			zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId = 0;
		}
	}

	public void CrossMine_RecordList_Req()
	{
		CrossMineRecordListReq req = new CrossMineRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.CrossMineRecordListReq);
	}

	public void CrossMine_RecordList_Ack(ByteString response)
	{
		CrossMineRecordListAck crossMineRecordListAck = CrossMineRecordListAck.Parser.ParseFrom(response);
		if (crossMineRecordListAck != null)
		{
			zbSelf.Instance.mineWarHandler.crossMineRecordTime = zbSelf.Instance.timeHandler.GetServerTime();
			zbSelf.Instance.mineWarHandler.crossMineRecordList = ((IEnumerable<CrossMineRecordDetail>)crossMineRecordListAck.RecordList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MineWarRecordList);
		}
	}

	public void Operate_Data_Req(bool open = false)
	{
		openOperate = open;
		OperateDataReq req = new OperateDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateDataReq);
	}

	public void Operate_Data_Ack(ByteString response)
	{
		OperateDataAck operateDataAck = OperateDataAck.Parser.ParseFrom(response);
		if (operateDataAck != null)
		{
			zbServerData.sGameData.OperateInfo = operateDataAck.OperateInfo;
			zbSelf.Instance.operateHandler.GetOperateCount();
			zbSelf.Instance.holidayHandler.GetHolidayCount();
			zbSelf.Instance.getWayHandler.InsertOperateGetWay();
			if (openOperate)
			{
				zbUIMgr.Instance.ShowOperate();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
			}
		}
	}

	public void Operate_RechargeDaily_Req(string uuid, int index)
	{
		OperateRechargeDailyReq operateRechargeDailyReq = new OperateRechargeDailyReq();
		operateRechargeDailyReq.UUID = uuid;
		operateRechargeDailyReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateRechargeDailyReq, Message.OperateRechargeDailyReq);
	}

	public void Operate_RechargeDaily_Ack(ByteString response)
	{
		OperateRechargeDailyAck operateRechargeDailyAck = OperateRechargeDailyAck.Parser.ParseFrom(response);
		if (operateRechargeDailyAck != null)
		{
			zbServerData.sGameData.OperateInfo.RechargeDailyList.Clear();
			zbServerData.sGameData.OperateInfo.RechargeDailyList.AddRange((IEnumerable<RechargeDailyDetail>)operateRechargeDailyAck.RechargeDailyList);
			zbSelf.Instance.dataHandler.DataChange(operateRechargeDailyAck.DataChange, ((IEnumerable<AwardDetail>)operateRechargeDailyAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_RechargeDailySum_Req(string uuid, int index)
	{
		OperateRechargeDailySumReq operateRechargeDailySumReq = new OperateRechargeDailySumReq();
		operateRechargeDailySumReq.UUID = uuid;
		operateRechargeDailySumReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateRechargeDailySumReq, Message.OperateRechargeDailySumReq);
	}

	public void Operate_RechargeDailySum_Ack(ByteString response)
	{
		OperateRechargeDailySumAck operateRechargeDailySumAck = OperateRechargeDailySumAck.Parser.ParseFrom(response);
		if (operateRechargeDailySumAck != null)
		{
			zbServerData.sGameData.OperateInfo.RechargeDailySumList.Clear();
			zbServerData.sGameData.OperateInfo.RechargeDailySumList.AddRange((IEnumerable<RechargeSumDetail>)operateRechargeDailySumAck.RechargeDailySumList);
			zbSelf.Instance.dataHandler.DataChange(operateRechargeDailySumAck.DataChange, ((IEnumerable<AwardDetail>)operateRechargeDailySumAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_RechargeSum_Req(string uuid, int index)
	{
		OperateRechargeSumReq operateRechargeSumReq = new OperateRechargeSumReq();
		operateRechargeSumReq.UUID = uuid;
		operateRechargeSumReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateRechargeSumReq, Message.OperateRechargeSumReq);
	}

	public void Operate_RechargeSum_Ack(ByteString response)
	{
		OperateRechargeSumAck operateRechargeSumAck = OperateRechargeSumAck.Parser.ParseFrom(response);
		if (operateRechargeSumAck != null)
		{
			zbServerData.sGameData.OperateInfo.RechargeSumList.Clear();
			zbServerData.sGameData.OperateInfo.RechargeSumList.AddRange((IEnumerable<RechargeSumDetail>)operateRechargeSumAck.RechargeSumList);
			zbSelf.Instance.dataHandler.DataChange(operateRechargeSumAck.DataChange, ((IEnumerable<AwardDetail>)operateRechargeSumAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_RechargeGiftFree_Req()
	{
	}

	public void Operate_RechargeGiftFree_Ack(ByteString response)
	{
	}

	public void Operate_RechargeDirect_Req(string uuid, int directId)
	{
		OperateRechargeDirectReq operateRechargeDirectReq = new OperateRechargeDirectReq();
		operateRechargeDirectReq.UUID = uuid;
		operateRechargeDirectReq.DirectId = directId;
		Instance.WsPackMsg((IMessage)(object)operateRechargeDirectReq, Message.OperateRechargeDirectReq);
	}

	public void Operate_RechargeDirect_Ack(ByteString response)
	{
		OperateRechargeDirectAck operateRechargeDirectAck = OperateRechargeDirectAck.Parser.ParseFrom(response);
		if (operateRechargeDirectAck != null)
		{
			zbServerData.sGameData.OperateInfo.RechargeGiftList.Clear();
			zbServerData.sGameData.OperateInfo.RechargeGiftList.AddRange((IEnumerable<RechargeGiftDetail>)operateRechargeDirectAck.RechargeGiftList);
			zbSelf.Instance.dataHandler.DataChange(operateRechargeDirectAck.DataChange, ((IEnumerable<AwardDetail>)operateRechargeDirectAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ConsumeDailySum_Req(string uuid, int index)
	{
		OperateConsumeDailySumReq operateConsumeDailySumReq = new OperateConsumeDailySumReq();
		operateConsumeDailySumReq.UUID = uuid;
		operateConsumeDailySumReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateConsumeDailySumReq, Message.OperateConsumeDailySumReq);
	}

	public void Operate_ConsumeDailySum_Ack(ByteString response)
	{
		OperateConsumeDailySumAck operateConsumeDailySumAck = OperateConsumeDailySumAck.Parser.ParseFrom(response);
		if (operateConsumeDailySumAck != null)
		{
			zbServerData.sGameData.OperateInfo.ConsumeDailySumList.Clear();
			zbServerData.sGameData.OperateInfo.ConsumeDailySumList.AddRange((IEnumerable<ConsumeSumDetail>)operateConsumeDailySumAck.ConsumeDailySumList);
			zbSelf.Instance.dataHandler.DataChange(operateConsumeDailySumAck.DataChange, ((IEnumerable<AwardDetail>)operateConsumeDailySumAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ConsumeSum_Req(string uuid, int index)
	{
		OperateConsumeSumReq operateConsumeSumReq = new OperateConsumeSumReq();
		operateConsumeSumReq.UUID = uuid;
		operateConsumeSumReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateConsumeSumReq, Message.OperateConsumeSumReq);
	}

	public void Operate_ConsumeSum_Ack(ByteString response)
	{
		OperateConsumeSumAck operateConsumeSumAck = OperateConsumeSumAck.Parser.ParseFrom(response);
		if (operateConsumeSumAck != null)
		{
			zbServerData.sGameData.OperateInfo.ConsumeSumList.Clear();
			zbServerData.sGameData.OperateInfo.ConsumeSumList.AddRange((IEnumerable<ConsumeSumDetail>)operateConsumeSumAck.ConsumeSumList);
			zbSelf.Instance.dataHandler.DataChange(operateConsumeSumAck.DataChange, ((IEnumerable<AwardDetail>)operateConsumeSumAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_LimitTimeHeroDraw_Req(string uuid, int count)
	{
		OperateLimitTimeHeroDrawReq operateLimitTimeHeroDrawReq = new OperateLimitTimeHeroDrawReq();
		operateLimitTimeHeroDrawReq.UUID = uuid;
		operateLimitTimeHeroDrawReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeHeroDrawReq, Message.OperateLimitTimeHeroDrawReq);
	}

	public void Operate_LimitTimeHeroDraw_Ack(ByteString response)
	{
		OperateLimitTimeHeroDrawAck operateLimitTimeHeroDrawAck = OperateLimitTimeHeroDrawAck.Parser.ParseFrom(response);
		if (operateLimitTimeHeroDrawAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_LimitTimeHeroDraw_Ack, operateLimitTimeHeroDrawAck);
		}
	}

	public void Operate_LimitTimeHeroScore_Req(string uuid, int index, int count)
	{
		OperateLimitTimeHeroScoreReq operateLimitTimeHeroScoreReq = new OperateLimitTimeHeroScoreReq();
		operateLimitTimeHeroScoreReq.UUID = uuid;
		operateLimitTimeHeroScoreReq.Index = index;
		operateLimitTimeHeroScoreReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeHeroScoreReq, Message.OperateLimitTimeHeroScoreReq);
	}

	public void Operate_LimitTimeHeroScore_Ack(ByteString response)
	{
		OperateLimitTimeHeroScoreAck operateLimitTimeHeroScoreAck = OperateLimitTimeHeroScoreAck.Parser.ParseFrom(response);
		if (operateLimitTimeHeroScoreAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeHeroList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeHeroList.AddRange((IEnumerable<LimitTimeHeroDetail>)operateLimitTimeHeroScoreAck.LimitTimeHeroList);
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeHeroScoreAck.DataChange, ((IEnumerable<AwardDetail>)operateLimitTimeHeroScoreAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_LimitTimeShop_Req(string uuid, int index, int count)
	{
		OperateLimitTimeShopReq operateLimitTimeShopReq = new OperateLimitTimeShopReq();
		operateLimitTimeShopReq.UUID = uuid;
		operateLimitTimeShopReq.Index = index;
		operateLimitTimeShopReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeShopReq, Message.OperateLimitTimeShopReq);
	}

	public void Operate_LimitTimeShop_Ack(ByteString response)
	{
		OperateLimitTimeShopAck operateLimitTimeShopAck = OperateLimitTimeShopAck.Parser.ParseFrom(response);
		if (operateLimitTimeShopAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeShopList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeShopList.AddRange((IEnumerable<LimitTimeShopDetail>)operateLimitTimeShopAck.LimitTimeShopList);
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeShopAck.DataChange, ((IEnumerable<AwardDetail>)operateLimitTimeShopAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_LoginSum_Req(string uuid, int index)
	{
		OperateLoginSumReq operateLoginSumReq = new OperateLoginSumReq();
		operateLoginSumReq.UUID = uuid;
		operateLoginSumReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateLoginSumReq, Message.OperateLoginSumReq);
	}

	public void Operate_LoginSum_Ack(ByteString response)
	{
		OperateLoginSumAck operateLoginSumAck = OperateLoginSumAck.Parser.ParseFrom(response);
		if (operateLoginSumAck != null)
		{
			zbServerData.sGameData.OperateInfo.LoginSumList.Clear();
			zbServerData.sGameData.OperateInfo.LoginSumList.AddRange((IEnumerable<LoginSumDetail>)operateLoginSumAck.LoginSumList);
			zbSelf.Instance.dataHandler.DataChange(operateLoginSumAck.DataChange, ((IEnumerable<AwardDetail>)operateLoginSumAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_DailyActive_Req(string uuid)
	{
		OperateDailyActiveReq operateDailyActiveReq = new OperateDailyActiveReq();
		operateDailyActiveReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateDailyActiveReq, Message.OperateDailyActiveReq);
	}

	public void Operate_DailyActive_Ack(ByteString response)
	{
		OperateDailyActiveAck operateDailyActiveAck = OperateDailyActiveAck.Parser.ParseFrom(response);
		if (operateDailyActiveAck != null)
		{
			zbServerData.sGameData.OperateInfo.DailyActiveList.Clear();
			zbServerData.sGameData.OperateInfo.DailyActiveList.AddRange((IEnumerable<DailyActiveDetail>)operateDailyActiveAck.DailyActiveList);
			zbSelf.Instance.dataHandler.DataChange(operateDailyActiveAck.DataChange, ((IEnumerable<AwardDetail>)operateDailyActiveAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_BarDraw_Req(string uuid, int index)
	{
		OperateBarDrawReq operateBarDrawReq = new OperateBarDrawReq();
		operateBarDrawReq.UUID = uuid;
		operateBarDrawReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateBarDrawReq, Message.OperateBarDrawReq);
	}

	public void Operate_BarDraw_Ack(ByteString response)
	{
		OperateBarDrawAck operateBarDrawAck = OperateBarDrawAck.Parser.ParseFrom(response);
		if (operateBarDrawAck != null)
		{
			zbServerData.sGameData.OperateInfo.BarDrawList.Clear();
			zbServerData.sGameData.OperateInfo.BarDrawList.AddRange((IEnumerable<BarDrawDetail>)operateBarDrawAck.BarDrawList);
			zbSelf.Instance.dataHandler.DataChange(operateBarDrawAck.DataChange, ((IEnumerable<AwardDetail>)operateBarDrawAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_PropExchange_Req(string uuid, int index, int count)
	{
		OperatePropExchangeReq operatePropExchangeReq = new OperatePropExchangeReq();
		operatePropExchangeReq.UUID = uuid;
		operatePropExchangeReq.Index = index;
		operatePropExchangeReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operatePropExchangeReq, Message.OperatePropExchangeReq);
	}

	public void Operate_PropExchange_Ack(ByteString response)
	{
		OperatePropExchangeAck operatePropExchangeAck = OperatePropExchangeAck.Parser.ParseFrom(response);
		if (operatePropExchangeAck != null)
		{
			zbServerData.sGameData.OperateInfo.PropExchangeList.Clear();
			zbServerData.sGameData.OperateInfo.PropExchangeList.AddRange((IEnumerable<PropExchangeDetail>)operatePropExchangeAck.PropExchangeList);
			zbSelf.Instance.dataHandler.DataChange(operatePropExchangeAck.DataChange, ((IEnumerable<AwardDetail>)operatePropExchangeAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_PropExchangeTarget_Req(string uuid, int index)
	{
		OperatePropExchangeTargetReq operatePropExchangeTargetReq = new OperatePropExchangeTargetReq();
		operatePropExchangeTargetReq.UUID = uuid;
		operatePropExchangeTargetReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operatePropExchangeTargetReq, Message.OperatePropExchangeTargetReq);
	}

	public void Operate_PropExchangeTarget_Ack(ByteString response)
	{
		OperatePropExchangeTargetAck operatePropExchangeTargetAck = OperatePropExchangeTargetAck.Parser.ParseFrom(response);
		if (operatePropExchangeTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.PropExchangeList.Clear();
			zbServerData.sGameData.OperateInfo.PropExchangeList.AddRange((IEnumerable<PropExchangeDetail>)operatePropExchangeTargetAck.PropExchangeList);
			zbSelf.Instance.dataHandler.DataChange(operatePropExchangeTargetAck.DataChange, ((IEnumerable<AwardDetail>)operatePropExchangeTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ScratchCard_Req(string uuid, int index)
	{
		OperateScratchCardReq operateScratchCardReq = new OperateScratchCardReq();
		operateScratchCardReq.UUID = uuid;
		operateScratchCardReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateScratchCardReq, Message.OperateScratchCardReq);
	}

	public void Operate_ScratchCard_Ack(ByteString response)
	{
		OperateScratchCardAck operateScratchCardAck = OperateScratchCardAck.Parser.ParseFrom(response);
		if (operateScratchCardAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateScratchCardAck.DataChange);
			zbServerData.sGameData.OperateInfo.ScratchCardList.Clear();
			zbServerData.sGameData.OperateInfo.ScratchCardList.AddRange((IEnumerable<ScratchCardDetail>)operateScratchCardAck.ScratchCardList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ScratchCardTarget_Req(string uuid, int index)
	{
		OperateScratchCardTargetReq operateScratchCardTargetReq = new OperateScratchCardTargetReq();
		operateScratchCardTargetReq.UUID = uuid;
		operateScratchCardTargetReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateScratchCardTargetReq, Message.OperateScratchCardTargetReq);
	}

	public void Operate_ScratchCardTarget_Ack(ByteString response)
	{
		OperateScratchCardTargetAck operateScratchCardTargetAck = OperateScratchCardTargetAck.Parser.ParseFrom(response);
		if (operateScratchCardTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.ScratchCardList.Clear();
			zbServerData.sGameData.OperateInfo.ScratchCardList.AddRange((IEnumerable<ScratchCardDetail>)operateScratchCardTargetAck.ScratchCardList);
			zbSelf.Instance.dataHandler.DataChange(operateScratchCardTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateScratchCardTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ScratchCardReset_Req(string uuid)
	{
		OperateScratchCardResetReq operateScratchCardResetReq = new OperateScratchCardResetReq();
		operateScratchCardResetReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateScratchCardResetReq, Message.OperateScratchCardResetReq);
	}

	public void Operate_ScratchCardReset_Ack(ByteString response)
	{
		OperateScratchCardResetAck operateScratchCardResetAck = OperateScratchCardResetAck.Parser.ParseFrom(response);
		if (operateScratchCardResetAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateScratchCardResetAck.DataChange);
			zbServerData.sGameData.OperateInfo.ScratchCardList.Clear();
			zbServerData.sGameData.OperateInfo.ScratchCardList.AddRange((IEnumerable<ScratchCardDetail>)operateScratchCardResetAck.ScratchCardList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_JiZi_Req(string uuid, int index, int count)
	{
		OperateJiZiReq operateJiZiReq = new OperateJiZiReq();
		operateJiZiReq.UUID = uuid;
		operateJiZiReq.Index = index;
		operateJiZiReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateJiZiReq, Message.OperateJiZiReq);
	}

	public void Operate_JiZi_Ack(ByteString response)
	{
		OperateJiZiAck operateJiZiAck = OperateJiZiAck.Parser.ParseFrom(response);
		if (operateJiZiAck != null)
		{
			zbServerData.sGameData.OperateInfo.JiZiList.Clear();
			zbServerData.sGameData.OperateInfo.JiZiList.AddRange((IEnumerable<JiZiDetail>)operateJiZiAck.JiZiList);
			zbSelf.Instance.dataHandler.DataChange(operateJiZiAck.DataChange, ((IEnumerable<AwardDetail>)operateJiZiAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_CipherBox_Req(string uuid, string cipher)
	{
		OperateCipherBoxReq operateCipherBoxReq = new OperateCipherBoxReq();
		operateCipherBoxReq.UUID = uuid;
		operateCipherBoxReq.Cipher = cipher;
		Instance.WsPackMsg((IMessage)(object)operateCipherBoxReq, Message.OperateCipherBoxReq);
	}

	public void Operate_CipherBox_Ack(ByteString response)
	{
		OperateCipherBoxAck operateCipherBoxAck = OperateCipherBoxAck.Parser.ParseFrom(response);
		if (operateCipherBoxAck != null)
		{
			zbServerData.sGameData.OperateInfo.CipherBoxList.Clear();
			zbServerData.sGameData.OperateInfo.CipherBoxList.AddRange((IEnumerable<CipherBoxDetail>)operateCipherBoxAck.CipherBoxList);
			zbSelf.Instance.dataHandler.DataChange(operateCipherBoxAck.DataChange, ((IEnumerable<AwardDetail>)operateCipherBoxAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_BarrageCmd_Req(string uuid, ELanguageType lang)
	{
		OperateBarrageCmdReq operateBarrageCmdReq = new OperateBarrageCmdReq();
		operateBarrageCmdReq.UUID = uuid;
		operateBarrageCmdReq.Lang = lang;
		Instance.WsPackMsg((IMessage)(object)operateBarrageCmdReq, Message.OperateBarrageCmdReq);
	}

	public void Operate_BarrageCmd_Ack(ByteString response)
	{
		OperateBarrageCmdAck operateBarrageCmdAck = OperateBarrageCmdAck.Parser.ParseFrom(response);
		if (operateBarrageCmdAck != null)
		{
			zbServerData.sGameData.OperateInfo.BarrageCmdList.Clear();
			zbServerData.sGameData.OperateInfo.BarrageCmdList.AddRange((IEnumerable<BarrageCmdDetail>)operateBarrageCmdAck.BarrageCmdList);
			zbSelf.Instance.dataHandler.DataChange(operateBarrageCmdAck.DataChange, ((IEnumerable<AwardDetail>)operateBarrageCmdAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_LimitTimeCatcherCoin_Req(string uuid, int count)
	{
		OperateLimitTimeCatcherCoinReq operateLimitTimeCatcherCoinReq = new OperateLimitTimeCatcherCoinReq();
		operateLimitTimeCatcherCoinReq.UUID = uuid;
		operateLimitTimeCatcherCoinReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeCatcherCoinReq, Message.OperateLimitTimeCatcherCoinReq);
	}

	public void Operate_LimitTimeCatcherCoin_Ack(ByteString response)
	{
		OperateLimitTimeCatcherCoinAck operateLimitTimeCatcherCoinAck = OperateLimitTimeCatcherCoinAck.Parser.ParseFrom(response);
		if (operateLimitTimeCatcherCoinAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeCatcherCoinAck.DataChange);
			zbServerData.sGameData.OperateInfo.LimitTimeCatcherList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeCatcherList.AddRange((IEnumerable<LimitTimeCatcherDetail>)operateLimitTimeCatcherCoinAck.LimitTimeCatcherList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_GmLimitTimeCatcher_CoinAck);
		}
	}

	public void Operate_LimitTimeCatcherGrab_Req(string uuid, int heroTid)
	{
		OperateLimitTimeCatcherGrabReq operateLimitTimeCatcherGrabReq = new OperateLimitTimeCatcherGrabReq();
		operateLimitTimeCatcherGrabReq.UUID = uuid;
		operateLimitTimeCatcherGrabReq.HeroTid = heroTid;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeCatcherGrabReq, Message.OperateLimitTimeCatcherGrabReq);
	}

	public void Operate_LimitTimeCatcherGrab_Ack(ByteString response)
	{
		OperateLimitTimeCatcherGrabAck operateLimitTimeCatcherGrabAck = OperateLimitTimeCatcherGrabAck.Parser.ParseFrom(response);
		if (operateLimitTimeCatcherGrabAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeCatcherList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeCatcherList.AddRange((IEnumerable<LimitTimeCatcherDetail>)operateLimitTimeCatcherGrabAck.LimitTimeCatcherList);
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeCatcherGrabAck.DataChange, ((IEnumerable<AwardDetail>)operateLimitTimeCatcherGrabAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_GmLimitTimeCatcher_GrabAck);
		}
	}

	public void Operate_LimitTimeCatcherAward_Req(string uuid)
	{
		OperateLimitTimeCatcherAwardReq operateLimitTimeCatcherAwardReq = new OperateLimitTimeCatcherAwardReq();
		operateLimitTimeCatcherAwardReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeCatcherAwardReq, Message.OperateLimitTimeCatcherAwardReq);
	}

	public void Operate_LimitTimeCatcherAward_Ack(ByteString response)
	{
		OperateLimitTimeCatcherAwardAck operateLimitTimeCatcherAwardAck = OperateLimitTimeCatcherAwardAck.Parser.ParseFrom(response);
		if (operateLimitTimeCatcherAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeCatcherList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeCatcherList.AddRange((IEnumerable<LimitTimeCatcherDetail>)operateLimitTimeCatcherAwardAck.LimitTimeCatcherList);
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeCatcherAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateLimitTimeCatcherAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_HolidayTaskData_Req(bool openWnd = false)
	{
		openHolidayTaskWnd = openWnd;
		OperateHolidayTaskDataReq req = new OperateHolidayTaskDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateHolidayTaskDataReq);
	}

	public void Operate_HolidayTaskData_Ack(ByteString response)
	{
		OperateHolidayTaskDataAck operateHolidayTaskDataAck = OperateHolidayTaskDataAck.Parser.ParseFrom(response);
		if (operateHolidayTaskDataAck != null)
		{
			zbServerData.sGameData.OperateInfo.HolidayTaskList.Clear();
			zbServerData.sGameData.OperateInfo.HolidayTaskList.AddRange((IEnumerable<HolidayTaskDetail>)operateHolidayTaskDataAck.HolidayTaskList);
			if (openHolidayTaskWnd)
			{
				zbUIMgr.Instance.ShowHolidayTask();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HolidayTask_Ack);
			}
		}
	}

	public void Operate_HolidayTaskTaskAward_Req(string uuid, int taskId, int exp)
	{
		OperateHolidayTaskTaskAwardReq operateHolidayTaskTaskAwardReq = new OperateHolidayTaskTaskAwardReq();
		operateHolidayTaskTaskAwardReq.UUID = uuid;
		operateHolidayTaskTaskAwardReq.TaskId = taskId;
		tempExp = exp;
		Instance.WsPackMsg((IMessage)(object)operateHolidayTaskTaskAwardReq, Message.OperateHolidayTaskTaskAwardReq);
	}

	public void Operate_HolidayTaskTaskAward_Ack(ByteString response)
	{
		OperateHolidayTaskTaskAwardAck operateHolidayTaskTaskAwardAck = OperateHolidayTaskTaskAwardAck.Parser.ParseFrom(response);
		if (operateHolidayTaskTaskAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.HolidayTaskList.Clear();
			zbServerData.sGameData.OperateInfo.HolidayTaskList.AddRange((IEnumerable<HolidayTaskDetail>)operateHolidayTaskTaskAwardAck.HolidayTaskList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HolidayTask_Ack);
			if (tempExp > 0)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11029"), tempExp));
				tempExp = 0;
			}
		}
	}

	public void Operate_HolidayTaskLevelAward_Req(string uuid, int level)
	{
		OperateHolidayTaskLevelAwardReq operateHolidayTaskLevelAwardReq = new OperateHolidayTaskLevelAwardReq();
		operateHolidayTaskLevelAwardReq.UUID = uuid;
		operateHolidayTaskLevelAwardReq.Level = level;
		Instance.WsPackMsg((IMessage)(object)operateHolidayTaskLevelAwardReq, Message.OperateHolidayTaskLevelAwardReq);
	}

	public void Operate_HolidayTaskLevelAward_Ack(ByteString response)
	{
		OperateHolidayTaskLevelAwardAck operateHolidayTaskLevelAwardAck = OperateHolidayTaskLevelAwardAck.Parser.ParseFrom(response);
		if (operateHolidayTaskLevelAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.HolidayTaskList.Clear();
			zbServerData.sGameData.OperateInfo.HolidayTaskList.AddRange((IEnumerable<HolidayTaskDetail>)operateHolidayTaskLevelAwardAck.HolidayTaskList);
			zbSelf.Instance.dataHandler.DataChange(operateHolidayTaskLevelAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateHolidayTaskLevelAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HolidayTask_Ack);
		}
	}

	public void Operate_HolidayTaskScoreAward_Req(string uuid, int index, int count)
	{
		OperateHolidayTaskScoreAwardReq operateHolidayTaskScoreAwardReq = new OperateHolidayTaskScoreAwardReq();
		operateHolidayTaskScoreAwardReq.UUID = uuid;
		operateHolidayTaskScoreAwardReq.Index = index;
		operateHolidayTaskScoreAwardReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateHolidayTaskScoreAwardReq, Message.OperateHolidayTaskScoreAwardReq);
	}

	public void Operate_HolidayTaskScoreAward_Ack(ByteString response)
	{
		OperateHolidayTaskScoreAwardAck operateHolidayTaskScoreAwardAck = OperateHolidayTaskScoreAwardAck.Parser.ParseFrom(response);
		if (operateHolidayTaskScoreAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.HolidayTaskList.Clear();
			zbServerData.sGameData.OperateInfo.HolidayTaskList.AddRange((IEnumerable<HolidayTaskDetail>)operateHolidayTaskScoreAwardAck.HolidayTaskList);
			zbSelf.Instance.dataHandler.DataChange(operateHolidayTaskScoreAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateHolidayTaskScoreAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HolidayTask_Ack);
		}
	}

	public void Operate_HolidayTaskBuyLevel_Req(string uuid, int count)
	{
		OperateHolidayTaskBuyLevelReq operateHolidayTaskBuyLevelReq = new OperateHolidayTaskBuyLevelReq();
		operateHolidayTaskBuyLevelReq.UUID = uuid;
		operateHolidayTaskBuyLevelReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateHolidayTaskBuyLevelReq, Message.OperateHolidayTaskBuyLevelReq);
	}

	public void Operate_HolidayTaskBuyLevel_Ack(ByteString response)
	{
		OperateHolidayTaskBuyLevelAck operateHolidayTaskBuyLevelAck = OperateHolidayTaskBuyLevelAck.Parser.ParseFrom(response);
		if (operateHolidayTaskBuyLevelAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateHolidayTaskBuyLevelAck.DataChange);
			zbServerData.sGameData.OperateInfo.HolidayTaskList.Clear();
			zbServerData.sGameData.OperateInfo.HolidayTaskList.AddRange((IEnumerable<HolidayTaskDetail>)operateHolidayTaskBuyLevelAck.HolidayTaskList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_HolidayTask_Ack);
		}
	}

	public void Operate_PrayLanternDraw_Req(string uuid, int count)
	{
		OperatePrayLanternDrawReq operatePrayLanternDrawReq = new OperatePrayLanternDrawReq();
		operatePrayLanternDrawReq.UUID = uuid;
		operatePrayLanternDrawReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operatePrayLanternDrawReq, Message.OperatePrayLanternDrawReq);
	}

	public void Operate_PrayLanternDraw_Ack(ByteString response)
	{
		OperatePrayLanternDrawAck operatePrayLanternDrawAck = OperatePrayLanternDrawAck.Parser.ParseFrom(response);
		if (operatePrayLanternDrawAck != null)
		{
			zbSelf.Instance.operateHandler.prayLanternDrawAckData = operatePrayLanternDrawAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_PrayLanternDraw_Ack);
		}
	}

	public void Operate_PrayLanternTarget_Req(string uuid, int index)
	{
		OperatePrayLanternTargetReq operatePrayLanternTargetReq = new OperatePrayLanternTargetReq();
		operatePrayLanternTargetReq.UUID = uuid;
		operatePrayLanternTargetReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operatePrayLanternTargetReq, Message.OperatePrayLanternTargetReq);
	}

	public void Operate_PrayLanternTarget_Ack(ByteString response)
	{
		OperatePrayLanternTargetAck operatePrayLanternTargetAck = OperatePrayLanternTargetAck.Parser.ParseFrom(response);
		if (operatePrayLanternTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.PrayLanternList.Clear();
			zbServerData.sGameData.OperateInfo.PrayLanternList.AddRange((IEnumerable<PrayLanternDetail>)operatePrayLanternTargetAck.PrayLanternList);
			zbSelf.Instance.dataHandler.DataChange(operatePrayLanternTargetAck.DataChange, ((IEnumerable<AwardDetail>)operatePrayLanternTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_PrayLanternSelect_Req(string uuid, int index)
	{
		OperatePrayLanternSelectReq operatePrayLanternSelectReq = new OperatePrayLanternSelectReq();
		operatePrayLanternSelectReq.UUID = uuid;
		operatePrayLanternSelectReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operatePrayLanternSelectReq, Message.OperatePrayLanternSelectReq);
	}

	public void Operate_PrayLanternSelect_Ack(ByteString response)
	{
		OperatePrayLanternSelectAck operatePrayLanternSelectAck = OperatePrayLanternSelectAck.Parser.ParseFrom(response);
		if (operatePrayLanternSelectAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operatePrayLanternSelectAck.DataChange);
			zbServerData.sGameData.OperateInfo.PrayLanternList.Clear();
			zbServerData.sGameData.OperateInfo.PrayLanternList.AddRange((IEnumerable<PrayLanternDetail>)operatePrayLanternSelectAck.PrayLanternList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_PrayLanternSelect_Ack);
		}
	}

	public void Operate_PrayLanternUltimate_Req(string uuid)
	{
		OperatePrayLanternUltimateReq operatePrayLanternUltimateReq = new OperatePrayLanternUltimateReq();
		operatePrayLanternUltimateReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operatePrayLanternUltimateReq, Message.OperatePrayLanternUltimateReq);
	}

	public void Operate_PrayLanternUltimate_Ack(ByteString response)
	{
		OperatePrayLanternUltimateAck operatePrayLanternUltimateAck = OperatePrayLanternUltimateAck.Parser.ParseFrom(response);
		if (operatePrayLanternUltimateAck != null)
		{
			zbServerData.sGameData.OperateInfo.PrayLanternList.Clear();
			zbServerData.sGameData.OperateInfo.PrayLanternList.AddRange((IEnumerable<PrayLanternDetail>)operatePrayLanternUltimateAck.PrayLanternList);
			zbSelf.Instance.dataHandler.DataChange(operatePrayLanternUltimateAck.DataChange, ((IEnumerable<AwardDetail>)operatePrayLanternUltimateAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ResourceExchange_Req(string uuid, int index, int count)
	{
		OperateResourceExchangeReq operateResourceExchangeReq = new OperateResourceExchangeReq();
		operateResourceExchangeReq.UUID = uuid;
		operateResourceExchangeReq.Index = index;
		operateResourceExchangeReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateResourceExchangeReq, Message.OperateResourceExchangeReq);
	}

	public void Operate_ResourceExchange_Ack(ByteString response)
	{
		OperateResourceExchangeAck operateResourceExchangeAck = OperateResourceExchangeAck.Parser.ParseFrom(response);
		if (operateResourceExchangeAck != null)
		{
			zbServerData.sGameData.OperateInfo.ResourceExchangeList.Clear();
			zbServerData.sGameData.OperateInfo.ResourceExchangeList.AddRange((IEnumerable<ResourceExchangeDetail>)operateResourceExchangeAck.ResourceExchangeList);
			zbSelf.Instance.dataHandler.DataChange(operateResourceExchangeAck.DataChange, ((IEnumerable<AwardDetail>)operateResourceExchangeAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_FusionData_Req()
	{
		OperateFusionDataReq req = new OperateFusionDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateFusionDataReq);
	}

	public List<List<int>> ParseFusionData(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return new List<List<int>>();
		}
		return JsonConvert.DeserializeObject<List<List<int>>>(data);
	}

	public void Operate_FusionData_Ack(ByteString response)
	{
		OperateFusionDataAck operateFusionDataAck = OperateFusionDataAck.Parser.ParseFrom(response);
		if (operateFusionDataAck != null)
		{
			zbServerData.sGameData.OperateInfo.FusionInfo = operateFusionDataAck.FusionInfo;
			zbSelf.Instance.operateHandler.fusionData = ParseFusionData(operateFusionDataAck.Data);
			zbUIMgr.Instance.ShowFusionEnter();
		}
	}

	public void Operate_FusionUpdate_Req()
	{
		OperateFusionUpdateReq operateFusionUpdateReq = new OperateFusionUpdateReq();
		operateFusionUpdateReq.BestScore = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.BestScore;
		operateFusionUpdateReq.Score = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.Score;
		operateFusionUpdateReq.Clean = zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.Clean;
		operateFusionUpdateReq.Data = JsonConvert.SerializeObject((object)zbSelf.Instance.operateHandler.fusionData);
		Instance.WsPackMsg((IMessage)(object)operateFusionUpdateReq, Message.OperateFusionUpdateReq, needTip: false);
	}

	public void Operate_FusionUpdate_Ack(ByteString response)
	{
		OperateFusionUpdateAck operateFusionUpdateAck = OperateFusionUpdateAck.Parser.ParseFrom(response);
		if (operateFusionUpdateAck != null)
		{
			zbServerData.sGameData.OperateInfo.FusionInfo = operateFusionUpdateAck.FusionInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_FusionDailyAward_Req(int target)
	{
		OperateFusionDailyAwardReq operateFusionDailyAwardReq = new OperateFusionDailyAwardReq();
		operateFusionDailyAwardReq.Target = target;
		Instance.WsPackMsg((IMessage)(object)operateFusionDailyAwardReq, Message.OperateFusionDailyAwardReq);
	}

	public void Operate_FusionDailyAward_Ack(ByteString response)
	{
		OperateFusionDailyAwardAck operateFusionDailyAwardAck = OperateFusionDailyAwardAck.Parser.ParseFrom(response);
		if (operateFusionDailyAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.FusionInfo = operateFusionDailyAwardAck.FusionInfo;
			zbSelf.Instance.dataHandler.DataChange(operateFusionDailyAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateFusionDailyAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_FusionAwardList);
		}
	}

	public void Operate_FusionTargetAward_Req(int target)
	{
		OperateFusionTargetAwardReq operateFusionTargetAwardReq = new OperateFusionTargetAwardReq();
		operateFusionTargetAwardReq.Target = target;
		Instance.WsPackMsg((IMessage)(object)operateFusionTargetAwardReq, Message.OperateFusionTargetAwardReq);
	}

	public void Operate_FusionTargetAward_Ack(ByteString response)
	{
		OperateFusionTargetAwardAck operateFusionTargetAwardAck = OperateFusionTargetAwardAck.Parser.ParseFrom(response);
		if (operateFusionTargetAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.FusionInfo = operateFusionTargetAwardAck.FusionInfo;
			zbSelf.Instance.dataHandler.DataChange(operateFusionTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateFusionTargetAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_FusionAwardList);
		}
	}

	public void Operate_FusionFriendsRank_Req()
	{
		OperateFusionFriendsRankReq req = new OperateFusionFriendsRankReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateFusionFriendsRankReq);
	}

	public void Operate_FusionFriendsRank_Ack(ByteString response)
	{
		OperateFusionFriendsRankAck operateFusionFriendsRankAck = OperateFusionFriendsRankAck.Parser.ParseFrom(response);
		if (operateFusionFriendsRankAck != null)
		{
			zbUIMgr.Instance.ShowFusionRank(((IEnumerable<RankDetail>)operateFusionFriendsRankAck.RankList).ToArray());
		}
	}

	public void Operate_InfiniteGiftAward_Req(string uuid, int index)
	{
		OperateInfiniteGiftAwardReq operateInfiniteGiftAwardReq = new OperateInfiniteGiftAwardReq();
		operateInfiniteGiftAwardReq.UUID = uuid;
		operateInfiniteGiftAwardReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateInfiniteGiftAwardReq, Message.OperateInfiniteGiftAwardReq);
	}

	public void Operate_InfiniteGiftAward_Ack(ByteString response)
	{
		OperateInfiniteGiftAwardAck operateInfiniteGiftAwardAck = OperateInfiniteGiftAwardAck.Parser.ParseFrom(response);
		if (operateInfiniteGiftAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.InfiniteGiftList.Clear();
			zbServerData.sGameData.OperateInfo.InfiniteGiftList.AddRange((IEnumerable<InfiniteGiftDetail>)operateInfiniteGiftAwardAck.InfiniteGiftList);
			zbSelf.Instance.dataHandler.DataChange(operateInfiniteGiftAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateInfiniteGiftAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_InfiniteGiftTarget_Req(string uuid, int index)
	{
		OperateInfiniteGiftTargetReq operateInfiniteGiftTargetReq = new OperateInfiniteGiftTargetReq();
		operateInfiniteGiftTargetReq.UUID = uuid;
		operateInfiniteGiftTargetReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateInfiniteGiftTargetReq, Message.OperateInfiniteGiftTargetReq);
	}

	public void Operate_InfiniteGiftTarget_Ack(ByteString response)
	{
		OperateInfiniteGiftTargetAck operateInfiniteGiftTargetAck = OperateInfiniteGiftTargetAck.Parser.ParseFrom(response);
		if (operateInfiniteGiftTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.InfiniteGiftList.Clear();
			zbServerData.sGameData.OperateInfo.InfiniteGiftList.AddRange((IEnumerable<InfiniteGiftDetail>)operateInfiniteGiftTargetAck.InfiniteGiftList);
			zbSelf.Instance.dataHandler.DataChange(operateInfiniteGiftTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateInfiniteGiftTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_InfiniteGiftTarget_Ack);
		}
	}

	public void Operate_InfiniteGiftFree_Req(string uuid)
	{
		OperateInfiniteGiftFreeReq operateInfiniteGiftFreeReq = new OperateInfiniteGiftFreeReq();
		operateInfiniteGiftFreeReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateInfiniteGiftFreeReq, Message.OperateInfiniteGiftFreeReq);
	}

	public void Operate_InfiniteGiftFree_Ack(ByteString response)
	{
		OperateInfiniteGiftFreeAck operateInfiniteGiftFreeAck = OperateInfiniteGiftFreeAck.Parser.ParseFrom(response);
		if (operateInfiniteGiftFreeAck != null)
		{
			zbServerData.sGameData.OperateInfo.InfiniteGiftList.Clear();
			zbServerData.sGameData.OperateInfo.InfiniteGiftList.AddRange((IEnumerable<InfiniteGiftDetail>)operateInfiniteGiftFreeAck.InfiniteGiftList);
			zbSelf.Instance.dataHandler.DataChange(operateInfiniteGiftFreeAck.DataChange, ((IEnumerable<AwardDetail>)operateInfiniteGiftFreeAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_InfiniteGiftFree_Ack);
		}
	}

	public void Operate_LimitTimeRouletteDraw_Req(string uuid, int count)
	{
		OperateLimitTimeRouletteDrawReq operateLimitTimeRouletteDrawReq = new OperateLimitTimeRouletteDrawReq();
		operateLimitTimeRouletteDrawReq.UUID = uuid;
		operateLimitTimeRouletteDrawReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeRouletteDrawReq, Message.OperateLimitTimeRouletteDrawReq);
	}

	public void Operate_LimitTimeRouletteDraw_Ack(ByteString response)
	{
		OperateLimitTimeRouletteDrawAck operateLimitTimeRouletteDrawAck = OperateLimitTimeRouletteDrawAck.Parser.ParseFrom(response);
		if (operateLimitTimeRouletteDrawAck != null)
		{
			zbSelf.Instance.operateHandler.timeRouletteAckData = operateLimitTimeRouletteDrawAck;
			zbServerData.sGameData.OperateInfo.LimitTimeRouletteList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeRouletteList.AddRange((IEnumerable<LimitTimeRouletteDetail>)operateLimitTimeRouletteDrawAck.LimitTimeRouletteList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TimeRoulette_Ack, operateLimitTimeRouletteDrawAck);
		}
	}

	public void Operate_LimitTimeRouletteTarget_Req(string uuid, int index)
	{
		OperateLimitTimeRouletteTargetReq operateLimitTimeRouletteTargetReq = new OperateLimitTimeRouletteTargetReq();
		operateLimitTimeRouletteTargetReq.UUID = uuid;
		operateLimitTimeRouletteTargetReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeRouletteTargetReq, Message.OperateLimitTimeRouletteTargetReq);
	}

	public void Operate_LimitTimeRouletteTarget_Ack(ByteString response)
	{
		OperateLimitTimeRouletteTargetAck operateLimitTimeRouletteTargetAck = OperateLimitTimeRouletteTargetAck.Parser.ParseFrom(response);
		if (operateLimitTimeRouletteTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeRouletteList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeRouletteList.AddRange((IEnumerable<LimitTimeRouletteDetail>)operateLimitTimeRouletteTargetAck.LimitTimeRouletteList);
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeRouletteTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateLimitTimeRouletteTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_LimitTimeTreasureDraw_Req(string uuid, int index)
	{
		OperateLimitTimeTreasureDrawReq operateLimitTimeTreasureDrawReq = new OperateLimitTimeTreasureDrawReq();
		operateLimitTimeTreasureDrawReq.UUID = uuid;
		operateLimitTimeTreasureDrawReq.Count = index;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeTreasureDrawReq, Message.OperateLimitTimeTreasureDrawReq);
	}

	public void Operate_LimitTimeTreasureDraw_Ack(ByteString response)
	{
		OperateLimitTimeTreasureDrawAck operateLimitTimeTreasureDrawAck = OperateLimitTimeTreasureDrawAck.Parser.ParseFrom(response);
		if (operateLimitTimeTreasureDrawAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeTreasureList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeTreasureList.AddRange((IEnumerable<LimitTimeRouletteDetail>)operateLimitTimeTreasureDrawAck.LimitTimeTreasureList);
			zbSelf.Instance.operateHandler.timeTreasureDrawAckData = operateLimitTimeTreasureDrawAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_LimitTimeTreasure_Ack);
		}
	}

	public void Operate_LimitTimeTreasureTarget_Req(string uuid, int index)
	{
		OperateLimitTimeTreasureTargetReq operateLimitTimeTreasureTargetReq = new OperateLimitTimeTreasureTargetReq();
		operateLimitTimeTreasureTargetReq.UUID = uuid;
		operateLimitTimeTreasureTargetReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeTreasureTargetReq, Message.OperateLimitTimeTreasureTargetReq);
	}

	public void Operate_LimitTimeTreasureTarget_Ack(ByteString response)
	{
		OperateLimitTimeTreasureTargetAck operateLimitTimeTreasureTargetAck = OperateLimitTimeTreasureTargetAck.Parser.ParseFrom(response);
		if (operateLimitTimeTreasureTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeTreasureList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeTreasureList.AddRange((IEnumerable<LimitTimeRouletteDetail>)operateLimitTimeTreasureTargetAck.LimitTimeTreasureList);
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeTreasureTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateLimitTimeTreasureTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_LimitTimeTreasure_Ack);
		}
	}

	public void Operate_LimitTimeVendorDraw_Req(string uuid, int index)
	{
		OperateLimitTimeVendorDrawReq operateLimitTimeVendorDrawReq = new OperateLimitTimeVendorDrawReq();
		operateLimitTimeVendorDrawReq.UUID = uuid;
		operateLimitTimeVendorDrawReq.Count = index;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeVendorDrawReq, Message.OperateLimitTimeVendorDrawReq);
	}

	public void Operate_LimitTimeVendorDraw_Ack(ByteString response)
	{
		OperateLimitTimeVendorDrawAck operateLimitTimeVendorDrawAck = OperateLimitTimeVendorDrawAck.Parser.ParseFrom(response);
		if (operateLimitTimeVendorDrawAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeVendorList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeVendorList.AddRange((IEnumerable<LimitTimeRouletteDetail>)operateLimitTimeVendorDrawAck.LimitTimeVendorList);
			zbSelf.Instance.operateHandler.timeVendorDrawData = operateLimitTimeVendorDrawAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_LimitTimeVendor_Ack);
		}
	}

	public void Operate_LimitTimeVendorTarget_Req(string uuid, int index)
	{
		OperateLimitTimeVendorTargetReq operateLimitTimeVendorTargetReq = new OperateLimitTimeVendorTargetReq();
		operateLimitTimeVendorTargetReq.UUID = uuid;
		operateLimitTimeVendorTargetReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)operateLimitTimeVendorTargetReq, Message.OperateLimitTimeVendorTargetReq);
	}

	public void Operate_LimitTimeVendorTarget_Ack(ByteString response)
	{
		OperateLimitTimeVendorTargetAck operateLimitTimeVendorTargetAck = OperateLimitTimeVendorTargetAck.Parser.ParseFrom(response);
		if (operateLimitTimeVendorTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.LimitTimeVendorList.Clear();
			zbServerData.sGameData.OperateInfo.LimitTimeVendorList.AddRange((IEnumerable<LimitTimeRouletteDetail>)operateLimitTimeVendorTargetAck.LimitTimeVendorList);
			zbSelf.Instance.dataHandler.DataChange(operateLimitTimeVendorTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateLimitTimeVendorTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_RestaurantActivateAward_Req(string uuid)
	{
		OperateRestaurantActivateAwardReq operateRestaurantActivateAwardReq = new OperateRestaurantActivateAwardReq();
		operateRestaurantActivateAwardReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateRestaurantActivateAwardReq, Message.OperateRestaurantActivateAwardReq);
	}

	public void Operate_RestaurantActivateAward_Ack(ByteString response)
	{
		OperateRestaurantActivateAwardAck operateRestaurantActivateAwardAck = OperateRestaurantActivateAwardAck.Parser.ParseFrom(response);
		if (operateRestaurantActivateAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.RestaurantList.Clear();
			zbServerData.sGameData.OperateInfo.RestaurantList.AddRange((IEnumerable<RestaurantDetail>)operateRestaurantActivateAwardAck.RestaurantList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_RestaurantActivateAward_Ack, operateRestaurantActivateAwardAck);
		}
	}

	public void Operate_RestaurantDailyAward_Req(string uuid)
	{
		OperateRestaurantDailyAwardReq operateRestaurantDailyAwardReq = new OperateRestaurantDailyAwardReq();
		operateRestaurantDailyAwardReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateRestaurantDailyAwardReq, Message.OperateRestaurantDailyAwardReq);
	}

	public void Operate_RestaurantDailyAward_Ack(ByteString response)
	{
		OperateRestaurantDailyAwardAck operateRestaurantDailyAwardAck = OperateRestaurantDailyAwardAck.Parser.ParseFrom(response);
		if (operateRestaurantDailyAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.RestaurantList.Clear();
			zbServerData.sGameData.OperateInfo.RestaurantList.AddRange((IEnumerable<RestaurantDetail>)operateRestaurantDailyAwardAck.RestaurantList);
			zbSelf.Instance.dataHandler.DataChange(operateRestaurantDailyAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateRestaurantDailyAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Restaurant);
		}
	}

	public void Operate_RestaurantBuyPiece_Req(string uuid, int[] slotData, int count)
	{
		OperateRestaurantBuyPieceReq operateRestaurantBuyPieceReq = new OperateRestaurantBuyPieceReq();
		operateRestaurantBuyPieceReq.UUID = uuid;
		operateRestaurantBuyPieceReq.SlotData.Clear();
		operateRestaurantBuyPieceReq.SlotData.AddRange((IEnumerable<int>)slotData);
		operateRestaurantBuyPieceReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)operateRestaurantBuyPieceReq, Message.OperateRestaurantBuyPieceReq);
	}

	public void Operate_RestaurantBuyPiece_Ack(ByteString response)
	{
		OperateRestaurantBuyPieceAck operateRestaurantBuyPieceAck = OperateRestaurantBuyPieceAck.Parser.ParseFrom(response);
		if (operateRestaurantBuyPieceAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateRestaurantBuyPieceAck.DataChange);
			zbServerData.sGameData.OperateInfo.RestaurantList.Clear();
			zbServerData.sGameData.OperateInfo.RestaurantList.AddRange((IEnumerable<RestaurantDetail>)operateRestaurantBuyPieceAck.RestaurantList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Restaurant);
		}
	}

	public void Operate_RestaurantDelPiece_Req(string uuid, int[] slotData, int slotIndex)
	{
		OperateRestaurantDelPieceReq operateRestaurantDelPieceReq = new OperateRestaurantDelPieceReq();
		operateRestaurantDelPieceReq.UUID = uuid;
		operateRestaurantDelPieceReq.SlotData.Clear();
		operateRestaurantDelPieceReq.SlotData.AddRange((IEnumerable<int>)slotData);
		operateRestaurantDelPieceReq.SlotIndex = slotIndex;
		Instance.WsPackMsg((IMessage)(object)operateRestaurantDelPieceReq, Message.OperateRestaurantDelPieceReq);
	}

	public void Operate_RestaurantDelPiece_Ack(ByteString response)
	{
		OperateRestaurantDelPieceAck operateRestaurantDelPieceAck = OperateRestaurantDelPieceAck.Parser.ParseFrom(response);
		if (operateRestaurantDelPieceAck != null)
		{
			zbServerData.sGameData.OperateInfo.RestaurantList.Clear();
			zbServerData.sGameData.OperateInfo.RestaurantList.AddRange((IEnumerable<RestaurantDetail>)operateRestaurantDelPieceAck.RestaurantList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Restaurant);
		}
	}

	public void Operate_RestaurantSlotSync_Req(string uuid, int[] slotData)
	{
		OperateRestaurantSlotSyncReq operateRestaurantSlotSyncReq = new OperateRestaurantSlotSyncReq();
		operateRestaurantSlotSyncReq.UUID = uuid;
		operateRestaurantSlotSyncReq.SlotData.Clear();
		operateRestaurantSlotSyncReq.SlotData.AddRange((IEnumerable<int>)slotData);
		Instance.WsPackMsg((IMessage)(object)operateRestaurantSlotSyncReq, Message.OperateRestaurantSlotSyncReq);
	}

	public void Operate_RestaurantSlotSync_Ack(ByteString response)
	{
		OperateRestaurantSlotSyncAck operateRestaurantSlotSyncAck = OperateRestaurantSlotSyncAck.Parser.ParseFrom(response);
		if (operateRestaurantSlotSyncAck != null)
		{
			zbServerData.sGameData.OperateInfo.RestaurantList.Clear();
			zbServerData.sGameData.OperateInfo.RestaurantList.AddRange((IEnumerable<RestaurantDetail>)operateRestaurantSlotSyncAck.RestaurantList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Restaurant);
		}
	}

	public void Operate_RestaurantTarget_Req(string uuid, int[] slotData)
	{
		OperateRestaurantTargetReq operateRestaurantTargetReq = new OperateRestaurantTargetReq();
		operateRestaurantTargetReq.UUID = uuid;
		operateRestaurantTargetReq.SlotData.Clear();
		operateRestaurantTargetReq.SlotData.AddRange((IEnumerable<int>)slotData);
		Instance.WsPackMsg((IMessage)(object)operateRestaurantTargetReq, Message.OperateRestaurantTargetReq);
	}

	public void Operate_RestaurantTarget_Ack(ByteString response)
	{
		OperateRestaurantTargetAck operateRestaurantTargetAck = OperateRestaurantTargetAck.Parser.ParseFrom(response);
		if (operateRestaurantTargetAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_RestaurantTarget_Ack, operateRestaurantTargetAck);
		}
	}

	public void Operate_RestaurantOfferPiece_Req(string uuid, int[] slotData, int slotIndex)
	{
		OperateRestaurantOfferPieceReq operateRestaurantOfferPieceReq = new OperateRestaurantOfferPieceReq();
		operateRestaurantOfferPieceReq.UUID = uuid;
		operateRestaurantOfferPieceReq.SlotData.Clear();
		operateRestaurantOfferPieceReq.SlotData.AddRange((IEnumerable<int>)slotData);
		operateRestaurantOfferPieceReq.SlotIndex = slotIndex;
		Instance.WsPackMsg((IMessage)(object)operateRestaurantOfferPieceReq, Message.OperateRestaurantOfferPieceReq);
	}

	public void Operate_RestaurantOfferPiece_Ack(ByteString response)
	{
		OperateRestaurantOfferPieceAck operateRestaurantOfferPieceAck = OperateRestaurantOfferPieceAck.Parser.ParseFrom(response);
		if (operateRestaurantOfferPieceAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_RestaurantOfferPiece_Ack, operateRestaurantOfferPieceAck);
		}
	}

	public void Operate_ThemeTaskData_Req()
	{
		OperateThemeTaskDataReq req = new OperateThemeTaskDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateThemeTaskDataReq);
	}

	public void Operate_ThemeTaskData_Ack(ByteString response)
	{
		OperateThemeTaskDataAck operateThemeTaskDataAck = OperateThemeTaskDataAck.Parser.ParseFrom(response);
		if (operateThemeTaskDataAck != null)
		{
			zbServerData.sGameData.OperateInfo.ThemeTaskList.Clear();
			zbServerData.sGameData.OperateInfo.ThemeTaskList.AddRange((IEnumerable<ThemeTaskDetail>)operateThemeTaskDataAck.ThemeTaskList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ThemeTaskTaskAward_Req(string uuid, int taskId)
	{
		OperateThemeTaskTaskAwardReq operateThemeTaskTaskAwardReq = new OperateThemeTaskTaskAwardReq();
		operateThemeTaskTaskAwardReq.UUID = uuid;
		operateThemeTaskTaskAwardReq.TaskId = taskId;
		Instance.WsPackMsg((IMessage)(object)operateThemeTaskTaskAwardReq, Message.OperateThemeTaskTaskAwardReq);
	}

	public void Operate_ThemeTaskTaskAward_Ack(ByteString response)
	{
		OperateThemeTaskTaskAwardAck operateThemeTaskTaskAwardAck = OperateThemeTaskTaskAwardAck.Parser.ParseFrom(response);
		if (operateThemeTaskTaskAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.ThemeTaskList.Clear();
			zbServerData.sGameData.OperateInfo.ThemeTaskList.AddRange((IEnumerable<ThemeTaskDetail>)operateThemeTaskTaskAwardAck.ThemeTaskList);
			zbSelf.Instance.dataHandler.DataChange(operateThemeTaskTaskAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateThemeTaskTaskAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ThemeTaskTargetAward_Req(string uuid)
	{
		OperateThemeTaskTargetAwardReq operateThemeTaskTargetAwardReq = new OperateThemeTaskTargetAwardReq();
		operateThemeTaskTargetAwardReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateThemeTaskTargetAwardReq, Message.OperateThemeTaskTargetAwardReq);
	}

	public void Operate_ThemeTaskTargetAward_Ack(ByteString response)
	{
		OperateThemeTaskTargetAwardAck operateThemeTaskTargetAwardAck = OperateThemeTaskTargetAwardAck.Parser.ParseFrom(response);
		if (operateThemeTaskTargetAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.ThemeTaskList.Clear();
			zbServerData.sGameData.OperateInfo.ThemeTaskList.AddRange((IEnumerable<ThemeTaskDetail>)operateThemeTaskTargetAwardAck.ThemeTaskList);
			zbSelf.Instance.dataHandler.DataChange(operateThemeTaskTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateThemeTaskTargetAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ZhanLingData_Req()
	{
		OperateZhanLingDataReq req = new OperateZhanLingDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateZhanLingDataReq);
	}

	public void Operate_ZhanLingData_Ack(ByteString response)
	{
		OperateZhanLingDataAck operateZhanLingDataAck = OperateZhanLingDataAck.Parser.ParseFrom(response);
		if (operateZhanLingDataAck != null)
		{
			zbServerData.sGameData.OperateInfo.ZhanLingKillList.Clear();
			zbServerData.sGameData.OperateInfo.ZhanLingKillList.AddRange((IEnumerable<ZhanLingKillDetail>)operateZhanLingDataAck.ZhanLingKillList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Operate_ZhanLingKillAward_Req(string UUID, int Level)
	{
		OperateZhanLingKillAwardReq operateZhanLingKillAwardReq = new OperateZhanLingKillAwardReq();
		operateZhanLingKillAwardReq.UUID = UUID;
		operateZhanLingKillAwardReq.Level = Level;
		Instance.WsPackMsg((IMessage)(object)operateZhanLingKillAwardReq, Message.OperateZhanLingKillAwardReq);
	}

	public void Operate_ZhanLingKillAward_Ack(ByteString response)
	{
		OperateZhanLingKillAwardAck operateZhanLingKillAwardAck = OperateZhanLingKillAwardAck.Parser.ParseFrom(response);
		if (operateZhanLingKillAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.ZhanLingKillList.Clear();
			zbServerData.sGameData.OperateInfo.ZhanLingKillList.AddRange((IEnumerable<ZhanLingKillDetail>)operateZhanLingKillAwardAck.ZhanLingKillList);
			zbSelf.Instance.dataHandler.DataChange(operateZhanLingKillAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateZhanLingKillAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Operate);
		}
	}

	public void Passport_BuyLevel_Req(int index, int count)
	{
		PassportBuyLevelReq passportBuyLevelReq = new PassportBuyLevelReq();
		passportBuyLevelReq.Count = count;
		passportBuyLevelReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)passportBuyLevelReq, Message.PassportBuyLevelReq);
	}

	public void Passport_BuyLevel_Ack(ByteString response)
	{
		PassportBuyLevelAck passportBuyLevelAck = PassportBuyLevelAck.Parser.ParseFrom(response);
		if (passportBuyLevelAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(passportBuyLevelAck.DataChange);
			zbServerData.sGameData.RechargeActiveInfo = passportBuyLevelAck.RechargeActiveInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Passport_Data);
		}
	}

	public void Passport_LevelAward_Req(int level)
	{
		PassportLevelAwardReq passportLevelAwardReq = new PassportLevelAwardReq();
		passportLevelAwardReq.Level = level;
		Instance.WsPackMsg((IMessage)(object)passportLevelAwardReq, Message.PassportLevelAwardReq);
	}

	public void Passport_LevelAward_Ack(ByteString response)
	{
		PassportLevelAwardAck passportLevelAwardAck = PassportLevelAwardAck.Parser.ParseFrom(response);
		if (passportLevelAwardAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = passportLevelAwardAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(passportLevelAwardAck.DataChange, ((IEnumerable<AwardDetail>)passportLevelAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Passport_Data);
		}
	}

	public void RechargeActive_ZhanLingMainAward_Req(int mainLevel)
	{
		RechargeActiveZhanLingMainAwardReq rechargeActiveZhanLingMainAwardReq = new RechargeActiveZhanLingMainAwardReq();
		rechargeActiveZhanLingMainAwardReq.MainLevel = mainLevel;
		Instance.WsPackMsg((IMessage)(object)rechargeActiveZhanLingMainAwardReq, Message.RechargeActiveZhanLingMainAwardReq);
	}

	public void RechargeActive_ZhanLingMainAward_Ack(ByteString response)
	{
		RechargeActiveZhanLingMainAwardAck rechargeActiveZhanLingMainAwardAck = RechargeActiveZhanLingMainAwardAck.Parser.ParseFrom(response);
		if (rechargeActiveZhanLingMainAwardAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveZhanLingMainAwardAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeActiveZhanLingMainAwardAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveZhanLingMainAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Activity);
		}
	}

	public void PvpSet_Data_Req()
	{
		PvpSetDataReq req = new PvpSetDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PvpSetDataReq);
	}

	public void PvpSet_Data_Ack(ByteString response)
	{
		PvpSetDataAck pvpSetDataAck = PvpSetDataAck.Parser.ParseFrom(response);
		if (pvpSetDataAck != null)
		{
			zbServerData.sGameData.PvpInfo = pvpSetDataAck.PvpInfo;
		}
	}

	public void PvpSet_ModStart_Req()
	{
		PvpSetModStartReq req = new PvpSetModStartReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PvpSetModStartReq);
	}

	public void PvpSet_ModStart_Ack(ByteString response)
	{
		PvpSetModStartAck pvpSetModStartAck = PvpSetModStartAck.Parser.ParseFrom(response);
		if (pvpSetModStartAck != null)
		{
			zbServerData.sGameData.PvpInfo = pvpSetModStartAck.PvpInfo;
			zbSelf.Instance.dataHandler.DataChange(pvpSetModStartAck.DataChange);
			zbUIMgr.Instance.ShowPvpModSelect();
		}
	}

	public void PvpSet_ModRefresh_Req()
	{
		PvpSetModRefreshReq req = new PvpSetModRefreshReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PvpSetModRefreshReq);
	}

	public void PvpSet_ModRefresh_Ack(ByteString response)
	{
		PvpSetModRefreshAck pvpSetModRefreshAck = PvpSetModRefreshAck.Parser.ParseFrom(response);
		if (pvpSetModRefreshAck != null)
		{
			zbServerData.sGameData.PvpInfo = pvpSetModRefreshAck.PvpInfo;
			zbSelf.Instance.dataHandler.DataChange(pvpSetModRefreshAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_PvpSet_ModRefresh);
		}
	}

	public void PvpSet_ModSelect_Req(int Step, int SelectId)
	{
		PvpSetModSelectReq pvpSetModSelectReq = new PvpSetModSelectReq();
		pvpSetModSelectReq.Step = Step;
		pvpSetModSelectReq.SelectId = SelectId;
		Instance.WsPackMsg((IMessage)(object)pvpSetModSelectReq, Message.PvpSetModSelectReq);
	}

	public void PvpSet_ModSelect_Ack(ByteString response)
	{
		PvpSetModSelectAck pvpSetModSelectAck = PvpSetModSelectAck.Parser.ParseFrom(response);
		if (pvpSetModSelectAck != null)
		{
			zbServerData.sGameData.PvpInfo = pvpSetModSelectAck.PvpInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_PvpSet_ModSelect);
		}
	}

	public void PvpSet_ModSubmit_Req()
	{
		PvpSetModSubmitReq req = new PvpSetModSubmitReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PvpSetModSubmitReq);
	}

	public void PvpSet_ModSubmit_Ack(ByteString response)
	{
		PvpSetModSubmitAck pvpSetModSubmitAck = PvpSetModSubmitAck.Parser.ParseFrom(response);
		if (pvpSetModSubmitAck != null)
		{
			zbServerData.sGameData.PvpInfo = pvpSetModSubmitAck.PvpInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_PvpSet_ModSubmit);
		}
	}

	public void PvpSet_ModCancel_Req()
	{
		PvpSetModCancelReq req = new PvpSetModCancelReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PvpSetModCancelReq);
	}

	public void PvpSet_ModCancel_Ack(ByteString response)
	{
		PvpSetModCancelAck pvpSetModCancelAck = PvpSetModCancelAck.Parser.ParseFrom(response);
		if (pvpSetModCancelAck != null)
		{
			zbServerData.sGameData.PvpInfo = pvpSetModCancelAck.PvpInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_PvpSet_ModSubmit);
		}
	}

	public void Rank_Top_Req()
	{
		RankTopReq req = new RankTopReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.RankTopReq);
	}

	public void Rank_Top_Ack(ByteString response)
	{
		RankTopAck rankTopAck = RankTopAck.Parser.ParseFrom(response);
		if (rankTopAck != null)
		{
			zbSelf.Instance.arenaHandler.UpdateArenaRankTop(rankTopAck);
			RankTopDetail rankTopDetail = ((IEnumerable<RankTopDetail>)rankTopAck.RankTopList).FirstOrDefault((RankTopDetail r) => r.RankType == ERankType.MainQuest);
			if (rankTopDetail != null)
			{
				zbSelf.Instance.rankHandler.UpdateMainTopReach(rankTopDetail);
			}
		}
	}

	public void Rank_List_Req(ERankType rankType, bool openRank = false)
	{
		RankListReq rankListReq = new RankListReq();
		rankListReq.RankType = rankType;
		openRankWnd = openRank;
		Instance.WsPackMsg((IMessage)(object)rankListReq, Message.RankListReq);
	}

	public void Rank_List_Ack(ByteString response)
	{
		RankListAck rankListAck = RankListAck.Parser.ParseFrom(response);
		if (rankListAck == null)
		{
			return;
		}
		if (rankListAck.RankType == ERankType.MainQuest)
		{
			zbSelf.Instance.rankHandler.UpdateMainQuestRankListAck(rankListAck);
		}
		if (openRankWnd)
		{
			if (rankListAck.RankType == ERankType.MainQuest)
			{
				zbSelf.Instance.rankHandler.m_mainQuestRankListAck = rankListAck;
				zbUIMgr.Instance.ShowRank(0);
			}
			else
			{
				zbSelf.Instance.rankHandler.m_endlessHordeRankListAck = rankListAck;
				zbUIMgr.Instance.ShowRank(2);
			}
			openRankWnd = false;
		}
		if (rankListAck.RankType == ERankType.EndlessHorde)
		{
			if (rankListAck.SelfRank != null)
			{
				zbSelf.Instance.endlessHordeHandler.UpdateEndlessHordeSelfRank(rankListAck.SelfRank.Rank);
			}
			zbSelf.Instance.endlessHordeHandler.rankListAck = rankListAck;
			zbSelf.Instance.endlessHordeHandler.GetRankValueBySelf(zbServerData.sGameData.EndlessHordeInfo.PassDistance);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_RankData_EndlessHorde);
		}
	}

	public void Rank_FirstReach_Req(ERankType rankType)
	{
		RankFirstReachReq rankFirstReachReq = new RankFirstReachReq();
		rankFirstReachReq.RankType = rankType;
		Instance.WsPackMsg((IMessage)(object)rankFirstReachReq, Message.RankFirstReachReq);
	}

	public void Rank_FirstReach_Ack(ByteString response)
	{
		RankFirstReachAck rankFirstReachAck = RankFirstReachAck.Parser.ParseFrom(response);
		if (rankFirstReachAck != null)
		{
			zbUIMgr.Instance.ShowRankFirstReach(((IEnumerable<RankFirstReachDetail>)rankFirstReachAck.ReachList).ToArray());
		}
	}

	public void Rank_FirstReachAward_Req(ERankType rankType)
	{
		RankFirstReachAwardReq rankFirstReachAwardReq = new RankFirstReachAwardReq();
		rankFirstReachAwardReq.RankType = rankType;
		Instance.WsPackMsg((IMessage)(object)rankFirstReachAwardReq, Message.RankFirstReachAwardReq);
	}

	public void Rank_FirstReachAward_Ack(ByteString response)
	{
		RankFirstReachAwardAck rankFirstReachAwardAck = RankFirstReachAwardAck.Parser.ParseFrom(response);
		if (rankFirstReachAwardAck != null)
		{
			zbServerData.sGameData.AuxAwardInfo = rankFirstReachAwardAck.AuxAwardInfo;
			zbSelf.Instance.dataHandler.DataChange(rankFirstReachAwardAck.DataChange, ((IEnumerable<AwardDetail>)rankFirstReachAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_FirstReachAward);
		}
	}

	public void OpenServer_RankTop_Req()
	{
		OpenServerRankTopReq req = new OpenServerRankTopReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OpenServerRankTopReq);
	}

	public void OpenServer_RankTop_Ack(ByteString response)
	{
		OpenServerRankTopAck openServerRankTopAck = OpenServerRankTopAck.Parser.ParseFrom(response);
		if (openServerRankTopAck != null)
		{
			zbSelf.Instance.rankHandler.UpdateOpenServerRankTop(((IEnumerable<OpsRankTopDetail>)openServerRankTopAck.RankTopList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_OpenServer_RankTop);
		}
	}

	public void OpenServer_RankList_Req(ERankType rankType)
	{
		OpenServerRankListReq openServerRankListReq = new OpenServerRankListReq();
		openServerRankListReq.RankType = rankType;
		Instance.WsPackMsg((IMessage)(object)openServerRankListReq, Message.OpenServerRankListReq);
	}

	public void OpenServer_RankList_Ack(ByteString response)
	{
		OpenServerRankListAck openServerRankListAck = OpenServerRankListAck.Parser.ParseFrom(response);
		if (openServerRankListAck != null)
		{
			zbUIMgr.Instance.ShowOpenServerRank(openServerRankListAck.OpsRankInfo);
		}
	}

	public void Season_Enter_Req()
	{
		SeasonEnterReq req = new SeasonEnterReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonEnterReq);
	}

	public void Season_Enter_Ack(ByteString response)
	{
		SeasonEnterAck seasonEnterAck = SeasonEnterAck.Parser.ParseFrom(response);
		if (seasonEnterAck != null)
		{
			zbSelf.Instance.seasonHandler.seasonEnterAck = seasonEnterAck;
			zbUIMgr.Instance.ShowSeasonWnd();
		}
	}

	public void Season_Data_Req()
	{
		SeasonDataReq req = new SeasonDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonDataReq);
	}

	public void Season_Data_Ack(ByteString response)
	{
		SeasonDataAck seasonDataAck = SeasonDataAck.Parser.ParseFrom(response);
		if (seasonDataAck != null)
		{
			zbSelf.Instance.seasonHandler.seasonDataAck = seasonDataAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Season_Online);
		}
	}

	public void Season_Map_Info_Req()
	{
		SeasonMapInfoReq req = new SeasonMapInfoReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonMapInfoReq);
	}

	public void Season_Map_Info_Ack(ByteString response)
	{
		SeasonMapInfoAck seasonMapInfoAck = SeasonMapInfoAck.Parser.ParseFrom(response);
		if (seasonMapInfoAck != null)
		{
			zbSelf.Instance.seasonHandler.areaList = ((IEnumerable<SeasonAreaInfo>)seasonMapInfoAck.AreaList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SeasonMap_Ack);
		}
	}

	public void Season_View_Req(float centerX, float centerY)
	{
		SeasonViewReq seasonViewReq = new SeasonViewReq();
		seasonViewReq.CenterX = centerX;
		seasonViewReq.CenterY = centerY;
		Instance.WsPackMsg((IMessage)(object)seasonViewReq, Message.SeasonViewReq);
	}

	public void Season_View_Ack(ByteString response)
	{
		SeasonViewAck.Parser.ParseFrom(response);
	}

	public void Season_Team_Pos_Req()
	{
		SeasonTeamPosReq req = new SeasonTeamPosReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonTeamPosReq);
	}

	public void Season_Team_Pos_Ack(ByteString response)
	{
		SeasonTeamPosAck seasonTeamPosAck = SeasonTeamPosAck.Parser.ParseFrom(response);
		if (seasonTeamPosAck != null)
		{
			zbSelf.Instance.seasonHandler.teamPosList = ((IEnumerable<SeasonTeamPosInfo>)seasonTeamPosAck.TeamPosList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Team_Pos);
		}
	}

	public void Season_March_Start_Req(int teamId, int action, float toX, float toY, int resIdx = 0)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		SeasonMarchStartReq seasonMarchStartReq = new SeasonMarchStartReq();
		seasonMarchStartReq.TeamId = teamId;
		seasonMarchStartReq.Action = action;
		Vector2 val = zbSelf.Instance.seasonHandler.TransformWorldPosToLogicPos(toX, toY);
		seasonMarchStartReq.ToX = val.x;
		seasonMarchStartReq.ToY = val.y;
		seasonMarchStartReq.ResIdx = resIdx;
		_ = zbSeasonMgr.Instance.SeasonMap.m_buildDict[new Vector2(toX, toY)];
		Instance.WsPackMsg((IMessage)(object)seasonMarchStartReq, Message.SeasonMarchStartReq);
	}

	public void Season_March_Start_Ack(ByteString response)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		SeasonMarchStartAck seasonMarchStartAck = SeasonMarchStartAck.Parser.ParseFrom(response);
		if (seasonMarchStartAck?.Move != null)
		{
			zbSeasonHandler zbSeasonHandler2 = zbSelf.Instance?.seasonHandler;
			if (zbSeasonHandler2 != null)
			{
				SeasonMoveDetail seasonMoveDetail = seasonMarchStartAck.Move.Clone();
				Vector2 val = zbSeasonHandler2.TransformLogicPosToWorldPos(seasonMoveDetail.ToX, seasonMoveDetail.ToY);
				Vector2 val2 = zbSeasonHandler2.TransformLogicPosToWorldPos(seasonMoveDetail.FromX, seasonMoveDetail.FromY);
				seasonMoveDetail.ToX = val.x;
				seasonMoveDetail.ToY = val.y;
				seasonMoveDetail.FromX = val2.x;
				seasonMoveDetail.FromY = val2.y;
				zbSeasonHandler2.AddLine(seasonMoveDetail);
			}
		}
	}

	public void Season_Move_Ntf(ByteString response)
	{
		SeasonMoveNtf seasonMoveNtf = SeasonMoveNtf.Parser.ParseFrom(response);
		if (seasonMoveNtf == null)
		{
			return;
		}
		foreach (SeasonMoveDetail move in seasonMoveNtf.MoveList)
		{
			zbSelf.Instance.seasonHandler.AddLine(move);
		}
	}

	public void Season_March_Arrive_Ntf(ByteString response)
	{
		SeasonMarchArriveNtf.Parser.ParseFrom(response);
	}

	public void Season_Map_Info_Ntf(ByteString response)
	{
		SeasonMapInfoNtf.Parser.ParseFrom(response);
	}

	public void Season_City_Detail_Req(int areaId)
	{
		SeasonCityDetailReq seasonCityDetailReq = new SeasonCityDetailReq();
		seasonCityDetailReq.CityId = areaId;
		Instance.WsPackMsg((IMessage)(object)seasonCityDetailReq, Message.SeasonCityDetailReq);
	}

	public void Season_City_Detail_Ack(ByteString response)
	{
		SeasonCityDetailAck seasonCityDetailAck = SeasonCityDetailAck.Parser.ParseFrom(response);
		if (seasonCityDetailAck != null)
		{
			zbSeasonMgr.Instance.SetUiElementVisible(visible: false);
			zbSeasonBuild cityInstance = zbSelf.Instance.seasonHandler.GetCityByAreaId(seasonCityDetailAck.CityId);
			zbSeasonMgr.Instance.SeasonMap.TrackCityByAreaId(seasonCityDetailAck.CityId, delegate
			{
				zbUIMgr.Instance.ShowSeasonBuildDetail(cityInstance);
			});
		}
	}

	public void Season_Res_Attack_Req(int teamId, int areaId, int resIndex)
	{
		SeasonResAttackReq seasonResAttackReq = new SeasonResAttackReq();
		seasonResAttackReq.TeamId = teamId;
		seasonResAttackReq.CityId = areaId;
		seasonResAttackReq.ResIdx = resIndex;
		Instance.WsPackMsg((IMessage)(object)seasonResAttackReq, Message.SeasonResAttackReq);
	}

	public void Season_Res_Attack_Ack(ByteString response)
	{
		SeasonResAttackAck.Parser.ParseFrom(response);
	}

	public void Season_Res_Occupy_Req(int teamId, int areaId, int resIndex)
	{
		SeasonResOccupyReq seasonResOccupyReq = new SeasonResOccupyReq();
		seasonResOccupyReq.TeamId = teamId;
		seasonResOccupyReq.CityId = areaId;
		seasonResOccupyReq.ResIdx = resIndex;
		Instance.WsPackMsg((IMessage)(object)seasonResOccupyReq, Message.SeasonResOccupyReq);
	}

	public void Season_Res_Occupy_Ack(ByteString response)
	{
		SeasonResOccupyAck.Parser.ParseFrom(response);
	}

	public void Season_Res_Abandon_Req(int cityId, int resId)
	{
		SeasonResAbandonReq seasonResAbandonReq = new SeasonResAbandonReq();
		seasonResAbandonReq.CityId = cityId;
		seasonResAbandonReq.ResIdx = resId;
		Instance.WsPackMsg((IMessage)(object)seasonResAbandonReq, Message.SeasonResAbandonReq);
	}

	public void Season_Res_Abandon_Ack(ByteString response)
	{
		SeasonResAbandonAck.Parser.ParseFrom(response);
	}

	public void Season_My_Territory_Req()
	{
		SeasonMyTerritoryReq req = new SeasonMyTerritoryReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonMyTerritoryReq);
	}

	public void Season_My_Territory_Ack(ByteString response)
	{
		SeasonMyTerritoryAck seasonMyTerritoryAck = SeasonMyTerritoryAck.Parser.ParseFrom(response);
		if (seasonMyTerritoryAck != null)
		{
			zbSelf.Instance.seasonHandler.seasonMyTerritoryAck = seasonMyTerritoryAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_MyTerritory_Ack);
		}
	}

	public void Season_Adjacent_Req()
	{
		SeasonAdjacentReq req = new SeasonAdjacentReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonAdjacentReq);
	}

	public void Season_Adjacent_Ack(ByteString response)
	{
		SeasonAdjacentAck.Parser.ParseFrom(response);
	}

	public void Season_Res_Produce_Ntf(ByteString response)
	{
		SeasonResProduceNtf.Parser.ParseFrom(response);
	}

	public void Season_Aoi_Ntf(ByteString response)
	{
		SeasonAoiNtf.Parser.ParseFrom(response);
	}

	public void Season_Declare_Req()
	{
		SeasonDeclareReq req = new SeasonDeclareReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonDeclareReq);
	}

	public void Season_Declare_Ack(ByteString response)
	{
		SeasonDeclareAck.Parser.ParseFrom(response);
	}

	public void Season_Declare_List_Req()
	{
		SeasonDeclareListReq req = new SeasonDeclareListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonDeclareListReq);
	}

	public void Season_Declare_List_Ack(ByteString response)
	{
		SeasonDeclareListAck seasonDeclareListAck = SeasonDeclareListAck.Parser.ParseFrom(response);
		if (seasonDeclareListAck != null)
		{
			zbSelf.Instance.seasonHandler.seasonDeclareList = ((IEnumerable<SeasonDeclareInfo>)seasonDeclareListAck.List).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DeclareList_Ack);
		}
	}

	public void Season_Garrison_Req()
	{
		SeasonGarrisonReq req = new SeasonGarrisonReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonGarrisonReq);
	}

	public void Season_Garrison_Ack(ByteString response)
	{
		SeasonGarrisonAck.Parser.ParseFrom(response);
	}

	public void Season_Recruit_Req()
	{
		SeasonRecruitReq req = new SeasonRecruitReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonRecruitReq);
	}

	public void Season_Recruit_Ack(ByteString response)
	{
		SeasonRecruitAck.Parser.ParseFrom(response);
	}

	public void Season_Guard_Info_Req(int cityId)
	{
		SeasonGuardInfoReq seasonGuardInfoReq = new SeasonGuardInfoReq();
		seasonGuardInfoReq.CityId = cityId;
		Instance.WsPackMsg((IMessage)(object)seasonGuardInfoReq, Message.SeasonGuardInfoReq);
	}

	public void Season_Guard_Info_Ack(ByteString response)
	{
		if (SeasonGuardInfoAck.Parser.ParseFrom(response) != null)
		{
			zbUIMgr.Instance.ShowSeasonGuard();
		}
	}

	public void Season_City_Attack_Req()
	{
		SeasonCityAttackReq req = new SeasonCityAttackReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonCityAttackReq);
	}

	public void Season_City_Attack_Ack(ByteString response)
	{
		SeasonCityAttackAck.Parser.ParseFrom(response);
	}

	public void Season_CityWar_Result_Ntf(ByteString response)
	{
		SeasonCityWarResultNtf.Parser.ParseFrom(response);
	}

	public void Season_City_Owner_Ntf(ByteString response)
	{
		SeasonCityOwnerNtf.Parser.ParseFrom(response);
	}

	public void Season_CityWar_Info_Req()
	{
		SeasonCityWarInfoReq req = new SeasonCityWarInfoReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonCityWarInfoReq);
	}

	public void Season_CityWar_Info_Ack(ByteString response)
	{
		SeasonCityWarInfoAck seasonCityWarInfoAck = SeasonCityWarInfoAck.Parser.ParseFrom(response);
		if (seasonCityWarInfoAck != null)
		{
			zbSelf.Instance.seasonHandler.seasonCityWarInfoAck = seasonCityWarInfoAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_CityWar_Ack);
		}
	}

	public void Season_CityWar_State_Ntf(ByteString response)
	{
		SeasonCityWarStateNtf seasonCityWarStateNtf = SeasonCityWarStateNtf.Parser.ParseFrom(response);
		if (seasonCityWarStateNtf != null && zbSeasonMgr.Instance.fsm.m_eState == ESeasonState.Running && seasonCityWarStateNtf.State == ESeasonCityWarState.Opening)
		{
			zbSeasonMgr.Instance.fsm.SetState(ESeasonState.CityWar);
		}
	}

	public void Season_Online_Reward_Req()
	{
		SeasonOnlineRewardReq req = new SeasonOnlineRewardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonOnlineRewardReq);
	}

	public void Season_Online_Reward_Ack(ByteString response)
	{
		SeasonOnlineRewardAck.Parser.ParseFrom(response);
	}

	public void Season_Merit_Rank_Req()
	{
		SeasonMeritRankReq req = new SeasonMeritRankReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonMeritRankReq);
	}

	public void Season_Merit_Rank_Ack(ByteString response)
	{
		SeasonMeritRankAck.Parser.ParseFrom(response);
	}

	public void Season_Battle_Result_Req(string uuid, BattleResultInfo battleResult)
	{
		SeasonBattleResultReq seasonBattleResultReq = new SeasonBattleResultReq();
		seasonBattleResultReq.Uuid = uuid;
		seasonBattleResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)seasonBattleResultReq, Message.SeasonBattleResultReq);
	}

	public void Season_Battle_Result_Ack(ByteString response)
	{
		SeasonBattleResultAck.Parser.ParseFrom(response);
	}

	public void Season_Repair_Req()
	{
		SeasonRepairReq req = new SeasonRepairReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonRepairReq);
	}

	public void Season_Repair_Ack(ByteString response)
	{
		SeasonRepairAck.Parser.ParseFrom(response);
	}

	public void Season_Airdrop_List_Req(bool openWnd = false)
	{
		SeasonAirdropListReq req = new SeasonAirdropListReq();
		openSeasonAirdropWnd = openWnd;
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonAirdropListReq);
	}

	public void Season_Airdrop_List_Ack(ByteString response)
	{
		SeasonAirdropListAck seasonAirdropListAck = SeasonAirdropListAck.Parser.ParseFrom(response);
		if (seasonAirdropListAck != null)
		{
			zbSelf.Instance.seasonHandler.seasonAirdropListAck = seasonAirdropListAck;
			if (openSeasonAirdropWnd)
			{
				zbUIMgr.Instance.ShowSeasonAirdropClash();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SeasonAirdrop);
			}
		}
	}

	public void Season_Airdrop_Refresh_Ntf(ByteString response)
	{
		SeasonAirdropRefreshNtf seasonAirdropRefreshNtf = SeasonAirdropRefreshNtf.Parser.ParseFrom(response);
		if (seasonAirdropRefreshNtf != null)
		{
			if (zbSelf.Instance.seasonHandler.seasonAirdropListAck == null)
			{
				zbSelf.Instance.seasonHandler.seasonAirdropListAck = new SeasonAirdropListAck();
			}
			zbSelf.Instance.seasonHandler.seasonAirdropListAck.List.Clear();
			zbSelf.Instance.seasonHandler.seasonAirdropListAck.List.AddRange((IEnumerable<SeasonAirdropInfo>)seasonAirdropRefreshNtf.List);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SeasonAirdrop);
		}
	}

	public void Season_Airdrop_Decrypt_Req(int DropId)
	{
		SeasonAirdropDecryptReq seasonAirdropDecryptReq = new SeasonAirdropDecryptReq();
		seasonAirdropDecryptReq.DropId = DropId;
		Instance.WsPackMsg((IMessage)(object)seasonAirdropDecryptReq, Message.SeasonAirdropDecryptReq);
	}

	public void Season_Airdrop_Decrypt_Ack(ByteString response)
	{
		SeasonAirdropDecryptAck seasonAirdropDecryptAck = SeasonAirdropDecryptAck.Parser.ParseFrom(response);
		if (seasonAirdropDecryptAck != null)
		{
			zbSelf.Instance.seasonHandler.seasonAirdropProg = seasonAirdropDecryptAck.Prog;
			zbSelf.Instance.seasonHandler.UpdateSeasonAirdropInfo(seasonAirdropDecryptAck.Info);
			zbUIMgr.Instance.ShowSeasonAirdropDetail(seasonAirdropDecryptAck.Info);
		}
	}

	public void Season_Airdrop_Progress_Ntf(ByteString response)
	{
		SeasonAirdropProgressNtf seasonAirdropProgressNtf = SeasonAirdropProgressNtf.Parser.ParseFrom(response);
		if (seasonAirdropProgressNtf != null)
		{
			zbSelf.Instance.seasonHandler.UpdateSeasonAirdropProgress(seasonAirdropProgressNtf.DropId, seasonAirdropProgressNtf.CurProg, seasonAirdropProgressNtf.PersonalProg);
		}
	}

	public void Season_Airdrop_Settle_Ntf(ByteString response)
	{
		SeasonAirdropSettleNtf.Parser.ParseFrom(response);
	}

	public void Season_Airdrop_Enter_Ntf(ByteString response)
	{
		SeasonAirdropEnterNtf.Parser.ParseFrom(response);
	}

	public void Season_Force_Info_Req()
	{
		SeasonForceInfoReq req = new SeasonForceInfoReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonForceInfoReq);
	}

	public void Season_Force_Info_Ack(ByteString response)
	{
		SeasonForceInfoAck seasonForceInfoAck = SeasonForceInfoAck.Parser.ParseFrom(response);
		if (seasonForceInfoAck != null)
		{
			zbUIMgr.Instance.ShowSeasonForce(seasonForceInfoAck.Info);
		}
	}

	public void Season_Force_Update_Req()
	{
		SeasonForceUpdateReq req = new SeasonForceUpdateReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonForceUpdateReq);
	}

	public void Season_Force_Update_Ack(ByteString response)
	{
		SeasonForceUpdateAck.Parser.ParseFrom(response);
	}

	public void Season_Force_Members_Req()
	{
		SeasonForceMembersReq req = new SeasonForceMembersReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonForceMembersReq);
	}

	public void Season_Force_Members_Ack(ByteString response)
	{
		SeasonForceMembersAck seasonForceMembersAck = SeasonForceMembersAck.Parser.ParseFrom(response);
		if (seasonForceMembersAck != null)
		{
			zbSelf.Instance.seasonHandler.memberList = seasonForceMembersAck.List;
			zbUIMgr.Instance.ShowSeasonForceMembers();
		}
	}

	public void Season_Office_Appoint_Req(long TargetRoleId, ESeasonOffice Office)
	{
		SeasonOfficeAppointReq seasonOfficeAppointReq = new SeasonOfficeAppointReq();
		seasonOfficeAppointReq.TargetRoleId = TargetRoleId;
		seasonOfficeAppointReq.Office = Office;
		Instance.WsPackMsg((IMessage)(object)seasonOfficeAppointReq, Message.SeasonOfficeAppointReq);
	}

	public void Season_Office_Appoint_Ack(ByteString response)
	{
		SeasonOfficeAppointAck seasonOfficeAppointAck = SeasonOfficeAppointAck.Parser.ParseFrom(response);
		if (seasonOfficeAppointAck != null)
		{
			zbSelf.Instance.seasonHandler.UpdateSeasonMemberInfo(seasonOfficeAppointAck.TargetRoleId, seasonOfficeAppointAck.Office);
		}
	}

	public void Season_Leader_Elect_Info_Req()
	{
		SeasonLeaderElectInfoReq req = new SeasonLeaderElectInfoReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonLeaderElectInfoReq);
	}

	public void Season_Leader_Elect_Info_Ack(ByteString response)
	{
		SeasonLeaderElectInfoAck.Parser.ParseFrom(response);
	}

	public void Season_Leader_Impeach_Req()
	{
		SeasonLeaderImpeachReq req = new SeasonLeaderImpeachReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonLeaderImpeachReq);
	}

	public void Season_Leader_Impeach_Ack(ByteString response)
	{
		SeasonLeaderImpeachAck.Parser.ParseFrom(response);
	}

	public void Season_Leader_Change_Ntf(ByteString response)
	{
		SeasonLeaderChangeNtf.Parser.ParseFrom(response);
	}

	public void Season_Tech_Info_Req()
	{
		SeasonTechInfoReq req = new SeasonTechInfoReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonTechInfoReq);
	}

	public void Season_Tech_Info_Ack(ByteString response)
	{
		if (SeasonTechInfoAck.Parser.ParseFrom(response) != null)
		{
			zbUIMgr.Instance.ShowSeasonForceTech();
		}
	}

	public void Season_Tech_Upgrade_Req()
	{
		SeasonTechUpgradeReq req = new SeasonTechUpgradeReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonTechUpgradeReq);
	}

	public void Season_Tech_Upgrade_Ack(ByteString response)
	{
		SeasonTechUpgradeAck.Parser.ParseFrom(response);
	}

	public void Season_Shop_List_Req()
	{
		SeasonShopListReq req = new SeasonShopListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonShopListReq);
	}

	public void Season_Shop_List_Ack(ByteString response)
	{
		if (SeasonShopListAck.Parser.ParseFrom(response) != null)
		{
			zbUIMgr.Instance.ShowSeasonForceShop();
		}
	}

	public void Season_Shop_Buy_Req()
	{
		SeasonShopBuyReq req = new SeasonShopBuyReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SeasonShopBuyReq);
	}

	public void Season_Shop_Buy_Ack(ByteString response)
	{
		SeasonShopBuyAck.Parser.ParseFrom(response);
	}

	public void Season_Force_Exp_Ntf(ByteString response)
	{
		SeasonForceExpNtf.Parser.ParseFrom(response);
	}

	public void Servant_Data_Req()
	{
		ServantDataReq req = new ServantDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ServantDataReq);
	}

	public void Servant_Data_Ack(ByteString response)
	{
		ServantDataAck servantDataAck = ServantDataAck.Parser.ParseFrom(response);
		if (servantDataAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantDataAck.ServantInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Servant);
		}
	}

	public void Servant_UpgradeLevel_Req(string servantId)
	{
		ServantUpgradeLevelReq servantUpgradeLevelReq = new ServantUpgradeLevelReq();
		servantUpgradeLevelReq.ServantId = servantId;
		Instance.WsPackMsg((IMessage)(object)servantUpgradeLevelReq, Message.ServantUpgradeLevelReq);
	}

	public void Servant_UpgradeLevel_Ack(ByteString response)
	{
		ServantUpgradeLevelAck servantUpgradeLevelAck = ServantUpgradeLevelAck.Parser.ParseFrom(response);
		if (servantUpgradeLevelAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantUpgradeLevelAck.ServantInfo;
			zbSelf.Instance.dataHandler.DataChange(servantUpgradeLevelAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ServantUpgrade);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11064"));
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		}
	}

	public void Servant_UpgradeQuality_Req(string servantId, string costServantId)
	{
		ServantUpgradeQualityReq servantUpgradeQualityReq = new ServantUpgradeQualityReq();
		servantUpgradeQualityReq.ServantId = servantId;
		upgradeQualityServantDetail = ((IEnumerable<ServantDetail>)zbServerData.sGameData.ServantInfo.ServantList).FirstOrDefault((ServantDetail s) => s.ServantId == servantId);
		servantUpgradeQualityReq.CostServantId = costServantId;
		Instance.WsPackMsg((IMessage)(object)servantUpgradeQualityReq, Message.ServantUpgradeQualityReq);
	}

	public void Servant_UpgradeQuality_Ack(ByteString response)
	{
		ServantUpgradeQualityAck servantUpgradeQualityAck = ServantUpgradeQualityAck.Parser.ParseFrom(response);
		if (servantUpgradeQualityAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantUpgradeQualityAck.ServantInfo;
			zbSelf.Instance.dataHandler.DataChange(servantUpgradeQualityAck.DataChange, ((IEnumerable<AwardDetail>)servantUpgradeQualityAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ServantUpgrade);
			ServantDetail servantDetail = ((IEnumerable<ServantDetail>)zbServerData.sGameData.ServantInfo.ServantList).FirstOrDefault((ServantDetail s) => s.ServantId == upgradeQualityServantDetail.ServantId);
			if (upgradeQualityServantDetail.Quality < servantDetail.Quality)
			{
				zbUIMgr.Instance.ShowServantUpgradeQuality(upgradeQualityServantDetail);
			}
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		}
	}

	public void Servant_UpgradeSkill_Req(string servantId)
	{
		ServantUpgradeSkillReq servantUpgradeSkillReq = new ServantUpgradeSkillReq();
		servantUpgradeSkillReq.ServantId = servantId;
		Instance.WsPackMsg((IMessage)(object)servantUpgradeSkillReq, Message.ServantUpgradeSkillReq);
	}

	public void Servant_UpgradeSkill_Ack(ByteString response)
	{
		ServantUpgradeSkillAck servantUpgradeSkillAck = ServantUpgradeSkillAck.Parser.ParseFrom(response);
		if (servantUpgradeSkillAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantUpgradeSkillAck.ServantInfo;
			zbSelf.Instance.dataHandler.DataChange(servantUpgradeSkillAck.DataChange);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11064"));
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ServantUpgrade);
		}
	}

	public void Servant_UpgradeSubSkill_Req(string servantId)
	{
		ServantUpgradeSubSkillReq servantUpgradeSubSkillReq = new ServantUpgradeSubSkillReq();
		servantUpgradeSubSkillReq.ServantId = servantId;
		Instance.WsPackMsg((IMessage)(object)servantUpgradeSubSkillReq, Message.ServantUpgradeSubSkillReq);
	}

	public void Servant_UpgradeSubSkill_Ack(ByteString response)
	{
		ServantUpgradeSubSkillAck servantUpgradeSubSkillAck = ServantUpgradeSubSkillAck.Parser.ParseFrom(response);
		if (servantUpgradeSubSkillAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantUpgradeSubSkillAck.ServantInfo;
			zbSelf.Instance.dataHandler.DataChange(servantUpgradeSubSkillAck.DataChange);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11064"));
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ServantUpgrade);
		}
	}

	public void Servant_SubSkillFlush_Req(string servantId, int[] lockList)
	{
		ServantSubSkillFlushReq servantSubSkillFlushReq = new ServantSubSkillFlushReq();
		servantSubSkillFlushReq.ServantId = servantId;
		servantSubSkillFlushReq.LockList.Clear();
		servantSubSkillFlushReq.LockList.AddRange((IEnumerable<int>)lockList);
		Instance.WsPackMsg((IMessage)(object)servantSubSkillFlushReq, Message.ServantSubSkillFlushReq);
	}

	public void Servant_SubSkillFlush_Ack(ByteString response)
	{
		ServantSubSkillFlushAck servantSubSkillFlushAck = ServantSubSkillFlushAck.Parser.ParseFrom(response);
		if (servantSubSkillFlushAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantSubSkillFlushAck.ServantInfo;
			zbSelf.Instance.dataHandler.DataChange(servantSubSkillFlushAck.DataChange);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11070"));
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Servant);
		}
	}

	public void Servant_UpgradeAttr_Req(string servantId)
	{
		ServantUpgradeAttrReq servantUpgradeAttrReq = new ServantUpgradeAttrReq();
		servantUpgradeAttrReq.ServantId = servantId;
		Instance.WsPackMsg((IMessage)(object)servantUpgradeAttrReq, Message.ServantUpgradeAttrReq);
	}

	public void Servant_UpgradeAttr_Ack(ByteString response)
	{
		ServantUpgradeAttrAck servantUpgradeAttrAck = ServantUpgradeAttrAck.Parser.ParseFrom(response);
		if (servantUpgradeAttrAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantUpgradeAttrAck.ServantInfo;
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11069"));
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11070"));
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ServantUpgrade);
		}
	}

	public void Servant_Lock_Req(string servantId, EBoolState isLock)
	{
		ServantLockReq servantLockReq = new ServantLockReq();
		servantLockReq.ServantId = servantId;
		servantLockReq.Lock = isLock;
		servantIsLock = isLock;
		Instance.WsPackMsg((IMessage)(object)servantLockReq, Message.ServantLockReq);
	}

	public void Servant_Lock_Ack(ByteString response)
	{
		ServantLockAck servantLockAck = ServantLockAck.Parser.ParseFrom(response);
		if (servantLockAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantLockAck.ServantInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Servant);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language((servantIsLock == EBoolState.True) ? "lang.10501" : "lang.11059"));
		}
	}

	public void Servant_Deploy_Req(string servantId)
	{
		ServantDeployReq servantDeployReq = new ServantDeployReq();
		servantDeployReq.ServantId = servantId;
		Instance.WsPackMsg((IMessage)(object)servantDeployReq, Message.ServantDeployReq);
	}

	public void Servant_Deploy_Ack(ByteString response)
	{
		ServantDeployAck servantDeployAck = ServantDeployAck.Parser.ParseFrom(response);
		if (servantDeployAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantDeployAck.ServantInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Servant);
		}
	}

	public void Servant_DecomposeCalc_Req(string servantId)
	{
		ServantDecomposeCalcReq servantDecomposeCalcReq = new ServantDecomposeCalcReq();
		servantDecomposeCalcReq.ServantId = servantId;
		Instance.WsPackMsg((IMessage)(object)servantDecomposeCalcReq, Message.ServantDecomposeCalcReq);
	}

	public void Servant_DecomposeCalc_Ack(ByteString response)
	{
		ServantDecomposeCalcAck.Parser.ParseFrom(response);
	}

	public void Servant_Decompose_Req(string servantId)
	{
		ServantDecomposeReq servantDecomposeReq = new ServantDecomposeReq();
		servantDecomposeReq.ServantId = servantId;
		Instance.WsPackMsg((IMessage)(object)servantDecomposeReq, Message.ServantDecomposeReq);
	}

	public void Servant_Decompose_Ack(ByteString response)
	{
		ServantDecomposeAck servantDecomposeAck = ServantDecomposeAck.Parser.ParseFrom(response);
		if (servantDecomposeAck != null)
		{
			zbServerData.sGameData.ServantInfo = servantDecomposeAck.ServantInfo;
			zbSelf.Instance.dataHandler.DataChange(servantDecomposeAck.DataChange, ((IEnumerable<AwardDetail>)servantDecomposeAck.AwardList).ToArray());
		}
	}

	public void Incubate_Data_Req()
	{
		IncubateDataReq req = new IncubateDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.IncubateDataReq);
	}

	public void Incubate_Data_Ack(ByteString response)
	{
		IncubateDataAck incubateDataAck = IncubateDataAck.Parser.ParseFrom(response);
		if (incubateDataAck != null)
		{
			zbServerData.sGameData.IncubateInfo = incubateDataAck.IncubateInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Incubate);
		}
	}

	public void Incubate_SlotUnlock_Req()
	{
		IncubateSlotUnlockReq req = new IncubateSlotUnlockReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.IncubateSlotUnlockReq);
	}

	public void Incubate_SlotUnlock_Ack(ByteString response)
	{
		IncubateSlotUnlockAck incubateSlotUnlockAck = IncubateSlotUnlockAck.Parser.ParseFrom(response);
		if (incubateSlotUnlockAck != null)
		{
			zbServerData.sGameData.IncubateInfo = incubateSlotUnlockAck.IncubateInfo;
			zbSelf.Instance.dataHandler.DataChange(incubateSlotUnlockAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Incubate);
		}
	}

	public void Incubate_Start_Req(int index, int propId)
	{
		IncubateStartReq incubateStartReq = new IncubateStartReq();
		incubateStartReq.Index = index;
		incubateStartReq.PropId = propId;
		Instance.WsPackMsg((IMessage)(object)incubateStartReq, Message.IncubateStartReq);
	}

	public void Incubate_Start_Ack(ByteString response)
	{
		IncubateStartAck incubateStartAck = IncubateStartAck.Parser.ParseFrom(response);
		if (incubateStartAck != null)
		{
			zbServerData.sGameData.IncubateInfo = incubateStartAck.IncubateInfo;
			zbSelf.Instance.dataHandler.DataChange(incubateStartAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Incubate);
		}
	}

	public void Incubate_Speed_Req(int index)
	{
		IncubateSpeedReq incubateSpeedReq = new IncubateSpeedReq();
		incubateSpeedReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)incubateSpeedReq, Message.IncubateSpeedReq);
	}

	public void Incubate_Speed_Ack(ByteString response)
	{
		IncubateSpeedAck incubateSpeedAck = IncubateSpeedAck.Parser.ParseFrom(response);
		if (incubateSpeedAck != null)
		{
			zbServerData.sGameData.IncubateInfo = incubateSpeedAck.IncubateInfo;
			zbSelf.Instance.dataHandler.DataChange(incubateSpeedAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Incubate);
			zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		}
	}

	public void Incubate_Complete_Req(int index)
	{
		IncubateCompleteReq incubateCompleteReq = new IncubateCompleteReq();
		incubateCompleteReq.Index = index;
		Instance.WsPackMsg((IMessage)(object)incubateCompleteReq, Message.IncubateCompleteReq);
	}

	public void Incubate_Complete_Ack(ByteString response)
	{
		IncubateCompleteAck incubateCompleteAck = IncubateCompleteAck.Parser.ParseFrom(response);
		if (incubateCompleteAck != null)
		{
			RepeatedField<ServantDetail> servantList = zbServerData.sGameData.ServantInfo.ServantList;
			RepeatedField<ServantDetail> servantList2 = incubateCompleteAck.ServantInfo.ServantList;
			HashSet<string> aIdSet = new HashSet<string>(((IEnumerable<ServantDetail>)servantList).Select((ServantDetail x) => x.ServantId));
			ServantDetail servantDetail = ((IEnumerable<ServantDetail>)servantList2).Where((ServantDetail x) => !aIdSet.Contains(x.ServantId)).FirstOrDefault();
			zbServerData.sGameData.ServantInfo = incubateCompleteAck.ServantInfo;
			zbServerData.sGameData.IncubateInfo = incubateCompleteAck.IncubateInfo;
			zbSelf.Instance.dataHandler.DataChange(incubateCompleteAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Incubate);
			if (servantDetail != null)
			{
				zbUIMgr.Instance.ShowServantGet(servantDetail);
			}
		}
	}

	public void Shop_Data_Req()
	{
		ShopDataReq req = new ShopDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ShopDataReq);
	}

	public void Shop_Data_Ack(ByteString response)
	{
		ShopDataAck shopDataAck = ShopDataAck.Parser.ParseFrom(response);
		if (shopDataAck != null)
		{
			zbSelf.Instance.shopHandler.UpdateShopList(((IEnumerable<ShopDetail>)shopDataAck.ShopList).ToArray());
		}
	}

	public void Shop_Buy_Req(EShopType ShopType, int Index, int Count)
	{
		ShopBuyReq shopBuyReq = new ShopBuyReq();
		shopBuyReq.ShopType = ShopType;
		shopBuyReq.Index = Index;
		shopBuyReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)shopBuyReq, Message.ShopBuyReq);
	}

	public void Shop_Buy_Ack(ByteString response)
	{
		ShopBuyAck shopBuyAck = ShopBuyAck.Parser.ParseFrom(response);
		if (shopBuyAck != null)
		{
			zbSelf.Instance.shopHandler.UpdateShopList(((IEnumerable<ShopDetail>)shopBuyAck.ShopList).ToArray());
			zbSelf.Instance.dataHandler.DataChange(shopBuyAck.DataChange, ((IEnumerable<AwardDetail>)shopBuyAck.AwardList).ToArray());
		}
	}

	public void RechargeActive_DailyPack_Req(int id)
	{
		RechargeActiveDailyPackReq rechargeActiveDailyPackReq = new RechargeActiveDailyPackReq();
		rechargeActiveDailyPackReq.Id = id;
		Instance.WsPackMsg((IMessage)(object)rechargeActiveDailyPackReq, Message.RechargeActiveDailyPackReq);
	}

	public void RechargeActive_DailyPack_Ack(ByteString response)
	{
		RechargeActiveDailyPackAck rechargeActiveDailyPackAck = RechargeActiveDailyPackAck.Parser.ParseFrom(response);
		if (rechargeActiveDailyPackAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveDailyPackAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeActiveDailyPackAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveDailyPackAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DailyPack_Ack);
		}
	}

	public void RechargeActive_DailyPackAll_Req()
	{
	}

	public void RechargeActive_DailyPackAll_Ack(ByteString response)
	{
	}

	public void Operate_SnowVeilActivate_Req(string uuid)
	{
		OperateSnowVeilActivateReq operateSnowVeilActivateReq = new OperateSnowVeilActivateReq();
		operateSnowVeilActivateReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateSnowVeilActivateReq, Message.OperateSnowVeilActivateReq);
	}

	public void Operate_SnowVeilActivate_Ack(ByteString response)
	{
		OperateSnowVeilActivateAck operateSnowVeilActivateAck = OperateSnowVeilActivateAck.Parser.ParseFrom(response);
		if (operateSnowVeilActivateAck != null)
		{
			zbServerData.sGameData.OperateInfo.SnowVeilList.Clear();
			zbServerData.sGameData.OperateInfo.SnowVeilList.AddRange((IEnumerable<SnowVeilDetail>)operateSnowVeilActivateAck.SnowVeilList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilActivate_Ack, operateSnowVeilActivateAck);
		}
	}

	public void Operate_SnowVeilTarget_Req(string uuid)
	{
		OperateSnowVeilTargetReq operateSnowVeilTargetReq = new OperateSnowVeilTargetReq();
		operateSnowVeilTargetReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateSnowVeilTargetReq, Message.OperateSnowVeilTargetReq);
	}

	public void Operate_SnowVeilTarget_Ack(ByteString response)
	{
		OperateSnowVeilTargetAck operateSnowVeilTargetAck = OperateSnowVeilTargetAck.Parser.ParseFrom(response);
		if (operateSnowVeilTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.SnowVeilList.Clear();
			zbServerData.sGameData.OperateInfo.SnowVeilList.AddRange((IEnumerable<SnowVeilDetail>)operateSnowVeilTargetAck.SnowVeilList);
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateSnowVeilTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilTarget_Ack);
		}
	}

	public void Operate_SnowVeilGo_Req(string uuid, int factor)
	{
		OperateSnowVeilGoReq operateSnowVeilGoReq = new OperateSnowVeilGoReq();
		operateSnowVeilGoReq.UUID = uuid;
		operateSnowVeilGoReq.Factor = factor;
		zbSelf.Instance.snowVeilHandler.m_goAckfactor = factor;
		Instance.WsPackMsg((IMessage)(object)operateSnowVeilGoReq, Message.OperateSnowVeilGoReq);
	}

	public void Operate_SnowVeilGo_Ack(ByteString response)
	{
		OperateSnowVeilGoAck operateSnowVeilGoAck = OperateSnowVeilGoAck.Parser.ParseFrom(response);
		if (operateSnowVeilGoAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilGoAck.DataChange);
			zbSelf.Instance.snowVeilHandler.UpdateSnowVeilDetail(operateSnowVeilGoAck.UUID, -1, operateSnowVeilGoAck.RoleState, ((IEnumerable<SnowVeilLandDetail>)operateSnowVeilGoAck.LandList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilGo_Ack);
		}
	}

	public void Operate_SnowVeilEventItem_Ack(ByteString response)
	{
		OperateSnowVeilEventItemAck operateSnowVeilEventItemAck = OperateSnowVeilEventItemAck.Parser.ParseFrom(response);
		if (operateSnowVeilEventItemAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilEventItemAck.DataChange);
			zbSelf.Instance.snowVeilHandler.UpdateSnowVeilDetail(operateSnowVeilEventItemAck.UUID, -1, operateSnowVeilEventItemAck.RoleState, ((IEnumerable<SnowVeilLandDetail>)operateSnowVeilEventItemAck.LandList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilGo_Ack);
		}
	}

	public void Operate_SnowVeilEventTurntable_Ack(ByteString response)
	{
		OperateSnowVeilEventTurntableAck operateSnowVeilEventTurntableAck = OperateSnowVeilEventTurntableAck.Parser.ParseFrom(response);
		if (operateSnowVeilEventTurntableAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilEventTurntableAck.DataChange);
			zbSelf.Instance.snowVeilHandler.UpdateSnowVeilDetail(operateSnowVeilEventTurntableAck.UUID, -1, operateSnowVeilEventTurntableAck.RoleState, ((IEnumerable<SnowVeilLandDetail>)operateSnowVeilEventTurntableAck.LandList).ToArray(), operateSnowVeilEventTurntableAck.Turntable);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilGo_Ack);
		}
	}

	public void Operate_SnowVeilEventRolling_Ack(ByteString response)
	{
		OperateSnowVeilEventRollingAck operateSnowVeilEventRollingAck = OperateSnowVeilEventRollingAck.Parser.ParseFrom(response);
		if (operateSnowVeilEventRollingAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilEventRollingAck.DataChange);
			zbSelf.Instance.snowVeilHandler.UpdateSnowVeilDetail(operateSnowVeilEventRollingAck.UUID, -1, operateSnowVeilEventRollingAck.RoleState, ((IEnumerable<SnowVeilLandDetail>)operateSnowVeilEventRollingAck.LandList).ToArray(), null, operateSnowVeilEventRollingAck.Rolling);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilGo_Ack);
		}
	}

	public void Operate_SnowVeilEventScore_Ack(ByteString response)
	{
		OperateSnowVeilEventScoreAck operateSnowVeilEventScoreAck = OperateSnowVeilEventScoreAck.Parser.ParseFrom(response);
		if (operateSnowVeilEventScoreAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilEventScoreAck.DataChange);
			zbSelf.Instance.snowVeilHandler.UpdateSnowVeilDetail(operateSnowVeilEventScoreAck.UUID, operateSnowVeilEventScoreAck.TotalScore, operateSnowVeilEventScoreAck.RoleState, ((IEnumerable<SnowVeilLandDetail>)operateSnowVeilEventScoreAck.LandList).ToArray(), null, null, operateSnowVeilEventScoreAck.TargetDetail, operateSnowVeilEventScoreAck.TargetNextGrand);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilGo_Ack);
		}
	}

	public void Operate_SnowVeilEventHighScore_Ack(ByteString response)
	{
		OperateSnowVeilEventHighScoreAck operateSnowVeilEventHighScoreAck = OperateSnowVeilEventHighScoreAck.Parser.ParseFrom(response);
		if (operateSnowVeilEventHighScoreAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilEventHighScoreAck.DataChange);
			zbSelf.Instance.snowVeilHandler.UpdateSnowVeilDetail(operateSnowVeilEventHighScoreAck.UUID, operateSnowVeilEventHighScoreAck.TotalScore, operateSnowVeilEventHighScoreAck.RoleState, ((IEnumerable<SnowVeilLandDetail>)operateSnowVeilEventHighScoreAck.LandList).ToArray(), null, null, operateSnowVeilEventHighScoreAck.TargetDetail, operateSnowVeilEventHighScoreAck.TargetNextGrand);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilGo_Ack);
		}
	}

	public void Operate_SnowVeilRolling_Req(string uuid)
	{
		OperateSnowVeilRollingReq operateSnowVeilRollingReq = new OperateSnowVeilRollingReq();
		operateSnowVeilRollingReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateSnowVeilRollingReq, Message.OperateSnowVeilRollingReq);
	}

	public void Operate_SnowVeilRolling_Ack(ByteString response)
	{
		OperateSnowVeilRollingAck operateSnowVeilRollingAck = OperateSnowVeilRollingAck.Parser.ParseFrom(response);
		if (operateSnowVeilRollingAck != null)
		{
			zbServerData.sGameData.OperateInfo.SnowVeilList.Clear();
			zbServerData.sGameData.OperateInfo.SnowVeilList.AddRange((IEnumerable<SnowVeilDetail>)operateSnowVeilRollingAck.SnowVeilList);
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilRollingAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilRolling_Ack, operateSnowVeilRollingAck);
		}
	}

	public void Operate_SnowVeilRollingEnd_Req(string uuid)
	{
		OperateSnowVeilRollingEndReq operateSnowVeilRollingEndReq = new OperateSnowVeilRollingEndReq();
		operateSnowVeilRollingEndReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateSnowVeilRollingEndReq, Message.OperateSnowVeilRollingEndReq);
	}

	public void Operate_SnowVeilRollingEnd_Ack(ByteString response)
	{
		OperateSnowVeilRollingEndAck operateSnowVeilRollingEndAck = OperateSnowVeilRollingEndAck.Parser.ParseFrom(response);
		if (operateSnowVeilRollingEndAck != null)
		{
			zbServerData.sGameData.OperateInfo.SnowVeilList.Clear();
			zbServerData.sGameData.OperateInfo.SnowVeilList.AddRange((IEnumerable<SnowVeilDetail>)operateSnowVeilRollingEndAck.SnowVeilList);
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilRollingEndAck.DataChange, ((IEnumerable<AwardDetail>)operateSnowVeilRollingEndAck.AwardList).ToArray());
			zbUIMgr.Instance.HideWnd(EWndId.SnowVeilRolling);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_SnowVeilDetail);
		}
	}

	public void Operate_SnowVeilTurntable_Req(string uuid)
	{
		OperateSnowVeilTurntableReq operateSnowVeilTurntableReq = new OperateSnowVeilTurntableReq();
		operateSnowVeilTurntableReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateSnowVeilTurntableReq, Message.OperateSnowVeilTurntableReq);
	}

	public void Operate_SnowVeilTurntable_Ack(ByteString response)
	{
		OperateSnowVeilTurntableAck operateSnowVeilTurntableAck = OperateSnowVeilTurntableAck.Parser.ParseFrom(response);
		if (operateSnowVeilTurntableAck != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SnowVeilTurntable_Ack, operateSnowVeilTurntableAck);
		}
	}

	public void Operate_SnowVeilPropBuy_Req(string uuid)
	{
		OperateSnowVeilPropBuyReq operateSnowVeilPropBuyReq = new OperateSnowVeilPropBuyReq();
		operateSnowVeilPropBuyReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)operateSnowVeilPropBuyReq, Message.OperateSnowVeilPropBuyReq);
	}

	public void Operate_SnowVeilPropBuy_Ack(ByteString response)
	{
		OperateSnowVeilPropBuyAck operateSnowVeilPropBuyAck = OperateSnowVeilPropBuyAck.Parser.ParseFrom(response);
		if (operateSnowVeilPropBuyAck != null)
		{
			zbServerData.sGameData.OperateInfo.SnowVeilList.Clear();
			zbServerData.sGameData.OperateInfo.SnowVeilList.AddRange((IEnumerable<SnowVeilDetail>)operateSnowVeilPropBuyAck.SnowVeilList);
			zbSelf.Instance.dataHandler.DataChange(operateSnowVeilPropBuyAck.DataChange, ((IEnumerable<AwardDetail>)operateSnowVeilPropBuyAck.AwardList).ToArray());
		}
	}

	public void Suit_Active_Req(int SuitId)
	{
		SuitActiveReq suitActiveReq = new SuitActiveReq();
		suitActiveReq.SuitId = SuitId;
		Instance.WsPackMsg((IMessage)(object)suitActiveReq, Message.SuitActiveReq);
	}

	public void Suit_Active_Ack(ByteString response)
	{
		SuitActiveAck suitActiveAck = SuitActiveAck.Parser.ParseFrom(response);
		if (suitActiveAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitActiveAck.SuitInfo;
			zbServerData.sGameData.HeadList.Clear();
			zbServerData.sGameData.HeadList.AddRange((IEnumerable<int>)suitActiveAck.HeadList);
			zbSelf.Instance.dataHandler.DataChange(suitActiveAck.DataChange);
			zbSelf.Instance.newTagHandler.AddNewSuit(suitActiveAck.SuitId);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Suit_Refresh);
			zbUIMgr.Instance.ShowPlayerSuitGet(suitActiveAck.SuitId);
		}
	}

	public void Suit_Set_Req(int SuitId)
	{
		SuitSetReq suitSetReq = new SuitSetReq();
		suitSetReq.SuitId = SuitId;
		Instance.WsPackMsg((IMessage)(object)suitSetReq, Message.SuitSetReq);
	}

	public void Suit_Set_Ack(ByteString response)
	{
		SuitSetAck suitSetAck = SuitSetAck.Parser.ParseFrom(response);
		if (suitSetAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitSetAck.SuitInfo;
			zbSelf.Instance.dataHandler.DataChange(suitSetAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Suit_Refresh);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10597"));
		}
	}

	public void Suit_Remove_Req()
	{
		SuitRemoveReq req = new SuitRemoveReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.SuitRemoveReq);
	}

	public void Suit_Remove_Ack(ByteString response)
	{
		SuitRemoveAck suitRemoveAck = SuitRemoveAck.Parser.ParseFrom(response);
		if (suitRemoveAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitRemoveAck.SuitInfo;
			zbSelf.Instance.dataHandler.DataChange(suitRemoveAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Suit_Refresh);
		}
	}

	public void Suit_UpgradeStar_Req(int SuitId)
	{
		SuitUpgradeStarReq suitUpgradeStarReq = new SuitUpgradeStarReq();
		suitUpgradeStarReq.SuitId = SuitId;
		curStarSuitId = SuitId;
		Instance.WsPackMsg((IMessage)(object)suitUpgradeStarReq, Message.SuitUpgradeStarReq);
	}

	public void Suit_UpgradeStar_Ack(ByteString response)
	{
		SuitUpgradeStarAck suitUpgradeStarAck = SuitUpgradeStarAck.Parser.ParseFrom(response);
		if (suitUpgradeStarAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitUpgradeStarAck.SuitInfo;
			zbSelf.Instance.dataHandler.DataChange(suitUpgradeStarAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Suit_Refresh);
			zbUIMgr.Instance.ShowPlayerSuitStarSuccess(curStarSuitId);
		}
	}

	public void Suit_Convert_Req(int[] SuitIds)
	{
		SuitConvertReq suitConvertReq = new SuitConvertReq();
		suitConvertReq.SelectList.Clear();
		suitConvertReq.SelectList.AddRange((IEnumerable<int>)SuitIds);
		Instance.WsPackMsg((IMessage)(object)suitConvertReq, Message.SuitConvertReq);
	}

	public void Suit_Convert_Ack(ByteString response)
	{
		SuitConvertAck suitConvertAck = SuitConvertAck.Parser.ParseFrom(response);
		if (suitConvertAck != null)
		{
			zbSelf.Instance.suitGunHandler.suitConvertAck = suitConvertAck;
			zbSelf.Instance.dataHandler.DataChange(suitConvertAck.DataChange, ((IEnumerable<AwardDetail>)suitConvertAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Suit_Refresh);
		}
	}

	public void Suit_SkillSet_Req(int Index, int SuitId)
	{
		SuitSkillSetReq suitSkillSetReq = new SuitSkillSetReq();
		suitSkillSetReq.Index = Index;
		suitSkillSetReq.SuitId = SuitId;
		Instance.WsPackMsg((IMessage)(object)suitSkillSetReq, Message.SuitSkillSetReq);
	}

	public void Suit_SkillSet_Ack(ByteString response)
	{
		SuitSkillSetAck suitSkillSetAck = SuitSkillSetAck.Parser.ParseFrom(response);
		if (suitSkillSetAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitSkillSetAck.SuitInfo;
			zbSelf.Instance.dataHandler.DataChange(suitSkillSetAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SuitSkill_Refresh);
		}
	}

	public void Suit_SkillRemove_Req(int Index)
	{
		SuitSkillRemoveReq suitSkillRemoveReq = new SuitSkillRemoveReq();
		suitSkillRemoveReq.Index = Index;
		Instance.WsPackMsg((IMessage)(object)suitSkillRemoveReq, Message.SuitSkillRemoveReq);
	}

	public void Suit_SkillRemove_Ack(ByteString response)
	{
		SuitSkillRemoveAck suitSkillRemoveAck = SuitSkillRemoveAck.Parser.ParseFrom(response);
		if (suitSkillRemoveAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitSkillRemoveAck.SuitInfo;
			zbSelf.Instance.dataHandler.DataChange(suitSkillRemoveAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SuitSkill_Refresh);
		}
	}

	public void Suit_SuperSkillSet_Req(int Index, int SuitId)
	{
		SuitSuperSkillSetReq suitSuperSkillSetReq = new SuitSuperSkillSetReq();
		suitSuperSkillSetReq.Index = Index;
		suitSuperSkillSetReq.SuitId = SuitId;
		Instance.WsPackMsg((IMessage)(object)suitSuperSkillSetReq, Message.SuitSuperSkillSetReq);
	}

	public void Suit_SuperSkillSet_Ack(ByteString response)
	{
		SuitSuperSkillSetAck suitSuperSkillSetAck = SuitSuperSkillSetAck.Parser.ParseFrom(response);
		if (suitSuperSkillSetAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitSuperSkillSetAck.SuitInfo;
			zbSelf.Instance.dataHandler.DataChange(suitSuperSkillSetAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SuitSkill_Refresh);
		}
	}

	public void Suit_SuperSkillRemove_Req(int Index)
	{
		SuitSuperSkillRemoveReq suitSuperSkillRemoveReq = new SuitSuperSkillRemoveReq();
		suitSuperSkillRemoveReq.Index = Index;
		Instance.WsPackMsg((IMessage)(object)suitSuperSkillRemoveReq, Message.SuitSuperSkillRemoveReq);
	}

	public void Suit_SuperSkillRemove_Ack(ByteString response)
	{
		SuitSuperSkillRemoveAck suitSuperSkillRemoveAck = SuitSuperSkillRemoveAck.Parser.ParseFrom(response);
		if (suitSuperSkillRemoveAck != null)
		{
			zbServerData.sGameData.SuitInfo = suitSuperSkillRemoveAck.SuitInfo;
			zbSelf.Instance.dataHandler.DataChange(suitSuperSkillRemoveAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SuitSkill_Refresh);
		}
	}

	public void Gun_Active_Req(int GunId)
	{
		GunActiveReq gunActiveReq = new GunActiveReq();
		gunActiveReq.GunId = GunId;
		Instance.WsPackMsg((IMessage)(object)gunActiveReq, Message.GunActiveReq);
	}

	public void Gun_Active_Ack(ByteString response)
	{
		GunActiveAck gunActiveAck = GunActiveAck.Parser.ParseFrom(response);
		if (gunActiveAck != null)
		{
			zbServerData.sGameData.GunInfo = gunActiveAck.GunInfo;
			zbSelf.Instance.dataHandler.DataChange(gunActiveAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Gun_Refresh);
			zbUIMgr.Instance.ShowPlayerGunGet(gunActiveAck.GunId);
		}
	}

	public void Gun_Set_Req(int GunId)
	{
		GunSetReq gunSetReq = new GunSetReq();
		gunSetReq.GunId = GunId;
		Instance.WsPackMsg((IMessage)(object)gunSetReq, Message.GunSetReq);
	}

	public void Gun_Set_Ack(ByteString response)
	{
		GunSetAck gunSetAck = GunSetAck.Parser.ParseFrom(response);
		if (gunSetAck != null)
		{
			zbServerData.sGameData.GunInfo = gunSetAck.GunInfo;
			zbSelf.Instance.dataHandler.DataChange(gunSetAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Gun_Refresh);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Suit_Refresh);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10597"));
		}
	}

	public void Gun_Remove_Req()
	{
		GunRemoveReq req = new GunRemoveReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.GunRemoveReq);
	}

	public void Gun_Remove_Ack(ByteString response)
	{
		GunRemoveAck gunRemoveAck = GunRemoveAck.Parser.ParseFrom(response);
		if (gunRemoveAck != null)
		{
			zbServerData.sGameData.GunInfo = gunRemoveAck.GunInfo;
			zbSelf.Instance.dataHandler.DataChange(gunRemoveAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Gun_Refresh);
		}
	}

	public void Gun_UpgradeStar_Req(int GunId)
	{
		GunUpgradeStarReq gunUpgradeStarReq = new GunUpgradeStarReq();
		gunUpgradeStarReq.GunId = GunId;
		curStarGunId = GunId;
		Instance.WsPackMsg((IMessage)(object)gunUpgradeStarReq, Message.GunUpgradeStarReq);
	}

	public void Gun_UpgradeStar_Ack(ByteString response)
	{
		GunUpgradeStarAck gunUpgradeStarAck = GunUpgradeStarAck.Parser.ParseFrom(response);
		if (gunUpgradeStarAck != null)
		{
			zbServerData.sGameData.GunInfo = gunUpgradeStarAck.GunInfo;
			zbSelf.Instance.dataHandler.DataChange(gunUpgradeStarAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Gun_Refresh);
			zbUIMgr.Instance.ShowPlayerGunStarSuccess(curStarGunId);
		}
	}

	public void Gun_Convert_Req(int[] GunIds)
	{
		GunConvertReq gunConvertReq = new GunConvertReq();
		gunConvertReq.SelectList.Clear();
		gunConvertReq.SelectList.AddRange((IEnumerable<int>)GunIds);
		Instance.WsPackMsg((IMessage)(object)gunConvertReq, Message.GunConvertReq);
	}

	public void Gun_Convert_Ack(ByteString response)
	{
		GunConvertAck gunConvertAck = GunConvertAck.Parser.ParseFrom(response);
		if (gunConvertAck != null)
		{
			zbSelf.Instance.suitGunHandler.gunConvertAck = gunConvertAck;
			zbSelf.Instance.dataHandler.DataChange(gunConvertAck.DataChange, ((IEnumerable<AwardDetail>)gunConvertAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Gun_Refresh);
		}
	}

	public void Gun_SetBullet_Req(int GunId, int bulletId)
	{
		GunSetBulletReq gunSetBulletReq = new GunSetBulletReq();
		gunSetBulletReq.GunId = GunId;
		gunSetBulletReq.BulletId = bulletId;
		Instance.WsPackMsg((IMessage)(object)gunSetBulletReq, Message.GunSetBulletReq);
	}

	public void Gun_SetBullet_Ack(ByteString response)
	{
		GunSetBulletAck gunSetBulletAck = GunSetBulletAck.Parser.ParseFrom(response);
		if (gunSetBulletAck != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10068"));
			zbServerData.sGameData.GunInfo = gunSetBulletAck.GunInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Gun_Refresh);
		}
	}

	public void Tactic_Compose_Req()
	{
		TacticComposeReq req = new TacticComposeReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TacticComposeReq);
	}

	private void Tactic_Compose_Ack(ByteString response)
	{
		TacticComposeAck.Parser.ParseFrom(response);
	}

	public void Tactic_StarUpgrade_Req()
	{
		TacticStarUpgradeReq req = new TacticStarUpgradeReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TacticStarUpgradeReq);
	}

	private void Tactic_StarUpgrade_Ack(ByteString response)
	{
		TacticStarUpgradeAck.Parser.ParseFrom(response);
	}

	public void Tactic_Convert_Req()
	{
		TacticConvertReq req = new TacticConvertReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TacticConvertReq);
	}

	private void Tactic_Convert_Ack(ByteString response)
	{
		TacticConvertAck.Parser.ParseFrom(response);
	}

	public void Peerage_Data_Req()
	{
		PeerageDataReq req = new PeerageDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PeerageDataReq);
	}

	public void Peerage_Data_Ack(ByteString response)
	{
		PeerageDataAck peerageDataAck = PeerageDataAck.Parser.ParseFrom(response);
		if (peerageDataAck != null)
		{
			zbServerData.sGameData.PeerageInfo = peerageDataAck.PeerageInfo;
			zbSelf.Instance.taskHandler.UpdatePeerageInfos();
		}
	}

	public void Peerage_Upgrade_Req()
	{
		PeerageUpgradeReq req = new PeerageUpgradeReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.PeerageUpgradeReq);
	}

	public void Peerage_Upgrade_Ack(ByteString response)
	{
		PeerageUpgradeAck peerageUpgradeAck = PeerageUpgradeAck.Parser.ParseFrom(response);
		if (peerageUpgradeAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(peerageUpgradeAck.DataChange);
			zbServerData.sGameData.PeerageInfo = peerageUpgradeAck.PeerageInfo;
			zbSelf.Instance.taskHandler.UpdatePeerageInfos();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Peerage_Upgrade);
			zbSelf.Instance.funcUnLockHandler.UpdateFunctionUnlockRecords(peerageUpgradeAck);
			zbUIMgr.Instance.ShowPeerageInfo(showPeerageup: true);
			if (zbUIMgr.Instance.IsShow(EWndId.MainLevel))
			{
				zbUIMgr.Instance.GetWndById(EWndId.MainLevel).m_MainBottom.Refresh();
			}
			zbProgressEventHelper.Instance.SetProgressEvent4(peerageUpgradeAck.PeerageInfo.PeerageId);
			zbGuide.Instance.CheckNewGuideStep();
		}
	}

	public void Peerage_UpgradeTask_Req(int taskId)
	{
		PeerageUpgradeTaskReq peerageUpgradeTaskReq = new PeerageUpgradeTaskReq();
		peerageUpgradeTaskReq.TaskId = taskId;
		Instance.WsPackMsg((IMessage)(object)peerageUpgradeTaskReq, Message.PeerageUpgradeTaskReq);
	}

	public void Peerage_UpgradeTask_Ack(ByteString response)
	{
		PeerageUpgradeTaskAck peerageUpgradeTaskAck = PeerageUpgradeTaskAck.Parser.ParseFrom(response);
		if (peerageUpgradeTaskAck != null)
		{
			zbServerData.sGameData.PeerageInfo = peerageUpgradeTaskAck.PeerageInfo;
			zbSelf.Instance.taskHandler.UpdatePeerageInfos();
			zbSelf.Instance.dataHandler.DataChange(peerageUpgradeTaskAck.DataChange, ((IEnumerable<AwardDetail>)peerageUpgradeTaskAck.AwardList).ToArray());
		}
	}

	public void DailyTask_Data_Req()
	{
		DailyTaskDataReq req = new DailyTaskDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.DailyTaskDataReq);
	}

	public void DailyTask_Data_Ack(ByteString response)
	{
		DailyTaskDataAck dailyTaskDataAck = DailyTaskDataAck.Parser.ParseFrom(response);
		if (dailyTaskDataAck != null)
		{
			zbServerData.sGameData.DailyTaskInfo = dailyTaskDataAck.DailyTaskInfo;
			zbSelf.Instance.taskHandler.UpdateDailyTaskInfo();
			zbPeerageWnd zbPeerageWnd2 = zbUIMgr.Instance.GetWndById(EWndId.Peerage) as zbPeerageWnd;
			if ((Object)(object)zbPeerageWnd2 != (Object)null)
			{
				zbPeerageWnd2.RefreshDailyTask();
			}
		}
	}

	public void DailyTask_Award_Req(ETaskType _taskType)
	{
		DailyTaskAwardReq dailyTaskAwardReq = new DailyTaskAwardReq();
		dailyTaskAwardReq.TaskType = _taskType;
		Instance.WsPackMsg((IMessage)(object)dailyTaskAwardReq, Message.DailyTaskAwardReq);
	}

	public void DailyTask_Award_Ack(ByteString response)
	{
		DailyTaskAwardAck dailyTaskAwardAck = DailyTaskAwardAck.Parser.ParseFrom(response);
		if (dailyTaskAwardAck != null)
		{
			zbServerData.sGameData.DailyTaskInfo = dailyTaskAwardAck.DailyTaskInfo;
			zbSelf.Instance.dataHandler.DataChange(dailyTaskAwardAck.DataChange, ((IEnumerable<AwardDetail>)dailyTaskAwardAck.AwardList).ToArray());
			zbSelf.Instance.taskHandler.UpdateDailyTaskInfo();
			if (zbSelf.Instance.journeyTwoHandler.IsActive())
			{
				Instance.Journey_Data_Req();
			}
		}
	}

	public void DailyTask_TargetAward_Req(int _active)
	{
		DailyTaskTargetAwardReq dailyTaskTargetAwardReq = new DailyTaskTargetAwardReq();
		dailyTaskTargetAwardReq.Active = _active;
		Instance.WsPackMsg((IMessage)(object)dailyTaskTargetAwardReq, Message.DailyTaskTargetAwardReq);
	}

	public void DailyTask_TargetAward_Ack(ByteString response)
	{
		DailyTaskTargetAwardAck dailyTaskTargetAwardAck = DailyTaskTargetAwardAck.Parser.ParseFrom(response);
		if (dailyTaskTargetAwardAck != null)
		{
			zbServerData.sGameData.DailyTaskInfo = dailyTaskTargetAwardAck.DailyTaskInfo;
			zbSelf.Instance.taskHandler.UpdateDailyTaskInfo();
			zbSelf.Instance.dataHandler.DataChange(dailyTaskTargetAwardAck.DataChange, ((IEnumerable<AwardDetail>)dailyTaskTargetAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DailyTask_Data);
		}
	}

	public void Treasure_Data_Req(bool openWnd = false, int tabIndex = 0)
	{
		TreasureDataReq req = new TreasureDataReq();
		openTreasureWnd = openWnd;
		openTreasureTabIndex = tabIndex;
		Instance.WsPackMsg((IMessage)(object)req, Message.TreasureDataReq);
	}

	public void Treasure_Data_Ack(ByteString response)
	{
		TreasureDataAck treasureDataAck = TreasureDataAck.Parser.ParseFrom(response);
		if (treasureDataAck != null)
		{
			zbSelf.Instance.treasureHandler.UpdateTreasureInfo(treasureDataAck.TreasureInfo);
			if (openTreasureWnd)
			{
				zbUIMgr.Instance.ShowTreasure(openTreasureTabIndex);
			}
		}
	}

	public void Treasure_Active_Req(int TreasureId)
	{
		TreasureActiveReq treasureActiveReq = new TreasureActiveReq();
		treasureActiveReq.TreasureId = TreasureId;
		Instance.WsPackMsg((IMessage)(object)treasureActiveReq, Message.TreasureActiveReq);
	}

	public void Treasure_Active_Ack(ByteString response)
	{
		TreasureActiveAck treasureActiveAck = TreasureActiveAck.Parser.ParseFrom(response);
		if (treasureActiveAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(treasureActiveAck.DataChange);
			zbSelf.Instance.treasureHandler.UpdateTreasureInfo(treasureActiveAck.TreasureInfo);
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		}
	}

	public void Treasure_UpgradeLevel_Req(int TreasureId)
	{
		TreasureUpgradeLevelReq treasureUpgradeLevelReq = new TreasureUpgradeLevelReq();
		treasureUpgradeLevelReq.TreasureId = TreasureId;
		Instance.WsPackMsg((IMessage)(object)treasureUpgradeLevelReq, Message.TreasureUpgradeLevelReq);
	}

	public void Treasure_UpgradeLevel_Ack(ByteString response)
	{
		TreasureUpgradeLevelAck treasureUpgradeLevelAck = TreasureUpgradeLevelAck.Parser.ParseFrom(response);
		if (treasureUpgradeLevelAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(treasureUpgradeLevelAck.DataChange);
			zbSelf.Instance.treasureHandler.UpdateTreasureInfo(treasureUpgradeLevelAck.TreasureInfo);
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		}
	}

	public void Treasure_ActiveAll_Req()
	{
		TreasureActiveAllReq req = new TreasureActiveAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TreasureActiveAllReq);
	}

	public void Treasure_ActiveAll_Ack(ByteString response)
	{
		TreasureActiveAllAck treasureActiveAllAck = TreasureActiveAllAck.Parser.ParseFrom(response);
		if (treasureActiveAllAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(treasureActiveAllAck.DataChange);
			zbSelf.Instance.treasureHandler.UpdateTreasureInfo(treasureActiveAllAck.TreasureInfo);
			zbUIMgr.Instance.ShowTreasureUpgrade();
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		}
	}

	public void Treasure_UpgradeLevelAll_Req()
	{
		TreasureUpgradeLevelAllReq req = new TreasureUpgradeLevelAllReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TreasureUpgradeLevelAllReq);
	}

	public void Treasure_UpgradeLevelAll_Ack(ByteString response)
	{
		TreasureUpgradeLevelAllAck treasureUpgradeLevelAllAck = TreasureUpgradeLevelAllAck.Parser.ParseFrom(response);
		if (treasureUpgradeLevelAllAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(treasureUpgradeLevelAllAck.DataChange);
			zbSelf.Instance.treasureHandler.UpdateTreasureInfo(treasureUpgradeLevelAllAck.TreasureInfo);
			zbUIMgr.Instance.ShowTreasureUpgrade();
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_upgrade.ogg");
		}
	}

	public void Treasure_Convert_Req(int[] SelectList)
	{
		TreasureConvertReq treasureConvertReq = new TreasureConvertReq();
		treasureConvertReq.SelectList.Clear();
		treasureConvertReq.SelectList.AddRange((IEnumerable<int>)SelectList);
		Instance.WsPackMsg((IMessage)(object)treasureConvertReq, Message.TreasureConvertReq);
	}

	public void Treasure_Convert_Ack(ByteString response)
	{
		TreasureConvertAck treasureConvertAck = TreasureConvertAck.Parser.ParseFrom(response);
		if (treasureConvertAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(treasureConvertAck.DataChange, ((IEnumerable<AwardDetail>)treasureConvertAck.AwardList).ToArray());
		}
	}

	public void RechargeActive_TreasurePrivilege_Req()
	{
		RechargeActiveTreasurePrivilegeReq req = new RechargeActiveTreasurePrivilegeReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.RechargeActiveTreasurePrivilegeReq);
	}

	public void RechargeActive_TreasurePrivilege_Ack(ByteString response)
	{
		RechargeActiveTreasurePrivilegeAck rechargeActiveTreasurePrivilegeAck = RechargeActiveTreasurePrivilegeAck.Parser.ParseFrom(response);
		if (rechargeActiveTreasurePrivilegeAck != null)
		{
			zbServerData.sGameData.RechargeActiveInfo = rechargeActiveTreasurePrivilegeAck.RechargeActiveInfo;
			zbSelf.Instance.dataHandler.DataChange(rechargeActiveTreasurePrivilegeAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveTreasurePrivilegeAck.AwardList).ToArray());
		}
	}

	public void HeroTrial_Data_Req()
	{
		HeroTrialDataReq req = new HeroTrialDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.HeroTrialDataReq);
	}

	public void HeroTrial_Data_Ack(ByteString response)
	{
		HeroTrialDataAck heroTrialDataAck = HeroTrialDataAck.Parser.ParseFrom(response);
		if (heroTrialDataAck != null)
		{
			zbSelf.Instance.trialHandler.UpdatTrailInfo(heroTrialDataAck.HeroTrialInfo);
		}
	}

	public void HeroTrial_FightStart_Req(ESkillType skillType, int levelId)
	{
		HeroTrialFightStartReq heroTrialFightStartReq = new HeroTrialFightStartReq();
		heroTrialFightStartReq.TrialType = skillType;
		heroTrialFightStartReq.LevelId = levelId;
		Instance.WsPackMsg((IMessage)(object)heroTrialFightStartReq, Message.HeroTrialFightStartReq);
	}

	public void HeroTrial_FightStart_Ack(ByteString response)
	{
		HeroTrialFightStartAck heroTrialFightStartAck = HeroTrialFightStartAck.Parser.ParseFrom(response);
		if (heroTrialFightStartAck != null)
		{
			zbSelf.Instance.trialHandler.UpdateSkillType(heroTrialFightStartAck.TrialType);
			zbSelf.Instance.trialHandler.UpdateLevelId(heroTrialFightStartAck.LevelId);
			zbSelf.Instance.combatHandler.StartShowCombat(heroTrialFightStartAck.BattleInfo);
		}
	}

	public void HeroTrial_FightResult_Req(ESkillType skillType, int levelId, BattleResultInfo battleResult)
	{
		HeroTrialFightResultReq heroTrialFightResultReq = new HeroTrialFightResultReq();
		heroTrialFightResultReq.TrialType = skillType;
		heroTrialFightResultReq.LevelId = levelId;
		heroTrialFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)heroTrialFightResultReq, Message.HeroTrialFightResultReq);
	}

	public void HeroTrial_FightResult_Ack(ByteString response)
	{
		HeroTrialFightResultAck heroTrialFightResultAck = HeroTrialFightResultAck.Parser.ParseFrom(response);
		if (heroTrialFightResultAck != null)
		{
			zbServerData.sGameData.HeroTrialInfo = heroTrialFightResultAck.HeroTrialInfo;
			zbSelf.Instance.dataHandler.DataChange(heroTrialFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = heroTrialFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(heroTrialFightResultAck.BattleSettle);
		}
	}

	public void ElementTrial_Data_Req()
	{
		ElementTrialDataReq req = new ElementTrialDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ElementTrialDataReq);
	}

	public void ElementTrial_Data_Ack(ByteString response)
	{
		ElementTrialDataAck elementTrialDataAck = ElementTrialDataAck.Parser.ParseFrom(response);
		if (elementTrialDataAck != null)
		{
			zbServerData.sGameData.ElementTrialInfo = elementTrialDataAck.ElementTrialInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_ElementTrial_Update);
		}
	}

	public void ElementTrial_FightStart_Req(ESkillAttackType type, int levelId)
	{
		ElementTrialFightStartReq elementTrialFightStartReq = new ElementTrialFightStartReq();
		elementTrialFightStartReq.TrialType = type;
		elementTrialFightStartReq.LevelId = levelId;
		Instance.WsPackMsg((IMessage)(object)elementTrialFightStartReq, Message.ElementTrialFightStartReq);
	}

	public void ElementTrial_FightStart_Ack(ByteString response)
	{
		ElementTrialFightStartAck elementTrialFightStartAck = ElementTrialFightStartAck.Parser.ParseFrom(response);
		if (elementTrialFightStartAck != null)
		{
			zbSelf.Instance.elementTrialHandler.UpdateElementTrialInfo(elementTrialFightStartAck.TrialType, elementTrialFightStartAck.LevelId);
			zbSelf.Instance.combatHandler.StartShowCombat(elementTrialFightStartAck.BattleInfo);
		}
	}

	public void ElementTrial_FightResult_Req(ESkillAttackType type, int levelId, BattleResultInfo battleResult)
	{
		ElementTrialFightResultReq elementTrialFightResultReq = new ElementTrialFightResultReq();
		elementTrialFightResultReq.TrialType = type;
		elementTrialFightResultReq.LevelId = levelId;
		elementTrialFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)elementTrialFightResultReq, Message.ElementTrialFightResultReq);
	}

	public void ElementTrial_FightResult_Ack(ByteString response)
	{
		ElementTrialFightResultAck elementTrialFightResultAck = ElementTrialFightResultAck.Parser.ParseFrom(response);
		if (elementTrialFightResultAck != null)
		{
			zbServerData.sGameData.ElementTrialInfo = elementTrialFightResultAck.ElementTrialInfo;
			zbSelf.Instance.dataHandler.DataChange(elementTrialFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = elementTrialFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(elementTrialFightResultAck.BattleSettle);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_ElementTrial_Update);
		}
	}

	public void ElementTrial_BuyCombat_Req(int count)
	{
		ElementTrialBuyCombatReq elementTrialBuyCombatReq = new ElementTrialBuyCombatReq();
		elementTrialBuyCombatReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)elementTrialBuyCombatReq, Message.ElementTrialBuyCombatReq);
	}

	public void ElementTrial_BuyCombat_Ack(ByteString response)
	{
		ElementTrialBuyCombatAck elementTrialBuyCombatAck = ElementTrialBuyCombatAck.Parser.ParseFrom(response);
		if (elementTrialBuyCombatAck != null)
		{
			zbServerData.sGameData.ElementTrialInfo = elementTrialBuyCombatAck.ElementTrialInfo;
			zbSelf.Instance.dataHandler.DataChange(elementTrialBuyCombatAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_ElementTrial_Update);
		}
	}

	public void ElementTrial_SaoDang_Req(ESkillAttackType type)
	{
		ElementTrialSaoDangReq elementTrialSaoDangReq = new ElementTrialSaoDangReq();
		elementTrialSaoDangReq.TrialType = type;
		Instance.WsPackMsg((IMessage)(object)elementTrialSaoDangReq, Message.ElementTrialSaoDangReq);
	}

	public void ElementTrial_SaoDang_Ack(ByteString response)
	{
		ElementTrialSaoDangAck elementTrialSaoDangAck = ElementTrialSaoDangAck.Parser.ParseFrom(response);
		if (elementTrialSaoDangAck != null)
		{
			zbServerData.sGameData.ElementTrialInfo = elementTrialSaoDangAck.ElementTrialInfo;
			zbSelf.Instance.dataHandler.DataChange(elementTrialSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)elementTrialSaoDangAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_ElementTrial_Update);
		}
	}

	public void RechargeActive_ZhanLingElementAward_Req(int level)
	{
		RechargeActiveZhanLingElementAwardReq rechargeActiveZhanLingElementAwardReq = new RechargeActiveZhanLingElementAwardReq();
		rechargeActiveZhanLingElementAwardReq.TrialLevel = level;
		Instance.WsPackMsg((IMessage)(object)rechargeActiveZhanLingElementAwardReq, Message.RechargeActiveZhanLingElementAwardReq);
	}

	public void RechargeActive_ZhanLingElementAward_Ack(ByteString response)
	{
		RechargeActiveZhanLingElementAwardAck rechargeActiveZhanLingElementAwardAck = RechargeActiveZhanLingElementAwardAck.Parser.ParseFrom(response);
		zbServerData.sGameData.RechargeActiveInfo = rechargeActiveZhanLingElementAwardAck.RechargeActiveInfo;
		zbSelf.Instance.dataHandler.DataChange(rechargeActiveZhanLingElementAwardAck.DataChange, ((IEnumerable<AwardDetail>)rechargeActiveZhanLingElementAwardAck.AwardList).ToArray());
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_ElementTrial_Update);
	}

	public void ElementTrial_Refresh_Req()
	{
		ElementTrialRefreshReq req = new ElementTrialRefreshReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ElementTrialRefreshReq);
	}

	public void ElementTrial_Refresh_Ack(ByteString response)
	{
		ElementTrialRefreshAck elementTrialRefreshAck = ElementTrialRefreshAck.Parser.ParseFrom(response);
		if (elementTrialRefreshAck != null)
		{
			zbServerData.sGameData.ElementTrialInfo = elementTrialRefreshAck.ElementTrialInfo;
			zbElementTrialWnd zbElementTrialWnd2 = zbUIMgr.Instance.GetWndById(EWndId.ElementTrial) as zbElementTrialWnd;
			if ((Object)(object)zbElementTrialWnd2 != (Object)null)
			{
				zbElementTrialWnd2.RefreshCustom();
			}
		}
	}

	public void ElementTrial_Upgrade_Req()
	{
		ElementTrialUpgradeReq req = new ElementTrialUpgradeReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.ElementTrialUpgradeReq);
	}

	public void ElementTrial_Upgrade_Ack(ByteString response)
	{
		ElementTrialUpgradeAck elementTrialUpgradeAck = ElementTrialUpgradeAck.Parser.ParseFrom(response);
		if (elementTrialUpgradeAck != null)
		{
			zbServerData.sGameData.ElementTrialInfo = elementTrialUpgradeAck.ElementTrialInfo;
			zbElementTrialWnd zbElementTrialWnd2 = zbUIMgr.Instance.GetWndById(EWndId.ElementTrial) as zbElementTrialWnd;
			zbSelf.Instance.dataHandler.DataChange(elementTrialUpgradeAck.DataChange, ((IEnumerable<AwardDetail>)elementTrialUpgradeAck.AwardList).ToArray());
			if ((Object)(object)zbElementTrialWnd2 != (Object)null)
			{
				zbElementTrialWnd2.RefreshCustom();
			}
		}
	}

	public void Truck_Data_Req(bool openWnd = false, int _openTruckTabIndex = 1, bool fromTask = false)
	{
		TruckDataReq req = new TruckDataReq();
		openTruckMainWnd = openWnd;
		openTruckTabIndex = _openTruckTabIndex;
		openFromTask = fromTask;
		Instance.WsPackMsg((IMessage)(object)req, Message.TruckDataReq);
	}

	public void Truck_Data_Ack(ByteString response)
	{
		TruckDataAck truckDataAck = TruckDataAck.Parser.ParseFrom(response);
		if (truckDataAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckDataAck.TruckSelfInfo);
			if (openTruckMainWnd)
			{
				zbUIMgr.Instance.ShowTruckMainWnd(openTruckTabIndex, openFromTask);
			}
		}
	}

	public void Truck_SlotUnlock_Req()
	{
		TruckSlotUnlockReq req = new TruckSlotUnlockReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TruckSlotUnlockReq);
	}

	public void Truck_SlotUnlock_Ack(ByteString response)
	{
		TruckSlotUnlockAck truckSlotUnlockAck = TruckSlotUnlockAck.Parser.ParseFrom(response);
		if (truckSlotUnlockAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckSlotUnlockAck.TruckSelfInfo);
			zbSelf.Instance.dataHandler.DataChange(truckSlotUnlockAck.DataChange);
		}
	}

	public void Truck_TransportOpen_Req(int TruckId)
	{
		openTruckId = TruckId;
		TruckTransportOpenReq truckTransportOpenReq = new TruckTransportOpenReq();
		truckTransportOpenReq.TruckId = TruckId;
		Instance.WsPackMsg((IMessage)(object)truckTransportOpenReq, Message.TruckTransportOpenReq);
	}

	public void Truck_TransportOpen_Ack(ByteString response)
	{
		TruckTransportOpenAck truckTransportOpenAck = TruckTransportOpenAck.Parser.ParseFrom(response);
		if (truckTransportOpenAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckTransportOpenAck.TruckSelfInfo);
			zbUIMgr.Instance.ShowTruckStartWnd(openTruckId);
		}
	}

	public void Truck_TransportRefresh_Req(int TruckId)
	{
		TruckTransportRefreshReq truckTransportRefreshReq = new TruckTransportRefreshReq();
		truckTransportRefreshReq.TruckId = TruckId;
		Instance.WsPackMsg((IMessage)(object)truckTransportRefreshReq, Message.TruckTransportRefreshReq);
	}

	public void Truck_TransportRefresh_Ack(ByteString response)
	{
		TruckTransportRefreshAck truckTransportRefreshAck = TruckTransportRefreshAck.Parser.ParseFrom(response);
		if (truckTransportRefreshAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckTransportRefreshAck.TruckSelfInfo);
			zbSelf.Instance.dataHandler.DataChange(truckTransportRefreshAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_TruckTransportRefresh);
		}
	}

	public void Truck_TransportStart_Req(int TruckId)
	{
		TruckTransportStartReq truckTransportStartReq = new TruckTransportStartReq();
		truckTransportStartReq.TruckId = TruckId;
		Instance.WsPackMsg((IMessage)(object)truckTransportStartReq, Message.TruckTransportStartReq);
	}

	public void Truck_TransportStart_Ack(ByteString response)
	{
		TruckTransportStartAck truckTransportStartAck = TruckTransportStartAck.Parser.ParseFrom(response);
		if (truckTransportStartAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckTransportStartAck.TruckSelfInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_TruckTransportStart);
		}
	}

	public void Truck_TransportView_Req(int TruckId)
	{
		TruckTransportViewReq truckTransportViewReq = new TruckTransportViewReq();
		truckTransportViewReq.TruckId = TruckId;
		Instance.WsPackMsg((IMessage)(object)truckTransportViewReq, Message.TruckTransportViewReq);
	}

	public void Truck_TransportView_Ack(ByteString response)
	{
		TruckTransportViewAck truckTransportViewAck = TruckTransportViewAck.Parser.ParseFrom(response);
		if (truckTransportViewAck != null)
		{
			zbUIMgr.Instance.ShowTruckBalance(truckTransportViewAck.TruckTransportInfo, _isRuning: true);
		}
	}

	public void Truck_TransportComplete_Req(int TruckId)
	{
		TruckTransportCompleteReq truckTransportCompleteReq = new TruckTransportCompleteReq();
		truckTransportCompleteReq.TruckId = TruckId;
		Instance.WsPackMsg((IMessage)(object)truckTransportCompleteReq, Message.TruckTransportCompleteReq);
	}

	public void Truck_TransportComplete_Ack(ByteString response)
	{
		TruckTransportCompleteAck truckTransportCompleteAck = TruckTransportCompleteAck.Parser.ParseFrom(response);
		if (truckTransportCompleteAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckTransportCompleteAck.TruckSelfInfo);
			zbUIMgr.Instance.ShowTruckBalance(truckTransportCompleteAck.TruckTransportInfo, _isRuning: false);
			zbSelf.Instance.dataHandler.DataChange(truckTransportCompleteAck.DataChange, ((IEnumerable<AwardDetail>)truckTransportCompleteAck.AwardList).ToArray());
		}
	}

	public void Truck_BuyTransport_Req(int Count)
	{
		TruckBuyTransportReq truckBuyTransportReq = new TruckBuyTransportReq();
		truckBuyTransportReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)truckBuyTransportReq, Message.TruckBuyTransportReq);
	}

	public void Truck_BuyTransport_Ack(ByteString response)
	{
		TruckBuyTransportAck truckBuyTransportAck = TruckBuyTransportAck.Parser.ParseFrom(response);
		if (truckBuyTransportAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckBuyTransportAck.TruckSelfInfo);
			zbSelf.Instance.dataHandler.DataChange(truckBuyTransportAck.DataChange);
		}
	}

	public void Truck_TransportRecordList_Req(bool openWnd = false)
	{
		openTruckRecordWnd = openWnd;
		TruckTransportRecordListReq req = new TruckTransportRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TruckTransportRecordListReq);
	}

	public void Truck_TransportRecordList_Ack(ByteString response)
	{
		TruckTransportRecordListAck truckTransportRecordListAck = TruckTransportRecordListAck.Parser.ParseFrom(response);
		if (truckTransportRecordListAck != null)
		{
			zbSelf.Instance.truckHandler.TruckTransportInfoList = ((IEnumerable<TruckTransportInfo>)truckTransportRecordListAck.RecordList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_TruckMatchRefresh);
			if (openTruckRecordWnd)
			{
				zbUIMgr.Instance.ShowTruckRecord(1);
			}
		}
	}

	public void Truck_MatchList_Req(bool openWnd = false)
	{
		TruckMatchListReq req = new TruckMatchListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TruckMatchListReq);
	}

	public void Truck_MatchList_Ack(ByteString response)
	{
		TruckMatchListAck truckMatchListAck = TruckMatchListAck.Parser.ParseFrom(response);
		if (truckMatchListAck != null)
		{
			zbSelf.Instance.truckHandler.MatchList = ((IEnumerable<TruckMatchDetail>)truckMatchListAck.MatchList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_TruckMatchRefresh);
		}
	}

	public void Truck_RobFightStart_Req(int PlayerId, int TransportId)
	{
		TruckRobFightStartReq truckRobFightStartReq = new TruckRobFightStartReq();
		truckRobFightStartReq.PlayerId = PlayerId;
		truckRobFightStartReq.TransportId = TransportId;
		Instance.WsPackMsg((IMessage)(object)truckRobFightStartReq, Message.TruckRobFightStartReq);
	}

	public void Truck_RobFightStart_Ack(ByteString response)
	{
		TruckRobFightStartAck truckRobFightStartAck = TruckRobFightStartAck.Parser.ParseFrom(response);
		if (truckRobFightStartAck != null)
		{
			zbUtil.Debug2LogFile("Truck_RobFightStart_Ack.txt", truckRobFightStartAck);
			zbSelf.Instance.truckHandler.currPlayerId = truckRobFightStartAck.PlayerId;
			zbSelf.Instance.truckHandler.currTransportId = truckRobFightStartAck.TransportId;
			zbSelf.Instance.combatHandler.StartShowCombat(truckRobFightStartAck.BattleInfo);
		}
	}

	public void Truck_RobFightResult_Req(int PlayerId, int TransportId, BattleResultInfo BattleResult)
	{
		TruckRobFightResultReq truckRobFightResultReq = new TruckRobFightResultReq();
		truckRobFightResultReq.PlayerId = PlayerId;
		truckRobFightResultReq.TransportId = TransportId;
		truckRobFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)truckRobFightResultReq, Message.TruckRobFightResultReq);
	}

	public void Truck_RobFightResult_Ack(ByteString response)
	{
		TruckRobFightResultAck truckRobFightResultAck = TruckRobFightResultAck.Parser.ParseFrom(response);
		if (truckRobFightResultAck != null)
		{
			zbSelf.Instance.truckHandler.bBestPrize = truckRobFightResultAck.BestPrize == EBoolState.True;
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckRobFightResultAck.TruckSelfInfo);
			zbCombatSystem.Instance.battleAwardList = truckRobFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(truckRobFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(truckRobFightResultAck.BattleSettle);
		}
	}

	public void Truck_BuyRob_Req(int Count)
	{
		TruckBuyRobReq truckBuyRobReq = new TruckBuyRobReq();
		truckBuyRobReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)truckBuyRobReq, Message.TruckBuyRobReq);
	}

	public void Truck_BuyRob_Ack(ByteString response)
	{
		TruckBuyRobAck truckBuyRobAck = TruckBuyRobAck.Parser.ParseFrom(response);
		if (truckBuyRobAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckBuyRobAck.TruckSelfInfo);
			zbSelf.Instance.dataHandler.DataChange(truckBuyRobAck.DataChange);
		}
	}

	public void Truck_RobRecordList_Req(bool openWnd = false)
	{
		openTruckRecordWnd = openWnd;
		TruckRobRecordListReq req = new TruckRobRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TruckRobRecordListReq);
	}

	public void Truck_RobRecordList_Ack(ByteString response)
	{
		TruckRobRecordListAck truckRobRecordListAck = TruckRobRecordListAck.Parser.ParseFrom(response);
		if (truckRobRecordListAck != null)
		{
			zbSelf.Instance.truckHandler.RobRecordList = ((IEnumerable<TruckRobRecordDetail>)truckRobRecordListAck.RecordList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_TruckMatchRefresh);
			if (openTruckRecordWnd)
			{
				zbUIMgr.Instance.ShowTruckRecord(0);
			}
		}
	}

	public void Truck_AddWanted_Req(int PlayerId)
	{
		TruckAddWantedReq truckAddWantedReq = new TruckAddWantedReq();
		truckAddWantedReq.PlayerId = PlayerId;
		Instance.WsPackMsg((IMessage)(object)truckAddWantedReq, Message.TruckAddWantedReq);
	}

	public void Truck_AddWanted_Ack(ByteString response)
	{
		TruckAddWantedAck truckAddWantedAck = TruckAddWantedAck.Parser.ParseFrom(response);
		if (truckAddWantedAck != null)
		{
			zbSelf.Instance.truckHandler.UpdateTruckSelfInfo(truckAddWantedAck.TruckSelfInfo);
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11141"));
		}
	}

	public void Train_Data_Req()
	{
		TrainDataReq req = new TrainDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TrainDataReq);
	}

	public void Train_Data_Ack(ByteString response)
	{
		TrainDataAck trainDataAck = TrainDataAck.Parser.ParseFrom(response);
		if (trainDataAck != null)
		{
			zbSelf.Instance.truckHandler.trainInfo = trainDataAck.TrainInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TrainData_Ack);
		}
	}

	public void Train_TransportOpen_Req(EQuality Quality)
	{
		TrainTransportOpenReq trainTransportOpenReq = new TrainTransportOpenReq();
		trainTransportOpenReq.Quality = Quality;
		Instance.WsPackMsg((IMessage)(object)trainTransportOpenReq, Message.TrainTransportOpenReq);
	}

	public void Train_TransportOpen_Ack(ByteString response)
	{
		TrainTransportOpenAck trainTransportOpenAck = TrainTransportOpenAck.Parser.ParseFrom(response);
		if (trainTransportOpenAck != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11370"));
			zbSelf.Instance.truckHandler.trainInfo = trainTransportOpenAck.TrainInfo;
			zbSelf.Instance.dataHandler.DataChange(trainTransportOpenAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TrainTransportOpen_Ack);
		}
	}

	public void Train_TransportTake_Req(bool openTaker = false)
	{
		TrainTransportTakeReq req = new TrainTransportTakeReq();
		zbSelf.Instance.truckHandler.openTaker = openTaker;
		Instance.WsPackMsg((IMessage)(object)req, Message.TrainTransportTakeReq);
	}

	public void Train_TransportTake_Ack(ByteString response)
	{
		TrainTransportTakeAck trainTransportTakeAck = TrainTransportTakeAck.Parser.ParseFrom(response);
		if (trainTransportTakeAck != null)
		{
			zbSelf.Instance.truckHandler.trainInfo = trainTransportTakeAck.TrainInfo;
			if (zbSelf.Instance.truckHandler.openTaker)
			{
				zbUIMgr.Instance.ShowTrainTaker();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TrainTransportTakeRefresh_Ack);
			}
		}
	}

	public void Train_TransportTakeRefresh_Req()
	{
		TrainTransportTakeRefreshReq req = new TrainTransportTakeRefreshReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TrainTransportTakeRefreshReq);
	}

	public void Train_TransportTakeRefresh_Ack(ByteString response)
	{
		TrainTransportTakeRefreshAck trainTransportTakeRefreshAck = TrainTransportTakeRefreshAck.Parser.ParseFrom(response);
		if (trainTransportTakeRefreshAck != null)
		{
			zbSelf.Instance.truckHandler.trainInfo = trainTransportTakeRefreshAck.TrainInfo;
			zbSelf.Instance.dataHandler.DataChange(trainTransportTakeRefreshAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TrainTransportTakeRefresh_Ack);
		}
	}

	public void Train_TransportTakeConfirm_Req(int TransportId, int CarriageIndex)
	{
		TrainTransportTakeConfirmReq trainTransportTakeConfirmReq = new TrainTransportTakeConfirmReq();
		trainTransportTakeConfirmReq.TransportId = TransportId;
		trainTransportTakeConfirmReq.CarriageIndex = CarriageIndex;
		Instance.WsPackMsg((IMessage)(object)trainTransportTakeConfirmReq, Message.TrainTransportTakeConfirmReq);
	}

	public void Train_TransportTakeConfirm_Ack(ByteString response)
	{
		TrainTransportTakeConfirmAck trainTransportTakeConfirmAck = TrainTransportTakeConfirmAck.Parser.ParseFrom(response);
		if (trainTransportTakeConfirmAck != null)
		{
			zbSelf.Instance.truckHandler.trainInfo = trainTransportTakeConfirmAck.TrainInfo;
			zbSelf.Instance.dataHandler.DataChange(trainTransportTakeConfirmAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TrainTransportOpen_Ack);
		}
	}

	public void Train_TransportAward_Req(int TransportId)
	{
		TrainTransportAwardReq trainTransportAwardReq = new TrainTransportAwardReq();
		trainTransportAwardReq.TransportId = TransportId;
		Instance.WsPackMsg((IMessage)(object)trainTransportAwardReq, Message.TrainTransportAwardReq);
	}

	public void Train_TransportAward_Ack(ByteString response)
	{
		TrainTransportAwardAck trainTransportAwardAck = TrainTransportAwardAck.Parser.ParseFrom(response);
		if (trainTransportAwardAck != null)
		{
			zbSelf.Instance.truckHandler.trainInfo = trainTransportAwardAck.TrainInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TrainTransportAward_Ack, trainTransportAwardAck);
		}
	}

	public void Train_RobMatch_Req()
	{
		TrainRobMatchReq req = new TrainRobMatchReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.TrainRobMatchReq);
	}

	public void Train_RobMatch_Ack(ByteString response)
	{
		TrainRobMatchAck trainRobMatchAck = TrainRobMatchAck.Parser.ParseFrom(response);
		if (trainRobMatchAck != null)
		{
			zbSelf.Instance.truckHandler.currRobTrain = trainRobMatchAck.RobTrain;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_TrainRobMatchAck_Ack, trainRobMatchAck);
		}
	}

	public void Train_RobFightStart_Req(int TransportId)
	{
		TrainRobFightStartReq trainRobFightStartReq = new TrainRobFightStartReq();
		trainRobFightStartReq.TransportId = TransportId;
		Instance.WsPackMsg((IMessage)(object)trainRobFightStartReq, Message.TrainRobFightStartReq);
	}

	public void Train_RobFightStart_Ack(ByteString response)
	{
		TrainRobFightStartAck trainRobFightStartAck = TrainRobFightStartAck.Parser.ParseFrom(response);
		if (trainRobFightStartAck != null)
		{
			zbUtil.Debug2LogFile("Train_RobFightStart_Ack.txt", trainRobFightStartAck);
			zbSelf.Instance.truckHandler.currPlayerId = 0;
			zbSelf.Instance.truckHandler.currTransportId = trainRobFightStartAck.TransportId;
			zbSelf.Instance.combatHandler.StartShowCombat(trainRobFightStartAck.BattleInfo);
		}
	}

	public void Train_RobFightResult_Req(int TransportId, BattleResultInfo BattleResult)
	{
		TrainRobFightResultReq trainRobFightResultReq = new TrainRobFightResultReq();
		trainRobFightResultReq.TransportId = TransportId;
		trainRobFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)trainRobFightResultReq, Message.TrainRobFightResultReq);
	}

	public void Train_RobFightResult_Ack(ByteString response)
	{
		TrainRobFightResultAck trainRobFightResultAck = TrainRobFightResultAck.Parser.ParseFrom(response);
		if (trainRobFightResultAck != null)
		{
			zbSelf.Instance.truckHandler.currRobTrain = trainRobFightResultAck.RobTrain;
			zbUtil.Debug2LogFile("Train_RobFightResult_Ack.txt", trainRobFightResultAck);
			zbCombatSystem.Instance.battleAwardList = trainRobFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(trainRobFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(trainRobFightResultAck.BattleSettle);
		}
	}

	public void Pay_CreateOrder_Req(int goodsId)
	{
		PayCreateOrderReq payCreateOrderReq = new PayCreateOrderReq();
		payCreateOrderReq.GoodsId = goodsId;
		payCreateOrderReq.AppChannel = zbLoginUtil.appChannel;
		payCreateOrderReq.Platform = zbLoginUtil.platForm;
		Instance.WsPackMsg((IMessage)(object)payCreateOrderReq, Message.PayCreateOrderReq);
	}

	public void Pay_CreateOrder_Ack(ByteString response)
	{
		PayCreateOrderAck payCreateOrderAck = PayCreateOrderAck.Parser.ParseFrom(response);
		if (payCreateOrderAck != null)
		{
			zbChannel_Pay.OnCreateOrderAck(payCreateOrderAck);
		}
	}

	public void Pay_CreateOrderSign_Req(int goodsId)
	{
		PayCreateOrderSignReq payCreateOrderSignReq = new PayCreateOrderSignReq();
		payCreateOrderSignReq.GoodsId = goodsId;
		payCreateOrderSignReq.AppChannel = zbLoginUtil.appChannel;
		payCreateOrderSignReq.Platform = zbLoginUtil.platForm;
		Instance.WsPackMsg((IMessage)(object)payCreateOrderSignReq, Message.PayCreateOrderSignReq);
	}

	public void Pay_CreateOrderSign_Ack(ByteString response)
	{
		PayCreateOrderSignAck payCreateOrderSignAck = PayCreateOrderSignAck.Parser.ParseFrom(response);
		if (payCreateOrderSignAck != null)
		{
			zbChannel_Pay.OnCreateOrderSignAck(payCreateOrderSignAck);
		}
	}

	public void Pay_VerifyOrder_Req(PayVerifyOrderReq req)
	{
		Instance.WsPackMsg((IMessage)(object)req, Message.PayVerifyOrderReq);
	}

	public void Pay_VerifyOrder_Ack(ByteString response)
	{
		PayVerifyOrderAck.Parser.ParseFrom(response);
	}

	public void Pay_VerifyOrderV2_Req(PayVerifyOrderV2Req req)
	{
		Instance.WsPackMsg((IMessage)(object)req, Message.PayVerifyOrderV2Req);
	}

	public void Pay_VerifyOrderV2_Ack(ByteString response)
	{
		PayVerifyOrderV2Ack.Parser.ParseFrom(response);
	}

	public void Pay_SuccessOrder_Req()
	{
		Instance.WsPackMsg(null, Message.PaySuccessOrderReq);
	}

	public void Pay_SuccessOrder_Ack(ByteString response)
	{
		PaySuccessOrderAck paySuccessOrderAck = PaySuccessOrderAck.Parser.ParseFrom(response);
		if (paySuccessOrderAck != null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10234"));
			zbServerData.sGameData.RechargeShopInfo = paySuccessOrderAck.RechargeShopInfo;
			zbServerData.sGameData.RechargeActiveInfo = paySuccessOrderAck.RechargeActiveInfo;
			zbServerData.sGameData.SubscribeInfo = paySuccessOrderAck.SubscribeInfo;
			zbServerData.sGameData.VipInfo = paySuccessOrderAck.VipInfo;
			zbSelf.Instance.dataHandler.DataChange(paySuccessOrderAck.DataChange, ((IEnumerable<AwardDetail>)paySuccessOrderAck.AwardList).ToArray());
			zbChannel_Report.ReportDataByChannel(EReportType.E_PURCHASE, (paySuccessOrderAck.TotalPrice * 100f).ToString(), (IList<OrderDetail>)paySuccessOrderAck.OrderList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SuccessOrder_Ack);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Subscribe_Update);
		}
	}

	public void Pay_VoucherConsume_Req(int goodsId)
	{
		PayVoucherConsumeReq payVoucherConsumeReq = new PayVoucherConsumeReq();
		payVoucherConsumeReq.GoodsId = goodsId;
		Instance.WsPackMsg((IMessage)(object)payVoucherConsumeReq, Message.PayVoucherConsumeReq);
	}

	public void Pay_VoucherConsume_Ack(ByteString response)
	{
		PayVoucherConsumeAck payVoucherConsumeAck = PayVoucherConsumeAck.Parser.ParseFrom(response);
		if (payVoucherConsumeAck != null)
		{
			zbServerData.sGameData.RechargeShopInfo = payVoucherConsumeAck.RechargeShopInfo;
			zbServerData.sGameData.RechargeActiveInfo = payVoucherConsumeAck.RechargeActiveInfo;
			zbServerData.sGameData.SubscribeInfo = payVoucherConsumeAck.SubscribeInfo;
			zbSelf.Instance.dataHandler.DataChange(payVoucherConsumeAck.DataChange, ((IEnumerable<AwardDetail>)payVoucherConsumeAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_SuccessOrder_Ack);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Subscribe_Update);
		}
	}

	public void Vip_Data_Req()
	{
		VipDataReq req = new VipDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.VipDataReq);
	}

	public void Vip_Data_Ack(ByteString response)
	{
		VipDataAck vipDataAck = VipDataAck.Parser.ParseFrom(response);
		if (vipDataAck != null)
		{
			zbServerData.sGameData.VipInfo = vipDataAck.VipInfo;
			zbUIMgr.Instance.ShowVipTequanWnd();
		}
	}

	public void Vip_FreeExp_Req()
	{
		VipFreeExpReq req = new VipFreeExpReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.VipFreeExpReq);
	}

	public void Vip_FreeExp_Ack(ByteString response)
	{
		VipFreeExpAck vipFreeExpAck = VipFreeExpAck.Parser.ParseFrom(response);
		if (vipFreeExpAck != null)
		{
			zbServerData.sGameData.VipInfo = vipFreeExpAck.VipInfo;
			zbSelf.Instance.dataHandler.DataChange(vipFreeExpAck.DataChange, ((IEnumerable<AwardDetail>)vipFreeExpAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_VipInfo_Update);
		}
	}

	public void Vip_DailyFree_Req(int VipLevel)
	{
		VipDailyFreeReq vipDailyFreeReq = new VipDailyFreeReq();
		vipDailyFreeReq.VipLevel = VipLevel;
		Instance.WsPackMsg((IMessage)(object)vipDailyFreeReq, Message.VipDailyFreeReq);
	}

	public void Vip_DailyFree_Ack(ByteString response)
	{
		VipDailyFreeAck vipDailyFreeAck = VipDailyFreeAck.Parser.ParseFrom(response);
		if (vipDailyFreeAck != null)
		{
			zbServerData.sGameData.VipInfo = vipDailyFreeAck.VipInfo;
			zbSelf.Instance.dataHandler.DataChange(vipDailyFreeAck.DataChange, ((IEnumerable<AwardDetail>)vipDailyFreeAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_VipInfo_Update);
		}
	}

	public void Account_RebateAward_Req()
	{
		AccountRebateAwardReq req = new AccountRebateAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.AccountRebateAwardReq);
	}

	public void Account_RebateAward_Ack(ByteString response)
	{
		AccountRebateAwardAck accountRebateAwardAck = AccountRebateAwardAck.Parser.ParseFrom(response);
		if (accountRebateAwardAck != null)
		{
			zbServerData.sGameData.AuxAwardInfo = accountRebateAwardAck.AuxAwardInfo;
			zbSelf.Instance.dataHandler.DataChange(accountRebateAwardAck.DataChange, ((IEnumerable<AwardDetail>)accountRebateAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_RebateAward_Ack);
		}
	}

	public void Wall_Data_Req(bool open = false, int tabIndex = 0)
	{
		WallDataReq req = new WallDataReq();
		openWall = open;
		openWallTabIndex = tabIndex;
		Instance.WsPackMsg((IMessage)(object)req, Message.WallDataReq);
	}

	public void Wall_Data_Ack(ByteString response)
	{
		WallDataAck wallDataAck = WallDataAck.Parser.ParseFrom(response);
		if (wallDataAck != null)
		{
			zbSelf.Instance.wallHandler.UpdateWallInfo(wallDataAck.WallInfo);
			if (openWall)
			{
				zbUIMgr.Instance.ShowWall(openWallTabIndex);
			}
		}
	}

	public void Wall_Active_Req(int WallId)
	{
		WallActiveReq wallActiveReq = new WallActiveReq();
		wallActiveReq.WallId = WallId;
		Instance.WsPackMsg((IMessage)(object)wallActiveReq, Message.WallActiveReq);
	}

	public void Wall_Active_Ack(ByteString response)
	{
		WallActiveAck wallActiveAck = WallActiveAck.Parser.ParseFrom(response);
		if (wallActiveAck != null)
		{
			zbSelf.Instance.newTagHandler.AddNewWall(wallActiveAck.WallId);
			zbSelf.Instance.dataHandler.DataChange(wallActiveAck.DataChange);
			zbSelf.Instance.wallHandler.UpdateWallInfo(wallActiveAck.WallInfo);
			zbUIMgr.Instance.ShowWallGet(wallActiveAck.WallId);
		}
	}

	public void Wall_Set_Req(int WallId)
	{
		WallSetReq wallSetReq = new WallSetReq();
		wallSetReq.WallId = WallId;
		Instance.WsPackMsg((IMessage)(object)wallSetReq, Message.WallSetReq);
	}

	public void Wall_Set_Ack(ByteString response)
	{
		WallSetAck wallSetAck = WallSetAck.Parser.ParseFrom(response);
		if (wallSetAck != null)
		{
			zbSelf.Instance.wallHandler.UpdateWallInfo(wallSetAck.WallInfo);
		}
	}

	public void Wall_UpgradeLevel_Req()
	{
		WallUpgradeLevelReq req = new WallUpgradeLevelReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WallUpgradeLevelReq);
	}

	public void Wall_UpgradeLevel_Ack(ByteString response)
	{
		WallUpgradeLevelAck wallUpgradeLevelAck = WallUpgradeLevelAck.Parser.ParseFrom(response);
		if (wallUpgradeLevelAck != null)
		{
			zbSelf.Instance.wallHandler.DataChange(zbServerData.sGameData.DefenseInfo, wallUpgradeLevelAck.DefenseInfo);
			zbServerData.sGameData.DefenseInfo = wallUpgradeLevelAck.DefenseInfo;
			zbSelf.Instance.dataHandler.DataChange(wallUpgradeLevelAck.DataChange);
			zbSelf.Instance.wallHandler.UpdateWallInfo(wallUpgradeLevelAck.WallInfo);
			zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_AWARD);
		}
	}

	public void Wall_UpgradeStar_Req(int WallId)
	{
		WallUpgradeStarReq wallUpgradeStarReq = new WallUpgradeStarReq();
		wallUpgradeStarReq.WallId = WallId;
		curStarWallId = WallId;
		Instance.WsPackMsg((IMessage)(object)wallUpgradeStarReq, Message.WallUpgradeStarReq);
	}

	public void Wall_UpgradeStar_Ack(ByteString response)
	{
		WallUpgradeStarAck wallUpgradeStarAck = WallUpgradeStarAck.Parser.ParseFrom(response);
		if (wallUpgradeStarAck != null)
		{
			zbSelf.Instance.wallHandler.DataChange(zbServerData.sGameData.DefenseInfo, wallUpgradeStarAck.DefenseInfo);
			zbSelf.Instance.dataHandler.DataChange(wallUpgradeStarAck.DataChange);
			zbSelf.Instance.wallHandler.UpdateWallInfo(wallUpgradeStarAck.WallInfo);
			zbUIMgr.Instance.ShowWallStarSuccess(curStarWallId);
		}
	}

	public void Wall_Convert_Req(int[] SelectList)
	{
		WallConvertReq wallConvertReq = new WallConvertReq();
		wallConvertReq.SelectList.Clear();
		wallConvertReq.SelectList.AddRange((IEnumerable<int>)SelectList);
		Instance.WsPackMsg((IMessage)(object)wallConvertReq, Message.WallConvertReq);
	}

	public void Wall_Convert_Ack(ByteString response)
	{
		WallConvertAck wallConvertAck = WallConvertAck.Parser.ParseFrom(response);
		if (wallConvertAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(wallConvertAck.DataChange, ((IEnumerable<AwardDetail>)wallConvertAck.AwardList).ToArray());
		}
	}

	public void Wall_SkillSet_Req(int Index, int WallId)
	{
		WallSkillSetReq wallSkillSetReq = new WallSkillSetReq();
		wallSkillSetReq.Index = Index;
		wallSkillSetReq.WallId = WallId;
		Instance.WsPackMsg((IMessage)(object)wallSkillSetReq, Message.WallSkillSetReq);
	}

	public void Wall_SkillSet_Ack(ByteString response)
	{
		WallSkillSetAck wallSkillSetAck = WallSkillSetAck.Parser.ParseFrom(response);
		if (wallSkillSetAck != null)
		{
			zbSelf.Instance.wallHandler.UpdateWallInfo(wallSkillSetAck.WallInfo);
		}
	}

	public void Wall_SkillRemove_Req(int Index)
	{
		WallSkillRemoveReq wallSkillRemoveReq = new WallSkillRemoveReq();
		wallSkillRemoveReq.Index = Index;
		Instance.WsPackMsg((IMessage)(object)wallSkillRemoveReq, Message.WallSkillRemoveReq);
	}

	public void Wall_SkillRemove_Ack(ByteString response)
	{
		WallSkillRemoveAck wallSkillRemoveAck = WallSkillRemoveAck.Parser.ParseFrom(response);
		if (wallSkillRemoveAck != null)
		{
			zbSelf.Instance.wallHandler.UpdateWallInfo(wallSkillRemoveAck.WallInfo);
		}
	}

	public void Operate_WallSubjectData_Req()
	{
		OperateWallSubjectDataReq req = new OperateWallSubjectDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.OperateWallSubjectDataReq);
	}

	public void Operate_WallSubjectData_Ack(ByteString response)
	{
		OperateWallSubjectDataAck operateWallSubjectDataAck = OperateWallSubjectDataAck.Parser.ParseFrom(response);
		if (operateWallSubjectDataAck != null)
		{
			zbServerData.sGameData.OperateInfo.WallSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.WallSubjectList.AddRange((IEnumerable<WallSubjectDetail>)operateWallSubjectDataAck.WallSubjectList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WallSubject);
		}
	}

	public void Operate_WallSubjectFightStart_Req(string UUID, int LevelId)
	{
		OperateWallSubjectFightStartReq operateWallSubjectFightStartReq = new OperateWallSubjectFightStartReq();
		operateWallSubjectFightStartReq.UUID = UUID;
		operateWallSubjectFightStartReq.LevelId = LevelId;
		Instance.WsPackMsg((IMessage)(object)operateWallSubjectFightStartReq, Message.OperateWallSubjectFightStartReq);
	}

	public void Operate_WallSubjectFightStart_Ack(ByteString response)
	{
		OperateWallSubjectFightStartAck operateWallSubjectFightStartAck = OperateWallSubjectFightStartAck.Parser.ParseFrom(response);
		if (operateWallSubjectFightStartAck != null)
		{
			zbUtil.Debug2LogFile("Operate_WallSubjectFightStart_Ack.txt", operateWallSubjectFightStartAck);
			zbSelf.Instance.wallSubjectHandler.currUUID = operateWallSubjectFightStartAck.UUID;
			zbSelf.Instance.wallSubjectHandler.currLevelId = operateWallSubjectFightStartAck.LevelId;
			zbSelf.Instance.combatHandler.StartShowCombat(operateWallSubjectFightStartAck.BattleInfo);
		}
	}

	public void Operate_WallSubjectFightResult_Req(string UUID, int LevelId, BattleResultInfo battleResult)
	{
		OperateWallSubjectFightResultReq operateWallSubjectFightResultReq = new OperateWallSubjectFightResultReq();
		operateWallSubjectFightResultReq.UUID = UUID;
		operateWallSubjectFightResultReq.LevelId = LevelId;
		operateWallSubjectFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)operateWallSubjectFightResultReq, Message.OperateWallSubjectFightResultReq);
	}

	public void Operate_WallSubjectFightResult_Ack(ByteString response)
	{
		OperateWallSubjectFightResultAck operateWallSubjectFightResultAck = OperateWallSubjectFightResultAck.Parser.ParseFrom(response);
		if (operateWallSubjectFightResultAck != null)
		{
			zbUtil.Debug2LogFile("Operate_WallSubjectFightResult_Ack.txt", operateWallSubjectFightResultAck);
			zbServerData.sGameData.OperateInfo.WallSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.WallSubjectList.AddRange((IEnumerable<WallSubjectDetail>)operateWallSubjectFightResultAck.WallSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateWallSubjectFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = operateWallSubjectFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(operateWallSubjectFightResultAck.BattleSettle);
		}
	}

	public void Operate_WallSubjectChapterAward_Req(string UUID, int ChapterId)
	{
		OperateWallSubjectChapterAwardReq operateWallSubjectChapterAwardReq = new OperateWallSubjectChapterAwardReq();
		operateWallSubjectChapterAwardReq.UUID = UUID;
		operateWallSubjectChapterAwardReq.ChapterId = ChapterId;
		Instance.WsPackMsg((IMessage)(object)operateWallSubjectChapterAwardReq, Message.OperateWallSubjectChapterAwardReq);
	}

	public void Operate_WallSubjectChapterAward_Ack(ByteString response)
	{
		OperateWallSubjectChapterAwardAck operateWallSubjectChapterAwardAck = OperateWallSubjectChapterAwardAck.Parser.ParseFrom(response);
		if (operateWallSubjectChapterAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.WallSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.WallSubjectList.AddRange((IEnumerable<WallSubjectDetail>)operateWallSubjectChapterAwardAck.WallSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateWallSubjectChapterAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateWallSubjectChapterAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WallSubject);
		}
	}

	public void Operate_WallSubjectBarDraw_Req(string UUID, int Count)
	{
		OperateWallSubjectBarDrawReq operateWallSubjectBarDrawReq = new OperateWallSubjectBarDrawReq();
		operateWallSubjectBarDrawReq.UUID = UUID;
		operateWallSubjectBarDrawReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)operateWallSubjectBarDrawReq, Message.OperateWallSubjectBarDrawReq);
	}

	public void Operate_WallSubjectBarDraw_Ack(ByteString response)
	{
		OperateWallSubjectBarDrawAck operateWallSubjectBarDrawAck = OperateWallSubjectBarDrawAck.Parser.ParseFrom(response);
		if (operateWallSubjectBarDrawAck != null)
		{
			zbServerData.sGameData.OperateInfo.WallSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.WallSubjectList.AddRange((IEnumerable<WallSubjectDetail>)operateWallSubjectBarDrawAck.WallSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateWallSubjectBarDrawAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WallSubjectBarDraw_Ack, operateWallSubjectBarDrawAck);
		}
	}

	public void Operate_WallSubjectZhanLingAward_Req(string UUID)
	{
		OperateWallSubjectZhanLingAwardReq operateWallSubjectZhanLingAwardReq = new OperateWallSubjectZhanLingAwardReq();
		operateWallSubjectZhanLingAwardReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)operateWallSubjectZhanLingAwardReq, Message.OperateWallSubjectZhanLingAwardReq);
	}

	public void Operate_WallSubjectZhanLingAward_Ack(ByteString response)
	{
		OperateWallSubjectZhanLingAwardAck operateWallSubjectZhanLingAwardAck = OperateWallSubjectZhanLingAwardAck.Parser.ParseFrom(response);
		if (operateWallSubjectZhanLingAwardAck != null)
		{
			zbServerData.sGameData.OperateInfo.WallSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.WallSubjectList.AddRange((IEnumerable<WallSubjectDetail>)operateWallSubjectZhanLingAwardAck.WallSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateWallSubjectZhanLingAwardAck.DataChange, ((IEnumerable<AwardDetail>)operateWallSubjectZhanLingAwardAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WallSubject);
		}
	}

	public void Operate_WallSubjectRoulette_Req(string UUID, int Count)
	{
		OperateWallSubjectRouletteReq operateWallSubjectRouletteReq = new OperateWallSubjectRouletteReq();
		operateWallSubjectRouletteReq.UUID = UUID;
		operateWallSubjectRouletteReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)operateWallSubjectRouletteReq, Message.OperateWallSubjectRouletteReq);
	}

	public void Operate_WallSubjectRoulette_Ack(ByteString response)
	{
		OperateWallSubjectRouletteAck operateWallSubjectRouletteAck = OperateWallSubjectRouletteAck.Parser.ParseFrom(response);
		if (operateWallSubjectRouletteAck != null)
		{
			zbServerData.sGameData.OperateInfo.WallSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.WallSubjectList.AddRange((IEnumerable<WallSubjectDetail>)operateWallSubjectRouletteAck.WallSubjectList);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WallSubjectRoulette_Ack, operateWallSubjectRouletteAck);
		}
	}

	public void Operate_WallSubjectRouletteTarget_Req(string UUID)
	{
		OperateWallSubjectRouletteTargetReq operateWallSubjectRouletteTargetReq = new OperateWallSubjectRouletteTargetReq();
		operateWallSubjectRouletteTargetReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)operateWallSubjectRouletteTargetReq, Message.OperateWallSubjectRouletteTargetReq);
	}

	public void Operate_WallSubjectRouletteTarget_Ack(ByteString response)
	{
		OperateWallSubjectRouletteTargetAck operateWallSubjectRouletteTargetAck = OperateWallSubjectRouletteTargetAck.Parser.ParseFrom(response);
		if (operateWallSubjectRouletteTargetAck != null)
		{
			zbServerData.sGameData.OperateInfo.WallSubjectList.Clear();
			zbServerData.sGameData.OperateInfo.WallSubjectList.AddRange((IEnumerable<WallSubjectDetail>)operateWallSubjectRouletteTargetAck.WallSubjectList);
			zbSelf.Instance.dataHandler.DataChange(operateWallSubjectRouletteTargetAck.DataChange, ((IEnumerable<AwardDetail>)operateWallSubjectRouletteTargetAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WallSubject);
		}
	}

	public void World_Data_Req(bool open = false)
	{
		openWorld = open;
		WorldDataReq req = new WorldDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldDataReq);
	}

	public void World_Data_Ack(ByteString response)
	{
		WorldDataAck worldDataAck = WorldDataAck.Parser.ParseFrom(response);
		if (worldDataAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldDataAck.WorldInfo);
			if (openWorld)
			{
				zbUIMgr.Instance.ShowWorld();
			}
		}
	}

	public void WorldDungeon_Open_Req(int GridId, EDifficultyLevel DungeonLevel, int[] SelectList)
	{
		WorldDungeonOpenReq worldDungeonOpenReq = new WorldDungeonOpenReq();
		worldDungeonOpenReq.GridId = GridId;
		worldDungeonOpenReq.DungeonLevel = DungeonLevel;
		worldDungeonOpenReq.SelectList.Clear();
		worldDungeonOpenReq.SelectList.AddRange((IEnumerable<int>)SelectList);
		Instance.WsPackMsg((IMessage)(object)worldDungeonOpenReq, Message.WorldDungeonOpenReq);
	}

	public void WorldDungeon_Open_Ack(ByteString response)
	{
		WorldDungeonOpenAck worldDungeonOpenAck = WorldDungeonOpenAck.Parser.ParseFrom(response);
		if (worldDungeonOpenAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.OnWorldDungeonOpenAck(worldDungeonOpenAck.DungeonBaseInfo, worldDungeonOpenAck.DungeonMapInfo, worldDungeonOpenAck.DungeonAwardInfo);
			zbSelf.Instance.worldDungeonHandler.OnTriggerEventCheck();
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldDungeonOpenAck.WorldInfo);
			zbSelf.Instance.dataHandler.DataChange(worldDungeonOpenAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_Open_Ack);
		}
	}

	public void WorldDungeon_Continue_Req()
	{
		Instance.WsPackMsg(null, Message.WorldDungeonContinueReq);
	}

	public void WorldDungeon_Continue_Ack(ByteString response)
	{
		WorldDungeonContinueAck worldDungeonContinueAck = WorldDungeonContinueAck.Parser.ParseFrom(response);
		if (worldDungeonContinueAck == null)
		{
			return;
		}
		zbSelf.Instance.worldDungeonHandler.OnWorldDungeonOpenAck(worldDungeonContinueAck.DungeonBaseInfo, worldDungeonContinueAck.DungeonMapInfo, worldDungeonContinueAck.DungeonAwardInfo);
		zbSelf.Instance.worldDungeonHandler.OnTriggerEventCheck();
		zbCombatRecorder zbCombatRecorder2 = new zbCombatRecorder();
		zbCombatRecorder2.Init();
		if (zbCombatRecorder2.Load())
		{
			BattleBaseInfo battleBaseInfo = zbCombatRecorder2.loadedData.recordCombatSystem.battleInfo.BattleBaseInfo;
			if (battleBaseInfo.BattleType == EBattleFuncType.DungeonBossFight || battleBaseInfo.BattleType == EBattleFuncType.DungeonEasyFight || battleBaseInfo.BattleType == EBattleFuncType.DungeonRiotFight)
			{
				zbCombatRecorder2.Restore();
			}
		}
	}

	public void WorldDungeon_Close_Req()
	{
		WorldDungeonCloseReq req = new WorldDungeonCloseReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldDungeonCloseReq);
	}

	public void WorldDungeon_Close_Ack(ByteString response)
	{
		WorldDungeonCloseAck worldDungeonCloseAck = WorldDungeonCloseAck.Parser.ParseFrom(response);
		if (worldDungeonCloseAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(worldDungeonCloseAck.DataChange);
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonCloseAck.DungeonBaseInfo, worldDungeonCloseAck.DungeonMapInfo, worldDungeonCloseAck.DungeonAwardInfo, ((IEnumerable<AwardDetail>)worldDungeonCloseAck.DungeonAwardInfo.AwardList).ToArray(), worldDungeonCloseAck.DungeonResultInfo);
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldDungeonCloseAck.WorldInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_Close_Ack);
		}
	}

	public void WorldDungeon_OpenCell_Req(string FromCellId, string OpenCellId)
	{
		WorldDungeonOpenCellReq worldDungeonOpenCellReq = new WorldDungeonOpenCellReq();
		worldDungeonOpenCellReq.FromCellId = FromCellId;
		worldDungeonOpenCellReq.OpenCellId = OpenCellId;
		Instance.WsPackMsg((IMessage)(object)worldDungeonOpenCellReq, Message.WorldDungeonOpenCellReq);
	}

	public void WorldDungeon_OpenCell_Ack(ByteString response)
	{
		WorldDungeonOpenCellAck worldDungeonOpenCellAck = WorldDungeonOpenCellAck.Parser.ParseFrom(response);
		if (worldDungeonOpenCellAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonOpenCellAck.DungeonBaseInfo, worldDungeonOpenCellAck.DungeonMapInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_UpdateMap);
			zbSelf.Instance.worldDungeonHandler.OnTriggerEventCheck();
		}
	}

	public void WorldDungeon_Move_Req(DungeonRoleSiteInfo dungeonRoleSiteInfo)
	{
		WorldDungeonMoveReq worldDungeonMoveReq = new WorldDungeonMoveReq();
		worldDungeonMoveReq.RoleSiteInfo = dungeonRoleSiteInfo;
		Instance.WsPackMsg((IMessage)(object)worldDungeonMoveReq, Message.WorldDungeonMoveReq, needTip: false);
	}

	public void WorldDungeon_Move_Ack(ByteString response)
	{
		WorldDungeonMoveAck worldDungeonMoveAck = WorldDungeonMoveAck.Parser.ParseFrom(response);
		if (worldDungeonMoveAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonMoveAck.DungeonBaseInfo, worldDungeonMoveAck.DungeonMapInfo);
		}
	}

	public void WorldDungeon_LogInfo_Req()
	{
		WorldDungeonLogInfoReq req = new WorldDungeonLogInfoReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldDungeonLogInfoReq);
	}

	public void WorldDungeon_LogInfo_Ack(ByteString response)
	{
		WorldDungeonLogInfoAck worldDungeonLogInfoAck = WorldDungeonLogInfoAck.Parser.ParseFrom(response);
		if (worldDungeonLogInfoAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpdateDungeonLogInfo(worldDungeonLogInfoAck.DungeonLogInfo);
			zbUIMgr.Instance.ShowDungeonLog(((IEnumerable<AwardDetail>)zbSelf.Instance.worldDungeonHandler.dungeonAwardInfo.AwardList).ToArray(), worldDungeonLogInfoAck.DungeonLogInfo);
		}
	}

	public void WorldDungeon_EventTrigger_Req(string CellId, int GridId)
	{
		WorldDungeonEventTriggerReq worldDungeonEventTriggerReq = new WorldDungeonEventTriggerReq();
		worldDungeonEventTriggerReq.CellId = CellId;
		worldDungeonEventTriggerReq.GridId = GridId;
		Instance.WsPackMsg((IMessage)(object)worldDungeonEventTriggerReq, Message.WorldDungeonEventTriggerReq);
	}

	public void WorldDungeon_EventFightStart_Ack(ByteString response)
	{
		WorldDungeonEventFightStartAck worldDungeonEventFightStartAck = WorldDungeonEventFightStartAck.Parser.ParseFrom(response);
		if (worldDungeonEventFightStartAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonEventFightStartAck.DungeonBaseInfo, worldDungeonEventFightStartAck.DungeonMapInfo);
			zbSelf.Instance.worldDungeonHandler.OnTriggerFight(worldDungeonEventFightStartAck.BattleInfo);
		}
	}

	public void WorldDungeon_EventModRand_Ack(ByteString response)
	{
		WorldDungeonEventModRandAck worldDungeonEventModRandAck = WorldDungeonEventModRandAck.Parser.ParseFrom(response);
		if (worldDungeonEventModRandAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonEventModRandAck.DungeonBaseInfo, worldDungeonEventModRandAck.DungeonMapInfo);
			zbSelf.Instance.worldDungeonHandler.OnTriggerModRand();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_UpdateMap);
		}
	}

	public void WorldDungeon_EventPopup_Ack(ByteString response)
	{
		WorldDungeonEventPopupAck worldDungeonEventPopupAck = WorldDungeonEventPopupAck.Parser.ParseFrom(response);
		if (worldDungeonEventPopupAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonEventPopupAck.DungeonBaseInfo, worldDungeonEventPopupAck.DungeonMapInfo);
			zbSelf.Instance.worldDungeonHandler.OnTriggerPopUp();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_UpdateMap);
		}
	}

	public void WorldDungeon_EventFightResult_Req(BattleResultInfo BattleResult)
	{
		WorldDungeonEventFightResultReq worldDungeonEventFightResultReq = new WorldDungeonEventFightResultReq();
		worldDungeonEventFightResultReq.BattleResult = BattleResult;
		Instance.WsPackMsg((IMessage)(object)worldDungeonEventFightResultReq, Message.WorldDungeonEventFightResultReq);
	}

	public void WorldDungeon_EventFightResult_Ack(ByteString response)
	{
		WorldDungeonEventFightResultAck worldDungeonEventFightResultAck = WorldDungeonEventFightResultAck.Parser.ParseFrom(response);
		if (worldDungeonEventFightResultAck == null)
		{
			return;
		}
		RepeatedField<AwardDetail> awardList = zbSelf.Instance.worldDungeonHandler.dungeonAwardInfo.AwardList;
		RepeatedField<AwardDetail> awardList2 = worldDungeonEventFightResultAck.DungeonAwardInfo.AwardList;
		zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonEventFightResultAck.DungeonBaseInfo, worldDungeonEventFightResultAck.DungeonMapInfo, worldDungeonEventFightResultAck.DungeonAwardInfo);
		List<AwardDetail> list = new List<AwardDetail>((IEnumerable<AwardDetail>)awardList2);
		foreach (AwardDetail existing in awardList)
		{
			AwardDetail awardDetail = list.FirstOrDefault((AwardDetail a) => a.Type == existing.Type && a.Tid == existing.Tid && a.Count == existing.Count);
			if (awardDetail != null)
			{
				list.Remove(awardDetail);
			}
		}
		RepeatedField<AwardDetail> val = new RepeatedField<AwardDetail>();
		val.Add((IEnumerable<AwardDetail>)list);
		zbCombatSystem.Instance.battleAwardList = val;
		zbCombatSystem.Instance.SetBanlanceData(worldDungeonEventFightResultAck.BattleSettle);
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_UpdateMap);
		zbSelf.Instance.worldDungeonHandler.OnTriggerEventCheck();
		zbSelf.Instance.worldHandler.UpdateWorldInfo(worldDungeonEventFightResultAck.WorldInfo);
	}

	public void WorldDungeon_EventModSelect_Req(int modId)
	{
		WorldDungeonEventModSelectReq worldDungeonEventModSelectReq = new WorldDungeonEventModSelectReq();
		worldDungeonEventModSelectReq.SelectId = modId;
		Instance.WsPackMsg((IMessage)(object)worldDungeonEventModSelectReq, Message.WorldDungeonEventModSelectReq);
	}

	public void WorldDungeon_EventModSelect_Ack(ByteString response)
	{
		WorldDungeonEventModSelectAck worldDungeonEventModSelectAck = WorldDungeonEventModSelectAck.Parser.ParseFrom(response);
		if (worldDungeonEventModSelectAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonEventModSelectAck.DungeonBaseInfo, worldDungeonEventModSelectAck.DungeonMapInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_UpdateMap);
			zbSelf.Instance.worldDungeonHandler.OnTriggerEventCheck();
		}
	}

	public void WorldDungeon_EventPopupConfirm_Req()
	{
		WorldDungeonEventPopupConfirmReq req = new WorldDungeonEventPopupConfirmReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldDungeonEventPopupConfirmReq);
	}

	public void WorldDungeon_EventPopupConfirm_Ack(ByteString response)
	{
		WorldDungeonEventPopupConfirmAck worldDungeonEventPopupConfirmAck = WorldDungeonEventPopupConfirmAck.Parser.ParseFrom(response);
		if (worldDungeonEventPopupConfirmAck != null)
		{
			zbSelf.Instance.worldDungeonHandler.UpDateDungeonMapInfo(worldDungeonEventPopupConfirmAck.DungeonBaseInfo, worldDungeonEventPopupConfirmAck.DungeonMapInfo, worldDungeonEventPopupConfirmAck.DungeonAwardInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_UpdateMap);
			zbSelf.Instance.worldDungeonHandler.OnDungeonPassStateCheck();
			zbSelf.Instance.worldDungeonHandler.OnTriggerEventCheck();
		}
	}

	public void WorldEvent_Radar_Req()
	{
		WorldEventRadarReq req = new WorldEventRadarReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldEventRadarReq);
	}

	public void WorldEvent_Radar_Ack(ByteString response)
	{
		WorldEventRadarAck worldEventRadarAck = WorldEventRadarAck.Parser.ParseFrom(response);
		if (worldEventRadarAck != null)
		{
			zbSelf.Instance.worldHandler.worldEventRadarAckData = worldEventRadarAck;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldEventRadar_Ack);
		}
	}

	public void WorldEvent_AwardBox_Req(int gridId)
	{
		WorldEventAwardBoxReq worldEventAwardBoxReq = new WorldEventAwardBoxReq();
		worldEventAwardBoxReq.GridId = gridId;
		Instance.WsPackMsg((IMessage)(object)worldEventAwardBoxReq, Message.WorldEventAwardBoxReq);
	}

	public void WorldEvent_AwardBox_Ack(ByteString response)
	{
		WorldEventAwardBoxAck worldEventAwardBoxAck = WorldEventAwardBoxAck.Parser.ParseFrom(response);
		if (worldEventAwardBoxAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldEventAwardBoxAck.WorldInfo);
			zbSelf.Instance.dataHandler.DataChange(worldEventAwardBoxAck.DataChange, ((IEnumerable<AwardDetail>)worldEventAwardBoxAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldEventAwardBox_Ack);
		}
	}

	public void WorldEvent_FightStart_Req(int gridId)
	{
		WorldEventFightStartReq worldEventFightStartReq = new WorldEventFightStartReq();
		worldEventFightStartReq.GridId = gridId;
		Instance.WsPackMsg((IMessage)(object)worldEventFightStartReq, Message.WorldEventFightStartReq);
	}

	public void WorldEvent_FightStart_Ack(ByteString response)
	{
		WorldEventFightStartAck worldEventFightStartAck = WorldEventFightStartAck.Parser.ParseFrom(response);
		if (worldEventFightStartAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldEventFightStartAck.WorldInfo);
			zbSelf.Instance.combatHandler.StartShowCombat(worldEventFightStartAck.BattleInfo, worldEventFightStartAck.GridId);
		}
	}

	public void WorldEvent_FightResult_Req(int gridId, BattleResultInfo battleResult)
	{
		WorldEventFightResultReq worldEventFightResultReq = new WorldEventFightResultReq();
		worldEventFightResultReq.GridId = gridId;
		worldEventFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)worldEventFightResultReq, Message.WorldEventFightResultReq);
	}

	public void WorldEvent_FightResult_Ack(ByteString response)
	{
		WorldEventFightResultAck worldEventFightResultAck = WorldEventFightResultAck.Parser.ParseFrom(response);
		if (worldEventFightResultAck != null)
		{
			zbCombatSystem.Instance.battleAwardList = worldEventFightResultAck.AwardList;
			zbCombatSystem.Instance.SetBanlanceData(worldEventFightResultAck.BattleSettle);
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldEventFightResultAck.WorldInfo);
			zbSelf.Instance.dataHandler.DataChange(worldEventFightResultAck.DataChange);
		}
	}

	public void WorldBoss_Data_Req(bool open = false)
	{
		openWorldBoss = open;
		WorldBossDataReq req = new WorldBossDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldBossDataReq);
	}

	public void WorldBoss_Data_Ack(ByteString response)
	{
		WorldBossDataAck worldBossDataAck = WorldBossDataAck.Parser.ParseFrom(response);
		if (worldBossDataAck != null)
		{
			zbServerData.sGameData.WorldBossSelfInfo = worldBossDataAck.WorldBossSelfInfo;
			zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfInfo(worldBossDataAck.WorldBossSelfInfo);
			zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfRank(worldBossDataAck.WorldBossSelfRank);
			zbSelf.Instance.worldBossHandler.UpdateWorldBossInfo(worldBossDataAck.WorldBossInfo);
			if (openWorldBoss)
			{
				zbUIMgr.Instance.ShowWorldBoss();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldBoss_Data);
			}
		}
	}

	public void WorldBoss_FightStart_Req(int uuid)
	{
		WorldBossFightStartReq worldBossFightStartReq = new WorldBossFightStartReq();
		worldBossFightStartReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)worldBossFightStartReq, Message.WorldBossFightStartReq);
	}

	public void WorldBoss_FightStart_Ack(ByteString response)
	{
		WorldBossFightStartAck worldBossFightStartAck = WorldBossFightStartAck.Parser.ParseFrom(response);
		if (worldBossFightStartAck != null)
		{
			zbSelf.Instance.combatHandler.StartShowCombat(worldBossFightStartAck.BattleInfo);
		}
	}

	public void WorldBoss_FightResult_Req(int uuid, BattleResultInfo battleResult, long hurt)
	{
		WorldBossFightResultReq worldBossFightResultReq = new WorldBossFightResultReq();
		worldBossFightResultReq.UUID = uuid;
		worldBossFightResultReq.BattleResult = battleResult;
		worldBossFightResultReq.BossHurt = hurt;
		Instance.WsPackMsg((IMessage)(object)worldBossFightResultReq, Message.WorldBossFightResultReq);
	}

	public void WorldBoss_FightResult_Ack(ByteString response)
	{
		WorldBossFightResultAck worldBossFightResultAck = WorldBossFightResultAck.Parser.ParseFrom(response);
		if (worldBossFightResultAck != null)
		{
			zbCombatSystem.Instance.battleAwardList = worldBossFightResultAck.AwardList;
			zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfInfo(worldBossFightResultAck.WorldBossSelfInfo);
			zbSelf.Instance.worldBossHandler.UpdateWorldBossInfo(worldBossFightResultAck.WorldBossInfo);
			zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfRank(worldBossFightResultAck.WorldBossSelfRank);
			zbSelf.Instance.dataHandler.DataChange(worldBossFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(worldBossFightResultAck.BattleSettle);
		}
	}

	public void WorldBoss_FightReborn_Req(int uuid, string BattleUUID)
	{
		WorldBossFightRebornReq worldBossFightRebornReq = new WorldBossFightRebornReq();
		worldBossFightRebornReq.UUID = uuid;
		worldBossFightRebornReq.BattleUUID = BattleUUID;
		Instance.WsPackMsg((IMessage)(object)worldBossFightRebornReq, Message.WorldBossFightRebornReq);
	}

	public void WorldBoss_FightReborn_Ack(ByteString response)
	{
		WorldBossFightRebornAck worldBossFightRebornAck = WorldBossFightRebornAck.Parser.ParseFrom(response);
		if (worldBossFightRebornAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(worldBossFightRebornAck.DataChange);
			zbCombatSystem.Instance.isReborn = worldBossFightRebornAck.Reborn;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldBoss_Reborn);
		}
	}

	public void WorldBoss_SaoDang_Req(int uuid)
	{
		WorldBossSaoDangReq worldBossSaoDangReq = new WorldBossSaoDangReq();
		worldBossSaoDangReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)worldBossSaoDangReq, Message.WorldBossSaoDangReq);
	}

	public void WorldBoss_SaoDang_Ack(ByteString response)
	{
		WorldBossSaoDangAck worldBossSaoDangAck = WorldBossSaoDangAck.Parser.ParseFrom(response);
		if (worldBossSaoDangAck != null)
		{
			zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfInfo(worldBossSaoDangAck.WorldBossSelfInfo);
			zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfRank(worldBossSaoDangAck.WorldBossSelfRank);
			zbSelf.Instance.worldBossHandler.UpdateWorldBossInfo(worldBossSaoDangAck.WorldBossInfo);
			zbSelf.Instance.dataHandler.DataChange(worldBossSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)worldBossSaoDangAck.AwardList).ToArray());
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldBoss_Data);
		}
	}

	public void WorldBoss_BuyCombat_Req(int count)
	{
		WorldBossBuyCombatReq worldBossBuyCombatReq = new WorldBossBuyCombatReq();
		worldBossBuyCombatReq.Count = count;
		Instance.WsPackMsg((IMessage)(object)worldBossBuyCombatReq, Message.WorldBossBuyCombatReq);
	}

	public void WorldBoss_BuyCombat_Ack(ByteString response)
	{
		WorldBossBuyCombatAck worldBossBuyCombatAck = WorldBossBuyCombatAck.Parser.ParseFrom(response);
		if (worldBossBuyCombatAck != null)
		{
			zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfInfo(worldBossBuyCombatAck.WorldBossSelfInfo);
			zbSelf.Instance.dataHandler.DataChange(worldBossBuyCombatAck.DataChange);
			zbUIMgr.Instance.HideWnd(EWndId.WorldBossBuyCount);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldBoss_Data);
		}
	}

	public void WorldBoss_RankList_Req(int uuid)
	{
		WorldBossRankListReq worldBossRankListReq = new WorldBossRankListReq();
		worldBossRankListReq.UUID = uuid;
		Instance.WsPackMsg((IMessage)(object)worldBossRankListReq, Message.WorldBossRankListReq);
	}

	public void WorldBoss_RankList_Ack(ByteString response)
	{
		WorldBossRankListAck worldBossRankListAck = WorldBossRankListAck.Parser.ParseFrom(response);
		if (worldBossRankListAck != null)
		{
			zbUIMgr.Instance.ShowWorldBossRank(((IEnumerable<RankDetail>)worldBossRankListAck.RankList).ToArray());
		}
	}

	public void WorldBoss_RecordList_Req()
	{
		WorldBossRecordListReq req = new WorldBossRecordListReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldBossRecordListReq);
	}

	public void WorldBoss_RecordList_Ack(ByteString response)
	{
		WorldBossRecordListAck worldBossRecordListAck = WorldBossRecordListAck.Parser.ParseFrom(response);
		if (worldBossRecordListAck != null)
		{
			zbUIMgr.Instance.ShowWorldBossLog(((IEnumerable<WorldBossHurtRecordDetail>)worldBossRecordListAck.RecordList).ToArray());
		}
	}

	public void Feature_List_Req(bool open = false)
	{
		FeatureListReq req = new FeatureListReq();
		m_openChallengeWnd = open;
		Instance.WsPackMsg((IMessage)(object)req, Message.FeatureListReq);
	}

	public void Feature_List_Ack(ByteString response)
	{
		FeatureListAck featureListAck = FeatureListAck.Parser.ParseFrom(response);
		if (featureListAck != null)
		{
			if (featureListAck.FeatureWorldLair != null)
			{
				zbServerData.sGameData.WorldLairInfo = featureListAck.FeatureWorldLair.WorldLairInfo;
				zbSelf.Instance.worldLairHandler.UpdateWorldLairHelpCount(featureListAck.FeatureWorldLair.HelpCount);
			}
			if (featureListAck.FeatureLegionBoss != null)
			{
				zbServerData.sGameData.LegionSelfInfo = featureListAck.FeatureLegionBoss.LegionSelfInfo;
				zbSelf.Instance.legionHandler.UpdateLegionBossInfo(featureListAck.FeatureLegionBoss.LegionBossInfo);
				zbSelf.Instance.legionHandler.UpdateLegionBossSelfRank(featureListAck.FeatureLegionBoss.SelfRank);
			}
			zbSelf.Instance.trialHandler.UpdatTrailInfo(featureListAck.FeatureTrialInfo);
			if (featureListAck.FeatureWorldBoss != null)
			{
				zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfInfo(featureListAck.FeatureWorldBoss.WorldBossSelfInfo);
				zbSelf.Instance.worldBossHandler.UpdateWorldBossInfo(featureListAck.FeatureWorldBoss.WorldBossInfo);
				zbSelf.Instance.worldBossHandler.UpdateWorldBossSelfRank(featureListAck.FeatureWorldBoss.SelfRank);
			}
			if (featureListAck.FeatureEndlessHorde != null)
			{
				zbServerData.sGameData.EndlessHordeInfo = featureListAck.FeatureEndlessHorde.EndlessHordeInfo;
				zbSelf.Instance.endlessHordeHandler.UpdateEndlessHordeSelfRank(featureListAck.FeatureEndlessHorde.SelfRank);
			}
			if (m_openChallengeWnd)
			{
				zbUIMgr.Instance.ShowChallenge();
			}
			else
			{
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Feature_Update);
			}
		}
	}

	public void WorldLair_Data_Req()
	{
		WorldLairDataReq req = new WorldLairDataReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldLairDataReq);
	}

	public void WorldLair_Data_Ack(ByteString response)
	{
		WorldLairDataAck worldLairDataAck = WorldLairDataAck.Parser.ParseFrom(response);
		if (worldLairDataAck != null)
		{
			zbServerData.sGameData.WorldLairInfo = worldLairDataAck.WorldLairInfo;
			zbSelf.Instance.worldLairHandler.m_worldLairHelpInfos = ((IEnumerable<WorldLairHelpInfo>)worldLairDataAck.WorldLairHelpList).ToArray();
			if (worldLairDataAck.WorldLairInfo.SelfLairInfo != null)
			{
				zbGameSave.Instance.SetInt("WorldLairLevel" + zbSelf.Instance.GetRoleId(), worldLairDataAck.WorldLairInfo.SelfLairInfo.Level);
			}
			zbWorldLairWnd zbWorldLairWnd2 = zbUIMgr.Instance.GetWndById(EWndId.WorldLair) as zbWorldLairWnd;
			if ((Object)(object)zbWorldLairWnd2 != (Object)null && zbWorldLairWnd2.IsShow())
			{
				zbWorldLairWnd2.RefreshCustom();
			}
			else
			{
				zbUIMgr.Instance.ShowWorldLair();
			}
		}
	}

	public void WorldLair_SaoDang_Req()
	{
		WorldLairSaoDangReq req = new WorldLairSaoDangReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldLairSaoDangReq);
	}

	public void WorldLair_SaoDang_Ack(ByteString response)
	{
		WorldLairSaoDangAck worldLairSaoDangAck = WorldLairSaoDangAck.Parser.ParseFrom(response);
		if (worldLairSaoDangAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(worldLairSaoDangAck.DataChange, ((IEnumerable<AwardDetail>)worldLairSaoDangAck.AwardList).ToArray());
			zbServerData.sGameData.WorldLairInfo = worldLairSaoDangAck.WorldLairInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WorldLairSaoDang);
		}
	}

	public void WorldLair_FightStart_Req()
	{
		WorldLairFightStartReq req = new WorldLairFightStartReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldLairFightStartReq);
	}

	public void WorldLair_FightStart_Ack(ByteString response)
	{
		WorldLairFightStartAck worldLairFightStartAck = WorldLairFightStartAck.Parser.ParseFrom(response);
		if (worldLairFightStartAck != null)
		{
			zbSelf.Instance.combatHandler.StartShowCombat(worldLairFightStartAck.BattleInfo);
		}
	}

	public void WorldLair_FightResult_Req(BattleResultInfo battleResult)
	{
		WorldLairFightResultReq worldLairFightResultReq = new WorldLairFightResultReq();
		worldLairFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)worldLairFightResultReq, Message.WorldLairFightResultReq);
	}

	public void WorldLair_FightResult_Ack(ByteString response)
	{
		WorldLairFightResultAck worldLairFightResultAck = WorldLairFightResultAck.Parser.ParseFrom(response);
		if (worldLairFightResultAck != null)
		{
			zbServerData.sGameData.WorldLairInfo = worldLairFightResultAck.WorldLairInfo;
			zbCombatSystem.Instance.battleAwardList = worldLairFightResultAck.AwardList;
			zbSelf.Instance.dataHandler.DataChange(worldLairFightResultAck.DataChange);
			zbCombatSystem.Instance.SetBanlanceData(worldLairFightResultAck.BattleSettle);
		}
	}

	public void WorldLair_HelpData_Req(string UUID)
	{
		WorldLairHelpDataReq worldLairHelpDataReq = new WorldLairHelpDataReq();
		worldLairHelpDataReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)worldLairHelpDataReq, Message.WorldLairHelpDataReq);
	}

	public void WorldLair_HelpData_Ack(ByteString response)
	{
		WorldLairHelpDataAck worldLairHelpDataAck = WorldLairHelpDataAck.Parser.ParseFrom(response);
		if (worldLairHelpDataAck != null)
		{
			zbUIMgr.Instance.ShowWorldLairFight(worldLairHelpDataAck.HelpInfo.HelpLairInfo, isHelp: true);
		}
	}

	public void WorldLair_HelpFightStart_Req(string UUId)
	{
		WorldLairHelpFightStartReq worldLairHelpFightStartReq = new WorldLairHelpFightStartReq();
		worldLairHelpFightStartReq.UUID = UUId;
		Instance.WsPackMsg((IMessage)(object)worldLairHelpFightStartReq, Message.WorldLairHelpFightStartReq);
	}

	public void WorldLair_HelpFightStart_Ack(ByteString response)
	{
		WorldLairHelpFightStartAck worldLairHelpFightStartAck = WorldLairHelpFightStartAck.Parser.ParseFrom(response);
		if (worldLairHelpFightStartAck != null)
		{
			zbSelf.Instance.worldLairHandler.UpdateWorldLairUuid(worldLairHelpFightStartAck.UUID);
			zbSelf.Instance.combatHandler.StartShowCombat(worldLairHelpFightStartAck.BattleInfo);
		}
	}

	public void WorldLair_HelpFightResult_Req(string uuid, BattleResultInfo battleResult)
	{
		WorldLairHelpFightResultReq worldLairHelpFightResultReq = new WorldLairHelpFightResultReq();
		worldLairHelpFightResultReq.UUID = uuid;
		worldLairHelpFightResultReq.BattleResult = battleResult;
		Instance.WsPackMsg((IMessage)(object)worldLairHelpFightResultReq, Message.WorldLairHelpFightResultReq);
	}

	public void WorldLair_HelpFightResult_Ack(ByteString response)
	{
		WorldLairHelpFightResultAck worldLairHelpFightResultAck = WorldLairHelpFightResultAck.Parser.ParseFrom(response);
		if (worldLairHelpFightResultAck != null)
		{
			zbSelf.Instance.dataHandler.DataChange(worldLairHelpFightResultAck.DataChange);
			zbCombatSystem.Instance.battleAwardList = worldLairHelpFightResultAck.AwardList;
			zbServerData.sGameData.WorldLairInfo = worldLairHelpFightResultAck.WorldLairInfo;
			zbSelf.Instance.worldLairHandler.m_worldLairInfo = worldLairHelpFightResultAck.HelpInfo.HelpLairInfo;
			zbSelf.Instance.worldLairHandler.m_worldLairHelpRoleBaseInfo = worldLairHelpFightResultAck.HelpInfo.HelpRoleInfo;
			zbCombatSystem.Instance.SetBanlanceData(worldLairHelpFightResultAck.BattleSettle);
			zbSelf.Instance.worldLairHandler.UpdateWorldLairUuid(null);
		}
	}

	public void WorldLair_FightRecord_Req(string UUID)
	{
		WorldLairFightRecordReq worldLairFightRecordReq = new WorldLairFightRecordReq();
		worldLairFightRecordReq.UUID = UUID;
		Instance.WsPackMsg((IMessage)(object)worldLairFightRecordReq, Message.WorldLairFightRecordReq);
	}

	public void WorldLair_FightRecord_Ack(ByteString response)
	{
		WorldLairFightRecordAck worldLairFightRecordAck = WorldLairFightRecordAck.Parser.ParseFrom(response);
		if (worldLairFightRecordAck != null)
		{
			zbUIMgr.Instance.ShowWorldLairRecord(((IEnumerable<WorldLairFightRecordDetail>)worldLairFightRecordAck.RecordList).ToArray());
		}
	}

	public void WorldLair_BuyFightNum_Req(int Count = 1)
	{
		WorldLairBuyFightNumReq worldLairBuyFightNumReq = new WorldLairBuyFightNumReq();
		worldLairBuyFightNumReq.Count = Count;
		Instance.WsPackMsg((IMessage)(object)worldLairBuyFightNumReq, Message.WorldLairBuyFightNumReq);
	}

	public void WorldLair_BuyFightNum_Ack(ByteString response)
	{
		WorldLairBuyFightNumAck worldLairBuyFightNumAck = WorldLairBuyFightNumAck.Parser.ParseFrom(response);
		if (worldLairBuyFightNumAck != null)
		{
			zbServerData.sGameData.WorldLairInfo = worldLairBuyFightNumAck.WorldLairInfo;
			zbSelf.Instance.dataHandler.DataChange(worldLairBuyFightNumAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WorldlairBuyNum);
		}
	}

	public void WorldLair_Refresh_Req()
	{
		WorldLairRefreshReq req = new WorldLairRefreshReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldLairRefreshReq);
	}

	public void WorldLair_Refresh_Ack(ByteString response)
	{
		WorldLairRefreshAck worldLairRefreshAck = WorldLairRefreshAck.Parser.ParseFrom(response);
		if (worldLairRefreshAck != null)
		{
			zbServerData.sGameData.WorldLairInfo = worldLairRefreshAck.WorldLairInfo;
			zbSelf.Instance.worldLairHandler.m_worldLairHelpInfos = ((IEnumerable<WorldLairHelpInfo>)worldLairRefreshAck.WorldLairHelpList).ToArray();
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_WorldLairRefresh);
		}
	}

	public void World_ReconExplore_Req(int teamId, int gridId)
	{
		zbSelf.Instance.newTagHandler.AddNewGridId(gridId.ToString());
		WorldReconExploreReq worldReconExploreReq = new WorldReconExploreReq();
		worldReconExploreReq.ReconId = teamId;
		worldReconExploreReq.GridId = gridId;
		Instance.WsPackMsg((IMessage)(object)worldReconExploreReq, Message.WorldReconExploreReq);
	}

	public void World_ReconExplore_Ack(ByteString response)
	{
		WorldReconExploreAck worldReconExploreAck = WorldReconExploreAck.Parser.ParseFrom(response);
		if (worldReconExploreAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldReconExploreAck.WorldInfo);
			zbSelf.Instance.dataHandler.DataChange(worldReconExploreAck.DataChange);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldReconExplore_Ack);
		}
	}

	public void World_ReconExplored_Req(int teamId)
	{
		WorldReconExploredReq worldReconExploredReq = new WorldReconExploredReq();
		worldReconExploredReq.ReconId = teamId;
		Instance.WsPackMsg((IMessage)(object)worldReconExploredReq, Message.WorldReconExploredReq);
	}

	public void World_ReconExplored_Ack(ByteString response)
	{
		WorldReconExploredAck worldReconExploredAck = WorldReconExploredAck.Parser.ParseFrom(response);
		if (worldReconExploredAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldReconExploredAck.WorldInfo);
			zbUIMgr.Instance.ShowWorldReconExplored(((IEnumerable<AwardDetail>)worldReconExploredAck.AwardList).ToArray());
			zbSelf.Instance.dataHandler.DataChange(worldReconExploredAck.DataChange);
		}
	}

	public void World_ReconExploreCancel_Req(int teamId)
	{
		WorldReconExploreCancelReq worldReconExploreCancelReq = new WorldReconExploreCancelReq();
		worldReconExploreCancelReq.ReconId = teamId;
		Instance.WsPackMsg((IMessage)(object)worldReconExploreCancelReq, Message.WorldReconExploreCancelReq);
	}

	public void World_ReconExploreCancel_Ack(ByteString response)
	{
		WorldReconExploreCancelAck worldReconExploreCancelAck = WorldReconExploreCancelAck.Parser.ParseFrom(response);
		if (worldReconExploreCancelAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldReconExploreCancelAck.WorldInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldReconExploreCancel_Ack);
		}
	}

	public void WorldResource_Activate_Req(int GridId)
	{
		WorldResourceActivateReq worldResourceActivateReq = new WorldResourceActivateReq();
		worldResourceActivateReq.GridId = GridId;
		Instance.WsPackMsg((IMessage)(object)worldResourceActivateReq, Message.WorldResourceActivateReq);
	}

	public void WorldResource_Activate_Ack(ByteString response)
	{
		WorldResourceActivateAck worldResourceActivateAck = WorldResourceActivateAck.Parser.ParseFrom(response);
		if (worldResourceActivateAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldResourceActivateAck.WorldInfo);
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_WorldResourceActivate_Ack);
		}
	}

	public void WorldResource_IdleAward_Req()
	{
		WorldResourceIdleAwardReq req = new WorldResourceIdleAwardReq();
		Instance.WsPackMsg((IMessage)(object)req, Message.WorldResourceIdleAwardReq);
	}

	public void WorldResource_IdleAward_Ack(ByteString response)
	{
		WorldResourceIdleAwardAck worldResourceIdleAwardAck = WorldResourceIdleAwardAck.Parser.ParseFrom(response);
		if (worldResourceIdleAwardAck != null)
		{
			zbSelf.Instance.worldHandler.UpdateWorldInfo(worldResourceIdleAwardAck.WorldInfo);
			zbSelf.Instance.dataHandler.DataChange(worldResourceIdleAwardAck.DataChange, ((IEnumerable<AwardDetail>)worldResourceIdleAwardAck.AwardList).ToArray());
		}
	}

	private bool IsCanHeart()
	{
		bool flag = true;
		zbTipConnectWnd zbTipConnectWnd2 = zbUIMgr.Instance.GetWndById(EWndId.TipConnect) as zbTipConnectWnd;
		if ((Object)null != (Object)(object)zbTipConnectWnd2 && zbTipConnectWnd2.IsShow())
		{
			flag = false;
		}
		if (flag && zbGameStateMachine.Instance.IsFighting())
		{
			flag = false;
		}
		return flag;
	}

	private void InitMsgHandle()
	{
		RegisterCallback(Message.ErrorAck, Error_Ack);
		RegisterCallback(Message.HeartBeatAck, HeartBeat_Ack);
		RegisterCallback(Message.GameKickAck, Game_Kick_Ack);
		RegisterCallback(Message.GameLoginAck, Game_Login_Ack);
		RegisterCallback(Message.RoleReadyAck, Role_Ready_Ack);
		RegisterCallback(Message.RoleCreateAck, Role_Create_Ack);
		RegisterCallback(Message.GameLoginOutAck, Game_LoginOut_Ack);
		RegisterCallback(Message.SyncNtf, MessageSync_Ntf);
		RegisterCallback(Message.ResetRoleNameAck, Reset_RoleName_Ack);
		RegisterCallback(Message.RewardsRedeemCodeAck, Rewards_RedeemCode_Ack);
		RegisterCallback(Message.AbnormalClientReportAck, Abnormal_ClientReport_Ack);
		RegisterCallback(Message.MainQuestDataAck, MainQuest_Data_Ack);
		RegisterCallback(Message.MainQuestFightStartAck, MainQuest_FightStart_Ack);
		RegisterCallback(Message.MainQuestFightResultAck, MainQuest_FightResult_Ack);
		RegisterCallback(Message.MainQuestScoreAwardAck, MainQuest_ScoreAward_Ack);
		RegisterCallback(Message.HeroTrialDataAck, HeroTrial_Data_Ack);
		RegisterCallback(Message.HeroTrialFightStartAck, HeroTrial_FightStart_Ack);
		RegisterCallback(Message.HeroTrialFightResultAck, HeroTrial_FightResult_Ack);
		RegisterCallback(Message.ElementTrialDataAck, ElementTrial_Data_Ack);
		RegisterCallback(Message.ElementTrialFightStartAck, ElementTrial_FightStart_Ack);
		RegisterCallback(Message.ElementTrialFightResultAck, ElementTrial_FightResult_Ack);
		RegisterCallback(Message.ElementTrialBuyCombatAck, ElementTrial_BuyCombat_Ack);
		RegisterCallback(Message.ElementTrialSaoDangAck, ElementTrial_SaoDang_Ack);
		RegisterCallback(Message.RechargeActiveZhanLingElementAwardAck, RechargeActive_ZhanLingElementAward_Ack);
		RegisterCallback(Message.ElementTrialRefreshAck, ElementTrial_Refresh_Ack);
		RegisterCallback(Message.ElementTrialUpgradeAck, ElementTrial_Upgrade_Ack);
		RegisterCallback(Message.IdleDataAck, Idle_Data_Ack);
		RegisterCallback(Message.IdleGainAck, Idle_Gain_Ack);
		RegisterCallback(Message.IdleFastGainAck, Idle_FastGain_Ack);
		RegisterCallback(Message.EliteQuestDataAck, EliteQuest_Data_Ack);
		RegisterCallback(Message.EliteQuestFightStartAck, EliteQuest_FightStart_Ack);
		RegisterCallback(Message.EliteQuestFightResultAck, EliteQuest_FightResult_Ack);
		RegisterCallback(Message.EliteQuestBuyCombatAck, EliteQuest_BuyCombat_Ack);
		RegisterCallback(Message.EliteQuestScoreAwardAck, EliteQuest_ScoreAward_Ack);
		RegisterCallback(Message.EliteQuestHelpRecordListAck, EliteQuest_HelpRecordList_Ack);
		RegisterCallback(Message.EliteQuestHelpAwardAck, EliteQuest_HelpAward_Ack);
		RegisterCallback(Message.EliteQuestHelpAwardFastAck, EliteQuest_HelpAwardFast_Ack);
		RegisterCallback(Message.MailGetListAck, Mail_GetList_Ack);
		RegisterCallback(Message.MailReadAck, Mail_Read_Ack);
		RegisterCallback(Message.MailGetAwardAck, Mail_GetAward_Ack);
		RegisterCallback(Message.MailDeleteAck, Mail_Delete_Ack);
		RegisterCallback(Message.MailGetAllAwardAck, Mail_GetAllAward_Ack);
		RegisterCallback(Message.FriendsListAck, Friends_List_Ack);
		RegisterCallback(Message.FriendsRecommendAck, Friends_Recommend_Ack);
		RegisterCallback(Message.FriendsSearchAck, Friends_Search_Ack);
		RegisterCallback(Message.FriendsDeleteAck, Friends_Delete_Ack);
		RegisterCallback(Message.FriendsApplyAck, Friends_Apply_Ack);
		RegisterCallback(Message.FriendsApplyBatchAck, Friends_ApplyBatch_Ack);
		RegisterCallback(Message.FriendsApplyListAck, Friends_ApplyList_Ack);
		RegisterCallback(Message.FriendsApplyPassAck, Friends_ApplyPass_Ack);
		RegisterCallback(Message.FriendsApplyPassAllAck, Friends_ApplyPassAll_Ack);
		RegisterCallback(Message.FriendsApplyRefuseAck, Friends_ApplyRefuse_Ack);
		RegisterCallback(Message.FriendsApplyRefuseAllAck, Friends_ApplyRefuseAll_Ack);
		RegisterCallback(Message.FriendsActionGiveAck, Friends_ActionGive_Ack);
		RegisterCallback(Message.FriendsActionCollectAck, Friends_ActionCollect_Ack);
		RegisterCallback(Message.FriendsActionGiveAllAck, Friends_ActionGiveAll_Ack);
		RegisterCallback(Message.FriendsActionCollectAllAck, Friends_ActionCollectAll_Ack);
		RegisterCallback(Message.BarDataAck, Bar_Data_Ack);
		RegisterCallback(Message.BarDrawAck, Bar_Draw_Ack);
		RegisterCallback(Message.BarTargetAwardAck, Bar_TargetAward_Ack);
		RegisterCallback(Message.ShopDataAck, Shop_Data_Ack);
		RegisterCallback(Message.ShopBuyAck, Shop_Buy_Ack);
		RegisterCallback(Message.RechargeActiveDailyPackAck, RechargeActive_DailyPack_Ack);
		RegisterCallback(Message.AuxFuncUnlockAwardAck, Aux_FuncUnlockAward_Ack);
		RegisterCallback(Message.PayCreateOrderAck, Pay_CreateOrder_Ack);
		RegisterCallback(Message.PayCreateOrderSignAck, Pay_CreateOrderSign_Ack);
		RegisterCallback(Message.PayVerifyOrderAck, Pay_VerifyOrder_Ack);
		RegisterCallback(Message.PayVerifyOrderV2Ack, Pay_VerifyOrderV2_Ack);
		RegisterCallback(Message.PaySuccessOrderAck, Pay_SuccessOrder_Ack);
		RegisterCallback(Message.PayVoucherConsumeAck, Pay_VoucherConsume_Ack);
		RegisterCallback(Message.VipDataAck, Vip_Data_Ack);
		RegisterCallback(Message.VipFreeExpAck, Vip_FreeExp_Ack);
		RegisterCallback(Message.VipDailyFreeAck, Vip_DailyFree_Ack);
		RegisterCallback(Message.AccountRebateAwardAck, Account_RebateAward_Ack);
		RegisterCallback(Message.HeroComposeAck, Hero_Compose_Ack);
		RegisterCallback(Message.HeroQualityUpgradeAck, Hero_QualityUpgrade_Ack);
		RegisterCallback(Message.HeroFragDecomposeAck, Hero_FragDecompose_Ack);
		RegisterCallback(Message.HeroRarityUpgradeAck, Hero_RarityUpgrade_Ack);
		RegisterCallback(Message.HeroAwakenUpgradeAck, HeroAwaken_Upgrade_Ack);
		RegisterCallback(Message.HeroAwakenAdvanceAck, HeroAwaken_Advance_Ack);
		RegisterCallback(Message.HeroAwakenGeneOpenHoleAck, HeroAwaken_GeneOpenHole_Ack);
		RegisterCallback(Message.HeroAwakenGeneRefreshHoleAck, HeroAwaken_GeneRefreshHole_Ack);
		RegisterCallback(Message.HeroAwakenGeneSetAck, HeroAwaken_GeneSet_Ack);
		RegisterCallback(Message.HeroAwakenGeneRemoveAck, HeroAwaken_GeneRemove_Ack);
		RegisterCallback(Message.HeroAwakenGeneComposeAck, HeroAwaken_GeneCompose_Ack);
		RegisterCallback(Message.HeroFollowUpgradeAck, HeroFollow_Upgrade_Ack);
		RegisterCallback(Message.EquipSlotLevelUpgradeAck, EquipSlot_LevelUpgrade_Ack);
		RegisterCallback(Message.EquipSlotLevelUpgradeAllAck, EquipSlot_LevelUpgradeAll_Ack);
		RegisterCallback(Message.EquipSlotEquipSetAck, EquipSlot_EquipSet_Ack);
		RegisterCallback(Message.EquipSlotEquipUpgradeAck, EquipSlot_EquipUpgrade_Ack);
		RegisterCallback(Message.EquipSlotEquipLockAck, EquipSlot_EquipLock_Ack);
		RegisterCallback(Message.EquipSlotGemSetAck, EquipSlot_GemSet_Ack);
		RegisterCallback(Message.EquipSlotGemReplaceAck, EquipSlot_GemReplace_Ack);
		RegisterCallback(Message.EquipSlotGemRemoveAck, EquipSlot_GemRemove_Ack);
		RegisterCallback(Message.EquipSlotGemPlanAck, EquipSlot_GemPlan_Ack);
		RegisterCallback(Message.EquipDecomposeAck, Equip_Decompose_Ack);
		RegisterCallback(Message.EquipLockAck, Equip_Lock_Ack);
		RegisterCallback(Message.GemComposePreviewAck, Gem_ComposePreview_Ack);
		RegisterCallback(Message.GemComposeAck, Gem_Compose_Ack);
		RegisterCallback(Message.GemLockAck, Gem_Lock_Ack);
		RegisterCallback(Message.GemDecomposeAck, Gem_Decompose_Ack);
		RegisterCallback(Message.GemCastAck, Gem_Cast_Ack);
		RegisterCallback(Message.HomelandV2DataAck, HomelandV2_Data_Ack);
		RegisterCallback(Message.HomelandV2DataSyncAck, HomelandV2_DataSync_Ack);
		RegisterCallback(Message.HomelandV2FloorUnlockAck, HomelandV2_FloorUnlock_Ack);
		RegisterCallback(Message.HomelandV2RoomCreateAck, HomelandV2_RoomCreate_Ack);
		RegisterCallback(Message.HomelandV2RoomRepairAck, HomelandV2_RoomRepair_Ack);
		RegisterCallback(Message.HomelandV2RoomUpgradeAck, HomelandV2_RoomUpgrade_Ack);
		RegisterCallback(Message.HomelandV2RoomUpgradeFastAck, HomelandV2_RoomUpgradeFast_Ack);
		RegisterCallback(Message.HomelandV2RoomDestroyAck, HomelandV2_RoomDestroy_Ack);
		RegisterCallback(Message.HomelandV2RoomMoveAck, HomelandV2_RoomMove_Ack);
		RegisterCallback(Message.HomelandV2RoomComposeAck, HomelandV2_RoomCompose_Ack);
		RegisterCallback(Message.HomelandV2RoomEditAck, HomelandV2_RoomEdit_Ack);
		RegisterCallback(Message.HomelandV2ReplaceWorkerAck, HomelandV2_ReplaceWorker_Ack);
		RegisterCallback(Message.HomelandV2ReplaceWorkerSelectAck, HomelandV2_ReplaceWorkerSelect_Ack);
		RegisterCallback(Message.WarRoomHeroSetAck, WarRoom_HeroSet_Ack);
		RegisterCallback(Message.WorldDataAck, World_Data_Ack);
		RegisterCallback(Message.WorldDungeonOpenAck, WorldDungeon_Open_Ack);
		RegisterCallback(Message.WorldDungeonContinueAck, WorldDungeon_Continue_Ack);
		RegisterCallback(Message.WorldDungeonCloseAck, WorldDungeon_Close_Ack);
		RegisterCallback(Message.WorldDungeonOpenCellAck, WorldDungeon_OpenCell_Ack);
		RegisterCallback(Message.WorldDungeonMoveAck, WorldDungeon_Move_Ack);
		RegisterCallback(Message.WorldDungeonEventFightStartAck, WorldDungeon_EventFightStart_Ack);
		RegisterCallback(Message.WorldDungeonEventModRandAck, WorldDungeon_EventModRand_Ack);
		RegisterCallback(Message.WorldDungeonEventPopupAck, WorldDungeon_EventPopup_Ack);
		RegisterCallback(Message.WorldDungeonEventFightResultAck, WorldDungeon_EventFightResult_Ack);
		RegisterCallback(Message.WorldDungeonEventModSelectAck, WorldDungeon_EventModSelect_Ack);
		RegisterCallback(Message.WorldDungeonEventPopupConfirmAck, WorldDungeon_EventPopupConfirm_Ack);
		RegisterCallback(Message.WorldEventRadarAck, WorldEvent_Radar_Ack);
		RegisterCallback(Message.WorldEventAwardBoxAck, WorldEvent_AwardBox_Ack);
		RegisterCallback(Message.WorldEventFightStartAck, WorldEvent_FightStart_Ack);
		RegisterCallback(Message.WorldEventFightResultAck, WorldEvent_FightResult_Ack);
		RegisterCallback(Message.WorldDungeonLogInfoAck, WorldDungeon_LogInfo_Ack);
		RegisterCallback(Message.WorldBossDataAck, WorldBoss_Data_Ack);
		RegisterCallback(Message.WorldBossFightStartAck, WorldBoss_FightStart_Ack);
		RegisterCallback(Message.WorldBossFightRebornAck, WorldBoss_FightReborn_Ack);
		RegisterCallback(Message.WorldBossFightResultAck, WorldBoss_FightResult_Ack);
		RegisterCallback(Message.WorldBossSaoDangAck, WorldBoss_SaoDang_Ack);
		RegisterCallback(Message.WorldBossBuyCombatAck, WorldBoss_BuyCombat_Ack);
		RegisterCallback(Message.WorldBossRankListAck, WorldBoss_RankList_Ack);
		RegisterCallback(Message.WorldBossRecordListAck, WorldBoss_RecordList_Ack);
		RegisterCallback(Message.FeatureListAck, Feature_List_Ack);
		RegisterCallback(Message.WorldLairDataAck, WorldLair_Data_Ack);
		RegisterCallback(Message.WorldLairFightStartAck, WorldLair_FightStart_Ack);
		RegisterCallback(Message.WorldLairFightResultAck, WorldLair_FightResult_Ack);
		RegisterCallback(Message.WorldLairHelpDataAck, WorldLair_HelpData_Ack);
		RegisterCallback(Message.WorldLairHelpFightStartAck, WorldLair_HelpFightStart_Ack);
		RegisterCallback(Message.WorldLairHelpFightResultAck, WorldLair_HelpFightResult_Ack);
		RegisterCallback(Message.WorldLairSaoDangAck, WorldLair_SaoDang_Ack);
		RegisterCallback(Message.WorldLairFightRecordAck, WorldLair_FightRecord_Ack);
		RegisterCallback(Message.WorldLairBuyFightNumAck, WorldLair_BuyFightNum_Ack);
		RegisterCallback(Message.WorldLairRefreshAck, WorldLair_Refresh_Ack);
		RegisterCallback(Message.BagItemSaleAck, Bag_ItemSale_Ack);
		RegisterCallback(Message.BagLimitItemSaleAck, Bag_LimitItemSale_Ack);
		RegisterCallback(Message.BagItemComposeAck, Bag_ItemCompose_Ack);
		RegisterCallback(Message.BagRandBoxOpenAck, Bag_RandBoxOpen_Ack);
		RegisterCallback(Message.BagSelectBoxViewAck, Bag_SelectBoxView_Ack);
		RegisterCallback(Message.BagSelectBoxOpenAck, Bag_SelectBoxOpen_Ack);
		RegisterCallback(Message.BagComposeBoxOpenAck, Bag_ComposeBoxOpen_Ack);
		RegisterCallback(Message.BagGeneComposeCalcAck, Bag_GeneComposeCalc_Ack);
		RegisterCallback(Message.BagGeneComposeAck, Bag_GeneCompose_Ack);
		RegisterCallback(Message.BagHeadFrameActiveAck, Bag_HeadFrameActive_Ack);
		RegisterCallback(Message.ResetHeadIdAck, Reset_HeadId_Ack);
		RegisterCallback(Message.ResetHeadFrameIdAck, Reset_HeadFrameId_Ack);
		RegisterCallback(Message.AuxExchangeMoneyAck, Aux_ExchangeMoney_Ack);
		RegisterCallback(Message.AuxExchangeHalfAck, Aux_ExchangeHalf_Ack);
		RegisterCallback(Message.AuxResourceQuickBuyAck, Aux_ResourceQuickBuy_Ack);
		RegisterCallback(Message.AuxResourceConvertAck, Aux_ResourceConvert_Ack);
		RegisterCallback(Message.LegionRecommendAck, Legion_Recommend_Ack);
		RegisterCallback(Message.LegionSearchAck, Legion_Search_Ack);
		RegisterCallback(Message.LegionTipsInfoAck, Legion_TipsInfo_Ack);
		RegisterCallback(Message.LegionCreateAck, Legion_Create_Ack);
		RegisterCallback(Message.LegionApplyAck, Legion_Apply_Ack);
		RegisterCallback(Message.LegionJoinQuickAck, Legion_JoinQuick_Ack);
		RegisterCallback(Message.LegionClearJoinCdAck, Legion_ClearJoinCD_Ack);
		RegisterCallback(Message.LegionQuitAck, Legion_Quit_Ack);
		RegisterCallback(Message.LegionDataAck, Legion_Data_Ack);
		RegisterCallback(Message.LegionMembersAck, Legion_Members_Ack);
		RegisterCallback(Message.LegionApplyListAck, Legion_ApplyList_Ack);
		RegisterCallback(Message.LegionApplyPassAck, Legion_ApplyPass_Ack);
		RegisterCallback(Message.LegionApplyPassAllAck, Legion_ApplyPassAll_Ack);
		RegisterCallback(Message.LegionApplyRefuseAck, Legion_ApplyRefuse_Ack);
		RegisterCallback(Message.LegionApplyRefuseAllAck, Legion_ApplyRefuseAll_Ack);
		RegisterCallback(Message.LegionKickAck, Legion_Kick_Ack);
		RegisterCallback(Message.LegionNameChangeAck, Legion_NameChange_Ack);
		RegisterCallback(Message.LegionIconChangeAck, Legion_IconChange_Ack);
		RegisterCallback(Message.LegionSignChangeAck, Legion_SignChange_Ack);
		RegisterCallback(Message.LegionJoinSetAck, Legion_JoinSet_Ack);
		RegisterCallback(Message.LegionOfficialSetAck, Legion_OfficialSet_Ack);
		RegisterCallback(Message.LegionLogListAck, Legion_LogList_Ack);
		RegisterCallback(Message.LegionAuxRewardAck, LegionAuxReward_Ack);
		RegisterCallback(Message.LegionAuxDonateAck, LegionAux_Donate_Ack);
		RegisterCallback(Message.LegionAuxDonateTargetAck, LegionAux_DonateTarget_Ack);
		RegisterCallback(Message.LegionAuxDinnerAck, LegionAux_Dinner_Ack);
		RegisterCallback(Message.LegionBossFightStartAck, LegionBoss_FightStart_Ack);
		RegisterCallback(Message.LegionBossFightResultAck, LegionBoss_FightResult_Ack);
		RegisterCallback(Message.LegionBossSaoDangAck, LegionBoss_SaoDang_Ack);
		RegisterCallback(Message.LegionBossPassAwardAck, LegionBoss_PassAward_Ack);
		RegisterCallback(Message.LegionBossBuyCombatAck, LegionBoss_BuyCombat_Ack);
		RegisterCallback(Message.LegionBossRankListAck, LegionBoss_RankList_Ack);
		RegisterCallback(Message.LegionBossRecordListAck, LegionBoss_RecordList_Ack);
		RegisterCallback(Message.LegionWarDataAck, LegionWar_Data_Ack);
		RegisterCallback(Message.LegionWarSignUpAck, LegionWar_SignUp_Ack);
		RegisterCallback(Message.LegionWarSignUpListAck, LegionWar_SignUpList_Ack);
		RegisterCallback(Message.LegionWarMapInfoAck, LegionWar_MapInfo_Ack);
		RegisterCallback(Message.LegionWarMapInfoNtf, LegionWar_MapInfo_Ntf);
		RegisterCallback(Message.LegionWarMapMoveNtf, LegionWar_MapMove_Ntf);
		RegisterCallback(Message.LegionWarPointInfoAck, LegionWar_PointInfo_Ack);
		RegisterCallback(Message.LegionWarEnterPointAck, LegionWar_EnterPoint_Ack);
		RegisterCallback(Message.LegionWarRestAck, LegionWar_Rest_Ack);
		RegisterCallback(Message.LegionWarGotoAck, LegionWar_Goto_Ack);
		RegisterCallback(Message.LegionWarPointAwardAck, LegionWar_PointAward_Ack);
		RegisterCallback(Message.LegionWarFightStartAck, LegionWar_FightStart_Ack);
		RegisterCallback(Message.LegionWarFightResultAck, LegionWar_FightResult_Ack);
		RegisterCallback(Message.LegionWarFightCancelAck, LegionWar_FightCancel_Ack);
		RegisterCallback(Message.LegionWarLegionRankListAck, LegionWar_LegionRankList_Ack);
		RegisterCallback(Message.LegionWarPlayerRankListAck, LegionWar_PlayerRankList_Ack);
		RegisterCallback(Message.LegionWarSettleLegionInfoAck, LegionWar_SettleLegionInfo_Ack);
		RegisterCallback(Message.LegionWarSettlePlayerInfoAck, LegionWar_SettlePlayerInfo_Ack);
		RegisterCallback(Message.LegionWarEnterWarAck, LegionWar_EnterWar_Ack);
		RegisterCallback(Message.LegionWarMarkPointAck, LegionWar_MarkPoint_Ack);
		RegisterCallback(Message.LegionWarModSelectAck, LegionWar_ModSelect_Ack);
		RegisterCallback(Message.LegionWarZhanLingLevelAck, LegionWar_ZhanLingLevel_Ack);
		RegisterCallback(Message.LegionWarZhanLingAwardAck, LegionWar_ZhanLingAward_Ack);
		RegisterCallback(Message.LegionWarWatchMapInfoAck, LegionWar_WatchMapInfo_Ack);
		RegisterCallback(Message.LegionWarTaskAwardAck, LegionWar_TaskAward_Ack);
		RegisterCallback(Message.LegionWarRecordListAck, LegionWar_RecordList_Ack);
		RegisterCallback(Message.ChatSendAck, Chat_Send_Ack);
		RegisterCallback(Message.ChatDataAck, Chat_Data_Ack);
		RegisterCallback(Message.ChatActionShareAck, Chat_ActionShare_Ack);
		RegisterCallback(Message.ChatActionDataAck, Chat_ActionData_Ack);
		RegisterCallback(Message.ChatScrollNtf, Chat_Scroll_Ntf);
		RegisterCallback(Message.ChatReceiveNtf, Chat_Receive_Ntf);
		RegisterCallback(Message.OperateDataAck, Operate_Data_Ack);
		RegisterCallback(Message.OperateRechargeDailyAck, Operate_RechargeDaily_Ack);
		RegisterCallback(Message.OperateRechargeDailySumAck, Operate_RechargeDailySum_Ack);
		RegisterCallback(Message.OperateRechargeSumAck, Operate_RechargeSum_Ack);
		RegisterCallback(Message.OperateRechargeDirectAck, Operate_RechargeDirect_Ack);
		RegisterCallback(Message.OperateConsumeDailySumAck, Operate_ConsumeDailySum_Ack);
		RegisterCallback(Message.OperateConsumeSumAck, Operate_ConsumeSum_Ack);
		RegisterCallback(Message.OperateLimitTimeHeroDrawAck, Operate_LimitTimeHeroDraw_Ack);
		RegisterCallback(Message.OperateLimitTimeHeroScoreAck, Operate_LimitTimeHeroScore_Ack);
		RegisterCallback(Message.OperateLimitTimeShopAck, Operate_LimitTimeShop_Ack);
		RegisterCallback(Message.OperateLoginSumAck, Operate_LoginSum_Ack);
		RegisterCallback(Message.OperateDailyActiveAck, Operate_DailyActive_Ack);
		RegisterCallback(Message.OperateBarDrawAck, Operate_BarDraw_Ack);
		RegisterCallback(Message.OperatePropExchangeAck, Operate_PropExchange_Ack);
		RegisterCallback(Message.OperatePropExchangeTargetAck, Operate_PropExchangeTarget_Ack);
		RegisterCallback(Message.OperateScratchCardAck, Operate_ScratchCard_Ack);
		RegisterCallback(Message.OperateScratchCardTargetAck, Operate_ScratchCardTarget_Ack);
		RegisterCallback(Message.OperateScratchCardResetAck, Operate_ScratchCardReset_Ack);
		RegisterCallback(Message.OperateJiZiAck, Operate_JiZi_Ack);
		RegisterCallback(Message.OperateCipherBoxAck, Operate_CipherBox_Ack);
		RegisterCallback(Message.OperateBarrageCmdAck, Operate_BarrageCmd_Ack);
		RegisterCallback(Message.OperateLimitTimeCatcherCoinAck, Operate_LimitTimeCatcherCoin_Ack);
		RegisterCallback(Message.OperateLimitTimeCatcherGrabAck, Operate_LimitTimeCatcherGrab_Ack);
		RegisterCallback(Message.OperateLimitTimeCatcherAwardAck, Operate_LimitTimeCatcherAward_Ack);
		RegisterCallback(Message.OperateHolidayTaskDataAck, Operate_HolidayTaskData_Ack);
		RegisterCallback(Message.OperateHolidayTaskTaskAwardAck, Operate_HolidayTaskTaskAward_Ack);
		RegisterCallback(Message.OperateHolidayTaskLevelAwardAck, Operate_HolidayTaskLevelAward_Ack);
		RegisterCallback(Message.OperateHolidayTaskScoreAwardAck, Operate_HolidayTaskScoreAward_Ack);
		RegisterCallback(Message.OperateHolidayTaskBuyLevelAck, Operate_HolidayTaskBuyLevel_Ack);
		RegisterCallback(Message.OperatePrayLanternDrawAck, Operate_PrayLanternDraw_Ack);
		RegisterCallback(Message.OperatePrayLanternTargetAck, Operate_PrayLanternTarget_Ack);
		RegisterCallback(Message.OperatePrayLanternSelectAck, Operate_PrayLanternSelect_Ack);
		RegisterCallback(Message.OperatePrayLanternUltimateAck, Operate_PrayLanternUltimate_Ack);
		RegisterCallback(Message.OperateResourceExchangeAck, Operate_ResourceExchange_Ack);
		RegisterCallback(Message.OperateFusionDataAck, Operate_FusionData_Ack);
		RegisterCallback(Message.OperateFusionUpdateAck, Operate_FusionUpdate_Ack);
		RegisterCallback(Message.OperateFusionDailyAwardAck, Operate_FusionDailyAward_Ack);
		RegisterCallback(Message.OperateFusionTargetAwardAck, Operate_FusionTargetAward_Ack);
		RegisterCallback(Message.OperateFusionFriendsRankAck, Operate_FusionFriendsRank_Ack);
		RegisterCallback(Message.OperateInfiniteGiftAwardAck, Operate_InfiniteGiftAward_Ack);
		RegisterCallback(Message.OperateInfiniteGiftTargetAck, Operate_InfiniteGiftTarget_Ack);
		RegisterCallback(Message.OperateLimitTimeRouletteDrawAck, Operate_LimitTimeRouletteDraw_Ack);
		RegisterCallback(Message.OperateLimitTimeRouletteTargetAck, Operate_LimitTimeRouletteTarget_Ack);
		RegisterCallback(Message.OperateLimitTimeTreasureDrawAck, Operate_LimitTimeTreasureDraw_Ack);
		RegisterCallback(Message.OperateLimitTimeTreasureTargetAck, Operate_LimitTimeTreasureTarget_Ack);
		RegisterCallback(Message.OperateLimitTimeVendorDrawAck, Operate_LimitTimeVendorDraw_Ack);
		RegisterCallback(Message.OperateLimitTimeVendorTargetAck, Operate_LimitTimeVendorTarget_Ack);
		RegisterCallback(Message.OperateInfiniteGiftFreeAck, Operate_InfiniteGiftFree_Ack);
		RegisterCallback(Message.OperateRestaurantActivateAwardAck, Operate_RestaurantActivateAward_Ack);
		RegisterCallback(Message.OperateRestaurantDailyAwardAck, Operate_RestaurantDailyAward_Ack);
		RegisterCallback(Message.OperateRestaurantBuyPieceAck, Operate_RestaurantBuyPiece_Ack);
		RegisterCallback(Message.OperateRestaurantDelPieceAck, Operate_RestaurantDelPiece_Ack);
		RegisterCallback(Message.OperateRestaurantSlotSyncAck, Operate_RestaurantSlotSync_Ack);
		RegisterCallback(Message.OperateRestaurantTargetAck, Operate_RestaurantTarget_Ack);
		RegisterCallback(Message.OperateRestaurantOfferPieceAck, Operate_RestaurantOfferPiece_Ack);
		RegisterCallback(Message.OperateThemeTaskDataAck, Operate_ThemeTaskData_Ack);
		RegisterCallback(Message.OperateThemeTaskTaskAwardAck, Operate_ThemeTaskTaskAward_Ack);
		RegisterCallback(Message.OperateThemeTaskTargetAwardAck, Operate_ThemeTaskTargetAward_Ack);
		RegisterCallback(Message.OperateZhanLingDataAck, Operate_ZhanLingData_Ack);
		RegisterCallback(Message.OperateZhanLingKillAwardAck, Operate_ZhanLingKillAward_Ack);
		RegisterCallback(Message.EndlessMineDataAck, EndlessMine_Data_Ack);
		RegisterCallback(Message.EndlessMineActivateAwardAck, EndlessMine_ActivateAward_Ack);
		RegisterCallback(Message.EndlessMineDigAck, EndlessMine_Dig_Ack);
		RegisterCallback(Message.EndlessMineDrillAck, EndlessMine_Drill_Ack);
		RegisterCallback(Message.EndlessMineBombAck, EndlessMine_Bomb_Ack);
		RegisterCallback(Message.EndlessMinePropBuyAck, EndlessMine_PropBuy_Ack);
		RegisterCallback(Message.EndlessMineExchangeAwardAck, EndlessMine_ExchangeAward_Ack);
		RegisterCallback(Message.EndlessMineTargetAwardAck, EndlessMine_TargetAward_Ack);
		RegisterCallback(Message.OperateSnowVeilActivateAck, Operate_SnowVeilActivate_Ack);
		RegisterCallback(Message.OperateSnowVeilTargetAck, Operate_SnowVeilTarget_Ack);
		RegisterCallback(Message.OperateSnowVeilGoAck, Operate_SnowVeilGo_Ack);
		RegisterCallback(Message.OperateSnowVeilEventItemAck, Operate_SnowVeilEventItem_Ack);
		RegisterCallback(Message.OperateSnowVeilEventTurntableAck, Operate_SnowVeilEventTurntable_Ack);
		RegisterCallback(Message.OperateSnowVeilEventRollingAck, Operate_SnowVeilEventRolling_Ack);
		RegisterCallback(Message.OperateSnowVeilEventScoreAck, Operate_SnowVeilEventScore_Ack);
		RegisterCallback(Message.OperateSnowVeilEventHighScoreAck, Operate_SnowVeilEventHighScore_Ack);
		RegisterCallback(Message.OperateSnowVeilRollingAck, Operate_SnowVeilRolling_Ack);
		RegisterCallback(Message.OperateSnowVeilRollingEndAck, Operate_SnowVeilRollingEnd_Ack);
		RegisterCallback(Message.OperateSnowVeilTurntableAck, Operate_SnowVeilTurntable_Ack);
		RegisterCallback(Message.OperateSnowVeilPropBuyAck, Operate_SnowVeilPropBuy_Ack);
		RegisterCallback(Message.RechargeShopGrowthGiftAck, RechargeShop_GrowthGift_Ack);
		RegisterCallback(Message.RechargeShopVipCardAwardAck, RechargeShop_VipCardAward_Ack);
		RegisterCallback(Message.RechargeActiveDataAck, RechargeActive_Data_Ack);
		RegisterCallback(Message.RechargeActiveNovicePackAck, RechargeActive_NovicePack_Ack);
		RegisterCallback(Message.RechargeActiveNovicePackExtraAck, RechargeActive_NovicePackExtra_Ack);
		RegisterCallback(Message.ActivityDataAck, Activity_Data_Ack);
		RegisterCallback(Message.ActivitySignAck, Activity_Sign_Ack);
		RegisterCallback(Message.ActivitySignTargetAwardAck, Activity_SignTargetAward_Ack);
		RegisterCallback(Message.ActivityTiliBankAwardAck, Activity_TiliBankAward_Ack);
		RegisterCallback(Message.ActivityTiliBankAwardAllAck, Activity_TiliBankAwardAll_Ack);
		RegisterCallback(Message.ActivityBubbleGiftRandAck, Activity_BubbleGiftRand_Ack);
		RegisterCallback(Message.ActivityBubbleGiftBuyAck, Activity_BubbleGiftBuy_Ack);
		RegisterCallback(Message.PeerageDataAck, Peerage_Data_Ack);
		RegisterCallback(Message.PeerageUpgradeAck, Peerage_Upgrade_Ack);
		RegisterCallback(Message.PeerageUpgradeTaskAck, Peerage_UpgradeTask_Ack);
		RegisterCallback(Message.DailyTaskDataAck, DailyTask_Data_Ack);
		RegisterCallback(Message.DailyTaskAwardAck, DailyTask_Award_Ack);
		RegisterCallback(Message.DailyTaskTargetAwardAck, DailyTask_TargetAward_Ack);
		RegisterCallback(Message.PassportBuyLevelAck, Passport_BuyLevel_Ack);
		RegisterCallback(Message.PassportLevelAwardAck, Passport_LevelAward_Ack);
		RegisterCallback(Message.RechargeActiveZhanLingMainAwardAck, RechargeActive_ZhanLingMainAward_Ack);
		RegisterCallback(Message.HandbookDataAck, Handbook_Data_Ack);
		RegisterCallback(Message.HandbookMonsterActiveAck, Handbook_MonsterActive_Ack);
		RegisterCallback(Message.HandbookMonsterUpgradeAck, Handbook_MonsterUpgrade_Ack);
		RegisterCallback(Message.HandbookModActiveAwardAck, Handbook_ModActiveAward_Ack);
		RegisterCallback(Message.HandbookModTypeTargetAwardAck, Handbook_ModTypeTargetAward_Ack);
		RegisterCallback(Message.HandbookModTargetAwardAck, Handbook_ModTargetAward_Ack);
		RegisterCallback(Message.RoleDataAck, Role_Data_Ack);
		RegisterCallback(Message.RoleTipsInfoAck, Role_TipsInfo_Ack);
		RegisterCallback(Message.RoleMiniTipsInfoAck, Role_MiniTipsInfo_Ack);
		RegisterCallback(Message.RoleTipsDetailAck, Role_TipsDetail_Ack);
		RegisterCallback(Message.RolePvpTipsInfoAck, Role_PvpTipsInfo_Ack);
		RegisterCallback(Message.RoleFightStartAck, Role_FightStart_Ack);
		RegisterCallback(Message.RoleFightResultAck, Role_FightResult_Ack);
		RegisterCallback(Message.WorldReconExploreAck, World_ReconExplore_Ack);
		RegisterCallback(Message.WorldReconExploredAck, World_ReconExplored_Ack);
		RegisterCallback(Message.WorldReconExploreCancelAck, World_ReconExploreCancel_Ack);
		RegisterCallback(Message.WorldResourceActivateAck, WorldResource_Activate_Ack);
		RegisterCallback(Message.WorldResourceIdleAwardAck, WorldResource_IdleAward_Ack);
		RegisterCallback(Message.AuxFreeGiftAwardAck, Aux_FreeGiftAward_Ack);
		RegisterCallback(Message.RankTopAck, Rank_Top_Ack);
		RegisterCallback(Message.RankListAck, Rank_List_Ack);
		RegisterCallback(Message.RankFirstReachAck, Rank_FirstReach_Ack);
		RegisterCallback(Message.RankFirstReachAwardAck, Rank_FirstReachAward_Ack);
		RegisterCallback(Message.OpenServerRankTopAck, OpenServer_RankTop_Ack);
		RegisterCallback(Message.OpenServerRankListAck, OpenServer_RankList_Ack);
		RegisterCallback(Message.SuitActiveAck, Suit_Active_Ack);
		RegisterCallback(Message.SuitSetAck, Suit_Set_Ack);
		RegisterCallback(Message.SuitRemoveAck, Suit_Remove_Ack);
		RegisterCallback(Message.SuitUpgradeStarAck, Suit_UpgradeStar_Ack);
		RegisterCallback(Message.SuitSkillSetAck, Suit_SkillSet_Ack);
		RegisterCallback(Message.SuitSkillRemoveAck, Suit_SkillRemove_Ack);
		RegisterCallback(Message.SuitSuperSkillSetAck, Suit_SuperSkillSet_Ack);
		RegisterCallback(Message.SuitSuperSkillRemoveAck, Suit_SuperSkillRemove_Ack);
		RegisterCallback(Message.SuitConvertAck, Suit_Convert_Ack);
		RegisterCallback(Message.GunActiveAck, Gun_Active_Ack);
		RegisterCallback(Message.GunSetAck, Gun_Set_Ack);
		RegisterCallback(Message.GunRemoveAck, Gun_Remove_Ack);
		RegisterCallback(Message.GunUpgradeStarAck, Gun_UpgradeStar_Ack);
		RegisterCallback(Message.GunConvertAck, Gun_Convert_Ack);
		RegisterCallback(Message.GunSetBulletAck, Gun_SetBullet_Ack);
		RegisterCallback(Message.AuxPlayerSuggestAck, Aux_PlayerSuggest_Ack);
		RegisterCallback(Message.AuxProgressTrackAck, Aux_ProgressTrack_Ack);
		RegisterCallback(Message.AuxReminderListAck, Aux_ReminderList_Ack);
		RegisterCallback(Message.EndlessHordeDataAck, EndlessHorde_Data_Ack);
		RegisterCallback(Message.EndlessHordeFightStartAck, EndlessHorde_FightStart_Ack);
		RegisterCallback(Message.EndlessHordeFightForwardAck, EndlessHorde_FightForward_Ack);
		RegisterCallback(Message.EndlessHordeFightResultAck, EndlessHorde_FightResult_Ack);
		RegisterCallback(Message.EndlessHordeBuyCombatAck, EndlessHorde_BuyCombat_Ack);
		RegisterCallback(Message.EndlessHordeSaoDangAck, EndlessHorde_SaoDang_Ack);
		RegisterCallback(Message.RechargeActiveZhanLingEndlessAwardAck, RechargeActive_ZhanLingEndlessAward_Ack);
		RegisterCallback(Message.MechaDataAck, Mecha_Data_Ack);
		RegisterCallback(Message.MechaActiveAck, Mecha_Active_Ack);
		RegisterCallback(Message.MechaSetAck, Mecha_Set_Ack);
		RegisterCallback(Message.MechaUpgradeLevelAck, Mecha_UpgradeLevel_Ack);
		RegisterCallback(Message.MechaUpgradeStarAck, Mecha_UpgradeStar_Ack);
		RegisterCallback(Message.MechaUpgradeSkillAck, Mecha_UpgradeSkill_Ack);
		RegisterCallback(Message.MechaConvertAck, Mecha_Convert_Ack);
		RegisterCallback(Message.MechaResetCalcAck, Mecha_ResetCalc_Ack);
		RegisterCallback(Message.MechaResetAck, Mecha_Reset_Ack);
		RegisterCallback(Message.MechaFastUpgradeLevelCalcAck, Mecha_FastUpgradeLevelCalc_Ack);
		RegisterCallback(Message.MechaFastUpgradeLevelAck, Mecha_FastUpgradeLevel_Ack);
		RegisterCallback(Message.JourneyDataAck, Journey_Data_Ack);
		RegisterCallback(Message.JourneyChapterOneBarDrawAck, Journey_ChapterOneBarDraw_Ack);
		RegisterCallback(Message.JourneyChapterOneBarTargetAck, Journey_ChapterOneBarTarget_Ack);
		RegisterCallback(Message.JourneyChapterOneSignAck, Journey_ChapterOneSign_Ack);
		RegisterCallback(Message.JourneyChapterOneRechargeSumAck, Journey_ChapterOneRechargeSum_Ack);
		RegisterCallback(Message.JourneyChapterOneExchangeAck, Journey_ChapterOneExchange_Ack);
		RegisterCallback(Message.JourneyChapterOneZhanLingAwardAck, Journey_ChapterOneZhanLingAward_Ack);
		RegisterCallback(Message.JourneyChapterTwoBarDrawAck, Journey_ChapterTwoBarDraw_Ack);
		RegisterCallback(Message.JourneyChapterTwoBarTargetAck, Journey_ChapterTwoBarTarget_Ack);
		RegisterCallback(Message.JourneyChapterTwoActiveAck, Journey_ChapterTwoActive_Ack);
		RegisterCallback(Message.JourneyChapterTwoDiamondCostAck, Journey_ChapterTwoDiamondCost_Ack);
		RegisterCallback(Message.JourneyChapterTwoExchangeAck, Journey_ChapterTwoExchange_Ack);
		RegisterCallback(Message.JourneyChapterTwoZhanLingAwardAck, Journey_ChapterTwoZhanLingAward_Ack);
		RegisterCallback(Message.JourneyChapterThreeBarDrawAck, Journey_ChapterThreeBarDraw_Ack);
		RegisterCallback(Message.JourneyChapterThreeBarTargetAck, Journey_ChapterThreeBarTarget_Ack);
		RegisterCallback(Message.JourneyChapterThreeSignAck, Journey_ChapterThreeSign_Ack);
		RegisterCallback(Message.JourneyChapterThreeExchangeAck, Journey_ChapterThreeExchange_Ack);
		RegisterCallback(Message.JourneyChapterThreeDrawCountAck, Journey_ChapterThreeDrawCount_Ack);
		RegisterCallback(Message.JourneyChapterThreeZhanLingAwardAck, Journey_ChapterThreeZhanLingAward_Ack);
		RegisterCallback(Message.WallDataAck, Wall_Data_Ack);
		RegisterCallback(Message.WallActiveAck, Wall_Active_Ack);
		RegisterCallback(Message.WallSetAck, Wall_Set_Ack);
		RegisterCallback(Message.WallUpgradeLevelAck, Wall_UpgradeLevel_Ack);
		RegisterCallback(Message.WallUpgradeStarAck, Wall_UpgradeStar_Ack);
		RegisterCallback(Message.WallConvertAck, Wall_Convert_Ack);
		RegisterCallback(Message.WallSkillSetAck, Wall_SkillSet_Ack);
		RegisterCallback(Message.WallSkillRemoveAck, Wall_SkillRemove_Ack);
		RegisterCallback(Message.LegendaryDataAck, Legendary_Data_Ack);
		RegisterCallback(Message.LegendaryFightStartAck, Legendary_FightStart_Ack);
		RegisterCallback(Message.LegendaryFightResultAck, Legendary_FightResult_Ack);
		RegisterCallback(Message.RechargeActiveZhanLingLegendaryAck, RechargeActive_ZhanLingLegendary_Ack);
		RegisterCallback(Message.FootholdDataAck, Foothold_Data_Ack);
		RegisterCallback(Message.FootholdActiveAck, Foothold_Active_Ack);
		RegisterCallback(Message.FootholdSetAck, Foothold_Set_Ack);
		RegisterCallback(Message.FootholdUpgradeStarAck, Foothold_UpgradeStar_Ack);
		RegisterCallback(Message.FootholdConvertAck, Foothold_Convert_Ack);
		RegisterCallback(Message.ArenaDataAck, Arena_Data_Ack);
		RegisterCallback(Message.ArenaCombatListAck, Arena_CombatList_Ack);
		RegisterCallback(Message.ArenaFightStartAck, Arena_FightStart_Ack);
		RegisterCallback(Message.ArenaFightResultAck, Arena_FightResult_Ack);
		RegisterCallback(Message.ArenaBuyCombatAck, Arena_BuyCombat_Ack);
		RegisterCallback(Message.ArenaSaoDangAck, Arena_SaoDang_Ack);
		RegisterCallback(Message.ArenaRecordListAck, Arena_RecordList_Ack);
		RegisterCallback(Message.ArenaBattleResultAck, Arena_BattleResult_Ack);
		RegisterCallback(Message.ArenaRankListAck, Arena_RankList_Ack);
		RegisterCallback(Message.ArenaGiftBuyAck, Arena_GiftBuy_Ack);
		RegisterCallback(Message.CrossLadderDataAck, CrossLadder_Data_Ack);
		RegisterCallback(Message.CrossLadderSeasonInitAck, CrossLadder_SeasonInit_Ack);
		RegisterCallback(Message.CrossLadderCombatListAck, CrossLadder_CombatList_Ack);
		RegisterCallback(Message.CrossLadderFightStartAck, CrossLadder_FightStart_Ack);
		RegisterCallback(Message.CrossLadderFightResultAck, CrossLadder_FightResult_Ack);
		RegisterCallback(Message.CrossLadderBuyCombatAck, CrossLadder_BuyCombat_Ack);
		RegisterCallback(Message.CrossLadderUpgradeFightStartAck, CrossLadder_UpgradeFightStart_Ack);
		RegisterCallback(Message.CrossLadderUpgradeFightResultAck, CrossLadder_UpgradeFightResult_Ack);
		RegisterCallback(Message.CrossLadderUpgradeConfirmAck, CrossLadder_UpgradeConfirm_Ack);
		RegisterCallback(Message.CrossLadderTargetAwardAck, CrossLadder_TargetAward_Ack);
		RegisterCallback(Message.CrossLadderRecordListAck, CrossLadder_RecordList_Ack);
		RegisterCallback(Message.CrossLadderFriendListAck, CrossLadder_FriendList_Ack);
		RegisterCallback(Message.TreasureDataAck, Treasure_Data_Ack);
		RegisterCallback(Message.TreasureActiveAck, Treasure_Active_Ack);
		RegisterCallback(Message.TreasureUpgradeLevelAck, Treasure_UpgradeLevel_Ack);
		RegisterCallback(Message.TreasureActiveAllAck, Treasure_ActiveAll_Ack);
		RegisterCallback(Message.TreasureUpgradeLevelAllAck, Treasure_UpgradeLevelAll_Ack);
		RegisterCallback(Message.TreasureConvertAck, Treasure_Convert_Ack);
		RegisterCallback(Message.RechargeActiveTreasurePrivilegeAck, RechargeActive_TreasurePrivilege_Ack);
		RegisterCallback(Message.MineWarDataAck, MineWar_Data_Ack);
		RegisterCallback(Message.MineWarEmptyPointAck, MineWar_EmptyPoint_Ack);
		RegisterCallback(Message.MineWarPointDataAck, MineWar_PointData_Ack);
		RegisterCallback(Message.MineWarFightStartAck, MineWar_FightStart_Ack);
		RegisterCallback(Message.MineWarFightResultAck, MineWar_FightResult_Ack);
		RegisterCallback(Message.MineWarOccupyPointAck, MineWar_OccupyPoint_Ack);
		RegisterCallback(Message.MineWarBuyCombatAck, MineWar_BuyCombat_Ack);
		RegisterCallback(Message.MineWarIdleAwardAck, MineWar_IdleAward_Ack);
		RegisterCallback(Message.MineWarRecordListAck, MineWar_RecordList_Ack);
		RegisterCallback(Message.MineWarRecordSettleAck, MineWar_RecordSettle_Ack);
		RegisterCallback(Message.MineWarSaoDangAck, MineWar_SaoDang_Ack);
		RegisterCallback(Message.CrossMineServerListAck, CrossMine_ServerList_Ack);
		RegisterCallback(Message.CrossMineDataAck, CrossMine_Data_Ack);
		RegisterCallback(Message.CrossMinePointDataAck, CrossMine_PointData_Ack);
		RegisterCallback(Message.CrossMineDestroyEmptyPointAck, CrossMine_DestroyEmptyPoint_Ack);
		RegisterCallback(Message.CrossMineFightStartAck, CrossMine_FightStart_Ack);
		RegisterCallback(Message.CrossMineFightResultAck, CrossMine_FightResult_Ack);
		RegisterCallback(Message.CrossMineRecordListAck, CrossMine_RecordList_Ack);
		RegisterCallback(Message.ServantDataAck, Servant_Data_Ack);
		RegisterCallback(Message.ServantUpgradeLevelAck, Servant_UpgradeLevel_Ack);
		RegisterCallback(Message.ServantUpgradeQualityAck, Servant_UpgradeQuality_Ack);
		RegisterCallback(Message.ServantUpgradeSkillAck, Servant_UpgradeSkill_Ack);
		RegisterCallback(Message.ServantUpgradeSubSkillAck, Servant_UpgradeSubSkill_Ack);
		RegisterCallback(Message.ServantUpgradeAttrAck, Servant_UpgradeAttr_Ack);
		RegisterCallback(Message.ServantSubSkillFlushAck, Servant_SubSkillFlush_Ack);
		RegisterCallback(Message.ServantLockAck, Servant_Lock_Ack);
		RegisterCallback(Message.ServantDeployAck, Servant_Deploy_Ack);
		RegisterCallback(Message.ServantDecomposeCalcAck, Servant_DecomposeCalc_Ack);
		RegisterCallback(Message.ServantDecomposeAck, Servant_Decompose_Ack);
		RegisterCallback(Message.IncubateDataAck, Incubate_Data_Ack);
		RegisterCallback(Message.IncubateSlotUnlockAck, Incubate_SlotUnlock_Ack);
		RegisterCallback(Message.IncubateStartAck, Incubate_Start_Ack);
		RegisterCallback(Message.IncubateSpeedAck, Incubate_Speed_Ack);
		RegisterCallback(Message.IncubateCompleteAck, Incubate_Complete_Ack);
		RegisterCallback(Message.CrossPeakDataAck, CrossPeak_Data_Ack);
		RegisterCallback(Message.CrossPeakFightStartAck, CrossPeak_FightStart_Ack);
		RegisterCallback(Message.CrossPeakFightResultAck, CrossPeak_FightResult_Ack);
		RegisterCallback(Message.CrossPeakRecordListAck, CrossPeak_RecordList_Ack);
		RegisterCallback(Message.CrossPeakTopAck, CrossPeak_Top_Ack);
		RegisterCallback(Message.CrossPeakSaoDangAck, CrossPeak_SaoDang_Ack);
		RegisterCallback(Message.CrossPeakBuyCombatAck, CrossPeak_BuyCombat_Ack);
		RegisterCallback(Message.TruckDataAck, Truck_Data_Ack);
		RegisterCallback(Message.TruckSlotUnlockAck, Truck_SlotUnlock_Ack);
		RegisterCallback(Message.TruckTransportOpenAck, Truck_TransportOpen_Ack);
		RegisterCallback(Message.TruckTransportRefreshAck, Truck_TransportRefresh_Ack);
		RegisterCallback(Message.TruckTransportStartAck, Truck_TransportStart_Ack);
		RegisterCallback(Message.TruckTransportViewAck, Truck_TransportView_Ack);
		RegisterCallback(Message.TruckTransportCompleteAck, Truck_TransportComplete_Ack);
		RegisterCallback(Message.TruckTransportRecordListAck, Truck_TransportRecordList_Ack);
		RegisterCallback(Message.TruckMatchListAck, Truck_MatchList_Ack);
		RegisterCallback(Message.TruckRobFightStartAck, Truck_RobFightStart_Ack);
		RegisterCallback(Message.TruckRobFightResultAck, Truck_RobFightResult_Ack);
		RegisterCallback(Message.TruckRobRecordListAck, Truck_RobRecordList_Ack);
		RegisterCallback(Message.TruckBuyTransportAck, Truck_BuyTransport_Ack);
		RegisterCallback(Message.TruckBuyRobAck, Truck_BuyRob_Ack);
		RegisterCallback(Message.TruckAddWantedAck, Truck_AddWanted_Ack);
		RegisterCallback(Message.TrainDataAck, Train_Data_Ack);
		RegisterCallback(Message.TrainTransportOpenAck, Train_TransportOpen_Ack);
		RegisterCallback(Message.TrainTransportTakeRefreshAck, Train_TransportTakeRefresh_Ack);
		RegisterCallback(Message.TrainTransportTakeConfirmAck, Train_TransportTakeConfirm_Ack);
		RegisterCallback(Message.TrainRobMatchAck, Train_RobMatch_Ack);
		RegisterCallback(Message.TrainRobFightStartAck, Train_RobFightStart_Ack);
		RegisterCallback(Message.TrainRobFightResultAck, Train_RobFightResult_Ack);
		RegisterCallback(Message.TrainTransportTakeAck, Train_TransportTake_Ack);
		RegisterCallback(Message.TrainTransportAwardAck, Train_TransportAward_Ack);
		RegisterCallback(Message.FacilityDataAck, Facility_Data_Ack);
		RegisterCallback(Message.FacilityRecommendAck, Facility_Recommend_Ack);
		RegisterCallback(Message.FacilityCreateAck, Facility_Create_Ack);
		RegisterCallback(Message.FacilityCloseAck, Facility_Close_Ack);
		RegisterCallback(Message.FacilityViewAck, Facility_View_Ack);
		RegisterCallback(Message.FacilityRoomPreviewAck, Facility_RoomPreview_Ack);
		RegisterCallback(Message.FacilityEventCommonAck, Facility_EventCommon_Ack);
		RegisterCallback(Message.FacilityEventLockedChestAck, Facility_EventLockedChest_Ack);
		RegisterCallback(Message.FacilityEventMonsterAck, Facility_EventMonster_Ack);
		RegisterCallback(Message.FacilityEventShopAck, Facility_EventShop_Ack);
		RegisterCallback(Message.FacilityEventRollingAck, Facility_EventRolling_Ack);
		RegisterCallback(Message.FacilityEventStairAck, Facility_EventStair_Ack);
		RegisterCallback(Message.FacilityStairTeleportAck, Facility_StairTeleport_Ack);
		RegisterCallback(Message.FacilityOpenChestAck, Facility_OpenChest_Ack);
		RegisterCallback(Message.FacilityFightResultAck, Facility_FightResult_Ack);
		RegisterCallback(Message.FacilityRollingAck, Facility_Rolling_Ack);
		RegisterCallback(Message.FacilityRollingEndAck, Facility_RollingEnd_Ack);
		RegisterCallback(Message.FacilityShopBuyAck, Facility_ShopBuy_Ack);
		RegisterCallback(Message.FacilityQuitAck, Facility_Quit_Ack);
		RegisterCallback(Message.OperateBossSubDataAck, Operate_BossSubData_Ack);
		RegisterCallback(Message.OperateBossSubFightStartAck, Operate_BossSubFightStart_Ack);
		RegisterCallback(Message.OperateBossSubFightResultAck, Operate_BossSubFightResult_Ack);
		RegisterCallback(Message.OperateBossSubZhanLingAwardAck, Operate_BossSubZhanLingAward_Ack);
		RegisterCallback(Message.OperateBossSubRouletteAck, Operate_BossSubRoulette_Ack);
		RegisterCallback(Message.OperateBossSubRouletteTargetAck, Operate_BossSubRouletteTarget_Ack);
		RegisterCallback(Message.OperateBossSubTaskAwardAck, Operate_BossSubTaskAward_Ack);
		RegisterCallback(Message.OperateBossSubSaoDangAck, Operate_BossSubSaoDang_Ack);
		RegisterCallback(Message.OperateWallSubjectDataAck, Operate_WallSubjectData_Ack);
		RegisterCallback(Message.OperateWallSubjectFightStartAck, Operate_WallSubjectFightStart_Ack);
		RegisterCallback(Message.OperateWallSubjectFightResultAck, Operate_WallSubjectFightResult_Ack);
		RegisterCallback(Message.OperateWallSubjectChapterAwardAck, Operate_WallSubjectChapterAward_Ack);
		RegisterCallback(Message.OperateWallSubjectBarDrawAck, Operate_WallSubjectBarDraw_Ack);
		RegisterCallback(Message.OperateWallSubjectZhanLingAwardAck, Operate_WallSubjectZhanLingAward_Ack);
		RegisterCallback(Message.OperateWallSubjectRouletteAck, Operate_WallSubjectRoulette_Ack);
		RegisterCallback(Message.OperateWallSubjectRouletteTargetAck, Operate_WallSubjectRouletteTarget_Ack);
		RegisterCallback(Message.OperateMechaSubjectDataAck, Operate_MechaSubjectData_Ack);
		RegisterCallback(Message.OperateMechaSubjectFightStartAck, Operate_MechaSubjectFightStart_Ack);
		RegisterCallback(Message.OperateMechaSubjectFightResultAck, Operate_MechaSubjectFightResult_Ack);
		RegisterCallback(Message.OperateMechaSubjectChapterAwardAck, Operate_MechaSubjectChapterAward_Ack);
		RegisterCallback(Message.OperateMechaSubjectBarDrawAck, Operate_MechaSubjectBarDraw_Ack);
		RegisterCallback(Message.OperateMechaSubjectZhanLingAwardAck, Operate_MechaSubjectZhanLingAward_Ack);
		RegisterCallback(Message.OperateMechaSubjectRouletteAck, Operate_MechaSubjectRoulette_Ack);
		RegisterCallback(Message.OperateMechaSubjectRouletteTargetAck, Operate_MechaSubjectRouletteTarget_Ack);
		RegisterCallback(Message.PushSetDataAck, PushSet_Data_Ack);
		RegisterCallback(Message.PushSetCloseAck, PushSet_Close_Ack);
		RegisterCallback(Message.PvpSetDataAck, PvpSet_Data_Ack);
		RegisterCallback(Message.PvpSetModStartAck, PvpSet_ModStart_Ack);
		RegisterCallback(Message.PvpSetModRefreshAck, PvpSet_ModRefresh_Ack);
		RegisterCallback(Message.PvpSetModSelectAck, PvpSet_ModSelect_Ack);
		RegisterCallback(Message.PvpSetModSubmitAck, PvpSet_ModSubmit_Ack);
		RegisterCallback(Message.PvpSetModCancelAck, PvpSet_ModCancel_Ack);
		RegisterCallback(Message.SeasonEnterAck, Season_Enter_Ack);
		RegisterCallback(Message.SeasonDataAck, Season_Data_Ack);
		RegisterCallback(Message.SeasonMapInfoAck, Season_Map_Info_Ack);
		RegisterCallback(Message.SeasonViewAck, Season_View_Ack);
		RegisterCallback(Message.SeasonMarchStartAck, Season_March_Start_Ack);
		RegisterCallback(Message.SeasonMoveNtf, Season_Move_Ntf);
		RegisterCallback(Message.SeasonMarchArriveNtf, Season_March_Arrive_Ntf);
		RegisterCallback(Message.SeasonMapInfoNtf, Season_Map_Info_Ntf);
		RegisterCallback(Message.SeasonCityDetailAck, Season_City_Detail_Ack);
		RegisterCallback(Message.SeasonResOccupyAck, Season_Res_Occupy_Ack);
		RegisterCallback(Message.SeasonResAbandonAck, Season_Res_Abandon_Ack);
		RegisterCallback(Message.SeasonMyTerritoryAck, Season_My_Territory_Ack);
		RegisterCallback(Message.SeasonAdjacentAck, Season_Adjacent_Ack);
		RegisterCallback(Message.SeasonResProduceNtf, Season_Res_Produce_Ntf);
		RegisterCallback(Message.SeasonAoiNtf, Season_Aoi_Ntf);
		RegisterCallback(Message.SeasonDeclareAck, Season_Declare_Ack);
		RegisterCallback(Message.SeasonDeclareListAck, Season_Declare_List_Ack);
		RegisterCallback(Message.SeasonCityWarStateNtf, Season_CityWar_State_Ntf);
		RegisterCallback(Message.SeasonGarrisonAck, Season_Garrison_Ack);
		RegisterCallback(Message.SeasonRecruitAck, Season_Recruit_Ack);
		RegisterCallback(Message.SeasonGuardInfoAck, Season_Guard_Info_Ack);
		RegisterCallback(Message.SeasonCityAttackAck, Season_City_Attack_Ack);
		RegisterCallback(Message.SeasonCityWarResultNtf, Season_CityWar_Result_Ntf);
		RegisterCallback(Message.SeasonCityOwnerNtf, Season_City_Owner_Ntf);
		RegisterCallback(Message.SeasonOnlineRewardAck, Season_Online_Reward_Ack);
		RegisterCallback(Message.SeasonCityWarInfoAck, Season_CityWar_Info_Ack);
		RegisterCallback(Message.SeasonMeritRankAck, Season_Merit_Rank_Ack);
		RegisterCallback(Message.SeasonBattleResultAck, Season_Battle_Result_Ack);
		RegisterCallback(Message.SeasonRepairAck, Season_Repair_Ack);
		RegisterCallback(Message.SeasonAirdropListAck, Season_Airdrop_List_Ack);
		RegisterCallback(Message.SeasonAirdropRefreshNtf, Season_Airdrop_Refresh_Ntf);
		RegisterCallback(Message.SeasonAirdropDecryptAck, Season_Airdrop_Decrypt_Ack);
		RegisterCallback(Message.SeasonAirdropProgressNtf, Season_Airdrop_Progress_Ntf);
		RegisterCallback(Message.SeasonAirdropSettleNtf, Season_Airdrop_Settle_Ntf);
		RegisterCallback(Message.SeasonAirdropEnterNtf, Season_Airdrop_Enter_Ntf);
		RegisterCallback(Message.SeasonForceInfoAck, Season_Force_Info_Ack);
		RegisterCallback(Message.SeasonForceUpdateAck, Season_Force_Update_Ack);
		RegisterCallback(Message.SeasonForceMembersAck, Season_Force_Members_Ack);
		RegisterCallback(Message.SeasonOfficeAppointAck, Season_Office_Appoint_Ack);
		RegisterCallback(Message.SeasonLeaderElectInfoAck, Season_Leader_Elect_Info_Ack);
		RegisterCallback(Message.SeasonLeaderImpeachAck, Season_Leader_Impeach_Ack);
		RegisterCallback(Message.SeasonLeaderChangeNtf, Season_Leader_Change_Ntf);
		RegisterCallback(Message.SeasonTechUpgradeAck, Season_Tech_Upgrade_Ack);
		RegisterCallback(Message.SeasonShopListAck, Season_Shop_List_Ack);
		RegisterCallback(Message.SeasonShopBuyAck, Season_Shop_Buy_Ack);
		RegisterCallback(Message.SeasonResAttackAck, Season_Res_Attack_Ack);
		RegisterCallback(Message.SeasonForceExpNtf, Season_Force_Exp_Ntf);
		RegisterCallback(Message.SeasonTeamPosAck, Season_Team_Pos_Ack);
	}

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnEnable()
	{
		m_SequenceId = 1;
		InitMsgHandle();
		zbWsHeartStateMachine.Instance.Init();
	}

	public int GetSequenceId()
	{
		m_SequenceId++;
		return m_SequenceId;
	}

	public void RegisterCallback(Message msg, Action<ByteString> callback)
	{
		if (!m_messageCallbacks.ContainsKey(msg))
		{
			m_messageCallbacks.Add(msg, callback);
		}
	}

	public void UnregisterCallback(Message msg)
	{
		if (m_messageCallbacks.ContainsKey(msg))
		{
			m_messageCallbacks.Remove(msg);
		}
	}

	public void WsPackMsg(IMessage req, Message messageId, bool needTip = true)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (m_webSocket != null)
		{
			Packet packet = new Packet();
			packet.MsgId = (int)messageId;
			packet.Seq = GetSequenceId();
			packet.Ver = 20260921;
			packet.Tm = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			if (req == null)
			{
				packet.Data = ByteString.Empty;
			}
			else
			{
				packet.Data = MessageExtensions.ToByteString(req);
			}
			packet.Sign = zbLoginUtil.PacketSign(packet, zbLoginUtil.appSecret);
			byte[] array = MessageExtensions.ToByteArray((IMessage)(object)packet);
			m_webSocket.SendAsBinary(new BufferSegment(array, 0, array.Length));
			if (needTip)
			{
				ShowTipConnect();
			}
			zbProgressEventHelper.Instance.SetProgressEvent1((int)messageId);
		}
	}

	public void Connect()
	{
		Free();
		if (m_needCreateAndConnectSocketTime == -1f)
		{
			m_needCreateAndConnectSocket = false;
			m_needCreateAndConnectSocketTime = 0f;
			CreateAndOpenWebSocket();
		}
		else
		{
			m_needCreateAndConnectSocket = true;
			m_needCreateAndConnectSocketTime = Time.realtimeSinceStartup;
		}
	}

	private void CreateAndOpenWebSocket()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		if (!string.IsNullOrEmpty(m_wsUrl))
		{
			m_webSocket = new WebSocket(new Uri(m_wsUrl));
			WebSocket webSocket = m_webSocket;
			webSocket.OnOpen = (OnWebSocketOpenDelegate)Delegate.Combine((Delegate?)(object)webSocket.OnOpen, (Delegate?)new OnWebSocketOpenDelegate(OnWebSocketOpen));
			WebSocket webSocket2 = m_webSocket;
			webSocket2.OnMessage = (OnWebSocketMessageDelegate)Delegate.Combine((Delegate?)(object)webSocket2.OnMessage, (Delegate?)new OnWebSocketMessageDelegate(OnMessageReceived));
			WebSocket webSocket3 = m_webSocket;
			webSocket3.OnClosed = (OnWebSocketClosedDelegate)Delegate.Combine((Delegate?)(object)webSocket3.OnClosed, (Delegate?)new OnWebSocketClosedDelegate(OnWebSocketClosed));
			m_webSocket.Open();
		}
	}

	public void Free()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		if (m_webSocket != null)
		{
			WebSocket webSocket = m_webSocket;
			webSocket.OnOpen = (OnWebSocketOpenDelegate)Delegate.Remove((Delegate?)(object)webSocket.OnOpen, (Delegate?)new OnWebSocketOpenDelegate(OnWebSocketOpen));
			WebSocket webSocket2 = m_webSocket;
			webSocket2.OnMessage = (OnWebSocketMessageDelegate)Delegate.Remove((Delegate?)(object)webSocket2.OnMessage, (Delegate?)new OnWebSocketMessageDelegate(OnMessageReceived));
			WebSocket webSocket3 = m_webSocket;
			webSocket3.OnClosed = (OnWebSocketClosedDelegate)Delegate.Remove((Delegate?)(object)webSocket3.OnClosed, (Delegate?)new OnWebSocketClosedDelegate(OnWebSocketClosed));
			m_webSocket.Close();
			m_webSocket = null;
		}
	}

	private void OnWebSocketOpen(WebSocket webSocket)
	{
		HideTipConnect();
		zbWsHeartStateMachine.Instance.GetState()?.OnNotifySocketOpen();
		Instance.Game_Login_Req();
	}

	private void OnMessageReceived(WebSocket webSocket, string message)
	{
		HideTipConnect();
		Packet packet = JsonParser.Default.Parse<Packet>(message);
		if (packet != null && m_messageCallbacks.TryGetValue((Message)packet.MsgId, out var value))
		{
			switch ((Message)packet.MsgId)
			{
			case Message.LegionBossBuyCombatAck:
			case Message.ArenaBuyCombatAck:
			case Message.EliteQuestBuyCombatAck:
			case Message.ElementTrialBuyCombatAck:
			case Message.EndlessHordeBuyCombatAck:
			case Message.WorldLairBuyFightNumAck:
			case Message.WorldBossBuyCombatAck:
			case Message.CrossLadderBuyCombatAck:
			case Message.MineWarBuyCombatAck:
			case Message.TruckBuyTransportAck:
			case Message.TruckBuyRobAck:
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10978"));
				break;
			}
			value?.Invoke(packet.Data);
		}
	}

	private void OnWebSocketClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)code == 1006 && zbWsHeartStateMachine.Instance.GetStateId() == EHeartState.Heart)
		{
			zbWsHeartStateMachine.Instance.SetState(EHeartState.Retry);
		}
	}

	private void Update()
	{
		if (m_needCreateAndConnectSocket && Time.realtimeSinceStartup - m_needCreateAndConnectSocketTime > 1f)
		{
			m_needCreateAndConnectSocket = false;
			CreateAndOpenWebSocket();
		}
		else if (zbGameStateMachine.Instance.GetStateId() == EGameState.Main)
		{
			zbWsHeartStateMachine.Instance.Update();
		}
	}

	private void OnDestroy()
	{
		if (m_webSocket != null)
		{
			m_webSocket.Close();
			m_webSocket = null;
		}
	}

	public void ShowTipConnect(bool bNeedTimeOutClose = true, float effectDelay = 1.5f)
	{
		zbUIMgr.Instance.ShowTipConnect(bNeedTimeOutClose, effectDelay);
	}

	public void HideTipConnect()
	{
		zbTipConnectWnd zbTipConnectWnd2 = zbUIMgr.Instance.GetWndById(EWndId.TipConnect) as zbTipConnectWnd;
		if ((Object)(object)zbTipConnectWnd2 != (Object)null)
		{
			zbTipConnectWnd2.SetResponsed(responsed: true);
		}
	}
}
