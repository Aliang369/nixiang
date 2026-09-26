using System;
using Proto.Enums;
using UnityEngine;

public class zbMonsterAttackState(zbMonsterStateMachine fsm, EMonsterState eState) : zbMonsterState(fsm, eState)
{
	private bool m_bCanAttack = true;

	private float nextFireTime;

	private const float SHOOT_DELAY_TIME = 7f / 15f;

	private const float MONSTER_10112_DELAY_TIME = 5f / 6f;

	private float m_shootTimer = -1f;

	private float m_monster10112Timer = -1f;

	public override void OnEnter()
	{
		base.OnEnter();
		nextFireTime = 0f;
		m_shootTimer = -1f;
		m_monster10112Timer = -1f;
		m_monster.anim.spineAnimation.PlayAnimation("idle", loop: true);
		m_monster.anim.spineAnimation.SetTimeScale(1f);
		m_monster.anim.spineAnimation.StopAnimation(1);
	}

	public override void OnExit()
	{
		base.OnExit();
		m_shootTimer = -1f;
		m_monster10112Timer = -1f;
	}

	public override void OnUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		if (zbCombatSystem.Instance.m_isCelebrating)
		{
			Vector3 val = Vector3.down * zbCombatSystem.Instance.scrollSpeed * Time.deltaTime;
			Transform monsterTF = m_monster.monsterTF;
			monsterTF.position += val;
			return;
		}
		Move();
		if (!m_monster.isUsingSprite)
		{
			m_monster.SetSkeletonTimeScale(m_monster.bodyScale * (float)m_monster.monsterRecord.artSpeed / 100f);
		}
		if (m_monster.CalBuffStopBool() || !m_monster.canMoveOrAttack)
		{
			nextFireTime = 0f;
			m_bCanAttack = false;
			m_shootTimer = -1f;
		}
		else if (m_monster.monsterIndex10112 && m_monster.monsterIndex10112Action)
		{
			UpdateLogicTimers();
		}
		else if (m_monster.monsterIndex40421.monsterIndex40421)
		{
			((MonoBehaviour)m_monster).StartCoroutine(m_monster.monsterIndex40421.monsterSkill.ActionTriggerAttackSkill());
		}
		else if (m_monster.monsterSkillIndex3011 == 3 && m_monster.monsterSkillIndex30113.bigActionNum <= 0)
		{
			((MonoBehaviour)m_monster).StartCoroutine(m_monster.monster30113Class.BigDamage());
		}
		else if (m_monster.IsEnterAttackRange(0.2f))
		{
			m_bCanAttack = true;
			if (m_monster.attackCount == 0)
			{
				m_monster.attackCount++;
				zbBuffIndex13 zbBuffIndex35 = (zbBuffIndex13)zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(m_monster.GetUUID(), 24);
				if (zbBuffIndex35 != null)
				{
					zbCombatSystem.Instance.buffSystem.CreateBuff(zbBuffIndex35.skillBase, zbBuffIndex35.skillBase.skillAttrList, m_monster.GetUUID(), (int)zbBuffIndex35.buffRecord.param);
					return;
				}
			}
			if (m_bCanAttack && m_fsm.GetStateId() != EMonsterState.Die)
			{
				UpdateAttackWall();
			}
			else
			{
				nextFireTime = 0f;
			}
			UpdateLogicTimers();
		}
		else
		{
			m_fsm.SetState(EMonsterState.Move);
		}
	}

	private void Move()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		for (int num = m_monster.extraDirectionList.Count - 1; num >= 0; num--)
		{
			zbExtraDirection zbExtraDirection2 = m_monster.extraDirectionList[num];
			val += zbExtraDirection2.direction * zbExtraDirection2.speed;
			zbExtraDirection2.duration -= Time.deltaTime;
			if (zbExtraDirection2.duration <= 0f)
			{
				m_monster.extraDirectionList.RemoveAt(num);
			}
		}
		Vector3 val2 = val * Time.deltaTime;
		float alertPositionY = m_monster.combatData.defense.GetAlertPositionY();
		float min = m_monster.combatData.monsterSpawner.GetMinX() - m_monster.cachPositionX;
		float max = m_monster.combatData.monsterSpawner.GetMaxX() - m_monster.cachPositionX;
		val2.x = Math.Clamp(val2.x, min, max);
		val2.y = Math.Max(val2.y, alertPositionY - m_monster.cachPositionY);
		if (val2 != Vector3.zero)
		{
			Transform monsterTF = m_monster.monsterTF;
			monsterTF.position += val2;
		}
	}

	private void UpdateAttackWall()
	{
		nextFireTime += Time.deltaTime;
		float attrValueByType = m_monster.GetAttrValueByType(EAttrMonsterType.AttackSpeed);
		if (nextFireTime >= attrValueByType)
		{
			if (m_shootTimer <= 0f)
			{
				m_monster.attackCount++;
				TriggerAttack();
			}
			nextFireTime = 0f;
		}
	}

	private void TriggerAttack()
	{
		if (m_monster.monsterSkillIndex3014 == 3)
		{
			m_monster.monsterSkillIndex30143.skill1TimeKeep += m_monster.monsterSkillIndex30143.delCd;
			m_monster.monsterSkillIndex30143.skill2TimeKeep += m_monster.monsterSkillIndex30143.delCd;
			m_monster.monsterSkillIndex30143.skill3TimeKeep += m_monster.monsterSkillIndex30143.delCd;
		}
		if (m_monster.monsterIndex10112)
		{
			if (!m_monster.monsterIndex10112Action)
			{
				m_monster.monsterIndex10112Action = true;
				m_monster.monsterIndex10112SkillBase?.PlayTackAnimation(0, flag: false);
			}
			m_monster10112Timer = 5f / 6f;
		}
		else
		{
			m_monster.anim.spineAnimation.PlayAnimation("attack", loop: false);
			m_monster.anim.spineAnimation.AddAnimation("idle", loop: true);
		}
		m_shootTimer = 7f / 15f;
	}

	private void UpdateLogicTimers()
	{
		if (m_shootTimer > 0f)
		{
			m_shootTimer -= Time.deltaTime;
			if (m_shootTimer <= 0f)
			{
				ExecuteShootEffect();
				m_shootTimer = -1f;
			}
		}
		if (!(m_monster10112Timer > 0f))
		{
			return;
		}
		m_monster10112Timer -= Time.deltaTime;
		if (m_monster10112Timer <= 0f)
		{
			if (m_monster.monsterIndex10112)
			{
				m_monster.SetCurHp(0L);
			}
			m_monster10112Timer = -1f;
		}
	}

	private void ExecuteShootEffect()
	{
		if (m_fsm.GetStateId() != EMonsterState.Die && m_monster.canMoveOrAttack)
		{
			if (m_monster.monsterRecord.atkType == 2)
			{
				m_monster.combatData.monsterBulletEffectPoolMgr.GetMonsterBulletEffect(m_monster.monsterRecord.id).Init(m_monster.combatData, m_monster);
			}
			else
			{
				zbCombatSystem.Instance.combatFormula.CalcWallDamage(m_monster, m_monster.combatData.defense);
			}
		}
	}
}
