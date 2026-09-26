using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFriendTabAdd
{
	private RepeatedField<FriendsCommonDetail> m_recommendList;

	private readonly Button m_btnAllRefresh;

	public readonly Button m_btnAllApply;

	private readonly zbFriendWnd m_parent;

	private readonly VisualElement m_RootElement;

	private readonly ListView m_listView;

	private int m_currTab;

	private readonly VisualElement m_noListView;

	private TextField m_InputPlayerID;

	private Label m_lbPlaceHolder;

	private Button m_btnSearchPlayer;

	private Button m_btnReset;

	private zbDebouncedPnl m_inputTouch;

	private List<Button> m_btns;

	private DateTime m_lastApplyTime;

	public zbFriendTabAdd(VisualElement root, zbFriendWnd parent)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		m_lastApplyTime = DateTime.MinValue;
		base._002Ector();
		m_parent = parent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnAllRefresh = UQueryExtensions.Q<Button>(m_RootElement, "btn_allRefresh", (string)null);
		m_btnAllApply = UQueryExtensions.Q<Button>(m_RootElement, "btn_allApply", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "inputPanel", (string)null);
		m_InputPlayerID = UQueryExtensions.Q<TextField>(val, "input_playerID", (string)null);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(val, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(m_parent, OnInputClick);
		m_lbPlaceHolder = new Label();
		((VisualElement)m_lbPlaceHolder).name = "custom-placeholder";
		((VisualElement)m_lbPlaceHolder).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)m_lbPlaceHolder).style.left = StyleLength.op_Implicit(8f);
		((VisualElement)m_lbPlaceHolder).style.top = StyleLength.op_Implicit(2f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)m_lbPlaceHolder);
		((VisualElement)m_lbPlaceHolder).pickingMode = (PickingMode)1;
		((TextElement)m_lbPlaceHolder).text = zbUtil.Language("lang.10497");
		((VisualElement)m_InputPlayerID).Add((VisualElement)(object)m_lbPlaceHolder);
		m_btnSearchPlayer = UQueryExtensions.Q<Button>(val, "btn_searchPlayer", (string)null);
		m_btnReset = UQueryExtensions.Q<Button>(val, "btn_reset", (string)null);
		m_btns = new List<Button> { m_btnAllRefresh, m_btnAllApply };
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_parent.m_uiEvent.RegisterCallback<FocusEvent>((VisualElement)(object)m_InputPlayerID, (Action)OnFocus, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<BlurEvent>((VisualElement)(object)m_InputPlayerID, (Action)OnBlur, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllRefresh, (Action<ClickEvent>)OnAllRefresh, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllApply, (Action<ClickEvent>)OnAllApply, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSearchPlayer, (Action<ClickEvent>)OnSearch, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReset, (Action<ClickEvent>)OnReset, "", (TrickleDown)0);
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Friends_Refresh, OnRefreshFriendRecommendList);
		m_parent.m_gameEevent.RegisterEvent(EGameEvent.Event_Friends_Search, OnRefreshFriendSearchList);
	}

	private void OnInputClick()
	{
		((Focusable)m_InputPlayerID).Focus();
	}

	private void OnBlur()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseField<string>)(object)m_InputPlayerID).value == "")
		{
			((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private void OnFocus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnReset(ClickEvent evt)
	{
		Refresh(m_currTab);
	}

	private void OnAllRefresh(ClickEvent evt)
	{
		Refresh(m_currTab);
	}

	private void OnAllApply(ClickEvent evt)
	{
		if (m_lastApplyTime != DateTime.MinValue)
		{
			double num = 60.0 - (DateTime.Now - m_lastApplyTime).TotalSeconds;
			if (num > 0.0)
			{
				string msg = string.Format(zbUtil.Language("lang.10608"), Math.Ceiling(num));
				zbUIMgr.Instance.BubbleInfo(msg);
				return;
			}
		}
		if (m_recommendList == null || m_recommendList.Count == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10126"));
			return;
		}
		List<int> list = new List<int>();
		foreach (FriendsCommonDetail recommend in m_recommendList)
		{
			int item = (int)recommend.RoleBaseInfo.RoleId;
			if (!zbFriendHandler.AppliedPlayerIds.Contains(item))
			{
				list.Add(item);
				zbFriendHandler.AppliedPlayerIds.Add(item);
			}
		}
		if (list.Count == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10609"));
			return;
		}
		zbWsMsgHandler.Instance.Friends_ApplyBatch_Req(list.ToArray());
		m_lastApplyTime = DateTime.Now;
	}

	public void OnRefreshFriendRecommendList(object obj)
	{
		m_recommendList = zbSelf.Instance.friendHandler.recommendList;
		RefreshList(m_recommendList);
		m_noListView.visible = m_recommendList.Count <= 0;
	}

	public void OnRefreshFriendSearchList(object obj)
	{
		RefreshList(zbSelf.Instance.friendHandler.searchList);
		m_noListView.visible = false;
	}

	public void Refresh(int currTab)
	{
		m_currTab = currTab;
		zbWsMsgHandler.Instance.Friends_Recommend_Req();
	}

	public void RefreshList(RepeatedField<RoleBaseInfo> _roleBaseInfoList)
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbFriendItem(m_parent, m_parent.m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (index >= 0 && index < _roleBaseInfoList.Count)
			{
				RoleBaseInfo roleBaseInfo = _roleBaseInfoList[index];
				if (roleBaseInfo != null)
				{
					(element as zbFriendItem).Init(roleBaseInfo, m_currTab);
				}
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)_roleBaseInfoList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	public void RefreshList(RepeatedField<FriendsCommonDetail> recommendList)
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbFriendItem(m_parent, m_parent.m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (index >= 0 && index < recommendList.Count)
			{
				FriendsCommonDetail friendsCommonDetail = recommendList[index];
				if (friendsCommonDetail != null)
				{
					(element as zbFriendItem).Init(isApplied: zbFriendHandler.AppliedPlayerIds.Contains((int)friendsCommonDetail.RoleBaseInfo.RoleId), info: friendsCommonDetail.RoleBaseInfo, currTab: m_currTab, loginInfo: friendsCommonDetail.RoleLoginInfo);
				}
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)recommendList;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	private void OnSearch(ClickEvent evt)
	{
		string text = ((BaseField<string>)(object)m_InputPlayerID).value.Trim();
		if (!(text == ""))
		{
			if (!zbCharChecker.IsSafeChatMessage(text))
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10074"));
			}
			else
			{
				zbWsMsgHandler.Instance.Friends_Search_Req(text);
			}
		}
	}
}
