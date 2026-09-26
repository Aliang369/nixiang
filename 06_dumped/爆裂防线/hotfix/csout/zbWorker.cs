using System;
using Proto.Enums;
using Proto.Types;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbWorker : MonoBehaviour
{
	[NonSerialized]
	public zbWorkerStateMachine fsm;

	private BoxCollider2D m_boxCollider;

	[NonSerialized]
	public SkeletonAnimation m_skeletonAnimation;

	[NonSerialized]
	public zbRoom targetRoom;

	[NonSerialized]
	public WorkerDetail m_homelandWorkerDetail;

	[NonSerialized]
	public ESortMainLayerType m_sortType = ESortMainLayerType.Worker;

	private bool m_bInitStyle;

	[NonSerialized]
	public string lastStyleInfoString;

	[NonSerialized]
	public WorkerStyleInfo initStyleInfo;

	protected void Awake()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		fsm = new zbWorkerStateMachine();
		fsm.Init(this);
		m_boxCollider = ((Component)this).GetComponent<BoxCollider2D>();
		m_skeletonAnimation = ((Component)((Component)this).transform.Find("spineAnimation")).GetComponent<SkeletonAnimation>();
		((SkeletonRenderer)m_skeletonAnimation).updateWhenInvisible = (UpdateMode)0;
		zbSortMainMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		m_bInitStyle = false;
	}

	protected void OnEnable()
	{
		fsm.OnEnable();
	}

	protected void Update()
	{
		fsm.Update();
		UpdateZorder();
	}

	private void UpdateZorder()
	{
		zbSortMainMgr.Instance.UpdateSortOrder(m_sortType, ((Component)this).gameObject);
	}

	public void SetFacing(bool left)
	{
		if (left)
		{
			((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX = 1f;
		}
		else
		{
			((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX = -1f;
		}
	}

	public bool IsFaceLeft()
	{
		return ((SkeletonRenderer)m_skeletonAnimation).Skeleton.ScaleX == 1f;
	}

	public TrackEntry SetAnimation(string animationName, bool loop)
	{
		TrackEntry result = m_skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
		m_skeletonAnimation.AnimationState.TimeScale = zbRoomDefine.Instance.GetAnimationTimeScale(animationName);
		return result;
	}

	public void Refresh(WorkerDetail workerDetail)
	{
		if (!m_bInitStyle)
		{
			m_homelandWorkerDetail = workerDetail;
			initStyleInfo = workerDetail.StyleInfo.Clone();
			zbStyleInfoHelper.RefreshStyleInfo(m_skeletonAnimation, m_homelandWorkerDetail.StyleInfo);
			m_bInitStyle = true;
		}
		else
		{
			WorkerStyleInfo styleInfo = m_homelandWorkerDetail.StyleInfo;
			m_homelandWorkerDetail = workerDetail;
			m_homelandWorkerDetail.StyleInfo = styleInfo;
		}
		RefreshState();
	}

	public void RefreshState()
	{
		if (m_homelandWorkerDetail.RoomId > 0)
		{
			fsm.SetState(EWorkerState.Work);
		}
		else
		{
			fsm.SetState(EWorkerState.Dating);
		}
	}

	public WorkerStyleInfo GetNewStyleInfo()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		zbRoom roomByPosition = zbHomeMgr.Instance.roomMgr.GetRoomByPosition(((Component)this).transform.position);
		if (m_homelandWorkerDetail != null && (Object)(object)roomByPosition != (Object)null)
		{
			while (m_homelandWorkerDetail.StyleInfo.Suit.Count < 4)
			{
				m_homelandWorkerDetail.StyleInfo.Suit.Add(0);
			}
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(roomByPosition.roomData.TypeId);
			int[] array = recordByKey.manSuit;
			if (m_homelandWorkerDetail.StyleInfo.Gender == EGender.Female)
			{
				array = recordByKey.womanSuit;
			}
			int[] array2 = array;
			foreach (int num in array2)
			{
				zbNpcSuitCsvRecord recordByKey2 = zbDataConfig.Instance.npcSuitCsvTable.GetRecordByKey(num);
				if (recordByKey2 != null)
				{
					if (recordByKey2.type == 1)
					{
						m_homelandWorkerDetail.StyleInfo.Suit[0] = num;
					}
					else if (recordByKey2.type == 2)
					{
						m_homelandWorkerDetail.StyleInfo.Suit[1] = num;
					}
					else if (recordByKey2.type == 3)
					{
						m_homelandWorkerDetail.StyleInfo.Suit[2] = num;
					}
					else if (recordByKey2.type == 4)
					{
						m_homelandWorkerDetail.StyleInfo.Suit[3] = num;
					}
				}
			}
		}
		return m_homelandWorkerDetail.StyleInfo;
	}

	public void RefreshRoomSuit()
	{
		GetNewStyleInfo();
		zbStyleInfoHelper.RefreshStyleInfo(m_skeletonAnimation, m_homelandWorkerDetail.StyleInfo);
		lastStyleInfoString = m_homelandWorkerDetail.StyleInfo.ToString();
	}

	public zbFloor GetCurrentFloor()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = 1.8f;
		float y = ((Component)this).transform.position.y;
		foreach (zbFloor floor in zbHomeMgr.Instance.floorMgr.floors)
		{
			float y2 = ((Component)floor).transform.position.y;
			float num2 = num / 2f;
			if (y >= y2 - num2 && y <= y2 + num2)
			{
				return floor;
			}
		}
		return null;
	}

	public zbRoom GetCurrentRoom()
	{
		zbFloor currentFloor = GetCurrentFloor();
		if ((Object)(object)currentFloor == (Object)null)
		{
			return null;
		}
		foreach (zbRoom room in zbHomeMgr.Instance.roomMgr.roomList)
		{
			if (room.floor.floorIndex == currentFloor.floorIndex && IsInsideRoom(room))
			{
				return room;
			}
		}
		return null;
	}

	private bool IsInsideRoom(zbRoom room)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 position2 = ((Component)room).transform.position;
		float num = room.GetRoomWidth() / 2f;
		float num2 = position2.x - num;
		float num3 = position2.x + num;
		float num4 = 0.9f;
		float num5 = position2.y - num4;
		float num6 = position2.y + num4;
		bool num7 = position.x >= num2 && position.x <= num3;
		bool flag = position.y >= num5 && position.y <= num6;
		return num7 & flag;
	}

	public string GetWorkAnimationName()
	{
		HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(m_homelandWorkerDetail.RoomId);
		if (homelandRoomDetail != null && homelandRoomDetail.TypeId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.CommRadarId)
		{
			return "action_pad";
		}
		return "action_wrench";
	}
}
