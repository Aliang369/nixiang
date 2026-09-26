using System.Collections;
using UnityEngine;

public class zbWorkerDatingSubIdleState : zbWorkerDatingSubState
{
	private Coroutine m_stateCoroutine;

	public zbWorkerDatingSubIdleState(zbWorkerDatingSubFsm fsm, EWorkerDatingSubState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.worker.RefreshRoomSuit();
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
		m_stateCoroutine = ((MonoBehaviour)m_fsm.worker).StartCoroutine(IdleWalkLoop());
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

	private IEnumerator IdleWalkLoop()
	{
		while (true)
		{
			zbRoom roomByPosition = zbHomeMgr.Instance.roomMgr.GetRoomByPosition(((Component)m_fsm.worker).transform.position);
			if (!((Object)(object)roomByPosition == (Object)null))
			{
				Vector3 targetPos = roomByPosition.GetRandomWalkablePos();
				m_fsm.worker.SetAnimation("walk", loop: true);
				targetPos.z = ((Component)m_fsm.worker).transform.position.z;
				while (Vector3.Distance(((Component)m_fsm.worker).transform.position, targetPos) > 0.1f)
				{
					targetPos.z = ((Component)m_fsm.worker).transform.position.z;
					m_fsm.worker.SetFacing(((Component)m_fsm.worker).transform.position.x > targetPos.x);
					((Component)m_fsm.worker).transform.position = Vector3.MoveTowards(((Component)m_fsm.worker).transform.position, targetPos, zbRoomDefine.Instance.walkSpeed * Time.deltaTime);
					yield return null;
				}
				m_fsm.worker.SetAnimation("idle", loop: true);
				yield return (object)new WaitForSeconds(Rand.Range(2f, 4f));
				continue;
			}
			break;
		}
	}
}
