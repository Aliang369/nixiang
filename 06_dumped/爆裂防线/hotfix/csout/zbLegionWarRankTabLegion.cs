using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarRankTabLegion
{
	private zbLegionWarRankWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_scrollView;

	private VisualElement m_mine;

	private VisualElement m_mineNoRankInfo;

	private VisualElement m_mineIcon;

	private VisualElement m_mineLegionInfo;

	private Label m_lbMineLegionPoint;

	private VisualElement m_noListView;

	private Label m_lbRank;

	public void OnEnable(zbLegionWarRankWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_mine = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_mineNoRankInfo = UQueryExtensions.Q<VisualElement>(m_RootElement, "noRankInfo", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_mineIcon = UQueryExtensions.Q<VisualElement>(m_mine, "icon", (string)null);
		m_mineLegionInfo = UQueryExtensions.Q<VisualElement>(m_mine, "legion", (string)null);
		m_lbMineLegionPoint = UQueryExtensions.Q<Label>(m_mine, "lb_legionPoint", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null);
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
		LegionWarLegionRankListAck legionWarLegionRankListAck = zbSelf.Instance.legionWarHandler.legionWarLegionRankListAck;
		bool flag = legionWarLegionRankListAck == null || legionWarLegionRankListAck.RankList.Count == 0;
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_scrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (legionWarLegionRankListAck != null)
		{
			for (int i = 0; i < legionWarLegionRankListAck.RankList.Count; i++)
			{
				zbLegionWarRankLegionItem zbLegionWarRankLegionItem2 = new zbLegionWarRankLegionItem();
				zbLegionWarRankLegionItem2.OnEnable(m_wndParent);
				zbLegionWarRankLegionItem2.Init(legionWarLegionRankListAck.RankList[i]);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbLegionWarRankLegionItem2);
			}
		}
	}

	private void RefreshSelfInfo()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		m_mine.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.legionWarHandler.legionWarLegionRankListAck != null)
		{
			LegionWarLegionRankDetail legionWarLegionRankDetail = ((IEnumerable<LegionWarLegionRankDetail>)zbSelf.Instance.legionWarHandler.legionWarLegionRankListAck.RankList).FirstOrDefault((LegionWarLegionRankDetail a) => a.LegionFlagInfo.LegionId == zbServerData.sGameData.LegionSelfInfo.LegionId);
			m_mineNoRankInfo.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(legionWarLegionRankDetail != null));
			m_mineIcon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(legionWarLegionRankDetail == null));
			if (legionWarLegionRankDetail != null)
			{
				m_mine.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbSelf.Instance.legionWarHandler.InitLegionInfo(m_mineLegionInfo, legionWarLegionRankDetail.LegionFlagInfo);
				zbSelf.Instance.rankHandler.InitRank(UQueryExtensions.Q<Label>(m_mine, "lb_rank", (string)null), legionWarLegionRankDetail.RankData.Rank);
				((TextElement)m_lbMineLegionPoint).text = legionWarLegionRankDetail.RankData.Value.ToString();
			}
			else
			{
				((VisualElement)m_lbRank).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
				((TextElement)m_lbRank).text = zbUtil.Language("lang.10512");
			}
		}
	}
}
