using System;
using UnityEngine.UIElements;

public class zbOpenServerRankRuleWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private Label m_lbRule;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbRule = UQueryExtensions.Q<Label>(m_RootElement, "lb_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
