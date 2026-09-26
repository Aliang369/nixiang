using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasureWnd : zbTabWnd
{
	private GameObject m_treasureBg;

	public SkeletonAnimation m_barSpine;

	public SkeletonAnimation m_barChoukaSpine;

	public GameObject m_oneObject;

	public SpriteRenderer m_spriteTreasure;

	private VisualElement m_nodeTab;

	private VisualElement m_spriteTabSel;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	public Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[2];

	public zbTreasureTabList m_moduleList;

	public zbTreasureTabBar m_moduleBar;

	private VisualElement m_wndBottom;

	private Button m_btnMineWar;

	private Button m_btnClose;

	public Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_treasureBg = ((Component)m_GameObjectSceneRoot.transform.Find("treasureBg")).gameObject;
		m_barSpine = ((Component)m_GameObjectSceneRoot.transform.Find("barSpine")).GetComponent<SkeletonAnimation>();
		m_barChoukaSpine = ((Component)m_GameObjectSceneRoot.transform.Find("barChoukaSpine")).GetComponent<SkeletonAnimation>();
		m_oneObject = ((Component)m_GameObjectSceneRoot.transform.Find("oneObject")).gameObject;
		m_spriteTreasure = ((Component)m_oneObject.transform.Find("treasure")).GetComponent<SpriteRenderer>();
		m_nodeTab = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i]);
		}
		m_wndBottom = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_bottom", (string)null);
		m_btnMineWar = UQueryExtensions.Q<Button>(m_RootElement, "btn_mineWar", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMineWar, (Action<ClickEvent>)OnMineWar, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_TreasureInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
	}

	public void RefreshCustom(int tabIndex)
	{
		RefreshNotify();
		CheckShouldConvert();
		SelectTab(tabIndex);
		if (!zbGuide.Instance.GetGuideBool(EGuideStep.guide_step_Treasure_2) && zbSelf.Instance.funHandler.IsNeedGuide(EFuncType.Treasure))
		{
			zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Treasure_2);
			zbGuide.Instance.ExecGuide(EGuideStep.guide_step_Treasure_2);
		}
	}

	public void SetUIVisible(bool vis)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		m_moneyPanel.SetDisplayStyle((DisplayStyle)(!vis));
		m_nodeTab.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
		m_wndBottom.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!vis));
	}

	private void CheckShouldConvert()
	{
		List<TreasureDetail> treasureShouldConvert = zbSelf.Instance.treasureHandler.GetTreasureShouldConvert();
		if (treasureShouldConvert.Count > 0)
		{
			zbUIMgr.Instance.ShowTreasureConvert(treasureShouldConvert.ToArray());
		}
	}

	public override void DoSelectTab()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		m_treasureBg.gameObject.SetActive(m_currTab == 0);
		((Component)m_barSpine).gameObject.SetActive(m_currTab == 1);
		((Component)m_barChoukaSpine).gameObject.SetActive(false);
		m_oneObject.gameObject.SetActive(false);
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		switch (m_currTab)
		{
		case 0:
			if (m_moduleList == null)
			{
				m_moduleList = new zbTreasureTabList();
				m_moduleList.OnEnable(this, m_tabLayers[0]);
			}
			m_moduleList.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			CreateMoneyPanel(new EMoneyType[0]);
			break;
		case 1:
			if (m_moduleBar == null)
			{
				m_moduleBar = new zbTreasureTabBar();
				m_moduleBar.OnEnable(this, m_tabLayers[1]);
			}
			m_moduleBar.Refresh();
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			CreateMoneyPanel(new EMoneyType[2]
			{
				EMoneyType.BarTreasurePlus,
				EMoneyType.BarTreasure
			});
			break;
		}
	}

	private void RefreshNotify()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		m_notifys[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasTreasureNofity()));
		m_notifys[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.barHandler.HasTreasureBarRedPoint()));
	}

	private void OnMineWar(ClickEvent evt)
	{
		zbLinkHelper.OpenMineWar();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnTouch(ClickEvent evt)
	{
		if (m_currTab == 0)
		{
			m_moduleList.OnTouch();
		}
		else
		{
			m_moduleBar.OnTouch();
		}
	}

	private void OnEventUpdate(object data)
	{
		DoSelectTab();
		RefreshNotify();
	}
}
