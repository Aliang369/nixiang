using System;
using UnityEngine.UIElements;

public class zbTacticInfoWnd : zbBaseWnd
{
	private VisualElement m_bgClose;

	private ListView m_listView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bgClose = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_touch", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listview", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgClose, (Action)OnClose, "", (TrickleDown)0);
	}

	private void OnClose()
	{
		Hide();
	}
}
