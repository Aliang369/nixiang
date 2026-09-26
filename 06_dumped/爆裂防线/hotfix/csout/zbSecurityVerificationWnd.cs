using System;
using UnityEngine.UIElements;

public class zbSecurityVerificationWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnConfirm;

	private Button m_btnClose;

	private Label m_lbDesc;

	private IntegerField[] m_intTextFields = (IntegerField[])(object)new IntegerField[4];

	private int[] answerCode = new int[4];

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		AddDark();
		for (int i = 0; i < 4; i++)
		{
			m_intTextFields[i] = UQueryExtensions.Q<IntegerField>(m_RootElement, $"intField{i}", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ChangeEvent<int>>((VisualElement)(object)m_intTextFields[i], (Action<ChangeEvent<int>>)delegate(ChangeEvent<int> evt)
			{
				OnValueChange(evt, tempIndex);
			}, "", (TrickleDown)0);
			TextElement val = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_intTextFields[i], "", (string)null);
			if (val != null)
			{
				m_uiEvent.RegisterCallback<KeyDownEvent>((VisualElement)(object)val, (Action<KeyDownEvent>)delegate(KeyDownEvent evt)
				{
					OnKeyDown(evt, tempIndex);
				}, "", (TrickleDown)0);
			}
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnCheckCode, "", (TrickleDown)0);
	}

	private void OnKeyDown(KeyDownEvent evt, int index)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)((KeyboardEventBase<KeyDownEvent>)(object)evt).keyCode == 8 && string.IsNullOrEmpty(((TextInputBaseField<int>)(object)m_intTextFields[index]).text) && index > 0)
		{
			((EventBase)evt).StopPropagation();
			TextElement obj = UQueryExtensions.Q<TextElement>((VisualElement)(object)m_intTextFields[index - 1], (string)null, (string)null);
			((Focusable)UQueryExtensions.Q<TextElement>((VisualElement)(object)m_intTextFields[index], (string)null, (string)null)).Blur();
			((Focusable)obj).Focus();
		}
	}

	private void OnValueChange(ChangeEvent<int> changed, int index)
	{
		int newValue = changed.newValue;
		if (newValue < 0 || newValue > 9)
		{
			((BaseField<int>)(object)m_intTextFields[index]).value = 0;
		}
		else if (!string.IsNullOrEmpty(((TextInputBaseField<int>)(object)m_intTextFields[index]).text))
		{
			if (index < 3)
			{
				((Focusable)m_intTextFields[index + 1]).Focus();
			}
			else
			{
				OnCheckCode();
			}
		}
	}

	private void OnCheckCode()
	{
		for (int i = 0; i < 4 && ((BaseField<int>)(object)m_intTextFields[i]).value == answerCode[i]; i++)
		{
		}
	}

	public void RefreshCustom()
	{
		GenerateRandomCode();
		for (int i = 0; i < 4; i++)
		{
			UQueryExtensions.Q<TextElement>((VisualElement)(object)m_intTextFields[i], (string)null, (string)null).text = null;
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void GenerateRandomCode()
	{
		for (int i = 0; i < 4; i++)
		{
			answerCode[i] = Rand.Range(1, 10);
		}
	}
}
