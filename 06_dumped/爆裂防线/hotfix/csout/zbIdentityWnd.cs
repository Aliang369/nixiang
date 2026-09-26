using System;
using UnityEngine.UIElements;

public class zbIdentityWnd : zbBaseWnd
{
	private Button m_btnOk;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "button_ok", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)onButtonOk, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)onButtonOk, "", (TrickleDown)0);
		AddDark();
	}

	public void onButtonOk(ClickEvent evt)
	{
		base.Hide();
	}
}
