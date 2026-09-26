using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyOneBarRateWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private zbItem m_item;

	private Button m_btnConfirm;

	private List<zbRateData> m_listItemDatas = new List<zbRateData>();

	private MultiColumnListView m_listView;

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private int rowCount;

	private bool columnsInited;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
		InitColumns();
	}

	private void InitColumns()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (columnsInited)
		{
			return;
		}
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(120f),
				makeCell = () => (VisualElement)(object)new zbJourneyOneBarRateItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbJourneyOneBarRateItem)?.Refresh(m_listItemDatas[num]);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		columnsInited = true;
	}

	public void RefreshCustom(int journeyType = 1)
	{
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		switch (journeyType)
		{
		case 1:
		{
			zbChapterOneBarProbabilityCsvRecord[] records2 = zbDataConfig.Instance.chapterOneBarProbabilityCsvTable.GetRecords().records;
			foreach (zbChapterOneBarProbabilityCsvRecord zbChapterOneBarProbabilityCsvRecord2 in records2)
			{
				zbItemData zbItemData3 = new zbItemData((EResourceType)zbChapterOneBarProbabilityCsvRecord2.type, zbChapterOneBarProbabilityCsvRecord2.id, zbChapterOneBarProbabilityCsvRecord2.count);
				if (zbChapterOneBarProbabilityCsvRecord2.bigPrize == 0)
				{
					zbRateData item2 = new zbRateData(zbItemData3, zbChapterOneBarProbabilityCsvRecord2.probability);
					m_listItemDatas.Add(item2);
				}
				else if (zbChapterOneBarProbabilityCsvRecord2.bigPrize == 1)
				{
					m_item.Refresh(zbItemData3);
				}
			}
			break;
		}
		case 2:
		{
			zbChapterTwoBarProbabilityCsvRecord[] records3 = zbDataConfig.Instance.chapterTwoBarProbabilityCsvTable.GetRecords().records;
			foreach (zbChapterTwoBarProbabilityCsvRecord zbChapterTwoBarProbabilityCsvRecord2 in records3)
			{
				zbItemData zbItemData4 = new zbItemData((EResourceType)zbChapterTwoBarProbabilityCsvRecord2.type, zbChapterTwoBarProbabilityCsvRecord2.id, zbChapterTwoBarProbabilityCsvRecord2.count);
				if (zbChapterTwoBarProbabilityCsvRecord2.bigPrize == 0)
				{
					zbRateData item3 = new zbRateData(zbItemData4, zbChapterTwoBarProbabilityCsvRecord2.probability);
					m_listItemDatas.Add(item3);
				}
				else if (zbChapterTwoBarProbabilityCsvRecord2.bigPrize == 1)
				{
					m_item.Refresh(zbItemData4);
				}
			}
			break;
		}
		case 3:
		{
			zbChapterThreeBarProbabilityCsvRecord[] records = zbDataConfig.Instance.chapterThreeBarProbabilityCsvTable.GetRecords().records;
			foreach (zbChapterThreeBarProbabilityCsvRecord zbChapterThreeBarProbabilityCsvRecord2 in records)
			{
				zbItemData zbItemData2 = new zbItemData((EResourceType)zbChapterThreeBarProbabilityCsvRecord2.type, zbChapterThreeBarProbabilityCsvRecord2.id, zbChapterThreeBarProbabilityCsvRecord2.count);
				if (zbChapterThreeBarProbabilityCsvRecord2.bigPrize == 0)
				{
					zbRateData item = new zbRateData(zbItemData2, zbChapterThreeBarProbabilityCsvRecord2.probability);
					m_listItemDatas.Add(item);
				}
				else if (zbChapterThreeBarProbabilityCsvRecord2.bigPrize == 1)
				{
					m_item.Refresh(zbItemData2);
				}
			}
			break;
		}
		}
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int j = 0; j < rowCount; j++)
		{
			rowIndexes.Add(j);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
