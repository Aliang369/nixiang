using System.Collections;
using UnityEngine;

public class zbRoomBuildingSubOutState : zbRoomBuildingSubState
{
	private Coroutine m_stateCoroutine;

	public zbRoomBuildingSubOutState(zbRoomBuildingSubFsm fsm, ERoomBuildingSubState eState)
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
		m_stateCoroutine = ((MonoBehaviour)m_fsm.room).StartCoroutine(PlayBuildingOut());
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

	private IEnumerator PlayBuildingOut()
	{
		((Component)m_fsm.room.m_spriteRenderer).gameObject.SetActive(true);
		m_fsm.room.floor.RefreshMaskVisible();
		yield return m_fsm.room.PlayBuildingRoomAnimation("out");
		m_fsm.SetState(ERoomBuildingSubState.Exit);
	}
}
