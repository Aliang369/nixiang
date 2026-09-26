using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSubjectTabZhanLing
{
	private zbMechaSubjectWnd m_wndParent;

	private VisualElement m_RootElement;

	private ListView m_listView;

	private ScrollView m_scrollView;

	private Button m_btnGet;

	private Button m_btnActive;

	private zbMechaSubjectZhanLingItem m_grandPrize;

	private List<int> m_grandIndexes;

	private int m_curGrandIndex = -1;

	private List<zbMechaSubjectZhanLingCsvRecord> m_records;

	public void OnEnable(zbMechaSubjectWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "btn_active", (string)null);
		m_grandPrize = new zbMechaSubjectZhanLingItem();
		m_grandPrize = UQueryExtensions.Q<zbMechaSubjectZhanLingItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(m_wndParent);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		InitListView();
	}

	private void InitListView()
	{
		m_records = zbSelf.Instance.mechaSubjectHandler.m_zhanlingRecords;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbMechaSubjectZhanLingItem obj = new zbMechaSubjectZhanLingItem();
			obj.OnEnable(m_wndParent);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbMechaSubjectZhanLingCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbMechaSubjectZhanLingCsvRecord>;
			(element as zbMechaSubjectZhanLingItem).Refresh(list[index]);
		};
		RefreshGrandPrize();
		((BaseVerticalCollectionView)m_listView).itemsSource = m_records;
	}

	public void Refresh()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_data != null)
		{
			((BaseVerticalCollectionView)m_listView).RefreshItems();
			RefreshPreGrandPrizeByScroll(needRefresh: true);
			bool flag = m_wndParent.m_data.ZhanLingInfo.Active == EBoolState.False;
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((TextElement)m_btnActive).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_wndParent.m_data.ZhanLingInfo.GoodsId);
		}
	}

	private void RefreshGrandPrize()
	{
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Count; i++)
		{
			if (m_records[i].isGrand > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
	}

	private void OnGet()
	{
		if (zbSelf.Instance.mechaSubjectHandler.HasZhanLingNotify(m_wndParent.m_data))
		{
			zbWsMsgHandler.Instance.Operate_MechaSubjectZhanLingAward_Req(m_wndParent.m_data.BaseInfo.UUID);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10426"));
		}
	}

	private void OnActive()
	{
		if (m_wndParent.m_data.ZhanLingInfo.Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(m_wndParent.m_data.ZhanLingInfo.GoodsId);
		}
	}

	public void Update()
	{
		RefreshPreGrandPrizeByScroll();
	}

	private void RefreshPreGrandPrizeByScroll(bool needRefresh = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(m_scrollView.scrollOffset.y / ((BaseVerticalCollectionView)m_listView).fixedItemHeight);
		int num2 = -1;
		for (int i = 0; i < m_grandIndexes.Count; i++)
		{
			if (m_grandIndexes[i] >= num + 6)
			{
				num2 = i;
				break;
			}
		}
		if (num2 != -1 && ((num2 != m_curGrandIndex) | needRefresh))
		{
			m_curGrandIndex = num2;
			int index = m_grandIndexes[m_curGrandIndex];
			zbMechaSubjectZhanLingCsvRecord record = m_records[index];
			m_grandPrize.Refresh(record);
			m_grandPrize.SetMaskVisible(visible: false);
		}
	}
}
