using System;
using UnityEngine.UIElements;

public class zbSupportContactWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbTitle;

	private Label m_labDesc;

	private Button m_btnOpenLink;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_labDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnOpenLink = UQueryExtensions.Q<Button>(m_RootElement, "btn_openLink", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOpenLink, (Action)onOpenLink, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
		SetSupportUIText();
	}

	private void SetSupportUIText()
	{
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.10629");
		((TextElement)m_labDesc).text = "指挥官，您可以通过微信联系我们，我们会尽快协助您解决问题。";
	}

	private static void onOpenLink()
	{
		zbUtil.LinkToURLWithoutParams("https://work.weixin.qq.com/kfid/kfca2a9534954e2c840");
	}

	private void OnClose()
	{
		Hide();
	}
}
