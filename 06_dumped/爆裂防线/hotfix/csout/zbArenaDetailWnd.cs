using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaDetailWnd : zbBaseWnd
{
	private VisualElement m_bgPick;

	private zbMonsterFollowFoothold m_monster;

	private zbSkillSelected skillSelected;

	private Button m_btnDiamond;

	private Label m_lbDiamond;

	private Label m_lbRank;

	private VisualElement m_nodeFoothold;

	private Label m_lbDesc;

	private VisualElement m_nodeCount;

	private Label m_lbCount;

	private Button m_btnAdd;

	private Button m_btnFight;

	private Button m_btnHead;

	public Button m_btnGuide;

	private Button m_btnTouchTest;

	private EBattleFuncType m_battleFuncType;

	private RoleBaseInfo m_roleBaseInfo;

	private ArenaRankInfo m_arenaRankInfo;

	private CrossLadderAchieveInfo m_achieveInfo;

	private int m_peakRank;

	private TruckTransportDetail m_truckInfo;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_monster = zbUtil.FindGameObjectRecursive(((Component)this).transform, "monsterFollowFoothold").GetComponent<zbMonsterFollowFoothold>();
		m_bgPick = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_pick", (string)null);
		skillSelected = new zbSkillSelected();
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "skillSelected", (string)null);
		skillSelected.OnEnable(this, root);
		m_btnDiamond = UQueryExtensions.Q<Button>(m_RootElement, "btn_diamond", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>(m_RootElement, "lb_diamond", (string)null);
		m_btnGuide = UQueryExtensions.Q<Button>(m_RootElement, "btn_guide", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_rank", (string)null);
		m_nodeFoothold = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_foothold", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_nodeCount = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_count", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_RootElement, "btn_head", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgPick, (Action<ClickEvent>)OnMonsterPreview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnTouchTest, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ArenaInfo, delegate
		{
			RefreshArenaInfo();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckSelfInfo, delegate
		{
			RefreshTruckInfo();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_CommFight_Ack, OnCommFightAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TruckMatchRefresh, OnCommFightAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_PvpTipsInfo, delegate
		{
			RefreshPvpTipsInfo();
		});
	}

	public void RefreshCustom(EBattleFuncType battleFuncType, RoleBaseInfo roleBaseInfo, ArenaRankInfo arenaRankInfo = null, CrossLadderAchieveInfo achieveInfo = null, int peakRank = 0, TruckTransportDetail truckInfo = null)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		m_battleFuncType = battleFuncType;
		m_roleBaseInfo = roleBaseInfo;
		m_arenaRankInfo = arenaRankInfo;
		m_achieveInfo = achieveInfo;
		m_peakRank = peakRank;
		m_truckInfo = truckInfo;
		skillSelected.Refresh(m_roleBaseInfo);
		if (m_roleBaseInfo != null)
		{
			zbFootholdCsvRecord recordByKey = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(m_roleBaseInfo.FootholdId);
			if (recordByKey != null)
			{
				string footholdEffectDesc = zbSelf.Instance.guardHandler.GetFootholdEffectDesc(recordByKey);
				((TextElement)m_lbDesc).text = recordByKey.name + footholdEffectDesc;
				m_nodeFoothold.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(footholdEffectDesc != "")));
				zbUtil.AutoFontSize((TextElement)(object)m_lbDesc, 22f, 14f, 660f, 140f);
			}
			else
			{
				m_nodeFoothold.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			((VisualElement)m_btnDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeCount.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_battleFuncType != EBattleFuncType.Arena && m_battleFuncType != EBattleFuncType.Truck) ? 1 : 0));
			((VisualElement)m_lbRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_battleFuncType != EBattleFuncType.PeakArena));
			if (m_battleFuncType == EBattleFuncType.Arena)
			{
				RefreshArenaInfo();
			}
			else if (m_battleFuncType == EBattleFuncType.CrossLadderMatch)
			{
				RefreshAchieveInfo();
			}
			else if (m_battleFuncType == EBattleFuncType.PeakArena)
			{
				RefreshPeakInfo();
			}
			else if (m_battleFuncType == EBattleFuncType.Truck)
			{
				RefreshTruckInfo();
			}
			zbWsMsgHandler.Instance.Role_PvpTipsInfo_Req((int)m_roleBaseInfo.RoleId);
		}
	}

	private void RefreshArenaInfo()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10868") + ": " + (zbServerData.sGameData.ArenaInfo.DailyCount - zbServerData.sGameData.ArenaInfo.CombatCount) + "/" + (zbServerData.sGameData.ArenaInfo.DailyCount - zbServerData.sGameData.ArenaInfo.BuyCount);
		if (m_arenaRankInfo == null)
		{
			return;
		}
		((TextElement)m_lbDiamond).text = m_arenaRankInfo.Diamond.ToString();
		((VisualElement)m_btnDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_arenaRankInfo.Diamond <= 0));
		List<int> list = new List<int>();
		List<zbArenaMonsterCsvRecord> list2 = zbDataConfig.Instance.arenaMonsterCsvTable.GetRecords().records.ToList();
		list2.Sort((zbArenaMonsterCsvRecord a, zbArenaMonsterCsvRecord b) => b.rank.CompareTo(a.rank));
		foreach (zbArenaMonsterCsvRecord item in list2)
		{
			if (m_arenaRankInfo.Rank >= item.rank)
			{
				list.Add(item.mst1);
				list.Add(item.mst2);
				list.Add(item.mst3);
				list.Add(item.mst4);
			}
		}
		m_monster.Refresh(m_roleBaseInfo.FootholdId, list);
	}

	private void RefreshAchieveInfo()
	{
		if (m_achieveInfo != null)
		{
			zbLadderCsvRecord recordByKey = zbDataConfig.Instance.ladderCsvTable.GetRecordByKey(m_achieveInfo.LadderId);
			if (recordByKey != null)
			{
				List<int> monsterIdList = new List<int> { recordByKey.mst1, recordByKey.mst2, recordByKey.mst3, recordByKey.mst4 };
				m_monster.Refresh(m_roleBaseInfo.FootholdId, monsterIdList);
			}
		}
	}

	private void RefreshPeakInfo()
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = zbServerData.sGameData.CrossPeakSelfInfo.DailyCount - zbServerData.sGameData.CrossPeakSelfInfo.CombatCount + "/" + (zbServerData.sGameData.CrossPeakSelfInfo.DailyCount - zbServerData.sGameData.CrossPeakSelfInfo.BuyCount);
		int num = zbSelf.Instance.crossPeakHandler.crossPeakRankInfo.BestRank;
		if (num == 0)
		{
			num = 51;
		}
		int num2 = 0;
		foreach (KeyValuePair<int, zbPeakRankAwardCsvRecord> record in zbDataConfig.Instance.peakRankAwardCsvTable.GetRecords().records)
		{
			if (record.Value.rank >= m_peakRank && record.Value.rank < num)
			{
				num2 += record.Value.bestAward[2];
			}
		}
		((TextElement)m_lbRank).text = m_peakRank.ToString();
		((TextElement)m_lbDiamond).text = num2.ToString();
		((VisualElement)m_btnDiamond).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(num2 <= 0));
		zbPeakRobotCsvRecord recordByKey = zbDataConfig.Instance.peakRobotCsvTable.GetRecordByKey(m_peakRank);
		if (recordByKey != null)
		{
			List<int> monsterIdList = new List<int> { recordByKey.mst1, recordByKey.mst2, recordByKey.mst3, recordByKey.mst4 };
			m_monster.Refresh(m_roleBaseInfo.FootholdId, monsterIdList);
		}
	}

	private void RefreshTruckInfo()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCount).text = zbUtil.Language("lang.11127") + ": " + (zbSelf.Instance.truckHandler.GetRobCombatLimit() - zbServerData.sGameData.TruckSelfInfo.RobUsed);
		bool flag = zbSelf.Instance.truckHandler.GetRobBuyLimit() - zbServerData.sGameData.TruckSelfInfo.RobBought > 0;
		((VisualElement)m_btnAdd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (m_truckInfo != null)
		{
			zbTrainCsvRecord recordByKey = zbDataConfig.Instance.trainCsvTable.GetRecordByKey((int)m_truckInfo.Quality);
			if (recordByKey != null)
			{
				List<int> monsterIdList = new List<int> { recordByKey.mst1, recordByKey.mst2, recordByKey.mst3, recordByKey.mst4 };
				m_monster.Refresh(m_roleBaseInfo.FootholdId, monsterIdList);
			}
		}
	}

	private void OnMonsterPreview(ClickEvent evt)
	{
		zbSelf.Instance.monsterHandler.OnShowMonsterPreview(evt);
	}

	private void OnAdd()
	{
		if (m_battleFuncType == EBattleFuncType.Arena)
		{
			zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.Arena);
		}
		else if (m_battleFuncType == EBattleFuncType.Truck)
		{
			if (zbSelf.Instance.truckHandler.GetRobBuyLimit() - zbServerData.sGameData.TruckSelfInfo.RobBought > 0)
			{
				zbUIMgr.Instance.ShowBuyCount(EBattleFuncType.Truck, 1);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10160"));
			}
		}
	}

	private void OnFight()
	{
		if (m_battleFuncType == EBattleFuncType.Arena)
		{
			if (zbSelf.Instance.arenaHandler.HasFightCountNotify())
			{
				zbWsMsgHandler.Instance.Arena_FightStart_Req((int)m_roleBaseInfo.RoleId, m_arenaRankInfo.Rank);
			}
			else if (zbSelf.Instance.arenaHandler.buyLimit > zbServerData.sGameData.ArenaInfo.BuyCount)
			{
				OnAdd();
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo("挑战次数不足");
			}
		}
		else if (m_battleFuncType == EBattleFuncType.CrossLadderMatch)
		{
			if (!zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
			{
				Hide();
				zbUIMgr.Instance.HideWnd(EWndId.CrossLadderMatchSuccess);
				zbUIMgr.Instance.ShowArenaMain();
			}
			else
			{
				zbWsMsgHandler.Instance.CrossLadder_FightStart_Req((int)m_roleBaseInfo.RoleId);
			}
		}
		else if (m_battleFuncType == EBattleFuncType.PeakArena)
		{
			if (zbServerData.sGameData.CrossPeakSelfInfo.DailyCount > 0)
			{
				zbWsMsgHandler.Instance.CrossPeak_FightStart_Req((int)m_roleBaseInfo.RoleId, m_peakRank);
			}
			else if (zbSelf.Instance.crossPeakHandler.buyLimit > zbServerData.sGameData.CrossPeakSelfInfo.BuyCount)
			{
				OnAdd();
			}
		}
		else if (m_battleFuncType == EBattleFuncType.Truck)
		{
			if (zbSelf.Instance.truckHandler.GetRobCombatLimit() - zbServerData.sGameData.TruckSelfInfo.RobUsed > 0)
			{
				zbWsMsgHandler.Instance.Truck_RobFightStart_Req((int)m_roleBaseInfo.RoleId, m_truckInfo.TransportId);
			}
			else
			{
				OnAdd();
			}
		}
	}

	private void OnHead()
	{
		if (!zbUtil.IsNpc(m_roleBaseInfo.RoleId))
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_roleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10882"));
		}
	}

	private void OnTouchTest()
	{
		Hide();
	}

	private void OnCommFightAck(object obj)
	{
		OnTouchTest();
	}

	private void RefreshPvpTipsInfo()
	{
		skillSelected.refreshSkill();
	}
}
