using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

internal class zbLimitTimeShopItem : zbViewBaseItem
{
	private VisualElement m_bg;

	private zbItem m_item;

	private VisualElement m_off;

	private Label m_lbName;

	private Label m_lbCount;

	private Label m_lbDiamond;

	private Button m_btnTouch;

	private TextElement m_lbButton;

	private LimitTimeShopItemDetail m_data;

	private string m_uuid;

	public zbLimitTimeShopItem(VisualTreeAsset tAssets, zbLimitTimeShopWnd wndParent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(wndParent);
		m_off = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "off", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_diamond", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lbButton = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_btnTouch, (string)null, (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(LimitTimeShopItemDetail data, string uuid)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_uuid = uuid;
		m_item.Refresh(new zbItemData(m_data.AwardDetail.Type, m_data.AwardDetail.Tid, m_data.AwardDetail.Count));
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_data.AwardDetail.Type, m_data.AwardDetail.Tid);
		((TextElement)m_lbName).text = resorceIconPath.strName;
		((TextElement)m_lbCount).text = m_data.LimitNum - m_data.Count + "/" + m_data.LimitNum;
		((VisualElement)m_lbCount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.LimitNum == 0));
		if (m_data.Discount > 0)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/off/off" + m_data.Discount + ".png");
			m_off.style.backgroundImage = new StyleBackground(val);
			m_off.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_off.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		((TextElement)m_lbDiamond).text = m_data.SalePrice.ToString();
		if (m_data.LimitNum == 0 || m_data.Count < m_data.LimitNum)
		{
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
			m_lbButton.text = zbUtil.Language("lang.10277");
		}
		else
		{
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_180_80_green.png"));
			m_lbButton.text = zbUtil.Language("lang.10278");
		}
	}

	private void OnTouch()
	{
		if (m_data == null)
		{
			return;
		}
		if (m_data.LimitNum == 0 || m_data.LimitNum > m_data.Count)
		{
			if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_data.SalePrice))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 2L, m_data.SalePrice);
			}
			else
			{
				zbWsMsgHandler.Instance.Operate_LimitTimeShop_Req(m_uuid, m_data.Index, 1);
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10278"));
		}
	}
}
