using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbAcCenter
{
	private zbAccountWnd m_parent;

	private VisualElement m_centerElement;

	private Button m_btnBack;

	private Button m_btnAct;

	private Label m_lblAc;

	public zbAcCenter(VisualElement root, zbAccountWnd parent)
	{
		m_centerElement = root;
		m_parent = parent;
		m_btnBack = UQueryExtensions.Q<Button>(m_centerElement, "button_back", (string)null);
		m_btnAct = UQueryExtensions.Q<Button>(m_centerElement, "button_act", (string)null);
		m_lblAc = UQueryExtensions.Q<Label>(m_centerElement, "lblaccount", (string)null);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnBack, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAct, (Action<ClickEvent>)OnAct, "", (TrickleDown)0);
	}

	public void SetVisible(bool visi)
	{
		if (m_centerElement != null)
		{
			m_centerElement.visible = visi;
		}
	}

	public void SetAccountInfo()
	{
		((TextElement)m_lblAc).text = zbAccountMgr.GetLastLoginAccount();
	}

	private void OnAct(ClickEvent evt)
	{
		m_parent.ShowLoginPnl();
	}

	private void OnBack(ClickEvent evt)
	{
		if ((Object)(object)m_parent != (Object)null)
		{
			m_parent.Hide();
		}
	}
}
