using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityResultWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbTitle;

	private Label m_lbSubTitle;

	private Label m_lbDescMail;

	private VisualElement m_nodeTarget;

	private VisualElement m_spriteProgress;

	private Label m_lbProgress;

	private VisualElement[] m_nodeTargets = (VisualElement[])(object)new VisualElement[3];

	private zbItem[] m_itemTargets = new zbItem[3];

	private Label[] m_lbTargets = (Label[])(object)new Label[3];

	private Button m_btnProgress;

	private VisualElement m_tip;

	private VisualElement m_doubleAward;

	private IVisualElementScheduledItem m_hideSchedule;

	private Label m_lbCount;

	private Label m_lbRoom;

	private Label m_lbDesc;

	private Button m_btnResult;

	private Button m_btnCancel;

	public zbFacilityCsvRecord m_record;

	public FacilityMapInfo m_facilityMapInfo;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbSubTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_subTitle", (string)null);
		m_lbDescMail = UQueryExtensions.Q<Label>(m_RootElement, "lb_descMail", (string)null);
		m_nodeTarget = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_target", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_progress", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_nodeTargets[i] = UQueryExtensions.Q<VisualElement>(m_nodeTarget, $"target{i}", (string)null);
			m_lbTargets[i] = UQueryExtensions.Q<Label>(m_nodeTargets[i], "lb_target", (string)null);
			m_itemTargets[i] = UQueryExtensions.Q<zbItem>(m_nodeTargets[i], "item", (string)null);
			m_itemTargets[i].OnEnable(this);
		}
		m_lbDesc = UQueryExtensions.Q<Label>(m_nodeTarget, "lb_desc", (string)null);
		m_btnProgress = UQueryExtensions.Q<Button>(m_nodeTarget, "btn_progress", (string)null);
		m_tip = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "tip", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnProgress, (Action<ClickEvent>)OnShowTip, "", (TrickleDown)0);
		m_doubleAward = UQueryExtensions.Q<VisualElement>(m_nodeTarget, "double_award", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_lbRoom = UQueryExtensions.Q<Label>(m_RootElement, "lb_room", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnResult = UQueryExtensions.Q<Button>(m_RootElement, "btn_result", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnResult, (Action)OnResult, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Facility, OnEventUpdate);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (zbSelf.Instance.facilityHandler.facilityMapInfo != null)
		{
			m_facilityMapInfo = zbSelf.Instance.facilityHandler.facilityMapInfo;
			int canExploreCount = zbSelf.Instance.facilityHandler.GetCanExploreCount();
			if (canExploreCount > 0)
			{
				((TextElement)m_lbCount).text = zbUtil.Language("lang.11297") + $"<color=#2DF421>{canExploreCount}</color>";
			}
			else
			{
				((TextElement)m_lbCount).text = zbUtil.Language("lang.11297") + $"<color=#C50000>{canExploreCount}</color>";
			}
			int num = m_facilityMapInfo.RoomList.Count - m_facilityMapInfo.BaseInfo.ExploreTotal;
			if (num > 0)
			{
				((TextElement)m_lbRoom).text = zbUtil.Language("lang.11295") + $"<color=#C50000>{num}</color>";
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.11308");
				((VisualElement)m_lbDesc).style.color = StyleColor.op_Implicit(new Color(1f, 7f / 51f, 7f / 51f));
			}
			else
			{
				((TextElement)m_lbRoom).text = zbUtil.Language("lang.11295") + $"<color=#2DF421>{num}</color>";
				((TextElement)m_lbDesc).text = zbUtil.Language("lang.11309");
				((VisualElement)m_lbDesc).style.color = StyleColor.op_Implicit(new Color(20f / 51f, 1f, 0.14901961f));
			}
			RefreshTarget();
		}
	}

	private void RefreshTarget()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		m_record = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(m_facilityMapInfo.BaseInfo.FacilityType);
		((TextElement)m_lbProgress).text = m_facilityMapInfo.BaseInfo.ExploreTotal.ToString();
		((TextElement)m_lbTargets[0]).text = m_record.tierTarget1.ToString();
		((TextElement)m_lbTargets[1]).text = m_record.tierTarget2.ToString();
		((TextElement)m_lbTargets[2]).text = m_record.tierTarget3.ToString();
		m_doubleAward.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_facilityMapInfo.BaseInfo.DoubleAward != EBoolState.True));
		int num = ((m_facilityMapInfo.BaseInfo.DoubleAward != EBoolState.True) ? 1 : 2);
		int[][] tierAwardList = m_record.tierAwardList1;
		int num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array = tierAwardList[num2];
			m_itemTargets[0].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2] * num));
		}
		tierAwardList = m_record.tierAwardList2;
		num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array2 = tierAwardList[num2];
			m_itemTargets[1].Refresh(new zbItemData((EResourceType)array2[0], array2[1], array2[2] * num));
		}
		tierAwardList = m_record.tierAwardList3;
		num2 = 0;
		if (num2 < tierAwardList.Length)
		{
			int[] array3 = tierAwardList[num2];
			m_itemTargets[2].Refresh(new zbItemData((EResourceType)array3[0], array3[1], array3[2] * num));
		}
		float progress = GetProgress();
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(progress * 510f, (LengthUnit)0));
	}

	private float GetProgress()
	{
		int[] array = new int[3] { m_record.tierTarget1, m_record.tierTarget2, m_record.tierTarget3 };
		float num = 1f / 3f;
		for (int i = 0; i < 3; i++)
		{
			int num2 = array[i];
			if (m_facilityMapInfo.BaseInfo.ExploreTotal < num2)
			{
				int num3 = ((i != 0) ? array[i - 1] : 0);
				float num4 = (float)(m_facilityMapInfo.BaseInfo.ExploreTotal - num3) / (float)(num2 - num3);
				return Mathf.Clamp01((float)i * num + num4 * num);
			}
		}
		return 1f;
	}

	private void OnShowTip(ClickEvent evt)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_hideSchedule != null)
		{
			m_hideSchedule.Pause();
			m_hideSchedule = null;
		}
		m_hideSchedule = ((VisualElement)m_btnProgress).schedule.Execute((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			m_tip.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}).StartingIn(5000L);
	}

	private void OnResult()
	{
		zbWsMsgHandler.Instance.Facility_Close_Req(m_facilityMapInfo.BaseInfo.FacilityId);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
