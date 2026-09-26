using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbMonsterDieState(zbMonsterStateMachine fsm, EMonsterState eState) : zbMonsterState(fsm, eState)
{
	private Vector2 targetPosition;

	private float targetPositionTime;

	private bool callBacked;

	private bool m_dieStateTimerActive;

	private float m_dieStateTimer;

	private List<zbMonster> monstersListById = new List<zbMonster>();

	private bool callBackActioned;

	private float deathBlow = 10f;

	private float deathBlowTime;

	public override void OnEnter()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		callBacked = false;
		callBackActioned = false;
		m_dieStateTimerActive = false;
		m_dieStateTimer = 0f;
		deathBlowTime = 0f;
		if (m_fsm.monster.IsBoss())
		{
			zbCombatSystem.Instance.bigBossDied = true;
		}
		m_fsm.monster.TriggerDie();
		m_fsm.monster.SetBoxColliderEnabel(bEnabale: false);
		int num = 0;
		m_fsm.monster.combatData.monsters.GetRandomMonsterList(5, monsterRandomList, m_fsm.monster.GetUUID());
		foreach (zbBuffBase item in m_fsm.monster.buffDieTodo)
		{
			zbSkillSystem.Instance.CreateSubSkill((int)item.buffRecord.param, item.skillBase.hero, item.skillBase, item.skillAttrList, ((Component)m_fsm.monster).transform.position, Vector2.up, m_fsm.monster.GetUUID(), (monsterRandomList.Count > num) ? monsterRandomList[num].GetUUID() : 0);
			num++;
		}
		((zbBuffIndex13)zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(m_fsm.monster.GetUUID(), 24))?.TriggerDieAction();
		if ((m_fsm.monster.unTriggerDieSkill && !m_fsm.monster.monsterIndex40151.monsterIndex40151) || zbCombatSystem.Instance.m_isCelebrating)
		{
			CallBack();
		}
		else
		{
			TriggerDieSkill();
		}
	}

	public void TriggerDieSkill()
	{
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		if (m_fsm.monster.monsterIndex10161.monsterSkill != null)
		{
			callBacked = true;
			m_fsm.monster.monsterIndex10161.monsterSkill.TriggerDieSkill(CallBack);
		}
		if (m_fsm.monster.monsterIndex10362.monsterSkill != null)
		{
			callBacked = true;
			m_fsm.monster.monsterIndex10362.monsterSkill.TriggerDieSkill(CallBack);
		}
		if (m_fsm.monster.monsterIndex30062.monsterSkill != null)
		{
			callBacked = true;
			m_fsm.monster.monsterIndex30062.monsterSkill.TriggerDieSkill(CallBack);
		}
		if (m_fsm.monster.monsterIndex10223.monsterSkill != null)
		{
			callBacked = true;
			m_fsm.monster.monsterIndex10223.monsterSkill.TriggerDieSkill(CallBack);
		}
		if (m_fsm.monster.monsterIndex40151.monsterIndex40151)
		{
			callBacked = true;
			m_fsm.monster.monsterIndex40151.monsterSkill.TriggerDieSkill(CallBack);
		}
		if (m_fsm.monster.monster30111Class != null)
		{
			callBacked = true;
			m_fsm.monster.monster30111Class.TriggerDieSkill(CallBack);
		}
		if (m_fsm.monster.monster30121Class != null)
		{
			callBacked = true;
			m_fsm.monster.monster30112Class.TriggerDieSkill(CallBack);
		}
		if (m_fsm.monster.monster30131Class != null)
		{
			callBacked = true;
			m_fsm.monster.monster30131Class.TriggerDieSkill(CallBack);
		}
		zbHero zbHero2 = m_fsm.monster.combatData.masterTeam.heroList[0];
		if (m_fsm.monster.monsterIndex20091.monsterSkill != null)
		{
			zbSkillSystem.Instance.CreateSubSkill(m_fsm.monster.monsterIndex20091.skillSubTid, zbHero2, null, zbHero2.heroAttr.skillAttrList, ((Component)m_fsm.monster).transform.position, Vector2.up);
		}
		if (m_fsm.monster.monsterIndex10382.skillIndexFlag && !m_fsm.monster.monsterIndex10382.monsterSkill.IsBuffStop())
		{
			((MonoBehaviour)m_fsm.monster).StartCoroutine(MonsterIndex10382Coroutine());
			targetPosition = new Vector2(((Component)m_fsm.monster).transform.position.x, Mathf.Max(-5.6f, ((Component)m_fsm.monster).transform.position.y - m_fsm.monster.monsterIndex10382.distance));
			targetPositionTime = (((Component)m_fsm.monster).transform.position.y - targetPosition.y) / m_fsm.monster.monsterIndex10382.speed;
			m_fsm.monster.anim.spineAnimation.PlayAnimation("run", loop: true);
		}
		else if (m_fsm.monster.monsterIndex40111.skillIndexFlag && m_fsm.monster.IsEnterAttackRange() && !m_fsm.monster.monsterIndex40111.monsterSkill.IsBuffStop())
		{
			((MonoBehaviour)m_fsm.monster).StartCoroutine(MonsterIndex40111Coroutine());
			m_fsm.monster.monsterIndex40111.monsterSkill.PlayTackAnimation(0, flag: true);
		}
		else if (!callBacked)
		{
			CallBack();
		}
		if (!m_fsm.monster.monsterIndex20132.skillIndexFlag)
		{
			return;
		}
		m_fsm.monster.combatData.monsters.GetMonsterListByTid(m_fsm.monster.monsterIndex20132.monsterTid, monstersListById);
		foreach (zbMonster item in monstersListById)
		{
			item.MonsterExtraProperties[(EAttrMonsterType)m_fsm.monster.monsterIndex20132.attrMonsterType] += m_fsm.monster.monsterIndex20132.attrPercent;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		if (zbCombatSystem.Instance.scrollSpeed > 0f && ((Component)m_fsm.monster).transform.position.y > m_fsm.monster.combatData.defense.GetAlertPositionY() + 0.5f)
		{
			Transform transform = ((Component)m_fsm.monster).transform;
			transform.position += Vector3.down * zbCombatSystem.Instance.scrollSpeed * Time.deltaTime;
		}
		if (m_fsm.monster.monsterIndex10382.skillIndexFlag && m_fsm.monster.monsterIndex10382.beginMove)
		{
			((Component)m_fsm.monster).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)m_fsm.monster).transform.position), targetPosition, m_fsm.monster.monsterIndex10382.speed * Time.deltaTime));
		}
		UpdateDieTimer();
	}

	private IEnumerator MonsterIndex10382Coroutine()
	{
		yield return (object)new WaitForSeconds(0.5f);
		m_fsm.monster.monsterIndex10382.beginMove = true;
		yield return (object)new WaitForSeconds(targetPositionTime);
		if (((Component)m_fsm.monster).transform.position.y <= -5.5f)
		{
			zbCombatSystem.Instance.combatFormula.CalcWallDamage(null, m_fsm.monster.combatData.defense, (int)(m_fsm.monster.GetAttrValueByType(EAttrMonsterType.Damage) * m_fsm.monster.monsterIndex10382.damagePercent), ((Component)m_fsm.monster).transform.position.x);
		}
		CallBack();
	}

	private IEnumerator MonsterIndex40111Coroutine()
	{
		while (m_fsm.monster.monsterIndex40111.attackNum > 0)
		{
			m_fsm.monster.monsterIndex40111.attackNum--;
			zbCombatSystem.Instance.combatFormula.CalcWallDamage(null, m_fsm.monster.combatData.defense, (int)m_fsm.monster.GetAttrValueByType(EAttrMonsterType.Damage), ((Component)m_fsm.monster).transform.position.x);
			yield return (object)new WaitForSeconds(0.1f);
		}
		CallBack();
	}

	public void CallBack()
	{
		if (callBackActioned)
		{
			return;
		}
		if (m_fsm.monster.curExp > 0 && zbCombatSystem.Instance.isPVPBattleType)
		{
			m_fsm.monster.combatData.pvpMonsterDieNumber++;
			if (m_fsm.monster.combatData.pvpMonsterDieList.TryGetValue(m_fsm.monster.GetTid(), out var value))
			{
				m_fsm.monster.combatData.pvpMonsterDieList[m_fsm.monster.GetTid()] = value + 1;
			}
			else
			{
				m_fsm.monster.combatData.pvpMonsterDieList.Add(m_fsm.monster.GetTid(), 1);
			}
		}
		callBackActioned = true;
		StartDieTimer();
	}

	public void StartDieTimer()
	{
		m_fsm.monster.combatData.monsterDieFxPool.GetDieFx().Init(m_fsm.monster);
		m_dieStateTimerActive = true;
		m_dieStateTimer = 0f;
	}

	private void UpdateDieTimer()
	{
		if (!m_dieStateTimerActive)
		{
			deathBlowTime += Time.deltaTime;
			if (deathBlowTime > deathBlow)
			{
				m_dieStateTimerActive = true;
				m_dieStateTimer = 0f;
			}
		}
		if (m_dieStateTimerActive)
		{
			m_dieStateTimer += Time.deltaTime;
			if (m_dieStateTimer >= 0.25f)
			{
				DieTimer();
			}
		}
	}

	private void DieTimer()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		m_fsm.monster.ReturnToPool();
		m_dieStateTimerActive = false;
		if (m_fsm.monster.combatData.dataIndex == 1 && m_fsm.monster.turntable == 1 && m_fsm.monster.isEliteBoss && zbCombatSystem.Instance.dataResult == null && !zbCombatSystem.Instance.isPVPBattleType)
		{
			zbUIMgr.Instance.ShowModWheelWnd(m_fsm.monster.combatData.masterTeam, m_fsm.monster.monsterRecord.id);
		}
		if ((zbCombatSystem.Instance.IsWorldLair() || zbCombatSystem.Instance.isLegionBoss()) && m_fsm.monster.curExp > 0 && !zbCombatSystem.Instance.combatFPS.IsVeryLowPerfomanceMode())
		{
			m_fsm.monster.combatData.monsterDieEffectPoolMgr.GetMonsterDieEffect().InitData(m_fsm.monster.combatData, m_fsm.monster.GetCurPosition(), ((Component)m_fsm.monster).transform.localScale);
		}
		if (m_fsm.monster.skillMgr != null)
		{
			m_fsm.monster.skillMgr.Free();
		}
	}
}
