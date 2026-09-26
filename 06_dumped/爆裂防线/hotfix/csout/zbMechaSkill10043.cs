using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbMechaSkill10043 : zbMechaSkillBase
{
	private HashSet<int> m_monstersInRange = new HashSet<int>();

	private HashSet<int> m_monstersKnockback = new HashSet<int>();

	private float m_damageInterval;

	private float m_damageIntervalPre;

	protected override void OnInited()
	{
		base.OnInited();
		m_damageIntervalPre = 0f;
		m_monstersInRange.Clear();
		m_monstersKnockback.Clear();
		ReturnToPool(99f);
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		m_damageInterval = mechaSkillRecord.param[0];
		skillAttrList[EAttrSkillType.Knockback] = _mechaSkillRecord.param[1];
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Add(component.GetUUID());
			m_monstersKnockback.Add(component.GetUUID());
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

	protected override void OnDisable()
	{
		base.OnDisable();
		m_monstersInRange.Clear();
		m_monstersKnockback.Clear();
	}

	protected override void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		float num = mechaSkillRecord.param[2] / 100f;
		float num2 = mechaSkillRecord.param[3] / 100f;
		((Component)this).transform.Translate(Vector3.up * num * Time.deltaTime, (Space)0);
		if (((Component)((Component)this).transform).transform.position.y >= combatData.defense.GetAlertPositionY() + num2)
		{
			ReturnToPool();
		}
		m_damageIntervalPre += Time.deltaTime;
		if (m_damageIntervalPre >= m_damageInterval)
		{
			m_damageIntervalPre -= m_damageInterval;
			UpdateMonsterCollision();
		}
	}

	private void UpdateMonsterCollision()
	{
		foreach (int item in m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, mechaSkillRecord.atkType);
			ShootKnockbackMonster(monsterById);
		}
	}

	private void ShootKnockbackMonster(zbMonster monster)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (mechaSkillRecord.param[1] > 0f)
		{
			zbSkillSystem.Instance.CreateSubSkill(10203, hero, this, skillAttrList, Vector3.zero, Vector2.up, monster.GetUUID());
		}
	}
}
