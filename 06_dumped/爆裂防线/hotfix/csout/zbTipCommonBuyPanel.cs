using System;
using System.Globalization;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbTipCommonBuyPanel
{
	private zbTipCommonWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_btnGoto;

	private Label m_lbTitle;

	private zbItem[] m_items = new zbItem[4];

	private VisualElement m_bonus;

	private Label m_lbBonus;

	private Label m_lbTime;

	private Label m_lbLimitCount;

	private Button m_btnBuy;

	private Label m_lbCost;

	private Label m_lbPrice;

	private zbGetWayBuyPanelData m_data;

	private float m_price;

	public void OnEnable(zbTipCommonWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_bonus = UQueryExtensions.Q<VisualElement>(m_RootElement, "bonus", (string)null);
		m_lbBonus = UQueryExtensions.Q<Label>(m_RootElement, "lb_bonus", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbLimitCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_limitCount", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_lbPrice = UQueryExtensions.Q<Label>(m_RootElement, "lb_price", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action<ClickEvent>)OnBuy, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
	}

	public void Refresh(zbGetWayBuyPanelData data)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (data != null)
		{
			m_data = data;
			if (m_data.Bonus != "")
			{
				m_bonus.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbBonus).text = m_data.Bonus;
			}
			else
			{
				m_bonus.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			((VisualElement)m_lbLimitCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.Limit <= 0));
			((TextElement)m_lbLimitCount).text = zbUtil.Language("lang.10805") + " " + (m_data.Limit - m_data.Count) + "/" + m_data.Limit;
			RefreshName();
			RefreshAwards();
			RefreshPrice();
			RefreshTime();
		}
	}

	private void RefreshName()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (m_data.Type == EGetWayBuyPanelType.RechargeGift || m_data.Type == EGetWayBuyPanelType.RechargeGiftDirect)
		{
			zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(4);
			((TextElement)m_lbTitle).text = recordByKey.name;
			((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			zbGetWayCsvRecord recordByKey2 = zbDataConfig.Instance.getWayCsvTable.GetRecordByKey((int)GetGetWay());
			((TextElement)m_lbTitle).text = recordByKey2.name;
			((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void RefreshAwards()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		AwardDetail[] awardList = m_data.AwardList;
		foreach (AwardDetail awardDetail in awardList)
		{
			if (num < m_items.Length)
			{
				zbItemData data = new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count);
				((VisualElement)m_items[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num].Refresh(data);
			}
			num++;
		}
	}

	private void RefreshPrice()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		m_price = ExtractFloatByFilter(m_data.Price);
		if (m_price <= 0f)
		{
			((TextElement)m_lbPrice).text = zbUtil.Language("lang.10326");
			((VisualElement)m_lbPrice).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		if (m_data.MoneyType == EMoneyType.EmptyValue)
		{
			((TextElement)m_lbPrice).text = m_data.Price;
			((VisualElement)m_lbPrice).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Money, (long)m_data.MoneyType);
		((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((TextElement)m_lbCost).text = m_data.Price;
		((VisualElement)m_lbPrice).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney((int)m_data.MoneyType, (long)m_price))
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.white);
		}
	}

	public float ExtractFloatByFilter(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return 0f;
		}
		if (float.TryParse(new string(input.Where((char c) => char.IsDigit(c) || c == '.' || c == '-').ToArray()), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return result;
		}
		return 0f;
	}

	public void OnGoto(ClickEvent evt)
	{
		zbSelf.Instance.getWayHandler.GetWayBuyIndexOrId = m_data.IndexOrId;
		if (m_data.Type == EGetWayBuyPanelType.RechargeGift || m_data.Type == EGetWayBuyPanelType.RechargeGiftDirect)
		{
			if (m_data.BaseInfo.Type == EOperatePlanType.Holiday)
			{
				zbUIMgr.Instance.ShowRechargeGift(m_data.BaseInfo.UUID);
			}
			else
			{
				zbUIMgr.Instance.ShowVip();
			}
		}
		else
		{
			zbLinkHelper.OpenLink(zbDataConfig.Instance.getWayCsvTable.GetRecordByKey((int)GetGetWay()).link, 0);
		}
	}

	public void OnBuy(ClickEvent evt)
	{
		if (m_data.MoneyType != EMoneyType.EmptyValue && !zbSelf.Instance.moneyHandler.HasEnoughMoney((int)m_data.MoneyType, (long)m_price))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, (long)m_data.MoneyType, (long)m_price);
			return;
		}
		switch (m_data.Type)
		{
		case EGetWayBuyPanelType.RechargeGift:
			zbChannel_Pay.OnPay(m_data.IndexOrId);
			break;
		case EGetWayBuyPanelType.RechargeGiftDirect:
			zbWsMsgHandler.Instance.Operate_RechargeDirect_Req(m_data.BaseInfo.UUID, m_data.IndexOrId);
			break;
		case EGetWayBuyPanelType.DailyPack:
		{
			zbVipDailyPackCsvRecord recordByKey = zbDataConfig.Instance.vipDailyPackCsvTable.GetRecordByKey(m_data.IndexOrId);
			if (recordByKey != null)
			{
				zbChannel_Pay.OnPayWithMoney(recordByKey.goodsId);
			}
			break;
		}
		case EGetWayBuyPanelType.ArenaGift:
			zbWsMsgHandler.Instance.Arena_GiftBuy_Req(m_data.IndexOrId);
			break;
		case EGetWayBuyPanelType.Shop1:
			zbWsMsgHandler.Instance.Shop_Buy_Req(EShopType.Shop1, m_data.IndexOrId, 1);
			break;
		case (EGetWayBuyPanelType)3:
			break;
		}
	}

	private EGetWayType GetGetWay()
	{
		return m_data.Type switch
		{
			EGetWayBuyPanelType.DailyPack => EGetWayType.DailyPack, 
			EGetWayBuyPanelType.ArenaGift => EGetWayType.ArenaGift, 
			EGetWayBuyPanelType.Shop1 => EGetWayType.Shop1, 
			_ => EGetWayType.None, 
		};
	}

	public void RefreshTime()
	{
		if (m_data == null || (m_data.Type != EGetWayBuyPanelType.RechargeGift && m_data.Type != EGetWayBuyPanelType.RechargeGiftDirect && m_data.Type != EGetWayBuyPanelType.DailyPack))
		{
			return;
		}
		if (m_data.Type == EGetWayBuyPanelType.RechargeGift || m_data.Type == EGetWayBuyPanelType.RechargeGiftDirect)
		{
			long directGiftLeftTime = zbSelf.Instance.operateHandler.GetDirectGiftLeftTime(m_data.BaseInfo.UUID);
			if (directGiftLeftTime > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(directGiftLeftTime);
			}
			else
			{
				((TextElement)m_lbTime).text = zbUtil.Language("lang.10397");
			}
		}
		else if (m_data.Type == EGetWayBuyPanelType.DailyPack)
		{
			((TextElement)m_lbTime).text = zbSelf.Instance.timeHandler.GetRefreshTimeStr();
		}
	}
}
