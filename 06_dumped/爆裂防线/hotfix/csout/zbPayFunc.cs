using System;
using UnityEngine.UIElements;

public class zbPayFunc : zbBaseWnd
{
	private int m_goodsId;

	private Label m_lbTitle;

	private Label m_lblVoucherPrice;

	private Label m_lblPrice;

	private Label m_lbVoucher;

	private Label m_lbMoney;

	private VisualElement m_voucherbg;

	private VisualElement m_moneybg;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lbltitle", (string)null);
		m_voucherbg = UQueryExtensions.Q<VisualElement>(m_RootElement, "voucher", (string)null);
		m_lbVoucher = UQueryExtensions.Q<Label>(m_RootElement, "lb_voucher", (string)null);
		m_lblVoucherPrice = UQueryExtensions.Q<Label>(m_voucherbg, "voucher_price", (string)null);
		m_lbMoney = UQueryExtensions.Q<Label>(m_RootElement, "lb_money", (string)null);
		m_moneybg = UQueryExtensions.Q<VisualElement>(m_RootElement, "money", (string)null);
		m_lblPrice = UQueryExtensions.Q<Label>(m_moneybg, "price", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_voucherbg, (Action)PayWithVoucher, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_moneybg, (Action)PayWithMoney, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(int goodsId)
	{
		m_goodsId = goodsId;
		((TextElement)m_lblPrice).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(goodsId);
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(goodsId);
		if (recordByKey == null)
		{
			return;
		}
		int voucherPrice = recordByKey.voucherPrice;
		if (recordByKey.referId != 0)
		{
			zbVipShopCsvRecord recordByKey2 = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(recordByKey.referId);
			if (recordByKey2 != null)
			{
				voucherPrice = recordByKey2.voucherPrice;
			}
		}
		((TextElement)m_lblVoucherPrice).text = $"{voucherPrice}";
	}

	public void PayWithVoucher()
	{
		zbWsMsgHandler.Instance.Pay_VoucherConsume_Req(m_goodsId);
		Hide();
	}

	public void PayWithMoney()
	{
		zbChannel_Pay.OnPayWithMoney(m_goodsId);
		Hide();
	}
}
