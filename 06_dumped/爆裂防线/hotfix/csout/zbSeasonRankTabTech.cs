using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonRankTabTech
{
	private zbSeasonRankWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbTime;

	private ListView m_listView;

	private List<RankDetail> m_listItemDatas = new List<RankDetail>();

	private VisualElement m_mine;

	private Label m_mineLbRank;

	private Label m_mineLbScore;

	private RankData m_selfInfo;

	public void OnEnable(zbSeasonRankWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_mineLbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
		m_mineLbScore = UQueryExtensions.Q<Label>(m_mine, "lb_power", (string)null);
		InitListView();
	}

	public void Refresh()
	{
		UpdateListView();
		RefreshSelfInfo();
	}

	private void InitListView()
	{
		m_listView.makeItem = delegate
		{
			zbSeasonRankItemMember obj = new zbSeasonRankItemMember();
			obj.OnEnable(m_wndParent);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbSeasonRankItemMember).Refresh(m_listItemDatas[index]);
		};
	}

	private void UpdateListView()
	{
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
		int num = m_listItemDatas.FindIndex((RankDetail x) => x.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId);
		((BaseVerticalCollectionView)m_listView).ScrollToItem(num);
	}

	private void RefreshSelfInfo()
	{
		RoleBaseInfo roleBaseInfo = zbServerData.sGameData.RoleBaseInfo;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_mine, roleBaseInfo);
		bool flag = m_selfInfo != null;
		if (flag)
		{
			zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, (int)m_selfInfo.Value);
		}
		else
		{
			zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, zbServerData.sGameData.ArenaInfo.Rank);
		}
		((TextElement)m_mineLbScore).text = string.Format(zbUtil.Language("lang.11462"), flag ? m_selfInfo.Value : 0);
	}
}
