using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonSearchWnd : zbTabWnd
{
	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_btns = (Button[])(object)new Button[2];

	private Button m_btnTouch;

	private Button m_btnClose;

	private Sprite m_selectTabSprite;

	private Sprite m_unSelectTabSprite;

	private zbSeasonSearchResource m_moduleSearchResource;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn" + i, (string)null);
		}
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_selectTabSprite = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_192x72_0.png");
		m_unSelectTabSprite = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_192x72_1.png");
		InitTab(m_btns, m_tabLayers, null);
		AddDark();
	}

	public void RefreshCustom(int index = 0)
	{
		m_currTab = index;
		SelectTab(m_currTab);
	}

	public override void DoSelectTab()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		switch (m_currTab)
		{
		case 0:
			if (m_moduleSearchResource == null)
			{
				m_moduleSearchResource = new zbSeasonSearchResource();
				m_moduleSearchResource.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleSearchResource.Refresh();
			break;
		}
		for (int i = 0; i < 2; i++)
		{
			bool flag = i == m_currTab;
			((VisualElement)m_btns[i]).style.backgroundImage = new StyleBackground(flag ? m_selectTabSprite : m_unSelectTabSprite);
		}
	}

	public void OnClose()
	{
		Hide();
	}
}
