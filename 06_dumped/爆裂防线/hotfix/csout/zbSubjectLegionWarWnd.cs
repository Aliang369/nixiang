using System;
using UnityEngine.UIElements;

public class zbSubjectLegionWarWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement m_bg;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Button m_btnGoto;

	private Action<bool> m_callback;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "m_bg", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>(m_RootElement, "btn_goto", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action<ClickEvent>)OnGoto, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(Action<bool> callback)
	{
		m_callback = callback;
		((TextElement)m_lbTime).text = zbSelf.Instance.timeHandler.GetSubjectLegionWarTime();
	}

	private void OnGoto(ClickEvent evt)
	{
		Hide();
		zbLinkHelper.OpenLegion();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
		m_callback?.Invoke(obj: true);
	}
}
