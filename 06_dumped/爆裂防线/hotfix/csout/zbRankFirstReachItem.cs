using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbRankFirstReachItem : zbViewBaseItem
{
	private VisualElement m_playerInfo;

	private Label m_lbNotReach;

	private Label m_lbTime;

	private Label m_lbProgress;

	private zbItem m_award;

	private Button m_btnHead;

	private VisualElement m_redDot;

	private zbRankFirstReachWnd m_parent;

	private RankFirstReachDetail m_rankFirstReachDetail;

	public zbRankFirstReachItem(VisualTreeAsset tAsset, zbRankFirstReachWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_parent = parent;
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_lbNotReach = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_notReach", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_award = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "award", (string)null);
		m_redDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "red_dot", (string)null);
		m_award.OnEnable(m_parent);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
	}

	private void OnHead()
	{
		if (m_rankFirstReachDetail != null && m_rankFirstReachDetail.RoleBaseInfo != null && (int)m_rankFirstReachDetail.RoleBaseInfo.RoleId > 0)
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankFirstReachDetail.RoleBaseInfo.RoleId);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10499"));
		}
	}

	public void Init(RankFirstReachDetail rankFirstReachDetail)
	{
		m_rankFirstReachDetail = rankFirstReachDetail;
		bool flag = m_rankFirstReachDetail.RoleBaseInfo != null;
		m_playerInfo.visible = flag;
		((VisualElement)m_lbNotReach).visible = !flag;
		((VisualElement)m_lbTime).visible = flag;
		zbRankAwardCsvRecord zbRankAwardCsvRecord2 = zbSelf.Instance.rankHandler.GetRankAwardRecordByType(ERankType.MainQuest).FirstOrDefault((zbRankAwardCsvRecord t) => t.id == rankFirstReachDetail.ReachId);
		m_award.Refresh(new zbItemData((EResourceType)zbRankAwardCsvRecord2.awardList[0][0], zbRankAwardCsvRecord2.awardList[0][1], zbRankAwardCsvRecord2.awardList[0][2]));
		RankFirstReachAwardDetail[] array = ((IEnumerable<RankFirstReachAwardDetail>)zbServerData.sGameData.AuxAwardInfo.RankList).Where((RankFirstReachAwardDetail r) => r.RankType == ERankType.MainQuest).ToArray();
		if (array != null)
		{
			bool hadReceived = array.Length != 0 && array[0].RecordId >= rankFirstReachDetail.ReachId;
			bool canReceive = rankFirstReachDetail.ReachTm > 0 && !hadReceived;
			m_redDot.visible = canReceive;
			if (hadReceived)
			{
				m_award.SetGettedVisible(vis: true);
			}
			m_award.SetTouchCallback(delegate
			{
				if (canReceive)
				{
					zbWsMsgHandler.Instance.Rank_FirstReachAward_Req(ERankType.MainQuest);
				}
				else if (hadReceived)
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10148"));
				}
				else
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10280"));
				}
			});
		}
		((TextElement)m_lbProgress).text = zbResourceHelper.GetMainLevelName(zbRankAwardCsvRecord2.value);
		if (flag)
		{
			zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_rankFirstReachDetail.RoleBaseInfo);
			((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(rankFirstReachDetail.ReachTm);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
