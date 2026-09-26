using System;
using Proto.Enums;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFormationMechaItem
{
	private zbFormationWnd m_wndParent;

	private VisualElement m_RootElement;

	private Transform m_nodeMecha;

	private SkeletonAnimation m_spineAvatar;

	private SkeletonAnimation m_spineChuChang;

	private SkeletonAnimation m_spineSelect;

	private Button m_btnTouch;

	private zbMechaCsvRecord m_record;

	public void OnEnable(zbFormationWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_nodeMecha = m_wndParent.GetMyGameObjectSceneRoot().transform.Find("mecha");
		m_spineAvatar = ((Component)m_nodeMecha.Find("heroAvatar")).GetComponent<SkeletonAnimation>();
		m_spineChuChang = ((Component)m_nodeMecha.Find("chuChang")).GetComponent<SkeletonAnimation>();
		m_spineSelect = ((Component)m_nodeMecha.Find("select")).GetComponent<SkeletonAnimation>();
		((Component)m_spineChuChang).gameObject.SetActive(false);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_WorldEventAwardBox_Ack, OnEventUpdate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnEventUpdate);
	}

	public void RefreshWithoutSpine()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		((Component)m_nodeMecha).gameObject.SetActive(!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha) && zbServerData.sGameData.MechaInfo.MechaList.Count > 0);
		m_RootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Mecha) || zbServerData.sGameData.MechaInfo.MechaList.Count <= 0) ? 1 : 0));
		if (zbSelf.Instance.GetMechaId() > 0)
		{
			m_record = zbSelf.Instance.mechaHandler.GetMechaRecord(zbSelf.Instance.GetMechaId());
		}
		if (m_record != null)
		{
			((Component)m_spineAvatar).gameObject.SetActive(true);
			float num = m_record.scale * 0.65f;
			((Component)m_spineAvatar).transform.localScale = new Vector3(0f - num, num, num);
		}
		else
		{
			((Component)m_spineAvatar).gameObject.SetActive(false);
		}
		if (zbSelf.Instance.mechaHandler.mechaSetId > 0 && m_record != null && zbSelf.Instance.mechaHandler.mechaSetId == m_record.id)
		{
			ActionChuchangAnim();
			zbSelf.Instance.mechaHandler.mechaSetId = 0;
		}
	}

	public void RefreshSpinePart()
	{
		if (m_record != null)
		{
			zbMechaAvatarCsvRecord recordByKey = zbDataConfig.Instance.mechaAvatarCsvTable.GetRecordByKey(m_record.avatarId);
			if (recordByKey != null)
			{
				zbUtil.RefreshSpine(m_spineAvatar, recordByKey.fightBone + "_SkeletonData.asset", "idle", loop: true);
			}
		}
	}

	public void RefreshSelect(int skillType)
	{
		((Component)m_spineSelect).gameObject.SetActive(99 == skillType);
	}

	public void ActionChuchangAnim()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		((Component)m_spineChuChang).gameObject.SetActive(true);
		TrackEntry val = m_spineChuChang.AnimationState.SetAnimation(0, "animation", false);
		if (val == null)
		{
			return;
		}
		val.Complete += (TrackEntryDelegate)delegate
		{
			TrackEntry current = m_spineAvatar.AnimationState.GetCurrent(0);
			if (current != null && current.Animation.Name != "win")
			{
				m_spineAvatar.AnimationState.SetAnimation(0, "win", false);
			}
			m_spineAvatar.AnimationState.AddAnimation(0, "idle", true, 0f);
			((Component)m_spineChuChang).gameObject.SetActive(false);
		};
	}

	public void OnTouch(ClickEvent evt)
	{
		if (zbSelf.Instance.GetMechaId() <= 0 && zbServerData.sGameData.MechaInfo.MechaList.Count > 0)
		{
			zbMechaCsvRecord mechaRecord = zbSelf.Instance.mechaHandler.GetMechaRecord(zbServerData.sGameData.MechaInfo.MechaList[0].MechaId);
			if (mechaRecord != null)
			{
				zbUIMgr.Instance.ShowMechaDetail(mechaRecord);
				return;
			}
		}
		m_wndParent.RefreshSelectList(99);
		m_wndParent.SetEditPanelVisible(vis: true);
	}

	private void OnEventUpdate(object data)
	{
		m_wndParent.RefreshMechaDetail();
	}
}
