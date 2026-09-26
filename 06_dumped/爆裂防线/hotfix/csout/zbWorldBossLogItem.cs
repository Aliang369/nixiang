using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldBossLogItem : zbViewBaseItem
{
	private VisualElement m_playerInfo;

	private Label m_lbName;

	private Label m_lbPower;

	private Label m_lbDmg;

	private Label m_lbTime;

	private Button m_btnHead;

	private WorldBossHurtRecordDetail m_detail;

	public zbWorldBossLogItem(VisualTreeAsset tAsset, zbWorldBossLogWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_power", (string)null);
		m_lbDmg = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_dmg", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_time", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_head", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnClickHead, "", (TrickleDown)0);
	}

	public void Init(WorldBossHurtRecordDetail detail)
	{
		m_detail = detail;
		InitPlayerInfo();
		((TextElement)m_lbDmg).text = zbUtil.ConvertToTBMK(m_detail.Hurt);
		((TextElement)m_lbTime).text = zbUtil.FormatUnixTimeToUtcString(m_detail.Time);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void InitPlayerInfo()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_detail.RoleBaseInfo.RoleName;
		((TextElement)m_lbPower).text = m_detail.RoleBaseInfo.TeamAttack.ToString();
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_detail.RoleBaseInfo.HeadId).icon;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "header", (string)null).style.backgroundImage = new StyleBackground(val);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_detail.RoleBaseInfo.HeadFrameId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null).style.backgroundImage = new StyleBackground(val2);
		((TextElement)UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null)).text = m_detail.RoleBaseInfo.Level.ToString();
	}

	private void OnClickHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_detail.RoleBaseInfo.RoleId);
	}
}
