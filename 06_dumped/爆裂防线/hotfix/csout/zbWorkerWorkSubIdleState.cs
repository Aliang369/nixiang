using System.Collections;
using UnityEngine;

public class zbWorkerWorkSubIdleState : zbWorkerWorkSubState
{
	private Coroutine m_stateCoroutine;

	public zbWorkerWorkSubIdleState(zbWorkerWorkSubFsm fsm, EWorkerWorkSubState eState)
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
		string workAnimationName = m_fsm.worker.GetWorkAnimationName();
		bool isWalkWorkAniation = IsWalkWorkAnimation(workAnimationName);
		while (true)
		{
			zbRoom currRoom = zbHomeMgr.Instance.roomMgr.GetRoomByPosition(((Component)m_fsm.worker).transform.position);
			if ((Object)(object)currRoom == (Object)null)
			{
				break;
			}
			Vector3 targetPos = currRoom.GetRandomWalkablePos();
			if (isWalkWorkAniation)
			{
				m_fsm.worker.SetAnimation(workAnimationName, loop: true);
				targetPos.z = ((Component)m_fsm.worker).transform.position.z;
				while (Vector3.Distance(((Component)m_fsm.worker).transform.position, targetPos) > 0.1f)
				{
					targetPos.z = ((Component)m_fsm.worker).transform.position.z;
					m_fsm.worker.SetFacing(((Component)m_fsm.worker).transform.position.x > targetPos.x);
					((Component)m_fsm.worker).transform.position = Vector3.MoveTowards(((Component)m_fsm.worker).transform.position, targetPos, zbRoomDefine.Instance.walkSpeed * Time.deltaTime);
					yield return null;
				}
				m_fsm.worker.SetAnimation("idle", loop: true);
				yield return (object)new WaitForSeconds(Rand.Range(3f, 8f));
				continue;
			}
			if (Vector3.Distance(((Component)m_fsm.worker).transform.position, targetPos) < 0.3f)
			{
				yield return (object)new WaitForSeconds(Rand.Range(0.5f, 1.5f));
				continue;
			}
			m_fsm.worker.SetAnimation("walk", loop: true);
			targetPos.z = ((Component)m_fsm.worker).transform.position.z;
			while (Vector3.Distance(((Component)m_fsm.worker).transform.position, targetPos) > 0.1f)
			{
				if ((Object)(object)currRoom == (Object)null)
				{
					yield break;
				}
				targetPos.z = ((Component)m_fsm.worker).transform.position.z;
				m_fsm.worker.SetFacing(((Component)m_fsm.worker).transform.position.x > targetPos.x);
				((Component)m_fsm.worker).transform.position = Vector3.MoveTowards(((Component)m_fsm.worker).transform.position, targetPos, zbRoomDefine.Instance.walkSpeed * Time.deltaTime);
				yield return null;
			}
			m_fsm.worker.SetAnimation("idle", loop: true);
			yield return (object)new WaitForSeconds(0.5f);
			m_fsm.worker.SetAnimation(workAnimationName, loop: true);
			yield return (object)new WaitForSeconds(Rand.Range(4f, 6f));
			m_fsm.worker.SetAnimation("idle", loop: true);
			yield return (object)new WaitForSeconds(Rand.Range(2f, 3f));
		}
		m_fsm.worker.SetAnimation("idle", loop: true);
	}

	private bool IsWalkWorkAnimation(string animationName)
	{
		return animationName == "action_plate";
	}
}
