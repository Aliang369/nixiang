using System;
using UnityEngine.UIElements;

public class zbTipNewversionWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnOk;

	private Label m_lblDesc;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "btn_ok", (string)null);
		m_lblDesc = UQueryExtensions.Q<Label>(m_RootElement, "lbldesc", (string)null);
		((TextElement)m_lblDesc).text = zbUtil.Language("lang.10785");
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnBntOk, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	private void OnBntOk(ClickEvent cvt)
	{
		Hide();
		zbUIMgr.Instance.ShowSupportContact();
	}
}
