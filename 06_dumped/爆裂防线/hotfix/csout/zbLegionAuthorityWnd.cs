using System;
using UnityEngine.UIElements;

public class zbLegionAuthorityWnd : zbBaseWnd
{
	private Label m_lbInfo;

	private Label[] m_lbAuths = (Label[])(object)new Label[6];

	private Label[] m_lbTitles = (Label[])(object)new Label[4];

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_lbInfo = UQueryExtensions.Q<Label>(m_RootElement, "lb_info", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_lbTitles[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_title" + i, (string)null);
		}
		for (int j = 0; j < 6; j++)
		{
			m_lbAuths[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_auth" + j, (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
