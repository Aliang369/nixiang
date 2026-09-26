using System;
using UnityEngine.UIElements;

public class zbSeasonRechargeTabPrivilegeCard
{
	private zbSeasonRechargeWnd m_wndParent;

	private VisualElement m_root;

	private Label m_lbTitle;

	private ScrollView m_scrollView;

	private Button m_btnBuy;

	public void OnEnable(zbSeasonRechargeWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_root = root;
		m_lbTitle = UQueryExtensions.Q<Label>(m_root, "lb_title", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_root, (string)null, (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_root, "btn_buy", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
	}

	public void Refresh()
	{
	}

	private void OnBuy()
	{
	}
}
