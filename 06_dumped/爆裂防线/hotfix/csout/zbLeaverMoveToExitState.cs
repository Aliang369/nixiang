using System;
using System.Collections;
using UnityEngine;

public class zbLeaverMoveToExitState : zbLeaverState
{
	private Coroutine m_stateCoroutine;

	public zbLeaverMoveToExitState(zbLeaverStateMachine fsm, ELeaverState eState)
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
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.worker).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_fsm.worker.SetAnimation("walk_dejection", loop: true);
		zbRoom roomDating = zbHomeMgr.Instance.roomMgr.GetRoomDating();
		Vector3 centerPos = roomDating.GetCenterPos();
		if (Math.Abs(((Component)m_fsm.worker).transform.position.y - centerPos.y) < 0.2f && ((Component)m_fsm.worker).transform.position.x < roomDating.GetLeftDoorPos().x)
		{
			m_stateCoroutine = ((MonoBehaviour)m_fsm.worker).StartCoroutine(MoveToWild());
		}
		else
		{
			m_stateCoroutine = ((MonoBehaviour)m_fsm.worker).StartCoroutine(MoveToRoomDating());
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

	public IEnumerator MoveToWild()
	{
		Vector3 targetPos = zbRoomDefine.Instance.RecruiterOuterPosition;
		zbHomeMgr.Instance.wallMgr.leftWall.GetLeftDoorPos();
		Vector3 rightDoorPos = zbHomeMgr.Instance.wallMgr.leftWall.GetRightDoorPos();
		if (((Component)m_fsm.worker).transform.position.x > rightDoorPos.x)
		{
			yield return zbHomeMgr.Instance.wallMgr.leftWall.OpenDoor();
			yield return MoveTo(rightDoorPos);
			yield return zbHomeMgr.Instance.wallMgr.leftWall.CloseDoor(1.5f);
		}
		yield return MoveTo(targetPos);
		zbHomeMgr.Instance.leaverMgr.RemoveLeaver(m_fsm.worker.m_homelandWorkerDetail.WorkerId);
	}

	public IEnumerator MoveToRoomDating()
	{
		zbRoom roomDating = zbHomeMgr.Instance.roomMgr.GetRoomDating();
		yield return MoveToRoom(roomDating);
		yield return MoveToWild();
	}

	public IEnumerator MoveToRoom(zbRoom targetRoom)
	{
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
		yield return (object)new WaitForSeconds(0.25f);
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
