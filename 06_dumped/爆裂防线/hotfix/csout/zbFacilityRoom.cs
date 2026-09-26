using System;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbFacilityRoom : MonoBehaviour
{
	[NonSerialized]
	public FacilityRoomDetail roomData;

	[NonSerialized]
	public zbFacilityRoomCsvRecord roomRecord;

	[NonSerialized]
	public zbFacilityRoomHud roomHud;

	[NonSerialized]
	public SkeletonAnimation m_spineAmbient;

	public SkeletonAnimation m_spineProtect;

	private zbFacilityWnd m_parentWnd;

	private zbFacilityPanel m_parent;

	private bool m_canExplore;

	private EFacilityRoomState m_lastRoomState;

	private bool m_isExploreAnim;

	private void Awake()
	{
		Transform val = ((Component)this).transform.Find("visibleObject");
		m_spineAmbient = ((Component)val.Find("spineAmbient")).GetComponent<SkeletonAnimation>();
		m_spineProtect = ((Component)val.Find("spineProtect")).GetComponent<SkeletonAnimation>();
		((Component)m_spineProtect).gameObject.SetActive(false);
		roomHud = new zbFacilityRoomHud();
		roomHud.Init(this);
	}

	private void OnEnable()
	{
	}

	public void Refresh(zbFacilityWnd parentWnd, zbFacilityPanel parent, int roomId, bool canExplore, bool firstIn)
	{
		m_parentWnd = parentWnd;
		m_parent = parent;
		m_canExplore = canExplore;
		roomData = zbSelf.Instance.facilityHandler.GetFacilityRoomDetail(roomId);
		if (roomId <= 0)
		{
			zbUtil.RefreshSpine(m_spineAmbient, "animation/ui/shenrutansuo/room/room_feiqi/room_feiqi_SkeletonData.asset", "animation", loop: true);
			((Component)m_spineProtect).gameObject.SetActive(false);
			roomHud.SetVisible(visible: false);
			return;
		}
		if (!canExplore)
		{
			zbUtil.RefreshSpine(m_spineAmbient, "animation/ui/shenrutansuo/room/room_weitansuo/room_weitansuo_SkeletonData.asset", "animation", loop: true);
			((Component)m_spineProtect).gameObject.SetActive(false);
			roomHud.SetVisible(visible: false);
			return;
		}
		roomHud.SetVisible(visible: true);
		roomRecord = zbDataConfig.Instance.facilityRoomCsvTable.GetRecordByKey((int)roomData.RoomType);
		int item = zbSelf.Instance.facilityHandler.GetFloorAndAllFloor(m_parentWnd.m_record, roomData.RoomId).Item1;
		if (m_lastRoomState == EFacilityRoomState.Locked && roomData.RoomState != EFacilityRoomState.Locked && !firstIn && (item != 0 || roomData.RoomType != EFacilityRoomType.EntranceStair))
		{
			m_isExploreAnim = true;
			if (roomData.ExplorerRole == zbSelf.Instance.GetRoleId())
			{
				zbUIMgr.Instance.ShowModel();
				m_parent.m_playerList[0].Refresh(this, isCurrentRoom: true);
				m_parent.m_playerList[0].PlayExploreAnimation();
			}
			roomHud.PlayProgressAnim(delegate
			{
				PlayOffOnRoomAnimation();
			});
		}
		else
		{
			RefreshDetail();
		}
	}

	public void RefreshDetail()
	{
		if (roomData.RoomState > EFacilityRoomState.Locked)
		{
			if (roomData.RoomType == EFacilityRoomType.Chest || roomData.RoomType == EFacilityRoomType.Rolling || roomData.RoomType == EFacilityRoomType.LockedChest || roomData.RoomType == EFacilityRoomType.Shop || roomData.RoomType == EFacilityRoomType.Monster)
			{
				if (roomData.RoomState != EFacilityRoomState.Completed)
				{
					zbUtil.RefreshSpine(m_spineAmbient, roomRecord.spine + "_SkeletonData.asset", "off", loop: true);
					m_spineAmbient.AnimationState.SetAnimation(0, "off", true);
				}
				else
				{
					zbUtil.RefreshSpine(m_spineAmbient, roomRecord.spine + "_SkeletonData.asset", "on", loop: true);
					m_spineAmbient.AnimationState.SetAnimation(0, "on", true);
				}
			}
			else
			{
				zbUtil.RefreshSpine(m_spineAmbient, roomRecord.spine + "_SkeletonData.asset", "animation", loop: true);
			}
		}
		else
		{
			zbUtil.RefreshSpine(m_spineAmbient, "animation/ui/shenrutansuo/room/room_weitansuo/room_weitansuo_SkeletonData.asset", "animation", loop: true);
		}
		roomHud.Refresh(m_parentWnd);
		m_lastRoomState = roomData.RoomState;
	}

	public void RefreshProtect()
	{
		if (roomRecord == null || roomRecord.explorerExpire <= 0 || roomData == null || roomData.RoomState == EFacilityRoomState.Locked || roomData.RoomState == EFacilityRoomState.Completed || m_isExploreAnim)
		{
			((Component)m_spineProtect).gameObject.SetActive(false);
		}
		else
		{
			((Component)m_spineProtect).gameObject.SetActive(roomData.ExplorerExpire > zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	public void PlayOffOnRoomAnimation()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		roomHud.Refresh(m_parentWnd);
		m_lastRoomState = roomData.RoomState;
		if (roomData.RoomType == EFacilityRoomType.Chest || roomData.RoomType == EFacilityRoomType.Rolling || roomData.RoomType == EFacilityRoomType.LockedChest || roomData.RoomType == EFacilityRoomType.Shop || roomData.RoomType == EFacilityRoomType.Monster)
		{
			zbUtil.RefreshSpine(m_spineAmbient, roomRecord.spine + "_SkeletonData.asset", "off", loop: true);
			TrackEntry val = m_spineAmbient.AnimationState.SetAnimation(0, "off_on", false);
			string text = ((roomData.RoomType == EFacilityRoomType.Shop || roomData.RoomType == EFacilityRoomType.Rolling || roomData.RoomType == EFacilityRoomType.Monster) ? "off" : "on");
			m_spineAmbient.AnimationState.AddAnimation(0, text, true, 0f);
			if (val != null)
			{
				val.Complete += (TrackEntryDelegate)delegate
				{
					OnAnimationEnd();
				};
			}
		}
		else
		{
			zbUtil.RefreshSpine(m_spineAmbient, roomRecord.spine + "_SkeletonData.asset", "animation", loop: true);
			OnAnimationEnd();
		}
	}

	private void OnAnimationEnd()
	{
		m_isExploreAnim = false;
		zbUIMgr.Instance.HideModel();
		if (zbSelf.Instance.facilityHandler.triggerAckAwardList != null)
		{
			zbUIMgr.Instance.ShowAward(zbSelf.Instance.facilityHandler.triggerAckAwardList, OnCallBack);
			zbSelf.Instance.facilityHandler.triggerAckAwardList = null;
		}
		else
		{
			OnCallBack();
		}
	}

	private void OnCallBack(bool isOK = true)
	{
		FacilityRoomDetail roomDetail = zbSelf.Instance.facilityHandler.triggerAckRoomDetail;
		if (roomDetail == null)
		{
			return;
		}
		if (roomDetail.RoomType == EFacilityRoomType.LockedChest)
		{
			zbUIMgr.Instance.ShowFacilityEventChest(roomDetail);
		}
		else if (roomDetail.RoomType == EFacilityRoomType.Monster)
		{
			if (zbSelf.Instance.facilityHandler.triggerAckMonster != null)
			{
				zbUIMgr.Instance.ShowFacilityEventMonster(roomDetail, zbSelf.Instance.facilityHandler.triggerAckMonster.RobotBaseInfo, zbSelf.Instance.facilityHandler.triggerAckMonster.BattleInfo);
			}
		}
		else if (roomDetail.RoomType == EFacilityRoomType.Shop)
		{
			zbUIMgr.Instance.ShowFacilityEventShop(roomDetail, zbSelf.Instance.facilityHandler.triggerAckShopInfo);
		}
		else if (roomDetail.RoomType == EFacilityRoomType.Rolling)
		{
			zbUIMgr.Instance.ShowFacilityEventRolling(roomDetail);
		}
		else if (roomDetail.RoomType == EFacilityRoomType.ExitStair)
		{
			zbUIMgr.Instance.ShowFacilityMessage(zbUtil.Language("lang.11269"), zbUtil.Language("lang.11270"), needTili: false, delegate(bool flag)
			{
				if (flag)
				{
					zbWsMsgHandler.Instance.Facility_StairTeleport_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, roomDetail.RoomId);
				}
			}, 0L);
		}
		zbSelf.Instance.facilityHandler.ResetFacilityTrigger();
	}

	public void OnClick()
	{
		if (roomData == null)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11288"));
		}
		else if (!m_canExplore)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11307"));
		}
		else if (roomData.RoomType == EFacilityRoomType.EntranceStair)
		{
			if (zbSelf.Instance.facilityHandler.GetFloorAndAllFloor(m_parentWnd.m_record, roomData.RoomId).Item1 == 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11266"));
				return;
			}
			zbUIMgr.Instance.ShowFacilityMessage(zbUtil.Language("lang.11267"), zbUtil.Language("lang.11268"), needTili: false, delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Facility_StairTeleport_Req(m_parentWnd.m_facilityMapInfo.BaseInfo.FacilityId, roomData.RoomId);
				}
			}, 0L);
		}
		else if (roomData.RoomState == EFacilityRoomState.Activated)
		{
			if (roomData.RoomType == EFacilityRoomType.Shop)
			{
				zbWsMsgHandler.Instance.Facility_EventTrigger_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, roomData.RoomId);
			}
			else
			{
				zbWsMsgHandler.Instance.Facility_RoomPreview_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, roomData.RoomId);
			}
		}
		else if (roomData.RoomState == EFacilityRoomState.Completed)
		{
			if (roomData.RoomType == EFacilityRoomType.Chest || roomData.RoomType == EFacilityRoomType.Trash)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10157"));
			}
			else if (roomData.RoomType == EFacilityRoomType.Rolling)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11310"));
			}
			else if (roomData.RoomType == EFacilityRoomType.Shop)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11312"));
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11275"));
			}
		}
		else if (roomData.RoomState == EFacilityRoomState.Locked)
		{
			if (zbSelf.Instance.facilityHandler.GetCanExploreCount() <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11281"));
			}
			else if (m_parentWnd.m_facilityMapInfo.BaseInfo.FacilityCreator.RoleId != zbSelf.Instance.GetRoleId() && zbSelf.Instance.facilityHandler.GetLeftShareRoomCount(m_parentWnd.m_record, m_parentWnd.m_facilityMapInfo.BaseInfo) <= 0)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11339"));
			}
			else if (!zbSelf.Instance.moneyHandler.HasEnoughMoney(4, zbSelf.Instance.facilityHandler.tiliCost))
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, zbSelf.Instance.facilityHandler.tiliCost);
			}
			else
			{
				zbWsMsgHandler.Instance.Facility_EventTrigger_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, roomData.RoomId, needExploreAnim: true);
			}
		}
		else if (roomData.RoomState == EFacilityRoomState.Ready)
		{
			zbWsMsgHandler.Instance.Facility_RoomPreview_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, roomData.RoomId);
		}
	}

	public Vector3 GetLeftPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(-0.55f, -0.48f, -1f);
	}

	public Vector3 GetCenterPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(0f, -0.48f, -1f);
	}

	private void Update()
	{
		roomHud.Update();
		RefreshProtect();
	}
}
