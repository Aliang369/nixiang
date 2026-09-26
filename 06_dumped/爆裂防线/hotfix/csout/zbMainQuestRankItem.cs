using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestRankItem : zbViewBaseItem
{
	private VisualElement m_bg;

	private Label m_lbRank;

	private VisualElement m_playerInfo;

	private Label m_lbProgress;

	private Label m_lbTime;

	private Button m_btnTouch;

	private zbMainQuestRankWnd m_wndParent;

	private RankDetail m_rankDetail;

	public zbMainQuestRankItem(VisualTreeAsset tAsset, zbMainQuestRankWnd wndParent)
	{
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(RankDetail rankDetail)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		m_rankDetail = rankDetail;
		if (rankDetail.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId)
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/worldBoss/rank_bg1.png"));
		}
		else
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/legion/rank_bg.png"));
		}
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_rankDetail.RoleBaseInfo);
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_rankDetail.RankData.Rank);
		((TextElement)m_lbProgress).text = zbResourceHelper.GetMainLevelName((int)m_rankDetail.RankData.Value);
		((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(m_rankDetail.RankData.RankTm);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
	}
}
