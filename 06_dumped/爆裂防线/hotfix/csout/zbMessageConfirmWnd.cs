using System;
using UnityEngine.UIElements;

public class zbMessageConfirmWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private Button m_btnConfirm;

	private Button m_btnClose;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(string desc, Action<bool> callback, bool showClostBtn = true)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDesc).text = desc;
		m_callback = callback;
		((VisualElement)m_btnClose).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!showClostBtn));
	}

	private void OnConfirm(ClickEvent evt)
	{
		m_callback?.Invoke(obj: true);
		Hide();
	}

	private void OnClose(ClickEvent evt)
	{
		m_callback?.Invoke(obj: false);
		Hide();
	}
}
