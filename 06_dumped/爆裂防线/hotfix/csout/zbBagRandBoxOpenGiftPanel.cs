using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBagRandBoxOpenGiftPanel
{
	private zbBagRandBoxOpenWnd m_wndParent;

	private VisualElement m_RootElement;

	private zbItem m_item;

	private Label m_lbName;

	private Label m_lbCount;

	private Button m_btnBuy;

	private Label m_lbCost;

	private int m_keyId;

	private int m_groupCount;

	private int m_cost;

	public void OnEnable(zbBagRandBoxOpenWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action<ClickEvent>)OnBuy, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
	}

	public void Refresh(int id, long buyCount)
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		m_keyId = id;
		m_item.Refresh(new zbItemData(EResourceType.Item, id, 0L), isNameVis: false, isCountVis: false);
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Item, id);
		((TextElement)m_lbName).text = resorceIconPath.strName;
		zbItemCsvRecord recordByKey = zbDataConfig.Instance.itemCsvTable.GetRecordByKey(id);
		m_groupCount = (int)Math.Ceiling((double)buyCount / (double)recordByKey.quickBuy[0]);
		m_cost = m_groupCount * recordByKey.quickBuy[1];
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10718") + m_groupCount * recordByKey.quickBuy[0];
		((TextElement)m_lbCost).text = m_cost.ToString();
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(3, m_cost))
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
		if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(3, m_cost))
		{
			zbUIMgr.Instance.BubbleInfo("红钻数量不足");
		}
		else
		{
			zbWsMsgHandler.Instance.Aux_ResourceQuickBuy_Req(EResourceType.Item, m_keyId, m_groupCount);
		}
	}
}
