using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbVipTabVipCardPanel
{
	private zbVipWnd m_wndParent;

	private zbVipTabVipCard m_parent;

	private VisualElement m_rootElement;

	private Label m_lbFuncTitle;

	private Label m_lbFuncDesc;

	private Button m_btnFuncBuy;

	private Label m_lbFuncBuyDay;

	private Label m_lbFuncBuyPrice;

	private VisualElement m_nodeFuncBought;

	private VisualElement m_progressFunc;

	private Label m_lbProgressFunc;

	private Button m_btnFuncBought;

	private Label m_lbFuncBoughtDay;

	private VisualElement m_weekCard;

	private Label m_lbWeekTitle;

	private Label m_lbWeekDiamond;

	private Label m_lbWeekOff;

	private Label m_lbWeekState;

	private zbItem[] m_weekItems = new zbItem[4];

	private Button m_btnWeekBuy;

	private Label m_lbWeekBuyDay;

	private Label m_lbWeekBuyPrice;

	private VisualElement m_nodeWeekBought;

	private VisualElement m_progressWeek;

	private Label m_lbProgressWeek;

	private Button m_btnMainQuest;

	private Button m_btnElite;

	private Button m_btnLegendary;

	private Button m_btnFacility;

	private VisualElement m_commonCard;

	private Label m_lbCommonTitle;

	private Label m_lbCommonDesc;

	private Button m_btnCommonBuy;

	private Label m_lbCommonBuy;

	private VisualElement m_crossLadderCard;

	private Label m_lbCrossLadderTitle;

	private Label m_lbCrossLadderDesc;

	private Button m_btnCrossLadderBuy;

	private Label m_lbCrossLadderBuyDay;

	private Label m_lbCrossLadderBuyPrice;

	private VisualElement m_nodeCrossLadderBought;

	private VisualElement m_progressCrossLadder;

	private Label m_lbProgressCrossLadder;

	private Button m_btnCrossLadderBought;

	private Label m_lbCrossLadderBoughtDay;

	private VisualElement m_nodeDaily;

	private VisualElement m_spriteDaily;

	private Label m_lbDaily;

	private Button m_btnDaily;

	private VisualElement m_notifyDaily;

	private VipCardShopDetail m_funcCardInfo;

	private VipCardShopDetail m_weekCardInfo;

	private int m_commonGoodsId;

	private int m_curTab = -1;

	public void OnEnable(zbVipWnd wndParent, zbVipTabVipCard parent, VisualElement root)
	{
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_parent = parent;
		m_rootElement = root;
		m_weekCard = UQueryExtensions.Q<VisualElement>(m_rootElement, "week", (string)null);
		m_lbWeekTitle = UQueryExtensions.Q<Label>(m_weekCard, "lb_title", (string)null);
		m_lbWeekDiamond = UQueryExtensions.Q<Label>(m_weekCard, "lb_diamond", (string)null);
		m_lbWeekOff = UQueryExtensions.Q<Label>(m_weekCard, "lb_off", (string)null);
		m_lbWeekState = UQueryExtensions.Q<Label>(m_weekCard, "lb_state", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_weekItems[i] = UQueryExtensions.Q<zbItem>(m_weekCard, "item" + i, (string)null);
			m_weekItems[i].OnEnable(m_wndParent);
		}
		m_btnWeekBuy = UQueryExtensions.Q<Button>(m_weekCard, "btn_buy", (string)null);
		m_lbWeekBuyDay = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnWeekBuy, "lb_buyDay", (string)null);
		m_lbWeekBuyPrice = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnWeekBuy, "lb_buyPrice", (string)null);
		m_nodeWeekBought = UQueryExtensions.Q<VisualElement>(m_weekCard, "bought", (string)null);
		m_progressWeek = UQueryExtensions.Q<VisualElement>(m_nodeWeekBought, "progress", (string)null);
		m_lbProgressWeek = UQueryExtensions.Q<Label>(m_nodeWeekBought, "lb_progress", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWeekBuy, (Action)OnWeekBuy, "", (TrickleDown)0);
		m_btnMainQuest = UQueryExtensions.Q<Button>(m_weekCard, "btn_mainQuest", (string)null);
		m_btnElite = UQueryExtensions.Q<Button>(m_weekCard, "btn_elite", (string)null);
		m_btnLegendary = UQueryExtensions.Q<Button>(m_weekCard, "btn_legendary", (string)null);
		m_btnFacility = UQueryExtensions.Q<Button>(m_weekCard, "btn_facility", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMainQuest, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.MainQuest);
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnElite, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.EliteQuest);
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegendary, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.Legendary);
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFacility, (Action<ClickEvent>)delegate
		{
			OnDoubleTip(EFuncType.Facility);
		}, "", (TrickleDown)0);
		m_commonCard = UQueryExtensions.Q<VisualElement>(m_rootElement, "common", (string)null);
		m_lbCommonTitle = UQueryExtensions.Q<Label>(m_commonCard, "lb_title", (string)null);
		m_lbCommonDesc = UQueryExtensions.Q<Label>(m_commonCard, "lb_desc", (string)null);
		m_btnCommonBuy = UQueryExtensions.Q<Button>(m_commonCard, "btn_buy", (string)null);
		m_lbCommonBuy = UQueryExtensions.Q<Label>(m_commonCard, "lb_buy", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCommonBuy, (Action)OnCommonBuy, "", (TrickleDown)0);
		m_crossLadderCard = UQueryExtensions.Q<VisualElement>(m_rootElement, "crossLadder", (string)null);
		m_lbCrossLadderTitle = UQueryExtensions.Q<Label>(m_crossLadderCard, "lb_title", (string)null);
		m_lbCrossLadderDesc = UQueryExtensions.Q<Label>(m_crossLadderCard, "lb_desc", (string)null);
		m_btnCrossLadderBuy = UQueryExtensions.Q<Button>(m_crossLadderCard, "btn_buy", (string)null);
		m_lbCrossLadderBuyDay = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnCrossLadderBuy, "lb_buyDay", (string)null);
		m_lbCrossLadderBuyPrice = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnCrossLadderBuy, "lb_buyPrice", (string)null);
		m_nodeCrossLadderBought = UQueryExtensions.Q<VisualElement>(m_crossLadderCard, "bought", (string)null);
		m_progressCrossLadder = UQueryExtensions.Q<VisualElement>(m_nodeCrossLadderBought, "progress", (string)null);
		m_lbProgressCrossLadder = UQueryExtensions.Q<Label>(m_nodeCrossLadderBought, "lb_progress", (string)null);
		m_btnCrossLadderBought = UQueryExtensions.Q<Button>(m_nodeCrossLadderBought, "btn_bought", (string)null);
		m_lbCrossLadderBoughtDay = UQueryExtensions.Q<Label>(m_nodeCrossLadderBought, "lb_moreDays", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCrossLadderBuy, (Action)OnCommonBuy, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCrossLadderBought, (Action)OnCommonBuy, "", (TrickleDown)0);
		m_nodeDaily = UQueryExtensions.Q<VisualElement>(m_rootElement, "node_daily", (string)null);
		m_spriteDaily = UQueryExtensions.Q<VisualElement>(m_nodeDaily, "bg_daily", (string)null);
		m_lbDaily = UQueryExtensions.Q<Label>(m_nodeDaily, "lb_daily", (string)null);
		m_btnDaily = UQueryExtensions.Q<Button>(m_nodeDaily, "btn_daily", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDaily, (Action)OnDaily, "", (TrickleDown)0);
		m_notifyDaily = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnDaily, (Vector2?)new Vector2(15f, -2f), 1f, true);
		m_notifyDaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public void Refresh()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		m_weekCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_parent.m_curTab != EVipCardTab.Week) ? 1 : 0));
		m_commonCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_parent.m_curTab != EVipCardTab.Truck && m_parent.m_curTab != EVipCardTab.Treasure) ? 1 : 0));
		m_crossLadderCard.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_parent.m_curTab != EVipCardTab.CrossLadder));
		m_nodeDaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_parent.m_curTab == EVipCardTab.Week)
		{
			RefreshWeekCard();
		}
		else if (m_parent.m_curTab == EVipCardTab.Truck)
		{
			RefreshTruckCard();
		}
		else if (m_parent.m_curTab == EVipCardTab.Treasure)
		{
			RefreshTreasureCard();
		}
		else if (m_parent.m_curTab == EVipCardTab.CrossLadder)
		{
			RefreshCrossLadderCard();
		}
		RefreshScene();
	}

	public void RefreshScene()
	{
		m_curTab = (int)m_parent.m_curTab;
		if (m_parent.m_curTab == EVipCardTab.Week)
		{
			m_wndParent.m_spineScene.SetSkeletonDataAsset("animation/ui/yueka_bg5_1/yueka_bg5_1_SkeletonData.asset");
		}
		else if (m_parent.m_curTab == EVipCardTab.Truck)
		{
			m_wndParent.m_spineScene.SetSkeletonDataAsset("animation/ui/yueka_bg3/yueka_bg3_SkeletonData.asset");
		}
		else if (m_parent.m_curTab == EVipCardTab.Treasure)
		{
			m_wndParent.m_spineScene.SetSkeletonDataAsset("animation/ui/yueka_bg2/yueka_bg2_SkeletonData.asset");
		}
		else if (m_parent.m_curTab == EVipCardTab.CrossLadder)
		{
			m_wndParent.m_spineScene.SetSkeletonDataAsset("animation/ui/yueka_bg4/yueka_bg4_SkeletonData.asset");
		}
		m_wndParent.m_spineScene.SetAnimation("animation", isLoop: true);
	}

	private void RefreshWeekCard()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_weekItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		int num2 = (int)zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"resource"][(object)"goodsId"];
		int num3 = (int)zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"resource"][(object)"cardDays"];
		foreach (JToken item in (JArray)zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"resource"][(object)"dailyAward"])
		{
			if (num < m_weekItems.Length)
			{
				((VisualElement)m_weekItems[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbItemData data = new zbItemData((EResourceType)(int)item[(object)0], (int)item[(object)1], (int)item[(object)2]);
				m_weekItems[num].Refresh(data);
			}
			num++;
		}
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(num2);
		if (recordByKey != null)
		{
			int[] array = recordByKey.awardList[0];
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)array[0], array[1]);
			((VisualElement)m_lbWeekDiamond).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
			((TextElement)m_lbWeekDiamond).text = "x" + array[2];
		}
		((TextElement)m_lbWeekBuyPrice).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(num2);
		((TextElement)m_lbWeekBuyDay).text = string.Format(zbUtil.Language("lang.10603"), num3);
		m_weekCardInfo = zbSelf.Instance.vipHandler.GetVipCardShopDetail(num2);
		if (m_weekCardInfo != null && m_weekCardInfo.DailyAward != EActiveState.Pending)
		{
			((VisualElement)m_btnWeekBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeWeekBought.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int num4 = (int)Math.Ceiling((double)(m_weekCardInfo.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime()) / 86400.0);
			float num5 = Mathf.Clamp((float)num4 / (float)num3, 0f, 1f);
			m_progressWeek.style.width = StyleLength.op_Implicit(new Length(num5 * 636f, (LengthUnit)0));
			((TextElement)m_lbProgressWeek).text = string.Format(zbUtil.Language("lang.10604"), num4);
			if (m_weekCardInfo.DailyAward == EActiveState.Active)
			{
				((TextElement)m_lbWeekState).text = zbUtil.Language("lang.10606");
			}
			else
			{
				((TextElement)m_lbWeekState).text = zbUtil.Language("lang.10607");
			}
			for (int j = 0; j < num; j++)
			{
				m_weekItems[j].SetSpineVisible(m_weekCardInfo.DailyAward == EActiveState.Active);
				m_weekItems[j].SetGettedVisible(m_weekCardInfo.DailyAward != EActiveState.Active);
				if (m_weekCardInfo.DailyAward == EActiveState.Active)
				{
					m_weekItems[j].SetTouchCallback(delegate
					{
						OnWeekGet();
					});
				}
				else
				{
					m_weekItems[j].SetTouchCallback(null);
				}
			}
		}
		else
		{
			((VisualElement)m_btnWeekBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeWeekBought.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbWeekState).text = zbUtil.Language("lang.10605");
		}
	}

	private void RefreshTruckCard()
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCommonTitle).text = zbUtil.Language("lang.11200");
		string text = "";
		foreach (zbVipTruckCsvRecord item in zbDataConfig.Instance.vipTruckCsvTable.GetRecords().records.Values.ToList())
		{
			text = text + item.desc + "\n";
		}
		((TextElement)m_lbCommonDesc).text = text;
		m_commonGoodsId = zbSelf.Instance.vipHandler.m_vipBaseRecord["truckPrivilege"].Value<int>((object)"goodsId");
		if (zbServerData.sGameData.TruckSelfInfo.Privilege == EBoolState.True)
		{
			((TextElement)m_lbCommonBuy).text = zbUtil.Language("lang.10721");
		}
		else
		{
			((TextElement)m_lbCommonBuy).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_commonGoodsId);
		}
		m_nodeDaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.truckHandler.HasFreePackRedDot()));
		RefreshDaily(10);
	}

	private void RefreshTreasureCard()
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbCommonTitle).text = zbUtil.Language("lang.11201");
		string text = "";
		foreach (zbTreasurePrivilegeCsvRecord item in zbDataConfig.Instance.treasurePrivilegeCsvTable.GetRecords().records.Values.ToList())
		{
			text = text + item.desc + "\n";
		}
		((TextElement)m_lbCommonDesc).text = text;
		m_commonGoodsId = zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.GoodsId;
		if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True)
		{
			((TextElement)m_lbCommonBuy).text = zbUtil.Language("lang.10721");
		}
		else
		{
			((TextElement)m_lbCommonBuy).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_commonGoodsId);
		}
		m_nodeDaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasFreePackRedDot()));
		RefreshDaily(8);
	}

	private void RefreshCrossLadderCard()
	{
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		foreach (zbVipCrossLadderCsvRecord item in zbDataConfig.Instance.vipCrossLadderCsvTable.GetRecords().records.Values.ToList())
		{
			text = text + item.desc + "\n";
		}
		((TextElement)m_lbCrossLadderDesc).text = text;
		m_commonGoodsId = zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"crossLadder"].Value<int>((object)"goodsId");
		int num = (int)zbSelf.Instance.vipHandler.m_vipBaseRecord["vipCard"][(object)"crossLadder"][(object)"cardDays"];
		((TextElement)m_lbCrossLadderBuyPrice).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_commonGoodsId);
		((TextElement)m_btnCrossLadderBought).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_commonGoodsId);
		bool flag = zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard();
		((TextElement)m_lbCrossLadderBuyDay).text = string.Format(zbUtil.Language("lang.10916"), num);
		m_nodeCrossLadderBought.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_btnCrossLadderBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (flag)
		{
			((TextElement)m_lbCrossLadderBoughtDay).text = string.Format(zbUtil.Language("lang.10917"), num);
			int num2 = (int)Math.Floor((double)(zbSelf.Instance.crossLadderHandler.GetCrossLadderCardDetail().ExpireTm - zbSelf.Instance.timeHandler.GetServerTime()) / 86400.0);
			float num3 = Mathf.Clamp((float)num2 / (float)num, 0f, 1f);
			m_progressCrossLadder.style.width = StyleLength.op_Implicit(new Length(num3 * 564f, (LengthUnit)0));
			((TextElement)m_lbProgressCrossLadder).text = string.Format(zbUtil.Language("lang.10604"), num2);
		}
		m_nodeDaily.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.crossLadderHandler.HasFreePackRedDot()));
		RefreshDaily(7);
	}

	private void RefreshDaily(int type)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		zbFreeGiftCsvRecord recordByKey = zbDataConfig.Instance.freeGiftCsvTable.GetRecordByKey(type);
		if (recordByKey != null)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)recordByKey.award[0], recordByKey.award[1]);
			m_spriteDaily.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
			((TextElement)m_lbDaily).text = "x" + recordByKey.award[2];
		}
	}

	private void OnFuncBuy()
	{
		if (m_funcCardInfo != null)
		{
			zbChannel_Pay.OnPay(m_funcCardInfo.GoodsId);
		}
	}

	private void OnWeekBuy()
	{
		if (m_weekCardInfo != null)
		{
			zbChannel_Pay.OnPay(m_weekCardInfo.GoodsId);
		}
	}

	private void OnDoubleTip(EFuncType type)
	{
		if (type == EFuncType.MainQuest)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11359"), zbUtil.Language("lang.11054")));
			return;
		}
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey((int)type);
		if (recordByKey != null)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11359"), recordByKey.name));
		}
	}

	private void OnWeekGet()
	{
		zbWsMsgHandler.Instance.RechargeShop_VipCardAward_Req(m_weekCardInfo.GoodsId);
	}

	private void OnCommonBuy()
	{
		if ((m_parent.m_curTab == EVipCardTab.Truck && zbServerData.sGameData.TruckSelfInfo.Privilege == EBoolState.True) || (m_parent.m_curTab == EVipCardTab.Treasure && zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10721"));
		}
		else
		{
			zbChannel_Pay.OnPay(m_commonGoodsId);
		}
	}

	private void OnDaily()
	{
		if (m_parent.m_curTab == EVipCardTab.Truck && zbSelf.Instance.truckHandler.HasFreePackRedDot())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(10);
		}
		else if (m_parent.m_curTab == EVipCardTab.Treasure && zbSelf.Instance.treasureHandler.HasFreePackRedDot())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(8);
		}
		else if (m_parent.m_curTab == EVipCardTab.CrossLadder && zbSelf.Instance.crossLadderHandler.HasFreePackRedDot())
		{
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(7);
		}
	}
}
