using System;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbShopItemVip : zbViewBaseItem
{
	private zbShopWnd m_wndParent;

	private VisualElement m_spriteBg;

	private SpineVisualElement m_spine;

	private VisualElement m_spriteIcon;

	private VisualElement m_soldOut;

	private Label m_lbName;

	private Label m_lbLimted;

	private Label m_lbCount;

	private Button m_btnItemTip;

	private VisualElement m_iconOff;

	private Button m_btnBuy;

	private Label m_lbCost;

	private Label m_lbFree;

	private VisualElement m_notify;

	private VisualElement m_lock;

	private ShopSlotDetail m_data;

	private EShopType m_shopType;

	private int vipLimmit;

	public zbShopItemVip(zbShopWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/shop/tb_shopvip.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteBg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spine = UQueryExtensions.Q<SpineVisualElement>((VisualElement)(object)this, "spine", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbLimted = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_limited", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_btnItemTip = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_item", (string)null);
		m_soldOut = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "soldOut", (string)null);
		m_iconOff = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_off", (string)null);
		m_lock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "lock", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_buy", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_cost", (string)null);
		m_lbFree = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_free", (string)null);
		m_notify = zbUtil.CreateNotifyTag(m_spriteBg);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action<ClickEvent>)OnBuy, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnItemTip, (Action<ClickEvent>)OnShowItemTip, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(int shopIndex, EShopType shopType)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
		{
			if (shop.ShopType != shopType)
			{
				continue;
			}
			vipLimmit = shop.VipLevel;
			foreach (ShopSlotDetail slot in shop.SlotList)
			{
				if (slot.Index == shopIndex)
				{
					m_data = slot;
					break;
				}
			}
			break;
		}
		m_lock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((vipLimmit <= zbSelf.Instance.GetVipLevel()) ? 1 : 0));
		int offByItemId = zbSelf.Instance.shopHandler.GetOffByItemId(shopIndex, shopType);
		string resName = ((zbSelf.Instance.shopHandler.GetEffectByItemId(shopIndex, shopType) > 0) ? "ui/image/button/btn_260_88_yellow.png" : "ui/image/button/btn_260_88_green.png");
		((VisualElement)m_btnBuy).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(resName));
		bool flag = offByItemId > 0;
		m_iconOff.visible = flag;
		if (flag)
		{
			m_iconOff.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/operate/off/off{offByItemId}.png"));
		}
		m_shopType = shopType;
		string shopVipFrameTex = zbResourceHelper.GetShopVipFrameTex(zbResourceHelper.GetResourceQuality(m_data.SlotItem.Type, m_data.SlotItem.Tid));
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(shopVipFrameTex);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteBg.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteBg.MarkDirtyRepaint();
		}
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_data.SlotItem.Type, m_data.SlotItem.Tid);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteIcon.MarkDirtyRepaint();
		}
		((TextElement)m_lbName).text = resorceIconPath.strName;
		((TextElement)m_lbCount).text = "x" + m_data.SlotItem.Count;
		string text = zbUtil.Language("lang.10123");
		if (m_data.RefreshType == ERefreshType.Weekly)
		{
			text = zbUtil.Language("lang.10124");
		}
		else if (m_data.RefreshType == ERefreshType.Monthly)
		{
			text = zbUtil.Language("lang.10125");
		}
		if (m_data.Limit <= 0)
		{
			((VisualElement)m_lbLimted).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_soldOut.visible = false;
		}
		else
		{
			((VisualElement)m_lbLimted).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLimted).text = text + ":" + (m_data.Limit - m_data.Count) + "/" + m_data.Limit;
			m_soldOut.visible = m_data.Count >= m_data.Limit;
		}
		stResorceIconInfo resorceIconPath2 = zbResourceHelper.GetResorceIconPath(EResourceType.Money, (long)m_data.MoneyType);
		Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath2.strIconPath);
		if ((Object)(object)val3 != (Object)null)
		{
			((VisualElement)m_lbCost).style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
			((VisualElement)m_lbCost).MarkDirtyRepaint();
		}
		((TextElement)m_lbCost).text = m_data.Price.ToString();
		((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.Price <= 0));
		((VisualElement)m_lbFree).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.Price != 0) ? 1 : 0));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.Price != 0 || m_data.Count != 0) ? 1 : 0));
		if (zbSelf.Instance.shopHandler.getWayItem != null && zbSelf.Instance.shopHandler.getWayItem.type == m_data.SlotItem.Type && zbSelf.Instance.shopHandler.getWayItem.id == m_data.SlotItem.Tid)
		{
			((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_spine).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public void OnBuy(ClickEvent evt)
	{
		if (zbSelf.Instance.GetVipLevel() < vipLimmit)
		{
			zbUIMgr.Instance.BubbleInfo($"VIP{vipLimmit}解锁");
		}
		else if (m_data.Limit > 0 && m_data.Count >= m_data.Limit)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10393"));
		}
		else if (m_data.Price == 0)
		{
			zbWsMsgHandler.Instance.Shop_Buy_Req(m_shopType, m_data.Index, 1);
		}
		else
		{
			zbUIMgr.Instance.ShowShopConfirm(m_shopType, m_data);
		}
	}

	private void OnShowItemTip(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowTip(m_data.SlotItem.Type, m_data.SlotItem.Tid, 0L);
	}
}
