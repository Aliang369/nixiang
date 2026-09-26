using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbVipTabVip
{
	private zbVipWnd m_wndParent;

	private VisualElement m_RootElement;

	private List<DiamondShopDetail> m_diamondShopDetails;

	private VisualElement m_nodeRechargeSum;

	private SpineVisualElement m_spineRechargeSum;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private Button m_btnRechargeSum;

	private zbItem[] m_items = new zbItem[3];

	private int m_rechargeSumType;

	private VisualElement m_diamondPanel;

	private VisualElement[] m_diamondItems = (VisualElement[])(object)new VisualElement[6];

	private VisualElement[] m_diamondDouble = (VisualElement[])(object)new VisualElement[6];

	private Label[] m_lbDiamondMoney = (Label[])(object)new Label[6];

	private Label[] m_lbDiamondPrice = (Label[])(object)new Label[6];

	private Label[] m_lbDiamondDoubleText = (Label[])(object)new Label[6];

	private Button[] m_btnDiamondBuy = (Button[])(object)new Button[6];

	private Button m_btnCall;

	public void OnEnable(zbVipWnd wndParent, VisualElement root)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_diamondPanel = UQueryExtensions.Q<VisualElement>(m_RootElement, "diamond", (string)null);
		for (int i = 0; i < 6; i++)
		{
			int localIndex = i;
			m_diamondItems[i] = UQueryExtensions.Q<VisualElement>(m_diamondPanel, "diamond" + i, (string)null);
			m_lbDiamondMoney[i] = UQueryExtensions.Q<Label>(m_diamondItems[i], "lb_money", (string)null);
			m_lbDiamondPrice[i] = UQueryExtensions.Q<Label>(m_diamondItems[i], "lb_price", (string)null);
			m_diamondDouble[i] = UQueryExtensions.Q<VisualElement>(m_diamondItems[i], "isDouble", (string)null);
			m_lbDiamondDoubleText[i] = UQueryExtensions.Q<Label>(m_diamondItems[i], "lb_isDouble", (string)null);
			m_diamondDouble[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_btnDiamondBuy[i] = UQueryExtensions.Q<Button>(m_diamondItems[i], "btnbuy", (string)null);
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDiamondBuy[i], (Action)delegate
			{
				OnDiamondItemClick(localIndex);
			}, "", (TrickleDown)0);
		}
		m_nodeRechargeSum = UQueryExtensions.Q<VisualElement>(m_diamondPanel, "node_rechargeSum", (string)null);
		m_spineRechargeSum = UQueryExtensions.Q<SpineVisualElement>(m_nodeRechargeSum, "spine", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_nodeRechargeSum, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_nodeRechargeSum, "lb_progress", (string)null);
		m_btnRechargeSum = UQueryExtensions.Q<Button>(m_nodeRechargeSum, "btn_rechargeSum", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRechargeSum, (Action)OnRechargeSum, "", (TrickleDown)0);
		for (int num = 0; num < 3; num++)
		{
			m_items[num] = UQueryExtensions.Q<zbItem>(m_nodeRechargeSum, "item" + num, (string)null);
			m_items[num].OnEnable(m_wndParent);
		}
		m_btnCall = UQueryExtensions.Q<Button>(m_RootElement, "btn_call", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action)OnCall, "", (TrickleDown)0);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventSuccessOrderAck);
	}

	public void Refresh(int index = 0, int diff = 0)
	{
		RefreshDiamondShop();
		RefreshRechargeSum();
	}

	private void RefreshDiamondShop()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		m_diamondShopDetails = new List<DiamondShopDetail>();
		if (zbServerData.sGameData.RechargeShopInfo != null && zbServerData.sGameData.RechargeShopInfo.DiamondShop != null)
		{
			m_diamondShopDetails = ((IEnumerable<DiamondShopDetail>)zbServerData.sGameData.RechargeShopInfo.DiamondShop).ToList();
		}
		for (int i = 0; i < m_diamondShopDetails.Count; i++)
		{
			DiamondShopDetail diamondShopDetail = m_diamondShopDetails[i];
			if (m_diamondItems[i] != null && diamondShopDetail != null)
			{
				string priceByGoodsId = zbSelf.Instance.vipHandler.GetPriceByGoodsId(diamondShopDetail.GoodsId);
				((TextElement)m_lbDiamondMoney[i]).text = priceByGoodsId;
				((TextElement)m_lbDiamondPrice[i]).text = diamondShopDetail.Diamond.ToString();
				bool flag = diamondShopDetail.GiveDiamond > 0 && diamondShopDetail.Diamond == diamondShopDetail.GiveDiamond;
				m_diamondDouble[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
				if (flag)
				{
					((TextElement)m_lbDiamondDoubleText[i]).text = $"首次额外+{diamondShopDetail.GiveDiamond}";
				}
			}
		}
	}

	private void OnDiamondItemClick(int index)
	{
		if (index >= 0 && index < m_diamondShopDetails.Count)
		{
			DiamondShopDetail diamondShopDetail = m_diamondShopDetails[index];
			if (diamondShopDetail != null)
			{
				zbChannel_Pay.OnPay(diamondShopDetail.GoodsId);
			}
		}
	}

	private void RefreshRechargeSum()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		JourneyTargetRecordDetail journeyRechargeSumTargetDetail = zbSelf.Instance.vipHandler.GetJourneyRechargeSumTargetDetail();
		if (journeyRechargeSumTargetDetail != null)
		{
			m_rechargeSumType = 1;
			int[][] awardList = zbDataConfig.Instance.chapterOneRechargeSumCsvTable.GetRecordByKey(journeyRechargeSumTargetDetail.Index).awardList;
			RefreshRechargeSumDetail(awardList, zbServerData.sGameData.JourneyInfo.ChapterOne.RechargeSum.Reach, journeyRechargeSumTargetDetail.Target, journeyRechargeSumTargetDetail.State);
		}
		else
		{
			RechargeTargetData rechargeTargetData = zbSelf.Instance.vipHandler.GetRechargeTargetData();
			if (rechargeTargetData != null)
			{
				m_rechargeSumType = 2;
				int[][] awardList2 = ((IEnumerable<AwardDetail>)rechargeTargetData.TargetDetail.AwardList).Select((AwardDetail a) => new int[3]
				{
					(int)a.Type,
					(int)a.Tid,
					(int)a.Count
				}).ToArray();
				RefreshRechargeSumDetail(awardList2, rechargeTargetData.Count, rechargeTargetData.TargetDetail.Target, rechargeTargetData.TargetDetail.State);
			}
			else
			{
				m_rechargeSumType = 0;
			}
		}
		m_nodeRechargeSum.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_rechargeSumType <= 0));
	}

	private void RefreshRechargeSumDetail(int[][] awardList, int count, int target, ECommonState state)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		string text = ((state == ECommonState.Yes) ? "sel" : "unsel");
		m_spineRechargeSum.AnimationState.SetAnimation(0, text, true);
		float num = Mathf.Clamp((float)count / (float)target, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num * 296f, (LengthUnit)0));
		((TextElement)m_lbProgress).text = count + "/" + target;
		for (int i = 0; i < 3; i++)
		{
			if (i < awardList.Length)
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				int[] award = awardList[i];
				m_items[i].Refresh(new zbItemData((EResourceType)award[0], award[1], award[2]));
				m_items[i].SetTouchCallback(delegate
				{
					if (state != ECommonState.Yes)
					{
						zbUIMgr.Instance.ShowTip((EResourceType)award[0], award[1], 0L);
					}
					else
					{
						OnRechargeSum();
					}
				});
			}
			else
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private void OnRechargeSum()
	{
		if (zbSelf.Instance.vipHandler.GetJourneyRechargeSumTargetDetail() != null)
		{
			zbUIMgr.Instance.ShowJourneyOneRechargeSum();
			return;
		}
		RechargeTargetData rechargeTargetData = zbSelf.Instance.vipHandler.GetRechargeTargetData();
		if (rechargeTargetData != null)
		{
			zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey((int)rechargeTargetData.OperateType);
			if (rechargeTargetData.OperateType == EOperateType.RechargeSum)
			{
				zbUIMgr.Instance.ShowRechargeSum(rechargeTargetData.UUID, recordByKey);
			}
			else if (rechargeTargetData.OperateType == EOperateType.RechargeDailySum)
			{
				zbUIMgr.Instance.ShowRechargeDailySum(rechargeTargetData.UUID, recordByKey);
			}
			else if (rechargeTargetData.OperateType == EOperateType.RechargeDaily)
			{
				zbUIMgr.Instance.ShowRechargeDaily(rechargeTargetData.UUID, recordByKey);
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10156"));
			RefreshRechargeSum();
		}
	}

	private void OnCall()
	{
		zbUIMgr.Instance.ShowSupportContact();
	}

	private void OnEventSuccessOrderAck(object data)
	{
		RefreshRechargeSum();
		if (m_rechargeSumType == 1)
		{
			zbWsMsgHandler.Instance.Journey_Data_Req();
		}
		else if (m_rechargeSumType == 2)
		{
			zbWsMsgHandler.Instance.Operate_Data_Req();
		}
	}
}
