using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneComposeCalcWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Button m_btnCompose;

	private MultiColumnListView m_listView;

	private List<zbItemData> m_listItemDatas = new List<zbItemData>();

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 5;

	private bool columnsInited;

	private AwardDetail[] m_awardList;

	private int m_composeIndex;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnCompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_compose", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCompose, (Action<ClickEvent>)OnCompose, "", (TrickleDown)0);
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
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
				width = Length.op_Implicit(128f),
				makeCell = delegate
				{
					zbItem obj = new zbItem();
					obj.OnEnable(this);
					return (VisualElement)(object)obj;
				},
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_006d: Unknown result type (might be due to invalid IL or missing references)
					//IL_005b: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbItem)?.Refresh(m_listItemDatas[num]);
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

	public void RefreshCustom(AwardDetail[] awardlist, int index)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		m_awardList = awardlist;
		m_composeIndex = index;
		m_listItemDatas.Clear();
		AwardDetail[] awardList = m_awardList;
		foreach (AwardDetail awardDetail in awardList)
		{
			m_listItemDatas.Add(new zbItemData(awardDetail.Type, awardDetail.Tid, awardDetail.Count));
		}
		int num = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		rowIndexes.Clear();
		for (int j = 0; j < num; j++)
		{
			rowIndexes.Add(j);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
	}

	private void OnCompose(ClickEvent evt)
	{
		List<BagSelectDetail> list = new List<BagSelectDetail>();
		AwardDetail[] awardList = m_awardList;
		foreach (AwardDetail awardDetail in awardList)
		{
			list.Add(new BagSelectDetail
			{
				Tid = awardDetail.Tid,
				Count = awardDetail.Count
			});
		}
		zbWsMsgHandler.Instance.Bag_GeneCompose_Req(list.ToArray(), m_composeIndex);
		Hide();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
