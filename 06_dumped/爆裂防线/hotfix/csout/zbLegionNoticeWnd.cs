using System;
using UnityEngine.UIElements;

public class zbLegionNoticeWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnConfirm;

	private TextField m_textField;

	private VisualElement m_textElement;

	private Button m_btnTouch;

	private zbDebouncedPnl m_inputTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_textField = UQueryExtensions.Q<TextField>(m_RootElement, (string)null, (string)null);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Data, OnUpdateEventLegionNotice);
	}

	private void OnInputClick()
	{
		((Focusable)m_textField).Focus();
	}

	public override void Refresh()
	{
		((BaseField<string>)(object)m_textField).value = zbSelf.Instance.legionHandler.legionInfo.LegionSetInfo.LegionSign;
	}

	private void OnConfirm(ClickEvent evt)
	{
		if (((BaseField<string>)(object)m_textField).value == "")
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10082"));
			return;
		}
		string legionSign = ((BaseField<string>)(object)m_textField).value.Trim();
		zbWsMsgHandler.Instance.Legion_SignChange_Req(legionSign);
	}

	private void OnUpdateEventLegionNotice(object data)
	{
		Hide();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
