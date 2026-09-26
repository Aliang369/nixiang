using System;
using UnityEngine.UIElements;

public class zbWorldReconBuyWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private Button m_btnBuy;

	private Label m_lbBuyDesc;

	private int m_giftGoodId = 310151;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbBuyDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_BuyDesc", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Activity, OnEventUpdate);
	}

	public override void Refresh()
	{
		string format = zbUtil.Language("lang.10164");
		string arg = zbUtil.AddHighlightStyle(zbUtil.Language("lang.10165"), "E2000E", 30);
		string text = string.Format(format, arg);
		((TextElement)m_btnBuy).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_giftGoodId);
		((TextElement)m_lbBuyDesc).text = text;
	}

	private void OnBuy()
	{
		zbChannel_Pay.OnPay(m_giftGoodId);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		OnClose();
	}
}
