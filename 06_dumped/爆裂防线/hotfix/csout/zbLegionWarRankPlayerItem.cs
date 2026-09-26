using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarRankPlayerItem : zbViewBaseItem
{
	private zbLegionWarRankWnd m_wndParent;

	private Button m_btnHead;

	private Label m_lbPlayerPoint;

	private Label m_lbRank;

	private VisualElement m_playerInfo;

	private LegionWarPlayerRankDetail m_detail;

	public zbLegionWarRankPlayerItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarRankPlayer.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbLegionWarRankWnd wndParent)
	{
		m_wndParent = wndParent;
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		m_lbPlayerPoint = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_playerPoint", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnPlayer, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(LegionWarPlayerRankDetail detail)
	{
		m_detail = detail;
		((TextElement)m_lbPlayerPoint).text = detail.RankData.Value.ToString();
		zbSelf.Instance.legionWarHandler.InitPlayerInfo(m_playerInfo, m_detail.RoleBaseInfo, m_detail.LegionFlagInfo);
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, m_detail.RankData.Rank);
	}

	private void OnPlayer()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_detail.RoleBaseInfo.RoleId);
	}
}
