using System;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRuleWnd : zbBaseWnd
{
	public Button m_btnClose;

	private Button m_btnTouch;

	private Label m_lbRuleDesc;

	private ScrollView m_scrollView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbRuleDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_rule", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
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

	private void OnClose()
	{
		((VisualElement)m_scrollView).schedule.Execute((Action)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			m_scrollView.scrollOffset = Vector2.zero;
		}).StartingIn(1L);
		Hide();
	}
}
