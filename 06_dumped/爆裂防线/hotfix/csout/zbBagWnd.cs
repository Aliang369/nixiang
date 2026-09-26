using System;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBagWnd : zbTabWnd
{
	private Button m_btnClose;

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[4];

	private Button[] m_tabButtons = (Button[])(object)new Button[4];

	private zbBagTab[] m_panels = new zbBagTab[4];

	private VisualElement m_notifyProp;

	private VisualElement m_notifyBox;

	private Sprite m_selectedIcon;

	private Sprite m_unSelectedIcon;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_selectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_160x73_0.png");
		m_unSelectedIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_160x73_1.png");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 4; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
		}
		m_notifyProp = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[2], (Vector2?)new Vector2(-3f, -10f), 1f, true);
		m_notifyBox = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[3], (Vector2?)new Vector2(-3f, -10f), 1f, true);
		InitTab(m_tabButtons, m_tabLayers, OnSelect);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Box, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Prop, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Gene, OnEventUpdate);
	}

	public void RefreshCustom(int tabIndex)
	{
		base.Refresh();
		RefreshNotify();
		SelectTab(tabIndex);
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		m_notifyProp.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.itemHandler.HasPropWorkerNotify()));
		m_notifyBox.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.itemHandler.HasAnyBoxNotify()));
	}

	private void OnSelect(int index)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_tabButtons.Count(); i++)
		{
			bool flag = i == index;
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground(flag ? m_selectedIcon : m_unSelectedIcon);
		}
	}

	public override void DoSelectTab()
	{
		if (m_panels[m_currTab] == null)
		{
			m_panels[m_currTab] = new zbBagTab();
			m_panels[m_currTab].OnEnable(this, m_tabLayers[m_currTab], m_currTab);
		}
		m_panels[m_currTab].Refresh();
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		DoSelectTab();
		RefreshNotify();
	}
}
