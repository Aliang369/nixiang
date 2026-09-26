using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainLevelTaskPanel
{
	private zbMainLevelWnd m_wndParent;

	private Label m_lbTaskName;

	private Label m_lbTaskName2;

	private Label m_lbTaskProVal;

	private Label m_lbToUpgrade;

	private Label m_lbIsTopPeerage;

	private SpineVisualElement m_spineShuzi;

	private VisualElement m_progressBg;

	public VisualElement m_iconTaskGuide;

	public VisualElement m_taskBg;

	private VisualElement m_taskProgress;

	public Button m_btnTask;

	private Button m_btnTaskTouch;

	private bool m_shouldScrollTaskName;

	private float m_TaskNameCurrentX;

	private float m_TaskTextWidth;

	private const float SCROLL_GAP = 100f;

	private const float SCROLL_SPEED = 40f;

	private const float TouchTimeDuration = 20f;

	public void OnEnable(zbMainLevelWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_taskBg = UQueryExtensions.Q<VisualElement>(root, "task_bg", (string)null);
		m_progressBg = UQueryExtensions.Q<VisualElement>(m_taskBg, "progress_bg", (string)null);
		m_btnTaskTouch = UQueryExtensions.Q<Button>(m_taskBg, "touch", (string)null);
		m_lbTaskName = UQueryExtensions.Q<Label>(m_taskBg, "lb_task", (string)null);
		m_lbTaskName2 = UQueryExtensions.Q<Label>(m_taskBg, "lb_task2", (string)null);
		m_lbTaskProVal = UQueryExtensions.Q<Label>(m_taskBg, "lb_proVal", (string)null);
		m_lbToUpgrade = UQueryExtensions.Q<Label>(m_taskBg, "lb_toUpgrade", (string)null);
		m_lbIsTopPeerage = UQueryExtensions.Q<Label>(m_taskBg, "lb_isTopPeerage", (string)null);
		m_iconTaskGuide = UQueryExtensions.Q<VisualElement>(root, "icon_taskGuide", (string)null);
		m_spineShuzi = UQueryExtensions.Q<SpineVisualElement>(root, "spine_shuzi", (string)null);
		m_btnTask = UQueryExtensions.Q<Button>(root, "btn_task", (string)null);
		m_taskProgress = UQueryExtensions.Q<VisualElement>(m_progressBg, "progress", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTaskTouch, (Action)OnTaskCheck, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTask, (Action)OnTask, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		RefreshTaskInfo();
	}

	private void RefreshTaskInfo()
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Unknown result type (might be due to invalid IL or missing references)
		//IL_068f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fd: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).All((TaskDetail t) => t.State == EActiveState.Done);
		bool flag2 = zbSelf.Instance.taskHandler.IsTopPeerage();
		bool flag3 = zbSelf.Instance.taskHandler.taskList.ToArray().All((TaskDetail t) => t.State == EActiveState.Done);
		m_spineShuzi.AnimationState.SetAnimation(0, zbServerData.sGameData.PeerageInfo.PeerageId.ToString(), false).TimeScale = 0f;
		((VisualElement)m_lbToUpgrade).visible = flag && !flag2;
		((VisualElement)m_lbTaskName).visible = !((VisualElement)m_lbToUpgrade).visible;
		((VisualElement)m_lbTaskName2).visible = !((VisualElement)m_lbToUpgrade).visible;
		((VisualElement)m_lbTaskProVal).visible = !((VisualElement)m_lbToUpgrade).visible;
		m_progressBg.visible = !((VisualElement)m_lbToUpgrade).visible;
		m_shouldScrollTaskName = ((VisualElement)m_lbTaskName).visible;
		((VisualElement)m_lbTaskName).style.translate = new StyleTranslate(new Translate(Length.op_Implicit(0f), Length.op_Implicit(0f), 0f));
		m_taskBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag2 & flag3) ? 1 : 0));
		TaskDetail taskDetail = (flag2 ? zbSelf.Instance.taskHandler.taskList.ToArray().FirstOrDefault((TaskDetail t) => t.State == EActiveState.Active) : ((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).FirstOrDefault((TaskDetail t) => t.State == EActiveState.Active));
		if (taskDetail != null)
		{
			int num = 0;
			int num2 = 0;
			if (flag2)
			{
				zbDailyTaskCsvRecord recordByKey = zbDataConfig.Instance.dailyTaskCsvTable.GetRecordByKey(taskDetail.TaskType);
				if (recordByKey == null)
				{
					return;
				}
				num = recordByKey.type;
				num2 = recordByKey.target;
			}
			else
			{
				zbPeerageTaskCsvRecord recordByKey2 = zbDataConfig.Instance.peerageTaskCsvTable.GetRecordByKey(taskDetail.TaskId);
				if (recordByKey2 == null)
				{
					return;
				}
				num = recordByKey2.type;
				num2 = recordByKey2.target;
			}
			zbTaskTypeCsvRecord recordByKey3 = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(num);
			if (taskDetail != null)
			{
				recordByKey3 = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(taskDetail.TaskType);
			}
			string desc = recordByKey3.desc;
			if (num2 > 0)
			{
				switch (num)
				{
				case 113:
				{
					string name = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(num2).name;
					((TextElement)m_lbTaskName).text = string.Format(desc, name);
					break;
				}
				case 118:
				case 119:
				case 142:
				{
					string arg = zbUtil.Language(zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(num2).name);
					((TextElement)m_lbTaskName).text = string.Format(desc, arg);
					break;
				}
				default:
					((TextElement)m_lbTaskName).text = string.Format(desc, num2);
					break;
				}
			}
			Vector2 val = ((TextElement)m_lbTaskName).MeasureTextSize(((TextElement)m_lbTaskName).text, 320f, (MeasureMode)0, 0f, (MeasureMode)0);
			m_TaskTextWidth = val.x;
			((VisualElement)m_lbTaskName2).visible = !((VisualElement)m_lbToUpgrade).visible && m_shouldScrollTaskName;
			((TextElement)m_lbTaskName2).text = ((TextElement)m_lbTaskName).text;
			((VisualElement)m_lbTaskName2).style.translate = new StyleTranslate(new Translate(Length.op_Implicit(m_TaskTextWidth + 100f), Length.op_Implicit(0f), 0f));
			((TextElement)m_lbTaskProVal).text = $"{taskDetail.Reach} / {taskDetail.Target}";
			m_taskProgress.style.width = new StyleLength(new Length(Mathf.Min((float)taskDetail.Reach / (float)taskDetail.Target * 100f, 100f), (LengthUnit)1));
			return;
		}
		TaskDetail taskDetail2 = (flag2 ? zbSelf.Instance.taskHandler.taskList.ToArray().FirstOrDefault((TaskDetail t) => t.State == EActiveState.Pending) : ((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).FirstOrDefault((TaskDetail t) => t.State == EActiveState.Pending));
		if (taskDetail2 == null)
		{
			return;
		}
		int num3 = 0;
		int num4 = 0;
		if (flag2)
		{
			zbDailyTaskCsvRecord recordByKey4 = zbDataConfig.Instance.dailyTaskCsvTable.GetRecordByKey(taskDetail2.TaskType);
			if (recordByKey4 == null)
			{
				return;
			}
			num3 = recordByKey4.type;
			num4 = recordByKey4.target;
		}
		else
		{
			zbPeerageTaskCsvRecord recordByKey5 = zbDataConfig.Instance.peerageTaskCsvTable.GetRecordByKey(taskDetail2.TaskId);
			if (recordByKey5 == null)
			{
				return;
			}
			num3 = recordByKey5.type;
			num4 = recordByKey5.target;
		}
		zbTaskTypeCsvRecord recordByKey6 = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(num3);
		if (taskDetail2 != null)
		{
			recordByKey6 = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(taskDetail2.TaskType);
		}
		string desc2 = recordByKey6.desc;
		if (num4 > 0)
		{
			switch (num3)
			{
			case 113:
			{
				string name2 = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(num4).name;
				((TextElement)m_lbTaskName).text = string.Format(desc2, name2);
				break;
			}
			case 118:
			case 119:
			case 142:
			{
				string arg2 = zbUtil.Language(zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(num4).name);
				((TextElement)m_lbTaskName).text = string.Format(desc2, arg2);
				break;
			}
			default:
				((TextElement)m_lbTaskName).text = string.Format(desc2, num4);
				break;
			}
		}
		Vector2 val2 = ((TextElement)m_lbTaskName).MeasureTextSize(((TextElement)m_lbTaskName).text, 320f, (MeasureMode)0, 0f, (MeasureMode)0);
		m_TaskTextWidth = val2.x;
		((VisualElement)m_lbTaskName2).visible = !((VisualElement)m_lbToUpgrade).visible && m_shouldScrollTaskName;
		((TextElement)m_lbTaskName2).text = ((TextElement)m_lbTaskName).text;
		((VisualElement)m_lbTaskName2).style.translate = new StyleTranslate(new Translate(Length.op_Implicit(m_TaskTextWidth + 100f), Length.op_Implicit(0f), 0f));
		((TextElement)m_lbTaskProVal).text = $"{taskDetail2.Reach} / {taskDetail2.Target}";
		m_taskProgress.style.width = new StyleLength(new Length(Mathf.Min((float)taskDetail2.Reach / (float)taskDetail2.Target * 100f, 100f), (LengthUnit)1));
	}

	public void OnUpdateTask()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (m_shouldScrollTaskName)
		{
			m_TaskNameCurrentX -= 40f * Time.deltaTime;
			if (m_TaskNameCurrentX <= 0f - (m_TaskTextWidth + 100f))
			{
				m_TaskNameCurrentX += m_TaskTextWidth + 100f;
			}
			((VisualElement)m_lbTaskName).style.translate = new StyleTranslate(new Translate(Length.op_Implicit(m_TaskNameCurrentX), Length.op_Implicit(0f), 0f));
			((VisualElement)m_lbTaskName2).style.translate = new StyleTranslate(new Translate(Length.op_Implicit(m_TaskNameCurrentX + m_TaskTextWidth + 100f), Length.op_Implicit(0f), 0f));
		}
	}

	public void OnTask()
	{
		zbLinkHelper.OpenPeeerage();
	}

	private void OnTaskCheck()
	{
		zbProgressEventHelper.Instance.SetProgressTrack(622);
		bool flag = zbSelf.Instance.taskHandler.IsTopPeerage();
		if (((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).All((TaskDetail t) => t.State == EActiveState.Done) && !flag)
		{
			zbWsMsgHandler.Instance.Peerage_Upgrade_Req();
			return;
		}
		TaskDetail taskDetail = (flag ? zbSelf.Instance.taskHandler.taskList.ToArray().FirstOrDefault((TaskDetail t) => t.State == EActiveState.Active) : ((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).FirstOrDefault((TaskDetail t) => t.State == EActiveState.Active));
		if (taskDetail != null)
		{
			if (flag)
			{
				zbWsMsgHandler.Instance.DailyTask_Award_Req((ETaskType)taskDetail.TaskType);
			}
			else
			{
				zbWsMsgHandler.Instance.Peerage_UpgradeTask_Req(taskDetail.TaskId);
			}
			return;
		}
		TaskDetail taskDetail2 = (flag ? zbSelf.Instance.taskHandler.taskList.ToArray().FirstOrDefault((TaskDetail t) => t.State == EActiveState.Pending) : ((IEnumerable<TaskDetail>)zbServerData.sGameData.PeerageInfo.TaskList).FirstOrDefault((TaskDetail t) => t.State == EActiveState.Pending));
		if (taskDetail2 != null)
		{
			if (114 == taskDetail2.TaskType || 109 == taskDetail2.TaskType)
			{
				if (m_wndParent.m_currTab == 1)
				{
					m_wndParent.SelectTab(0);
				}
				zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_wndParent.m_moduleLevel.m_btnFight);
			}
			else
			{
				zbLinkHelper.OpenLink(zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(taskDetail2.TaskType).link, taskDetail2.Param);
			}
		}
		else
		{
			zbLinkHelper.OpenPeeerage();
		}
	}

	public void RefreshGuideFinger()
	{
		if (zbUIMgr.Instance.m_lastTouchTime > 20f && !zbUIMgr.Instance.IsShow(EWndId.GuideFinger) && !zbGuide.Instance.IsGuiding() && zbUIMgr.Instance.IsTopCascadWnd(EWndId.MainLevel))
		{
			zbUIMgr.Instance.ShowGuideFinger((VisualElement)(object)m_btnTask);
		}
	}
}
