using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFriendTiLiWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private readonly List<FriendsDetail> m_sortFriendList = new List<FriendsDetail>();

	public readonly List<FriendsDetail> m_pendingFriendList = new List<FriendsDetail>();

	private Button m_btnTouch;

	private Button m_btnClose;

	public ListView m_listView;

	private Button m_btnGetTiLi;

	private Label m_lbCount;

	private VisualElement m_noListView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/friend/tb_friendItem.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnGetTiLi = UQueryExtensions.Q<Button>(m_RootElement, "btn_getTiLi", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Tili });
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetTiLi, (Action<ClickEvent>)OnGetAllTili, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Friends_Refresh, OnFriendsRefresh);
		AddDark();
	}

	public void OnFriendsRefresh(object obj)
	{
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		int collectCount = zbServerData.sGameData.FriendsInfo.CollectCount;
		((TextElement)m_lbCount).text = string.Format("{0}: {1}", zbUtil.Language("lang.10491"), zbSelf.Instance.friendHandler.Max_Friends_Collect_Number - collectCount);
		RefreshPendingFriends();
		RefreshList();
		m_noListView.visible = m_pendingFriendList == null || m_pendingFriendList.Count <= 0;
		((VisualElement)m_listView).visible = m_pendingFriendList != null && m_pendingFriendList.Count > 0;
	}

	public void RefreshList()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbFriendTiliItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (m_pendingFriendList[index] != null)
			{
				(element as zbFriendTiliItem).Init(m_pendingFriendList[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_pendingFriendList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	public void OnGetAllTili(ClickEvent evt)
	{
		if (m_pendingFriendList == null || m_pendingFriendList.Count <= 0)
		{
			return;
		}
		if (m_pendingFriendList.Exists((FriendsDetail f) => f.ActionMe == EFriendActionCollectState.Pending))
		{
			if (zbServerData.sGameData.FriendsInfo.CollectCount < zbSelf.Instance.friendHandler.Max_Friends_Collect_Number)
			{
				zbWsMsgHandler.Instance.Friends_ActionCollectAll_Req();
				m_pendingFriendList.Clear();
				((BaseVerticalCollectionView)m_listView).RefreshItems();
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10438"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10678"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
			}
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10439"));
		}
	}

	public void RefreshPendingFriends()
	{
		zbSelf.Instance.friendHandler.GetSortFriendsList(m_sortFriendList);
		m_pendingFriendList.Clear();
		foreach (FriendsDetail sortFriend in m_sortFriendList)
		{
			if (sortFriend.ActionMe == EFriendActionCollectState.Pending)
			{
				m_pendingFriendList.Add(sortFriend);
			}
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
