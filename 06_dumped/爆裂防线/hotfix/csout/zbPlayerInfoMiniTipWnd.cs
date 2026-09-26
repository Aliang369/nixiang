using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerInfoMiniTipWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement m_headIcon;

	private VisualElement m_headFrame;

	private Label m_lbName;

	private Label m_lbLevel;

	private Label m_lbId;

	private Label m_lbServer;

	private Label m_lbLegionName;

	private Button m_btnInfo;

	private Button m_btnCopy;

	private RoleMiniTipsInfo m_roleTipsInfo;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_headIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "head", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "headFrame", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbId = UQueryExtensions.Q<Label>(m_RootElement, "lb_id", (string)null);
		m_lbServer = UQueryExtensions.Q<Label>(m_RootElement, "lb_server", (string)null);
		m_lbLegionName = UQueryExtensions.Q<Label>(m_RootElement, "lb_legionId", (string)null);
		m_btnInfo = UQueryExtensions.Q<Button>(m_RootElement, "btn_info", (string)null);
		m_btnCopy = UQueryExtensions.Q<Button>(m_RootElement, "btn_copy", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnInfo, (Action)OnShowPlayerInfo, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCopy, (Action<ClickEvent>)OnCopyRoleID, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Friends_Delete, OnEventUpdate);
		AddDark();
	}

	public void RefreshCustom(RoleMiniTipsInfo _roleTipsInfo)
	{
		if (_roleTipsInfo != null)
		{
			m_roleTipsInfo = _roleTipsInfo;
			RefreshView();
			UpdateHeadAndHeadFrameDisplay();
		}
	}

	private void RefreshView()
	{
		((TextElement)m_lbName).text = m_roleTipsInfo.RoleBaseInfo.RoleName;
		((TextElement)m_lbLevel).text = m_roleTipsInfo.RoleBaseInfo.Level.ToString();
		((TextElement)m_lbId).text = m_roleTipsInfo.RoleBaseInfo.RoleId.ToString();
		((TextElement)m_lbServer).text = "Server: " + zbUtil.SetServerString(m_roleTipsInfo.RoleBaseInfo.ServerId);
		if (m_roleTipsInfo.LegionFlagInfo.LegionId != 0)
		{
			((TextElement)m_lbLegionName).text = zbUtil.Language("lang.10467") + "：" + m_roleTipsInfo.LegionFlagInfo.LegionName;
		}
		else
		{
			((TextElement)m_lbLegionName).text = zbUtil.Language("lang.10467") + "：--";
		}
	}

	private void UpdateHeadAndHeadFrameDisplay()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_roleTipsInfo.RoleBaseInfo.HeadId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex);
		m_headIcon.style.backgroundImage = new StyleBackground(val);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_roleTipsInfo.RoleBaseInfo.HeadFrameId);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex);
		m_headFrame.style.backgroundImage = new StyleBackground(val2);
	}

	private void OnShowPlayerInfo()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_roleTipsInfo.RoleBaseInfo.RoleId);
	}

	private void OnCopyRoleID(ClickEvent evt)
	{
		zbUtil.CopyString(m_roleTipsInfo.RoleBaseInfo.RoleId.ToString());
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		if (IsShow())
		{
			OnClose();
		}
	}
}
