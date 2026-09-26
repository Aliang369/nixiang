using System;
using UnityEngine.UIElements;

public class zbSpeedBox : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnOk;

	private Button m_btnCancel;

	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom()
	{
		zbCombatSystem.Instance.Pause();
		zbAudioMgr.Instance.PauseAllAudio();
	}

	private void OnOk(ClickEvent evt)
	{
		zbCombatSystem.Instance.Resume();
		zbAudioMgr.Instance.ResumeAllAudio();
		Hide();
	}

	private void OnClose(ClickEvent evt)
	{
	}
}
