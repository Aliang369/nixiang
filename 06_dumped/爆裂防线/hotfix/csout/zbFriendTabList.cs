using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFriendTabList
{
	private readonly List<FriendsDetail> m_sortFriendList;

	private Label m_lbFirendCount;

	private zbFriendWnd m_parent;

	private VisualElement m_RootElement;

	private ListView m_listView;

	private int m_currTab;

	private Button m_btnSendAll;

	private Button m_btnGetTiLi;

	private readonly VisualElement m_noListView;

	private VisualElement m_notify;

	public zbFriendTabList(VisualElement root, zbFriendWnd parent)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		m_sortFriendList = new List<FriendsDetail>();
		base._002Ector();
		m_parent = parent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_lbFirendCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_firendCount", (string)null);
		m_btnSendAll = UQueryExtensions.Q<Button>(m_RootElement, "btn_sendAll", (string)null);
		m_btnGetTiLi = UQueryExtensions.Q<Button>(m_RootElement, "btn_getTiLi", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGetTiLi, (Vector2?)new Vector2(0f, 0f), 1f, true);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSendAll, (Action<ClickEvent>)OnSendAll, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetTiLi, (Action)OnShowTiLiWnd, "", (TrickleDown)0);
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Friends_Refresh, OnFriendsRefresh);
	}

	public void OnFriendsRefresh(object obj)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		RefreshList();
		((TextElement)m_lbFirendCount).text = zbUtil.Language("lang.10127") + ": " + m_sortFriendList.Count + "/" + zbSelf.Instance.friendHandler.Max_Friends_Number;
		m_noListView.visible = m_sortFriendList.Count <= 0;
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.friendHandler.HasFriendTiliRedPoint()));
	}

	public void OnSendAll(ClickEvent evt)
	{
		if (m_sortFriendList == null || m_sortFriendList.Count <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10492"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
			return;
		}
		bool flag = false;
		foreach (FriendsDetail sortFriend in m_sortFriendList)
		{
			if (!flag && sortFriend.ActionHim == EBoolState.False)
			{
				flag = true;
			}
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			zbWsMsgHandler.Instance.Friends_ActionGiveAll_Req();
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10437"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
		}
		if (!flag)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10439"));
		}
	}

	private void OnShowTiLiWnd()
	{
		zbUIMgr.Instance.ShowFriendsTili();
	}

	public void Refresh(int currTab)
	{
		m_currTab = currTab;
		RefreshList();
	}

	public void RefreshList()
	{
		zbSelf.Instance.friendHandler.GetSortFriendsList(m_sortFriendList);
		m_listView.makeItem = () => (VisualElement)(object)new zbFriendItem(m_parent, m_parent.m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_sortFriendList[index] != null)
			{
				(element as zbFriendItem).Init(m_sortFriendList[index].RoleBaseInfo, m_currTab);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_sortFriendList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}
}
