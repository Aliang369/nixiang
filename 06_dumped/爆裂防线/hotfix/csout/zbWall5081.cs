using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbWall5081 : zbWallSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float m_moveSpeed;

	public stWallSkillIndex5081 wallSkillIndex5081Data;

	private bool triggerSubSkill;

	protected override void OnInited()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		m_monstersInRange.Clear();
		triggerSubSkill = false;
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		float num = Math.Clamp(((Component)this).transform.position.x, combatData.monsterSpawner.GetMinX() + 0.3f, combatData.monsterSpawner.GetMaxX() - 0.3f);
		((Component)this).transform.position = new Vector3(num, -8f, ((Component)this).transform.position.z);
		ReturnToPool(20f);
	}

	public override void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		base.SetWallAtr(_damage, _wallSkillRecord);
		m_damageIntervalPre = wallSkillIndex5081Data.damageInterval;
		m_damageInterval = wallSkillIndex5081Data.damageInterval;
		m_moveSpeed = wallSkillIndex5081Data.moveSpeed;
		skillAttrList[EAttrSkillType.SlowDown] = wallSkillIndex5081Data.slowDumpPrecent;
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Add(component.GetUUID());
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Remove(component.GetUUID());
		}
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		skillTF.Translate(m_direction * m_moveSpeed * Time.deltaTime, (Space)0);
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
	}

	private void UpdateMonsterCollision()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		foreach (int item in m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
		if (m_monstersInRange.Count > 0 && !triggerSubSkill)
		{
			triggerSubSkill = true;
			if (wallSkillIndex5081Data.damageRange > 0f)
			{
				zbWall5081_2 obj = (zbWall5081_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5081_2.prefab", combatData.defense, new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y + 2.5f), Vector2.up);
				obj.wallSkillIndex5081Data = wallSkillIndex5081Data;
				obj.SetWallAtr(1f, wallSkillIndex5081Data.wallSkillIndex5081.wallSkillRecord);
			}
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, wallSkillRecord.atkType);
			zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monsterById.GetUUID(), 1, wallSkillIndex5081Data.slowDumpTime);
		}
	}
}
