using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

internal class zbHolidayTaskChallengeItem : zbViewBaseItem
{
	private zbHolidayTaskWnd m_wndParent;

	private Label m_lbExp;

	private Label m_lbDesc;

	private Label m_lbProval;

	private Label m_lbVal;

	private Button m_btnGoto;

	private Button m_btnGet;

	private VisualElement m_spriteGetted;

	private VisualElement m_mask;

	private VisualElement m_notify;

	private TaskDetail m_taskDetail;

	private zbTaskTypeCsvRecord m_taskTypeRecord;

	private int m_exp;

	public zbHolidayTaskChallengeItem(zbHolidayTaskWnd wndParent, VisualTreeAsset tAssets)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_wndParent = wndParent;
		((VisualElement)this).Add((VisualElement)(object)tAssets.Instantiate());
		m_lbExp = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_exp", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_desc", (string)null);
		m_lbProval = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_proVal", (string)null);
		m_lbVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_val", (string)null);
		m_btnGoto = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_goto", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_get", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(-3f, -6f), 1f, true);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_getted", (string)null);
		m_mask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "mask", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGoto, (Action)OnGoto, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(TaskDetail taskDetail)
	{
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		m_taskDetail = taskDetail;
		m_taskTypeRecord = zbDataConfig.Instance.taskTypeCsvTable.GetRecordByKey(m_taskDetail.TaskType);
		bool flag = m_taskDetail.State == EActiveState.Active;
		bool flag2 = m_taskDetail.State == EActiveState.Done;
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language(m_taskTypeRecord.desc), m_taskDetail.Target);
		m_mask.visible = flag2;
		((VisualElement)m_btnGoto).visible = !flag && !flag2;
		((VisualElement)m_btnGet).visible = flag && !flag2;
		m_spriteGetted.visible = flag2 && !flag;
		m_exp = zbDataConfig.Instance.holidayTaskTaskCsvTable.GetRecordByKey(m_taskDetail.TaskId).exp;
		((TextElement)m_lbExp).text = "+" + m_exp;
		((TextElement)m_lbVal).text = $"{m_taskDetail.Reach} / {m_taskDetail.Target}";
		((VisualElement)m_lbProval).style.width = StyleLength.op_Implicit(new Length(flag ? 100f : Mathf.Min((float)m_taskDetail.Reach / (float)m_taskDetail.Target * 100f, 100f), (LengthUnit)1));
	}

	private void OnGoto()
	{
		zbLinkHelper.OpenLink(m_taskTypeRecord.link, m_taskDetail.Param);
	}

	private void OnGet()
	{
		zbWsMsgHandler.Instance.Operate_HolidayTaskTaskAward_Req(m_wndParent.m_data.BaseInfo.UUID, m_taskDetail.TaskId, m_exp);
	}
}
