using System;
using UnityEngine.UIElements;

public class zbVipIosReviewShopItem : zbViewBaseItem
{
	private zbVipIosReviewShop m_parent;

	private Label m_lblTitle;

	private Label m_lblGet;

	private Button m_btnBuy;

	private zbVipShopCsvRecord m_data;

	public zbVipIosReviewShopItem(zbVipIosReviewShop parent)
	{
		m_parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/vip/tb_iosreviewshopitem.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lblTitle = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbltitle", (string)null);
		m_lblGet = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lblget", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btnbuy", (string)null);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
	}

	public void Init(zbVipShopCsvRecord data)
	{
		m_data = data;
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(data.referId);
		if (recordByKey != null)
		{
			((TextElement)m_lblTitle).text = recordByKey.goodsName;
			((TextElement)m_btnBuy).text = $"¥{recordByKey.price}";
		}
		((TextElement)m_lblGet).text = $"X {data.voucher}";
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_data != null)
		{
			zbChannel_Pay.OnPay(m_data.goodsId);
		}
	}
}
