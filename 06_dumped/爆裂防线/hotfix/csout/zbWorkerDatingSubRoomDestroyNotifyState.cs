using System.Collections;
using UnityEngine;

public class zbWorkerDatingSubRoomDestroyNotifyState : zbWorkerDatingSubState
{
	private Coroutine m_stateCoroutine;

	public zbWorkerDatingSubRoomDestroyNotifyState(zbWorkerDatingSubFsm fsm, EWorkerDatingSubState eState)
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
		m_fsm.SetState(EWorkerDatingSubState.MoveToRoom);
	}
}
