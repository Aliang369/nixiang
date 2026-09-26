using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;

public class zbDungeonHeroRunState : zbDungeonHeroState
{
	private const float m_checkRadius = 0.6f;

	private Transform m_heroTransform;

	private DungeonMapCellDetail m_mapCellDetail;

	private AudioSource m_runAudioS;

	public zbDungeonHeroRunState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_heroTransform = ((Component)m_fsm.hero).transform;
		m_fsm.hero.SetAnimation("run", loop: true);
	}

	public override void OnExit()
	{
		base.OnExit();
		StopMoveAudio();
	}

	public override void ProcessMoveInput(Vector2 joystickInput)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMoveInput(joystickInput);
		Vector3 val = Vector2.op_Implicit(joystickInput);
		if (val == Vector3.zero)
		{
			m_fsm.SetState(EDungeonHeroState.Idle);
			if ((Object)(object)m_runAudioS != (Object)null)
			{
				zbAudioMgr.Instance.StopAudio(m_runAudioS);
				m_runAudioS = null;
			}
		}
		else
		{
			PlayMoveAudio();
			m_fsm.hero.heroMoveDir = val;
			Move(val);
		}
	}

	public void Move(Vector3 movedir)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_fsm.hero.currentRoom == (Object)null)
		{
			return;
		}
		m_mapCellDetail = m_fsm.hero.currentRoom.dungeonMapCellDetail;
		if (m_fsm.hero.currentRoom.IsElevator() && (movedir == Vector3.up || movedir == Vector3.down))
		{
			zbDungeonRoom nextElevatorRoom = zbDungeonMgr.Instance.GetNextElevatorRoom();
			if ((Object)(object)nextElevatorRoom != (Object)null && nextElevatorRoom.IsElevator() && !(Vector2.Distance(Vector2.op_Implicit(m_heroTransform.position), Vector2.op_Implicit(m_fsm.hero.currentRoom.GetCenterPos())) > 0.8f))
			{
				m_fsm.SetState(EDungeonHeroState.Elevaror);
			}
		}
		else if (movedir == Vector3.left || movedir == Vector3.right)
		{
			m_fsm.hero.SetFacing(m_fsm.hero.heroMoveDir == Vector3.left);
			MoveCheck(movedir);
		}
	}

	public void MoveCheck(Vector3 movedir)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (m_fsm.hero.currentRoom.IsInside(m_heroTransform.position.x, m_heroTransform.position.y))
		{
			MoveCheckInCurRoom(movedir);
		}
		else
		{
			MoveCheckOutCurRoom(movedir);
		}
	}

	public void MoveCheckOutCurRoom(Vector3 movedir)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		float num = m_heroTransform.position.x + movedir.x * m_fsm.hero.moveSpeed * Time.unscaledDeltaTime;
		m_heroTransform.position = new Vector3(num, m_heroTransform.position.y, m_heroTransform.position.z);
		zbDungeonRoom nextRoom = zbDungeonMgr.Instance.GetNextRoom(m_fsm.hero.currentRoom, m_fsm.hero.heroMoveDir == Vector3.left);
		if ((Object)(object)nextRoom != (Object)null && nextRoom.IsInside(m_heroTransform.position.x, m_heroTransform.position.y))
		{
			m_fsm.hero.SetCurrentRoom(nextRoom);
		}
	}

	public void MoveCheckInCurRoom(Vector3 movedir)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		float num = m_heroTransform.position.x + movedir.x * m_fsm.hero.moveSpeed * Time.unscaledDeltaTime;
		if (m_fsm.hero.currentRoom.IsHaveDoor(m_fsm.hero.heroMoveDir == Vector3.left))
		{
			if (Mathf.Abs(m_fsm.hero.currentRoom.GetDoorPos(m_fsm.hero.heroMoveDir == Vector3.left).x - m_heroTransform.position.x) <= 0.6f && !zbDungeonMgr.Instance.dungeonHandler.IsDoorOpen(m_mapCellDetail, m_fsm.hero.heroMoveDir == Vector3.left))
			{
				m_fsm.SetState(EDungeonHeroState.OpenDoor);
				return;
			}
			m_heroTransform.position = new Vector3(num, m_heroTransform.position.y, m_heroTransform.position.z);
		}
		else if (m_fsm.hero.currentRoom.CanMove(num, m_heroTransform.position.y))
		{
			m_heroTransform.position = new Vector3(num, m_heroTransform.position.y, m_heroTransform.position.z);
		}
		TriggerRoomEvent();
	}

	public void TriggerRoomEvent()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		DungeonMapGridDetail roomGridIDetailFromPositionX = m_fsm.hero.currentRoom.GetRoomGridIDetailFromPositionX(m_heroTransform.position.x);
		if (roomGridIDetailFromPositionX != null && roomGridIDetailFromPositionX.EventType != EDungeonEventType.EmptyValue && (roomGridIDetailFromPositionX.Trigger == EDungeonEventTrigger.Passive || roomGridIDetailFromPositionX.Trigger == EDungeonEventTrigger.Active))
		{
			m_fsm.hero.curDungeonMapGridDetail = roomGridIDetailFromPositionX;
			SkeletonAnimation skeletonAnimationInGrid = m_fsm.hero.currentRoom.GetSkeletonAnimationInGrid(roomGridIDetailFromPositionX.GridId);
			if (!((Object)(object)skeletonAnimationInGrid == (Object)null) && Mathf.Abs(((Component)skeletonAnimationInGrid).transform.position.x - m_heroTransform.position.x) <= eventTriggerRange && isFaceToEvent(((Component)skeletonAnimationInGrid).transform))
			{
				m_fsm.SetState(EDungeonHeroState.Event);
			}
		}
	}

	private bool isFaceToEvent(Transform eventTf)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (m_heroTransform.position.x > eventTf.position.x && m_fsm.hero.heroMoveDir == Vector3.left)
		{
			return true;
		}
		if (m_heroTransform.position.x < eventTf.position.x && m_fsm.hero.heroMoveDir == Vector3.right)
		{
			return true;
		}
		return false;
	}
}
