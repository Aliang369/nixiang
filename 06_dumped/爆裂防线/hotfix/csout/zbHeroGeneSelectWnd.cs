using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroGeneSelectWnd : zbTabWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_tabButtons = (Button[])(object)new Button[5];

	private VisualElement[] m_tabIcons = (VisualElement[])(object)new VisualElement[5];

	private zbHeroGeneSelectTab[] m_panels = new zbHeroGeneSelectTab[5];

	private Button m_btnTip;

	private VisualElement m_nodeTip;

	private Button m_btnTipTouch;

	private VisualElement m_nodeGetWay;

	private zbItem[] m_items = new zbItem[6];

	private Sprite m_selectedIcon;

	private Sprite m_unSelectedIcon;

	public GeneSlotDetail m_geneSlotDetail;

	public ESkillType m_skillType;

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
		m_btnTip = UQueryExtensions.Q<Button>(m_RootElement, "btn_tip", (string)null);
		m_nodeTip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_btnTipTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_tipTouch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTip, (Action<ClickEvent>)OnTip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTipTouch, (Action<ClickEvent>)OnTipTouch, "", (TrickleDown)0);
		m_nodeGetWay = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_getWay", (string)null);
		for (int j = 0; j < 6; j++)
		{
			m_items[j] = UQueryExtensions.Q<zbItem>(m_nodeGetWay, "item" + j, (string)null);
			m_items[j].OnEnable(this);
		}
		InitTab(m_tabButtons, m_tabLayers, OnSelect);
		AddDark();
	}

	public void RefreshCustom(GeneSlotDetail geneSlotDetail, ESkillType skillType, int index)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		SetTipVisible(vis: false);
		RefreshGetWay();
		for (int i = 0; i < 5; i++)
		{
			((VisualElement)m_tabButtons[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_geneSlotDetail = geneSlotDetail;
		m_skillType = skillType;
		m_index = index;
		if (geneSlotDetail != null)
		{
			m_selectTabIndex = (int)(m_geneSlotDetail.GeneShape - 1);
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
			m_panels[m_currTab] = new zbHeroGeneSelectTab();
			m_panels[m_currTab].OnEnable(this, m_tabLayers[m_currTab], m_currTab);
		}
		m_panels[m_currTab].Refresh();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnTip(ClickEvent evt)
	{
		SetTipVisible(vis: true);
	}

	private void OnTipTouch(ClickEvent evt)
	{
		SetTipVisible(vis: false);
	}

	private void SetTipVisible(bool vis)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		m_nodeTip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		((VisualElement)m_btnTipTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	private void RefreshGetWay()
	{
		for (int i = 0; i < 6; i++)
		{
			m_items[i].Refresh(new zbItemData(EResourceType.Box, 5005001 + i, 1L), isNameVis: false, isCountVis: false);
		}
	}
}
