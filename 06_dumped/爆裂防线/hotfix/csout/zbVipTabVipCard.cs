using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbVipTabVipCard
{
	private zbVipWnd m_wndParent;

	private VisualElement m_rootElement;

	private zbVipTabVipCardPanel m_panel = new zbVipTabVipCardPanel();

	private ScrollView m_tabScrollView;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Button m_btnCall;

	private List<zbVipTabVipCardItem> m_tabItems = new List<zbVipTabVipCardItem>();

	private List<EVipCardTab> m_vipCardTabs = new List<EVipCardTab>();

	public EVipCardTab m_curTab;

	public EVipCardTab m_targetTab;

	private int m_curTabIndex;

	public bool m_needScroll = true;

	private bool m_isFirstIn = true;

	public void OnEnable(zbVipWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_rootElement = root;
		m_panel.OnEnable(m_wndParent, this, m_rootElement);
		m_tabScrollView = UQueryExtensions.Q<ScrollView>(m_rootElement, "tabScrollView", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_rootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_rootElement, "btn_right", (string)null);
		m_btnCall = UQueryExtensions.Q<Button>(m_rootElement, "btn_call", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action)OnCall, "", (TrickleDown)0);
		GetVipCardTabs();
	}

	public void Refresh(int tabIndex = -1)
	{
		GetVipCardTabs();
		if (m_vipCardTabs.Count != 0)
		{
			if (tabIndex >= 0 && tabIndex < m_vipCardTabs.Count)
			{
				m_curTabIndex = tabIndex;
			}
			m_curTabIndex = Mathf.Clamp(m_curTabIndex, 0, m_vipCardTabs.Count - 1);
			m_curTab = m_vipCardTabs[m_curTabIndex];
			m_panel.Refresh();
			RefreshScrollTo(m_isFirstIn);
			m_isFirstIn = false;
		}
	}

	public void RefreshSelect()
	{
		m_curTab = m_targetTab;
		int num = m_vipCardTabs.FindIndex((EVipCardTab r) => r == m_targetTab);
		if (num >= 0)
		{
			m_curTabIndex = num;
		}
		m_panel.Refresh();
		RefreshScrollTo(isFirstIn: false);
	}

	private List<EVipCardTab> GetVipCardTabs()
	{
		m_vipCardTabs.Clear();
		m_vipCardTabs.Add(EVipCardTab.Week);
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Truck) && zbSelf.Instance.truckHandler.IsTruckOpen())
		{
			m_vipCardTabs.Add(EVipCardTab.Truck);
		}
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Treasure))
		{
			m_vipCardTabs.Add(EVipCardTab.Treasure);
		}
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.CrossLadder) && zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
		{
			m_vipCardTabs.Add(EVipCardTab.CrossLadder);
		}
		return m_vipCardTabs;
	}

	private void RefreshScrollTo(bool isFirstIn)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		GetVipCardTabs();
		DisplayStyle val = (DisplayStyle)(m_vipCardTabs.Count <= 1);
		((VisualElement)m_tabScrollView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_vipCardTabs.Count <= 1));
		((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit(val);
		((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit(val);
		if (m_vipCardTabs.Count != m_tabItems.Count)
		{
			m_tabItems.Clear();
			((VisualElement)m_tabScrollView).Clear();
			foreach (EVipCardTab vipCardTab in m_vipCardTabs)
			{
				zbVipTabVipCardItem zbVipTabVipCardItem2 = new zbVipTabVipCardItem(m_wndParent, this, vipCardTab);
				m_tabItems.Add(zbVipTabVipCardItem2);
				((VisualElement)m_tabScrollView).Add((VisualElement)(object)zbVipTabVipCardItem2);
			}
		}
		int num = -1;
		for (int i = 0; i < m_tabItems.Count; i++)
		{
			m_tabItems[i].Refresh();
			if (m_curTab == m_vipCardTabs[i])
			{
				num = i;
			}
		}
		if (num >= 0 && (m_needScroll | isFirstIn))
		{
			int indexToScroll = num;
			((VisualElement)m_tabScrollView).schedule.Execute((Action)delegate
			{
				zbUtil.ScrollHorizontalToIndex(m_tabScrollView, 2, ((VisualElement)m_tabScrollView).childCount, 310f, indexToScroll);
			}).ExecuteLater(50L);
		}
	}

	private void OnRight()
	{
		m_needScroll = true;
		int count = m_vipCardTabs.Count;
		if (count > 0)
		{
			m_curTabIndex = (m_curTabIndex + 1) % count;
			m_curTab = m_vipCardTabs[m_curTabIndex];
			m_panel.Refresh();
			RefreshScrollTo(isFirstIn: false);
		}
	}

	private void OnLeft()
	{
		m_needScroll = true;
		int count = m_vipCardTabs.Count;
		if (count > 0)
		{
			m_curTabIndex = (m_curTabIndex - 1 + count) % count;
			m_curTab = m_vipCardTabs[m_curTabIndex];
			m_panel.Refresh();
			RefreshScrollTo(isFirstIn: false);
		}
	}

	private void OnCall()
	{
		zbUIMgr.Instance.ShowSupportContact();
	}
}
