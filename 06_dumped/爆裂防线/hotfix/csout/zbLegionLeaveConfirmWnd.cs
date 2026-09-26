using System;
using UnityEngine.UIElements;

public class zbLegionLeaveConfirmWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private Label m_lbDiamond;

	private Label m_lbTitle;

	private Label m_lbContent;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbDiamond = UQueryExtensions.Q<Label>(m_RootElement, "lb_diamond", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbContent = UQueryExtensions.Q<Label>(m_RootElement, "lb_content", (string)null);
		((TextElement)m_lbContent).text = string.Format(zbUtil.Language("lang.10488"), ((object)zbSelf.Instance.legionHandler.GetLegionBase()[(object)"clearCDDiamond"]).ToString());
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnConfirm(ClickEvent evt)
	{
		DateTime utcNow = DateTime.UtcNow;
		bool num = utcNow.DayOfWeek == DayOfWeek.Sunday;
		bool flag = utcNow.Hour >= 11 && utcNow.Hour < 22;
		if (num & flag)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11262"));
		}
		else
		{
			zbWsMsgHandler.Instance.Legion_Quit_Req();
		}
	}
}
