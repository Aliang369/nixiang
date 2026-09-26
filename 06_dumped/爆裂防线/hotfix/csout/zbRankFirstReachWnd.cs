using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbRankFirstReachWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Button m_btnTouch;

	private ListView m_listView;

	private Label m_lbDesc;

	private RankFirstReachDetail[] m_rankFirstReachDetails;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/rank/tb_rankFirstReach.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_FirstReachAward, OnUpdateEventFirstReach);
	}

	private void OnUpdateEventFirstReach(object data)
	{
		UpdateListView();
	}

	public void RefreshCustom(RankFirstReachDetail[] rankFirstReachDetails)
	{
		m_rankFirstReachDetails = rankFirstReachDetails;
		UpdateListView();
	}

	private void UpdateListView()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listView).Clear();
		bool flag = m_rankFirstReachDetails.Length != 0;
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		RankFirstReachDetail[] reversedDetails = ((IEnumerable<RankFirstReachDetail>)GetFilteredReachDetails(m_rankFirstReachDetails)).Reverse().ToArray();
		m_listView.makeItem = () => (VisualElement)(object)new zbRankFirstReachItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbRankFirstReachItem).Init(reversedDetails[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = reversedDetails;
	}

	private RankFirstReachDetail[] GetFilteredReachDetails(RankFirstReachDetail[] m_rankFirstReachDetails)
	{
		List<RankFirstReachDetail> list = m_rankFirstReachDetails.Where((RankFirstReachDetail item) => item.RoleBaseInfo != null).ToList();
		List<RankFirstReachDetail> list2 = m_rankFirstReachDetails.Where((RankFirstReachDetail item) => item.RoleBaseInfo == null).ToList();
		List<RankFirstReachDetail> list3 = new List<RankFirstReachDetail>();
		if (list.Count < 8)
		{
			int count = 8 - list.Count;
			list3.AddRange(list);
			list3.AddRange(list2.Take(count));
		}
		else
		{
			list3.AddRange(list);
			if (list2.Count > 0)
			{
				list3.Add(list2[0]);
			}
		}
		return list3.ToArray();
	}

	private void OnClose()
	{
		Hide();
	}
}
