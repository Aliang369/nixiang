using System.Collections;
using UnityEngine;

public class zbDoorCloseState : zbDoorState
{
	private Coroutine m_stateCoroutine;

	public zbDoorCloseState(zbDoorStateMachine fsm, EDoorState eState)
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
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.room).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)m_fsm.room).StartCoroutine(CloseDoorCoroutine());
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.room).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
	}

	public IEnumerator CloseDoorCoroutine()
	{
		yield return m_fsm.room.CloseDoor(m_fsm.isLeftDoor);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (IsWorkerNearDoor())
		{
			m_fsm.SetState(EDoorState.Open);
		}
	}
}
