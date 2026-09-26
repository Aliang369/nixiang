using System.Collections;
using UnityEngine;

public class zbRoomBuildingSubLoopState : zbRoomBuildingSubState
{
	private Coroutine m_stateCoroutine;

	public zbRoomBuildingSubLoopState(zbRoomBuildingSubFsm fsm, ERoomBuildingSubState eState)
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
		m_stateCoroutine = ((MonoBehaviour)m_fsm.room).StartCoroutine(PlayBuildingLoop());
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

	private IEnumerator PlayBuildingLoop()
	{
		yield return m_fsm.room.PlayBuildingRoomAnimation("loop");
		m_fsm.SetState(ERoomBuildingSubState.Out);
	}
}
