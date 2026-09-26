using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRankTabArena
{
	private zbRankWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_scrollView;

	private VisualElement m_noListView;

	private VisualElement m_mine;

	private VisualElement m_mineNoRankInfo;

	private Label m_mineLbRank;

	private RankDetail[] m_rankList;

	private RankData m_selfInfo;

	public void OnEnable(zbRankWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_mineNoRankInfo = UQueryExtensions.Q<VisualElement>(m_mine, "noRankInfo", (string)null);
		m_mineLbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
	}

	public void Refresh()
	{
		RefreshList(zbSelf.Instance.rankHandler.arenaRankListAck);
	}

	private void RefreshList(ArenaRankListAck rankListAck)
	{
		if (rankListAck?.RankList == null)
		{
			zbWsMsgHandler.Instance.Arena_RankList_Req(openWnd: true);
			m_wndParent.RefreshTops(Array.Empty<RoleBaseInfo>());
			m_rankList = Array.Empty<RankDetail>();
			m_selfInfo = null;
			UpdateListView();
			RefreshSelfInfo();
			return;
		}
		List<RoleBaseInfo> list = new List<RoleBaseInfo>(3);
		int num = Mathf.Min(3, rankListAck.RankList.Count);
		for (int i = 0; i < num; i++)
		{
			list.Add(rankListAck.RankList[i].RoleBaseInfo);
		}
		m_wndParent.RefreshTops(list.ToArray());
		m_rankList = ((IEnumerable<RankDetail>)rankListAck.RankList).ToArray();
		m_selfInfo = rankListAck.SelfRank;
		UpdateListView();
		RefreshSelfInfo();
	}

	private void RefreshSelfInfo()
	{
		RoleBaseInfo roleBaseInfo = zbServerData.sGameData.RoleBaseInfo;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_mine, roleBaseInfo);
		bool flag = m_selfInfo != null;
		m_mineNoRankInfo.visible = !flag;
		if (flag)
		{
			zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, (int)m_selfInfo.Value);
		}
		else
		{
			zbSelf.Instance.rankHandler.InitRank(m_mineLbRank, zbServerData.sGameData.ArenaInfo.Rank);
		}
	}

	private void UpdateListView()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_scrollView).Clear();
		bool flag = m_rankList.Length != 0;
		((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		RankDetail[] rankList = m_rankList;
		foreach (RankDetail rankDetail in rankList)
		{
			zbArenaRankItem zbArenaRankItem2 = new zbArenaRankItem();
			zbArenaRankItem2.OnEnable(m_wndParent);
			zbArenaRankItem2.Refresh(rankDetail);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbArenaRankItem2);
		}
	}

	public void BeforeClose()
	{
		((VisualElement)m_scrollView).schedule.Execute((Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			m_scrollView.scrollOffset = Vector2.zero;
		}).StartingIn(1L);
		zbSelf.Instance.rankHandler.arenaRankListAck = null;
	}
}
