using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBoxOpenWnd : zbBaseWnd
{
	private List<AwardDetail> m_listItemDatas = new List<AwardDetail>();

	private MultiColumnListView m_listView;

	private List<int> rowIndexes = new List<int>();

	private int columnCount = 3;

	private int rowCount;

	private Label m_lbTitle;

	private Label m_lbBatchLimit;

	private Button m_btnConfirm;

	private Button m_btnClose;

	private Button m_btnTouch;

	private AwardDetail[] m_awardList;

	private int m_boxTid;

	private List<SelectDetail> m_selectList = new List<SelectDetail>();

	private long m_haveCount;

	private int m_batchLimit;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbBatchLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_batchLimit", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Box, OnEventUpdate);
		InitColumns();
	}

	private void InitColumns()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		m_listView.columns.Clear();
		for (int i = 0; i < columnCount; i++)
		{
			int columnIndex = i;
			m_listView.columns.Add(new Column
			{
				width = Length.op_Implicit(200f),
				makeCell = () => (VisualElement)(object)new zbBoxOpenItem(this),
				bindCell = delegate(VisualElement element, int row)
				{
					//IL_0072: Unknown result type (might be due to invalid IL or missing references)
					//IL_0060: Unknown result type (might be due to invalid IL or missing references)
					int num = row * columnCount + columnIndex;
					if (num < m_listItemDatas.Count)
					{
						(element as zbBoxOpenItem)?.Refresh(m_listItemDatas[num], num + 1, m_batchLimit);
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
					}
					else
					{
						element.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
					}
				}
			});
		}
	}

	public void RefreshCustom(AwardDetail[] awardList, int boxTid)
	{
		m_awardList = awardList;
		m_boxTid = boxTid;
		m_selectList.Clear();
		m_haveCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Box, boxTid);
		((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11035"), GetSelectCount(), m_haveCount);
		zbBoxCsvRecord recordByKey = zbDataConfig.Instance.boxCsvTable.GetRecordByKey(boxTid);
		if (recordByKey != null)
		{
			((TextElement)m_lbBatchLimit).text = string.Format(zbUtil.Language("lang.11036"), recordByKey.batchLimit);
			m_batchLimit = recordByKey.batchLimit;
		}
		else
		{
			((TextElement)m_lbBatchLimit).text = "";
			m_batchLimit = (int)m_haveCount;
		}
		RefreshListView();
	}

	public void RefreshListView()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		m_listItemDatas.Clear();
		AwardDetail[] awardList = m_awardList;
		foreach (AwardDetail item in awardList)
		{
			m_listItemDatas.Add(item);
		}
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_listItemDatas.Count <= 0));
		rowIndexes.Clear();
		rowCount = Mathf.CeilToInt((float)m_listItemDatas.Count / (float)columnCount);
		for (int j = 0; j < rowCount; j++)
		{
			rowIndexes.Add(j);
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_listView, "unity-multi-column-view__header-container", (string)null).style.opacity = StyleFloat.op_Implicit(0f);
		((VisualElement)m_listView).style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	public void RefreshSelectList(SelectDetail select)
	{
		bool flag = false;
		foreach (SelectDetail select2 in m_selectList)
		{
			if (select.Index == select2.Index)
			{
				flag = true;
				select2.Count = select.Count;
				break;
			}
		}
		if (!flag && select.Count > 0)
		{
			m_selectList.Add(select);
		}
		((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11035"), GetSelectCount(), m_haveCount);
	}

	public int GetSelectCount()
	{
		int num = 0;
		foreach (SelectDetail select in m_selectList)
		{
			num += select.Count;
		}
		return num;
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public void OnConfirm()
	{
		if (m_selectList.Count <= 0 || !CheckSelectListHasCount())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10237"));
		}
		else
		{
			zbWsMsgHandler.Instance.Bag_SelectBoxOpen_Req(m_boxTid, m_selectList.ToArray());
		}
	}

	private bool CheckSelectListHasCount()
	{
		foreach (SelectDetail select in m_selectList)
		{
			if (select.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	private void OnEventUpdate(object data)
	{
		Hide();
	}
}
