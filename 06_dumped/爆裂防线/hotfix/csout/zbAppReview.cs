using System;
using UnityEngine.UIElements;

public class zbAppReview : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnReview;

	private Button m_btnCall;

	private Label m_lblMsg;

	private Label m_lblTitle;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnReview = UQueryExtensions.Q<Button>(m_RootElement, "btn_review", (string)null);
		m_btnCall = UQueryExtensions.Q<Button>(m_RootElement, "btn_call", (string)null);
		m_lblMsg = UQueryExtensions.Q<Label>(m_RootElement, "lb_message", (string)null);
		m_lblTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lblTitle).text = "尊敬的指挥官";
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReview, (Action)OnReview, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action)OnCall, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
		((TextElement)m_lblMsg).text = "尊敬的指挥官，您对目前的避难所体验是否满意？我们非常期待聆听您的宝贵意见和建议，让庇护所变得更安全、更强大";
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnReview()
	{
		zbSdkHelper.Option("appreview", "");
	}

	private void OnCall()
	{
		zbUtil.LinkToURLWithParams("https://blfx.mydreamjoy.com/community/#/feedback/suggest");
	}
}
