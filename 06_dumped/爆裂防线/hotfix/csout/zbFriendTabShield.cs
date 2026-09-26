using System.Collections;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFriendTabShield
{
	private zbFriendWnd m_parent;

	private VisualElement m_RootElement;

	private VisualElement m_noListView;

	private ListView m_listView;

	private int m_currTab;

	private RepeatedField<RoleBaseInfo> m_shielding;

	public zbFriendTabShield(VisualElement root, zbFriendWnd parent)
	{
		m_parent = parent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Friends_Refresh, RefreshShielList);
	}

	private void RefreshShielList(object obj)
	{
		RefreshList();
		m_noListView.visible = m_shielding.Count <= 0;
	}

	public void Refresh(int currTab)
	{
		m_currTab = currTab;
		zbWsMsgHandler.Instance.Friends_ApplyList_Req();
	}

	public void RefreshList()
	{
		m_shielding = zbSelf.Instance.chatHandler.shielding;
		m_listView.makeItem = () => (VisualElement)(object)new zbFriendItem(m_parent, m_parent.m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_shielding[index] != null)
			{
				(element as zbFriendItem).Init(m_shielding[index], m_currTab);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)m_shielding;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}
}
