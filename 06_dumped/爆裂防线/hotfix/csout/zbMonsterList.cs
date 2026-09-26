using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonsterList
{
	private zbMonsterWnd m_parent;

	private zbEncyclopediaWnd m_wndParent;

	private MultiColumnListView m_listView;

	private List<zbHandbookCsvRecord> m_listItemDatas = new List<zbHandbookCsvRecord>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 3;

	private int rowCount;

	public zbMonsterList(MultiColumnListView listView, zbMonsterWnd parent, zbEncyclopediaWnd wndParent)
	{
		m_parent = parent;
		m_wndParent = wndParent;
		m_listView = listView;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
	}

	public void Refresh()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		switch (m_parent.CurrentTab)
		{
		case 0:
		case 1:
			columnCount = 3;
			break;
		case 2:
			columnCount = 2;
			break;
		default:
			columnCount = 3;
			break;
		}
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit((float)((m_parent.CurrentTab == 2) ? 340 : 226)),
				makeCell = delegate
				{
					bool isBoss = m_parent.CurrentTab == 2;
					return (VisualElement)(object)new zbMonsterItem(m_wndParent, isBoss);
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0082: Unknown result type (might be due to invalid IL or missing references)
					//IL_0070: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						zbMonsterItem obj = element as zbMonsterItem;
						obj?.Refresh(m_listItemDatas[num]);
						obj.IsMonsterUnlocked(m_listItemDatas[num].id, out var _);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
		GenerateData();
	}

	private void GenerateData()
	{
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		zbHandbookCsvTableData records = zbDataConfig.Instance.handbookCsvTable.GetRecords();
		switch (m_parent.CurrentTab)
		{
		case 0:
			foreach (zbHandbookCsvRecord value in records.records.Values)
			{
				if (value.type == 1 && value.isShow > 0)
				{
					m_listItemDatas.Add(value);
				}
			}
			break;
		case 1:
			foreach (zbHandbookCsvRecord value2 in records.records.Values)
			{
				if (value2.type == 2 && value2.isShow > 0)
				{
					m_listItemDatas.Add(value2);
				}
			}
			break;
		case 2:
			foreach (zbHandbookCsvRecord value3 in records.records.Values)
			{
				if (value3.type == 3 && value3.isShow > 0)
				{
					m_listItemDatas.Add(value3);
				}
			}
			break;
		}
		m_listItemDatas.Sort((zbHandbookCsvRecord a, zbHandbookCsvRecord b) => a.index.CompareTo(b.index));
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int num = 0; num < rowCount; num++)
		{
			rowIndexes.Add(num);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	public void ScrollToCanGetReward()
	{
		bool isBoss = m_parent.CurrentTab == 2;
		zbMonsterItem zbMonsterItem2 = new zbMonsterItem(m_wndParent, isBoss);
		for (int i = 0; i < m_listItemDatas.Count; i++)
		{
			zbHandbookCsvRecord zbHandbookCsvRecord2 = m_listItemDatas[i];
			if (!zbMonsterItem2.IsMonsterUnlocked(zbHandbookCsvRecord2.id, out var detail) || detail.Level != 0)
			{
				continue;
			}
			int row = i / columnCount;
			((VisualElement)m_listView).schedule.Execute((Action)delegate
			{
				((VisualElement)m_listView).schedule.Execute((Action)delegate
				{
					((BaseVerticalCollectionView)m_listView).ScrollToItem(row + 1);
				});
			});
			break;
		}
	}
}
