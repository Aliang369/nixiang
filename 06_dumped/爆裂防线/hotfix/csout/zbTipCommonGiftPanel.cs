using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipCommonGiftPanel
{
	private zbTipCommonWnd m_wndParent;

	private VisualElement m_RootElement;

	private zbItem m_item;

	private Label m_lbName;

	private Label m_lbCount;

	private Button m_btnBuy;

	private Label m_lbCost;

	private Label m_lbPrice;

	private int m_groupCount;

	private int m_cost;

	private VoucherShopDetail m_commonShopDetail;

	public void OnEnable(zbTipCommonWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_lbPrice = UQueryExtensions.Q<Label>(m_RootElement, "lb_price", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action<ClickEvent>)OnBuy, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
	}

	public void Refresh(long buyCount)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_lbPrice).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_commonShopDetail = null;
		m_item.Refresh(new zbItemData(m_wndParent.m_type, m_wndParent.m_id, 0L), isNameVis: false, isCountVis: false);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_wndParent.m_type, m_wndParent.m_id);
		((TextElement)m_lbName).text = resorceIconPath.strName;
		if (m_wndParent.m_type == EResourceType.Money)
		{
			if (m_wndParent.m_id == 3)
			{
				using (IEnumerator<VoucherShopDetail> enumerator = zbServerData.sGameData.RechargeShopInfo.VoucherShop.GetEnumerator())
				{
					while (enumerator.MoveNext() && (m_commonShopDetail = enumerator.Current).Voucher < buyCount)
					{
					}
				}
				((TextElement)m_lbCount).text = zbUtil.Language("lang.10718") + m_commonShopDetail.Voucher;
				((TextElement)m_lbPrice).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_commonShopDetail.GoodsId);
				((VisualElement)m_lbCost).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				((VisualElement)m_lbPrice).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			else
			{
				zbMoneyCsvRecord recordByKey = zbDataConfig.Instance.moneyCsvTable.GetRecordByKey((int)m_wndParent.m_id);
				m_groupCount = (int)Math.Ceiling((double)buyCount / (double)recordByKey.quickBuy[0]);
				m_cost = m_groupCount * recordByKey.quickBuy[1];
				((TextElement)m_lbCount).text = zbUtil.Language("lang.10718") + m_groupCount * recordByKey.quickBuy[0];
				((TextElement)m_lbCost).text = m_cost.ToString();
			}
		}
		else if (m_wndParent.m_type == EResourceType.Item)
		{
			zbItemCsvRecord recordByKey2 = zbDataConfig.Instance.itemCsvTable.GetRecordByKey((int)m_wndParent.m_id);
			m_groupCount = (int)Math.Ceiling((double)buyCount / (double)recordByKey2.quickBuy[0]);
			m_cost = m_groupCount * recordByKey2.quickBuy[1];
			((TextElement)m_lbCount).text = zbUtil.Language("lang.10718") + m_groupCount * recordByKey2.quickBuy[0];
			((TextElement)m_lbCost).text = m_cost.ToString();
		}
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_cost))
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.red);
		}
		else
		{
			((VisualElement)m_lbCost).style.color = StyleColor.op_Implicit(Color.white);
		}
	}

	public void OnBuy(ClickEvent evt)
	{
		if (m_commonShopDetail != null)
		{
			zbChannel_Pay.OnPay(m_commonShopDetail.GoodsId);
		}
		else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(2, m_cost))
		{
			zbUIMgr.Instance.BubbleInfo("钻石数量不足");
		}
		else
		{
			zbWsMsgHandler.Instance.Aux_ResourceQuickBuy_Req(m_wndParent.m_type, m_wndParent.m_id, m_groupCount);
		}
	}
}
