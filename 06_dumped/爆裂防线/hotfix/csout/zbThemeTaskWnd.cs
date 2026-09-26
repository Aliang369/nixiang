using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbThemeTaskWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Label m_lbTime;

	private VisualElement m_spriteBanner;

	private ScrollView m_targetListView;

	private ListView m_listView;

	private Label m_lbExp;

	private Button m_btnClose;

	private List<OperateTargetDetail> m_TargetListViewDatas = new List<OperateTargetDetail>();

	private List<TaskDetail> m_ListViewDatas = new List<TaskDetail>();

	private ThemeTaskDetail m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_themeTask.uxml");
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_targetListView = UQueryExtensions.Q<ScrollView>(m_RootElement, "targetListView", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_lbExp = UQueryExtensions.Q<Label>(m_RootElement, "lb_exp", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEvent);
	}

	public void RefreshCustom(ThemeTaskDetail data, bool isFirstIn = true)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		RefreshLeftTime();
		GetListViewDatas();
		UpdateListView();
		RefreshTarget();
		if (isFirstIn)
		{
			zbWsMsgHandler.Instance.Operate_ThemeTaskData_Req();
			if (zbSelf.Instance.restaurantHandler.GetRestaurantDetail() != null)
			{
				m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/themeTask/themeTask_restaurant.jpg"));
			}
			else if (zbSelf.Instance.endlessMineHandler.GetEndlessMineDetail() != null)
			{
				m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/operate/themeTask/themeTask_endlessMine.jpg"));
			}
		}
	}

	private void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbThemeTaskItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbThemeTaskItem).Refresh(m_ListViewDatas[index], m_data.BaseInfo.UUID);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_ListViewDatas;
	}

	private void GetListViewDatas()
	{
		m_ListViewDatas.Clear();
		foreach (TaskDetail task in m_data.TaskList)
		{
			m_ListViewDatas.Add(task);
		}
		m_ListViewDatas.Sort(delegate(TaskDetail v1, TaskDetail v2)
		{
			TaskDetail taskDetail = GetTaskDetail(v1.TaskId);
			TaskDetail taskDetail2 = GetTaskDetail(v2.TaskId);
			if (taskDetail != null && taskDetail2 != null)
			{
				if (taskDetail.State == EActiveState.Active && taskDetail2.State == EActiveState.Active)
				{
					return v1.TaskId.CompareTo(v2.TaskId);
				}
				if (taskDetail.State == EActiveState.Active)
				{
					return -1;
				}
				if (taskDetail2.State == EActiveState.Active)
				{
					return 1;
				}
				if (taskDetail.State == EActiveState.Done && taskDetail2.State == EActiveState.Done)
				{
					return v1.TaskId.CompareTo(v2.TaskId);
				}
				if (taskDetail.State == EActiveState.Done)
				{
					return 1;
				}
				if (taskDetail2.State == EActiveState.Done)
				{
					return -1;
				}
				return v1.TaskId.CompareTo(v2.TaskId);
			}
			return 0;
		});
	}

	private TaskDetail GetTaskDetail(int taskId)
	{
		foreach (TaskDetail task in m_data.TaskList)
		{
			if (taskId == task.TaskId)
			{
				return task;
			}
		}
		return null;
	}

	private void RefreshTarget()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbExp).text = m_data.TotalExp.ToString();
		((VisualElement)m_targetListView).Clear();
		foreach (OperateTargetDetail target in m_data.TargetList)
		{
			m_TargetListViewDatas.Add(target);
		}
		foreach (OperateTargetDetail target2 in m_data.TargetList)
		{
			zbThemeTaskTargetItem zbThemeTaskTargetItem2 = new zbThemeTaskTargetItem(this);
			zbThemeTaskTargetItem2.Refresh(target2, m_data, target2.Index - 1);
			((VisualElement)zbThemeTaskTargetItem2).style.marginRight = StyleLength.op_Implicit(-60f);
			((VisualElement)m_targetListView).Add((VisualElement)(object)zbThemeTaskTargetItem2);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void RefreshLeftTime()
	{
		if (m_data != null)
		{
			long seconds = m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(seconds);
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnUpdateEvent(object data)
	{
		if (m_data != null)
		{
			ThemeTaskDetail themeTaskInfoByUUID = zbSelf.Instance.operateHandler.GetThemeTaskInfoByUUID(m_data.BaseInfo.UUID);
			RefreshCustom(themeTaskInfoByUUID, isFirstIn: false);
		}
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.Operate_ThemeTaskData_Req();
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		zbWsMsgHandler.Instance.Operate_ThemeTaskData_Req();
	}
}
