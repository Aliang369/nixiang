using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityEventChestWnd : zbBaseWnd
{
	private SpineVisualElement m_spine;

	private VisualElement m_bg;

	private Button m_btnClose;

	private Label m_lbTime;

	private VisualElement m_spriteTili;

	private Label m_lbTili;

	private Label m_lbChestKey;

	private Button m_btnOpen;

	private FacilityRoomDetail m_roomDetail;

	private bool m_isClosing;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spine = UQueryExtensions.Q<SpineVisualElement>(m_RootElement, "spine", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_spriteTili = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_tili", (string)null);
		m_lbTili = UQueryExtensions.Q<Label>(m_RootElement, "lb_tili", (string)null);
		m_lbChestKey = UQueryExtensions.Q<Label>(m_RootElement, "lb_chestKey", (string)null);
		m_btnOpen = UQueryExtensions.Q<Button>(m_RootElement, "btn_open", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOpen, (Action)OnOpen, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_FacilityOpenChest_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, delegate
		{
			RefreshDetail();
		});
	}

	public void RefreshCustom(FacilityRoomDetail roomDetail)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_bg.style.opacity = StyleFloat.op_Implicit(0f);
		((TextElement)m_btnOpen).text = zbUtil.Language("lang.10786");
		m_isClosing = false;
		m_roomDetail = roomDetail;
		RefreshDetail();
		RefreshLeftTime();
		m_spine.AnimationState.SetAnimation(0, "in", false);
		m_spine.AnimationState.AddAnimation(0, "close", true, 0f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
		((MonoBehaviour)this).StartCoroutine(FadeInEx());
	}

	private void RefreshDetail()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		m_spriteTili.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_roomDetail.RoomState != EFacilityRoomState.Ready));
		((TextElement)m_lbTili).text = zbSelf.Instance.facilityHandler.tiliCost.ToString();
		((VisualElement)m_lbChestKey).style.color = StyleColor.op_Implicit((zbSelf.Instance.moneyHandler.GetCountByTid(4) >= zbSelf.Instance.facilityHandler.tiliCost) ? Color.white : Color.red);
		zbMoneyCsvRecord recordByKey = zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(36);
		((VisualElement)m_lbChestKey).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		((VisualElement)m_lbChestKey).style.color = StyleColor.op_Implicit((zbSelf.Instance.moneyHandler.GetCountByTid(36) >= 1) ? Color.white : Color.red);
	}

	private IEnumerator FadeInEx()
	{
		((MonoBehaviour)this).StartCoroutine(FadeCoroutine(m_bg, 0f, 1f, 17f / 30f));
		yield return (object)new WaitForSeconds(0.5f);
	}

	private IEnumerator FadeCoroutine(VisualElement element, float startOpacity, float endOpacity, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			float num = Mathf.SmoothStep(startOpacity, endOpacity, time / duration);
			element.style.opacity = StyleFloat.op_Implicit(num);
			time += Time.deltaTime;
			yield return null;
		}
		element.style.opacity = StyleFloat.op_Implicit(endOpacity);
	}

	private void RefreshLeftTime()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (m_roomDetail != null)
		{
			long num = m_roomDetail.ExplorerExpire - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbTime).text = zbUtil.Language("lang.11274") + ": " + zbUtil.FormatTimeMMSS(num);
			}
			else
			{
				((VisualElement)m_lbTime).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	private void OnOpen()
	{
		if (m_roomDetail.RoomState == EFacilityRoomState.Completed)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11279"));
		}
		else if (m_roomDetail.RoomState == EFacilityRoomState.Ready && !zbSelf.Instance.moneyHandler.HasEnoughMoney(4, zbSelf.Instance.facilityHandler.tiliCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, zbSelf.Instance.facilityHandler.tiliCost);
		}
		else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(36, 1L))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 36L, 1L);
		}
		else if (m_roomDetail.RoomState == EFacilityRoomState.Ready)
		{
			zbWsMsgHandler.Instance.Facility_EventTrigger_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, m_roomDetail.RoomId, needExploreAnim: false, openCombatWnd: false, openLockedChest: true);
		}
		else
		{
			zbWsMsgHandler.Instance.Facility_OpenChest_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, m_roomDetail.RoomId);
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnClose()
	{
		long num = m_roomDetail.ExplorerExpire - zbSelf.Instance.timeHandler.GetServerTime();
		if (m_roomDetail.ExplorerRole == zbSelf.Instance.GetRoleId() && num > 0 && m_roomDetail.RoomState != EFacilityRoomState.Completed)
		{
			string message = string.Format(zbUtil.Language("lang.11278"), "<color=red>" + zbUtil.FormatTimeMMSS(num) + "</color>");
			zbUIMgr.Instance.ShowFacilityMessage(zbUtil.Language("lang.10143"), message, needTili: false, delegate(bool isOK)
			{
				if (isOK)
				{
					OnCloseAnimation();
				}
			}, m_roomDetail.ExplorerExpire);
		}
		else
		{
			OnCloseAnimation();
		}
	}

	private void OnCloseAnimation()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (!m_isClosing)
		{
			m_isClosing = true;
			m_bg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_spine.AnimationState.ClearTrack(0);
			m_spine.AnimationState.SetAnimation(0, "out", false).Complete += new TrackEntryDelegate(OnOutAnimationComplete);
		}
	}

	private void OnOutAnimationComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Animation.Name == "out")
		{
			Hide();
		}
	}

	private void OnEventUpdate(object data)
	{
		if (data is FacilityOpenChestAck facilityOpenChestAck)
		{
			m_roomDetail = zbSelf.Instance.facilityHandler.GetFacilityRoomDetail(m_roomDetail.RoomId);
			zbUIMgr.Instance.ShowModel();
			m_spine.AnimationState.SetAnimation(0, "close_open", false);
			m_spine.AnimationState.AddAnimation(0, "open", true, 0f);
			zbAudioMgr.Instance.PlayAudio("audio/ui/ui_award.ogg");
			((MonoBehaviour)this).StartCoroutine(ShowAwardList(((IEnumerable<AwardDetail>)facilityOpenChestAck.AwardList).ToArray()));
		}
	}

	private IEnumerator ShowAwardList(AwardDetail[] awardList)
	{
		yield return (object)new WaitForSeconds(1.6666666f);
		((TextElement)m_btnOpen).text = zbUtil.Language("lang.10148");
		zbUIMgr.Instance.HideModel();
		zbUIMgr.Instance.ShowAward(awardList, delegate
		{
			OnClose();
		});
	}
}
