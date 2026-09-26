using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonRankItemMember : zbViewBaseItem
{
	private zbSeasonRankWnd m_wndParent;

	private VisualElement m_bg;

	private Label m_lbRank;

	private VisualElement m_playerInfo;

	private Label m_lbScore;

	private Button m_btnTouch;

	private zbItem m_item;

	private RankDetail m_rankDetail;

	public zbSeasonRankItemMember()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonRankMember.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbSeasonRankWnd wndParent)
	{
		m_wndParent = wndParent;
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_lbScore = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_score", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_item = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item", (string)null);
		m_item.OnEnable(m_wndParent);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(RankDetail rankDetail)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		m_rankDetail = rankDetail;
		if (m_rankDetail.RankData.Rank <= 3)
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossPeak/rankAward_bg" + m_rankDetail.RankData.Rank + ".png"));
			zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_rankDetail.RankData.Rank);
		}
		else
		{
			m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/crossPeak/rankAward_bg.png"));
			((VisualElement)m_lbRank).style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			((TextElement)m_lbRank).text = m_rankDetail.RankData.Rank.ToString();
		}
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_rankDetail.RoleBaseInfo);
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_rankDetail.RankData.Rank);
		((TextElement)m_lbScore).text = string.Format(zbUtil.Language("lang.11462"), m_rankDetail.RankData.Value);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
	}
}
