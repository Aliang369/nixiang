using System.Collections;
using UnityEngine;

public class zbRoomDestroySubOutState : zbRoomDestroySubState
{
	private Coroutine m_stateCoroutine;

	public zbRoomDestroySubOutState(zbRoomDestroySubFsm fsm, ERoomDestroySubState eState)
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
		m_stateCoroutine = ((MonoBehaviour)m_fsm.room).StartCoroutine(PlayDestroyOut());
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

	private IEnumerator PlayDestroyOut()
	{
		yield return m_fsm.room.PlayBuildingRoomAnimation("out");
		m_fsm.SetState(ERoomDestroySubState.Exit);
	}
}
