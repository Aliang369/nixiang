using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldExploreCancelWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private Label m_lbDesc;

	private Label m_lbTip;

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private WorldReconDetail m_reconDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldReconExploreCancel_Ack, OnEventUpdate);
		AddDark();
	}

	public void RefreshCustom(WorldReconDetail reconDetail)
	{
		m_reconDetail = reconDetail;
	}

	private void OnConfirm()
	{
		zbWsMsgHandler.Instance.World_ReconExploreCancel_Req(m_reconDetail.ReconId);
		zbSelf.Instance.newTagHandler.RemoveNewGridId(m_reconDetail.GridId.ToString());
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		if (m_reconDetail != null && m_reconDetail.State == EWorldReconState.Explore && zbSelf.Instance.timeHandler.GetServerTime() >= m_reconDetail.StateTm)
		{
			OnClose();
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10255") + ": " + zbUtil.Language("lang.10210"));
		}
	}

	private void OnEventUpdate(object obj)
	{
		OnClose();
	}
}
