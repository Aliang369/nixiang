using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroRaritySelectWnd : zbTabWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private VisualElement m_listNo;

	private MultiColumnListView[] m_tabLayers = (MultiColumnListView[])(object)new MultiColumnListView[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private zbHeroRaritySelectList[] m_panels = new zbHeroRaritySelectList[3];

	public VisualElement listNo;

	private Button m_btnConfirm;

	private Sprite m_selectedIcon;

	private Sprite m_unSelectedIcon;

	private Action<int> m_callback;

	public int m_selectHeroTid;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_selectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_96x50_0.png");
		m_unSelectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_96x50_1.png");
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		Button[] tabButtons = m_tabButtons;
		VisualElement[] tabLayers = (VisualElement[])(object)m_tabLayers;
		InitTab(tabButtons, tabLayers, null);
		AddDark();
	}

	public void RefreshCustom(int heroTid, Action<int> callback)
	{
		m_selectHeroTid = heroTid;
		m_callback = callback;
		int tabIndex = 0;
		if (heroTid > 0)
		{
			HeroDetail heroDetail = ((IEnumerable<HeroDetail>)zbServerData.sGameData.HeroList).FirstOrDefault((HeroDetail x) => x.HeroTid == heroTid);
			if (heroDetail != null)
			{
				tabIndex = (int)(heroDetail.Rarity - 1);
			}
		}
		else
		{
			bool flag = false;
			for (int num = 0; num < 3; num++)
			{
				if (flag)
				{
					break;
				}
				foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
				{
					if (hero.Rarity == (ERarityType)(num + 1) && hero.Quality >= EQuality.Orange)
					{
						tabIndex = num;
						flag = true;
						break;
					}
				}
			}
		}
		SelectTab(tabIndex);
	}

	public override void DoSelectTab()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_tabButtons.Count(); i++)
		{
			bool flag = i == m_currTab;
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(flag ? m_selectedIcon : m_unSelectedIcon);
		}
		if (m_panels[m_currTab] == null)
		{
			m_panels[m_currTab] = new zbHeroRaritySelectList();
			m_panels[m_currTab].OnEnable(this, m_tabLayers[m_currTab]);
		}
		m_panels[m_currTab].Refresh();
	}

	public void RefreshSelect(int heroTid)
	{
		m_selectHeroTid = heroTid;
		m_panels[m_currTab].Refresh();
	}

	private void OnConfirm(ClickEvent evt)
	{
		m_callback?.Invoke(m_selectHeroTid);
		Hide();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
