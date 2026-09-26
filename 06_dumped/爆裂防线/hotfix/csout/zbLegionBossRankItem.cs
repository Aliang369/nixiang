using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionBossRankItem : zbViewBaseItem
{
	private VisualElement m_playerInfo;

	private Label m_lbName;

	private Label m_lbPower;

	private Label m_lbDmg;

	private Label m_lbTime;

	private Label m_lbPosition;

	private Label m_lbRank;

	private Button m_btnHead;

	private RankDetail m_rankDetail;

	public zbLegionBossRankItem(VisualTreeAsset tAsset, zbLegionBossRankWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_power", (string)null);
		m_lbDmg = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_dmg", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_rank", (string)null);
		m_lbPosition = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_position", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
	}

	public void Init(RankDetail rankDetail)
	{
		m_rankDetail = rankDetail;
		InitPlayerInfo();
		ELegionOfficial officialById = zbSelf.Instance.legionHandler.GetOfficialById((int)m_rankDetail.RoleBaseInfo.RoleId);
		((TextElement)m_lbPosition).text = zbSelf.Instance.legionHandler.GetOfficialName(officialById);
		((TextElement)m_lbRank).text = m_rankDetail.RankData.Rank.ToString();
		((TextElement)m_lbDmg).text = m_rankDetail.RankData.Value.ToString();
		((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(m_rankDetail.RankData.RankTm);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void InitPlayerInfo()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_rankDetail.RoleBaseInfo.RoleName;
		((TextElement)m_lbPower).text = m_rankDetail.RoleBaseInfo.TeamAttack.ToString();
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_rankDetail.RoleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "head", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_rankDetail.RoleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null)).text = m_rankDetail.RoleBaseInfo.Level.ToString();
	}

	private void OnClickHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_rankDetail.RoleBaseInfo.RoleId);
	}
}
