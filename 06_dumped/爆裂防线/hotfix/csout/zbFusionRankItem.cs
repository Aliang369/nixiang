using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFusionRankItem : zbViewBaseItem
{
	private Label m_lbRank;

	private VisualElement m_playerInfo;

	private Label m_lbProgress;

	private Label m_lbTime;

	private Button m_btnHead;

	private VisualElement m_bg;

	private zbFusionRankWnd m_wndParent;

	private RankDetail m_rankDetail;

	public zbFusionRankItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_fusionRank.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbFusionRankWnd wndParent)
	{
		m_wndParent = wndParent;
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnClickHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
	}

	public void Refresh(RankDetail rankDetail)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		bool flag = rankDetail.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId;
		m_bg.style.backgroundImage = new StyleBackground(flag ? m_wndParent.m_rankBgMine : m_wndParent.m_rankBg);
		m_rankDetail = rankDetail;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_rankDetail.RoleBaseInfo);
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_rankDetail.RankData.Rank);
		((TextElement)m_lbProgress).text = m_rankDetail.RankData.Value.ToString();
		((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(m_rankDetail.RankData.RankTm);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
