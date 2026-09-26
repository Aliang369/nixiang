using System.Collections;
using Proto.Types;
using UnityEngine;

public class zbRoomHeroMoveToTopState : zbRoomHeroState
{
	private Coroutine m_stateCoroutine;

	public zbRoomHeroMoveToTopState(zbRoomHeroStateMachine fsm, ERoomHeroState eState)
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
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.roomHero).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		WarRoomDetail warRoomDetailByHeroTid = zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(m_fsm.roomHero.curHeroTid);
		zbRoom room = zbHomeMgr.Instance.roomMgr.GetRoom(warRoomDetailByHeroTid.RoomId);
		if (((Component)m_fsm.roomHero).transform.position.x == zbRoomDefine.Instance.RoomHeroElevatorTopPosition.x)
		{
			m_fsm.roomHero.m_sortType = ESortMainLayerType.Worker;
			zbElevator elevator = room.floor.elevator;
			((Component)m_fsm.roomHero).transform.position = elevator.GetCenterPos();
			m_fsm.SetState(ERoomHeroState.MoveToRoom);
		}
		else
		{
			m_stateCoroutine = ((MonoBehaviour)m_fsm.roomHero).StartCoroutine(MoveToTop(room));
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.roomHero).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
	}

	private IEnumerator MoveToTop(zbRoom targetRoom)
	{
		yield return MoveTo(zbRoomDefine.Instance.RoomHeroElevatorTopPosition);
		yield return EnterTopElevator();
		zbElevator targetElevator = targetRoom.floor.elevator;
		yield return MoveToByDuration(targetElevator.GetInsidePosition(), 0.5f);
		yield return ExitElevator(targetElevator);
		m_fsm.SetState(ERoomHeroState.MoveToRoom);
	}

	private IEnumerator MoveToByDuration(Vector3 targetPos, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.roomHero.SetAnimation("run", loop: true);
		m_fsm.roomHero.SetFacing(((Component)m_fsm.roomHero).transform.position.x > targetPos.x);
		Vector3 startPos = ((Component)m_fsm.roomHero).transform.position;
		float elapsed = 0f;
		targetPos.z = startPos.z;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			Vector3 position = Vector3.Lerp(startPos, targetPos, num);
			position.z = startPos.z;
			((Component)m_fsm.roomHero).transform.position = position;
			yield return null;
		}
		((Component)m_fsm.roomHero).transform.position = targetPos;
	}

	private IEnumerator MoveTo(Vector3 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.roomHero.SetAnimation("run", loop: true);
		m_fsm.roomHero.SetFacing(((Component)m_fsm.roomHero).transform.position.x > targetPos.x);
		targetPos.z = ((Component)m_fsm.roomHero).transform.position.z;
		while (Vector3.Distance(((Component)m_fsm.roomHero).transform.position, targetPos) > 0.01f)
		{
			targetPos.z = ((Component)m_fsm.roomHero).transform.position.z;
			((Component)m_fsm.roomHero).transform.position = Vector3.MoveTowards(((Component)m_fsm.roomHero).transform.position, targetPos, zbRoomDefine.Instance.runSpeed * Time.deltaTime);
			yield return null;
		}
	}

	private IEnumerator EnterTopElevator()
	{
		zbHomeMgr.Instance.roomHeroMgr.elevatorTopLeft.OpenDoor();
		zbHomeMgr.Instance.roomHeroMgr.elevatorTopRight.OpenDoor();
		yield return MoveTo(zbRoomDefine.Instance.RoomHeroElevatorTopPosition + new Vector3(0f, 0.25f, 0f));
		zbHomeMgr.Instance.roomHeroMgr.elevatorTopLeft.CloseDoor();
		zbHomeMgr.Instance.roomHeroMgr.elevatorTopRight.CloseDoor();
		yield return (object)new WaitForSeconds(0.25f);
		m_fsm.roomHero.m_sortType = ESortMainLayerType.Back;
		yield return null;
	}

	private IEnumerator ExitElevator(zbElevator elevator)
	{
		elevator.OpenDoor();
		m_fsm.roomHero.m_sortType = ESortMainLayerType.Worker;
		Vector3 centerPos = elevator.GetCenterPos();
		yield return MoveTo(centerPos);
		elevator.CloseDoor();
		yield return (object)new WaitForSeconds(0.5f);
	}
}
