using System;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterMoveState : zbMonsterState
{
	private Vector3 m_direction;

	private float m_wallAlertPositionY;

	public zbMonsterMoveState(zbMonsterStateMachine fsm, EMonsterState eState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		m_direction = Vector3.down;
		base._002Ector(fsm, eState);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_monster.anim.spineAnimation.PlayAnimation("move", loop: true);
		m_wallAlertPositionY = m_monster.combatData.defense.GetAlertPositionY();
		m_monster.anim.spineAnimation.RandomizeCurrentTrackTime();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		Vector3 framePos = m_monster.m_framePos;
		if (m_combatSystem.m_isCelebrating)
		{
			Vector3 val = Vector3.down * (m_combatSystem.scrollSpeed + m_monster.GetAttrValueByType(EAttrMonsterType.MoveSpeed)) * Time.deltaTime;
			m_monster.monsterTF.position = framePos + val;
			return;
		}
		if (m_monster.IsEnterAttackRange() && !m_monster.monsterIndex40151.monsterIndex40151)
		{
			m_fsm.SetState(EMonsterState.Attack);
		}
		else
		{
			Move(framePos);
		}
		if (m_monster.monsterAvatarRecord.avatarId != 13004)
		{
			return;
		}
		zbSpineAnimation spineAnimation = m_monster.anim.spineAnimation;
		TrackEntry current = spineAnimation.skeletonAnimation.AnimationState.GetCurrent(0);
		if (current == null)
		{
			return;
		}
		if (current.Animation.Name == "move")
		{
			if (m_monster.GetAttrValueByType(EAttrMonsterType.MoveSpeed) >= 1f)
			{
				spineAnimation.PlayAnimation("run", loop: true);
			}
		}
		else if (m_monster.GetAttrValueByType(EAttrMonsterType.MoveSpeed) < 1f)
		{
			spineAnimation.PlayAnimation("move", loop: true);
		}
	}

	private void Move(Vector3 curPos)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (m_monster.monsterIndex40151.monsterIndex40151)
		{
			MoveTowardsTarget(new Vector3(m_combatSystem.monster3006Reborn.monster3006PositionX, m_combatSystem.monster3006Reborn.monster3006PositionY, curPos.z), curPos, nearestFlag: true);
			return;
		}
		Vector3 nearestPointOnRoute = GetNearestPointOnRoute(curPos);
		if (IsAboveRouteLine(m_monster.cachPositionY, nearestPointOnRoute.y) && (double)Math.Abs(curPos.x - nearestPointOnRoute.x) > 0.001)
		{
			MoveTowardsTarget(nearestPointOnRoute, curPos, nearestFlag: true);
			return;
		}
		Vector3 targetPos = default(Vector3);
		((Vector3)(ref targetPos))._002Ector(curPos.x, m_wallAlertPositionY, curPos.z);
		MoveTowardsTarget(targetPos, curPos, nearestFlag: false);
	}

	private bool IsAboveRouteLine(float currentPosY, float routePointY)
	{
		return currentPosY > routePointY + 0.01f;
	}

	private Vector3 GetNearestPointOnRoute(Vector3 monsterPos)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		zbCombatRoute combatRoute = m_monster.combatData.combatRoute;
		float routeY = combatRoute.routeY;
		float tsRouteLinePosX = combatRoute.tsRouteLinePosX;
		float num = tsRouteLinePosX - combatRoute.routeWidth / 2f;
		float num2 = tsRouteLinePosX + combatRoute.routeWidth / 2f;
		return new Vector3(Mathf.Clamp(monsterPos.x, num, num2), routeY, monsterPos.z);
	}

	private bool MoveTowardsTarget(Vector3 targetPos, Vector3 curPos, bool nearestFlag)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = targetPos - curPos;
		Vector3 val2 = ((Vector3)(ref val)).normalized;
		float deltaTime = Time.deltaTime;
		Vector3 val3 = Vector3.zero;
		float num = m_monster.GetAttrValueByType(EAttrMonsterType.MoveSpeed);
		if (!nearestFlag && m_monster.monsterIndex10051)
		{
			Vector3 val4;
			if (m_monster.monsterIndex10051Time > 0f)
			{
				val4 = Vector3.down + Vector3.left;
				val2 = ((Vector3)(ref val4)).normalized;
			}
			else
			{
				val4 = Vector3.down + Vector3.right;
				val2 = ((Vector3)(ref val4)).normalized;
			}
			if (m_monster.monsterIndex10051Time <= 0f - m_monster.monsterIndex10051Read)
			{
				m_monster.monsterIndex10051Flag = 1;
			}
			if (m_monster.monsterIndex10051Time >= m_monster.monsterIndex10051Read)
			{
				m_monster.monsterIndex10051Flag = -1;
			}
			m_monster.monsterIndex10051Time += deltaTime * (float)m_monster.monsterIndex10051Flag;
		}
		for (int num2 = m_monster.extraDirectionList.Count - 1; num2 >= 0; num2--)
		{
			zbExtraDirection zbExtraDirection2 = m_monster.extraDirectionList[num2];
			val3 += zbExtraDirection2.direction * zbExtraDirection2.speed;
			zbExtraDirection2.duration -= deltaTime;
			if (zbExtraDirection2.duration <= 0f)
			{
				m_monster.extraDirectionList.RemoveAt(num2);
			}
		}
		if (m_monster.CalBuffStopBool() || !m_monster.canMoveOrAttack)
		{
			val2 = Vector3.zero;
		}
		zbBuffIndex16 zbBuffIndex35 = (zbBuffIndex16)zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(m_monster.GetUUID(), 1601);
		if (zbBuffIndex35 != null)
		{
			num = 0f;
			val3 += Vector3.right * zbBuffIndex35.moveRange;
		}
		if (OverlapsFootholdIndex1901())
		{
			num = 0f;
		}
		Vector3 val5 = val2 * num * Math.Clamp(m_monster.GetSlowDownPercent(), 0f, 1f) * deltaTime;
		if ((m_monster.monsterIndex10441.horizontalSpeed < 0f && m_monster.cachPositionX <= m_monster.combatData.monsterSpawner.GetMinX()) || (m_monster.monsterIndex10441.horizontalSpeed > 0f && m_monster.cachPositionX >= m_monster.combatData.monsterSpawner.GetMaxX()))
		{
			m_monster.monsterIndex10441.horizontalSpeed = 0f - m_monster.monsterIndex10441.horizontalSpeed;
		}
		val5 += new Vector3(m_monster.monsterIndex10441.horizontalSpeed * deltaTime, 0f);
		m_monster.SetSkeletonTimeScale(val5.y / deltaTime, num <= 0f);
		val5 = val5 + val3 * deltaTime + Vector3.down * m_combatSystem.scrollSpeed * deltaTime;
		float min = m_monster.combatData.monsterSpawner.GetMinX() - m_monster.cachPositionX;
		float max = m_monster.combatData.monsterSpawner.GetMaxX() - m_monster.cachPositionX;
		float min2 = m_monster.combatData.defense.GetAlertPositionY() - m_monster.cachPositionY;
		float max2 = m_monster.combatData.monsterSpawner.GetMaxY() - m_monster.cachPositionY;
		val5.x = Math.Clamp(val5.x, min, max);
		val5.y = Math.Clamp(val5.y, min2, max2);
		if (((Vector3)(ref val5)).sqrMagnitude >= ((Vector3)(ref val)).sqrMagnitude)
		{
			if (((Vector3)(ref val)).sqrMagnitude > 0f)
			{
				m_monster.monsterTF.position = targetPos;
			}
			return true;
		}
		if (val5 != Vector3.zero)
		{
			m_monster.monsterTF.position = curPos + val5;
		}
		return false;
	}

	private bool OverlapsFootholdIndex1901()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (m_monster.monsterRecord.moveType == 2)
		{
			return false;
		}
		zbSkillPool zbSkillPool2 = m_monster.combatData.skillPoolMgr.FindSkillString("prefab/combat/foothold/footholdIndex1901.prefab");
		if (zbSkillPool2 != null)
		{
			foreach (zbFootholdIndex1901 activeSkill in zbSkillPool2.m_activeSkills)
			{
				if (activeSkill.boundsCircle2D.Overlaps(m_monster.monsterBoxBounds))
				{
					return true;
				}
			}
		}
		return false;
	}
}
