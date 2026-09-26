using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHolidayTaskAward
{
	private zbHolidayTaskWnd m_wndParent;

	private ListView m_listView;

	private ScrollView m_scrollView;

	private zbHolidayTaskAwardItem m_grandPrize;

	private Button m_btnGet;

	private Button m_btnActive;

	private Button m_btnBigGet;

	private VisualElement m_notify;

	private VisualElement m_notifyBig;

	private List<zbHolidayTaskLevelCsvRecord> m_records;

	private List<int> m_grandIndexes;

	private int m_curGrandIndex = -1;

	public void OnEnable(zbHolidayTaskWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_listView = UQueryExtensions.Q<ListView>(root, "ListView", (string)null);
		m_grandPrize = new zbHolidayTaskAwardItem();
		m_grandPrize = UQueryExtensions.Q<zbHolidayTaskAwardItem>(root, "grandPrize", (string)null);
		m_grandPrize.OnEnable(m_wndParent);
		m_btnGet = UQueryExtensions.Q<Button>(root, "btn_get", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(root, "btn_active", (string)null);
		m_btnBigGet = UQueryExtensions.Q<Button>(root, "btn_bigGet", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet);
		m_notifyBig = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnBigGet);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBigGet, (Action)OnGet, "", (TrickleDown)0);
		InitListView();
	}

	public void Refresh()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_data != null)
		{
			((TextElement)m_btnActive).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_wndParent.m_data.GoodsId);
			((VisualElement)m_btnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_data.ActiveState != 0) ? 1 : 0));
			((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_wndParent.m_data.ActiveState != 0) ? 1 : 0));
			((VisualElement)m_btnBigGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_wndParent.m_data.ActiveState != 1));
			m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasLevelGift(m_wndParent.m_data.BaseInfo.UUID)));
			m_notifyBig.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasLevelGift(m_wndParent.m_data.BaseInfo.UUID)));
			RefreshPreGrandPrizeByScroll(needRefresh: true);
			UpdateListView();
		}
	}

	private void InitListView()
	{
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbHolidayTaskAwardItem obj = new zbHolidayTaskAwardItem();
			obj.OnEnable(m_wndParent);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbHolidayTaskLevelCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbHolidayTaskLevelCsvRecord>;
			(element as zbHolidayTaskAwardItem).Refresh(list[index]);
		};
		m_records = zbDataConfig.Instance.holidayTaskLevelCsvTable.GetRecords().records.Values.ToList();
		m_grandIndexes = new List<int>();
		for (int num = 0; num < m_records.Count; num++)
		{
			if (m_records[num].actAward > 0)
			{
				m_grandIndexes.Add(num);
			}
		}
		((BaseVerticalCollectionView)m_listView).itemsSource = m_records;
	}

	private void UpdateListView()
	{
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		if (m_records.Count > 0)
		{
			int num = Math.Min(m_wndParent.m_data.Level, 0);
			((BaseVerticalCollectionView)m_listView).ScrollToItem(num);
		}
	}

	private void OnGet()
	{
		if (zbSelf.Instance.operateHandler.HasLevelGift(m_wndParent.m_data.BaseInfo.UUID))
		{
			zbWsMsgHandler.Instance.Operate_HolidayTaskLevelAward_Req(m_wndParent.m_data.BaseInfo.UUID, m_wndParent.m_data.Level);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10426"));
		}
	}

	private void OnActive()
	{
		if (m_wndParent.m_data != null && m_wndParent.m_data.ActiveState == 0)
		{
			zbChannel_Pay.OnPay(m_wndParent.m_data.GoodsId);
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
			zbHolidayTaskLevelCsvRecord record = m_records[index];
			m_grandPrize.Refresh(record);
			m_grandPrize.SetMaskVisible(visible: false);
		}
	}
}
