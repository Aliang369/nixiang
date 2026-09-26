using System.Collections;
using UnityEngine;

public class zbZombieMoveState : zbZombieState
{
	private Coroutine m_stateCoroutine;

	public zbZombieMoveState(zbZombieStateMachine fsm, EZombieState eState)
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
			((MonoBehaviour)m_fsm.zombie).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		m_stateCoroutine = ((MonoBehaviour)m_fsm.zombie).StartCoroutine(MoveToTargetPosition());
	}

	public override void OnExit()
	{
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.zombie).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		base.OnExit();
	}

	public IEnumerator MoveToTargetPosition()
	{
		m_fsm.zombie.SetAnimation("walk_zombie", loop: true);
		Vector3 zombieTargetPosition = zbRoomDefine.Instance.ZombieTargetPosition;
		Vector3 targetPos = new Vector3(zombieTargetPosition.x + Rand.Range(-0.5f, 0.5f), ((Component)m_fsm.zombie).transform.position.y, zombieTargetPosition.z);
		m_fsm.zombie.SetFacing(((Component)m_fsm.zombie).transform.position.x > targetPos.x);
		targetPos.z = ((Component)m_fsm.zombie).transform.position.z;
		while (Vector3.Distance(((Component)m_fsm.zombie).transform.position, targetPos) > 0.01f)
		{
			targetPos.z = ((Component)m_fsm.zombie).transform.position.z;
			((Component)m_fsm.zombie).transform.position = Vector3.MoveTowards(((Component)m_fsm.zombie).transform.position, targetPos, zbRoomDefine.Instance.walkSpeed * Time.deltaTime);
			yield return null;
		}
		zbHomeMgr.Instance.paotaiMgr.paotai.ForceSweep();
		m_fsm.SetState(EZombieState.Idle);
	}
}
