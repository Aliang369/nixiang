using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHolidayTaskChallenge
{
	private VisualTreeAsset m_itemTA;

	private zbHolidayTaskWnd m_wndParent;

	private ListView m_listView;

	private Button[] m_btnDays = (Button[])(object)new Button[6];

	private VisualElement[] m_spriteSelects = (VisualElement[])(object)new VisualElement[6];

	private VisualElement[] m_spriteLocks = (VisualElement[])(object)new VisualElement[6];

	private VisualElement[] m_spriteEnds = (VisualElement[])(object)new VisualElement[6];

	private VisualElement[] m_notifys = (VisualElement[])(object)new VisualElement[6];

	private Sprite m_openIcon;

	private Sprite m_lockIcon;

	private int m_curIndex = -1;

	private int m_today;

	public void OnEnable(zbHolidayTaskWnd wndParent, VisualElement root)
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_holidayTaskChallengeItem.uxml");
		m_openIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/holidayTask/holidayTask_tab1.png");
		m_lockIcon = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/holidayTask/holidayTask_tab0.png");
		m_wndParent = wndParent;
		m_listView = UQueryExtensions.Q<ListView>(root, "ListView", (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 116f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		for (int i = 0; i < 6; i++)
		{
			m_btnDays[i] = UQueryExtensions.Q<Button>(root, "btn_day" + i, (string)null);
			m_spriteSelects[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnDays[i], "select", (string)null);
			m_spriteLocks[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnDays[i], "lock", (string)null);
			m_spriteEnds[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnDays[i], "end", (string)null);
			m_notifys[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnDays[i], (Vector2?)new Vector2(-10f, -10f), 1f, true);
			int tempIndex = i;
			wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDays[i], (Action)delegate
			{
				OnSelect(tempIndex);
			}, "", (TrickleDown)0);
		}
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
	}

	public void Refresh()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_data != null)
		{
			m_today = zbSelf.Instance.operateHandler.GetHolidayTaskToday(m_wndParent.m_data.BaseInfo.UUID);
			if (m_curIndex > -1)
			{
				OnSelect(m_curIndex);
			}
			else
			{
				OnSelect(m_today);
			}
			for (int i = 0; i < 6; i++)
			{
				m_spriteLocks[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i <= m_today) ? 1 : 0));
				((VisualElement)m_btnDays[i]).style.backgroundImage = new StyleBackground((i >= m_today) ? m_lockIcon : m_openIcon);
				m_spriteEnds[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_today < i + 1 || !IsCompleteByDay(i)) ? 1 : 0));
				m_notifys[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasTaskGiftByDay(m_wndParent.m_data.BaseInfo.UUID, i)));
			}
		}
	}

	private void UpdateListView(TaskDetail[] taskList)
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbHolidayTaskChallengeItem(m_wndParent, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbHolidayTaskChallengeItem).Refresh(taskList[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = taskList;
	}

	private void GenerateData(int day)
	{
		List<TaskDetail> list = new List<TaskDetail>();
		if (m_wndParent.m_data != null)
		{
			foreach (TaskDetail task in m_wndParent.m_data.TaskList)
			{
				zbHolidayTaskTaskCsvRecord recordByKey = zbDataConfig.Instance.holidayTaskTaskCsvTable.GetRecordByKey(task.TaskId);
				if (recordByKey != null && recordByKey.day == day)
				{
					list.Add(task);
				}
			}
		}
		List<TaskDetail> list2 = list.OrderBy((TaskDetail t) => (t.State != EActiveState.Active) ? ((t.State == EActiveState.Pending) ? 1 : 2) : 0).ToList();
		UpdateListView(list2.ToArray());
	}

	private void OnSelect(int index)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (index > m_today)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10273"), index + 1));
			return;
		}
		m_curIndex = Math.Min(index, 5);
		for (int i = 0; i < 6; i++)
		{
			m_spriteSelects[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i != m_curIndex) ? 1 : 0));
		}
		GenerateData(m_curIndex + 1);
	}

	private bool IsCompleteByDay(int index)
	{
		if (m_wndParent.m_data == null)
		{
			return false;
		}
		foreach (TaskDetail task in m_wndParent.m_data.TaskList)
		{
			zbHolidayTaskTaskCsvRecord recordByKey = zbDataConfig.Instance.holidayTaskTaskCsvTable.GetRecordByKey(task.TaskId);
			if (recordByKey != null && recordByKey.day == index + 1 && task.State != EActiveState.Done)
			{
				return false;
			}
		}
		return true;
	}
}
