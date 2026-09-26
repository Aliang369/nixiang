using System;
using UnityEngine.UIElements;

public class zbSeasonForceTechTabUpgrade
{
	private zbSeasonForceTechWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLevel;

	private Label m_lbOutPut0;

	private Label m_lbOutPut1;

	private Button m_btnUpgrade;

	private Label m_lbCost;

	public void OnEnable(zbSeasonForceTechWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbOutPut0 = UQueryExtensions.Q<Label>(m_RootElement, "lb_outPut0", (string)null);
		m_lbOutPut0 = UQueryExtensions.Q<Label>(m_RootElement, "lb_outPut1", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnUpgrade, "lb_cost", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
	}

	public void Refresh()
	{
	}

	private void OnUpgrade()
	{
	}
}
