using System.Collections;
using UnityEngine;

public class zbLegionHeroFloor : zbLegionHeroState
{
	private Coroutine m_floorCt;

	public zbLegionHeroFloor(zbLegionHeroMachine fsm)
		: base(fsm)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_floorCt != null)
		{
			((MonoBehaviour)legionHero).StopCoroutine(m_floorCt);
		}
		m_floorCt = ((MonoBehaviour)legionHero).StartCoroutine(FloorRoutine());
	}

	private IEnumerator FloorRoutine()
	{
		legionHero.PlayAnimation("walk");
		legionHero.SetFacing(left: false);
		int fromFloor = legionHero.curFloor;
		int targetFloor = ((fromFloor == 0) ? 1 : 0);
		Vector2 floorElePos = legionHero.floorElePos[legionHero.curFloor];
		Vector2 tarfloorElePos = legionHero.floorElePos[targetFloor];
		float distance = Vector2.Distance(Vector2.op_Implicit(((Component)legionHero).transform.position), floorElePos);
		while (distance > 0.01f)
		{
			Vector3 position = ((Component)legionHero).transform.position;
			Vector2 val = Vector2.MoveTowards(Vector2.op_Implicit(position), floorElePos, legionHero.moveSpeed * Time.unscaledDeltaTime);
			((Component)legionHero).transform.position = new Vector3(val.x, val.y, position.z);
			distance = Vector2.Distance(Vector2.op_Implicit(position), floorElePos);
			yield return null;
		}
		((Component)legionHero).transform.position = new Vector3(floorElePos.x, floorElePos.y, ((Component)legionHero).transform.position.z);
		zbLegionSceneMgr.Instance.PlayDoorAnimation(fromFloor + 1, "anim_open", loop: false);
		yield return (object)new WaitForSeconds(0.5f);
		legionHero.SetPlayerVisible(visible: false);
		legionHero.PlayAnimation("idle");
		zbLegionSceneMgr.Instance.PlayDoorAnimation(fromFloor + 1, "anim_close", loop: false, addClose: true);
		yield return (object)new WaitForSeconds(1.5f);
		zbLegionSceneMgr.Instance.PlayDoorAnimation(targetFloor + 1, "anim_open", loop: false);
		legionHero.curFloor = targetFloor;
		MoveIn(new Vector3(tarfloorElePos.x, tarfloorElePos.y, ((Component)legionHero).transform.position.z));
		yield return (object)new WaitForSeconds(0.5f);
		zbLegionSceneMgr.Instance.PlayDoorAnimation(targetFloor + 1, "anim_close", loop: false, addClose: true);
		m_fsm.SetState(ELegionHeroState.Idle);
		yield return null;
	}

	private void MoveIn(Vector3 pos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		((Component)legionHero).transform.position = pos;
		legionHero.SetFacing(left: true);
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_floorCt != null)
		{
			((MonoBehaviour)legionHero).StopCoroutine(m_floorCt);
			m_floorCt = null;
		}
		m_fsm.exitChangeFloor = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
