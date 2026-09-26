using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbMainQuestJoinFriend
{
	private readonly List<FriendsDetail> m_sortFriendList = new List<FriendsDetail>();

	private zbMainQuestJoinWnd m_parent;

	private VisualElement m_RootElement;

	private VisualElement m_listNo;

	private ListView m_listView;

	private int m_currTab;

	public zbMainQuestJoinFriend(VisualElement root, zbMainQuestJoinWnd parent)
	{
		m_parent = parent;
		m_RootElement = root;
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
	}

	public void Refresh(int currTab)
	{
		m_currTab = currTab;
		RefreshList();
	}

	public void RefreshList()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		m_sortFriendList.Clear();
		zbSelf.Instance.friendHandler.GetSortFriendsList(m_sortFriendList);
		m_sortFriendList.Sort((FriendsDetail a, FriendsDetail b) => (b?.RoleBaseInfo?.TeamAttack).GetValueOrDefault().CompareTo((a?.RoleBaseInfo?.TeamAttack).GetValueOrDefault()));
		m_listView.makeItem = () => (VisualElement)(object)new zbMainQuestJoinItem(m_parent, m_parent.m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbMainQuestJoinItem zbMainQuestJoinItem2 = (zbMainQuestJoinItem)(object)element;
			FriendsDetail friendsDetail = m_sortFriendList[index];
			if (friendsDetail?.RoleBaseInfo != null)
			{
				zbMainQuestJoinItem2.Init(friendsDetail.RoleBaseInfo);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_sortFriendList;
		((BaseVerticalCollectionView)m_listView).Rebuild();
		m_listNo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_sortFriendList.Count > 0));
	}
}
