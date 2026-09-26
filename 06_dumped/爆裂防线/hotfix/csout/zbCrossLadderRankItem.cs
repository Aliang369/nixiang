using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbCrossLadderRankItem : zbViewBaseItem
{
	private Label m_lbRank;

	private VisualElement m_playerInfo;

	private Button m_btnHead;

	private VisualElement m_bg;

	private zbCrossLadderRankWnd m_wndParent;

	private CrossLadderFriendDetail m_rankDetail;

	private zbLadderItem m_itemLadder;

	public zbCrossLadderRankItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/crossLadder/tb_crossLadderRank.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbCrossLadderRankWnd wndParent)
	{
		m_wndParent = wndParent;
		m_itemLadder = UQueryExtensions.Q<zbLadderItem>((VisualElement)(object)this, "ladder", (string)null);
		m_itemLadder.OnEnable(m_wndParent);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "player", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnClickHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
	}

	public void Refresh(CrossLadderFriendDetail rankDetail, int index)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		bool flag = rankDetail.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId;
		m_bg.style.backgroundImage = new StyleBackground(flag ? m_wndParent.m_rankBgMine : m_wndParent.m_rankBg);
		m_rankDetail = rankDetail;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_rankDetail.RoleBaseInfo);
		zbSelf.Instance.rankHandler.InitRank(m_lbRank, index);
		m_itemLadder.Refresh(rankDetail.LadderId);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}
}
