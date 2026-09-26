using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBarRateNormalWnd : zbBaseWnd
{
	public VisualTreeAsset m_TreeAsset;

	private List<zbBarProbabilityCsvRecord> m_listItemDatas = new List<zbBarProbabilityCsvRecord>();

	private Label m_lbTitle;

	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Button m_btnRule;

	private ListView m_listView;

	private List<zbBarRateNormalItem> m_ListViewDatas = new List<zbBarRateNormalItem>();

	public EBarType m_barType;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action<ClickEvent>)OnRule, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		m_listItemDatas.Clear();
		((VisualElement)m_listView).Clear();
		if ((Object)(object)m_TreeAsset == (Object)null)
		{
			return;
		}
		zbBarProbabilityCsvRecord[] records = zbDataConfig.Instance.barProbabilityCsvTable.GetRecords().records;
		foreach (zbBarProbabilityCsvRecord zbBarProbabilityCsvRecord2 in records)
		{
			if (zbBarProbabilityCsvRecord2.barType == 1)
			{
				m_listItemDatas.Add(zbBarProbabilityCsvRecord2);
			}
		}
		m_TreeAsset.Instantiate();
		m_listView.makeItem = () => (VisualElement)(object)new zbBarRateNormalItem(m_TreeAsset);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_listItemDatas[index] != null)
			{
				(element as zbBarRateNormalItem).Refresh(m_listItemDatas[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnRule(ClickEvent evt)
	{
	}
}
