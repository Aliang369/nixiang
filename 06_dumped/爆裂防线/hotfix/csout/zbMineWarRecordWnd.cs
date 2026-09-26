using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarRecordWnd : zbTabWnd
{
	private VisualTreeAsset m_itemTA;

	private SpriteRenderer m_spriteFoothold;

	private zbPlayerAvatar m_spineAvatar;

	private zbArenaBottom m_bottomPanel;

	private ListView[] m_listView = (ListView[])(object)new ListView[2];

	private VisualElement m_NoListView;

	private Button m_btnClose;

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[2];

	private VisualElement m_spriteTabSel;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mineWar/tb_mineWarRecord.uxml");
		m_spriteFoothold = ((Component)m_GameObjectSceneRoot.transform.Find("bottomBg").Find("foothold")).GetComponent<SpriteRenderer>();
		m_spineAvatar = ((Component)((Component)m_spriteFoothold).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
		m_bottomPanel = new zbArenaBottom();
		m_bottomPanel = UQueryExtensions.Q<zbArenaBottom>(m_RootElement, "arenaBottom", (string)null);
		m_bottomPanel.OnEnable(this, m_spriteFoothold, m_spineAvatar);
		m_NoListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTabbg", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(val, "tab_btn" + i, (string)null);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i]);
			m_listView[i] = UQueryExtensions.Q<ListView>(m_tabLayers[i], "listView", (string)null);
			((BaseVerticalCollectionView)m_listView[i]).virtualizationMethod = (CollectionVirtualizationMethod)0;
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView[i], (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
			UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView[i], (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		}
		InitTab(m_tabButtons, m_tabLayers, null);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_MineWarRecordList, UpdateListView);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_ReminderList, RefreshNotify);
	}

	public void RefreshCustom()
	{
		RefreshNotify();
	}

	private void RefreshNotify(object data = null)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		m_notifys[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.MineRecord)));
		m_notifys[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.CrossMineRecord)));
	}

	public override void DoSelectTab()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		switch (m_currTab)
		{
		case 0:
			if ((float)zbSelf.Instance.timeHandler.GetServerTime() - zbSelf.Instance.mineWarHandler.mineWarRecordTime > 60f)
			{
				zbWsMsgHandler.Instance.MineWar_RecordList_Req();
			}
			else
			{
				UpdateListView(null);
			}
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if ((float)zbSelf.Instance.timeHandler.GetServerTime() - zbSelf.Instance.mineWarHandler.crossMineRecordTime > 60f)
			{
				zbWsMsgHandler.Instance.CrossMine_RecordList_Req();
			}
			else
			{
				UpdateListView(null);
			}
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			break;
		}
	}

	private void UpdateListView(object data)
	{
		m_bottomPanel.Refresh();
		bool flag = ((m_currTab != 0) ? (zbSelf.Instance.mineWarHandler.crossMineRecordList.Length != 0) : (zbSelf.Instance.mineWarHandler.mineWarRecordList.Length != 0));
		((VisualElement)m_listView[m_currTab]).Clear();
		((VisualElement)m_listView[m_currTab]).visible = flag;
		m_NoListView.visible = !flag;
		m_listView[m_currTab].makeItem = () => (VisualElement)(object)new zbMineWarRecordItem(m_itemTA, this);
		m_listView[m_currTab].bindItem = delegate(VisualElement element, int index)
		{
			zbMineWarRecordItem zbMineWarRecordItem2 = element as zbMineWarRecordItem;
			if (m_currTab == 0)
			{
				zbMineWarRecordItem2.Init(zbSelf.Instance.mineWarHandler.mineWarRecordList[index], null);
			}
			else
			{
				zbMineWarRecordItem2.Init(null, zbSelf.Instance.mineWarHandler.crossMineRecordList[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView[m_currTab]).itemsSource = null;
		if (m_currTab == 0)
		{
			((BaseVerticalCollectionView)m_listView[m_currTab]).itemsSource = zbSelf.Instance.mineWarHandler.mineWarRecordList;
		}
		else
		{
			((BaseVerticalCollectionView)m_listView[m_currTab]).itemsSource = zbSelf.Instance.mineWarHandler.crossMineRecordList;
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
