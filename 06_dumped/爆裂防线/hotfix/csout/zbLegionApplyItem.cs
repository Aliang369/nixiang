using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionApplyItem : zbViewBaseItem
{
	private Label m_lbName;

	private Label m_iconPower;

	private Label m_lbApplyTime;

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private VisualElement m_playerInfo;

	private LegionApplyDetail m_item;

	private zbLegionApplyWnd m_parent;

	public zbLegionApplyItem(VisualTreeAsset tAsset, zbLegionApplyWnd parent)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_parent = parent;
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_iconPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "icon_power", (string)null);
		m_lbApplyTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_applyTime", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_confirm", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnCancel, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
	}

	public void Init(LegionApplyDetail item)
	{
		((TextElement)m_lbName).text = item.RoleBaseInfo.RoleName;
		((TextElement)m_iconPower).text = item.RoleBaseInfo.TeamAttack.ToString();
		((TextElement)m_lbApplyTime).text = item.ApplyTm.ToString();
		m_item = item;
		InitPlayerInfo();
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void InitPlayerInfo()
	{
		((TextElement)m_lbName).text = m_item.RoleBaseInfo.RoleName;
		((TextElement)m_iconPower).text = m_item.RoleBaseInfo.TeamAttack.ToString();
		zbUtil.InitPlayerInfo(UQueryExtensions.Q<VisualElement>(m_playerInfo, "header", (string)null), frame: UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null), lb_level: UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null), HeadId: m_item.RoleBaseInfo.HeadId, HeadFrameId: m_item.RoleBaseInfo.HeadFrameId, Level: m_item.RoleBaseInfo.Level);
	}

	private void OnCancel(ClickEvent evt)
	{
		m_parent.OnCancel((int)m_item.RoleBaseInfo.RoleId);
	}

	private void OnConfirm(ClickEvent evt)
	{
		m_parent.OnConfirm((int)m_item.RoleBaseInfo.RoleId);
	}
}
