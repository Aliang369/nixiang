using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbChatInputPanel
{
	private zbChatWnd m_parent;

	private VisualElement m_root;

	private TextField m_input;

	private Button m_btnSend;

	private Button m_btnFace;

	private zbDebouncedPnl m_inputTouch;

	public zbChatInputPanel(VisualElement root, zbChatWnd parent)
	{
		m_parent = parent;
		m_root = root;
		m_input = UQueryExtensions.Q<TextField>(root, "inputfield", (string)null);
		((BaseField<string>)(object)m_input).value = "";
		m_btnSend = UQueryExtensions.Q<Button>(root, "btnsend", (string)null);
		m_btnFace = UQueryExtensions.Q<Button>(root, "btnface", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFace, (Action<ClickEvent>)OnFace, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSend, (Action<ClickEvent>)OnSend, "", (TrickleDown)0);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(root, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(m_parent, OnInputClick);
	}

	private void OnInputClick()
	{
		((Focusable)m_input).Focus();
	}

	private void OnSend(ClickEvent evt)
	{
		string value = ((BaseField<string>)(object)m_input).value;
		if (value == "")
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10072"));
			return;
		}
		if (value.Length > 100)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10073"));
			return;
		}
		if (!zbCharChecker.IsSafeChatMessage(value))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10074"));
			return;
		}
		if ((Object)(object)m_parent != (Object)null)
		{
			m_parent.OnSend(value);
		}
		((BaseField<string>)(object)m_input).value = "";
	}

	private void OnFace(ClickEvent evt)
	{
		m_parent.ShowFace();
	}

	public void SetVisible(bool bVisible)
	{
		((BaseField<string>)(object)m_input).value = "";
		m_root.visible = bVisible;
		((VisualElement)m_input).visible = bVisible;
	}
}
