using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbArenaRankItem : zbViewBaseItem
{
	private VisualElement m_bg;

	private Label m_lbRank;

	private VisualElement m_playerInfo;

	private Button m_btnHead;

	private zbRankWnd m_wndParent;

	private RankDetail m_rankDetail;

	public zbArenaRankItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/arena/tb_arenaRank.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbRankWnd wndParent)
	{
		m_wndParent = wndParent;
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(RankDetail rankDetail)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		bool flag = rankDetail.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId;
		m_bg.style.backgroundImage = new StyleBackground(flag ? m_wndParent.m_rankBgMine : m_wndParent.m_rankBg);
		m_rankDetail = rankDetail;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_rankDetail.RoleBaseInfo);
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, (int)m_rankDetail.RankData.Value);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnClickHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
	}
}
