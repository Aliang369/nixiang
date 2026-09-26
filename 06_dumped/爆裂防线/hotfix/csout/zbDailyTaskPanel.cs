using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDailyTaskPanel
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnDailyActive;

	private zbPieProgressTex m_dailyTaskProgress;

	private Label m_lbDailyTaskProgress;

	private VisualElement m_notifyDailyActive;

	private Button m_btnFusion;

	private zbPieProgressTex m_fusionProgress;

	private Label m_lbFusionProgress;

	private VisualElement m_notifyFusion;

	private Button m_btnJourneyTwoActive;

	private zbPieProgressTex m_journeyTwoActiveProgress;

	private Label m_lbJourneyTwoActiveProgress;

	private VisualElement m_notifyJourneyTwoActive;

	private Label m_lbProVal;

	private Label m_lbCurActive;

	private Label m_lbDesc;

	private ListView m_listView;

	private zbItem[] m_targetItems = new zbItem[5];

	private Label[] m_lbTargets = (Label[])(object)new Label[5];

	private VisualElement m_iconTime;

	private Label m_lbTime;

	private zbPeerageWnd m_wndParent;

	private DailyActiveDetail m_dailyActiveDetail;

	private FusionInfo m_fusionInfo;

	public void OnEnable(zbPeerageWnd wndParent, VisualElement root)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Invalid comparison between Unknown and I4
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/peerage/tb_dailyTaskItem.uxml");
		m_wndParent = wndParent;
		m_btnDailyActive = UQueryExtensions.Q<Button>(root, "btn_dailyTask", (string)null);
		m_dailyTaskProgress = UQueryExtensions.Q<zbPieProgressTex>((VisualElement)(object)m_btnDailyActive, "progress", (string)null);
		m_lbDailyTaskProgress = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnDailyActive, "lb_progress", (string)null);
		m_notifyDailyActive = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnDailyActive, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_btnFusion = UQueryExtensions.Q<Button>(root, "btn_fusion", (string)null);
		m_fusionProgress = UQueryExtensions.Q<zbPieProgressTex>((VisualElement)(object)m_btnFusion, "progress", (string)null);
		m_lbFusionProgress = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnFusion, "lb_progress", (string)null);
		m_notifyFusion = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFusion, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_btnJourneyTwoActive = UQueryExtensions.Q<Button>(root, "btn_journeyTwoActive", (string)null);
		m_journeyTwoActiveProgress = UQueryExtensions.Q<zbPieProgressTex>((VisualElement)(object)m_btnJourneyTwoActive, "progress", (string)null);
		m_lbJourneyTwoActiveProgress = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnJourneyTwoActive, "lb_progress", (string)null);
		m_notifyJourneyTwoActive = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnJourneyTwoActive, (Vector2?)new Vector2(0f, 0f), 1f, true);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDailyActive, (Action)OnDailyActive, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFusion, (Action)OnFusion, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnJourneyTwoActive, (Action)OnJourneyTwoActive, "", (TrickleDown)0);
		((VisualElement)m_btnDailyActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnFusion).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnJourneyTwoActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_lbProVal = UQueryExtensions.Q<Label>(root, "lb_proVal", (string)null);
		m_lbCurActive = UQueryExtensions.Q<Label>(root, "lb_activeVal", (string)null);
		m_iconTime = UQueryExtensions.Q<VisualElement>(root, "icon_time", (string)null);
		if ((int)Application.platform == 8)
		{
			m_iconTime.style.top = StyleLength.op_Implicit(180f);
		}
		m_lbTime = UQueryExtensions.Q<Label>(root, "lb_time", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(root, "lb_desc", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(root, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 134f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		for (int i = 0; i < 5; i++)
		{
			m_targetItems[i] = UQueryExtensions.Q<zbItem>(root, $"item{i}", (string)null);
			m_targetItems[i].OnEnable(m_wndParent);
			m_lbTargets[i] = UQueryExtensions.Q<Label>(root, $"lb_target{i}", (string)null);
		}
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, delegate
		{
			RefreshOperate();
		});
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			RefreshOperate();
		});
	}

	public void Refresh()
	{
		InitHeader();
		UpdateListView();
		RefreshOperate();
	}

	public void RefreshOperate()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		string text = $"{zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active}/{80}";
		float progress = Math.Clamp((float)zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active / 80f, 0f, 1f);
		m_dailyActiveDetail = zbSelf.Instance.taskHandler.GetOperateDailyActiveDetail();
		if (m_dailyActiveDetail != null)
		{
			((VisualElement)m_btnDailyActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbDailyTaskProgress).text = text;
			m_dailyTaskProgress.progress = progress;
			m_notifyDailyActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasDailyActivieGift(m_dailyActiveDetail.BaseInfo.UUID)));
		}
		else
		{
			((VisualElement)m_btnDailyActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_fusionInfo = zbSelf.Instance.taskHandler.GetFusionInfo();
		if (m_fusionInfo != null)
		{
			((VisualElement)m_btnFusion).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbFusionProgress).text = text;
			m_fusionProgress.progress = progress;
			m_notifyFusion.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!zbSelf.Instance.operateHandler.HasFusionGift(isHoliday: true) && !zbSelf.Instance.operateHandler.HasFusionGift()) ? 1 : 0));
		}
		else
		{
			((VisualElement)m_btnFusion).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (zbSelf.Instance.journeyTwoHandler.IsActive() && zbSelf.Instance.journeyTwoHandler.GetActiveTarget() > 0)
		{
			int reach = zbServerData.sGameData.JourneyInfo.ChapterTwo.Active.Reach;
			int activeTarget = zbSelf.Instance.journeyTwoHandler.GetActiveTarget();
			((VisualElement)m_btnJourneyTwoActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbJourneyTwoActiveProgress).text = $"{reach}/{activeTarget}";
			m_journeyTwoActiveProgress.progress = Math.Clamp((float)reach / (float)activeTarget, 0f, 1f);
			m_notifyJourneyTwoActive.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.HasActiveNotify()));
		}
		else
		{
			((VisualElement)m_btnJourneyTwoActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnTargetItem(ClickEvent evt, int index)
	{
		int num = (index + 1) * 20;
		if (num <= zbServerData.sGameData.DailyTaskInfo.TaskTarget.Active)
		{
			zbWsMsgHandler.Instance.DailyTask_TargetAward_Req(num);
		}
	}

	public void UpdatePerSecond()
	{
		((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.timeHandler.GetRefreshTime() - zbSelf.Instance.timeHandler.GetServerTime());
	}

	public void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		List<TaskDetail> items = (from t in zbSelf.Instance.taskHandler.taskList.ToArray()
			orderby (t.State != EActiveState.Active) ? ((t.State == EActiveState.Pending) ? 1 : 2) : 0
			select t).ToList();
		m_listView.makeItem = () => (VisualElement)(object)new zbDailyTaskItem(m_itemTA, m_wndParent);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbDailyTaskItem).Init(items[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
	}

	private void InitHeader()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		DailyTaskInfo dailyTaskInfo = zbServerData.sGameData.DailyTaskInfo;
		zbDailyTaskTargetCsvTableData records = zbDataConfig.Instance.dailyTaskTargetCsvTable.GetRecords();
		((TextElement)m_lbCurActive).text = dailyTaskInfo.TaskTarget.Active.ToString();
		float num = (float)dailyTaskInfo.TaskTarget.Active / (float)records.records[records.records.Count() - 1].active * 100f;
		((VisualElement)m_lbProVal).style.width = StyleLength.op_Implicit(new Length(Mathf.Min(num, 100f), (LengthUnit)1));
		for (int i = 0; i < records.records.Count(); i++)
		{
			((TextElement)m_lbTargets[i]).text = records.records[i].active.ToString();
			int[][] awardList = records.records[i].awardList;
			foreach (int[] array in awardList)
			{
				zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
				m_targetItems[i].Refresh(data);
				bool flag = dailyTaskInfo.TaskTarget.Active >= records.records[i].active;
				bool flag2 = dailyTaskInfo.TaskTarget.AwardRecord.Contains(records.records[i].active);
				int tempIndex = i;
				if (flag2 || !flag)
				{
					m_targetItems[i].SetTouchCallback(null);
				}
				else
				{
					m_targetItems[i].SetTouchCallback(delegate(ClickEvent evt)
					{
						OnTargetItem(evt, tempIndex);
					});
				}
				m_targetItems[i].SetGettedVisible(flag2);
				m_targetItems[i].SetSpineVisible(!flag2 & flag);
			}
		}
	}

	private void OnDailyActive()
	{
		if (m_dailyActiveDetail != null)
		{
			zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(11);
			zbUIMgr.Instance.ShowDailyActive(m_dailyActiveDetail.BaseInfo.UUID, recordByKey);
		}
	}

	private void OnFusion()
	{
		zbWsMsgHandler.Instance.Operate_FusionData_Req();
	}

	private void OnJourneyTwoActive()
	{
		zbUIMgr.Instance.ShowJourneyTwoActive();
	}
}
