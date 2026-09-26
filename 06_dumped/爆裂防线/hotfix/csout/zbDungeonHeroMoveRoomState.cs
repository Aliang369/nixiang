using System.Collections;
using UnityEngine;

public class zbDungeonHeroMoveRoomState : zbDungeonHeroState
{
	private bool isCoroutineRunning;

	private Coroutine actionCoroutine;

	private Transform m_heroTransform;

	public zbDungeonHeroMoveRoomState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		zbAudioMgr.Instance.PlayAudio("audio/ui/open_door.ogg");
		base.OnEnter();
		m_heroTransform = ((Component)m_fsm.hero).transform;
		m_fsm.hero.SetAnimation("idle", loop: true);
		zbDungeonMgr.Instance.dungeonWnd.SetActionProgress(visible: false, 0f, Vector3.zero);
		isCoroutineRunning = true;
		PlayMoveAudio();
		if (m_fsm.hero.targetRoom.IsElevator() && m_fsm.hero.currentRoom.IsElevator())
		{
			actionCoroutine = ((MonoBehaviour)m_fsm.hero).StartCoroutine(MoveElevator());
		}
		else
		{
			actionCoroutine = ((MonoBehaviour)m_fsm.hero).StartCoroutine(MoveRoom());
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		StopAction();
		StopMoveAudio();
	}

	private void StopAction()
	{
		if (isCoroutineRunning && actionCoroutine != null)
		{
			((MonoBehaviour)m_fsm.hero).StopCoroutine(actionCoroutine);
			isCoroutineRunning = false;
			actionCoroutine = null;
		}
	}

	public override void ProcessMoveInput(Vector2 joystickInput)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMoveInput(joystickInput);
	}

	private IEnumerator MoveRoom()
	{
		yield return (object)new WaitForSeconds(0.3f);
		m_fsm.hero.SetFacing(m_fsm.hero.targetPosition.x < m_heroTransform.position.x);
		m_fsm.hero.SetAnimation("run", loop: true);
		m_fsm.hero.targetPosition.z = m_heroTransform.position.z;
		while (Vector2.Distance(Vector2.op_Implicit(m_heroTransform.position), Vector2.op_Implicit(m_fsm.hero.targetPosition)) > 0.01f)
		{
			m_heroTransform.position = Vector3.MoveTowards(m_heroTransform.position, m_fsm.hero.targetPosition, m_fsm.hero.moveSpeed * Time.unscaledDeltaTime);
			yield return null;
		}
		m_heroTransform.position = m_fsm.hero.targetPosition;
		yield return (object)new WaitForSeconds(0.1f);
		m_fsm.hero.SetCurrentRoom(m_fsm.hero.targetRoom);
		m_fsm.SetState(EDungeonHeroState.Run);
	}

	private IEnumerator MoveElevator()
	{
		zbDungeonRoom curRoom = m_fsm.hero.currentRoom;
		zbDungeonRoom nextRoom = m_fsm.hero.targetRoom;
		float elevatorOpenTime = 0.5f;
		curRoom.PlayElevatorAnimation(isOpen: true);
		yield return (object)new WaitForSeconds(0.5f);
		m_fsm.hero.SetAnimation("run", loop: true);
		Vector3 centerPos = curRoom.GetCenterPos();
		Vector3 curEnterStart = new Vector3(centerPos.x, centerPos.y + 0.3f, m_heroTransform.position.z);
		m_fsm.hero.SetFacing(m_heroTransform.position.x > centerPos.x);
		while (Vector2.Distance(Vector2.op_Implicit(m_heroTransform.position), Vector2.op_Implicit(curEnterStart)) > 0.01f)
		{
			m_heroTransform.position = Vector3.MoveTowards(m_heroTransform.position, curEnterStart, m_fsm.hero.moveSpeed * Time.unscaledDeltaTime);
			yield return null;
		}
		m_fsm.hero.SetAnimation("idle", loop: true);
		yield return (object)new WaitForSeconds(0.1f);
		m_fsm.hero.SetHeroVisible(visible: false);
		curRoom.PlayElevatorAnimation(isOpen: false);
		yield return (object)new WaitForSeconds(elevatorOpenTime);
		Vector3 nextRoomCenter = new Vector3(nextRoom.GetCenterPos().x, nextRoom.GetCenterPos().y + 0.3f, m_heroTransform.position.z);
		while (Vector2.Distance(Vector2.op_Implicit(m_heroTransform.position), Vector2.op_Implicit(nextRoomCenter)) > 0.01f)
		{
			m_heroTransform.position = Vector3.MoveTowards(m_heroTransform.position, nextRoomCenter, m_fsm.hero.moveSpeed * Time.unscaledDeltaTime);
			yield return null;
		}
		nextRoom.PlayElevatorAnimation(isOpen: true);
		yield return (object)new WaitForSeconds(elevatorOpenTime);
		m_fsm.hero.SetAnimation("run", loop: true);
		m_fsm.hero.SetHeroVisible(visible: true);
		Vector3 exitPos = nextRoom.GetCenterPos();
		exitPos.y += 0.08f;
		exitPos.z = m_heroTransform.position.z;
		while (Vector2.Distance(Vector2.op_Implicit(m_heroTransform.position), Vector2.op_Implicit(exitPos)) > 0.01f)
		{
			m_heroTransform.position = Vector3.MoveTowards(m_heroTransform.position, exitPos, m_fsm.hero.moveSpeed * Time.unscaledDeltaTime);
			yield return null;
		}
		m_heroTransform.position = exitPos;
		m_fsm.hero.SetAnimation("idle", loop: true);
		nextRoom.PlayElevatorAnimation(isOpen: false);
		m_fsm.hero.SetCurrentRoom(nextRoom);
		m_fsm.SetState(EDungeonHeroState.Idle);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
