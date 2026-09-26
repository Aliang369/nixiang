using System.Collections;
using Spine;
using UnityEngine;

public class zbLeaverChangeClothState : zbLeaverState
{
	private Coroutine m_stateCoroutine;

	public zbLeaverChangeClothState(zbLeaverStateMachine fsm, ELeaverState eState)
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
		base.OnEnable();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.worker).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_fsm.worker.SetAnimation("idle", loop: true);
		m_stateCoroutine = ((MonoBehaviour)m_fsm.worker).StartCoroutine(SetupClothAnimation());
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

	public IEnumerator SetupClothAnimation()
	{
		TrackEntry val = m_fsm.worker.SetAnimation("action_cloth", loop: false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.Animation.Duration);
		}
		yield return null;
		yield return null;
		m_fsm.worker.RefreshInitSuit();
		m_fsm.SetState(ELeaverState.MoveToExit);
	}
}
