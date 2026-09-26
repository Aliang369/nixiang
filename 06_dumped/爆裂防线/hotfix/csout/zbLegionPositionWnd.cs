using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbLegionPositionWnd : zbBaseWnd
{
	private Label m_lbMember;

	private Label m_lbAdviser;

	private Label m_lbLeaderVal;

	private Label m_lbAdviserVal;

	private Button m_btnClose;

	private Button m_btnConfirm;

	private Button m_btnRule;

	private Button m_btnTouch;

	private VisualElement[] m_btnsToggle = (VisualElement[])(object)new VisualElement[4];

	private VisualElement[] m_lbIsCheck = (VisualElement[])(object)new VisualElement[4];

	private ELegionOfficial m_targetOfficial = ELegionOfficial.Member;

	private ELegionOfficial m_myOfficial;

	private long m_playerId;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_lbMember = UQueryExtensions.Q<Label>(m_RootElement, "lb_member", (string)null);
		m_lbAdviser = UQueryExtensions.Q<Label>(m_RootElement, "lb_adviser", (string)null);
		m_lbLeaderVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_leaderVal", (string)null);
		m_lbAdviserVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_adviserVal", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnRule = UQueryExtensions.Q<Button>(m_RootElement, "btn_rule", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_btnsToggle[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_toggle" + i, (string)null);
			m_lbIsCheck[i] = UQueryExtensions.Q<VisualElement>(m_btnsToggle[i], "sprite_ok", (string)null);
			int tempIndex = i;
			((CallbackEventHandler)m_btnsToggle[i]).RegisterCallback<ClickEvent>((EventCallback<ClickEvent>)delegate
			{
				OnClickToggle(tempIndex);
			}, (TrickleDown)0);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRule, (Action<ClickEvent>)OnClickRule, "", (TrickleDown)0);
	}

	public void RefreshCustom(long playerId)
	{
		m_playerId = playerId;
		m_targetOfficial = zbSelf.Instance.legionHandler.GetOfficialById((int)playerId);
		m_myOfficial = zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId);
		InitLayoutByOfficial();
	}

	private void OnClickToggle(int index)
	{
		if (index != (int)m_targetOfficial)
		{
			m_lbIsCheck[index].visible = true;
			m_lbIsCheck[(int)m_targetOfficial].visible = false;
			m_targetOfficial = (ELegionOfficial)index;
		}
	}

	private void InitLayoutByOfficial()
	{
		((VisualElement)m_lbMember).visible = false;
		((VisualElement)m_lbAdviser).visible = false;
		for (int i = 0; i < 4; i++)
		{
			m_lbIsCheck[i].visible = ((m_targetOfficial == (ELegionOfficial)i) ? true : false);
		}
		if (m_targetOfficial == ELegionOfficial.Member)
		{
			((VisualElement)m_lbMember).visible = true;
		}
		else
		{
			((VisualElement)m_lbAdviser).visible = true;
		}
		if (m_myOfficial == ELegionOfficial.Adviser)
		{
			m_btnsToggle[2].visible = false;
			m_btnsToggle[3].visible = false;
			((VisualElement)m_lbLeaderVal).visible = false;
			((VisualElement)m_lbAdviserVal).visible = false;
		}
	}

	private void OnConfirm(ClickEvent evt)
	{
		if (m_targetOfficial == ELegionOfficial.EmptyValue)
		{
			if (zbSelf.Instance.legionHandler.CheckCanKickMember())
			{
				zbWsMsgHandler.Instance.Legion_Kick_Req((int)m_playerId);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.Legion_OfficialSet_Req((int)m_playerId, m_targetOfficial);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnClickRule(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionAuthority();
	}
}
