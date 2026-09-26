using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarRankTabPlayer
{
	private zbLegionWarRankWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_scrollView;

	private VisualElement m_noListView;

	private VisualElement m_mine;

	private VisualElement m_mineNoRankInfo;

	private VisualElement m_minePlayerInfo;

	private VisualElement m_mineIcon;

	private Label m_lbRank;

	private Label m_lbMinePlayerPoint;

	public void OnEnable(zbLegionWarRankWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
		m_mineNoRankInfo = UQueryExtensions.Q<VisualElement>(m_mine, "noRankInfo", (string)null);
		m_minePlayerInfo = UQueryExtensions.Q<VisualElement>(m_mine, "player", (string)null);
		m_mineIcon = UQueryExtensions.Q<VisualElement>(m_mine, "icon", (string)null);
		m_lbMinePlayerPoint = UQueryExtensions.Q<Label>(m_mine, "lb_playerPoint", (string)null);
	}

	public void RefreshCustom()
	{
		RefreshList();
		RefreshSelfInfo();
	}

	private void RefreshList()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_scrollView).Clear();
		LegionWarPlayerRankListAck legionWarPlayerRankListAck = zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck;
		bool flag = legionWarPlayerRankListAck == null || legionWarPlayerRankListAck.RankList.Count == 0;
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (legionWarPlayerRankListAck != null)
		{
			for (int i = 0; i < legionWarPlayerRankListAck.RankList.Count; i++)
			{
				zbLegionWarRankPlayerItem zbLegionWarRankPlayerItem2 = new zbLegionWarRankPlayerItem();
				zbLegionWarRankPlayerItem2.OnEnable(m_wndParent);
				zbLegionWarRankPlayerItem2.Init(legionWarPlayerRankListAck.RankList[i]);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbLegionWarRankPlayerItem2);
			}
		}
	}

	private void RefreshSelfInfo()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		m_mine.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck != null)
		{
			LegionWarPlayerRankDetail legionWarPlayerRankDetail = ((IEnumerable<LegionWarPlayerRankDetail>)zbSelf.Instance.legionWarHandler.legionWarPlayerRankListAck.RankList).FirstOrDefault((LegionWarPlayerRankDetail a) => a.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId());
			m_mineNoRankInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(legionWarPlayerRankDetail != null));
			m_mineIcon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(legionWarPlayerRankDetail == null));
			if (legionWarPlayerRankDetail != null)
			{
				m_mine.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbSelf.Instance.legionWarHandler.InitPlayerInfo(m_minePlayerInfo, legionWarPlayerRankDetail.RoleBaseInfo, legionWarPlayerRankDetail.LegionFlagInfo);
				zbSelf.Instance.rankHandler.InitRank(m_lbRank, legionWarPlayerRankDetail.RankData.Rank);
				((TextElement)m_lbMinePlayerPoint).text = legionWarPlayerRankDetail.RankData.Value.ToString();
			}
			else
			{
				((VisualElement)m_lbRank).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
				((TextElement)m_lbRank).text = zbUtil.Language("lang.10512");
			}
		}
	}
}
