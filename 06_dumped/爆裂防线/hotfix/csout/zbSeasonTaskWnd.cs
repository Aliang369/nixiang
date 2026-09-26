using System;
using UnityEngine.UIElements;

public class zbSeasonTaskWnd : zbBaseWnd
{
	private Button m_btnBack;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom()
	{
	}

	private void OnClose()
	{
		Hide();
	}
}
