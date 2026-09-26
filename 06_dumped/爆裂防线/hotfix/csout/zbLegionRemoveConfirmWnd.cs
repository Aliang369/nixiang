using System;
using UnityEngine.UIElements;

public class zbLegionRemoveConfirmWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnConfirm;

	private Label m_lbName;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
