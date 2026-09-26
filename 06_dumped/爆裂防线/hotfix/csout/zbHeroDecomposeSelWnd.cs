using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroDecomposeSelWnd : zbTabWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private VisualElement m_listNo;

	private MultiColumnListView[] m_tabLayers = (MultiColumnListView[])(object)new MultiColumnListView[4];

	private Button[] m_tabButtons = (Button[])(object)new Button[4];

	private zbHeroDecomposeSelList[] m_panels = new zbHeroDecomposeSelList[4];

	public VisualElement listNo;

	private Button m_btnConfirm;

	private Sprite m_selectedIcon;

	private Sprite m_unSelectedIcon;

	private zbHeroDecomposeWnd m_wndHeroDecompose;

	public List<ResourceDetail> decomposeList = new List<ResourceDetail>();

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_selectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_96x50_0.png");
		m_unSelectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_96x50_1.png");
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listNo = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		for (int i = 0; i < 4; i++)
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

	public void RefreshCustom(zbHeroDecomposeWnd wndHeroDecompose)
	{
		decomposeList.Clear();
		m_wndHeroDecompose = wndHeroDecompose;
		foreach (ResourceDetail decompose in m_wndHeroDecompose.decomposeList)
		{
			if (decompose != null)
			{
				decomposeList.Add(decompose);
			}
		}
		DoSelectTab();
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
			m_panels[m_currTab] = new zbHeroDecomposeSelList();
			m_panels[m_currTab].OnEnable(this, m_tabLayers[m_currTab]);
		}
		m_panels[m_currTab].Refresh();
	}

	public long GetSelCount(long id)
	{
		foreach (ResourceDetail decompose in decomposeList)
		{
			if (decompose.Tid == id)
			{
				return decompose.Count;
			}
		}
		return 0L;
	}

	public void RefreshDecomposeList(int option, ResourceDetail data)
	{
		if (option == 1)
		{
			foreach (ResourceDetail decompose in decomposeList)
			{
				if (decompose.Tid == data.Tid)
				{
					decomposeList.Remove(decompose);
					break;
				}
			}
			return;
		}
		decomposeList.Add(data);
	}

	public void AddToDecomposeList(ResourceDetail data, long count)
	{
		ResourceDetail data2 = new ResourceDetail
		{
			Tid = data.Tid,
			Count = count
		};
		RefreshDecomposeList(2, data2);
	}

	private void OnConfirm(ClickEvent evt)
	{
		m_wndHeroDecompose.decomposeList.Clear();
		foreach (ResourceDetail decompose in decomposeList)
		{
			m_wndHeroDecompose.decomposeList.Add(decompose);
		}
		m_wndHeroDecompose.RefreshDecomposeList();
		Hide();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
