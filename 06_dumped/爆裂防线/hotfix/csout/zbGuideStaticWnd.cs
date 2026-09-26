using System;
using UnityEngine.UIElements;

public class zbGuideStaticWnd : zbBaseWnd
{
	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	private void OnClose()
	{
		Hide();
	}
}
