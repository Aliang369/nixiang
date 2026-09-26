using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class zbVipIosReviewShop : zbBaseWnd
{
	private ScrollView m_itemList;

	private List<zbVipShopCsvRecord> m_shopList = new List<zbVipShopCsvRecord>();

	private Button m_BtnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemList = UQueryExtensions.Q<ScrollView>(m_RootElement, "itemlist", (string)null);
		m_BtnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_BtnClose, (Action)Hide, "", (TrickleDown)0);
	}

	public override void Refresh()
	{
		base.Refresh();
		((VisualElement)m_itemList).Clear();
		m_shopList.Clear();
		foreach (KeyValuePair<int, zbVipShopCsvRecord> record in zbDataConfig.Instance.vipShopCsvTable.GetRecords().records)
		{
			zbVipShopCsvRecord value = record.Value;
			if (value.goodsType == 2 && value.goodsSubType == 6)
			{
				m_shopList.Add(record.Value);
			}
		}
		for (int i = 0; i < m_shopList.Count; i++)
		{
			zbVipIosReviewShopItem zbVipIosReviewShopItem2 = new zbVipIosReviewShopItem(this);
			zbVipIosReviewShopItem2.Init(m_shopList[i]);
			((VisualElement)m_itemList).Add((VisualElement)(object)zbVipIosReviewShopItem2);
		}
	}
}
