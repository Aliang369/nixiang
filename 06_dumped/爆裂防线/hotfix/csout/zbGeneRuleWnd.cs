using System;
using System.Text;
using UnityEngine.UIElements;

public class zbGeneRuleWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnTouch;

	private Label m_lbRuleDesc;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbRuleDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_rule", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(string[] langKeys)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string key in langKeys)
		{
			stringBuilder.AppendLine(zbUtil.Language(key));
		}
		((TextElement)m_lbRuleDesc).text = stringBuilder.ToString();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
