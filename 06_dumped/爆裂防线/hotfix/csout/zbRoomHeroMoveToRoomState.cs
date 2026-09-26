using System.Collections;
using Proto.Types;
using UnityEngine;

public class zbRoomHeroMoveToRoomState : zbRoomHeroState
{
	private Coroutine m_stateCoroutine;

	public zbRoomHeroMoveToRoomState(zbRoomHeroStateMachine fsm, ERoomHeroState eState)
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
		if ((Object)(object)room == (Object)null)
		{
			m_fsm.SetState(ERoomHeroState.Idle);
		}
		else
		{
			m_stateCoroutine = ((MonoBehaviour)m_fsm.roomHero).StartCoroutine(MoveToRoom(room));
		}
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

	public IEnumerator MoveToRoom(zbRoom targetRoom)
	{
		yield return MoveTo(targetRoom.GetCenterPos());
		targetRoom.fsm.SetState(ERoomState.Building);
		yield return (object)new WaitForSeconds(1.8f);
		m_fsm.SetState(ERoomHeroState.Idle);
	}

	private IEnumerator MoveTo(Vector3 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.roomHero.SetAnimation("run", loop: true);
		m_fsm.roomHero.SetFacing(((Component)m_fsm.roomHero).transform.position.x > targetPos.x);
		targetPos.z = ((Component)m_fsm.roomHero).transform.position.z;
		while (Vector3.Distance(((Component)m_fsm.roomHero).transform.position, targetPos) > 0.01f)
		{
			targetPos.z = ((Component)m_fsm.roomHero).transform.position.z;
			((Component)m_fsm.roomHero).transform.position = Vector3.MoveTowards(((Component)m_fsm.roomHero).transform.position, targetPos, zbRoomDefine.Instance.runSpeed * Time.deltaTime);
			yield return null;
		}
	}
}
