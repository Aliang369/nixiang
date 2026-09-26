using System.Collections;
using Spine;
using UnityEngine;

public class zbWorkerDatingSubMoveToRoomState : zbWorkerDatingSubState
{
	private Coroutine m_stateCoroutine;

	public zbWorkerDatingSubMoveToRoomState(zbWorkerDatingSubFsm fsm, EWorkerDatingSubState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
	}

	public override void OnEnable()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.worker).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		zbRoom roomByPosition = zbHomeMgr.Instance.roomMgr.GetRoomByPosition(((Component)m_fsm.worker).transform.position);
		zbRoom roomDating = zbHomeMgr.Instance.roomMgr.GetRoomDating();
		if ((Object)(object)roomByPosition != (Object)null && (Object)(object)roomDating != (Object)null && roomByPosition.IsSameRoom(m_fsm.worker.targetRoom))
		{
			m_fsm.SetState(EWorkerDatingSubState.Idle);
		}
		else
		{
			m_stateCoroutine = ((MonoBehaviour)m_fsm.worker).StartCoroutine(MoveToRoom(roomDating));
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.worker).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
	}

	public IEnumerator MoveToRoom(zbRoom targetRoom)
	{
		m_fsm.worker.SetAnimation("run", loop: true);
		if (zbRoomDefine.Instance.IsInLeftWild(((Component)m_fsm.worker).transform.position))
		{
			zbRoom roomDating = zbHomeMgr.Instance.roomMgr.GetRoomDating();
			yield return MoveTo(roomDating.GetCenterPos());
		}
		zbRoom roomByPosition = zbHomeMgr.Instance.roomMgr.GetRoomByPosition(((Component)m_fsm.worker).transform.position);
		zbFloor floorByPosition = zbHomeMgr.Instance.roomMgr.GetFloorByPosition(((Component)m_fsm.worker).transform.position);
		zbFloor floor = targetRoom.floor;
		if (floorByPosition.floorIndex == floor.floorIndex)
		{
			yield return MoveOnSameFloor(roomByPosition, targetRoom);
		}
		else
		{
			yield return MoveBetweenFloors(floorByPosition, floor, roomByPosition, targetRoom);
		}
		if (m_fsm.worker.lastStyleInfoString != m_fsm.worker.GetNewStyleInfo().ToString())
		{
			yield return SetupClothAnimation();
		}
		m_fsm.SetState(EWorkerDatingSubState.Idle);
	}

	private IEnumerator SetupClothAnimation()
	{
		TrackEntry val = m_fsm.worker.SetAnimation("action_cloth", loop: false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.Animation.Duration);
		}
		yield return null;
		yield return null;
		m_fsm.worker.RefreshRoomSuit();
	}

	private IEnumerator MoveOnSameFloor(zbRoom startRoom, zbRoom targetRoom)
	{
		if ((Object)(object)startRoom != (Object)null && startRoom.IsSameRoom(targetRoom))
		{
			yield return null;
		}
		if ((Object)(object)startRoom != (Object)null)
		{
			zbFloor floor = targetRoom.floor;
			zbElevator elevator = floor.elevator;
			bool flag = startRoom.roomData.X <= 3;
			bool targetLeft = targetRoom.roomData.X <= 3;
			if (flag == targetLeft)
			{
				yield return MoveAlongRooms(startRoom, targetRoom);
				yield break;
			}
			zbRoom nearRoom = floor.GetRoomNearElevator(flag);
			yield return MoveAlongRooms(startRoom, nearRoom);
			bool isLeft = zbRoomDefine.Instance.ShouldExitFromLeft(nearRoom);
			zbRoom targetNearRoom = floor.GetRoomNearElevator(targetLeft);
			zbRoomDefine.Instance.ShouldEnterFromLeft(targetNearRoom);
			yield return MoveTo(nearRoom.GetDoorPos(isLeft));
			yield return MoveTo(elevator.GetCenterPos());
			yield return MoveTo(targetNearRoom.GetCenterPos());
			yield return MoveAlongRooms(targetNearRoom, targetRoom);
		}
		else
		{
			yield return MoveTo(targetRoom.GetCenterPos());
		}
	}

	private IEnumerator MoveAlongRooms(zbRoom startRoom, zbRoom endRoom)
	{
		if ((Object)(object)startRoom != (Object)null && startRoom.IsSameRoom(endRoom))
		{
			yield return null;
		}
		zbFloor floor = startRoom.floor;
		int x = startRoom.roomData.X;
		int endX = endRoom.roomData.X;
		bool moveRight = endX > x;
		int num = x;
		zbRoom zbRoom2 = startRoom;
		while (num != endX)
		{
			int nextX = (moveRight ? (num + 1) : (num - 1));
			zbRoom nextRoom = floor.GetRoomAt(nextX);
			if ((Object)(object)nextRoom != (Object)null)
			{
				bool isLeft = !moveRight;
				yield return MoveTo(zbRoom2.GetDoorPos(isLeft));
				yield return MoveTo(nextRoom.GetCenterPos());
				zbRoom2 = nextRoom;
			}
			num = nextX;
		}
	}

	private IEnumerator MoveBetweenFloors(zbFloor currentFloor, zbFloor targetFloor, zbRoom currentRoom, zbRoom targetRoom)
	{
		if ((Object)(object)currentRoom != (Object)null && currentRoom.IsSameRoom(targetRoom))
		{
			yield return null;
		}
		zbElevator currentElevator = currentFloor.elevator;
		zbElevator targetElevator = targetFloor.elevator;
		if ((Object)(object)currentRoom != (Object)null)
		{
			bool isLeft = currentRoom.roomData.X <= 3;
			zbRoom currentNearRoom = currentFloor.GetRoomNearElevator(isLeft);
			yield return MoveAlongRooms(currentRoom, currentNearRoom);
			bool isLeft2 = zbRoomDefine.Instance.ShouldExitFromLeft(currentNearRoom);
			yield return MoveTo(currentNearRoom.GetDoorPos(isLeft2));
			yield return MoveTo(currentElevator.GetCenterPos());
		}
		else
		{
			yield return MoveTo(currentElevator.GetCenterPos());
		}
		yield return EnterElevator(currentElevator);
		yield return (object)new WaitForSeconds(0.5f);
		yield return ExitElevator(targetElevator);
		bool isLeft3 = targetRoom.roomData.X <= 3;
		zbRoom targetNearRoom = targetFloor.GetRoomNearElevator(isLeft3);
		yield return MoveTo(targetNearRoom.GetCenterPos());
		yield return MoveOnSameFloor(targetNearRoom, targetRoom);
	}

	private IEnumerator EnterElevator(zbElevator elevator)
	{
		elevator.OpenDoor();
		yield return MoveTo(elevator.GetInsidePosition());
		elevator.CloseDoor();
		yield return (object)new WaitForSeconds(0.25f);
		m_fsm.worker.m_sortType = ESortMainLayerType.ElevatorInner;
	}

	private IEnumerator ExitElevator(zbElevator elevator)
	{
		Vector3 insidePosition = elevator.GetInsidePosition();
		((Component)m_fsm.worker).transform.position = insidePosition;
		elevator.OpenDoor();
		m_fsm.worker.m_sortType = ESortMainLayerType.Worker;
		Vector3 centerPos = elevator.GetCenterPos();
		yield return MoveTo(centerPos);
		elevator.CloseDoor();
		yield return (object)new WaitForSeconds(0.5f);
	}

	private IEnumerator MoveTo(Vector3 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.worker.SetAnimation("run", loop: true);
		m_fsm.worker.SetFacing(((Component)m_fsm.worker).transform.position.x > targetPos.x);
		targetPos.z = ((Component)m_fsm.worker).transform.position.z;
		while (Vector3.Distance(((Component)m_fsm.worker).transform.position, targetPos) > 0.01f)
		{
			targetPos.z = ((Component)m_fsm.worker).transform.position.z;
			((Component)m_fsm.worker).transform.position = Vector3.MoveTowards(((Component)m_fsm.worker).transform.position, targetPos, zbRoomDefine.Instance.runSpeed * Time.deltaTime);
			yield return null;
		}
	}
}
