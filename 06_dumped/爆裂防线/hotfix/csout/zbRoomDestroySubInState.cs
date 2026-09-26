using System.Collections;
using UnityEngine;

public class zbRoomDestroySubInState : zbRoomDestroySubState
{
	private Coroutine m_stateCoroutine;

	public zbRoomDestroySubInState(zbRoomDestroySubFsm fsm, ERoomDestroySubState eState)
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
		m_stateCoroutine = ((MonoBehaviour)m_fsm.room).StartCoroutine(PlayDestroyIn());
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

	private IEnumerator PlayDestroyIn()
	{
		yield return m_fsm.room.PlayBuildingRoomAnimation("in");
		m_fsm.SetState(ERoomDestroySubState.Loop);
	}
}
