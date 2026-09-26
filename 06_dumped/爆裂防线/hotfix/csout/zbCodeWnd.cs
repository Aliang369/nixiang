using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCodeWnd : zbBaseWnd
{
	private Button m_btnCode;

	private Button m_Button_close;

	private Button m_Button_AllClose;

	private TextField m_inputField;

	private Button m_btnPaste;

	private zbDebouncedPnl m_inputTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_Button_close = UQueryExtensions.Q<Button>(m_RootElement, "Button_close", (string)null);
		m_Button_AllClose = UQueryExtensions.Q<Button>(m_RootElement, "Button_AllClose", (string)null);
		m_btnCode = UQueryExtensions.Q<Button>(m_RootElement, "button_code", (string)null);
		m_inputField = UQueryExtensions.Q<TextField>(m_RootElement, "field_input", (string)null);
		m_btnPaste = UQueryExtensions.Q<Button>(m_RootElement, "button_paste", (string)null);
		((TextElement)m_btnPaste).text = zbUtil.Language("lang.10677");
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_close, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_AllClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCode, (Action<ClickEvent>)OnCode, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPaste, (Action<ClickEvent>)OnPaste, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Rewards_RedeemCode, Refresh);
	}

	private void OnInputClick()
	{
		((Focusable)m_inputField).Focus();
	}

	private void Refresh(object obj)
	{
		((BaseField<string>)(object)m_inputField).value = "";
	}

	public void OnCode(ClickEvent evt)
	{
		string text = ((BaseField<string>)(object)m_inputField).value.Trim();
		if (!(text == ""))
		{
			zbWsMsgHandler.Instance.Rewards_RedeemCode_Req(text);
		}
	}

	public void OnPaste(ClickEvent evt)
	{
		if (string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10676"));
		}
		else
		{
			((BaseField<string>)(object)m_inputField).value = GUIUtility.systemCopyBuffer;
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
