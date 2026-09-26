using System;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMonsterWnd
{
	private zbEncyclopediaWnd m_wndParent;

	private VisualElement m_RootElement;

	private MultiColumnListView[] m_tabLayers = (MultiColumnListView[])(object)new MultiColumnListView[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private zbMonsterList[] m_moudles = new zbMonsterList[3];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[3];

	private Label m_lbDesc;

	private Label m_lbTitle;

	private Sprite m_select;

	private Sprite m_unSelect;

	public zbHandbookCsvRecord curActiveRecord;

	private int m_currTab;

	public int CurrentTab => m_currTab;

	public void OnEnable(zbEncyclopediaWnd wndParent, VisualElement root)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_tab" + i, (string)null);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i], (Vector2?)new Vector2(10f, 7f), 1f, true);
		}
		m_select = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_260x90_1.png");
		m_unSelect = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_260x90_2.png");
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Handbook, RefreshUI);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_MonsterActive_Ack, OnMonsterActiveAck);
		Button[] tabButtons = m_tabButtons;
		VisualElement[] tabLayers = (VisualElement[])(object)m_tabLayers;
		InitLocalTabs(tabButtons, tabLayers, OnSelect);
		m_wndParent.CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
	}

	private void RefreshUI(object obj)
	{
		Refresh();
	}

	private void OnMonsterActiveAck(object obj)
	{
		RepeatedField<MonsterHandbookDetail> val = (zbServerData.sGameData?.HandbookInfo)?.MonsterList;
		if (val == null)
		{
			Refresh();
			return;
		}
		if (curActiveRecord != null && !zbUIMgr.Instance.IsShow(EWndId.MonsterDetail))
		{
			foreach (MonsterHandbookDetail item in val)
			{
				if (item.HandbookId == curActiveRecord.id)
				{
					zbUIMgr.Instance.ShowMonsterDetail(curActiveRecord, item);
					break;
				}
			}
		}
		Refresh();
	}

	public void Refresh()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		DoSelectTab();
		for (int i = 0; i < 3; i++)
		{
			bool flag = zbSelf.Instance.handbookHandler.CanMonsterTypeActive(i + 1);
			m_notifys[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		}
	}

	private void OnSelect(int index)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground((index == i) ? m_select : m_unSelect);
		}
	}

	public void DoSelectTab(bool scrollToReward = false)
	{
		if (m_moudles[m_currTab] == null)
		{
			m_moudles[m_currTab] = new zbMonsterList(m_tabLayers[m_currTab], this, m_wndParent);
		}
		m_moudles[m_currTab].Refresh();
		if (scrollToReward)
		{
			m_moudles[m_currTab].ScrollToCanGetReward();
		}
	}

	private void InitLocalTabs(Button[] tabButtons, VisualElement[] layers, Action<int> onSelect, int defaultIndex = 0)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < layers.Length; i++)
		{
			if (layers[i] != null)
			{
				layers[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
		for (int j = 0; j < tabButtons.Length; j++)
		{
			int idx = j;
			if (tabButtons[idx] != null)
			{
				m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)tabButtons[idx], (Action<ClickEvent>)delegate
				{
					SelectTab(idx, layers, onSelect);
				}, "", (TrickleDown)0);
			}
		}
		SelectTab(Mathf.Clamp(defaultIndex, 0, tabButtons.Length - 1), layers, onSelect);
	}

	private void SelectTab(int index, VisualElement[] layers, Action<int> onSelect)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (index == m_currTab && layers[index].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			onSelect?.Invoke(index);
			DoSelectTab(scrollToReward: true);
			return;
		}
		m_currTab = index;
		for (int i = 0; i < layers.Length; i++)
		{
			if (layers[i] != null)
			{
				layers[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i != index) ? 1 : 0));
			}
		}
		onSelect?.Invoke(index);
		DoSelectTab(scrollToReward: true);
	}
}
