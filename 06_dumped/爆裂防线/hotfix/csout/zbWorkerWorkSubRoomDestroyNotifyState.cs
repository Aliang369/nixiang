using System.Collections;
using UnityEngine;

public class zbWorkerWorkSubRoomDestroyNotifyState : zbWorkerWorkSubState
{
	private Coroutine m_stateCoroutine;

	public zbWorkerWorkSubRoomDestroyNotifyState(zbWorkerWorkSubFsm fsm, EWorkerWorkSubState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
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
		m_fsm.SetState(EWorkerWorkSubState.MoveToRoom);
	}
}
