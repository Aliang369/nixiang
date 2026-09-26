using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityEventShopItem : zbViewBaseItem
{
	private zbFacilityEventShopWnd m_wndParent;

	private VisualElement m_spriteBg;

	private VisualElement m_spriteIcon;

	private VisualElement m_soldOut;

	private Label m_lbName;

	private Label m_lbLimted;

	private Label m_lbCount;

	private Button m_btnItemTip;

	private Button m_btnBuy;

	private Label m_lbCost;

	private Label m_lbFree;

	private FacilityGoodsDetail m_data;

	private zbFacilityShopCsvRecord m_record;

	public zbFacilityEventShopItem(zbFacilityEventShopWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/facility/tb_facilityEventShop.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLimted = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_limited", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnItemTip = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_item", (string)null);
		m_soldOut = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "soldOut", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_buy", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_cost", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_free", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action<ClickEvent>)OnBuy, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnItemTip, (Action<ClickEvent>)OnShowItemTip, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(FacilityGoodsDetail data)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_record = zbDataConfig.Instance.facilityShopCsvTable.GetRecordByKey(m_data.GoodsId);
		string shopFrameTex = zbResourceHelper.GetShopFrameTex(zbResourceHelper.GetResourceQuality((EResourceType)m_record.resType, m_record.resId));
		m_spriteBg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(shopFrameTex));
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)m_record.resType, m_record.resId);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath.strIconPath));
		((TextElement)m_lbName).text = resorceIconPath.strName;
		((TextElement)m_lbCount).text = "x" + m_record.resCount;
		if (m_data.LimitNum <= 0)
		{
			((VisualElement)m_lbLimted).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_soldOut.visible = false;
		}
		else
		{
			((VisualElement)m_lbLimted).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLimted).text = zbUtil.Language("lang.10805") + " " + (m_data.LimitNum - m_data.BuyCount) + "/" + m_data.LimitNum;
			m_soldOut.visible = m_data.BuyCount >= m_data.LimitNum;
		}
		stResorceIconInfo resorceIconPath2 = zbResourceHelper.GetResorceIconPath(EResourceType.Money, m_record.moneyType);
		((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resorceIconPath2.strIconPath));
		((TextElement)m_lbCost).text = m_record.price.ToString();
		((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(m_record.price > 0f)));
		((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_record.price != 0f));
	}

	public void OnBuy(ClickEvent evt)
	{
		long num = m_wndParent.m_roomDetail.ExplorerExpire - zbSelf.Instance.timeHandler.GetServerTime();
		if (m_wndParent.m_roomDetail.ExplorerRole != zbSelf.Instance.GetRoleId() && num > 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11274") + ": " + zbUtil.FormatTimeHHMMSS(num));
		}
		else if (m_data.LimitNum > 0 && m_data.BuyCount >= m_data.LimitNum)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10393"));
		}
		else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(m_record.moneyType, (long)m_record.price))
		{
			if (m_record.moneyType == 2)
			{
				zbUIMgr.Instance.ShowVip(2);
			}
			else
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, m_record.moneyType, (long)m_record.price);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.Facility_ShopBuy_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, m_wndParent.m_roomDetail.RoomId, m_data.ShopSlot, 1);
		}
	}

	private void OnShowItemTip(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTip((EResourceType)m_record.resType, m_record.resId, 0L);
	}
}
