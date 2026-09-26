using System;
using Proto.Enums;
using Proto.Types;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVipTabLimitedItem : zbViewBaseItem
{
	private zbBaseWnd m_wndParent;

	private zbVipTabLimited.RechargeGiftWrapper m_data;

	private Label m_lbGiftName;

	private Label m_lblGiftSingleBought;

	private VisualElement m_priceNode;

	private VisualElement m_gettedMark;

	public Button m_btnBuy;

	private VisualElement m_bgNormal;

	private VisualElement m_bgError;

	private VisualElement m_ResIcon;

	private Label m_lblPrice;

	private Label m_lbLimitTime;

	private zbItem[] m_items;

	private VisualElement m_notifyNormal;

	private VisualElement m_bonus;

	private bool hasEnoughCastCost;

	private int[] m_cost;

	private IVisualElementScheduledItem m_refreshTimer;

	public zbVipTabLimitedItem(zbBaseWnd wndParent, VisualTreeAsset tAsset)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[4];
		hasEnoughCastCost = true;
		base._002Ector();
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = wndParent;
		m_lbGiftName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbltitle", (string)null);
		m_lblGiftSingleBought = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_limitCount", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btnbuy", (string)null);
		m_priceNode = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "nodeprice", (string)null);
		m_ResIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "resIcon", (string)null);
		m_lblPrice = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lblprice", (string)null);
		m_lbLimitTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_LimitTime", (string)null);
		m_gettedMark = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "itemgetted", (string)null);
		m_bgNormal = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg1", (string)null);
		m_bgError = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg2", (string)null);
		m_bonus = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bonus", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "awarditems", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(val, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_notifyNormal = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBuy, (Vector2?)new Vector2(5f, 5f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action<ClickEvent>)delegate
		{
			OnBuy();
		}, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbVipTabLimited.RechargeGiftWrapper data)
	{
		m_data = data;
		if (data != null)
		{
			ResetVisualState();
			switch (data.GiftType)
			{
			case 1:
				RefreshGoods(data.GoodsDetail);
				break;
			case 2:
				RefreshDirect(data.DirectDetail);
				break;
			case 3:
				RefreshFree(data.freeGiftDetail);
				break;
			}
			IVisualElementScheduledItem refreshTimer = m_refreshTimer;
			if (refreshTimer != null)
			{
				refreshTimer.Pause();
			}
			m_refreshTimer = ((VisualElement)this).schedule.Execute((Action)RefreshLeftTime).Every(1000L);
		}
	}

	private void RefreshGoods(RechargeGoodsDetail goods)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		m_bgError.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_gettedMark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_priceNode.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		for (int i = 0; i < m_items.Length; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		for (int j = 0; j < goods.AwardList.Count; j++)
		{
			m_items[j].Refresh(new zbItemData(goods.AwardList[j].Type, goods.AwardList[j].Tid, goods.AwardList[j].Count));
			((VisualElement)m_items[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(goods.GoodsId);
		((TextElement)m_lbGiftName).text = recordByKey.goodsName;
		bool flag = goods.Bonus != null && goods.Bonus != "";
		m_bonus.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			((TextElement)UQueryExtensions.Q<Label>(m_bonus, "lb_bonus", (string)null)).text = goods.Bonus;
		}
		int num = math.max(0, m_data.GoodsDetail.LimitNum - m_data.GoodsDetail.BoughtCount);
		((TextElement)m_lblGiftSingleBought).text = string.Format("{0}: {1}", zbUtil.Language("lang.10430"), num);
		bool flag2 = num <= 0;
		((TextElement)m_btnBuy).text = (flag2 ? zbUtil.Language("lang.10278") : zbSelf.Instance.vipHandler.GetPriceByGoodsId(goods.GoodsId));
		((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag2 ? "ui/image/button/btn_180_80_gray.png" : "ui/image/button/btn_180_80_green.png"));
		RefreshLeftTime();
	}

	private void RefreshDirect(RechargeDirectDetail direct)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		m_bgError.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_gettedMark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_priceNode.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		zbDirectShopCsvRecord recordByKey = zbDataConfig.Instance.directShopCsvTable.GetRecordByKey(direct.DirectId);
		if (recordByKey != null)
		{
			bool flag = recordByKey.bonus != null;
			m_bonus.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				((TextElement)UQueryExtensions.Q<Label>(m_bonus, "lb_bonus", (string)null)).text = recordByKey.bonus;
			}
			for (int i = 0; i < m_items.Length; i++)
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			for (int j = 0; j < recordByKey.awardList.Length; j++)
			{
				m_items[j].Refresh(new zbItemData((EResourceType)recordByKey.awardList[j][0], recordByKey.awardList[j][1], recordByKey.awardList[j][2]));
				((VisualElement)m_items[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			((TextElement)m_lbGiftName).text = recordByKey.name;
			int num = math.max(0, m_data.DirectDetail.LimitNum - m_data.DirectDetail.BoughtCount);
			((TextElement)m_lblGiftSingleBought).text = string.Format("{0}: {1}", zbUtil.Language("lang.10430"), num);
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)recordByKey.cost[0], recordByKey.cost[1]);
			m_ResIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
			((TextElement)m_lblPrice).text = recordByKey.cost[2].ToString();
			bool flag2 = num <= 0;
			m_priceNode.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
			((TextElement)m_btnBuy).text = (flag2 ? zbUtil.Language("lang.10278") : "");
			((VisualElement)m_btnBuy).pickingMode = (PickingMode)(flag2 ? 1 : 0);
			((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag2 ? "ui/image/button/btn_180_80_gray.png" : "ui/image/button/btn_180_80_green.png"));
			m_cost = recordByKey.cost;
			hasEnoughCastCost = zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)recordByKey.cost[0], recordByKey.cost[1], recordByKey.cost[2]) && num > 0;
			RefreshLeftTime();
		}
	}

	private void RefreshFree(FreeGiftDetail freeGift)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		m_bgError.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_priceNode.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_bonus.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		for (int i = 0; i < m_items.Length; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbFreeGiftCsvRecord zbFreeGiftCsvRecord2 = ((m_data.BaseInfo.Type != EOperatePlanType.Holiday) ? zbDataConfig.Instance.freeGiftCsvTable.GetRecordByKey(1) : zbDataConfig.Instance.freeGiftCsvTable.GetRecordByKey(2));
		int num = zbFreeGiftCsvRecord2.award.Length / 3;
		for (int j = 0; j < num; j++)
		{
			m_items[j].Refresh(new zbItemData((EResourceType)zbFreeGiftCsvRecord2.award[0], zbFreeGiftCsvRecord2.award[1], zbFreeGiftCsvRecord2.award[2]));
			((VisualElement)m_items[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		((TextElement)m_lbGiftName).text = zbUtil.Language("lang.10399");
		((TextElement)m_lblGiftSingleBought).text = ((freeGift.State == EBoolState.False) ? (zbUtil.Language("lang.10430") + ": 1") : (zbUtil.Language("lang.10430") + ": 0"));
		((TextElement)m_btnBuy).text = zbUtil.Language("lang.10090");
		m_gettedMark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(freeGift.State == EBoolState.False));
		((VisualElement)m_btnBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((freeGift.State != EBoolState.False) ? 1 : 0));
		((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
		m_notifyNormal.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasRechargeGiftNotify(1)));
	}

	private void OnBuy()
	{
		if (m_data == null)
		{
			return;
		}
		switch (m_data.GiftType)
		{
		case 1:
			if (m_data.GoodsDetail.LimitNum != 0 && m_data.GoodsDetail.BoughtCount >= m_data.GoodsDetail.LimitNum)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10398"));
			}
			else
			{
				zbChannel_Pay.OnPay(m_data.GoodsDetail.GoodsId);
			}
			break;
		case 2:
			if (m_data.DirectDetail.LimitNum != 0 && m_data.DirectDetail.BoughtCount >= m_data.DirectDetail.LimitNum)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10398"));
			}
			else if (!hasEnoughCastCost && m_cost != null)
			{
				zbUIMgr.Instance.ShowTip((EResourceType)m_cost[0], m_cost[1], m_cost[2]);
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_RechargeDirect_Req(m_data.BaseInfo.UUID, m_data.DirectDetail.DirectId);
			}
			break;
		case 3:
			zbWsMsgHandler.Instance.Aux_FreeGiftAward_Req(m_data.freeGiftDetail.FreeType);
			break;
		}
	}

	private void RefreshLeftTime()
	{
		if (m_data != null && m_data != null)
		{
			long directGiftLeftTime = zbSelf.Instance.operateHandler.GetDirectGiftLeftTime(m_data.BaseInfo.UUID);
			if (directGiftLeftTime > 0)
			{
				((TextElement)m_lbLimitTime).text = zbUtil.FormatTimeHHMMSS(directGiftLeftTime);
			}
			else
			{
				((TextElement)m_lbLimitTime).text = zbUtil.Language("lang.10397");
			}
		}
	}

	private void ResetVisualState()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		m_bgError.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_gettedMark.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_priceNode.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnBuy).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnBuy).pickingMode = (PickingMode)0;
		((TextElement)m_lbLimitTime).text = "";
		((TextElement)m_lblGiftSingleBought).text = "";
		for (int i = 0; i < m_items.Length; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_ResIcon.style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
		((TextElement)m_lblPrice).text = "";
	}

	public void Dispose()
	{
		IVisualElementScheduledItem refreshTimer = m_refreshTimer;
		if (refreshTimer != null)
		{
			refreshTimer.Pause();
		}
	}
}
