using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBossSubjectTaskItem : zbViewBaseItem
{
	private zbBossSubjectTaskWnd m_wndParent;

	private Label m_lbName;

	private Label m_lbDesc;

	private Label m_lbNeed;

	private Button m_btnTouch;

	private VisualElement m_spriteGetted;

	private VisualElement m_spriteProgress;

	private VisualElement m_notify;

	private zbItem[] m_items;

	private TaskDetail m_detail;

	private VisualElement m_mask;

	private zbTaskTypeCsvRecord m_taskTypeRecord;

	public zbBossSubjectTaskItem(VisualTreeAsset treeAsset, zbBossSubjectTaskWnd wndParent)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		m_items = new zbItem[3];
		base._002Ector();
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)treeAsset.Instantiate());
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_title", (string)null);
		m_lbNeed = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_need", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_receive", (string)null);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_received", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_mask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "mask", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(3f, 3f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(TaskDetail taskDetail)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		m_detail = taskDetail;
		float num = Mathf.Min((float)taskDetail.Reach / (float)m_detail.Target, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num * 304f, (LengthUnit)0));
		zbBossTaskCsvRecord recordByKey = zbDataConfig.Instance.bossTaskCsvTable.GetRecordByKey(m_detail.TaskId);
		m_taskTypeRecord = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(recordByKey.type);
		((TextElement)m_lbName).text = m_taskTypeRecord.name;
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language(m_taskTypeRecord.desc), m_detail.Target);
		((TextElement)m_lbNeed).text = $"{m_detail.Reach}/{m_detail.Target}";
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num2 = 0;
		int[][] award = recordByKey.award;
		foreach (int[] array in award)
		{
			if (num2 < m_items.Length)
			{
				zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
				((VisualElement)m_items[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_items[num2].Refresh(data);
			}
			num2++;
		}
		((VisualElement)m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_detail.State == EActiveState.Done));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_detail.State != EActiveState.Active));
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_detail.State != EActiveState.Done));
		m_mask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_detail.State != EActiveState.Done));
		if (m_detail.State == EActiveState.Active)
		{
			((TextElement)m_btnTouch).text = zbUtil.Language("lang.10090");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_green.png"));
		}
		else if (m_detail.State == EActiveState.Pending)
		{
			((TextElement)m_btnTouch).text = zbUtil.Language("lang.10155");
			((VisualElement)m_btnTouch).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_180_80_yellow.png"));
		}
	}

	private void OnTouch(ClickEvent evt)
	{
		if (m_detail.State == EActiveState.Active)
		{
			zbWsMsgHandler.Instance.Operate_BossSubTaskAward_Req(m_wndParent.bossSubjectDetail.BaseInfo.UUID, (ETaskType)m_detail.TaskType);
			return;
		}
		BossSubjectDetail bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail != null)
		{
			m_wndParent.Hide();
			if (m_detail.TaskType == 170)
			{
				zbUIMgr.Instance.ShowBossSubjectMain(bossSubjectDetail, 1);
			}
			else
			{
				zbUIMgr.Instance.ShowBossSubjectMain(bossSubjectDetail);
			}
		}
	}
}
