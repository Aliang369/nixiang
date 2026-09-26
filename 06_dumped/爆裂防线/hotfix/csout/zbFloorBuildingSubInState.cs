using System.Collections;
using UnityEngine;

public class zbFloorBuildingSubInState : zbFloorBuildingSubState
{
	private Coroutine m_stateCoroutine;

	public zbFloorBuildingSubInState(zbFloorBuildingSubFsm fsm, EFloorBuildingSubState eState)
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
			((MonoBehaviour)m_fsm.floor).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)m_fsm.floor).StartCoroutine(PlayBuildingIn());
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.floor).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
	}

	private IEnumerator PlayBuildingIn()
	{
		yield return m_fsm.floor.PlayBuildingFloorAnimation("in");
		m_fsm.SetState(EFloorBuildingSubState.Loop);
	}
}
