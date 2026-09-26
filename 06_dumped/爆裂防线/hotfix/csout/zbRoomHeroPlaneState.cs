using System.Collections;
using UnityEngine;

public class zbRoomHeroPlaneState : zbRoomHeroState
{
	private Coroutine m_stateCoroutine;

	public zbRoomHeroPlaneState(zbRoomHeroStateMachine fsm, ERoomHeroState eState)
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
		m_stateCoroutine = ((MonoBehaviour)m_fsm.roomHero).StartCoroutine(MoveOutPlane());
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

	public IEnumerator MoveOutPlane()
	{
		yield return MoveToByDuration(zbHomeMgr.Instance.roomHeroMgr.plane.GetInsidePosition(), 2f);
		m_fsm.roomHero.m_sortType = ESortMainLayerType.RoomGoldSpine;
		yield return MoveTo(zbHomeMgr.Instance.roomHeroMgr.plane.GetCenterPos());
		m_fsm.SetState(ERoomHeroState.MoveToTop);
	}

	private IEnumerator MoveToByDuration(Vector3 targetPos, float duration)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.roomHero.SetAnimation("run", loop: true);
		m_fsm.roomHero.SetFacing(((Component)m_fsm.roomHero).transform.position.x > targetPos.x);
		Vector3 startPos = ((Component)m_fsm.roomHero).transform.position;
		float elapsed = 0f;
		targetPos.z = startPos.z;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			Vector3 position = Vector3.Lerp(startPos, targetPos, num);
			position.z = startPos.z;
			((Component)m_fsm.roomHero).transform.position = position;
			yield return null;
		}
		((Component)m_fsm.roomHero).transform.position = targetPos;
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
