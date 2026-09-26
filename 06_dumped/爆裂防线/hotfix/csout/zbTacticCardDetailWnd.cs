using System;
using UnityEngine.UIElements;

public class zbTacticCardDetailWnd : zbBaseWnd
{
	private Button m_btnClose;

	private zbTacticUxmlCard m_uxmlCard;

	private VisualElement m_cardFrame;

	private Button m_btnUpgradeOrActivate;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_cardFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "card_frame", (string)null);
		m_uxmlCard = new zbTacticUxmlCard(UQueryExtensions.Q<TemplateContainer>(m_RootElement, "uxml_card", (string)null));
		m_btnUpgradeOrActivate = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade_or_activate", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgradeOrActivate, (Action<ClickEvent>)OnUpgradeOrActivate, "", (TrickleDown)0);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public void OnUpgradeOrActivate(ClickEvent evt)
	{
		zbUIMgr.Instance.BubbleInfo("激活或者升星");
	}
}
