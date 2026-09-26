using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class zbBarRateOperateWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private ListView m_listView;

	private List<zbBarProbabilityCsvRecord> m_listItemDatas = new List<zbBarProbabilityCsvRecord>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "list", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
		InitRecords();
	}

	private void InitRecords()
	{
		zbBarProbabilityCsvRecord[] records = zbDataConfig.Instance.barProbabilityCsvTable.GetRecords().records;
		foreach (zbBarProbabilityCsvRecord zbBarProbabilityCsvRecord2 in records)
		{
			if (zbBarProbabilityCsvRecord2.barType == 10)
			{
				m_listItemDatas.Add(zbBarProbabilityCsvRecord2);
			}
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbBarRateOperateItem();
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_listItemDatas[index] != null)
			{
				(element as zbBarRateOperateItem).Refresh(m_listItemDatas[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
	}

	public override void Refresh()
	{
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
