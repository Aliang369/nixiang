using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionMembersWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnBack;

	private Button m_btnAuth;

	private Label m_lbTitle;

	private Label m_lbMember;

	private Label m_lbContribute;

	private Label m_lbState;

	private Label m_lbOperate;

	private Label m_lbMemberCount;

	private Label m_lbMemberCountVal;

	private ListView m_listView;

	private zbLegionMemberItem m_preItem;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_LegionMemberItem.uxml");
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnAuth = UQueryExtensions.Q<Button>(m_RootElement, "btn_authority", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbMember = UQueryExtensions.Q<Label>(m_RootElement, "lb_member", (string)null);
		m_lbState = UQueryExtensions.Q<Label>(m_RootElement, "lb_state", (string)null);
		m_lbMemberCountVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_memberCountVal", (string)null);
		m_lbContribute = UQueryExtensions.Q<Label>(m_RootElement, "lb_contribute", (string)null);
		m_lbOperate = UQueryExtensions.Q<Label>(m_RootElement, "lb_operate", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuth, (Action<ClickEvent>)OnClickAuth, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Members, OnUpdateEventLegionMembers);
	}

	public void RefreshCustom()
	{
		if (zbSelf.Instance.legionHandler.memberList != null)
		{
			UpdateMembersList(zbSelf.Instance.legionHandler.memberList);
			((TextElement)m_lbMemberCountVal).text = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.MemberCount + "/" + zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.MemberMax;
		}
	}

	private void UpdateMembersList(RepeatedField<LegionMemberDetail> items = null)
	{
		if (items == null || items.Count == 0)
		{
			((VisualElement)m_listView).Clear();
			return;
		}
		List<LegionMemberDetail> itemsSource = (from item in (IEnumerable<LegionMemberDetail>)items
			orderby (int)zbSelf.Instance.legionHandler.GetOfficialById((int)item.RoleBaseInfo.RoleId) descending, (item.RoleLoginInfo.State == ERoleState.Online) ? 1 : 0 descending, zbSelf.Instance.timeHandler.GetServerTime() - item.RoleLoginInfo.LoginTm
			select item).ToList();
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbLegionMemberItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLegionMemberItem).Init(((BaseVerticalCollectionView)m_listView).itemsSource[index] as LegionMemberDetail, index);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = itemsSource;
	}

	private void OnClickAuth(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionAuthority();
	}

	public void OnClickOperate(zbLegionMemberItem item)
	{
		if (m_preItem != null)
		{
			if (item.Index == m_preItem.Index)
			{
				m_preItem.HidePanel();
				m_preItem = null;
				return;
			}
			m_preItem.HidePanel();
		}
		item.ShowPanel();
		m_preItem = item;
	}

	private void OnUpdateEventLegionMembers(object data)
	{
		UpdateMembersList(zbSelf.Instance.legionHandler.memberList);
		((TextElement)m_lbMemberCountVal).text = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.MemberCount + "/" + zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.MemberMax;
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
		if (m_preItem != null)
		{
			m_preItem.HidePanel();
		}
	}
}
