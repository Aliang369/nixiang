using System;
using System.Collections;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFriendTabApply
{
	private zbFriendWnd m_parent;

	private VisualElement m_RootElement;

	private VisualElement m_noListView;

	private ListView m_listView;

	private int m_currTab;

	private Button m_btnAllAgree;

	private Button m_btnAllRefuse;

	private RepeatedField<FriendsApplyDetail> m_applyList;

	public zbFriendTabApply(VisualElement root, zbFriendWnd parent)
	{
		m_parent = parent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_btnAllAgree = UQueryExtensions.Q<Button>(m_RootElement, "btn_allAgree", (string)null);
		m_btnAllRefuse = UQueryExtensions.Q<Button>(m_RootElement, "btn_allRefuse", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).mode = (ScrollViewMode)0;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllAgree, (Action<ClickEvent>)OnAllAgree, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllRefuse, (Action<ClickEvent>)OnAllRefuse, "", (TrickleDown)0);
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Friends_Refresh, RefreshApplyList);
	}

	private void OnAllAgree(ClickEvent evt)
	{
		if (m_applyList.Count > 0)
		{
			zbWsMsgHandler.Instance.Friends_ApplyPassAll_Req();
		}
	}

	private void OnAllRefuse(ClickEvent evt)
	{
		if (m_applyList.Count > 0)
		{
			zbWsMsgHandler.Instance.Friends_ApplyRefuseAll_Req();
		}
	}

	private void RefreshApplyList(object obj)
	{
		RefreshList();
		m_noListView.visible = m_applyList.Count <= 0;
	}

	public void Refresh(int currTab)
	{
		m_currTab = currTab;
		zbWsMsgHandler.Instance.Friends_ApplyList_Req();
	}

	public void RefreshList()
	{
		m_applyList = zbSelf.Instance.friendHandler.applyList;
		m_listView.makeItem = () => (VisualElement)(object)new zbFriendItem(m_parent, m_parent.m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_applyList[index] != null)
			{
				(element as zbFriendItem).Init(m_applyList[index].RoleBaseInfo, m_currTab);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)m_applyList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}
}
