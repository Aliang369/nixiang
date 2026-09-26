using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWallSubjectRelicWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTip;

	private ListView m_listView;

	private Button m_btnClose;

	private zbWallSubjectRelicCsvRecord[] m_records;

	private int[][] m_groupList;

	public WallSubjectDetail m_data;

	public HashSet<int> m_activeRelics = new HashSet<int>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		InitRecords();
	}

	private void InitRecords()
	{
		m_records = zbDataConfig.Instance.wallSubjectRelicCsvTable.GetRecords().records;
		m_groupList = GroupByFive(m_records.Select((zbWallSubjectRelicCsvRecord r) => r.id).ToArray());
		RefreshItmes();
	}

	public void RefreshCustom(WallSubjectDetail data)
	{
		if (data == null)
		{
			return;
		}
		m_data = data;
		m_activeRelics.Clear();
		foreach (int relic in data.BarInfo.RelicList)
		{
			m_activeRelics.Add(relic);
		}
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	public int[][] GroupByFive(int[] relicList)
	{
		if (relicList == null)
		{
			return Array.Empty<int[]>();
		}
		int num = relicList.Length;
		int num2 = (num + 4) / 5;
		int[][] array = new int[num2][];
		for (int i = 0; i < num2; i++)
		{
			int num3 = i * 5;
			int num4 = Math.Min(5, num - num3);
			array[i] = new int[num4];
			Array.Copy(relicList, num3, array[i], 0, num4);
		}
		return array;
	}

	private void RefreshItmes()
	{
		m_listView.makeItem = delegate
		{
			zbWallSubjectRelicCell obj = new zbWallSubjectRelicCell();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbWallSubjectRelicCell).Refresh(m_groupList[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_groupList;
	}

	public void OnClose(ClickEvent evt)
	{
		zbSelf.Instance.newTagHandler.RemoveAllNewRelics();
		Hide();
	}
}
