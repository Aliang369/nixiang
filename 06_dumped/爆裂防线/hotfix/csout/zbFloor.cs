using System;
using System.Collections;
using System.Collections.Generic;
using Best.HTTP.Shared.Extensions;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbFloor : MonoBehaviour
{
	[NonSerialized]
	public zbFloorStateMachine fsm;

	private SkeletonAnimation m_spineUnlock;

	[NonSerialized]
	public zbElevator elevator;

	[NonSerialized]
	public zbMergeLeft mergeLeft;

	[NonSerialized]
	public zbMergeRight mergeRight;

	[NonSerialized]
	public int floorIndex;

	private GameObject m_bgBlack;

	[NonSerialized]
	public Collider2D m_bgCol;

	private GameObject m_wallLeft;

	private GameObject m_wallRight;

	private GameObject m_wallDownLeft;

	private GameObject m_wallDownRight;

	private GameObject m_maskBlack11;

	private GameObject m_maskBlack12;

	private GameObject m_maskBlack13;

	private GameObject m_maskBlack21;

	private GameObject m_maskBlack22;

	private GameObject m_maskBlack23;

	private float ElEVATOR_WIDTH_HALF = 0.8f;

	[NonSerialized]
	public zbFloorHud floorHud;

	[NonSerialized]
	public EActiveState floorState;

	private void Awake()
	{
		fsm = new zbFloorStateMachine();
		fsm.Init(this);
		m_bgBlack = ((Component)((Component)this).transform.Find("bg_black")).gameObject;
		m_bgCol = m_bgBlack.GetComponent<Collider2D>();
		m_wallLeft = ((Component)((Component)this).transform.Find("wall_left")).gameObject;
		m_wallRight = ((Component)((Component)this).transform.Find("wall_right")).gameObject;
		m_wallDownLeft = ((Component)((Component)this).transform.Find("wall_down_left")).gameObject;
		m_wallDownRight = ((Component)((Component)this).transform.Find("wall_down_right")).gameObject;
		m_maskBlack11 = ((Component)((Component)this).transform.Find("maskBlack11")).gameObject;
		m_maskBlack12 = ((Component)((Component)this).transform.Find("maskBlack12")).gameObject;
		m_maskBlack13 = ((Component)((Component)this).transform.Find("maskBlack13")).gameObject;
		m_maskBlack21 = ((Component)((Component)this).transform.Find("maskBlack21")).gameObject;
		m_maskBlack22 = ((Component)((Component)this).transform.Find("maskBlack22")).gameObject;
		m_maskBlack23 = ((Component)((Component)this).transform.Find("maskBlack23")).gameObject;
		elevator = ((Component)((Component)this).transform.Find("elevator")).GetComponent<zbElevator>();
		m_spineUnlock = ((Component)((Component)this).transform.Find("spineUnlock")).GetComponent<SkeletonAnimation>();
		mergeLeft = ((Component)((Component)this).transform.Find("spineMergeLeft")).GetComponent<zbMergeLeft>();
		mergeRight = ((Component)((Component)this).transform.Find("spineMergeRight")).GetComponent<zbMergeRight>();
		((Component)mergeLeft).gameObject.SetActive(false);
		((Component)mergeRight).gameObject.SetActive(false);
		((Component)m_spineUnlock).gameObject.SetActive(false);
		floorIndex = Extensions.ToInt32(((Object)this).name.Replace("floor", ""), 0);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_wallLeft);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_wallRight);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_wallDownLeft);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_wallDownRight);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_maskBlack11);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_maskBlack12);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_maskBlack13);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_maskBlack21);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_maskBlack22);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Wall, m_maskBlack23);
		floorHud = new zbFloorHud();
		floorHud.Init(this);
	}

	private void OnEnable()
	{
		fsm.OnEnable();
	}

	public void Build()
	{
		elevator.Build();
		Refresh();
	}

	public void Refresh()
	{
		if (floorState == EActiveState.Active && zbSelf.Instance.homelandHandler.GetFloorState(floorIndex) == EActiveState.Done)
		{
			fsm.SetState(EFloorState.Building);
			floorState = zbSelf.Instance.homelandHandler.GetFloorState(floorIndex);
			return;
		}
		floorState = zbSelf.Instance.homelandHandler.GetFloorState(floorIndex);
		((Component)this).gameObject.SetActive(floorState != EActiveState.Pending || zbSelf.Instance.homelandHandler.GetNextActiveFloor() == floorIndex);
		m_bgBlack.SetActive(floorIndex < 0);
		m_wallLeft.SetActive(floorIndex >= 0);
		m_wallRight.SetActive(floorIndex >= 0);
		m_wallDownLeft.SetActive(floorIndex < 0);
		m_wallDownRight.SetActive(floorIndex < 0);
		((Component)m_spineUnlock).gameObject.SetActive(floorState != EActiveState.Done);
		((Component)elevator).gameObject.SetActive(floorState == EActiveState.Done);
		bool flag = floorState == EActiveState.Done && !zbUIMgr.Instance.IsShow(EWndId.HomeCreateDetail);
		((Component)mergeLeft).gameObject.SetActive(flag);
		((Component)mergeRight).gameObject.SetActive(flag);
		if ((floorState == EActiveState.Done) & flag)
		{
			elevator.Build();
			mergeLeft.Build();
			mergeRight.Build();
		}
		else if (floorState == EActiveState.Active)
		{
			m_spineUnlock.AnimationState.SetAnimation(0, "fix", true);
		}
		else
		{
			m_spineUnlock.AnimationState.SetAnimation(0, "animation", true);
		}
		RefreshMaskVisible();
		floorHud.Refresh();
	}

	public bool IsRoomCanMerge(zbRoom room)
	{
		if ((Object)(object)mergeLeft != (Object)null)
		{
			int[] roomList = mergeLeft.GetRoomComposeData().roomList;
			for (int i = 0; i < roomList.Length; i++)
			{
				if (roomList[i] == room.roomData.RoomId)
				{
					return true;
				}
			}
		}
		if ((Object)(object)mergeRight != (Object)null)
		{
			int[] roomList = mergeRight.GetRoomComposeData().roomList;
			for (int i = 0; i < roomList.Length; i++)
			{
				if (roomList[i] == room.roomData.RoomId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RefreshMaskVisible()
	{
		bool active = false;
		bool active2 = false;
		bool active3 = false;
		bool active4 = false;
		EActiveState eActiveState = zbSelf.Instance.homelandHandler.GetFloorState(floorIndex);
		if (eActiveState == EActiveState.Done || eActiveState == EActiveState.Active)
		{
			active = !zbHomeMgr.Instance.roomMgr.IsMustNull(1, floorIndex);
			active2 = !zbHomeMgr.Instance.roomMgr.IsMustNull(1, floorIndex);
			active3 = true;
			active4 = true;
			zbRoom roomAt = GetRoomAt(1);
			zbRoom roomAt2 = GetRoomAt(2);
			GetRoomAt(3);
			zbRoom roomAt3 = GetRoomAt(4);
			zbRoom roomAt4 = GetRoomAt(5);
			GetRoomAt(6);
			if ((Object)(object)roomAt != (Object)null && roomAt.roomRecord.length > 1)
			{
				active = false;
			}
			if ((Object)(object)roomAt != (Object)null && roomAt.roomRecord.length >= 3)
			{
				active2 = false;
			}
			if ((Object)(object)roomAt2 != (Object)null && roomAt2.roomRecord.length >= 2)
			{
				active2 = false;
			}
			if ((Object)(object)roomAt3 != (Object)null && roomAt3.roomRecord.length > 1)
			{
				active3 = false;
			}
			if ((Object)(object)roomAt3 != (Object)null && roomAt3.roomRecord.length >= 3)
			{
				active4 = false;
			}
			if ((Object)(object)roomAt4 != (Object)null && roomAt4.roomRecord.length >= 2)
			{
				active4 = false;
			}
		}
		m_maskBlack11.SetActive(active);
		m_maskBlack12.SetActive(active2);
		m_maskBlack21.SetActive(active3);
		m_maskBlack22.SetActive(active4);
	}

	public bool HasVaildRoom()
	{
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			if (room.Y == floorIndex)
			{
				return true;
			}
		}
		return false;
	}

	public zbRoom GetRoomAt(int x)
	{
		foreach (zbRoom room in zbHomeMgr.Instance.roomMgr.roomList)
		{
			if (!((Object)(object)room.floor == (Object)null) && room.roomData != null && room.floor.floorIndex == floorIndex && room.roomData.X == x)
			{
				return room;
			}
		}
		return null;
	}

	public Vector3 GetElevatorLeftDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(0f - ElEVATOR_WIDTH_HALF, 0f, 0f);
	}

	public Vector3 GetElevatorRightDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(ElEVATOR_WIDTH_HALF, 0f, 0f);
	}

	public Vector3 GetElevatorCenterPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public void PlayFloorUnlockAnimation()
	{
		foreach (zbRoomEmpty roomEmpty in zbHomeMgr.Instance.roomMgr.roomEmptyList)
		{
			if ((Object)(object)roomEmpty.floor != (Object)null && roomEmpty.floor.floorIndex == floorIndex)
			{
				roomEmpty.PlayerBuildingAnimation();
			}
		}
	}

	public zbRoom GetRoomNearElevator(bool isLeft)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		List<zbRoom> list = new List<zbRoom>();
		foreach (zbRoom room in zbHomeMgr.Instance.roomMgr.roomList)
		{
			if (room.floor.floorIndex != floorIndex)
			{
				continue;
			}
			if (isLeft)
			{
				if (room.roomData.X <= 3)
				{
					list.Add(room);
				}
			}
			else if (room.roomData.X > 3)
			{
				list.Add(room);
			}
		}
		zbRoom result = null;
		float num = float.MaxValue;
		foreach (zbRoom item in list)
		{
			float num2 = Mathf.Abs(((Component)item).transform.position.x - ((Component)elevator).transform.position.x);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public IEnumerator PlayBuildingFloorAnimation(string animName)
	{
		string text = "building_" + animName;
		TrackEntry val = m_spineUnlock.AnimationState.SetAnimation(0, text, false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.Animation.Duration);
		}
	}

	private void Update()
	{
		fsm.Update();
		floorHud.Update();
	}
}
