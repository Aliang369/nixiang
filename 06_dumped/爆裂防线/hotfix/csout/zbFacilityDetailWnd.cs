using System;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityDetailWnd : zbBaseWnd
{
	private Button m_btnFight;

	private Button m_btnClose;

	private Label m_lbLeftTime;

	private Label m_lbRoomCount;

	private FacilityBaseInfo m_facilityBaseInfo;

	public SkeletonAnimation m_anim;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_lbRoomCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_roomCount", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_leftTime", (string)null);
		m_anim = ((Component)m_GameObjectSceneRoot.transform.Find("spineScene").Find("SkeletonAnimation")).GetComponent<SkeletonAnimation>();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
	}

	public void RefreshCustom(FacilityBaseInfo facilityBaseInfo)
	{
		m_facilityBaseInfo = facilityBaseInfo;
		zbFacilityCsvRecord recordByKey = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(facilityBaseInfo.FacilityType);
		int leftShareRoomCount = zbSelf.Instance.facilityHandler.GetLeftShareRoomCount(recordByKey, m_facilityBaseInfo);
		((TextElement)m_lbRoomCount).text = leftShareRoomCount.ToString();
		RefreshLeftTime();
		((SkeletonRenderer)m_anim).skeleton.SetToSetupPose();
		m_anim.AnimationState.ClearTracks();
		m_anim.AnimationState.SetAnimation(0, recordByKey.quality.ToString(), true);
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_facilityBaseInfo != null && m_facilityBaseInfo.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime() <= 0)
		{
			Hide();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void RefreshLeftTime()
	{
		if (m_facilityBaseInfo != null)
		{
			((TextElement)m_lbLeftTime).text = zbUtil.FormatTimeHHMMSS(m_facilityBaseInfo.ExpireTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	private void OnFight()
	{
		zbWsMsgHandler.Instance.Facility_View_Req(m_facilityBaseInfo.FacilityId, openFacilityWnd: true);
		Hide();
	}
}
