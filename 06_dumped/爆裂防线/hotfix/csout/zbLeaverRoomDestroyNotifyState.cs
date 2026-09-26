using System.Collections;
using UnityEngine;

public class zbLeaverRoomDestroyNotifyState : zbLeaverState
{
	private Coroutine m_stateCoroutine;

	public zbLeaverRoomDestroyNotifyState(zbLeaverStateMachine fsm, ELeaverState eState)
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
			((MonoBehaviour)m_fsm.worker).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)m_fsm.worker).StartCoroutine(WaitAMoment());
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

	private IEnumerator WaitAMoment()
	{
		yield return null;
		yield return null;
		m_fsm.SetState(ELeaverState.MoveToExit);
	}
}
