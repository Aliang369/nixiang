using System;
using System.Collections;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbRoom : MonoBehaviour
{
	[NonSerialized]
	public zbRoomStateMachine fsm;

	[NonSerialized]
	public zbDoorStateMachine fsmLeftDoor;

	[NonSerialized]
	public zbDoorStateMachine fsmRightDoor;

	[NonSerialized]
	public HomelandRoomDetail roomData;

	[NonSerialized]
	public zbHomeRoomTypeCsvRecord roomRecord;

	[NonSerialized]
	public zbFloor floor;

	[NonSerialized]
	public zbRoomHud roomHud;

	[NonSerialized]
	public SpriteRenderer m_spriteRenderer;

	private BoxCollider2D m_boxCollider;

	[NonSerialized]
	public SkeletonAnimation m_spineState;

	private SkeletonAnimation m_spineAmbient;

	private SkeletonAnimation m_spineLeftDoor;

	private SkeletonAnimation m_spineRightDoor;

	private Transform m_trSpineState;

	private Transform m_trSpineLeftDoor;

	private Transform m_trSpineRightDoor;

	private float m_roomWidth;

	private float m_roomHeight = 1.8f;

	private float m_WalkPosGapY = 0.1f;

	[NonSerialized]
	public bool IsBroken;

	private int m_oldRoomId = -1;

	private zbRoomSpecimen m_roomSpecimen;

	private zbRoomHall m_roomHall;

	private void Awake()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		fsm = new zbRoomStateMachine();
		fsm.Init(this);
		fsmLeftDoor = new zbDoorStateMachine();
		fsmLeftDoor.Init(this, _isLeftDoor: true);
		fsmRightDoor = new zbDoorStateMachine();
		fsmRightDoor.Init(this, _isLeftDoor: false);
		Transform val = ((Component)this).transform.Find("visibleObject");
		m_spriteRenderer = ((Component)val.Find("room")).GetComponent<SpriteRenderer>();
		m_boxCollider = ((Component)val.Find("room")).GetComponent<BoxCollider2D>();
		m_spineAmbient = ((Component)val.Find("spineAmbient")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_spineAmbient).updateWhenInvisible = (UpdateMode)0;
		m_trSpineState = val.Find("spineState");
		m_trSpineLeftDoor = val.Find("spineLeftDoor");
		m_trSpineRightDoor = val.Find("spineRightDoor");
		((Component)m_trSpineState).gameObject.SetActive(false);
		((Component)m_trSpineLeftDoor).gameObject.SetActive(false);
		((Component)m_trSpineRightDoor).gameObject.SetActive(false);
		roomHud = new zbRoomHud();
		roomHud.Init(this);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.RoomBg, ((Component)m_spriteRenderer).gameObject);
		zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.RoomSpine, ((Component)m_spineAmbient).gameObject);
	}

	private void OnEnable()
	{
		fsm.OnEnable();
		fsmLeftDoor.OnEnable();
		fsmRightDoor.OnEnable();
	}

	private void OnDestroy()
	{
		if (roomHud != null)
		{
			roomHud.Free();
			roomHud = null;
		}
		ReleaseSpineState();
		ReleaseDoor(isLeft: true);
		ReleaseDoor(isLeft: false);
	}

	public void Refresh(HomelandRoomDetail _roomData)
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		if (_roomData.RoomId != m_oldRoomId)
		{
			zbHomeMgr.Instance.leaverMgr.OnRoomDestroy();
			zbHomeMgr.Instance.workerMgr.OnRoomDestroy();
			m_oldRoomId = _roomData.RoomId;
		}
		floor = ((Component)((Component)this).transform.parent).GetComponent<zbFloor>();
		WarRoomDetail warRoomDetail = zbSelf.Instance.homelandHandler.GetWarRoomDetail(_roomData.RoomId);
		if (warRoomDetail != null && zbSelf.Instance.homelandHandler.warRoomTypeList.Contains(warRoomDetail.SkillType))
		{
			roomData = _roomData;
			roomRecord = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(roomData.TypeId);
			RefreshPostion();
			RefreshRoomSprite(showBroken: true);
			return;
		}
		if (roomData != null && roomData.Level == 0 && _roomData.Level == 1)
		{
			fsm.SetState(ERoomState.Building);
		}
		else if (roomData != null && _roomData.Level > roomData.Level && roomRecord.type == 3)
		{
			fsm.SetState(ERoomState.Building);
		}
		else
		{
			((Component)m_spriteRenderer).gameObject.SetActive(true);
		}
		((Component)this).gameObject.SetActive(zbSelf.Instance.homelandHandler.GetFloorState(_roomData.Y) == EActiveState.Done);
		roomData = _roomData;
		roomRecord = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(roomData.TypeId);
		RefreshPostion();
		RefreshRoomSprite();
		RefreshRoomState();
		RefreshRoomWidth();
		RefreshRoomDoor();
		RefreshRoomHud();
		RefreshSpecimen();
		RefreshHall();
		GetRandomWalkablePos();
		((Object)this).name = "room_" + roomData.Y + "_" + roomData.X;
	}

	private void RefreshSpecimen()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (roomData.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.SpecimenId && roomData.Level > 0)
		{
			if ((Object)(object)m_roomSpecimen == (Object)null)
			{
				GameObject val = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/roomSpecimen.prefab"));
				m_roomSpecimen = val.GetComponent<zbRoomSpecimen>();
				((Component)m_roomSpecimen).transform.SetParent(((Component)this).transform, false);
				((Component)m_roomSpecimen).transform.localPosition = Vector3.zero;
				((Component)m_roomSpecimen).transform.localRotation = Quaternion.identity;
				((Component)m_roomSpecimen).transform.localScale = Vector3.one;
				zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Door, ((Component)m_roomSpecimen).gameObject);
			}
			m_roomSpecimen.Refresh(roomData);
		}
	}

	private void RefreshHall()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (roomData.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.HallId && roomData.Level > 0)
		{
			if ((Object)(object)m_roomHall == (Object)null)
			{
				GameObject val = Object.Instantiate<GameObject>(zbAssetLoader.Instance.LoadResouce<GameObject>("prefab/home/roomHall.prefab"));
				m_roomHall = val.GetComponent<zbRoomHall>();
				((Component)m_roomHall).transform.SetParent(((Component)this).transform, false);
				((Component)m_roomHall).transform.localPosition = Vector3.zero;
				((Component)m_roomHall).transform.localRotation = Quaternion.identity;
				((Component)m_roomHall).transform.localScale = Vector3.one;
				zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Door, ((Component)m_roomHall).gameObject);
			}
			m_roomHall.Refresh(roomData);
		}
	}

	private void RefreshPostion()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(zbRoomDefine.Instance.GetRoomPostionX(roomRecord.length, roomData.X), 0f, 0f);
		((Component)this).transform.localScale = Vector3.one;
	}

	public void RefreshRoomState()
	{
		if (fsm.GetStateId() != ERoomState.Building && fsm.GetStateId() != ERoomState.Destroy)
		{
			((Component)m_spriteRenderer).gameObject.SetActive(true);
			ReleaseSpineState();
		}
	}

	private void RefreshRoomWidth()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (roomRecord.length >= 3)
		{
			m_roomWidth = 9.4f;
		}
		else if (roomRecord.length == 2)
		{
			m_roomWidth = 6.2f;
		}
		else
		{
			m_roomWidth = 3f;
		}
		int num = Math.Max(roomRecord.length, 1);
		m_boxCollider.size = new Vector2((float)num * 3f, 1.8f);
	}

	private void RefreshRoomSprite(bool showBroken = false)
	{
		string text = "";
		string text2 = "";
		if ((roomData.Level == 0 || (IsFightRoom() && zbSelf.Instance.guideHandler.IsFirstEnterHomeWnd())) | showBroken)
		{
			text = roomRecord.iconBroken;
			text2 = roomRecord.spineBroken + "_SkeletonData.asset";
			IsBroken = true;
		}
		else
		{
			text = roomRecord.icon;
			text2 = roomRecord.door + "_SkeletonData.asset";
			IsBroken = false;
		}
		m_spriteRenderer.sprite = zbAssetLoader.Instance.LoadSpriteAsset(text);
		zbUtil.RefreshSpine(m_spineAmbient, text2, "ambient", loop: true);
	}

	private void RefreshRoomDoor()
	{
		string doorPath = roomRecord.leftdoor + "_SkeletonData.asset";
		string doorPath2 = roomRecord.rightdoor + "_SkeletonData.asset";
		if (roomData.Level <= 0)
		{
			doorPath = roomRecord.leftdoor_b + "_SkeletonData.asset";
			doorPath2 = roomRecord.rightdoo_b + "_SkeletonData.asset";
		}
		bool num = CanDoorVisible(isLeft: true);
		bool flag = CanDoorVisible(isLeft: false);
		if (num)
		{
			EnsureDoor(isLeft: true, doorPath);
		}
		else
		{
			ReleaseDoor(isLeft: true);
		}
		if (flag)
		{
			EnsureDoor(isLeft: false, doorPath2);
		}
		else
		{
			ReleaseDoor(isLeft: false);
		}
	}

	public void RefreshRoomHud()
	{
		roomHud.Refresh();
	}

	private void Update()
	{
		fsm.Update();
		fsmLeftDoor.Update();
		fsmRightDoor.Update();
		roomHud.Update();
	}

	public Vector3 GetLeftDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3((0f - m_roomWidth) / 2f, -0.9f + m_WalkPosGapY, 0f);
	}

	public Vector3 GetRightDoorPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(m_roomWidth / 2f, -0.9f + m_WalkPosGapY, 0f);
	}

	public Vector3 GetCenterPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + new Vector3(0f, -0.9f + m_WalkPosGapY, -0.1f);
	}

	public Vector3 GetDoorPos(bool isLeft)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (!isLeft)
		{
			return GetRightDoorPos();
		}
		return GetLeftDoorPos();
	}

	public SkeletonAnimation EnsureSpineState(string spinePath = null)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_spineState == (Object)null)
		{
			m_spineState = ((Component)m_trSpineState).gameObject.AddComponent<SkeletonAnimation>();
			((SkeletonRenderer)m_spineState).skeletonDataAsset = ((SkeletonRenderer)m_spineAmbient).skeletonDataAsset;
			((SkeletonRenderer)m_spineState).Initialize(false, false);
			((SkeletonRenderer)m_spineState).updateWhenInvisible = (UpdateMode)0;
			zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.RoomGoldSpine, ((Component)m_spineState).gameObject);
		}
		((Component)m_trSpineState).gameObject.SetActive(true);
		if (!string.IsNullOrEmpty(spinePath))
		{
			zbUtil.RefreshSpine(m_spineState, spinePath);
		}
		return m_spineState;
	}

	public void ReleaseSpineState()
	{
		if ((Object)(object)m_spineState != (Object)null)
		{
			Object.Destroy((Object)(object)m_spineState);
			m_spineState = null;
			((Component)m_trSpineState).gameObject.SetActive(false);
		}
	}

	private void EnsureDoor(bool isLeft, string doorPath)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Transform val = (isLeft ? m_trSpineLeftDoor : m_trSpineRightDoor);
		SkeletonAnimation val2 = ((Component)val).GetComponent<SkeletonAnimation>();
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = ((Component)val).gameObject.AddComponent<SkeletonAnimation>();
			((SkeletonRenderer)val2).skeletonDataAsset = ((SkeletonRenderer)m_spineAmbient).skeletonDataAsset;
			((SkeletonRenderer)val2).Initialize(false, false);
			((SkeletonRenderer)val2).updateWhenInvisible = (UpdateMode)0;
			zbSortMainMgr.Instance.InitSortOrder(ESortMainLayerType.Door, ((Component)val).gameObject);
		}
		zbUtil.RefreshSpine(val2, doorPath);
		((Component)val).gameObject.SetActive(true);
		if (isLeft)
		{
			m_spineLeftDoor = val2;
		}
		else
		{
			m_spineRightDoor = val2;
		}
	}

	private void ReleaseDoor(bool isLeft)
	{
		Transform obj = (isLeft ? m_trSpineLeftDoor : m_trSpineRightDoor);
		SkeletonAnimation component = ((Component)obj).GetComponent<SkeletonAnimation>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
			if (isLeft)
			{
				m_spineLeftDoor = null;
			}
			else
			{
				m_spineRightDoor = null;
			}
		}
		((Component)obj).gameObject.SetActive(false);
	}

	private void PlayLeftDoorOpenAnimation()
	{
		if ((Object)(object)m_spineLeftDoor != (Object)null && (Object)(object)((Component)m_spineLeftDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineLeftDoor, "left_anim_open"))
		{
			m_spineLeftDoor.AnimationState.SetAnimation(0, "left_anim_open", false);
		}
	}

	private void PlayLeftDoorCloseAnimation(float delayTime = 0f)
	{
		if ((Object)(object)m_spineLeftDoor != (Object)null && (Object)(object)((Component)m_spineLeftDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineLeftDoor, "left_anim_close"))
		{
			TrackEntry val = m_spineLeftDoor.AnimationState.SetAnimation(0, "left_anim_close", false);
			if (val != null && delayTime > 0f)
			{
				val.Delay = delayTime;
			}
		}
	}

	private void PlayRightDoorOpenAnimation()
	{
		if ((Object)(object)m_spineRightDoor != (Object)null && (Object)(object)((Component)m_spineRightDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineRightDoor, "right_anim_open"))
		{
			m_spineRightDoor.AnimationState.SetAnimation(0, "right_anim_open", false);
		}
	}

	private void PlayRightDoorCloseAnimation(float delayTime = 0f)
	{
		if ((Object)(object)m_spineRightDoor != (Object)null && (Object)(object)((Component)m_spineRightDoor).gameObject != (Object)null && zbUtil.CheckAnimationNameExist(m_spineRightDoor, "right_anim_close"))
		{
			TrackEntry val = m_spineRightDoor.AnimationState.SetAnimation(0, "right_anim_close", false);
			if (val != null && delayTime > 0f)
			{
				val.Delay = delayTime;
			}
		}
	}

	public IEnumerator OpenDoor(bool isLeft)
	{
		if (isLeft)
		{
			PlayLeftDoorOpenAnimation();
		}
		else
		{
			PlayRightDoorOpenAnimation();
		}
		yield return null;
	}

	public IEnumerator CloseDoor(bool isLeft, float delayTime = 0f)
	{
		if (isLeft)
		{
			PlayLeftDoorCloseAnimation(delayTime);
		}
		else
		{
			PlayRightDoorCloseAnimation(delayTime);
		}
		yield return null;
	}

	public IEnumerator PlayBuildingRoomAnimation(string animName)
	{
		string text = "building" + roomRecord.length + "_" + animName;
		TrackEntry val = EnsureSpineState("animation/home/room_building/room_building_SkeletonData.asset").AnimationState.SetAnimation(0, text, false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.Animation.Duration);
		}
	}

	public bool IsSameRoom(zbRoom testRoom)
	{
		if ((Object)(object)testRoom != (Object)null && roomData.X == testRoom.roomData.X)
		{
			return roomData.Y == testRoom.roomData.Y;
		}
		return false;
	}

	public float GetRoomWidth()
	{
		return m_roomWidth;
	}

	public float GetRoomHeight()
	{
		return m_roomHeight;
	}

	public bool CanMerge()
	{
		if (roomRecord.maxNum <= 0 || roomData.Level < 1 || roomRecord.length >= 3)
		{
			return false;
		}
		return true;
	}

	public Vector3 GetRandomWalkablePos()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Vector3 centerPos = GetCenterPos();
		float num = m_roomWidth * 0.5f;
		float min = ((Component)this).transform.position.x - num + 0.8f;
		float max = ((Component)this).transform.position.x + num - 0.8f;
		float num2 = centerPos.y + Rand.Range(-0.03f, 0.03f);
		return new Vector3(Rand.Range(min, max), num2, 0f);
	}

	public Vector2 GetScreenPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(zbCameraHelper.Instance.mainCamera.WorldToScreenPoint(GetCenterPos()));
	}

	public bool IsFightRoom()
	{
		return roomRecord.type == 2;
	}

	public bool IsNotOpen()
	{
		return zbSelf.Instance.homelandHandler.GetFloorState(floor.floorIndex) != EActiveState.Done;
	}

	private bool CanDoorVisible(bool isLeft)
	{
		if (isLeft)
		{
			if (roomData.Y != 0)
			{
				return roomData.X != 1;
			}
			return true;
		}
		if (roomData.Y != 0)
		{
			return roomData.X != 7 - roomRecord.length;
		}
		return true;
	}
}
