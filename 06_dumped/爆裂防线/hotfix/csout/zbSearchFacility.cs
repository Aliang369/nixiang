using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSearchFacility
{
	private zbFacilityHubWnd m_wndParent;

	private VisualElement m_root;

	private ListView m_listView;

	private VisualElement m_noListView;

	private List<zbFacilityItem> m_facilityItems = new List<zbFacilityItem>();

	public void OnEnable(zbFacilityHubWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_root = root;
		m_listView = UQueryExtensions.Q<ListView>(m_root, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_root, "noListView", (string)null);
		InitListView();
	}

	public void Refresh()
	{
		UpdateListView();
		zbSelf.Instance.facilityHandler.isFirstRecommend = false;
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbFacilityItem(m_wndParent);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (((BaseVerticalCollectionView)m_listView).itemsSource is FacilityBaseInfo[] array && index >= 0 && index < array.Length)
			{
				zbFacilityItem zbFacilityItem2 = (zbFacilityItem)(object)element;
				m_facilityItems.Add(zbFacilityItem2);
				zbFacilityItem2.Refresh(array[index]);
			}
		};
		m_listView.unbindItem = delegate(VisualElement element, int index)
		{
			zbFacilityItem item = (zbFacilityItem)(object)element;
			m_facilityItems.Remove(item);
		};
	}

	private void UpdateListView()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		m_facilityItems.Clear();
		FacilityBaseInfo[] array = zbSelf.Instance.facilityHandler.recommendList.OrderBy((FacilityBaseInfo kv) => kv.ExpireTm).ToArray();
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((array.Length != 0) ? 1 : 0));
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(array.Length == 0));
		if (array.Length != 0)
		{
			((BaseVerticalCollectionView)m_listView).itemsSource = array;
		}
	}

	public void UpdatePerSecond()
	{
		foreach (zbFacilityItem facilityItem in m_facilityItems)
		{
			facilityItem.RefreshLeftTime();
		}
	}
}
