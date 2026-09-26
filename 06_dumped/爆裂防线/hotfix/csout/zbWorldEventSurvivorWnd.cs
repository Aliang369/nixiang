using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldEventSurvivorWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Label m_lbName;

	private Label m_lbDesc;

	private Label m_lbAwardTitle;

	private VisualElement m_awardList;

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private Button m_btnTouch;

	private WorldEventDetail m_eventDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbAwardTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_awardTitle", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_awardList = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardList", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(WorldEventDetail eventDetail)
	{
		m_eventDetail = eventDetail;
	}

	private void OnClose()
	{
		Hide();
	}
}
