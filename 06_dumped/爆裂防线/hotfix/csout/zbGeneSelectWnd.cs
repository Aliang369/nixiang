using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneSelectWnd : zbTabWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_tabButtons = (Button[])(object)new Button[5];

	private VisualElement[] m_tabIcons = (VisualElement[])(object)new VisualElement[5];

	private zbGeneSelectTab[] m_panels = new zbGeneSelectTab[5];

	private Sprite m_selectedIcon;

	private Sprite m_unSelectedIcon;

	public int m_geneTid;

	public int m_index;

	public int m_selectTabIndex;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_selectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_160x73_0.png");
		m_unSelectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_160x73_1.png");
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 5; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_tabIcons[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_tabButtons[i], "bg_shape", (string)null);
		}
		InitTab(m_tabButtons, m_tabLayers, OnSelect);
		AddDark();
	}

	public void RefreshCustom(int geneTid, int index)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_tabButtons[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_geneTid = geneTid;
		m_index = index;
		zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey(geneTid);
		if (recordByKey != null)
		{
			m_selectTabIndex = recordByKey.geneShape - 1;
			SelectTab(m_selectTabIndex);
		}
	}

	private void OnSelect(int index)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_tabButtons.Count(); i++)
		{
			bool flag = i == index;
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(flag ? m_selectedIcon : m_unSelectedIcon);
			m_tabIcons[index].style.opacity = StyleFloat.op_Implicit(flag ? 1f : 0.7f);
		}
	}

	public override void DoSelectTab()
	{
		if (m_panels[m_currTab] == null)
		{
			m_panels[m_currTab] = new zbGeneSelectTab();
			m_panels[m_currTab].OnEnable(this, m_tabLayers[m_currTab], m_currTab);
		}
		m_panels[m_currTab].Refresh();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
