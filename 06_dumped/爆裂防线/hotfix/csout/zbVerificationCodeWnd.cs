using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbVerificationCodeWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbDesc;

	private Button m_btnConfirm;

	private TextField m_codeField;

	private zbDebouncedPnl m_inputTouch;

	private Label[] codeLabels;

	private string m_correctCode = "1234";

	private string m_descString;

	private string m_btnString;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_codeField = UQueryExtensions.Q<TextField>(m_RootElement, "codefield", (string)null);
		((TextInputBaseField<string>)(object)m_codeField).maxLength = 4;
		((TextInputBaseField<string>)(object)m_codeField).keyboardType = (TouchScreenKeyboardType)4;
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "codecontainer", (string)null);
		codeLabels = (Label[])(object)new Label[4];
		for (int i = 0; i < 4; i++)
		{
			codeLabels[i] = UQueryExtensions.Q<Label>(val, $"code{i + 1}", (string)null);
			((TextElement)codeLabels[i]).text = "";
		}
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_uiEvent.RegisterValueChangedCallback((BindableElement)(object)m_codeField, OnCodeChange);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
	}

	private void OnInputClick()
	{
		((Focusable)m_codeField).Focus();
	}

	public void OnCodeChange(string input)
	{
		for (int i = 0; i < codeLabels.Length; i++)
		{
			((TextElement)codeLabels[i]).text = ((i < input.Length) ? input[i].ToString() : "");
		}
	}

	public void RefreshCustom(string descString, string btnString, Action<bool> callback)
	{
		m_descString = descString;
		m_btnString = btnString;
		m_callback = callback;
		m_correctCode = Rand.Range(1000, 9999).ToString();
		((BaseField<string>)(object)m_codeField).value = "";
		((TextElement)m_lbDesc).text = string.Format(descString, m_correctCode);
		((TextElement)m_btnConfirm).text = btnString;
	}

	private void OnConfirm()
	{
		if (((BaseField<string>)(object)m_codeField).value == m_correctCode)
		{
			m_callback?.Invoke(obj: true);
			Hide();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10552"));
		}
	}

	public void OnClose()
	{
		m_callback?.Invoke(obj: false);
		Hide();
	}
}
