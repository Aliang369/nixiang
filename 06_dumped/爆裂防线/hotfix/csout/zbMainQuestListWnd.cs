using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestListWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Label m_lbDaytimeBg;

	private Label m_lbNightBg;

	private ListView m_listView;

	private Label m_lbLevelName;

	private Button m_btn_goTO;

	private Button m_btn_daytime;

	private VisualElement m_dayTimeRedDot;

	private Button m_btn_night;

	private VisualElement m_nightRedDot;

	private Label m_lbDesc;

	private int m_imperfectLevelID;

	private List<int> rowIndexes = new List<int>();

	private bool m_IsMain;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_QuestItem.uxml");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btn_goTO = UQueryExtensions.Q<Button>(m_RootElement, "btn_goTO", (string)null);
		m_btn_daytime = UQueryExtensions.Q<Button>(m_RootElement, "btn_daytime", (string)null);
		m_dayTimeRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btn_daytime, "red_dot", (string)null);
		m_btn_night = UQueryExtensions.Q<Button>(m_RootElement, "btn_night", (string)null);
		m_nightRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btn_night, "red_dot", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "list", (string)null);
		m_lbLevelName = UQueryExtensions.Q<Label>(m_RootElement, "lb_levelName", (string)null);
		m_lbDaytimeBg = UQueryExtensions.Q<Label>(m_RootElement, "lb_daytimeBg", (string)null);
		m_lbNightBg = UQueryExtensions.Q<Label>(m_RootElement, "lb_nightBg", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 166f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn_goTO, (Action<ClickEvent>)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn_daytime, (Action)OnDayTime, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn_night, (Action)OnNight, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Quest_Refresh, OnEventUpdateQuestList);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
	}

	private void OnEventUpdateQuestList(object obj)
	{
		RefreshQuestList();
	}

	public void RefreshCustom(int tabIndex)
	{
		base.Refresh();
		m_IsMain = tabIndex == 0;
		((VisualElement)m_lbDaytimeBg).visible = m_IsMain;
		((VisualElement)m_lbNightBg).visible = !m_IsMain;
		RefreshQuestList();
		ScrollToCurrentItem();
		m_imperfectLevelID = (m_IsMain ? zbSelf.Instance.mainQuestHandler.FindFirstImperfectLevel() : zbSelf.Instance.eliteQuestHandler.FindFirstImperfectLevel());
		((TextElement)m_lbLevelName).text = (m_IsMain ? zbResourceHelper.GetMainLevelName(m_imperfectLevelID) : zbResourceHelper.GetEliteLevelName(m_imperfectLevelID));
	}

	private void ScrollToCurrentItem()
	{
		int num = ((!m_IsMain) ? (((IEnumerable<MainQuestDetail>)zbServerData.sGameData.EliteQuestInfo.EliteList).FirstOrDefault((MainQuestDetail q) => q.Score > EBattleResultScore.Fail && q.Score - q.ScoreAward > 0)?.LevelId ?? zbServerData.sGameData.EliteQuestInfo.EliteLevel) : (((IEnumerable<MainQuestDetail>)zbServerData.sGameData.MainQuestInfo.QuestList).FirstOrDefault((MainQuestDetail q) => q.Score > EBattleResultScore.Fail && q.Score - q.ScoreAward > 0)?.LevelId ?? zbServerData.sGameData.MainQuestInfo.MainLevel));
		((BaseVerticalCollectionView)m_listView).ScrollToItemById(num);
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		m_dayTimeRedDot.visible = zbSelf.Instance.mainQuestHandler.HasRedPoint();
		m_nightRedDot.visible = zbSelf.Instance.eliteQuestHandler.HasRedPoint();
	}

	private void RefreshQuestList()
	{
		((VisualElement)m_listView).Clear();
		rowIndexes.Clear();
		int count = zbDataConfig.Instance.mainLevelCsvTable.GetRecords().records.Count;
		int count2 = zbDataConfig.Instance.eliteLevelCsvTable.GetRecords().records.Count;
		int num = (m_IsMain ? Mathf.Min(zbServerData.sGameData.MainQuestInfo.MainLevel + 3, count) : Mathf.Min(zbServerData.sGameData.EliteQuestInfo.EliteLevel + 3, count2));
		for (int i = 0; i < num; i++)
		{
			rowIndexes.Add(i);
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbQuestListItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbQuestListItem).Init(index, m_IsMain);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rowIndexes;
	}

	public void OnFight(ClickEvent evt)
	{
		if (m_IsMain)
		{
			zbUIMgr.Instance.ShowMainLevel(0, m_imperfectLevelID);
		}
		else
		{
			zbUIMgr.Instance.ShowEliteQuest(m_imperfectLevelID);
		}
	}

	private void OnDayTime()
	{
		RefreshCustom(0);
	}

	private void OnNight()
	{
		if (!zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.EliteQuest))
		{
			RefreshCustom(1);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
