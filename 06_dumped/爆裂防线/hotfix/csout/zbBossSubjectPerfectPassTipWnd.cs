using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbBossSubjectPerfectPassTipWnd : zbBaseWnd
{
	private Button m_btnCheck;

	private Button m_btnClose;

	private Button m_btnConfirm;

	private Button m_btnCancel;

	private Label m_iconCheck;

	private int currDiff;

	private BossSubjectDetail bossSubjectDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnCheck = UQueryExtensions.Q<Button>(m_RootElement, "btn_check", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_iconCheck = UQueryExtensions.Q<Label>(m_RootElement, "lb_isCheck", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheck, (Action)OnToggle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnToNew, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnFight, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(BossSubjectDetail _bossSubjectDetail, int toLevel)
	{
		currDiff = toLevel;
		bossSubjectDetail = _bossSubjectDetail;
		((VisualElement)m_iconCheck).visible = !zbSelf.Instance.bossSubjectHandler.shouldShowBossSubjectTip;
	}

	private void OnToggle()
	{
		zbSelf.Instance.bossSubjectHandler.shouldShowBossSubjectTip = !zbSelf.Instance.bossSubjectHandler.shouldShowBossSubjectTip;
		((VisualElement)m_iconCheck).visible = !zbSelf.Instance.bossSubjectHandler.shouldShowBossSubjectTip;
	}

	private void OnFight()
	{
		Hide();
		zbWsMsgHandler.Instance.Operate_BossSubFightStart_Req(bossSubjectDetail.BaseInfo.UUID, currDiff);
	}

	private void OnToNew()
	{
		Hide();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
