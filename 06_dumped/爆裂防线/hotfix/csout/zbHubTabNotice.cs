using System.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbHubTabNotice
{
	private zbHubWnd m_wndParent;

	private VisualElement m_RootElement;

	private ListView m_listView;

	private NoticeListAck m_NoticeListAckData;

	private VisualTreeAsset m_itemTA;

	public void OnEnable(zbHubWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_servernotic.uxml");
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "ListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 82f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
	}

	public void Refresh()
	{
		m_NoticeListAckData = zbLoginUtil.noticeListAckData;
		if (m_NoticeListAckData == null)
		{
			return;
		}
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).visible = m_NoticeListAckData.NoticeList.Count != 0;
		m_listView.makeItem = () => (VisualElement)(object)new zbNoticeItem(m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_NoticeListAckData.NoticeList[index] != null)
			{
				(element as zbNoticeItem).Init(m_wndParent, m_NoticeListAckData.NoticeList[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)m_NoticeListAckData.NoticeList;
	}
}
