using System.Collections;
using UnityEngine;

public class zbFloorBuildingSubOutState : zbFloorBuildingSubState
{
	private Coroutine m_stateCoroutine;

	public zbFloorBuildingSubOutState(zbFloorBuildingSubFsm fsm, EFloorBuildingSubState eState)
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
		m_stateCoroutine = ((MonoBehaviour)m_fsm.floor).StartCoroutine(PlayBuildingOut());
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

	private IEnumerator PlayBuildingOut()
	{
		yield return m_fsm.floor.PlayBuildingFloorAnimation("out");
		m_fsm.SetState(EFloorBuildingSubState.Exit);
	}
}
