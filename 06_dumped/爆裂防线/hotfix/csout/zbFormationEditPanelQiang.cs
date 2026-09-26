using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationEditPanelQiang
{
	private zbFormationWnd m_wndParent;

	private VisualElement m_RootElement;

	private Button m_tabSuit;

	private Button m_tabGun;

	private ScrollView m_scrollViewSuit;

	private ScrollView m_scrollViewGun;

	private List<zbFormationSuitItem> m_itemSuits = new List<zbFormationSuitItem>();

	private List<zbFormationGunItem> m_itemGuns = new List<zbFormationGunItem>();

	private int m_curTab;

	public void OnEnable(zbFormationWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_tabSuit = UQueryExtensions.Q<Button>(m_RootElement, "tab_suit", (string)null);
		m_tabGun = UQueryExtensions.Q<Button>(m_RootElement, "tab_gun", (string)null);
		m_scrollViewSuit = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollViewSuit", (string)null);
		m_scrollViewGun = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollViewGun", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tabSuit, (Action<ClickEvent>)delegate
		{
			SelectTab(0);
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_tabGun, (Action<ClickEvent>)delegate
		{
			SelectTab(1);
		}, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		SelectTab(0);
	}

	private void SelectTab(int tabIndex)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		m_curTab = tabIndex;
		if (m_curTab == 0)
		{
			((VisualElement)m_tabSuit).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/formation/formation_suit1.png"));
			((VisualElement)m_tabGun).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/formation/formation_gun0.png"));
			((VisualElement)m_scrollViewSuit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_scrollViewGun).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			RefreshScrollViewSuit();
		}
		else
		{
			((VisualElement)m_tabSuit).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/formation/formation_suit0.png"));
			((VisualElement)m_tabGun).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/formation/formation_gun1.png"));
			((VisualElement)m_scrollViewSuit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_scrollViewGun).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshScrollViewGun();
		}
	}

	private void RefreshScrollViewSuit()
	{
		List<zbPlayerSuitCsvRecord> suitRecords = GetSuitRecords();
		if (suitRecords.Count != m_itemSuits.Count)
		{
			m_itemSuits.Clear();
			((VisualElement)m_scrollViewSuit).Clear();
			foreach (zbPlayerSuitCsvRecord item in suitRecords)
			{
				_ = item;
				zbFormationSuitItem zbFormationSuitItem2 = new zbFormationSuitItem(m_wndParent);
				m_itemSuits.Add(zbFormationSuitItem2);
				((VisualElement)m_scrollViewSuit).Add((VisualElement)(object)zbFormationSuitItem2);
			}
		}
		for (int i = 0; i < m_itemSuits.Count; i++)
		{
			m_itemSuits[i].Refresh(suitRecords[i]);
		}
	}

	private void RefreshScrollViewGun()
	{
		List<zbPlayerGunCsvRecord> gunRecords = GetGunRecords();
		if (gunRecords.Count != m_itemGuns.Count)
		{
			m_itemGuns.Clear();
			((VisualElement)m_scrollViewGun).Clear();
			foreach (zbPlayerGunCsvRecord item in gunRecords)
			{
				_ = item;
				zbFormationGunItem zbFormationGunItem2 = new zbFormationGunItem(m_wndParent);
				m_itemGuns.Add(zbFormationGunItem2);
				((VisualElement)m_scrollViewGun).Add((VisualElement)(object)zbFormationGunItem2);
			}
		}
		for (int i = 0; i < m_itemGuns.Count; i++)
		{
			m_itemGuns[i].Refresh(gunRecords[i]);
		}
	}

	public List<zbPlayerSuitCsvRecord> GetSuitRecords()
	{
		List<zbPlayerSuitCsvRecord> list = new List<zbPlayerSuitCsvRecord>();
		List<zbPlayerSuitCsvRecord> list2 = new List<zbPlayerSuitCsvRecord>();
		List<zbPlayerSuitCsvRecord> list3 = new List<zbPlayerSuitCsvRecord>();
		foreach (KeyValuePair<int, zbPlayerSuitCsvRecord> record in zbDataConfig.Instance.playerSuitCsvTable.GetRecords().records)
		{
			if (record.Value.id != 1 && zbVersionMgr.IsReleased(record.Value.preShow))
			{
				if (zbSelf.Instance.suitGunHandler.GetPlayerSuitDetail(record.Value.id) != null)
				{
					list2.Add(record.Value);
				}
				else
				{
					list3.Add(record.Value);
				}
			}
		}
		list2.Sort((zbPlayerSuitCsvRecord a, zbPlayerSuitCsvRecord b) => a.sort.CompareTo(b.sort));
		list3.Sort((zbPlayerSuitCsvRecord a, zbPlayerSuitCsvRecord b) => a.sort.CompareTo(b.sort));
		list.AddRange(list2);
		list.AddRange(list3);
		return list;
	}

	public List<zbPlayerGunCsvRecord> GetGunRecords()
	{
		List<zbPlayerGunCsvRecord> list = new List<zbPlayerGunCsvRecord>();
		foreach (KeyValuePair<int, zbPlayerGunCsvRecord> record in zbDataConfig.Instance.playerGunCsvTable.GetRecords().records)
		{
			if (record.Value.id != 2 && zbVersionMgr.IsReleased(record.Value.preShow))
			{
				list.Add(record.Value);
			}
		}
		return list;
	}
}
