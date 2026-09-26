using System;
using UnityEngine.UIElements;

public class zbTacticUpgradeWnd : zbBaseWnd
{
	private VisualElement m_bgClose;

	private Button m_btnUpgrade;

	private ListView m_listView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bgClose = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_touch", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listview", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>(m_bgClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnUpgrade, "", (TrickleDown)0);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnUpgrade(ClickEvent evt)
	{
		zbUIMgr.Instance.BubbleInfo("点击一键升星了");
	}
}
