using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderTipWnd : zbTabWnd
{
	private Button m_btnClose;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private zbLeaderTipTabEquip m_moduleEquip;

	private zbLeaderTipTabGem m_moduleGem;

	private Sprite m_select;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_tabLayers[0] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel0", (string)null);
		m_tabLayers[1] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel1", (string)null);
		m_tabButtons[0] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab0", (string)null);
		m_tabButtons[1] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab1", (string)null);
		m_select = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		InitTab(m_tabButtons, m_tabLayers, OnSelect);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		DoSelectTab();
	}

	private void OnSelect(int index)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 2; i++)
		{
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground((i == index) ? m_select : null);
		}
	}

	public override void DoSelectTab()
	{
		switch (m_currTab)
		{
		case 0:
			if (m_moduleEquip == null)
			{
				m_moduleEquip = new zbLeaderTipTabEquip();
				m_moduleEquip.OnEnable(this, m_tabLayers[0]);
			}
			m_moduleEquip.Refresh();
			break;
		case 1:
			if (m_moduleGem == null)
			{
				m_moduleGem = new zbLeaderTipTabGem();
				m_moduleGem.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleGem.Refresh();
			break;
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
