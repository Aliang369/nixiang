using System.Collections;
using Proto.Types;
using UnityEngine;

public class zbRoomHeroIdleState : zbRoomHeroState
{
	private Coroutine m_stateCoroutine;

	public zbRoomHeroIdleState(zbRoomHeroStateMachine fsm, ERoomHeroState eState)
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
			((MonoBehaviour)m_fsm.roomHero).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
		WarRoomDetail warRoomDetailByHeroTid = zbSelf.Instance.heroHandler.GetWarRoomDetailByHeroTid(m_fsm.roomHero.curHeroTid);
		zbRoom room = zbHomeMgr.Instance.roomMgr.GetRoom(warRoomDetailByHeroTid.RoomId);
		if (Object.op_Implicit((Object)(object)room))
		{
			room.RefreshRoomState();
		}
		m_stateCoroutine = ((MonoBehaviour)m_fsm.roomHero).StartCoroutine(IdleWalkLoop());
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_stateCoroutine != null)
		{
			((MonoBehaviour)m_fsm.roomHero).StopCoroutine(m_stateCoroutine);
			m_stateCoroutine = null;
		}
	}

	private IEnumerator IdleWalkLoop()
	{
		while (true)
		{
			zbRoom roomByPosition = zbHomeMgr.Instance.roomMgr.GetRoomByPosition(((Component)m_fsm.roomHero).transform.position);
			if (!((Object)(object)roomByPosition == (Object)null))
			{
				Vector3 targetPos = roomByPosition.GetRandomWalkablePos();
				m_fsm.roomHero.SetAnimation("walk", loop: true);
				targetPos.z = ((Component)m_fsm.roomHero).transform.position.z;
				while (Vector3.Distance(((Component)m_fsm.roomHero).transform.position, targetPos) > 0.1f)
				{
					targetPos.z = ((Component)m_fsm.roomHero).transform.position.z;
					m_fsm.roomHero.SetFacing(((Component)m_fsm.roomHero).transform.position.x > targetPos.x);
					((Component)m_fsm.roomHero).transform.position = Vector3.MoveTowards(((Component)m_fsm.roomHero).transform.position, targetPos, zbRoomDefine.Instance.walkSpeed * Time.deltaTime);
					yield return null;
				}
				m_fsm.roomHero.SetAnimation("idle", loop: true);
				yield return (object)new WaitForSeconds(Rand.Range(2f, 4f));
				continue;
			}
			break;
		}
	}
}
