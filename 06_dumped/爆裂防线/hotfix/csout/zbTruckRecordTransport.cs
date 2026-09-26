using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTruckRecordTransport
{
	private VisualTreeAsset m_itemTA;

	public zbTruckRecordWnd m_wndParent;

	private VisualElement m_rootElement;

	private ListView m_listView;

	private VisualElement m_NoListView;

	public void OnEnable(zbTruckRecordWnd wndParent, VisualElement root)
	{
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/truck/tb_truckTransportRecord.uxml");
		m_wndParent = wndParent;
		m_rootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_rootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_NoListView = UQueryExtensions.Q<VisualElement>(m_rootElement, "list_no", (string)null);
	}

	public void Refresh()
	{
		UpdateListView();
	}

	private void UpdateListView()
	{
		if (zbSelf.Instance.truckHandler.TruckTransportInfoList != null)
		{
			TruckTransportInfo[] items = zbSelf.Instance.truckHandler.TruckTransportInfoList;
			bool flag = items.Length != 0;
			((VisualElement)m_listView).Clear();
			((VisualElement)m_listView).visible = flag;
			m_NoListView.visible = !flag;
			m_listView.makeItem = () => (VisualElement)(object)new zbTruckRecordTransportItem(m_itemTA, m_wndParent);
			m_listView.bindItem = delegate(VisualElement element, int index)
			{
				(element as zbTruckRecordTransportItem).Init(items[index]);
			};
			((BaseVerticalCollectionView)m_listView).itemsSource = null;
			((BaseVerticalCollectionView)m_listView).itemsSource = items;
			ScrollToTop();
		}
	}

	private void ScrollToTop()
	{
		ScrollView scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		if (scrollView != null)
		{
			((VisualElement)m_listView).schedule.Execute((Action)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				scrollView.scrollOffset = Vector2.zero;
			}).ExecuteLater(1L);
		}
	}

	public void OnCheck(int index)
	{
	}
}
