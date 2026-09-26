using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbBuyCountWnd : zbBaseWnd
{
	private zbConsumeTargetItem m_consumeTarget;

	private Button m_btnClose;

	private Button m_btnBuy;

	private Button m_btnTouch;

	private Label m_lbNum;

	private Label m_lbCount;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Label m_lbDesc;

	private Label m_lbDesc2;

	private VisualElement m_iconMoney;

	private Label m_lbPrice;

	private int m_count = 1;

	private JToken m_bossInfo;

	private int buyNum;

	private zbItemData costData = new zbItemData(EResourceType.EmptyValue, 0L, 0L);

	private EBattleFuncType battleFuncType;

	private int maxCount;

	private int extraParam;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_consumeTarget = UQueryExtensions.Q<zbConsumeTargetItem>(m_RootElement, "consumeTarget", (string)null);
		m_consumeTarget.OnEnable(this);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbDesc2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc2", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_iconMoney = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_money", (string)null);
		m_lbPrice = UQueryExtensions.Q<Label>(m_RootElement, "lb_price", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, delegate
		{
			RefreshOperate();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			RefreshOperate();
		});
	}

	public void RefreshCustom(EBattleFuncType _battleFuncType, int param)
	{
		//IL_079e: Unknown result type (might be due to invalid IL or missing references)
		RefreshOperate();
		battleFuncType = _battleFuncType;
		extraParam = param;
		if (battleFuncType == EBattleFuncType.WorldBoss)
		{
			m_bossInfo = (JToken)(object)zbDataConfig.Instance.worldBossBaseJsonTable.GetRecord();
			buyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldBossBuy, zbSelf.Instance.GetVipLevel());
			maxCount = buyNum - zbServerData.sGameData.WorldBossSelfInfo.BuyCombat;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = Extensions.Value<int>((IEnumerable<JToken>)m_bossInfo[(object)"buyCost"]);
		}
		else if (battleFuncType == EBattleFuncType.LegionBoss)
		{
			m_bossInfo = zbDataConfig.Instance.legionBaseJsonTable.GetRecord()["boss"];
			buyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.LegionBossBuy, zbSelf.Instance.GetVipLevel());
			maxCount = buyNum - zbServerData.sGameData.LegionSelfInfo.BossInfo.BuyCombat;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = Extensions.Value<int>((IEnumerable<JToken>)m_bossInfo[(object)"costDiamond"]);
		}
		else if (battleFuncType == EBattleFuncType.WorldLair || battleFuncType == EBattleFuncType.WorldLairHelp)
		{
			m_bossInfo = zbDataConfig.Instance.worldBaseJsonTable.GetRecord()["lair"];
			buyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.WorldLairBuy, zbSelf.Instance.GetVipLevel());
			maxCount = buyNum - zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.BuyNum;
			JToken[] array = ((IEnumerable<JToken>)m_bossInfo[(object)"buyCost"]).ToArray();
			costData.type = (EResourceType)Extensions.Value<int>((IEnumerable<JToken>)array[0]);
			costData.id = Extensions.Value<int>((IEnumerable<JToken>)array[1]);
			costData.count = Extensions.Value<int>((IEnumerable<JToken>)array[2]);
		}
		else if (battleFuncType == EBattleFuncType.ElementTrialPhy)
		{
			m_bossInfo = (JToken)(object)zbDataConfig.Instance.elementTrialBaseJsonTable.GetRecord();
			buyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.ElementTrialBuy, zbSelf.Instance.GetVipLevel());
			maxCount = buyNum - zbServerData.sGameData.ElementTrialInfo.BuyCombat;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = Extensions.Value<int>((IEnumerable<JToken>)m_bossInfo[(object)"buyCombatCost"]);
		}
		else if (battleFuncType == EBattleFuncType.EndlessHorde)
		{
			m_bossInfo = (JToken)(object)zbDataConfig.Instance.endlessHordeBaseJsonTable.GetRecord();
			buyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.EndlessHordeBuy, zbSelf.Instance.GetVipLevel());
			maxCount = buyNum - zbServerData.sGameData.EndlessHordeInfo.BuyCombat;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = Extensions.Value<int>((IEnumerable<JToken>)m_bossInfo[(object)"buyCombatCost"]);
		}
		else if (battleFuncType == EBattleFuncType.Arena)
		{
			m_bossInfo = (JToken)(object)zbDataConfig.Instance.arenaBaseJsonTable.GetRecord();
			buyNum = Extensions.Value<int>((IEnumerable<JToken>)m_bossInfo[(object)"buyLimit"]);
			maxCount = buyNum - zbServerData.sGameData.ArenaInfo.BuyCount;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = Extensions.Value<int>((IEnumerable<JToken>)m_bossInfo[(object)"buyDiamond"]);
		}
		else if (battleFuncType == EBattleFuncType.CrossLadderMatch)
		{
			buyNum = zbSelf.Instance.crossLadderHandler.GetBuyLimit();
			maxCount = buyNum - zbServerData.sGameData.CrossLadderInfo.SelfInfo.BuyCombat;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = zbSelf.Instance.crossLadderHandler.buyDiamond;
		}
		else if (battleFuncType == EBattleFuncType.MineWar)
		{
			buyNum = zbSelf.Instance.mineWarHandler.GetBuyLimit();
			maxCount = buyNum - zbSelf.Instance.mineWarHandler.mineWarBaseInfo.MineWarSelfInfo.BuyCombat;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = zbSelf.Instance.mineWarHandler.buyDiamond;
		}
		else if (battleFuncType == EBattleFuncType.PeakArena)
		{
			buyNum = zbSelf.Instance.crossPeakHandler.GetBuyLimit();
			maxCount = buyNum - zbServerData.sGameData.CrossPeakSelfInfo.BuyCount;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = zbSelf.Instance.crossPeakHandler.buyDiamond;
		}
		else if (battleFuncType == EBattleFuncType.Truck)
		{
			if (extraParam == 1)
			{
				buyNum = zbSelf.Instance.truckHandler.GetRobBuyLimit();
				maxCount = buyNum - zbServerData.sGameData.TruckSelfInfo.RobBought;
				costData.type = EResourceType.Money;
				costData.id = 2L;
				costData.count = zbSelf.Instance.truckHandler.dailyRobBuyDiamond;
			}
			else
			{
				buyNum = zbSelf.Instance.truckHandler.GetTransportBuyLimit();
				maxCount = buyNum - zbServerData.sGameData.TruckSelfInfo.TransportBought;
				costData.type = EResourceType.Money;
				costData.id = 2L;
				costData.count = zbSelf.Instance.truckHandler.dailyTransportBuyDiamond;
			}
		}
		else if (battleFuncType == EBattleFuncType.EliteQuest)
		{
			buyNum = zbSelf.Instance.vipHandler.CheckVipFuncLimited(EVipFuncType.EliteQuestBuy, zbSelf.Instance.GetVipLevel());
			m_bossInfo = (JToken)(object)zbDataConfig.Instance.questBaseJsonTable.GetRecord();
			maxCount = buyNum - zbServerData.sGameData.EliteQuestInfo.BuyCombat;
			costData.type = EResourceType.Money;
			costData.id = 2L;
			costData.count = Extensions.Value<int>((IEnumerable<JToken>)m_bossInfo[(object)"elite"][(object)"buyCombatDiamond"]);
		}
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(costData.type, costData.id);
		m_count = 1;
		((TextElement)m_lbNum).text = m_count.ToString();
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10736"), costData.count, resorceIconPath.strName);
		((TextElement)m_lbDesc2).text = string.Format(zbUtil.Language("lang.10487"), maxCount);
		m_iconMoney.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((TextElement)m_lbPrice).text = "X" + costData.count * m_count;
		if (battleFuncType == EBattleFuncType.MineWar)
		{
			string format = zbUtil.Language("lang.11001");
			((TextElement)m_lbDesc).text = string.Format(format, costData.count, resorceIconPath.strName);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnBuy()
	{
		if (maxCount == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10828"));
		}
		else if (!zbSelf.Instance.dataHandler.HasResourceEnough(costData.type, (int)costData.id, (int)costData.count * m_count))
		{
			zbUIMgr.Instance.ShowTip(costData.type, (int)costData.id, costData.count * m_count);
		}
		else if (battleFuncType == EBattleFuncType.WorldBoss)
		{
			zbWsMsgHandler.Instance.WorldBoss_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.LegionBoss)
		{
			zbWsMsgHandler.Instance.LegionBoss_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.WorldLair || battleFuncType == EBattleFuncType.WorldLairHelp)
		{
			zbWsMsgHandler.Instance.WorldLair_BuyFightNum_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.ElementTrialPhy)
		{
			zbWsMsgHandler.Instance.ElementTrial_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.EndlessHorde)
		{
			zbWsMsgHandler.Instance.EndlessHorde_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.Arena)
		{
			zbWsMsgHandler.Instance.Arena_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.CrossLadderMatch)
		{
			zbWsMsgHandler.Instance.CrossLadder_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.MineWar)
		{
			zbWsMsgHandler.Instance.MineWar_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.PeakArena)
		{
			zbWsMsgHandler.Instance.CrossPeak_BuyCombat_Req(m_count);
		}
		else if (battleFuncType == EBattleFuncType.Truck)
		{
			if (extraParam == 1)
			{
				zbWsMsgHandler.Instance.Truck_BuyRob_Req(m_count);
			}
			else
			{
				zbWsMsgHandler.Instance.Truck_BuyTransport_Req(m_count);
			}
		}
		else if (battleFuncType == EBattleFuncType.EliteQuest)
		{
			zbWsMsgHandler.Instance.EliteQuest_BuyCombat_Req(m_count);
		}
	}

	private void OnMin()
	{
		m_count = 1;
		((TextElement)m_lbNum).text = m_count.ToString();
		((TextElement)m_lbPrice).text = "X" + costData.count * m_count;
	}

	private void OnSubtract()
	{
		if (m_count != 1)
		{
			m_count--;
			((TextElement)m_lbNum).text = m_count.ToString();
			((TextElement)m_lbPrice).text = "X" + costData.count * m_count;
		}
	}

	private void OnAdd()
	{
		if (m_count != maxCount && maxCount != 0)
		{
			m_count++;
			((TextElement)m_lbNum).text = m_count.ToString();
			((TextElement)m_lbPrice).text = "X" + costData.count * m_count;
		}
	}

	private void OnMax()
	{
		if (maxCount != 0)
		{
			m_count = maxCount;
			((TextElement)m_lbNum).text = m_count.ToString();
			((TextElement)m_lbPrice).text = "X" + costData.count * m_count;
		}
	}

	private void OnEventUpdate(object data)
	{
		OperateConsumTargetData consumeTargetDetail = zbSelf.Instance.shopHandler.GetConsumeTargetDetail();
		if (consumeTargetDetail != null)
		{
			RefreshCustom(battleFuncType, extraParam);
			if (consumeTargetDetail.UUID == "")
			{
				zbWsMsgHandler.Instance.Journey_Data_Req();
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_Data_Req();
			}
		}
		else
		{
			Hide();
		}
	}

	private void RefreshOperate()
	{
		m_consumeTarget.Refresh();
	}
}
