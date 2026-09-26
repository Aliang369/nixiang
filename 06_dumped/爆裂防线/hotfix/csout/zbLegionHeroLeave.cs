using System.Collections;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine;

public class zbLegionHeroLeave : zbLegionHeroState
{
	private Coroutine m_refreshNewCt;

	public zbLegionHeroLeave(zbLegionHeroMachine fsm)
		: base(fsm)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_refreshNewCt != null)
		{
			((MonoBehaviour)legionHero).StopCoroutine(m_refreshNewCt);
			m_refreshNewCt = null;
		}
		m_refreshNewCt = ((MonoBehaviour)legionHero).StartCoroutine(MoveAndRefresh());
	}

	private IEnumerator MoveAndRefresh()
	{
		yield return ((MonoBehaviour)legionHero).StartCoroutine(MoveToGate());
		yield return ((MonoBehaviour)legionHero).StartCoroutine(RefreshNewMember());
	}

	private IEnumerator MoveToGate()
	{
		legionHero.PlayAnimation("run");
		legionHero.SetFacing(left: true);
		Vector3 position = ((Component)legionHero).transform.position;
		Vector3 targetPos = new Vector3(legionHero.floor1GatePos.x, legionHero.floor1GatePos.y, position.z);
		float distance = Vector2.Distance(Vector2.op_Implicit(position), Vector2.op_Implicit(targetPos));
		while (distance > 0.01f)
		{
			((Component)legionHero).transform.position = Vector3.MoveTowards(((Component)legionHero).transform.position, targetPos, legionHero.moveSpeed * 2f * Time.unscaledDeltaTime);
			distance = Vector3.Distance(((Component)legionHero).transform.position, targetPos);
			yield return null;
		}
		((Component)legionHero).transform.position = targetPos;
		zbLegionSceneMgr.Instance.PlayDoorAnimation(0, "anim_open", loop: false);
		yield return (object)new WaitForSeconds(0.5f);
		legionHero.SetPlayerVisible(visible: false);
		legionHero.PlayAnimation("idle");
		zbLegionSceneMgr.Instance.PlayDoorAnimation(0, "anim_close", loop: false, addClose: true);
		yield return (object)new WaitForSeconds(0.5f);
	}

	private IEnumerator RefreshNewMember()
	{
		yield return (object)new WaitForSeconds((float)Rand.Range(2, 6));
		RepeatedField<LegionMemberDetail> memberList = zbSelf.Instance.legionHandler.memberList;
		if (memberList != null && memberList.Count > 0)
		{
			for (int i = 0; i < memberList.Count; i++)
			{
				LegionMemberDetail legionMemberDetail = memberList[i];
				if (!IsInHeroList(legionMemberDetail.RoleBaseInfo.RoleId))
				{
					legionHero.RefreshMemberInfo(legionMemberDetail);
					break;
				}
			}
		}
		zbLegionSceneMgr.Instance.PlayDoorAnimation(0, "anim_open", loop: false);
		yield return (object)new WaitForSeconds(0.5f);
		MoveIn();
		zbLegionSceneMgr.Instance.PlayDoorAnimation(0, "anim_close", loop: false, addClose: true);
		yield return (object)new WaitForSeconds(0.5f);
		m_fsm.SetState(ELegionHeroState.Idle);
	}

	private void MoveIn()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		((Component)legionHero).transform.position = new Vector3(legionHero.floor1GatePos.x, legionHero.floor1GatePos.y, ((Component)legionHero).transform.position.z);
		legionHero.SetPlayerVisible(visible: true);
		legionHero.SetFacing(left: false);
	}

	private bool IsInHeroList(long roleId)
	{
		foreach (zbLegionHero hero in zbLegionSceneMgr.Instance.heroList)
		{
			if (hero.legionMemberDetail != null && hero.legionMemberDetail.RoleBaseInfo.RoleId == roleId)
			{
				return true;
			}
		}
		return false;
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_refreshNewCt != null)
		{
			((MonoBehaviour)legionHero).StopCoroutine(m_refreshNewCt);
			m_refreshNewCt = null;
		}
		m_fsm.exitLeaveFloor = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
